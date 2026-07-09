using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.Identity.Service.Domain.Models;

namespace PensionVault.Identity.Service.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetFilteredAsync(string? entityType, DateTime? from, DateTime? to);
}
