using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.Services;

/// <summary>
/// Service for managing user theme preferences.
/// Persists theme settings to database and ensures consistency across sessions.
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(ApplicationDbContext context, ILogger<ThemeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current theme for a user.
    /// Creates a default UserPreference if it doesn't exist.
    /// </summary>
    public async Task<string> GetThemeAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "light";

        try
        {
            var preference = await _context.UserPreferences.FindAsync(userId);

            if (preference == null)
            {
                // Create default preference if none exists
                preference = new UserPreference
                {
                    UserId = userId,
                    Theme = "light",
                    Language = "en",
                    UseSystemTheme = false,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserPreferences.Add(preference);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created default UserPreference for user {userId}");
            }

            return preference.Theme ?? "light";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting theme for user {userId}");
            return "light"; // Default to light theme on error
        }
    }

    /// <summary>
    /// Sets the user's theme preference to "light" or "dark".
    /// </summary>
    public async Task<bool> SetThemeAsync(string userId, string theme)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        // Validate theme value
        if (theme != "light" && theme != "dark")
        {
            _logger.LogWarning($"Invalid theme value: {theme}");
            return false;
        }

        try
        {
            var preference = await _context.UserPreferences.FindAsync(userId);

            if (preference == null)
            {
                preference = new UserPreference
                {
                    UserId = userId,
                    Theme = theme,
                    Language = "en",
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserPreferences.Add(preference);
            }
            else
            {
                preference.Theme = theme;
                preference.UpdatedAt = DateTime.UtcNow;
                _context.UserPreferences.Update(preference);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Theme updated to {theme} for user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting theme for user {userId}");
            return false;
        }
    }

    /// <summary>
    /// Toggles the user's theme between light and dark.
    /// </summary>
    public async Task<string> ToggleThemeAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "light";

        try
        {
            var currentTheme = await GetThemeAsync(userId);
            var newTheme = currentTheme == "light" ? "dark" : "light";

            await SetThemeAsync(userId, newTheme);
            return newTheme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error toggling theme for user {userId}");
            return "light";
        }
    }

    /// <summary>
    /// Gets whether user prefers system theme detection.
    /// </summary>
    public async Task<bool> GetUseSystemThemeAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        try
        {
            var preference = await _context.UserPreferences.FindAsync(userId);
            return preference?.UseSystemTheme ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting UseSystemTheme for user {userId}");
            return false;
        }
    }

    /// <summary>
    /// Sets whether user prefers system theme detection.
    /// </summary>
    public async Task<bool> SetUseSystemThemeAsync(string userId, bool useSystemTheme)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        try
        {
            var preference = await _context.UserPreferences.FindAsync(userId);

            if (preference == null)
            {
                preference = new UserPreference
                {
                    UserId = userId,
                    UseSystemTheme = useSystemTheme,
                    Theme = "light",
                    Language = "en",
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserPreferences.Add(preference);
            }
            else
            {
                preference.UseSystemTheme = useSystemTheme;
                preference.UpdatedAt = DateTime.UtcNow;
                _context.UserPreferences.Update(preference);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"UseSystemTheme set to {useSystemTheme} for user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting UseSystemTheme for user {userId}");
            return false;
        }
    }
}
