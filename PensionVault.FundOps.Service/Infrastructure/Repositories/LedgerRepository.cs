using Microsoft.EntityFrameworkCore;
using PensionVault.FundOps.Service.Domain.Models;
using PensionVault.FundOps.Service.Domain.Enums;
using PensionVault.FundOps.Service.Domain.Interfaces;
using PensionVault.FundOps.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.FundOps.Service.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly FundOpsDbContext _context;
    public LedgerRepository(FundOpsDbContext context) => _context = context;

    public Task<List<LedgerEntry>> GetByAccountAsync(Guid accountId)
        => _context.LedgerEntries
            .Where(e => e.AccountId == accountId)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();

    public Task<List<LedgerEntry>> GetAllAsync()
        => _context.LedgerEntries
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();

    public Task<decimal> SumByTypeAsync(Guid accountId, EntryType entryType)
        => _context.LedgerEntries
            .Where(e => e.AccountId == accountId && e.EntryType == entryType)
            .SumAsync(e => e.Amount);

    public Task<bool> InterestAlreadyCreditedAsync(Guid accountId, string financialYear)
        => _context.InterestCreditRecords
            .AnyAsync(r => r.AccountId == accountId && r.FinancialYear == financialYear);

    public Task<List<InterestCreditRecord>> GetInterestRecordsAsync(Guid accountId)
        => _context.InterestCreditRecords
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.FinancialYear)
            .ToListAsync();

    public async Task AddEntryAsync(LedgerEntry entry)
        => await _context.LedgerEntries.AddAsync(entry);

    public async Task AddInterestRecordAsync(InterestCreditRecord record)
        => await _context.InterestCreditRecords.AddAsync(record);
}
