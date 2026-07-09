using System;
using PensionVault.EmployerMember.Service.Domain.Enums;

namespace PensionVault.EmployerMember.Service.Domain.Models;

public class FundAccount
{
    public Guid AccountId { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Guid SchemeId { get; set; }
    public DateTime AccountOpenDate { get; set; }
    public decimal EmployeeContributionBalance { get; set; }
    public decimal EmployerContributionBalance { get; set; }
    public decimal InterestAccrued { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal VestingPercent { get; set; }
    public FundAccountStatus Status { get; set; } = FundAccountStatus.Active;

    // Navigation
    public Member Member { get; set; } = null!;
    public FundScheme Scheme { get; set; } = null!;
}
