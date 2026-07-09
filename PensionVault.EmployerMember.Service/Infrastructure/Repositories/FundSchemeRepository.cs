using Microsoft.EntityFrameworkCore;
using PensionVault.EmployerMember.Service.Domain.Models;
using PensionVault.EmployerMember.Service.Domain.Interfaces;
using PensionVault.EmployerMember.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.Infrastructure.Repositories;

public class FundSchemeRepository : IFundSchemeRepository
{
    private readonly EmployerMemberDbContext _context;
    public FundSchemeRepository(EmployerMemberDbContext context) => _context = context;

    public Task<FundScheme?> FindByIdAsync(Guid schemeId)
        => _context.FundSchemes.FindAsync(schemeId).AsTask();

    public Task<List<FundScheme>> GetAllAsync()
        => _context.FundSchemes.ToListAsync();

    public Task<FundScheme?> GetFirstAsync()
        => _context.FundSchemes.FirstOrDefaultAsync();

    public async Task AddAsync(FundScheme scheme)
        => await _context.FundSchemes.AddAsync(scheme);
}
