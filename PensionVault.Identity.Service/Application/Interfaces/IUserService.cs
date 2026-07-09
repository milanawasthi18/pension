using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.Identity.Service.Application.Dtos;

namespace PensionVault.Identity.Service.Application.Interfaces;

public interface IUserService
{
    Task<string> UploadProfileImageAsync(Guid userId, string fileName);
    Task UpdateProfileAsync(Guid userId, string name, string? phone);
    Task<UserSummaryDto> GetByIdAsync(Guid userId);
    Task<List<UserSummaryDto>> GetByOrgAndRoleAsync(Guid organisationId, string role);
    Task<List<UserSummaryDto>> GetAllAsync();
}
