using Microsoft.AspNetCore.DataProtection;
using Npgsql;
using SocialCoder.Web.Server.Models;
using Path = System.IO.Path;

namespace SocialCoder.Web.Server.Services;

public class SecureSettingsService
{
    private readonly ILogger<SecureSettingsService> _logger;
    private readonly IDataProtector _protector;
    private readonly string _filePath;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// This is used to isolate protectors. Ensures that data is protected for one purpose and cannot be used for another.
    /// </summary>
    private const string ProtectorPurpose = "ApplicationSettings.v1";

    public SecureSettingsService(IDataProtectionProvider provider, IConfiguration configuration, IWebHostEnvironment env, ILogger<SecureSettingsService> logger)
    {
        this._protector = provider.CreateProtector(ProtectorPurpose);
        this._logger = logger;
        this._configuration = configuration;

        // Store the file in the ContentRootPath, which is the base directory of the app
        this._filePath = Path.Combine(env.ContentRootPath, "protected_settings.json");
    }

    /// <summary>
    /// Persist the settings in ciphertext to disk.
    /// </summary>
    /// <param name="settings"></param>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
        var protectedJson = this._protector.Protect(json);
        await File.WriteAllTextAsync(this._filePath, protectedJson);
    }

    /// <summary>
    /// Attempt to load settings from disk
    /// </summary>
    /// <remarks>
    /// If the configuration from Aspire is provided, that will be chosen over the file on disk
    /// </remarks>
    /// <param name="settings">Instance to hydrate connection string for</param>
    /// <returns>Instace of <paramref name="settings"/> with connection string (if applicable)</returns>
    private AppSettings TryLoadFromConfiguration(AppSettings settings)
    {
        var existingConnectionString = this._configuration.GetConnectionString("socialcoder");

        if (existingConnectionString is null)
        {
            return settings;
        }

        var builder = new NpgsqlConnectionStringBuilder(existingConnectionString);

        settings.Postgres = new()
        {
            Database = builder.Database ?? string.Empty,
            Host = builder.Host ?? string.Empty,
            Password = builder.Password ?? string.Empty,
            Port = builder.Port,
            UserId = builder.Username ?? string.Empty
        };

        return settings;
    }

    /// <summary>
    /// Attempts to load settings from the disk (decryption included). If settings are not present, provides a default instance.
    /// </summary>
    /// <returns></returns>
    public AppSettings? LoadSettings()
    {
        if (!File.Exists(this._filePath))
        {
            return this.TryLoadFromConfiguration(new());
        }

        var protectedJson = File.ReadAllText(this._filePath);

        try
        {
            var json = this._protector.Unprotect(protectedJson);
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(json);
            return this.TryLoadFromConfiguration(settings ?? new());
        }
        catch (Exception ex)
        {
            this._logger.LogError("Error decrypting settings: {Exception}", ex);
            return null;
        }
    }
}