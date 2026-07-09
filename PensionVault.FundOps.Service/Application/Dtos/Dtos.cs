using System;
using System.Collections.Generic;
using PensionVault.FundOps.Service.Domain.Enums;

namespace PensionVault.FundOps.Service.Application.Dtos;

public record CreateRemittanceRequest(
    Guid EmployerId,
    string RemittancePeriod,
    decimal TotalEmployeeShare,
    decimal TotalEmployerShare,
    int CoverageCount,
    List<MemberContributionItem> MemberContributions
);

public record MemberContributionItem(
    Guid MemberId,
    decimal EmployeeAmount,
    decimal EmployerAmount
);

public record RemittanceResponse(
    Guid RemittanceId,
    Guid EmployerId,
    string EmployerName,
    string RemittancePeriod,
    decimal TotalEmployeeShare,
    decimal TotalEmployerShare,
    decimal TotalAmount,
    DateTime RemittanceDate,
    int CoverageCount,
    RemittanceStatus Status
);

public record MemberContributionResponse(
    Guid ContributionId,
    Guid MemberId,
    string MemberName,
    string Period,
    decimal EmployeeAmount,
    decimal EmployerAmount,
    decimal TotalAmount,
    DateTime PostedDate,
    ContributionStatus Status
);

public record ReconciliationReportResponse(
    Guid RemittanceId,
    string RemittancePeriod,
    int ExpectedCount,
    int ReconciledCount,
    decimal TotalExpectedAmount,
    decimal TotalReconciledAmount,
    string Status
);

public record DefaulterSummaryResponse(
    Guid EmployerId,
    string EmployerName,
    int MissingPeriods,
    decimal EstimatedShortfall,
    string LastRemittancePeriod
);

public record LedgerEntryResponse(
    Guid EntryId,
    Guid AccountId,
    EntryType EntryType,
    decimal Amount,
    decimal BalanceAfter,
    DateTime EntryDate,
    string? ReferenceId,
    LedgerEntryStatus Status
);

public record CreditInterestRequest(
    Guid AccountId,
    string FinancialYear,
    decimal InterestRate
);

public record InterestCreditResponse(
    Guid InterestId,
    Guid AccountId,
    string FinancialYear,
    decimal OpeningBalance,
    decimal TotalContributions,
    decimal InterestRateApplied,
    decimal InterestAmount,
    decimal ClosingBalance,
    DateTime CreditedDate,
    InterestCreditStatus Status
);

public record LedgerEntryRequest(
    Guid AccountId,
    EntryType EntryType,
    decimal Amount,
    decimal BalanceAfter,
    string? ReferenceId
);

public record CreateAnnuityRequest(
    Guid MemberId,
    AnnuityPlanType PlanType,
    decimal PurchaseValue,
    decimal MonthlyPension,
    DateTime AnnuityStartDate,
    string? NomineeDetails
);

public record AnnuityResponse(
    Guid AnnuityId,
    Guid MemberId,
    string MemberName,
    AnnuityPlanType PlanType,
    decimal PurchaseValue,
    decimal MonthlyPension,
    DateTime AnnuityStartDate,
    string? NomineeDetails,
    AnnuityStatus Status
);

public record ProcessDisbursementRequest(
    Guid AnnuityId,
    int Month,
    int Year,
    decimal TaxDeducted
);

public record PensionDisbursementResponse(
    Guid DisbursementId,
    Guid AnnuityId,
    Guid MemberId,
    string MemberName,
    int Month,
    int Year,
    decimal GrossAmount,
    decimal TaxDeducted,
    decimal NetAmount,
    DateTime? DisbursedDate,
    PensionDisbursementStatus Status
);

public record NomineeSettlementRequest(
    string NomineeName,
    string BankAccountRef,
    decimal SettlementAmount
);

// Helper records for external services
public record FundAccountResponse(
    Guid AccountId,
    Guid MemberId,
    Guid SchemeId,
    string SchemeName,
    DateTime AccountOpenDate,
    decimal EmployeeContributionBalance,
    decimal EmployerContributionBalance,
    decimal InterestAccrued,
    decimal TotalBalance,
    decimal VestingPercent,
    string Status
);
