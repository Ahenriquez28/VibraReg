using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using JwtAuthApi.Models;
using JwtAuthApi.Services;
using JwtAuthApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add JWT Settings as a dependency injection from appsettings.json
var jwtSettings = new JwtSettings();
//From appsettings.json, assign JwtSettings to out instance of the class JwtSettings
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);


//Singleton: Inject data into a class, it stays there permantly for when we call it 
//We make Jwtsettings a singleton with line 20, then in line 21 we make it like a class globally regonized with its current data
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenService>();

// PostgreSQL Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// *** ADD CORS ***
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost",           // Docker nginx
                "http://localhost:80",        // Docker nginx with port
                "http://localhost:5173"       // Local dev
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to support JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT Auth API with PostgreSQL", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();  // *** ADD THIS - Must be BEFORE UseAuthentication ***
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Auto-migrate database on startup
// Auto-migrate database on startup
// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    
    Console.WriteLine("🔍 Checking for admin user...");
    
    // ✅ Seed admin user if doesn't exist
    if (!db.AuthUsers.Any(u => u.Username == "ElSalvadorIsBetterThanMexico"))
    {
        Console.WriteLine("✅ Creating admin user...");
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("MexicoSucks");
        Console.WriteLine($"🔐 Hashed password: {hashedPassword}");
        
        db.AuthUsers.Add(new User
        {
            Username = "ElSalvadorIsBetterThanMexico",
            Email = "admin@vibra.com",
            PasswordHash = hashedPassword,
            FirstName = "Admin",
            LastName = "User",
            Roles = "Admin",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.SaveChanges();
        Console.WriteLine("✅ Admin user created!");
    }
    else
    {
        Console.WriteLine("⚠️  Admin user already exists");
    }
}

app.Run();

app.Run();

app.Run();
app.Run();