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
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost",           // Docker nginx
                "http://localhost:80",        // Docker nginx with port
                "http://localhost:5173",      // Local dev
                "http://vibra_atl",           // Docker container name
                "http://frontend"             // Docker service name
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Use CORS before routing
app.UseCors();

// Map controllers
app.MapControllers();

//app.Run();
app.Run("http://0.0.0.0:5222");  // ← ADD THIS LINE