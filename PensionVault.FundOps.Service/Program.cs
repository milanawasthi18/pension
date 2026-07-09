using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using PensionVault.FundOps.Service.Domain.Interfaces;
using PensionVault.FundOps.Service.Infrastructure.Data;
using PensionVault.FundOps.Service.Infrastructure.Repositories;
using PensionVault.FundOps.Service.Application.Interfaces;
using PensionVault.FundOps.Service.Application.Services;
using PensionVault.FundOps.Service.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Configure port 5003
builder.WebHost.UseUrls("http://localhost:5003");

// Database configuration
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FundOpsDbContext>(options =>
    options.UseSqlServer(connString));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IContributionRepository, ContributionRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<IAnnuityRepository, AnnuityRepository>();

// Services
builder.Services.AddScoped<IContributionService, ContributionService>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<IAnnuityService, AnnuityService>();

// JWT Forwarding config
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtForwardingHandler>();

// Polly Resilience Policies
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// HttpClients
builder.Services.AddHttpClient<EmployerMemberClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5002/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtForwardingHandler>()
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHttpClient<NotificationClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5005/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtForwardingHandler>()
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
    options.WithTitle("PensionVault FundOps API")
           .WithTheme(ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
