using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Client.Services;

public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
{
    private UserInfo? _userInfoCache;
    private readonly IAuthorizeApi _authorizeApi;

    public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi)
    {
        _authorizeApi = authorizeApi;
    }

    public async Task Login(LoginParameters parameters)
    {
        await _authorizeApi.Login(parameters);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task Register(RegisterParameters parameters)
    {
        await _authorizeApi.Register(parameters);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task Logout()
    {
        await _authorizeApi.Logout();
        _userInfoCache = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task<UserInfo> GetUserInfo()
    {
        if (_userInfoCache is not null && _userInfoCache.IsAuthenticated) return _userInfoCache;
        _userInfoCache = await _authorizeApi.GetUserInfo();
        return _userInfoCache;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        try
        {
            var userInfo = await GetUserInfo();

            if (userInfo.IsAuthenticated)
            {
                var claims =
                    new[] { new Claim(ClaimTypes.Name, userInfo.UserName) }.Concat(
                        userInfo.Claims.Select(x => new Claim(x.Key, x.Value)));
                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch(HttpRequestException ex)
        {
            Console.Error.WriteLine("Request Failed: " + ex);
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}