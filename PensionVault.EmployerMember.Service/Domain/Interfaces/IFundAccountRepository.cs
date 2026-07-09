using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Domain.Models;

namespace PensionVault.EmployerMember.Service.Domain.Interfaces;

public interface IFundAccountRepository
{
    Task<FundAccount?> FindByIdAsync(Guid accountId);
    Task<FundAccount?> FindActiveByMemberAsync(Guid memberId);
    Task<List<FundAccount>> GetByMemberAsync(Guid memberId);
    Task<bool> ExistsByMemberAsync(Guid memberId);
    Task AddAsync(FundAccount account);
}
