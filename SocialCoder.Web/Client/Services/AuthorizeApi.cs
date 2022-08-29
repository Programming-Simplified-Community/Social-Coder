using System.Net.Http.Json;
using SocialCoder.Web.Shared.ViewModels;
namespace SocialCoder.Web.Client.Services;

public class AuthorizeApi : IAuthorizeApi
{
    private readonly HttpClient _httpClient;

    public AuthorizeApi(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task Login(LoginParameters loginParameters)
    {
        //var stringContent = new StringContent(JsonSerializer.Serialize(loginParameters), Encoding.UTF8, "application/json");
        var result = await _httpClient.PostAsJsonAsync("api/auth/Login", loginParameters);
        if (result.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
    }

    public async Task Logout()
    {
        var result = await _httpClient.PostAsync("api/auth/Logout", null);
        result.EnsureSuccessStatusCode();
    }

    public async Task Register(RegisterParameters registerParameters)
    {
        var result = await _httpClient.PostAsJsonAsync("api/auth/Register", registerParameters);
        if (result.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
    }

    public async Task<UserInfo> GetUserInfo()
    {
        try
        {
            Console.WriteLine(_httpClient.BaseAddress);
            var result = await _httpClient.GetStringAsync("api/Auth/UserInfo");
            Console.WriteLine(result);
            
            if (string.IsNullOrEmpty(result))
                return new UserInfo
                {
                    IsAuthenticated = false
                };

            return Newtonsoft.Json.JsonConvert.DeserializeObject<UserInfo>(result)!;
        }
        catch (Exception ex)
        {
            #if DEBUG
                Console.Error.WriteLine($"Please make sure you have `AllowedHosts` set to `*` in your configuration file!");
            #else
            Console.Error.WriteLine($"Unable to authenticate");
            #endif
            return new UserInfo { IsAuthenticated = false };
        }
    }
}