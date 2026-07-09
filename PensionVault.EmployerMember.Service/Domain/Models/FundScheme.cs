using System;
using System.Collections.Generic;
using PensionVault.EmployerMember.Service.Domain.Enums;

namespace PensionVault.EmployerMember.Service.Domain.Models;

public class FundScheme
{
    public Guid SchemeId { get; set; } = Guid.NewGuid();
    public string SchemeName { get; set; } = string.Empty;
    public SchemeType SchemeType { get; set; }
    public decimal EmployeeContributionRate { get; set; }
    public decimal EmployerContributionRate { get; set; }
    public decimal InterestRatePA { get; set; }
    public string? VestingSchedule { get; set; } // JSON
    public SchemeStatus Status { get; set; } = SchemeStatus.Active;

    // Navigation
    public ICollection<FundAccount> FundAccounts { get; set; } = new List<FundAccount>();
}
