using Microsoft.EntityFrameworkCore;
using PensionVault.Identity.Service.Domain.Models;
using PensionVault.Identity.Service.Domain.Interfaces;
using PensionVault.Identity.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.Identity.Service.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IdentityDbContext _context;
    public AuditLogRepository(IdentityDbContext context) => _context = context;

    public async Task AddAsync(AuditLog auditLog)
        => await _context.AuditLogs.AddAsync(auditLog);

    public Task<List<AuditLog>> GetFilteredAsync(string? entityType, DateTime? from, DateTime? to)
    {
        var query = _context.AuditLogs.Include(a => a.User).AsQueryable();
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (from.HasValue) query = query.Where(a => a.Timestamp >= from);
        if (to.HasValue) query = query.Where(a => a.Timestamp <= to);

        return query.OrderByDescending(a => a.Timestamp).Take(1000).ToListAsync();
    }
}
