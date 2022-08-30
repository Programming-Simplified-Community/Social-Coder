using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Client.Services.Contracts;

/// <summary>
/// Basic contract for how a user will interface with our Identity services
/// </summary>
public interface IAuthorizeApi
{
    Task Logout();
    Task<UserInfo> GetUserInfo();
}