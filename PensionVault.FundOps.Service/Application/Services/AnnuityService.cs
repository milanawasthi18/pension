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

public class AnnuityService : IAnnuityService
{
    private readonly IAnnuityRepository _annuityRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmployerMemberClient _employerMemberClient;
    private readonly NotificationClient _notificationClient;

    public AnnuityService(
        IAnnuityRepository annuityRepo,
        ILedgerRepository ledgerRepo,
        IUnitOfWork unitOfWork,
        EmployerMemberClient employerMemberClient,
        NotificationClient notificationClient)
    {
        _annuityRepo = annuityRepo;
        _ledgerRepo = ledgerRepo;
        _unitOfWork = unitOfWork;
        _employerMemberClient = employerMemberClient;
        _notificationClient = notificationClient;
    }

    public async Task<AnnuityResponse> CreateAnnuityAsync(CreateAnnuityRequest request)
    {
        var member = await _employerMemberClient.GetMemberAsync(request.MemberId)
            ?? throw new KeyNotFoundException("Member not found.");

        var annuity = new AnnuityPlan
        {
            MemberId = request.MemberId,
            PlanType = request.PlanType,
            PurchaseValue = request.PurchaseValue,
            MonthlyPension = request.MonthlyPension,
            AnnuityStartDate = request.AnnuityStartDate,
            NomineeDetails = request.NomineeDetails,
            Status = AnnuityStatus.Active
        };
        await _annuityRepo.AddAsync(annuity);
        await _unitOfWork.SaveChangesAsync();

        await _notificationClient.SendAsync(
            member.UserId,
            $"Your annuity contract of type {request.PlanType} with monthly pension of ₹{request.MonthlyPension:N2} has been created.",
            "Annuity");

        return await GetAnnuityAsync(annuity.AnnuityId);
    }

    public async Task<AnnuityResponse> GetAnnuityAsync(Guid annuityId)
    {
        var a = await _annuityRepo.FindByIdAsync(annuityId)
            ?? throw new KeyNotFoundException("Annuity plan not found.");
        var member = await _employerMemberClient.GetMemberAsync(a.MemberId);
        return new AnnuityResponse(a.AnnuityId, a.MemberId, member?.Name ?? "Unknown",
            a.PlanType, a.PurchaseValue, a.MonthlyPension,
            a.AnnuityStartDate, a.NomineeDetails, a.Status);
    }

    public async Task<IEnumerable<AnnuityResponse>> GetAllAnnuitiesAsync()
    {
        var annuities = await _annuityRepo.GetAllAsync();
        var responses = new List<AnnuityResponse>();
        foreach (var a in annuities)
        {
            var member = await _employerMemberClient.GetMemberAsync(a.MemberId);
            responses.Add(new AnnuityResponse(
                a.AnnuityId, a.MemberId, member?.Name ?? "Unknown",
                a.PlanType, a.PurchaseValue, a.MonthlyPension,
                a.AnnuityStartDate, a.NomineeDetails, a.Status));
        }
        return responses;
    }

    public async Task<IEnumerable<PensionDisbursementResponse>> GetDisbursementsAsync(Guid annuityId)
    {
        var disbursements = await _annuityRepo.GetDisbursementsAsync(annuityId);
        var annuity = await _annuityRepo.FindByIdAsync(annuityId)
            ?? throw new KeyNotFoundException("Annuity plan not found.");
        var member = await _employerMemberClient.GetMemberAsync(annuity.MemberId);
        var memberName = member?.Name ?? "Unknown";

        return disbursements.Select(d => ToResponse(d, memberName));
    }

