using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.FundOps.Service.Domain.Models;
using PensionVault.FundOps.Service.Domain.Enums;

namespace PensionVault.FundOps.Service.Domain.Interfaces;

public interface ILedgerRepository
{
    Task<List<LedgerEntry>> GetByAccountAsync(Guid accountId);
    Task<List<LedgerEntry>> GetAllAsync();
    Task<decimal> SumByTypeAsync(Guid accountId, EntryType entryType);
    Task<bool> InterestAlreadyCreditedAsync(Guid accountId, string financialYear);
    Task<List<InterestCreditRecord>> GetInterestRecordsAsync(Guid accountId);
    Task AddEntryAsync(LedgerEntry entry);
    Task AddInterestRecordAsync(InterestCreditRecord record);
}
