using Microsoft.EntityFrameworkCore;
using PensionVault.EmployerMember.Service.Domain.Models;
using PensionVault.EmployerMember.Service.Domain.Interfaces;
using PensionVault.EmployerMember.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly EmployerMemberDbContext _context;
    public MemberRepository(EmployerMemberDbContext context) => _context = context;

    public Task<Member?> FindByIdAsync(Guid memberId)
        => _context.Members
            .Include(m => m.Employer)
            .FirstOrDefaultAsync(m => m.MemberId == memberId);

    public Task<Member?> FindByUserIdAsync(Guid userId)
        => _context.Members
            .Include(m => m.Employer)
            .FirstOrDefaultAsync(m => m.UserId == userId);

    public Task<List<Member>> GetAllAsync(Guid? employerId = null)
    {
        var query = _context.Members.Include(m => m.Employer).AsQueryable();
        if (employerId.HasValue)
            query = query.Where(m => m.EmployerId == employerId.Value);
        return query.ToListAsync();
    }

    public Task<bool> ExistsByMembershipNumberAsync(string membershipNumber, Guid? excludeId = null)
    {
        var query = _context.Members.Where(m => m.MembershipNumber == membershipNumber);
        if (excludeId.HasValue)
            query = query.Where(m => m.MemberId != excludeId.Value);
        return query.AnyAsync();
    }

    public Task<bool> ExistsByUserIdAsync(Guid userId)
        => _context.Members.AnyAsync(m => m.UserId == userId);

    public async Task AddAsync(Member member)
        => await _context.Members.AddAsync(member);
}
