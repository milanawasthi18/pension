using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PensionVault.Identity.Service.Application.Interfaces;

namespace PensionVault.Identity.Service.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _env;

    public UsersController(IUserService userService, IWebHostEnvironment env)
    {
        _userService = userService;
        _env = env;
    }

    [HttpPost("me/image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var uploadsFolder = Path.Combine(
            _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            "uploads", "profiles");
        Directory.CreateDirectory(uploadsFolder);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{userId}_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var fileUrl = await _userService.UploadProfileImageAsync(userId, fileName);
        return Ok(new { ProfileImageUrl = fileUrl });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        await _userService.UpdateProfileAsync(userId, request.Name, request.Phone);
        return Ok(new { request.Name, request.Phone });
    }
}

public record UpdateUserRequest(string Name, string? Phone);
