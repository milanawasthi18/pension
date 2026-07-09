using Microsoft.EntityFrameworkCore;
using PensionVault.ClaimsInvestment.Service.Domain.Models;
using PensionVault.ClaimsInvestment.Service.Domain.Interfaces;
using PensionVault.ClaimsInvestment.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PensionVault.ClaimsInvestment.Service.Infrastructure.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly ClaimsInvestmentDbContext _context;
    public ClaimRepository(ClaimsInvestmentDbContext context) => _context = context;

    public Task<BenefitClaim?> FindByIdAsync(Guid claimId)
        => _context.BenefitClaims
            .Include(c => c.Disbursements)
            .FirstOrDefaultAsync(c => c.ClaimId == claimId);

    public Task<List<BenefitClaim>> GetAllAsync()
        => _context.BenefitClaims
            .Include(c => c.Disbursements)
            .ToListAsync();

    public async Task AddAsync(BenefitClaim claim)
        => await _context.BenefitClaims.AddAsync(claim);

    public async Task AddDisbursementAsync(ClaimDisbursement disbursement)
        => await _context.ClaimDisbursements.AddAsync(disbursement);
}
