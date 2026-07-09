using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Domain.Models;

namespace PensionVault.EmployerMember.Service.Domain.Interfaces;

public interface IFundSchemeRepository
{
    Task<FundScheme?> FindByIdAsync(Guid schemeId);
    Task<List<FundScheme>> GetAllAsync();
    Task<FundScheme?> GetFirstAsync();
    Task AddAsync(FundScheme scheme);
}
