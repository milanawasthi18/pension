using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using PensionVault.Identity.Service.Domain.Interfaces;
using PensionVault.Identity.Service.Infrastructure.Data;
using PensionVault.Identity.Service.Infrastructure.Repositories;
using PensionVault.Identity.Service.Application.Interfaces;
using PensionVault.Identity.Service.Application.Services;
using PensionVault.Identity.Service.HttpClients;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure port 5001
builder.WebHost.UseUrls("http://localhost:5001");

// Database configuration
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(connString));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Typed HttpClients with Polly policies
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

builder.Services.AddHttpClient<EmployerMemberClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5002/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHttpClient<NotificationClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5005/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

// JWT Validation
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("PensionVault Identity API")
           .WithTheme(ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
