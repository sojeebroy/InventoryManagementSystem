namespace Inventory_Management_System.Services.Interfaces;

/// <summary>
/// Service for managing user language preferences and localization.
/// Supports English (en) and Spanish (es) with extensibility for more languages.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current language for a user.
    /// Returns "en" (English) or "es" (Spanish).
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Current language preference</returns>
    Task<string> GetLanguageAsync(string userId);

    /// <summary>
    /// Sets the user's language preference.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="language">Language code: "en" or "es"</param>
    /// <returns>Success status</returns>
    Task<bool> SetLanguageAsync(string userId, string language);

    /// <summary>
    /// Gets all supported languages.
    /// </summary>
    /// <returns>Dictionary of language codes and display names</returns>
    Dictionary<string, string> GetSupportedLanguages();

    /// <summary>
    /// Gets a localized string by key for the specified language.
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="language">Language code (e.g., "en", "es")</param>
    /// <returns>Localized string or the key if not found</returns>
    string GetString(string key, string language);

    /// <summary>
    /// Gets a localized string by key for the user's preferred language.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="key">The resource key</param>
    /// <returns>Localized string or the key if not found</returns>
    Task<string> GetStringAsync(string userId, string key);

    /// <summary>
    /// Gets all localization keys and values for a language.
    /// </summary>
    /// <param name="language">Language code</param>
    /// <returns>Dictionary of all localized strings</returns>
    Dictionary<string, string> GetAllStrings(string language);
}
