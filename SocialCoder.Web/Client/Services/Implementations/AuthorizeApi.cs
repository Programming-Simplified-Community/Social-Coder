using System.Net.Http.Json;
using SocialCoder.Web.Client.Services.Contracts;
using SocialCoder.Web.Shared.ViewModels;
namespace SocialCoder.Web.Client.Services.Implementations;

public class AuthorizeApi : IAuthorizeApi
{
    private readonly HttpClient _httpClient;

    public AuthorizeApi(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task Logout()
    {
        var result = await _httpClient.PostAsync("api/auth/Logout", null);
        result.EnsureSuccessStatusCode();
    }

    public async Task<UserInfo> GetUserInfo()
    {
        try
        {
            var result = await _httpClient.GetStringAsync("api/Auth/UserInfo");
            
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