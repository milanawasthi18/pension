using System;
using PensionVault.FundOps.Service.Domain.Enums;

namespace PensionVault.FundOps.Service.Domain.Models;

public class LedgerEntry
{
    public Guid EntryId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public EntryType EntryType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string? ReferenceId { get; set; }
    public LedgerEntryStatus Status { get; set; } = LedgerEntryStatus.Posted;
}
