using System;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Application.Dtos;
using PensionVault.EmployerMember.Service.Application.Interfaces;
using PensionVault.EmployerMember.Service.Domain.Interfaces;

namespace PensionVault.EmployerMember.Service.Application.Services;

public class FundAccountService : IFundAccountService
{
    private readonly IFundAccountRepository _accountRepo;
    private readonly IFundSchemeRepository _schemeRepo;
    private readonly IUnitOfWork _unitOfWork;

    public FundAccountService(
        IFundAccountRepository accountRepo,
        IFundSchemeRepository schemeRepo,
        IUnitOfWork unitOfWork)
    {
        _accountRepo = accountRepo;
        _schemeRepo = schemeRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<FundAccountResponse> GetByIdAsync(Guid accountId)
    {
        var a = await _accountRepo.FindByIdAsync(accountId)
            ?? throw new KeyNotFoundException($"FundAccount {accountId} not found.");
        return ToResponse(a);
    }

    public async Task<FundAccountResponse> GetActiveByMemberAsync(Guid memberId)
    {
        var a = await _accountRepo.FindActiveByMemberAsync(memberId)
            ?? throw new KeyNotFoundException($"No active FundAccount for member {memberId}.");
        return ToResponse(a);
    }

    public async Task<FundAccountResponse> UpdateBalanceAsync(Guid accountId, UpdateBalanceRequest request)
    {
        var a = await _accountRepo.FindByIdAsync(accountId)
            ?? throw new KeyNotFoundException($"FundAccount {accountId} not found.");

        a.TotalBalance += request.Delta;
        a.EmployeeContributionBalance += request.EmployeeDelta;
        a.EmployerContributionBalance += request.EmployerDelta;
        a.InterestAccrued += request.InterestDelta;

        await _unitOfWork.SaveChangesAsync();
        return ToResponse(a);
    }

    public async Task<FundAccountResponse> CreateDefaultAccountAsync(Guid memberId)
    {
        var defaultScheme = await _schemeRepo.GetFirstAsync()
            ?? throw new InvalidOperationException("No default scheme found.");

        var account = new Domain.Models.FundAccount
        {
            MemberId = memberId,
            SchemeId = defaultScheme.SchemeId,
            AccountOpenDate = DateTime.UtcNow,
            VestingPercent = 100,
            Status = Domain.Enums.FundAccountStatus.Active
        };
        await _accountRepo.AddAsync(account);
        await _unitOfWork.SaveChangesAsync();
        account.Scheme = defaultScheme;
        return ToResponse(account);
    }

    private static FundAccountResponse ToResponse(Domain.Models.FundAccount a) => new(
        a.AccountId, a.MemberId, a.SchemeId,
        a.Scheme?.SchemeName ?? "",
        a.AccountOpenDate,
        a.EmployeeContributionBalance,
        a.EmployerContributionBalance,
        a.InterestAccrued,
        a.TotalBalance,
        a.VestingPercent,
        a.Status.ToString());
}
