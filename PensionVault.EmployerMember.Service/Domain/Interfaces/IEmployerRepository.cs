using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Domain.Models;

namespace PensionVault.EmployerMember.Service.Domain.Interfaces;

public interface IEmployerRepository
{
    Task<Employer?> FindByIdAsync(Guid employerId);
    Task<List<Employer>> GetAllAsync();
    Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber);
    Task AddAsync(Employer employer);
}
