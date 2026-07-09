using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensionVault.EmployerMember.Service.Application.Dtos;
using PensionVault.EmployerMember.Service.Application.Interfaces;

namespace PensionVault.EmployerMember.Service.Controllers;

[ApiController]
[Route("api/schemes")]
[Authorize]
[Produces("application/json")]
public class SchemesController : ControllerBase
{
    private readonly ISchemeService _schemeService;
    public SchemesController(ISchemeService schemeService) => _schemeService = schemeService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await _schemeService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _schemeService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSchemeRequest request)
    {
        var result = await _schemeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.SchemeId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchemeRequest request)
    {
        return Ok(await _schemeService.UpdateAsync(id, request));
    }
}
