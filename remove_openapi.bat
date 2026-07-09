@echo off
rem Remove Microsoft.AspNetCore.OpenApi (conflicts with Swashbuckle) from all services
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.Identity.Service" package Microsoft.AspNetCore.OpenApi
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.EmployerMember.Service" package Microsoft.AspNetCore.OpenApi
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.FundOps.Service" package Microsoft.AspNetCore.OpenApi
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.ClaimsInvestment.Service" package Microsoft.AspNetCore.OpenApi
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.NotificationReporting.Service" package Microsoft.AspNetCore.OpenApi
