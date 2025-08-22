using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SocialCoder.Web.Server.Services;
using SocialCoder.Web.Shared.Models.Setup;

namespace SocialCoder.Web.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("settings")]
    public Task<IActionResult> GetSettings()
    {
        var currentSettings = this._settingsService.LoadSettings() ?? new();
        return Task.FromResult<IActionResult>(this.Ok(currentSettings));
    }

    [HttpGet("is-in-setup-mode")]
    public Task<IActionResult> IsInSetupMode()
    {
        return Task.FromResult<IActionResult>(this.Ok(this._appStateProvider.IsInSetupMode));
    }

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

    [HttpPut("save-oauth")]
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

    [HttpPost("finalize")]
    public async Task<IActionResult> FinalizeSetup()
    {
        var currentSettings = this._settingsService.LoadSettings();

        if (currentSettings is null)
        {
            return this.BadRequest();
        }

        if (currentSettings.OAuthSettings.Count == 0)
        {
            return this.BadRequest("Requires at least on OAuth provider to be configured");
        }

        if (currentSettings.Postgres is null)
        {
            return this.BadRequest("Requires a Postgres connection to be configured");
        }

        currentSettings.IsSetupComplete = true;
        await this._settingsService.SaveSettingsAsync(currentSettings);

        /*
         * A process cannot replace itself. We don't have a clean way to restart everything.
         * 
         * Ideally, this application should be running within a docker container where the restart policy is set to unless-stopped or always
         * Which will auto restart the application
         */

        this._application.StopApplication();
        return this.Ok();
    }

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

            return this.Ok(new ConnectionResult
            {
                Success = true,
                Message = "Database is reachable, and credentials are valid"
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