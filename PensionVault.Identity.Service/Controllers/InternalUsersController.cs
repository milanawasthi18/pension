using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Identity.Service.Application.Interfaces;
using PensionVault.Identity.Service.Domain.Interfaces;
using System.Linq;

namespace PensionVault.Identity.Service.Controllers;

[ApiController]
[Route("internal/users")]
public class InternalUsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditLogRepository _auditLogRepo;

    public InternalUsersController(IUserService userService, IAuditLogRepository auditLogRepo)
    {
        _userService = userService;
        _auditLogRepo = auditLogRepo;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("by-org/{orgId:guid}/role/{role}")]
    public async Task<IActionResult> GetByOrgAndRole(Guid orgId, string role)
    {
        var users = await _userService.GetByOrgAndRoleAsync(orgId, role);
        return Ok(users);
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var logs = await _auditLogRepo.GetFilteredAsync(entityType, from, to);
        var dtos = logs.Select(l => new {
            l.AuditId,
            UserName = l.User.Name,
            l.Action,
            l.EntityType,
            l.RecordId,
            l.Timestamp
        }).ToList();
        return Ok(dtos);
    }
}
