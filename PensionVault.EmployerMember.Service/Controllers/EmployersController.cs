using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.EmployerMember.Service.Application.Dtos;
using PensionVault.EmployerMember.Service.Application.Interfaces;
using PensionVault.EmployerMember.Service.HttpClients;

namespace PensionVault.EmployerMember.Service.Controllers;

[ApiController]
[Route("api/employers")]
[Authorize]
[Produces("application/json")]
public class EmployersController : ControllerBase
{
    private readonly IEmployerService _employerService;
    private readonly FundOpsClient _fundOpsClient;

    public EmployersController(IEmployerService employerService, FundOpsClient fundOpsClient)
    {
        _employerService = employerService;
        _fundOpsClient = fundOpsClient;
    }

    [HttpGet]
    [Authorize(Roles = "Member,FundAdmin,Admin,Compliance,Employer")]
    public async Task<IActionResult> GetAll() => Ok(await _employerService.GetAllAsync());

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employer,FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _employerService.GetByIdAsync(id));

    [HttpGet("me")]
    [Authorize(Roles = "Employer,FundAdmin,Admin")]
    public async Task<IActionResult> GetMyEmployer()
    {
        var orgClaim = User.FindFirst("OrganisationId");
        if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId))
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();
            try
            {
                return Ok(await _employerService.GetByUserIdAsync(userId));
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Employer profile not found.");
            }
        }
        return Ok(await _employerService.GetByIdAsync(orgId));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployerRequest request)
    {
        var result = await _employerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.EmployerId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Employer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployerRequest request)
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId) || orgId != id)
                return Forbid();

            var current = await _employerService.GetByIdAsync(id);
            request = request with { Status = current.Status };
        }
        return Ok(await _employerService.UpdateAsync(id, request));
    }

    [HttpGet("{id:guid}/remittances")]
    [Authorize(Roles = "Employer,FundAdmin,Admin")]
    public async Task<IActionResult> GetRemittances(Guid id)
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId) || orgId != id)
                return Forbid();
        }
        return Ok(await _fundOpsClient.GetRemittancesByEmployerAsync(id));
    }
}
