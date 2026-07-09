using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.FundOps.Service.Application.Dtos;
using PensionVault.FundOps.Service.Application.Interfaces;

namespace PensionVault.FundOps.Service.Controllers;

[ApiController]
[Route("api/remittances")]
[Authorize]
[Produces("application/json")]
public class RemittancesController : ControllerBase
{
    private readonly IContributionService _contributionService;

    public RemittancesController(IContributionService contributionService)
    {
        _contributionService = contributionService;
    }

    [HttpGet]
    [Authorize(Roles = "Employer,FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? employerId)
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId)) 
                return Ok(new List<RemittanceResponse>());
            return Ok(await _contributionService.GetEmployerRemittancesAsync(orgId));
        }

        if (employerId.HasValue)
        {
            return Ok(await _contributionService.GetEmployerRemittancesAsync(employerId.Value));
        }

        return Ok(await _contributionService.GetAllRemittancesAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Employer,FundAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRemittanceRequest request)
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim != null && Guid.TryParse(orgClaim.Value, out var orgId))
            {
                request = request with { EmployerId = orgId };
            }
        }
        var result = await _contributionService.CreateRemittanceAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.RemittanceId }, result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employer,FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _contributionService.GetRemittanceAsync(id));

    [HttpPost("{id:guid}/reconcile")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> Reconcile(Guid id)
        => Ok(await _contributionService.ReconcileAsync(id));

    [HttpGet("member/{memberId:guid}")]
    [Authorize(Roles = "Member,FundAdmin,Admin")]
    public async Task<IActionResult> GetMemberContributions(Guid memberId)
        => Ok(await _contributionService.GetMemberContributionsAsync(memberId));

    [HttpGet("{id:guid}/reconciliation-report")]
    [Authorize(Roles = "FundAdmin,Admin")]
    public async Task<IActionResult> GetReconciliationReport(Guid id)
        => Ok(await _contributionService.GetReconciliationReportAsync(id));

    [HttpGet("defaulters")]
    [Authorize(Roles = "FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetDefaulters()
        => Ok(await _contributionService.GetDefaultersAsync());

    [HttpGet("overdue")]
    [Authorize(Roles = "FundAdmin,Admin,Compliance")]
    public async Task<IActionResult> GetOverdueRemittances()
        => Ok(await _contributionService.GetOverdueRemittancesAsync());

    [HttpGet("employer/{employerId:guid}/defaulter-summary")]
    [Authorize(Roles = "FundAdmin,Admin,Compliance,Employer")]
    public async Task<IActionResult> GetDefaulterSummary(Guid employerId)
    {
        if (User.IsInRole("Employer"))
        {
            var orgClaim = User.FindFirst("OrganisationId");
            if (orgClaim == null || !Guid.TryParse(orgClaim.Value, out var orgId) || orgId != employerId) 
                return Forbid();
        }
        return Ok(await _contributionService.GetDefaulterSummaryAsync(employerId));
    }
}
