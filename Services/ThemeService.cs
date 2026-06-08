using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.Services;

public class ThemeService : IThemeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(ApplicationDbContext context, ILogger<ThemeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GetThemeAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "light";

        try
        {
            var preference = await _context.UserPreferences.FindAsync(userId);

            if (preference == null)
            {
                preference = new UserPreference
                {
                    UserId = userId,
                    Theme = "light",
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
            return "light";
        }
    }

    public async Task<bool> SetThemeAsync(string userId, string theme)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

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

