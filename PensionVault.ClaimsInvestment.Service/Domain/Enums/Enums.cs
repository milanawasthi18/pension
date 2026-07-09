namespace PensionVault.ClaimsInvestment.Service.Domain.Enums;

public enum ClaimType { Retirement, Resignation, MedicalGrounds, Death, PartialWithdrawal }
public enum ClaimStatus { Submitted, UnderReview, Approved, Rejected, Disbursed }
public enum DisbursementStatus { Pending, Disbursed, Failed }
public enum AssetClass { Equity, CorporateBonds, GovernmentSecurities, MoneyMarket, AlternativeAssets }
public enum CorpusStatus { Draft, Snapshot, Finalised, Archived }
