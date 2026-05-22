using Inventory_Management_System.Services.Interfaces;

namespace Inventory_Management_System.Helpers;

/// <summary>
/// Helper extension methods for using localization in Razor views
/// </summary>
public static class LocalizationHelper
{
    /// <summary>
    /// Get a localized string for the current request culture
    /// Usage in Razor: @LocalizationHelper.GetString("Nav.Dashboard")
    /// </summary>
    public static string GetString(string key)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        return GetString(key, culture);
    }

    /// <summary>
    /// Get a localized string for a specific language
    /// </summary>
    public static string GetString(string key, string language)
    {
        // Access localization service from HttpContext if available
        // Otherwise return the key as fallback
        try
        {
            // This would be called from views where we don't have direct DI
            // For now, return key as a safe fallback
            // In production, you'd resolve ILocalizationService from HttpContext.RequestServices
            return key;
        }
        catch
        {
            return key;
        }
    }
}
