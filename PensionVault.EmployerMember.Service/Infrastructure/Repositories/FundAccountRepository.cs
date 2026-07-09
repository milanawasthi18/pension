using Microsoft.EntityFrameworkCore;
using PensionVault.EmployerMember.Service.Domain.Models;
using PensionVault.EmployerMember.Service.Domain.Interfaces;
using PensionVault.EmployerMember.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.Infrastructure.Repositories;

public class FundAccountRepository : IFundAccountRepository
{
    private readonly EmployerMemberDbContext _context;
    public FundAccountRepository(EmployerMemberDbContext context) => _context = context;

    public Task<FundAccount?> FindByIdAsync(Guid accountId)
        => _context.FundAccounts.Include(a => a.Scheme).FirstOrDefaultAsync(a => a.AccountId == accountId);

    public Task<FundAccount?> FindActiveByMemberAsync(Guid memberId)
        => _context.FundAccounts
            .Include(a => a.Scheme)
            .FirstOrDefaultAsync(a => a.MemberId == memberId && a.Status == Domain.Enums.FundAccountStatus.Active);

    public Task<List<FundAccount>> GetByMemberAsync(Guid memberId)
        => _context.FundAccounts
            .Include(a => a.Scheme)
            .Where(a => a.MemberId == memberId)
            .ToListAsync();

    public Task<bool> ExistsByMemberAsync(Guid memberId)
        => _context.FundAccounts.AnyAsync(a => a.MemberId == memberId);

    public async Task AddAsync(FundAccount account)
        => await _context.FundAccounts.AddAsync(account);
}
