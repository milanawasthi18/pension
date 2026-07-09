namespace PensionVault.FundOps.Service.Domain.Enums;

public enum RemittanceStatus { Received, Reconciled, Shortfall, Pending }
public enum ContributionStatus { Pending, Received, Posted, Rejected }
public enum EntryType { ContributionCredit, InterestCredit, ClaimDebit, AnnuityDebit, WithdrawalDebit, CorrectionCredit, CorrectionDebit }
public enum LedgerEntryStatus { Posted, Reversed, OnHold }
public enum InterestCreditStatus { Computed, Credited, Reversed }
public enum AnnuityPlanType { SingleLife, JointLife, ReturnOfPurchase }
public enum AnnuityStatus { Active, Suspended, Terminated, NomineeSettled }
public enum PensionDisbursementStatus { Pending, Disbursed, Failed }
