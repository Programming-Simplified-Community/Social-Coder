using Microsoft.AspNetCore.DataProtection;
using SocialCoder.Web.Server.Models;
using Path = System.IO.Path;

namespace SocialCoder.Web.Server.Services;

public class SecureSettingsService
{
    private readonly ILogger<SecureSettingsService> _logger;
    private readonly IDataProtector _protector;
    private readonly string _filePath;

    /// <summary>
    /// This is used to isolate protectors. Ensures that data is protected for one purpose and cannot be used for another.
    /// </summary>
    private const string ProtectorPurpose = "ApplicationSettings.v1";

    public SecureSettingsService(IDataProtectionProvider provider, IWebHostEnvironment env, ILogger<SecureSettingsService> logger)
    {
        this._protector = provider.CreateProtector(ProtectorPurpose);
        this._logger = logger;
        // Store the file in the ContentRootPath, which is the base directory of the app
        this._filePath = Path.Combine(env.ContentRootPath, "protected_settings.json");
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
        var protectedJson = this._protector.Protect(json);
        await File.WriteAllTextAsync(this._filePath, protectedJson);
    }

    public AppSettings? LoadSettings()
    {
        if (!File.Exists(this._filePath))
        {
            return null;
        }

        var protectedJson = File.ReadAllText(this._filePath);

        try
        {
            var json = this._protector.Unprotect(protectedJson);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(json);
        }
        catch (Exception ex)
        {
            this._logger.LogError("Error decrypting settings: {Exception}", ex);
            return null;
        }
    }
}