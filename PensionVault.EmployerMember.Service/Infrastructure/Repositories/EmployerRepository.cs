using Microsoft.EntityFrameworkCore;
using PensionVault.EmployerMember.Service.Domain.Models;
using PensionVault.EmployerMember.Service.Domain.Interfaces;
using PensionVault.EmployerMember.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.Infrastructure.Repositories;

public class EmployerRepository : IEmployerRepository
{
    private readonly EmployerMemberDbContext _context;
    public EmployerRepository(EmployerMemberDbContext context) => _context = context;

    public Task<Employer?> FindByIdAsync(Guid employerId)
        => _context.Employers.Include(e => e.Members).FirstOrDefaultAsync(e => e.EmployerId == employerId);

    public Task<List<Employer>> GetAllAsync()
        => _context.Employers.ToListAsync();

    public Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber)
        => _context.Employers.AnyAsync(e => e.RegistrationNumber == registrationNumber);

    public async Task AddAsync(Employer employer)
        => await _context.Employers.AddAsync(employer);
}
