using System.Net.Http.Json;

namespace SocialCoder.Web.Client.Services;

public class AppState
{
    private readonly HttpClient _client;
    public bool IsInSetupMode { get; private set; }
    public bool IsLoaded { get; private set; }

    public AppState(HttpClient client)
    {
        this._client = client;
    }

    public async Task LoadStateAsync()
    {
        this.IsInSetupMode = await this._client.GetFromJsonAsync<bool>("api/Configuration/is-in-setup-mode");
        this.IsLoaded = true;
    }
}