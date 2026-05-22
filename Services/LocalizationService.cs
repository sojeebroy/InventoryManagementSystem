using Inventory_Management_System.Data;
using Inventory_Management_System.Models;
using Inventory_Management_System.Services.Interfaces;
using System.Collections.Concurrent;

namespace Inventory_Management_System.Services;

/// <summary>
/// Service for managing user language preferences and string localization.
/// Supports English and Spanish with in-memory caching.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LocalizationService> _logger;

    // Cache for localized strings: language -> (key -> value)
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> LocalizationCache
        = new();

    // Supported languages
    private static readonly Dictionary<string, string> SupportedLanguages = new()
    {
        { "en", "English" },
        { "es", "Español" }
    };

    public LocalizationService(ApplicationDbContext context, ILogger<LocalizationService> logger)
    {
        _context = context;
        _logger = logger;
        InitializeCache();
    }

    /// <summary>
    /// Initializes the localization cache with default values.
    /// </summary>
    private void InitializeCache()
    {
        // English strings
        LocalizationCache.TryAdd("en", new Dictionary<string, string>
        {
            // Navigation
            { "Nav.Home", "Home" },
            { "Nav.Dashboard", "Dashboard" },
            { "Nav.Inventories", "Inventories" },
            { "Nav.Settings", "Settings" },
            { "Nav.Profile", "Profile" },
            { "Nav.SignIn", "Sign In" },
            { "Nav.SignOut", "Sign Out" },
            { "Nav.Account", "Account" },

            // Common Actions
            { "Action.Add", "Add" },
            { "Action.Edit", "Edit" },
            { "Action.Delete", "Delete" },
            { "Action.Save", "Save" },
            { "Action.Cancel", "Cancel" },
            { "Action.Submit", "Submit" },
            { "Action.Back", "Back" },
            { "Action.Next", "Next" },
            { "Action.Previous", "Previous" },

            // Inventory
            { "Inventory.Title", "Inventories" },
            { "Inventory.AddNew", "Add New Inventory" },
            { "Inventory.Name", "Name" },
            { "Inventory.Description", "Description" },
            { "Inventory.Items", "Items" },
            { "Inventory.NoItems", "No items in this inventory yet" },
            { "Inventory.CustomId", "Custom ID Format" },

            // Items
            { "Item.Title", "Items" },
            { "Item.AddNew", "Add Item" },
            { "Item.Edit", "Edit Item" },
            { "Item.Delete", "Delete Item" },
            { "Item.Created", "Created" },
            { "Item.CreatedBy", "Created By" },

            // Messages
            { "Message.Success", "Success!" },
            { "Message.Error", "Error" },
            { "Message.Warning", "Warning" },
            { "Message.Info", "Information" },
            { "Message.ConfirmDelete", "Are you sure?" },

            // Theme
            { "Theme.Light", "Light Theme" },
            { "Theme.Dark", "Dark Theme" },
            { "Theme.Auto", "Auto (System)" },

            // Language
            { "Language.English", "English" },
            { "Language.Spanish", "Español" },
        });

        // Spanish strings
        LocalizationCache.TryAdd("es", new Dictionary<string, string>
        {
            // Navigation
            { "Nav.Home", "Inicio" },
            { "Nav.Dashboard", "Panel de Control" },
            { "Nav.Inventories", "Inventarios" },
            { "Nav.Settings", "Configuración" },
            { "Nav.Profile", "Perfil" },
            { "Nav.SignIn", "Iniciar Sesión" },
            { "Nav.SignOut", "Cerrar Sesión" },
            { "Nav.Account", "Cuenta" },

            // Common Actions
            { "Action.Add", "Agregar" },
            { "Action.Edit", "Editar" },
            { "Action.Delete", "Eliminar" },
            { "Action.Save", "Guardar" },
            { "Action.Cancel", "Cancelar" },
            { "Action.Submit", "Enviar" },
            { "Action.Back", "Atrás" },
            { "Action.Next", "Siguiente" },
            { "Action.Previous", "Anterior" },

            // Inventory
            { "Inventory.Title", "Inventarios" },
            { "Inventory.AddNew", "Agregar Nuevo Inventario" },
            { "Inventory.Name", "Nombre" },
            { "Inventory.Description", "Descripción" },
            { "Inventory.Items", "Elementos" },
            { "Inventory.NoItems", "No hay elementos en este inventario" },
            { "Inventory.CustomId", "Formato de ID Personalizado" },

            // Items
            { "Item.Title", "Elementos" },
            { "Item.AddNew", "Agregar Elemento" },
            { "Item.Edit", "Editar Elemento" },
            { "Item.Delete", "Eliminar Elemento" },
            { "Item.Created", "Creado" },
            { "Item.CreatedBy", "Creado Por" },

            // Messages
            { "Message.Success", "¡Éxito!" },
            { "Message.Error", "Error" },
            { "Message.Warning", "Advertencia" },
            { "Message.Info", "Información" },
            { "Message.ConfirmDelete", "¿Estás seguro?" },

            // Theme
            { "Theme.Light", "Tema Claro" },
            { "Theme.Dark", "Tema Oscuro" },
            { "Theme.Auto", "Automático (Sistema)" },

            // Language
            { "Language.English", "English" },
            { "Language.Spanish", "Español" },
        });
    }

    /// <summary>
    /// Gets the current language for a user.
    /// Creates a default UserPreference if it doesn't exist.
    /// </summary>
    public async Task<string> GetLanguageAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "en";

        try
        {
            var preference = await _context.UserPreferences.FindAsync(userId);

            if (preference == null)
            {
                preference = new UserPreference
                {
                    UserId = userId,
                    Language = "en",
                    Theme = "light",
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserPreferences.Add(preference);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created default UserPreference with language 'en' for user {userId}");
            }

            return preference.Language ?? "en";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting language for user {userId}");
            return "en";
        }
    }

    /// <summary>
    /// Sets the user's language preference to "en" or "es".
    /// </summary>
    public async Task<bool> SetLanguageAsync(string userId, string language)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        // Validate language value
        if (!SupportedLanguages.ContainsKey(language))
        {
            _logger.LogWarning($"Invalid language code: {language}");
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
                    Language = language,
                    Theme = "light",
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserPreferences.Add(preference);
            }
            else
            {
                preference.Language = language;
                preference.UpdatedAt = DateTime.UtcNow;
                _context.UserPreferences.Update(preference);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Language updated to {language} for user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting language for user {userId}");
            return false;
        }
    }

    /// <summary>
    /// Gets all supported languages.
    /// </summary>
    public Dictionary<string, string> GetSupportedLanguages()
    {
        return new Dictionary<string, string>(SupportedLanguages);
    }

    /// <summary>
    /// Gets a localized string by key for the specified language.
    /// </summary>
    public string GetString(string key, string language)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(language))
            return key;

        if (!LocalizationCache.TryGetValue(language, out var strings))
        {
            _logger.LogWarning($"Language '{language}' not found in cache");
            return key;
        }

        return strings.TryGetValue(key, out var value) ? value : key;
    }

    /// <summary>
    /// Gets a localized string by key for the user's preferred language.
    /// </summary>
    public async Task<string> GetStringAsync(string userId, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return key;

        var language = await GetLanguageAsync(userId);
        return GetString(key, language);
    }

    /// <summary>
    /// Gets all localization keys and values for a language.
    /// </summary>
    public Dictionary<string, string> GetAllStrings(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return new Dictionary<string, string>();

        if (!LocalizationCache.TryGetValue(language, out var strings))
        {
            _logger.LogWarning($"Language '{language}' not found in cache");
            return new Dictionary<string, string>();
        }

        return new Dictionary<string, string>(strings);
    }
}