    public async Task<PensionDisbursementResponse> ProcessDisbursementAsync(ProcessDisbursementRequest request)
    {
        var annuity = await _annuityRepo.FindByIdAsync(request.AnnuityId)
            ?? throw new KeyNotFoundException("Annuity not found.");
        if (annuity.Status != AnnuityStatus.Active)
            throw new InvalidOperationException("Annuity is not active.");

        var member = await _employerMemberClient.GetMemberAsync(annuity.MemberId)
            ?? throw new KeyNotFoundException("Member not found.");

        var netAmount = annuity.MonthlyPension - request.TaxDeducted;
        var disbursement = new MonthlyPensionDisbursement
        {
            AnnuityId = request.AnnuityId,
            MemberId = annuity.MemberId,
            Month = request.Month,
            Year = request.Year,
            GrossAmount = annuity.MonthlyPension,
            TaxDeducted = request.TaxDeducted,
            NetAmount = netAmount,
            DisbursedDate = DateTime.UtcNow,
            Status = Domain.Enums.PensionDisbursementStatus.Disbursed
        };
        await _annuityRepo.AddDisbursementAsync(disbursement);

        // Fetch active account and update balance via HTTP
        var account = await _employerMemberClient.GetActiveFundAccountAsync(annuity.MemberId);
        if (account != null)
        {
            var updatedAccount = await _employerMemberClient.UpdateBalanceAsync(
                account.AccountId,
                -annuity.MonthlyPension, // Debit
                0,
                0);

            // Log LedgerEntry locally into PV_FundOpsDb
            await _ledgerRepo.AddEntryAsync(new LedgerEntry
            {
                AccountId = account.AccountId,
                EntryType = EntryType.AnnuityDebit,
                Amount = annuity.MonthlyPension,
                BalanceAfter = updatedAccount?.TotalBalance ?? (account.TotalBalance - annuity.MonthlyPension),
                ReferenceId = disbursement.DisbursementId.ToString(),
                Status = LedgerEntryStatus.Posted
            });
        }

        await _unitOfWork.SaveChangesAsync();

        await _notificationClient.SendAsync(
            member.UserId,
            $"Monthly pension of ₹{netAmount:N2} has been disbursed for period {request.Month}/{request.Year}.",
            "Annuity");

        return ToResponse(disbursement, member.Name);
    }

    public async Task<AnnuityResponse> ProcessNomineeSettlementAsync(Guid annuityId, NomineeSettlementRequest request)
    {
        var annuity = await _annuityRepo.FindByIdAsync(annuityId)
            ?? throw new KeyNotFoundException("Annuity not found.");

        if (annuity.Status != AnnuityStatus.Active && annuity.Status != AnnuityStatus.Suspended)
            throw new InvalidOperationException("Annuity cannot be settled in its current state.");

        var member = await _employerMemberClient.GetMemberAsync(annuity.MemberId)
            ?? throw new KeyNotFoundException("Member not found.");

        annuity.Status = AnnuityStatus.NomineeSettled; // Settled
        annuity.NomineeDetails = $"{request.NomineeName} (Settled to {request.BankAccountRef})";

        var account = await _employerMemberClient.GetActiveFundAccountAsync(annuity.MemberId);
        if (account != null)
        {
            var updatedAccount = await _employerMemberClient.UpdateBalanceAsync(
                account.AccountId,
                -request.SettlementAmount, // Debit
                0,
                0);

            // Log LedgerEntry locally into PV_FundOpsDb
            await _ledgerRepo.AddEntryAsync(new LedgerEntry
            {
                AccountId = account.AccountId,
                EntryType = EntryType.AnnuityDebit,
                Amount = request.SettlementAmount,
                BalanceAfter = updatedAccount?.TotalBalance ?? (account.TotalBalance - request.SettlementAmount),
                ReferenceId = $"SETTLEMENT-{annuityId}",
                Status = LedgerEntryStatus.Posted
            });
        }

        await _unitOfWork.SaveChangesAsync();

        await _notificationClient.SendAsync(
            member.UserId,
            $"Annuity contract settled with nominee {request.NomineeName} for amount ₹{request.SettlementAmount:N2}.",
            "Annuity");

        return await GetAnnuityAsync(annuityId);
    }

    public async Task<AnnuityResponse> TerminateAnnuityAsync(Guid annuityId)
    {
        var annuity = await _annuityRepo.FindByIdAsync(annuityId)
            ?? throw new KeyNotFoundException("Annuity not found.");
        annuity.Status = AnnuityStatus.Terminated;
        await _unitOfWork.SaveChangesAsync();
        return await GetAnnuityAsync(annuityId);
    }

    private static PensionDisbursementResponse ToResponse(MonthlyPensionDisbursement d, string memberName) => new(
        d.DisbursementId, d.AnnuityId, d.MemberId, memberName,
        d.Month, d.Year, d.GrossAmount, d.TaxDeducted,
        d.NetAmount, d.DisbursedDate, d.Status);
}
