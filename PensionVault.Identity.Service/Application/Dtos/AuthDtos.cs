using System;

namespace PensionVault.Identity.Service.Application.Dtos;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Name, string Email, string Password, string Role, string? Phone = null, Guid? OrganisationId = null, string? EmployeeId = null);
public record RefreshTokenRequest(string RefreshToken);
public record AuthResponse(Guid UserId, string Name, string Email, string Role, string Token, string RefreshToken, DateTime TokenExpiry, string? EmployeeId = null, string? ProfileImageUrl = null);

public record UserSummaryDto(Guid UserId, string Name, string Email, string Role, Guid? OrganisationId, string? EmployeeId, string? Phone);
public record AuditLogDto(Guid AuditId, string UserName, string Action, string EntityType, string? RecordId, DateTime Timestamp);
