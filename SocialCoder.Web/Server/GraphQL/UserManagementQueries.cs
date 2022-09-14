using HotChocolate.AspNetCore.Authorization;
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

// ReSharper disable once InconsistentNaming
public partial class GraphQLQueries
{
    [UsePaging, UseOffsetPaging(IncludeTotalCount = true), UseProjection, UseFiltering, UseSorting, Authorize(Roles = new[]{Roles.Owner, Roles.Administrator})]
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