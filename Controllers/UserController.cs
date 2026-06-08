using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services.Interfaces;

namespace Inventory_Management_System.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IThemeService _themeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IThemeService themeService,
        UserManager<ApplicationUser> userManager,
        ILogger<UserController> logger)
    {
        _themeService = themeService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("theme")]
    public async Task<IActionResult> GetTheme()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var theme = await _themeService.GetThemeAsync(userId);
        return Ok(new { theme });
    }

    [HttpPost("theme")]
    public async Task<IActionResult> SetTheme([FromBody] SetThemeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Theme))
            return BadRequest(new { error = "Theme is required" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _themeService.SetThemeAsync(userId, request.Theme);
        if (!success)
            return BadRequest(new { error = "Invalid theme value" });

        _logger.LogInformation($"User {userId} changed theme to {request.Theme}");
        return Ok(new { success = true, theme = request.Theme });
    }

    [HttpPost("theme/toggle")]
    public async Task<IActionResult> ToggleTheme()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var newTheme = await _themeService.ToggleThemeAsync(userId);
        _logger.LogInformation($"User {userId} toggled theme to {newTheme}");
        return Ok(new { success = true, theme = newTheme });
    }
}

public class SetThemeRequest
{
    public string? Theme { get; set; }
}


