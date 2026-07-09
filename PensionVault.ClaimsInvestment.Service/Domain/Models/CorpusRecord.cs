using System;
using PensionVault.ClaimsInvestment.Service.Domain.Enums;

namespace PensionVault.ClaimsInvestment.Service.Domain.Models;

public class CorpusRecord
{
    public Guid CorpusId { get; set; } = Guid.NewGuid();
    public Guid SchemeId { get; set; }
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;
    public decimal TotalContributions { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public decimal InvestmentIncome { get; set; }
    public decimal ManagementExpenses { get; set; }
    public decimal ClosingCorpus { get; set; }
    public CorpusStatus Status { get; set; } = CorpusStatus.Draft;
}
