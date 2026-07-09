@echo off
rem Remove Swashbuckle from all services
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.Identity.Service" package Swashbuckle.AspNetCore
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.EmployerMember.Service" package Swashbuckle.AspNetCore
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.FundOps.Service" package Swashbuckle.AspNetCore
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.ClaimsInvestment.Service" package Swashbuckle.AspNetCore
dotnet remove "C:\pension\pensionvault_microservices\PensionVault.NotificationReporting.Service" package Swashbuckle.AspNetCore

rem Add Microsoft.AspNetCore.OpenApi and Scalar.AspNetCore to all services
dotnet add "C:\pension\pensionvault_microservices\PensionVault.Identity.Service" package Microsoft.AspNetCore.OpenApi
dotnet add "C:\pension\pensionvault_microservices\PensionVault.Identity.Service" package Scalar.AspNetCore

dotnet add "C:\pension\pensionvault_microservices\PensionVault.EmployerMember.Service" package Microsoft.AspNetCore.OpenApi
dotnet add "C:\pension\pensionvault_microservices\PensionVault.EmployerMember.Service" package Scalar.AspNetCore

dotnet add "C:\pension\pensionvault_microservices\PensionVault.FundOps.Service" package Microsoft.AspNetCore.OpenApi
dotnet add "C:\pension\pensionvault_microservices\PensionVault.FundOps.Service" package Scalar.AspNetCore

dotnet add "C:\pension\pensionvault_microservices\PensionVault.ClaimsInvestment.Service" package Microsoft.AspNetCore.OpenApi
dotnet add "C:\pension\pensionvault_microservices\PensionVault.ClaimsInvestment.Service" package Scalar.AspNetCore

dotnet add "C:\pension\pensionvault_microservices\PensionVault.NotificationReporting.Service" package Microsoft.AspNetCore.OpenApi
dotnet add "C:\pension\pensionvault_microservices\PensionVault.NotificationReporting.Service" package Scalar.AspNetCore
