using Microsoft.EntityFrameworkCore;
using PensionVault.Identity.Service.Domain.Models;
using PensionVault.Identity.Service.Domain.Enums;
using PensionVault.Identity.Service.Domain.Interfaces;
using PensionVault.Identity.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.Identity.Service.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
    public UserRepository(IdentityDbContext context) => _context = context;

    public Task<User?> FindByIdAsync(Guid userId)
        => _context.Users.FindAsync(userId).AsTask();

    public Task<User?> FindByEmailAsync(string email)
        => _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> FindByRefreshTokenAsync(string refreshToken)
        => _context.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

    public Task<bool> ExistsByEmailAsync(string email)
        => _context.Users.AnyAsync(u => u.Email == email);

    public Task<List<User>> GetByRoleAsync(UserRole role)
        => _context.Users.Where(u => u.Role == role).ToListAsync();

    public Task<List<User>> GetByOrgAndRoleAsync(Guid organisationId, UserRole role)
        => _context.Users
            .Where(u => u.OrganisationId == organisationId && u.Role == role)
            .ToListAsync();

    public async Task AddAsync(User user)
        => await _context.Users.AddAsync(user);
}
