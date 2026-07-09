using System;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Application.Dtos;

namespace PensionVault.EmployerMember.Service.Application.Interfaces;

public interface IFundAccountService
{
    Task<FundAccountResponse> GetByIdAsync(Guid accountId);
    Task<FundAccountResponse> GetActiveByMemberAsync(Guid memberId);
    Task<FundAccountResponse> UpdateBalanceAsync(Guid accountId, UpdateBalanceRequest request);
    Task<FundAccountResponse> CreateDefaultAccountAsync(Guid memberId);
}
