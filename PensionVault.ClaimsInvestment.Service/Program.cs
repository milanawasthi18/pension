using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using PensionVault.ClaimsInvestment.Service.Application.Interfaces;
using PensionVault.ClaimsInvestment.Service.Application.Services;
using PensionVault.ClaimsInvestment.Service.Domain.Interfaces;
using PensionVault.ClaimsInvestment.Service.HttpClients;
using PensionVault.ClaimsInvestment.Service.Infrastructure.Data;
using PensionVault.ClaimsInvestment.Service.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Port ──────────────────────────────────────────────────────────────────
builder.WebHost.UseUrls("http://localhost:5004");

// ── Database ─────────────────────────────────────────────────────────────
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClaimsInvestmentDbContext>(opt =>
    opt.UseSqlServer(connString));

// ── Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();

// ── Application Services ──────────────────────────────────────────────────
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();

// ── JWT Forwarding ────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtForwardingHandler>();

// ── Polly Policies ─────────────────────────────────────────────────────────
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry)));
var circuitBreaker = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// ── Typed HttpClients ─────────────────────────────────────────────────────
builder.Services.AddHttpClient<EmployerMemberClient>(c =>
{
    c.BaseAddress = new Uri("http://localhost:5002/");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtForwardingHandler>()
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreaker);

builder.Services.AddHttpClient<FundOpsClient>(c =>
{
    c.BaseAddress = new Uri("http://localhost:5003/");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtForwardingHandler>()
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreaker);

builder.Services.AddHttpClient<NotificationClient>(c =>
{
    c.BaseAddress = new Uri("http://localhost:5005/");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtForwardingHandler>()
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreaker);

// ── JWT Validation ────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
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
    options.WithTitle("PensionVault ClaimsInvestment API")
           .WithTheme(ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
