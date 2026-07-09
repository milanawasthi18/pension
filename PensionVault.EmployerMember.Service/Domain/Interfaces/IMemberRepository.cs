using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Domain.Models;

namespace PensionVault.EmployerMember.Service.Domain.Interfaces;

public interface IMemberRepository
{
    Task<Member?> FindByIdAsync(Guid memberId);
    Task<Member?> FindByUserIdAsync(Guid userId);
    Task<List<Member>> GetAllAsync(Guid? employerId = null);
    Task<bool> ExistsByMembershipNumberAsync(string membershipNumber, Guid? excludeId = null);
    Task<bool> ExistsByUserIdAsync(Guid userId);
    Task AddAsync(Member member);
}
