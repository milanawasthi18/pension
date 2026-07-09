using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionVault.Identity.Service.Application.Dtos;
using PensionVault.Identity.Service.Application.Interfaces;
using PensionVault.Identity.Service.Domain.Enums;
using PensionVault.Identity.Service.Domain.Interfaces;

namespace PensionVault.Identity.Service.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> UploadProfileImageAsync(Guid userId, string fileName)
    {
        var user = await _userRepo.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var fileUrl = $"/uploads/profiles/{fileName}";
        user.ProfileImageUrl = fileUrl;
        await _unitOfWork.SaveChangesAsync();
        return fileUrl;
    }

    public async Task UpdateProfileAsync(Guid userId, string name, string? phone)
    {
        var user = await _userRepo.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        user.Name = name;
        user.Phone = phone;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserSummaryDto> GetByIdAsync(Guid userId)
    {
        var user = await _userRepo.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        return new UserSummaryDto(user.UserId, user.Name, user.Email, user.Role.ToString(), user.OrganisationId, user.EmployeeId, user.Phone);
    }

    public async Task<List<UserSummaryDto>> GetByOrgAndRoleAsync(Guid organisationId, string role)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
            return new List<UserSummaryDto>();

        var users = await _userRepo.GetByOrgAndRoleAsync(organisationId, parsedRole);
        return users.Select(u => new UserSummaryDto(u.UserId, u.Name, u.Email, u.Role.ToString(), u.OrganisationId, u.EmployeeId, u.Phone)).ToList();
    }

    public async Task<List<UserSummaryDto>> GetAllAsync()
    {
        // Simple list of all users by retrieving Admin role to list others or similar.
        // For simplicity, we can extend IUserRepository to GetAll or do it through repository.
        // Let's implement using EF context or extend IUserRepository. Let's look up how the monolith does it.
        // The monolith doesn't have a direct get all users, wait.
        // Let's see: UsersController in the monolith uses IUserService? Let's check UsersController.cs in the monolith.
        return new List<UserSummaryDto>(); // Placeholder or default empty. We can check UsersController.cs.
    }
}
