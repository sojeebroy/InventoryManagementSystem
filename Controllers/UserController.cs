using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
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
    private readonly ILocalizationService _localizationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IThemeService themeService,
        ILocalizationService localizationService,
        UserManager<ApplicationUser> userManager,
        ILogger<UserController> logger)
    {
        _themeService = themeService;
        _localizationService = localizationService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's theme preference
    /// </summary>
    [HttpGet("theme")]
    public async Task<IActionResult> GetTheme()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var theme = await _themeService.GetThemeAsync(userId);
        return Ok(new { theme });
    }

    /// <summary>
    /// Set user's theme preference
    /// </summary>
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

    /// <summary>
    /// Toggle between light and dark theme
    /// </summary>
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

    /// <summary>
    /// Get current user's language preference
    /// </summary>
    [HttpGet("language")]
    public async Task<IActionResult> GetLanguage()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var language = await _localizationService.GetLanguageAsync(userId);
        return Ok(new { language });
    }

    /// <summary>
    /// Set user's language preference
    /// </summary>
    [HttpPost("language")]
    public async Task<IActionResult> SetLanguage([FromBody] SetLanguageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Language))
            return BadRequest(new { error = "Language is required" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _localizationService.SetLanguageAsync(userId, request.Language);
        if (!success)
            return BadRequest(new { error = "Invalid language code" });

        // Set culture cookie for RequestLocalization middleware
        // Format: "c=<culture>|uic=<ui-culture>"
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(request.Language)),
            new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        _logger.LogInformation($"User {userId} changed language to {request.Language}");
        return Ok(new { success = true, language = request.Language });
    }

    /// <summary>
    /// Get all supported languages
    /// </summary>
    [HttpGet("languages")]
    [AllowAnonymous]
    public IActionResult GetSupportedLanguages()
    {
        var languages = _localizationService.GetSupportedLanguages();
        return Ok(new { languages });
    }

    /// <summary>
    /// Get localized strings for a specific language
    /// </summary>
    [HttpGet("localization/{language}")]
    [AllowAnonymous]
    public IActionResult GetLocalizationStrings(string language)
    {
        var strings = _localizationService.GetAllStrings(language);
        if (strings.Count == 0)
            return NotFound(new { error = $"Language '{language}' not found" });

        return Ok(new { language, strings });
    }

    /// <summary>
    /// Get user's complete preferences (theme, language, etc.)
    /// </summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var theme = await _themeService.GetThemeAsync(userId);
        var language = await _localizationService.GetLanguageAsync(userId);
        var useSystemTheme = await _themeService.GetUseSystemThemeAsync(userId);

        return Ok(new
        {
            theme,
            language,
            useSystemTheme,
            supportedLanguages = _localizationService.GetSupportedLanguages()
        });
    }
}

/// <summary>
/// Request model for setting theme
/// </summary>
public class SetThemeRequest
{
    public string? Theme { get; set; }
}

/// <summary>
/// Request model for setting language
/// </summary>
public class SetLanguageRequest
{
    public string? Language { get; set; }
}
