@echo off
rem Run all pension vault microservices in separate windows
start "Gateway" dotnet run --project "C:\pension\pensionvault_microservices\PensionVault.Gateway"
start "Identity" dotnet run --project "C:\pension\pensionvault_microservices\PensionVault.Identity.Service"
start "EmployerMember" dotnet run --project "C:\pension\pensionvault_microservices\PensionVault.EmployerMember.Service"
start "FundOps" dotnet run --project "C:\pension\pensionvault_microservices\PensionVault.FundOps.Service"
start "ClaimsInvestment" dotnet run --project "C:\pension\pensionvault_microservices\PensionVault.ClaimsInvestment.Service"
start "NotificationReporting" dotnet run --project "C:\pension\pensionvault_microservices\PensionVault.NotificationReporting.Service"
