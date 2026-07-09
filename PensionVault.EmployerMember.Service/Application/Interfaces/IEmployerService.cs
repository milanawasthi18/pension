using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Application.Dtos;

namespace PensionVault.EmployerMember.Service.Application.Interfaces;

public interface IEmployerService
{
    Task<IEnumerable<EmployerResponse>> GetAllAsync();
    Task<EmployerResponse> GetByIdAsync(Guid id);
    Task<EmployerResponse> GetByUserIdAsync(Guid userId);
    Task<EmployerResponse> CreateAsync(CreateEmployerRequest request);
    Task<EmployerResponse> UpdateAsync(Guid id, UpdateEmployerRequest request);
}
