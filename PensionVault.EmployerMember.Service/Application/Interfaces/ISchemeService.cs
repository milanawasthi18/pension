using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Application.Dtos;

namespace PensionVault.EmployerMember.Service.Application.Interfaces;

public interface ISchemeService
{
    Task<IEnumerable<SchemeResponse>> GetAllAsync();
    Task<SchemeResponse> GetByIdAsync(Guid id);
    Task<SchemeResponse> CreateAsync(CreateSchemeRequest request);
    Task<SchemeResponse> UpdateAsync(Guid id, UpdateSchemeRequest request);
}
