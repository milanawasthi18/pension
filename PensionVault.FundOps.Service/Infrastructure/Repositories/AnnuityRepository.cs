using Microsoft.EntityFrameworkCore;
using PensionVault.FundOps.Service.Domain.Models;
using PensionVault.FundOps.Service.Domain.Interfaces;
using PensionVault.FundOps.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.FundOps.Service.Infrastructure.Repositories;

public class AnnuityRepository : IAnnuityRepository
{
    private readonly FundOpsDbContext _context;
    public AnnuityRepository(FundOpsDbContext context) => _context = context;

    public Task<AnnuityPlan?> FindByIdAsync(Guid annuityId)
        => _context.AnnuityPlans
            .FirstOrDefaultAsync(a => a.AnnuityId == annuityId);

    public Task<List<AnnuityPlan>> GetAllAsync()
        => _context.AnnuityPlans
            .OrderByDescending(a => a.AnnuityStartDate)
            .ToListAsync();

    public Task<List<MonthlyPensionDisbursement>> GetDisbursementsAsync(Guid annuityId)
        => _context.MonthlyPensionDisbursements
            .Where(d => d.AnnuityId == annuityId)
            .OrderByDescending(d => d.Year).ThenByDescending(d => d.Month)
            .ToListAsync();

    public Task<MonthlyPensionDisbursement?> FindDisbursementByIdAsync(Guid disbursementId)
        => _context.MonthlyPensionDisbursements
            .FirstOrDefaultAsync(d => d.DisbursementId == disbursementId);

    public async Task AddAsync(AnnuityPlan plan)
        => await _context.AnnuityPlans.AddAsync(plan);

    public async Task AddDisbursementAsync(MonthlyPensionDisbursement disbursement)
        => await _context.MonthlyPensionDisbursements.AddAsync(disbursement);
}
