using Microsoft.EntityFrameworkCore;
using RegistrationService.Data;
using RegistrationService.Services;
using Amazon.S3;
using Amazon.Runtime;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers();
builder.Services.AddDbContext<RegistrationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRegistrationService, RegistrationService.Services.RegistrationService>();
builder.Services.AddScoped<IEmailService, EmailService>();  // ← ADD THIS LINE

// ===== Hangfire for Scheduled Jobs =====
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => 
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

// ===== JWT Authentication =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "VibraApp",
            ValidAudience = "VibraUsers",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!")
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ===== Cloudflare R2 =====
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    
    var r2Config = new AmazonS3Config
    {
        ServiceURL = $"https://{config["R2:AccountId"]}.r2.cloudflarestorage.com",
        ForcePathStyle = true
    };

    var credentials = new BasicAWSCredentials(
        config["R2:AccessKeyId"],
        config["R2:SecretAccessKey"]
    );

    return new AmazonS3Client(credentials, r2Config);
});

// ===== Build app =====
var app = builder.Build();

app.UseCors();
app.UseAuthentication();  // ← Add this BEFORE UseAuthorization
app.UseAuthorization();   // ← Add this
app.UseHangfireDashboard("/hangfire");

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
    db.Database.Migrate();
}

var serviceProvider = app.Services;

RecurringJob.AddOrUpdate(
    "send-confirmation-emails",
    () => ScheduledJobs.SendConfirmationEmailsJob(serviceProvider),
    "0 17 30 3 *");  // March 23rd at 10 AM

RecurringJob.AddOrUpdate(
    "send-deadline-reminders",
    () => ScheduledJobs.SendDeadlineRemindersJob(serviceProvider),
    "0 10 1 4 *");   // April 1st at 10 AM

RecurringJob.AddOrUpdate(
    "cleanup-unconfirmed",
    () => ScheduledJobs.CleanupUnconfirmedJob(serviceProvider),
    "0 18 1 4 *");   // April 1st at 6 PM


app.Run("http://0.0.0.0:5001");