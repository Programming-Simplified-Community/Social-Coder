using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Client.Services;

public interface IAuthorizeApi
{
    Task Login(LoginParameters parameters);
    Task Register(RegisterParameters registerParameters);
    Task Logout();
    Task<UserInfo> GetUserInfo();
}