using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionVault.ClaimsInvestment.Service.Application.Dtos;
using PensionVault.ClaimsInvestment.Service.Application.Interfaces;
using PensionVault.ClaimsInvestment.Service.Domain.Models;
using PensionVault.ClaimsInvestment.Service.Domain.Enums;
using PensionVault.ClaimsInvestment.Service.Domain.Interfaces;
using PensionVault.ClaimsInvestment.Service.HttpClients;

namespace PensionVault.ClaimsInvestment.Service.Application.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _claimRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmployerMemberClient _employerMemberClient;
    private readonly FundOpsClient _fundOpsClient;
    private readonly NotificationClient _notificationClient;

    public ClaimService(
        IClaimRepository claimRepo,
        IUnitOfWork unitOfWork,
        EmployerMemberClient employerMemberClient,
        FundOpsClient fundOpsClient,
        NotificationClient notificationClient)
    {
        _claimRepo = claimRepo;
        _unitOfWork = unitOfWork;
        _employerMemberClient = employerMemberClient;
        _fundOpsClient = fundOpsClient;
        _notificationClient = notificationClient;
    }

    public async Task<ClaimResponse> SubmitClaimAsync(CreateClaimRequest request)
    {
        var member = await _employerMemberClient.GetMemberAsync(request.MemberId)
            ?? throw new KeyNotFoundException("Member not found.");

        var account = await _employerMemberClient.GetActiveFundAccountAsync(request.MemberId);
        var vestedAmount = account != null
            ? Math.Round(account.TotalBalance * (account.VestingPercent / 100m), 2)
            : 0;

        var claim = new BenefitClaim
        {
            MemberId = request.MemberId,
            ClaimType = request.ClaimType,
            ClaimDate = DateTime.UtcNow,
            EligibleAmount = request.EligibleAmount,
            VestedAmount = vestedAmount,
            TaxDeductible = Math.Round(request.EligibleAmount * 0.10m, 2),
            Status = ClaimStatus.Submitted
        };
        await _claimRepo.AddAsync(claim);
        await _unitOfWork.SaveChangesAsync();

        await _notificationClient.SendAsync(
            member.UserId,
            $"Your benefit claim of type {claim.ClaimType} for ₹{claim.EligibleAmount:N2} has been submitted and is under review.",
            "Claim");

        return await GetClaimAsync(claim.ClaimId);
    }

    public async Task<IEnumerable<ClaimResponse>> GetAllClaimsAsync()
    {
        var claims = await _claimRepo.GetAllAsync();
        var responses = new List<ClaimResponse>();
        foreach (var c in claims)
        {
            var member = await _employerMemberClient.GetMemberAsync(c.MemberId);
            responses.Add(ToResponse(c, member?.Name ?? "Unknown"));
        }
        return responses;
    }

    public async Task<IEnumerable<ClaimResponse>> GetClaimsByMemberAsync(Guid memberId)
    {
        var all = await _claimRepo.GetAllAsync();
        var member = await _employerMemberClient.GetMemberAsync(memberId);
        var name = member?.Name ?? "Unknown";
        return all.Where(c => c.MemberId == memberId).Select(c => ToResponse(c, name));
    }

    public async Task<ClaimResponse> GetClaimAsync(Guid claimId)
    {
        var claim = await _claimRepo.FindByIdAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");
        var member = await _employerMemberClient.GetMemberAsync(claim.MemberId);
        return ToResponse(claim, member?.Name ?? "Unknown");
    }

    public Task<ClaimResponse> ReviewClaimAsync(Guid claimId, ClaimActionRequest request)
        => UpdateStatusAsync(claimId, ClaimStatus.UnderReview);

    public Task<ClaimResponse> ApproveClaimAsync(Guid claimId, ClaimActionRequest request)
        => UpdateStatusAsync(claimId, ClaimStatus.Approved);

    public Task<ClaimResponse> RejectClaimAsync(Guid claimId, ClaimActionRequest request)
        => UpdateStatusAsync(claimId, ClaimStatus.Rejected);

    public async Task<DisbursementResponse> DisburseClaimAsync(Guid claimId, DisburseClaimRequest request)
    {
        var claim = await _claimRepo.FindByIdAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");
        if (claim.Status != ClaimStatus.Approved)
            throw new InvalidOperationException("Claim must be Approved before disbursement.");

        var disbursement = new ClaimDisbursement
        {
            ClaimId = claimId,
            MemberId = claim.MemberId,
            DisbursedAmount = request.DisbursedAmount,
            TaxDeducted = request.TaxDeducted,
            NetAmount = request.DisbursedAmount - request.TaxDeducted,
            BankAccountRef = request.BankAccountRef,
            DisbursedDate = DateTime.UtcNow,
            Status = DisbursementStatus.Disbursed
        };
        await _claimRepo.AddDisbursementAsync(disbursement);
        claim.Status = ClaimStatus.Disbursed;

        // Deduct from fund balance via EmployerMember service
        var account = await _employerMemberClient.GetActiveFundAccountAsync(claim.MemberId);
        if (account != null)
        {
            var updatedAccount = await _employerMemberClient.UpdateBalanceAsync(
                account.AccountId,
                -disbursement.NetAmount, 0, 0);

            // Write ledger debit entry via FundOps service
            await _fundOpsClient.WriteLedgerEntryAsync(
                account.AccountId,
                "ClaimDebit",
                disbursement.NetAmount,
                updatedAccount?.TotalBalance ?? (account.TotalBalance - disbursement.NetAmount),
                claimId.ToString());
        }

        await _unitOfWork.SaveChangesAsync();

        var member = await _employerMemberClient.GetMemberAsync(claim.MemberId);
        if (member != null)
        {
            await _notificationClient.SendAsync(
                member.UserId,
                $"Your claim payout of ₹{disbursement.NetAmount:N2} has been disbursed to account {disbursement.BankAccountRef}.",
                "Claim");
        }

        return new DisbursementResponse(
            disbursement.DisbursementId, disbursement.ClaimId,
            disbursement.DisbursedAmount, disbursement.TaxDeducted,
            disbursement.NetAmount, disbursement.BankAccountRef,
            disbursement.DisbursedDate, disbursement.Status);
    }

    public async Task<ClaimResponse> SubmitPartialWithdrawalAsync(CreatePartialWithdrawalRequest request)
    {
        var member = await _employerMemberClient.GetMemberAsync(request.MemberId)
            ?? throw new KeyNotFoundException("Member not found.");

        var claim = new BenefitClaim
        {
            MemberId = request.MemberId,
            ClaimType = ClaimType.PartialWithdrawal,
            ClaimDate = DateTime.UtcNow,
            EligibleAmount = request.RequestedAmount,
            VestedAmount = request.RequestedAmount,
            TaxDeductible = 0,
            Status = ClaimStatus.Submitted
        };
        await _claimRepo.AddAsync(claim);
        await _unitOfWork.SaveChangesAsync();

        await _notificationClient.SendAsync(
            member.UserId,
            $"Your partial withdrawal claim for ₹{claim.EligibleAmount:N2} due to {request.Reason} has been submitted.",
            "Claim");

        return await GetClaimAsync(claim.ClaimId);
    }

    public async Task<DisbursementResponse> DisbursePartialWithdrawalAsync(Guid claimId, DisbursePartialWithdrawalRequest request)
        => await DisburseClaimAsync(claimId, new DisburseClaimRequest(request.DisbursedAmount, 0, request.BankAccountRef));

    private async Task<ClaimResponse> UpdateStatusAsync(Guid claimId, ClaimStatus status)
    {
        var claim = await _claimRepo.FindByIdAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");
        claim.Status = status;
        await _unitOfWork.SaveChangesAsync();

        var member = await _employerMemberClient.GetMemberAsync(claim.MemberId);
        if (member != null)
        {
            string message = status switch
            {
                ClaimStatus.UnderReview => $"Your claim of type {claim.ClaimType} is now under review.",
                ClaimStatus.Approved => $"Congratulations! Your claim of type {claim.ClaimType} for ₹{claim.EligibleAmount:N2} has been APPROVED.",
                ClaimStatus.Rejected => $"Your claim of type {claim.ClaimType} has been rejected. Please contact your fund administrator.",
                _ => $"Your claim status has been updated to {status}."
            };
            await _notificationClient.SendAsync(member.UserId, message, "Claim");
        }

        return await GetClaimAsync(claimId);
    }

    private static ClaimResponse ToResponse(BenefitClaim c, string memberName) => new(
        c.ClaimId, c.MemberId, memberName,
        c.ClaimType, c.ClaimDate, c.EligibleAmount,
        c.VestedAmount, c.TaxDeductible,
        c.ProcessedById?.ToString(), c.Status);
}
