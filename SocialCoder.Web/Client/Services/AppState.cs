using System.Net.Http.Json;

namespace SocialCoder.Web.Client.Services;

public class AppState
{
    private readonly HttpClient _client;
    public bool IsInSetupMode { get; private set; }
    public bool IsLoaded { get; private set; }
    
    public AppState(HttpClient client)
    {
        _client = client;
    }

    public async Task LoadStateAsync()
    {
        IsInSetupMode = await _client.GetFromJsonAsync<bool>("api/Configuration/is-in-setup-mode");
        IsLoaded = true;
    }
}