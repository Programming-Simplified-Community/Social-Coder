using Microsoft.AspNetCore.DataProtection;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.Models.Setup;
using Path = System.IO.Path;

namespace SocialCoder.Web.Server.Services;

/// <summary>
/// Acts as a "source" for our encrypted settings file.
/// </summary>
public class EncryptedSettingsConfigurationSource : IConfigurationSource
{
    private readonly IServiceCollection _services;

    public EncryptedSettingsConfigurationSource(IServiceCollection services)
    {
        this._services = services;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var sp = this._services.BuildServiceProvider();
        var dp = sp.GetRequiredService<IDataProtectionProvider>();
        var env = sp.GetRequiredService<IWebHostEnvironment>();

        return new EncryptedSettingsConfigurationProvider(dp, env);
    }
}

/// <summary>
/// Acts as our "provider" which loads the settings from our encrypted file and adds them into the configuration.
/// </summary>
public class EncryptedSettingsConfigurationProvider : ConfigurationProvider
{
    private readonly IDataProtector _protector;
    private readonly IWebHostEnvironment _env;
    private const string ProtectorPurpose = "ApplicationSettings.v1";

    public EncryptedSettingsConfigurationProvider(IDataProtectionProvider provider, IWebHostEnvironment env)
    {
        this._protector = provider.CreateProtector(ProtectorPurpose);
        this._env = env;
    }

    public override void Load()
    {
        var filePath = Path.Combine(this._env.ContentRootPath, "protected_settings.json");
        if (!File.Exists(filePath))
        {
            this.Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        var protectedJson = File.ReadAllText(filePath);

        try
        {
            var json = this._protector.Unprotect(protectedJson);
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(json);
            this.Data = this.Flatten(settings);
        }
        catch (Exception)
        {
            this.Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Converts our settings into a flat dictionary where values are ":" delimited.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    private IDictionary<string, string?> Flatten(AppSettings? settings)
    {
        var dict = new Dictionary<string, string?>();
        settings ??= new AppSettings();

        var props = settings.GetType().GetProperties();

        foreach (var prop in props)
        {
            var value = prop.GetValue(settings, null);
            if (this.IsPrimitive(prop.PropertyType))
            {
                var strValue = string.Empty;

                if (value is null)
                {
                    strValue = this.GetDefault(prop.PropertyType)?.ToString() ?? this.GetDefault(prop.PropertyType)?.ToString();
                }
                else
                {
                    strValue = value.ToString();
                }

                dict.Add(prop.Name, strValue);
            }
            else
            {
                if (value is null)
                {
                    continue;
                }

                if (prop.Name.Equals("OAuthSettings", StringComparison.OrdinalIgnoreCase))
                {
                    var dictValue = (IDictionary<string, OAuthSetting>)value;

                    foreach (var (key, val) in dictValue)
                    {
                        dict.Add($"{prop.Name}:{key}:{nameof(OAuthSetting.ClientId)}", val.ClientId);
                        dict.Add($"{prop.Name}:{key}:{nameof(OAuthSetting.ClientSecret)}", val.ClientSecret);
                    }
                }
                else if (prop.PropertyType.IsAssignableFrom(typeof(PostgresConnection)))
                {
                    foreach (var connectionProp in typeof(PostgresConnection).GetProperties())
                    {
                        dict.Add($"{prop.Name}:{connectionProp.Name}", connectionProp.GetValue(value, null)?.ToString());
                    }
                }
            }
        }

        return dict;
    }

    private object? GetDefault(Type type)
    {
        if (type == typeof(bool))
        {
            return false;
        }

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
    private bool IsPrimitive(Type type)
    {
        return type.IsPrimitive || type == typeof(string);
    }
}