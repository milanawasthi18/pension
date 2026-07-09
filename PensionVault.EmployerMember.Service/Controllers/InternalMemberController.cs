using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PensionVault.EmployerMember.Service.Application.Dtos;
using PensionVault.EmployerMember.Service.Application.Interfaces;
using PensionVault.EmployerMember.Service.HttpClients;
using System.Collections.Generic;

namespace PensionVault.EmployerMember.Service.Controllers;

[ApiController]
[Route("internal")]
public class InternalMemberController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IFundAccountService _accountService;
    private readonly IEmployerService _employerService;
    private readonly ISchemeService _schemeService;
    private readonly IdentityClient _identityClient;

    public InternalMemberController(
        IMemberService memberService,
        IFundAccountService accountService,
        IEmployerService employerService,
        ISchemeService schemeService,
        IdentityClient identityClient)
    {
        _memberService = memberService;
        _accountService = accountService;
        _employerService = employerService;
        _schemeService = schemeService;
        _identityClient = identityClient;
    }

    [HttpGet("members/{id:guid}")]
    public async Task<IActionResult> GetMemberById(Guid id)
    {
        try
        {
            var result = await _memberService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("members/by-user/{userId:guid}")]
    public async Task<IActionResult> GetMemberByUserId(Guid userId)
    {
        try
        {
            var result = await _memberService.GetByUserIdAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("members/{id:guid}/fund-account")]
    public async Task<IActionResult> GetActiveFundAccount(Guid id)
    {
        try
        {
            var result = await _accountService.GetActiveByMemberAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("members/fund-accounts/{accountId:guid}/balance")]
    public async Task<IActionResult> UpdateBalance(Guid accountId, [FromBody] UpdateBalanceRequest request)
    {
        try
        {
            var result = await _accountService.UpdateBalanceAsync(accountId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("employers/{id:guid}")]
    public async Task<IActionResult> GetEmployerById(Guid id)
    {
        try
        {
            var result = await _employerService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("employers")]
    public async Task<IActionResult> CreateEmployerFromAuth([FromBody] CreateEmployerFromAuthRequest request)
    {
        var createRequest = new CreateEmployerRequest(
            request.CompanyName,
            "REG-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            null,
            Domain.Enums.RemittanceFrequency.Monthly,
            request.ContactDetails
        );
        var result = await _employerService.CreateAsync(createRequest);
        return Ok(new { EmployerId = result.EmployerId });
    }

    [HttpGet("employers/{id:guid}/users")]
    public async Task<IActionResult> GetEmployerUsers(Guid id)
    {
        // Query Identity.Service internal endpoint to fetch user records by orgId and role
        // For simplicity, we make a call using IdentityClient
        var users = await _identityClient.GetUsersByRoleAsync("Employer");
        // Filter users that belong to this organization (in microservices, Identity has org lookup)
        // Let's call /internal/users/by-org/{orgId}/role/Employer
        try
        {
            var client = new HttpClient(); // Quick fallback or use client instance
            var response = await client.GetFromJsonAsync<List<IdentityClient.UserInfo>>($"http://localhost:5001/internal/users/by-org/{id}/role/Employer");
            return Ok(response ?? new());
        }
        catch
        {
            return Ok(new List<object>());
        }
    }

    [HttpGet("schemes/{id:guid}")]
    public async Task<IActionResult> GetSchemeById(Guid id)
    {
        try
        {
            var result = await _schemeService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

public record CreateEmployerFromAuthRequest(string CompanyName, string ContactDetails);
