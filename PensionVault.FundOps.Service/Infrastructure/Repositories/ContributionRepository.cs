using Microsoft.EntityFrameworkCore;
using PensionVault.FundOps.Service.Domain.Models;
using PensionVault.FundOps.Service.Domain.Enums;
using PensionVault.FundOps.Service.Domain.Interfaces;
using PensionVault.FundOps.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.FundOps.Service.Infrastructure.Repositories;

public class ContributionRepository : IContributionRepository
{
    private readonly FundOpsDbContext _context;
    public ContributionRepository(FundOpsDbContext context) => _context = context;

    public Task<ContributionRemittance?> FindRemittanceByIdAsync(Guid remittanceId)
        => _context.ContributionRemittances
            .FirstOrDefaultAsync(r => r.RemittanceId == remittanceId);

    public Task<List<ContributionRemittance>> GetAllRemittancesAsync()
        => _context.ContributionRemittances
            .OrderByDescending(r => r.RemittanceDate)
            .ToListAsync();

    public Task<List<ContributionRemittance>> GetByEmployerAsync(Guid employerId)
        => _context.ContributionRemittances
            .Where(r => r.EmployerId == employerId)
            .OrderByDescending(r => r.RemittanceDate)
            .ToListAsync();

    public Task<List<ContributionRemittance>> GetByStatusesAsync(params RemittanceStatus[] statuses)
        => _context.ContributionRemittances
            .Where(r => statuses.Contains(r.Status))
            .OrderByDescending(r => r.RemittanceDate)
            .ToListAsync();

    public Task<int> CountPostedContributionsAsync(Guid remittanceId)
        => _context.MemberContributions
            .CountAsync(c => c.RemittanceId == remittanceId && c.Status == ContributionStatus.Posted);

    public Task<decimal> SumReconciledAmountAsync(Guid remittanceId)
        => _context.MemberContributions
            .Where(c => c.RemittanceId == remittanceId && c.Status == ContributionStatus.Posted)
            .SumAsync(c => c.TotalAmount);

    public Task<List<MemberContribution>> GetByMemberAsync(Guid memberId)
        => _context.MemberContributions
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.PostedDate)
            .ToListAsync();

    public async Task AddRemittanceAsync(ContributionRemittance remittance)
        => await _context.ContributionRemittances.AddAsync(remittance);

    public async Task AddContributionAsync(MemberContribution contribution)
        => await _context.MemberContributions.AddAsync(contribution);
}
