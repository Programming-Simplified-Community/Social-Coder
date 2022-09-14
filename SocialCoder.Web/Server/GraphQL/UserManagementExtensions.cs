using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;

namespace SocialCoder.Web.Server.GraphQL;

[ExtendObjectType(typeof(BasicUserAccountInfo))]
public sealed class UserManagementExtensions
{
    /// <summary>
    /// In GraphQL allow us to query a user's roles
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<List<string>> GetUserRoles([Parent] BasicUserAccountInfo user,
        CancellationToken cancellationToken,
        [Service] ApplicationDbContext context) => 
            await (from userRole in context.UserRoles
                    join role in context.Roles
                        on userRole.RoleId equals role.Id
                    where userRole.UserId == user.UserId
                    select role.Name)
                .ToListAsync(cancellationToken);
}