using Microsoft.AspNetCore.Authentication;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared;

namespace SocialCoder.Web.Server.Services.Contracts;

public interface IUserService
{
    /// <summary>
    /// Based on OAuth response, log this user into our application
    /// </summary>
    /// <param name="authResult">Authenticated result from OAuth provider</param>
    /// <returns></returns>
    Task<ResultOf<ApplicationUser>> GetUserFromOAuth(AuthenticateResult authResult);
}