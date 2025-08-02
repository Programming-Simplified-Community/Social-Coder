using HotChocolate.Authorization;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace SocialCoder.Web.Server.GraphQL;

public class BasicUserAccountInfo
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}

public class BasicRoleInfo
{
    public string RoleId { get; set; }

    public string Name { get; set; }
}

// ReSharper disable once InconsistentNaming
public partial class GraphQLQueries
{
    [UseProjection, Authorize(Roles= [Roles.Owner, Roles.Administrator])]
    public IOrderedQueryable<BasicRoleInfo> GetRoles([Service] ApplicationDbContext context)
        => context.Roles.Select(x => new BasicRoleInfo
            {
                RoleId = x.Id,
                Name = x.Name
            })
            .OrderBy(x => x.Name);
    
    [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize(Roles = [Roles.Owner, Roles.Administrator])]
    public IOrderedQueryable<BasicUserAccountInfo> GetUsers([Service] ApplicationDbContext context)
        => context.Users
            .Select(x => new BasicUserAccountInfo
            {
                UserId = x.Id,
                Email = x.Email,
                Username = x.UserName
            })
            .OrderBy(x => x.Username);
}