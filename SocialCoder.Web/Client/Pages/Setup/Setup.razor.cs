using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Nextended.Core.Extensions;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.Models.Setup;

namespace SocialCoder.Web.Client.Pages.Setup;


public partial class Setup : ComponentBase
{
    private TestConnectionRequest _connectionRequest = new();

    [Inject] public HttpClient Client { get; set; }
    [Inject] public ISnackbar Snackbar { get; set; }
    [Inject] public ILogger<Setup> Logger { get; set; }

    private readonly List<OAuthSetting> _providers =
    [
        new() { Name = "Microsoft" },
        new() { Name = "Google" },
        new() { Name = "GitHub" },
        new() { Name = "Discord" }
    ];

    private string GetProviderIcon(OAuthSetting setting)
    {
        return setting.Name switch
        {
            "Microsoft" => Icons.Custom.Brands.Microsoft,
            "Discord" => Icons.Custom.Brands.Discord,
            "GitHub" => Icons.Custom.Brands.GitHub,
            "Google" => Icons.Custom.Brands.Google,
            _ => string.Empty
        };
    }

    private bool CanFinish
    {
        get
        {
            var validProvider = this._providers.Any(x => !x.ClientId.IsNullOrWhiteSpace() && !x.ClientSecret.IsNullOrWhiteSpace());
            var validConnection = !this._connectionRequest.Database.IsNullOrWhiteSpace() && !this._connectionRequest.Host.IsNullOrWhiteSpace() &&
                                  !this._connectionRequest.UserId.IsNullOrWhiteSpace() && !this._connectionRequest.Password.IsNullOrWhiteSpace();

            return validConnection && validProvider;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var settings = await this.GetSettings();

        foreach (var provider in settings.OAuthSettings.Values)
        {
            var oauth = this._providers.FirstOrDefault(x => x.Name == provider.Name);

            if (oauth is null)
            {
                continue;
            }

            oauth.ClientId = provider.ClientId;
            oauth.ClientSecret = provider.ClientSecret;
        }

        this._connectionRequest = settings.Postgres ?? new();
    }

    private async Task<AppSettings> GetSettings()
    {
        try
        {
            var response = await this.Client.GetFromJsonAsync<AppSettings>("api/Configuration/settings");
            return response ?? new();
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occurred while retrieving settings: {Exception}", ex);
            this.Snackbar.Add("An error occurred", Severity.Error);
        }

        return new();
    }

    private async Task SaveDatabaseConnection()
    {
        try
        {
            var response = await this.Client.PutAsJsonAsync("api/configuration/save-connection", this._connectionRequest);

            if (response.IsSuccessStatusCode)
            {
                this.Snackbar.Add("Database connection saved", Severity.Success);
            }
            else
            {
                this.Logger.LogError("An error occurred while saving database connection: {Message}", await response.Content.ReadAsStringAsync());
                this.Snackbar.Add("An error occurred", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            this.Snackbar.Add("An error occurred", Severity.Error);
            this.Logger.LogError("An error occurred while saving database connection: {Exception}", ex);
        }
    }

    private async Task DeleteOauthProvider(string name)
    {
        try
        {
            var response = await this.Client.DeleteAsync($"api/Configuration/oauth-providers/{name}");

            if (response.IsSuccessStatusCode)
            {
                this.Snackbar.Add($"OAuth Provider: {name} successfully removed", Severity.Success);

                var oauth = this._providers.First(x => x.Name == name);
                oauth.ClientId = string.Empty;
                oauth.ClientSecret = string.Empty;
            }
            else
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        this.Snackbar.Add($"OAuth Provider: {name} not found", Severity.Error);
                        break;
                    case System.Net.HttpStatusCode.BadRequest:
                        this.Snackbar.Add("Must provide a valid provider name", Severity.Error);
                        break;
                    default:
                        throw new NotImplementedException($"{response.StatusCode} - Message not implemented");
                }
            }
        }
        catch (Exception ex)
        {
            this.Snackbar.Add("An error occurred", Severity.Error);
            this.Logger.LogError("An error occurred while deleting OAuth Provider: {Name}: {Exception}", name, ex);
        }
    }

    private async Task SaveOauthProvider(OAuthSetting setting)
    {
        try
        {
            var response = await this.Client.PutAsJsonAsync("api/configuration/oauth-providers", setting);

            if (response.IsSuccessStatusCode)
            {
                this.Snackbar.Add($"{setting.Name} OAuth saved", Severity.Success);
                return;
            }

            this.Snackbar.Add("Was unable to save OAuth", Severity.Error);
            this.Logger.LogError("Was unable to save \"{Provider}\" OAuth. {Message}", setting.Name,
                await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            this.Logger.LogError("Failed to save oauth provider: {Exception}", ex);
            this.Snackbar.Add("An error occurred", Severity.Error);
        }
    }

    private async Task FinalizeSetup()
    {
        try
        {
            var response = await this.Client.PostAsync("api/configuration/finalize", null);

            if (response.IsSuccessStatusCode)
            {
                this.Snackbar.Add("Setup Complete. Server requires restart", Severity.Success);
                return;
            }

            this.Snackbar.Add(await response.Content.ReadAsStringAsync(), Severity.Error);
            this.Logger.LogError("Was unable to complete setup: {Response}", await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            this.Logger.LogError("An error occurred while attempting to finalize setup: {Exception}", ex);
            this.Snackbar.Add("An error occurred while processing request", Severity.Error);
        }
    }

    private async Task TestConnection()
    {
        try
        {
            var response = await this.Client.PostAsJsonAsync("api/configuration/test-connection", this._connectionRequest);
            var result = await response.Content.ReadFromJsonAsync<ConnectionResult>();

            if (result is null)
            {
                this.Snackbar.Add("Was unable to parse result", Severity.Error);

                return;
            }

            this.Snackbar.Add(result.Message, response.IsSuccessStatusCode ? Severity.Success : Severity.Error);

            if (!response.IsSuccessStatusCode)
            {
                this.Logger.LogError("Was unable to reach Database. {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            this.Snackbar.Add("An error occurred", Severity.Error);
            this.Logger.LogError("An error occurred while testing connection to Database. {Exception}", ex);
        }

    }

    private async Task TestReachability()
    {
        try
        {
            var response = await this.Client.PostAsJsonAsync("api/configuration/test-reachability", (TestReachabilityRequest)this._connectionRequest);
            var result = await response.Content.ReadFromJsonAsync<ConnectionResult>();

            if (result is null)
            {
                this.Snackbar.Add("Was unable to parse result", Severity.Error);

                return;
            }

            this.Snackbar.Add(result.Message, response.IsSuccessStatusCode ? Severity.Success : Severity.Error);

            if (!response.IsSuccessStatusCode)
            {
                this.Logger.LogError("Was unable to reach Postgres. {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            this.Snackbar.Add("An error occurred", Severity.Error);
            this.Logger.LogError("An error occurred while testing reachability to Postgres. {Exception}", ex);
        }
    }
}