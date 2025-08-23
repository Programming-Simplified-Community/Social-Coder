using Microsoft.AspNetCore.Mvc;
using Nextended.Core.Extensions;
using Npgsql;
using SocialCoder.Web.Server.Attributes;
using SocialCoder.Web.Server.Services;
using SocialCoder.Web.Shared.Models.Setup;

namespace SocialCoder.Web.Server.Controllers;

/// <summary>
/// Allows the system administrator to configure the application on initial launch.
/// Once configured and finalized, this controller will no longer be available due to it not having authentication 
/// </summary>
[ApiController, OnlyInSetupMode, Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly AppStateProvider _appStateProvider;
    private readonly SecureSettingsService _settingsService;
    private readonly IHostApplicationLifetime _application;

    public ConfigurationController(AppStateProvider provider, SecureSettingsService settingsService, IHostApplicationLifetime application)
    {
        this._appStateProvider = provider;
        this._settingsService = settingsService;
        this._application = application;
    }

    /// <summary>
    /// Allows system administrator to retrieve the current configuration
    /// </summary>
    /// <returns></returns>
    [HttpGet("settings")]
    public Task<IActionResult> GetSettings()
    {
        var currentSettings = this._settingsService.LoadSettings() ?? new();
        return Task.FromResult<IActionResult>(this.Ok(currentSettings));
    }

    /// <summary>
    /// Helps determine whether the application is in setup mode or not.
    /// </summary>
    /// <returns></returns>
    [HttpGet("is-in-setup-mode")]
    public Task<IActionResult> IsInSetupMode()
    {
        return Task.FromResult<IActionResult>(this.Ok(this._appStateProvider.IsInSetupMode));
    }

    /// <summary>
    /// Saves postgres settings
    /// </summary>
    /// <param name="dbSettings"></param>
    /// <returns></returns>
    [HttpPut("save-connection")]
    public async Task<IActionResult> SaveConnectionSettings([FromBody] PostgresConnection dbSettings)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest();
        }

        var currentSettings = this._settingsService.LoadSettings() ?? new();
        currentSettings.Postgres = dbSettings;
        await this._settingsService.SaveSettingsAsync(currentSettings);
        return this.Ok();
    }

    /// <summary>
    /// Deletes an OAuth provider
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpDelete("oauth-providers/{name}")]
    public async Task<IActionResult> DeleteOAuthProvider(string name)
    {
        if (name.IsNullOrWhiteSpace())
        {
            return this.BadRequest();
        }

        var currentSettings = this._settingsService.LoadSettings() ?? new();

        if (!currentSettings.OAuthSettings.ContainsKey(name))
        {
            return this.NotFound(name);
        }

        currentSettings.OAuthSettings.Remove(name);
        await this._settingsService.SaveSettingsAsync(currentSettings);
        return this.Ok();
    }

    /// <summary>
    /// Adds an OAuth provider
    /// </summary>
    /// <param name="oauth"></param>
    /// <returns></returns>
    [HttpPut("oauth-providers")]
    public async Task<IActionResult> SaveOAuth([FromBody] OAuthSetting oauth)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest();
        }

        var currentSettings = this._settingsService.LoadSettings() ?? new();

        currentSettings.OAuthSettings[oauth.Name] = oauth;
        await this._settingsService.SaveSettingsAsync(currentSettings);

        return this.Ok();
    }

    /// <summary>
    /// Will verify if the current configuration is valid, then flip setup to complete. This endpoint automatically verifies
    /// at least 1 OAuth provider is configured, and a valid database connection is provided.
    /// </summary>
    /// <returns>
    /// Bad Request if: Oauth provider is not configured, or database connection is not valid
    /// </returns>
    [HttpPost("finalize")]
    public async Task<IActionResult> FinalizeSetup()
    {
        var currentSettings = this._settingsService.LoadSettings();

        if (currentSettings is null)
        {
            return this.BadRequest("Must have at least 1 OAuth provider configured, and a valid database connection");
        }

        if (currentSettings.OAuthSettings.Count == 0)
        {
            return this.BadRequest("Requires at least on OAuth provider to be configured");
        }

        if (currentSettings.Postgres is null)
        {
            return this.BadRequest("Requires a Postgres connection to be configured");
        }

        var dbTest = await this.TestReachability(currentSettings.Postgres);

        if (!dbTest.Success)
        {
            return this.BadRequest(dbTest.Message);
        }

        currentSettings.IsSetupComplete = true;
        await this._settingsService.SaveSettingsAsync(currentSettings);

        /*
         * A process cannot replace itself. We don't have a clean way to restart everything.
         * 
         * Ideally, this application should be running within a docker container where the restart policy is set to unless-stopped or always
         * Which will auto restart the application
         */

        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            this._application.StopApplication();
        });

        return this.Ok();
    }

    /// <summary>
    /// Checks to see if a connection can be established to the Postgres server.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("test-reachability")]
    public async Task<IActionResult> TestPostgresReachability([FromBody] TestReachabilityRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(new ConnectionResult
            {
                Success = false,
                Message = "Invalid input. Please check all fields"
            });
        }

        var result = await this.TestReachability(request);

        return result.Success ? this.Ok(result) : this.BadRequest(result);
    }

    private async Task<ConnectionResult> TestReachability(TestReachabilityRequest request)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = request.Host,
            Port = request.Port,
            Username = request.UserId,
            Password = request.Password,
            Timeout = 15,
            CommandTimeout = 15
        };

        await using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

        try
        {
            await connection.OpenAsync();

            return new ConnectionResult
            {
                Success = true,
                Message = "Database is reachable, and credentials are valid"
            };
        }
        catch (NpgsqlException ex)
        {
            return new ConnectionResult
            {
                Success = false,
                Message = $"Database error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new ConnectionResult
            {
                Success = false,
                Message = $"An unexpected error occurred: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Tests the connection to the Postgres database
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("test-connection")]
    public async Task<IActionResult> TestPostgresConnection([FromBody] TestConnectionRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(new ConnectionResult
            {
                Success = false,
                Message = "Invalid input. Please check all fields"
            });
        }

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = request.Host,
            Port = request.Port,
            Database = request.Database,
            Username = request.UserId,
            Password = request.Password,
            Timeout = 15,
            CommandTimeout = 15
        };

        await using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

        try
        {
            await connection.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT 1", connection);
            await cmd.ExecuteScalarAsync();

            return this.Ok(new ConnectionResult
            {
                Success = true,
                Message = "Connection successful"
            });
        }
        catch (NpgsqlException ex)
        {
            return this.BadRequest(new ConnectionResult
            {
                Success = false,
                Message = $"Database error: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            return this.BadRequest(new ConnectionResult
            {
                Success = false,
                Message = $"An unexpected error occurred: {ex.Message}"
            });
        }
    }
}