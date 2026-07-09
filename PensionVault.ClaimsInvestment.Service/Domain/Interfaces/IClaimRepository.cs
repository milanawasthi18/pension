using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.ClaimsInvestment.Service.Domain.Models;

namespace PensionVault.ClaimsInvestment.Service.Domain.Interfaces;

public interface IClaimRepository
{
    Task<BenefitClaim?> FindByIdAsync(Guid claimId);
    Task<List<BenefitClaim>> GetAllAsync();
    Task AddAsync(BenefitClaim claim);
    Task AddDisbursementAsync(ClaimDisbursement disbursement);
}
