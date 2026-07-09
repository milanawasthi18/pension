using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionVault.EmployerMember.Service.Application.Dtos;
using PensionVault.EmployerMember.Service.Application.Interfaces;
using PensionVault.EmployerMember.Service.Domain.Models;
using PensionVault.EmployerMember.Service.Domain.Enums;
using PensionVault.EmployerMember.Service.Domain.Interfaces;

namespace PensionVault.EmployerMember.Service.Application.Services;

public class EmployerService : IEmployerService
{
    private readonly IEmployerRepository _employerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public EmployerService(IEmployerRepository employerRepo, IUnitOfWork unitOfWork)
    {
        _employerRepo = employerRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EmployerResponse>> GetAllAsync()
    {
        var employers = await _employerRepo.GetAllAsync();
        return employers.Select(ToResponse);
    }

    public async Task<EmployerResponse> GetByIdAsync(Guid id)
    {
        var e = await _employerRepo.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("Employer not found.");
        return ToResponse(e);
    }

    public async Task<EmployerResponse> GetByUserIdAsync(Guid userId)
    {
        var e = await _employerRepo.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("No employer profile found for this user.");
        return ToResponse(e);
    }

    public async Task<EmployerResponse> CreateAsync(CreateEmployerRequest request)
    {
        if (await _employerRepo.ExistsByRegistrationNumberAsync(request.RegistrationNumber))
            throw new InvalidOperationException("Registration number already exists.");

        var employer = new Employer
        {
            CompanyName = request.CompanyName,
            RegistrationNumber = request.RegistrationNumber,
            Industry = request.Industry,
            RemittanceFrequency = request.RemittanceFrequency,
            ContactDetails = request.ContactDetails,
            Status = EmployerStatus.Active
        };
        await _employerRepo.AddAsync(employer);
        await _unitOfWork.SaveChangesAsync();
        return ToResponse(employer);
    }

    public async Task<EmployerResponse> UpdateAsync(Guid id, UpdateEmployerRequest request)
    {
        var employer = await _employerRepo.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("Employer not found.");
        employer.CompanyName = request.CompanyName;
        employer.Industry = request.Industry;
        employer.RemittanceFrequency = request.RemittanceFrequency;
        employer.ContactDetails = request.ContactDetails;
        employer.Status = request.Status;
        await _unitOfWork.SaveChangesAsync();
        return ToResponse(employer);
    }

    private static EmployerResponse ToResponse(Employer e) => new(
        e.EmployerId, e.CompanyName, e.RegistrationNumber, e.Industry,
        e.EnrolledMemberCount, e.RemittanceFrequency, e.ContactDetails, e.Status);
}
