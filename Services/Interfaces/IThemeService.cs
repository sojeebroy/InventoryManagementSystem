namespace Inventory_Management_System.Services.Interfaces;

/// <summary>
/// Service for managing user theme preferences (Light/Dark mode).
/// Handles theme persistence across sessions and devices.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current theme for a user.
    /// Returns "light" or "dark".
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Current theme preference</returns>
    Task<string> GetThemeAsync(string userId);

    /// <summary>
    /// Sets the user's theme preference.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="theme">Theme value: "light" or "dark"</param>
    /// <returns>Success status</returns>
    Task<bool> SetThemeAsync(string userId, string theme);

    /// <summary>
    /// Toggles the user's theme between light and dark.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The new theme value</returns>
    Task<string> ToggleThemeAsync(string userId);

    /// <summary>
    /// Gets whether user prefers system theme detection.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>True if user wants system theme, false otherwise</returns>
    Task<bool> GetUseSystemThemeAsync(string userId);

    /// <summary>
    /// Sets whether user prefers system theme detection.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="useSystemTheme">True to use system theme, false to use manual selection</param>
    /// <returns>Success status</returns>
    Task<bool> SetUseSystemThemeAsync(string userId, bool useSystemTheme);
}
