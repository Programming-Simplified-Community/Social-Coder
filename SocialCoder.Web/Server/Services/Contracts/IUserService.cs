using Microsoft.AspNetCore.Authentication;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Server.Services.Contracts;

public interface IUserService
{
    /// <summary>
    /// Based on OAuth response, log this user into our application
    /// </summary>
    /// <param name="authResult">Authenticated result from OAuth provider</param>
    /// <returns></returns>
    Task<ResultOf<ApplicationUser>> GetUserFromOAuth(AuthenticateResult authResult);

    /// <summary>
    /// Ban a user from the platform
    /// </summary>
    /// <param name="request"></param>
    /// <param name="callingUserId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<ApplicationUser?>> BanUser(BanUserRequest request, string callingUserId, CancellationToken cancellationToken = default);


    /// <summary>
    /// Add a role to user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="callingUserId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<ApplicationUser?>> AddRoleToUser(AddRoleToUserRequest request, string callingUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove role from user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="callingUserId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<ApplicationUser?>> RemoveRoleFromUser(RemoveRoleFromUserRequest request, string callingUserId, CancellationToken cancellationToken = default);
}