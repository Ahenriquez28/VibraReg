using VibraApiGateway.Interfaces;
using VibraApiGateway.Proxies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

var registrationServiceUrl = builder.Configuration["RegistrationService:BaseUrl"] ?? "http://localhost:5001";
var authServiceUrl = builder.Configuration["AdminService:BaseUrl"] ?? "http://localhost:5089";
var frontendUrl = builder.Configuration["Frontend:Url"] ?? "http://localhost:5173";

// Add HTTP clients for downstream services
builder.Services.AddHttpClient<IRegistrationProxy, RegistrationProxy>(client =>
{
    client.BaseAddress = new Uri(registrationServiceUrl);
});

builder.Services.AddHttpClient<IAuthProxy, AuthProxy>(client =>
{
    client.BaseAddress = new Uri(authServiceUrl);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost",
                "http://localhost:80",
                "http://localhost:5173",
                "http://vibra_atl",
                "http://frontend",
                "http://165.245.130.1",
                "http://vibraatl.com",
                "https://vibraatl.com",
                "https://www.vibraatl.com"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
var app = builder.Build();

// Use CORS before routing
app.UseCors();
app.UseCors("AllowFrontend");

// Map controllers
app.MapControllers();

//app.Run();
app.Run("http://0.0.0.0:5222");  // ← ADD THIS LINE
