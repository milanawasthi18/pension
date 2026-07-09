using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionVault.FundOps.Service.Application.Dtos;
using PensionVault.FundOps.Service.Application.Interfaces;
using PensionVault.FundOps.Service.Domain.Models;
using PensionVault.FundOps.Service.Domain.Enums;
using PensionVault.FundOps.Service.Domain.Interfaces;
using PensionVault.FundOps.Service.HttpClients;

namespace PensionVault.FundOps.Service.Application.Services;

public class LedgerService : ILedgerService
{
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmployerMemberClient _employerMemberClient;

    public LedgerService(
        ILedgerRepository ledgerRepo,
        IUnitOfWork unitOfWork,
        EmployerMemberClient employerMemberClient)
    {
        _ledgerRepo = ledgerRepo;
        _unitOfWork = unitOfWork;
        _employerMemberClient = employerMemberClient;
    }

    public async Task<IEnumerable<LedgerEntryResponse>> GetAccountLedgerAsync(Guid accountId)
    {
        var entries = await _ledgerRepo.GetByAccountAsync(accountId);
        return entries.Select(e => new LedgerEntryResponse(
            e.EntryId, e.AccountId, e.EntryType, e.Amount,
            e.BalanceAfter, e.EntryDate, e.ReferenceId, e.Status));
    }

    public async Task<IEnumerable<LedgerEntryResponse>> GetAllLedgerEntriesAsync()
    {
        var entries = await _ledgerRepo.GetAllAsync();
        return entries.Select(e => new LedgerEntryResponse(
            e.EntryId, e.AccountId, e.EntryType, e.Amount,
            e.BalanceAfter, e.EntryDate, e.ReferenceId, e.Status));
    }

    public async Task<InterestCreditResponse> CreditInterestAsync(CreditInterestRequest request)
    {
        // Get fund account via HTTP from EmployerMember service
        var account = await _employerMemberClient.UpdateBalanceAsync(request.AccountId, 0, 0, 0)
            ?? throw new KeyNotFoundException("Fund account not found.");

        if (await _ledgerRepo.InterestAlreadyCreditedAsync(request.AccountId, request.FinancialYear))
            throw new InvalidOperationException($"Interest already credited for {request.FinancialYear}.");

        var totalContributions = await _ledgerRepo.SumByTypeAsync(request.AccountId, EntryType.ContributionCredit);
        var openingBalance = account.TotalBalance - totalContributions;
        var interestAmount = Math.Round(
            (openingBalance + totalContributions / 2) * (request.InterestRate / 100), 2);

        var record = new InterestCreditRecord
        {
            AccountId = request.AccountId,
            FinancialYear = request.FinancialYear,
            OpeningBalance = openingBalance,
            TotalContributions = totalContributions,
            InterestRateApplied = request.InterestRate,
            InterestAmount = interestAmount,
            ClosingBalance = account.TotalBalance + interestAmount,
            CreditedDate = DateTime.UtcNow,
            Status = Domain.Enums.InterestCreditStatus.Credited
        };
        await _ledgerRepo.AddInterestRecordAsync(record);

        // Mutate balance via HTTP in EmployerMember service (interestDelta = interestAmount, delta = interestAmount)
        var updatedAccount = await _employerMemberClient.UpdateBalanceAsync(
            request.AccountId,
            interestAmount,
            0,
            0,
            interestAmount);

        var balanceAfter = updatedAccount?.TotalBalance ?? (account.TotalBalance + interestAmount);

        // Write LedgerEntry locally into PV_FundOpsDb
        await _ledgerRepo.AddEntryAsync(new LedgerEntry
        {
            AccountId = request.AccountId,
            EntryType = EntryType.InterestCredit,
            Amount = interestAmount,
            BalanceAfter = balanceAfter,
            ReferenceId = record.InterestId.ToString(),
            Status = LedgerEntryStatus.Posted
        });

        await _unitOfWork.SaveChangesAsync();

        return new InterestCreditResponse(
            record.InterestId, record.AccountId, record.FinancialYear,
            record.OpeningBalance, record.TotalContributions, record.InterestRateApplied,
            record.InterestAmount, record.ClosingBalance, record.CreditedDate, record.Status);
    }

    public async Task<IEnumerable<InterestCreditResponse>> GetInterestRecordsAsync(Guid accountId)
    {
        var records = await _ledgerRepo.GetInterestRecordsAsync(accountId);
        return records.Select(r => new InterestCreditResponse(
            r.InterestId, r.AccountId, r.FinancialYear,
            r.OpeningBalance, r.TotalContributions, r.InterestRateApplied,
            r.InterestAmount, r.ClosingBalance, r.CreditedDate, r.Status));
    }

    public async Task WriteLedgerEntryAsync(LedgerEntryRequest request)
    {
        await _ledgerRepo.AddEntryAsync(new LedgerEntry
        {
            AccountId = request.AccountId,
            EntryType = request.EntryType,
            Amount = request.Amount,
            BalanceAfter = request.BalanceAfter,
            ReferenceId = request.ReferenceId,
            Status = LedgerEntryStatus.Posted
        });
        await _unitOfWork.SaveChangesAsync();
    }
}
