using System;
using PensionVault.Identity.Service.Domain.Enums;

namespace PensionVault.Identity.Service.Domain.Models;

public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public Guid? OrganisationId { get; set; }
    public string? EmployeeId { get; set; }
    public string? ProfileImageUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Navigation
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
