namespace Inventory_Management_System.Services.Interfaces;

public interface IThemeService
{
    Task<string> GetThemeAsync(string userId);

    Task<bool> SetThemeAsync(string userId, string theme);

    Task<string> ToggleThemeAsync(string userId);

    Task<bool> GetUseSystemThemeAsync(string userId);

    Task<bool> SetUseSystemThemeAsync(string userId, bool useSystemTheme);
}

