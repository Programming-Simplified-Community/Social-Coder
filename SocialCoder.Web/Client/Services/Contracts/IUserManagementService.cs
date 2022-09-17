using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Client.Services.Contracts;

public interface IUserManagementService
{
    /// <summary>
    /// Enable privileged user to ban a user
    /// </summary>
    /// <remarks>
    /// <para>
    ///     We should not just hard delete a user from the database as that will allow them to immediately
    ///     return to the platform
    /// </para>
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ResultOf> BanUser(BanUserRequest request);

    /// <summary>
    /// Add a role to a specified user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ResultOf> AddRole(AddRoleToUserRequest request);

    /// <summary>
    /// Enable privileged user to remove a role from a specified user.
    /// Optional reason can be provided for auditing purposes 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ResultOf> RemoveRole(RemoveRoleFromUserRequest request);
}