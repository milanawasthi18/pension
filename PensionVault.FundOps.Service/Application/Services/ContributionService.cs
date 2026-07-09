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

public class ContributionService : IContributionService
{
    private readonly IContributionRepository _contributionRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmployerMemberClient _employerMemberClient;
    private readonly NotificationClient _notificationClient;

    public ContributionService(
        IContributionRepository contributionRepo,
        ILedgerRepository ledgerRepo,
        IUnitOfWork unitOfWork,
        EmployerMemberClient employerMemberClient,
        NotificationClient notificationClient)
    {
        _contributionRepo = contributionRepo;
        _ledgerRepo = ledgerRepo;
        _unitOfWork = unitOfWork;
        _employerMemberClient = employerMemberClient;
        _notificationClient = notificationClient;
    }

    public async Task<RemittanceResponse> CreateRemittanceAsync(CreateRemittanceRequest request)
    {
        var employer = await _employerMemberClient.GetEmployerAsync(request.EmployerId)
            ?? throw new KeyNotFoundException("Employer not found.");

        var total = request.TotalEmployeeShare + request.TotalEmployerShare;
        var remittance = new ContributionRemittance
        {
            EmployerId = request.EmployerId,
            RemittancePeriod = request.RemittancePeriod,
            TotalEmployeeShare = request.TotalEmployeeShare,
            TotalEmployerShare = request.TotalEmployerShare,
            TotalAmount = total,
            RemittanceDate = DateTime.UtcNow,
            CoverageCount = request.CoverageCount,
            Status = RemittanceStatus.Received
        };
        await _contributionRepo.AddRemittanceAsync(remittance);

        var notifications = new List<NotificationClient.NotificationItem>();

        foreach (var item in request.MemberContributions)
        {
            var contribution = new MemberContribution
            {
                RemittanceId = remittance.RemittanceId,
                MemberId = item.MemberId,
                Period = request.RemittancePeriod,
                EmployeeAmount = item.EmployeeAmount,
                EmployerAmount = item.EmployerAmount,
                TotalAmount = item.EmployeeAmount + item.EmployerAmount,
                PostedDate = DateTime.UtcNow,
                Status = ContributionStatus.Posted
            };
            await _contributionRepo.AddContributionAsync(contribution);

            // Fetch active account from EmployerMember service
            var account = await _employerMemberClient.GetActiveFundAccountAsync(item.MemberId);
            if (account != null)
            {
                // Put update to EmployerMember service
                var updatedAccount = await _employerMemberClient.UpdateBalanceAsync(
                    account.AccountId,
                    contribution.TotalAmount,
                    item.EmployeeAmount,
                    item.EmployerAmount);

                // Write LedgerEntry locally into PV_FundOpsDb
                await _ledgerRepo.AddEntryAsync(new LedgerEntry
                {
                    AccountId = account.AccountId,
                    EntryType = EntryType.ContributionCredit,
                    Amount = contribution.TotalAmount,
                    BalanceAfter = updatedAccount?.TotalBalance ?? (account.TotalBalance + contribution.TotalAmount),
                    ReferenceId = remittance.RemittanceId.ToString(),
                    Status = LedgerEntryStatus.Posted
                });
            }

            // Fetch member info from EmployerMember service to notify
            var member = await _employerMemberClient.GetMemberAsync(item.MemberId);
            if (member != null)
            {
                notifications.Add(new NotificationClient.NotificationItem(
                    member.UserId,
                    $"A contribution of ₹{item.EmployeeAmount + item.EmployerAmount:N2} has been posted to your account for period {request.RemittancePeriod}.",
                    "Contribution"));
            }
        }

        // Fetch employer users from EmployerMember service
        var employerUsers = await _employerMemberClient.GetEmployerUsersAsync(request.EmployerId);
        foreach (var u in employerUsers)
        {
            notifications.Add(new NotificationClient.NotificationItem(
                u.UserId,
                $"Remittance of ₹{total:N2} for period {request.RemittancePeriod} has been submitted successfully.",
                "Contribution"));
        }

        await _unitOfWork.SaveChangesAsync();

        // Dispatch all notifications in background asynchronously
        if (notifications.Count > 0)
        {
            await _notificationClient.SendBatchAsync(notifications);
        }

        return await GetRemittanceAsync(remittance.RemittanceId);
    }

    public async Task<RemittanceResponse> GetRemittanceAsync(Guid remittanceId)
    {
        var r = await _contributionRepo.FindRemittanceByIdAsync(remittanceId)
            ?? throw new KeyNotFoundException("Remittance not found.");
        var employer = await _employerMemberClient.GetEmployerAsync(r.EmployerId);
        return ToResponse(r, employer?.CompanyName ?? "Unknown");
    }

    public async Task<IEnumerable<RemittanceResponse>> GetEmployerRemittancesAsync(Guid employerId)
    {
        var remittances = await _contributionRepo.GetByEmployerAsync(employerId);
        var employer = await _employerMemberClient.GetEmployerAsync(employerId);
        var name = employer?.CompanyName ?? "Unknown";
        return remittances.Select(r => ToResponse(r, name));
    }

