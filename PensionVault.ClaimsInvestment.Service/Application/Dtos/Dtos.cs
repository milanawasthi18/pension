using System;
using PensionVault.ClaimsInvestment.Service.Domain.Enums;

namespace PensionVault.ClaimsInvestment.Service.Application.Dtos;

// ── Claims ──────────────────────────────────────────────────────────────────
public record CreateClaimRequest(Guid MemberId, ClaimType ClaimType, decimal EligibleAmount);
public record ClaimActionRequest(string? Remarks);
public record DisburseClaimRequest(decimal DisbursedAmount, decimal TaxDeducted, string BankAccountRef);

public record ClaimResponse(
    Guid ClaimId,
    Guid MemberId,
    string MemberName,
    ClaimType ClaimType,
    DateTime ClaimDate,
    decimal EligibleAmount,
    decimal VestedAmount,
    decimal TaxDeductible,
    string? ProcessedByName,
    ClaimStatus Status
);

public record DisbursementResponse(
    Guid DisbursementId,
    Guid ClaimId,
    decimal DisbursedAmount,
    decimal TaxDeducted,
    decimal NetAmount,
    string? BankAccountRef,
    DateTime? DisbursedDate,
    DisbursementStatus Status
);

public record CreatePartialWithdrawalRequest(Guid MemberId, decimal RequestedAmount, string Reason);
public record DisbursePartialWithdrawalRequest(decimal DisbursedAmount, string BankAccountRef);

// ── Investment ───────────────────────────────────────────────────────────────
public record CreatePortfolioRequest(
    Guid SchemeId,
    AssetClass AssetClass,
    decimal AllocationPercent,
    decimal InvestedValue,
    decimal CurrentValue,
    decimal YieldEarned
);

public record UpdatePortfolioRequest(
    decimal AllocationPercent,
    decimal InvestedValue,
    decimal CurrentValue,
    decimal YieldEarned
);

public record PortfolioResponse(
    Guid PortfolioId,
    Guid SchemeId,
    string SchemeName,
    AssetClass AssetClass,
    decimal AllocationPercent,
    decimal InvestedValue,
    decimal CurrentValue,
    decimal YieldEarned,
    DateTime LastUpdated
);

public record CreateCorpusRequest(
    Guid SchemeId,
    DateTime RecordDate,
    decimal TotalContributions,
    decimal TotalWithdrawals,
    decimal InvestmentIncome,
    decimal ManagementExpenses
);

public record CorpusResponse(
    Guid CorpusId,
    Guid SchemeId,
    string SchemeName,
    DateTime RecordDate,
    decimal OpeningCorpus,
    decimal TotalContributions,
    decimal TotalWithdrawals,
    decimal InvestmentIncome,
    decimal ManagementExpenses,
    decimal ClosingCorpus,
    CorpusStatus Status
);
