using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using PensionVault.EmployerMember.Service.Domain.Interfaces;
using PensionVault.EmployerMember.Service.Infrastructure.Data;
using PensionVault.EmployerMember.Service.Infrastructure.Repositories;
using PensionVault.EmployerMember.Service.Application.Interfaces;
using PensionVault.EmployerMember.Service.Application.Services;
using PensionVault.EmployerMember.Service.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Configure port 5002
builder.WebHost.UseUrls("http://localhost:5002");

// Database configuration
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EmployerMemberDbContext>(options =>
    options.UseSqlServer(connString));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmployerRepository, EmployerRepository>();
builder.Services.AddScoped<IFundSchemeRepository, FundSchemeRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IFundAccountRepository, FundAccountRepository>();

// Services
builder.Services.AddScoped<IEmployerService, EmployerService>();
builder.Services.AddScoped<ISchemeService, SchemeService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IFundAccountService, FundAccountService>();

// JWT forwarding concerns
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtForwardingHandler>();

// Polly Resilience Policies
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// Typed HttpClients
builder.Services.AddHttpClient<IdentityClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtForwardingHandler>()
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHttpClient<FundOpsClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5003/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<JwtForwardingHandler>()
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddHttpClient<ClaimsInvestmentClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5004/");
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
    options.WithTitle("PensionVault EmployerMember API")
           .WithTheme(ScalarTheme.DeepSpace)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