    public async Task<RemittanceResponse> ReconcileAsync(Guid remittanceId)
    {
        var remittance = await _contributionRepo.FindRemittanceByIdAsync(remittanceId)
            ?? throw new KeyNotFoundException("Remittance not found.");

        var postedCount = await _contributionRepo.CountPostedContributionsAsync(remittanceId);
        remittance.Status = postedCount == remittance.CoverageCount
            ? RemittanceStatus.Reconciled
            : RemittanceStatus.Shortfall;

        var employerUsers = await _employerMemberClient.GetEmployerUsersAsync(remittance.EmployerId);
        var notifications = employerUsers.Select(u => new NotificationClient.NotificationItem(
            u.UserId,
            $"Your remittance for period {remittance.RemittancePeriod} has been reconciled. Status: {remittance.Status}.",
            "Contribution"
        )).ToList();

        await _unitOfWork.SaveChangesAsync();

        if (notifications.Count > 0)
        {
            await _notificationClient.SendBatchAsync(notifications);
        }

        return await GetRemittanceAsync(remittanceId);
    }

    public async Task<IEnumerable<RemittanceResponse>> GetAllRemittancesAsync()
    {
        var remittances = await _contributionRepo.GetAllRemittancesAsync();
        var responses = new List<RemittanceResponse>();
        foreach (var r in remittances)
        {
            var employer = await _employerMemberClient.GetEmployerAsync(r.EmployerId);
            responses.Add(ToResponse(r, employer?.CompanyName ?? "Unknown"));
        }
        return responses;
    }

    public async Task<IEnumerable<MemberContributionResponse>> GetMemberContributionsAsync(Guid memberId)
    {
        var contributions = await _contributionRepo.GetByMemberAsync(memberId);
        var member = await _employerMemberClient.GetMemberAsync(memberId);
        var name = member?.Name ?? "Member";
        return contributions.Select(c => new MemberContributionResponse(
            c.ContributionId, c.MemberId, name,
            c.Period, c.EmployeeAmount, c.EmployerAmount,
            c.TotalAmount, c.PostedDate, c.Status));
    }

    public async Task<ReconciliationReportResponse> GetReconciliationReportAsync(Guid remittanceId)
    {
        var remittance = await _contributionRepo.FindRemittanceByIdAsync(remittanceId)
            ?? throw new KeyNotFoundException("Remittance not found.");

        var reconciledCount = await _contributionRepo.CountPostedContributionsAsync(remittanceId);
        var reconciledAmount = await _contributionRepo.SumReconciledAmountAsync(remittanceId);

        return new ReconciliationReportResponse(
            remittance.RemittanceId, remittance.RemittancePeriod,
            remittance.CoverageCount, reconciledCount,
            remittance.TotalAmount, reconciledAmount,
            remittance.Status.ToString());
    }

    public async Task<IEnumerable<RemittanceResponse>> GetDefaultersAsync()
    {
        // RemittanceStatus might need to map shortfall/default
        // Since we consolidated enums, we query shortfall & Received/Pending ones.
        var defaults = await _contributionRepo.GetByStatusesAsync(RemittanceStatus.Shortfall);
        var responses = new List<RemittanceResponse>();
        foreach (var r in defaults)
        {
            var employer = await _employerMemberClient.GetEmployerAsync(r.EmployerId);
            responses.Add(ToResponse(r, employer?.CompanyName ?? "Unknown"));
        }
        return responses;
    }

    public async Task<IEnumerable<RemittanceResponse>> GetOverdueRemittancesAsync()
    {
        var overdue = await _contributionRepo.GetByStatusesAsync(
            RemittanceStatus.Received, RemittanceStatus.Shortfall);
        var responses = new List<RemittanceResponse>();
        foreach (var r in overdue)
        {
            var employer = await _employerMemberClient.GetEmployerAsync(r.EmployerId);
            responses.Add(ToResponse(r, employer?.CompanyName ?? "Unknown"));
        }
        return responses;
    }

    public async Task<DefaulterSummaryResponse> GetDefaulterSummaryAsync(Guid employerId)
    {
        var employer = await _employerMemberClient.GetEmployerAsync(employerId)
            ?? throw new KeyNotFoundException("Employer not found.");

        var missingOrShortfall = await _contributionRepo.GetByStatusesAsync(RemittanceStatus.Shortfall);
        var employerDefaults = missingOrShortfall.Where(r => r.EmployerId == employerId).ToList();

        var allRemittances = await _contributionRepo.GetByEmployerAsync(employerId);
        var lastRemittance = allRemittances.FirstOrDefault();

        return new DefaulterSummaryResponse(
            employerId, employer.CompanyName,
            employerDefaults.Count,
            employerDefaults.Sum(r => r.TotalAmount),
            lastRemittance?.RemittancePeriod ?? "None");
    }

    private static RemittanceResponse ToResponse(ContributionRemittance r, string employerName) => new(
        r.RemittanceId, r.EmployerId, employerName,
        r.RemittancePeriod, r.TotalEmployeeShare, r.TotalEmployerShare,
        r.TotalAmount, r.RemittanceDate, r.CoverageCount, r.Status);
}
