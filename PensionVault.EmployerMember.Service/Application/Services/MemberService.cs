using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Application.Dtos;
using PensionVault.EmployerMember.Service.Application.Interfaces;
using PensionVault.EmployerMember.Service.Domain.Models;
using PensionVault.EmployerMember.Service.Domain.Enums;
using PensionVault.EmployerMember.Service.Domain.Interfaces;
using PensionVault.EmployerMember.Service.HttpClients;

namespace PensionVault.EmployerMember.Service.Application.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepo;
    private readonly IEmployerRepository _employerRepo;
    private readonly IFundAccountRepository _accountRepo;
    private readonly IFundSchemeRepository _schemeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FundOpsClient _fundOpsClient;
    private readonly ClaimsInvestmentClient _claimsClient;
    private readonly NotificationClient _notificationClient;
    private readonly IdentityClient _identityClient;

    public MemberService(
        IMemberRepository memberRepo,
        IEmployerRepository employerRepo,
        IFundAccountRepository accountRepo,
        IFundSchemeRepository schemeRepo,
        IUnitOfWork unitOfWork,
        FundOpsClient fundOpsClient,
        ClaimsInvestmentClient claimsClient,
        NotificationClient notificationClient,
        IdentityClient identityClient)
    {
        _memberRepo = memberRepo;
        _employerRepo = employerRepo;
        _accountRepo = accountRepo;
        _schemeRepo = schemeRepo;
        _unitOfWork = unitOfWork;
        _fundOpsClient = fundOpsClient;
        _claimsClient = claimsClient;
        _notificationClient = notificationClient;
        _identityClient = identityClient;
    }

    public async Task<IEnumerable<MemberResponse>> GetAllAsync(Guid? employerId = null)
    {
        var members = await _memberRepo.GetAllAsync(employerId);
        return members.Select(ToResponse);
    }

    public async Task<MemberResponse> GetByIdAsync(Guid id)
    {
        var m = await _memberRepo.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"Member {id} not found.");
        return ToResponse(m);
    }

    public async Task<MemberResponse> GetByUserIdAsync(Guid userId)
    {
        var m = await _memberRepo.FindByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Member profile not found for the current user.");
        return ToResponse(m);
    }

    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request)
    {
        if (await _memberRepo.ExistsByMembershipNumberAsync(request.MembershipNumber))
            throw new InvalidOperationException("Membership number already exists.");

        var member = new Member
        {
            UserId = request.UserId,
            MembershipNumber = request.MembershipNumber,
            Name = request.Name,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            NationalIdRef = request.NationalIdRef,
            EmployerId = request.EmployerId,
            JoiningDate = request.JoiningDate,
            DateOfRetirement = request.DateOfRetirement ?? request.DateOfBirth.AddYears(60),
            NomineeDetails = request.NomineeDetails,
            Status = MemberStatus.Active
        };
        await _memberRepo.AddAsync(member);

        var employer = await _employerRepo.FindByIdAsync(request.EmployerId);
        if (employer != null) employer.EnrolledMemberCount++;

        var defaultScheme = await _schemeRepo.GetFirstAsync();
        if (defaultScheme != null)
        {
            await _accountRepo.AddAsync(new FundAccount
            {
                MemberId = member.MemberId,
                SchemeId = defaultScheme.SchemeId,
                AccountOpenDate = DateTime.UtcNow,
                VestingPercent = 100,
                Status = FundAccountStatus.Active
            });
        }

        await _unitOfWork.SaveChangesAsync();

        await _notificationClient.SendAsync(
            request.UserId,
            $"Welcome to PensionVault, {member.Name}! Your EPF account has been created successfully.",
            "Contribution");

        return await GetByIdAsync(member.MemberId);
    }

    public async Task<MemberResponse> UpdateAsync(Guid id, UpdateMemberRequest request)
    {
        var member = await _memberRepo.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"Member {id} not found.");
        member.Name = request.Name;
        member.Gender = request.Gender;
        member.NationalIdRef = request.NationalIdRef;
        member.DateOfRetirement = request.DateOfRetirement;
        member.NomineeDetails = request.NomineeDetails;
        member.Status = request.Status;
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<MemberResponse> SelfEnrollAsync(Guid userId, SelfEnrollMemberRequest request)
    {
        if (await _memberRepo.ExistsByUserIdAsync(userId))
            throw new InvalidOperationException("You have already submitted an enrollment profile.");

        // Validate user exists via Identity.Service
        var userInfo = await _identityClient.GetUserAsync(userId);

        var member = new Member
        {
            UserId = userId,
            MembershipNumber = $"PENDING-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Name = userInfo?.Name ?? "Unknown",
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            NationalIdRef = request.NationalIdRef,
            EmployerId = request.EmployerId,
            JoiningDate = DateTime.UtcNow,
            DateOfRetirement = request.DateOfBirth.AddYears(60),
            NomineeDetails = request.NomineeDetails,
            Status = MemberStatus.Active
        };
        await _memberRepo.AddAsync(member);

        var employer = await _employerRepo.FindByIdAsync(request.EmployerId);
        if (employer != null) employer.EnrolledMemberCount++;

        var defaultScheme = await _schemeRepo.GetFirstAsync();
        if (defaultScheme != null)
        {
            await _accountRepo.AddAsync(new FundAccount
            {
                MemberId = member.MemberId,
                SchemeId = defaultScheme.SchemeId,
                AccountOpenDate = DateTime.UtcNow,
                VestingPercent = 100,
                Status = FundAccountStatus.Active
            });
        }

        await _unitOfWork.SaveChangesAsync();

        // Notify admins — get admin users by role from Identity.Service
        var adminUserIds = await _identityClient.GetUsersByRoleAsync("Admin");
        foreach (var adminId in adminUserIds)
        {
            await _notificationClient.SendAsync(
                adminId,
                $"Employee {member.Name} has submitted their profile. Awaiting Membership Number assignment.",
                "Compliance");
        }

        return await GetByIdAsync(member.MemberId);
    }

    public async Task<MemberResponse> ApproveAsync(Guid id, ApproveMemberRequest request)
    {
        var member = await _memberRepo.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("Member not found.");

        if (await _memberRepo.ExistsByMembershipNumberAsync(request.MembershipNumber, id))
            throw new InvalidOperationException("Membership number already exists.");

        member.MembershipNumber = request.MembershipNumber;

        if (member.EmployerId != request.EmployerId)
        {
            var oldEmp = await _employerRepo.FindByIdAsync(member.EmployerId);
            if (oldEmp != null) oldEmp.EnrolledMemberCount--;
            member.EmployerId = request.EmployerId;
            var newEmp = await _employerRepo.FindByIdAsync(request.EmployerId);
            if (newEmp != null) newEmp.EnrolledMemberCount++;
        }

        await _unitOfWork.SaveChangesAsync();

        await _notificationClient.SendAsync(
            member.UserId,
            $"Your profile has been approved! Your Membership Number is {member.MembershipNumber}.",
            "Compliance");

        return await GetByIdAsync(id);
    }

    public async Task<IEnumerable<FundAccountResponse>> GetFundAccountsAsync(Guid memberId)
    {
        var accounts = await _accountRepo.GetByMemberAsync(memberId);
        return accounts.Select(a => new FundAccountResponse(
            a.AccountId, a.MemberId, a.SchemeId,
            a.Scheme?.SchemeName ?? "",
            a.AccountOpenDate, a.EmployeeContributionBalance,
            a.EmployerContributionBalance, a.InterestAccrued,
            a.TotalBalance, a.VestingPercent, a.Status.ToString()));
    }

    public async Task<IEnumerable<object>> GetContributionsAsync(Guid memberId)
        => await _fundOpsClient.GetMemberContributionsAsync(memberId);

    public async Task<IEnumerable<object>> GetLedgerAsync(Guid memberId)
    {
        var accounts = await _accountRepo.GetByMemberAsync(memberId);
        var allEntries = new List<object>();
        foreach (var acc in accounts)
            allEntries.AddRange(await _fundOpsClient.GetLedgerByAccountAsync(acc.AccountId));
        return allEntries;
    }

    public async Task<IEnumerable<object>> GetClaimsAsync(Guid memberId)
        => await _claimsClient.GetClaimsByMemberAsync(memberId);

    private static MemberResponse ToResponse(Member m) => new(
        m.MemberId, m.MembershipNumber, m.Name, m.DateOfBirth,
        m.Gender, m.NationalIdRef, m.EmployerId,
        m.Employer?.CompanyName ?? "", m.JoiningDate,
        m.DateOfRetirement, m.NomineeDetails, m.Status, null);
}
