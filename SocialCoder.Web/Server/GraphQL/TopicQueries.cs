using HotChocolate.Authorization;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared.Models.CodeJam;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace SocialCoder.Web.Server.GraphQL;

// ReSharper disable once InconsistentNaming
public partial class GraphQLQueries
{
    [UsePaging, UseProjection, UseFiltering, UseSorting, Authorize]
    public IQueryable<CodeJamTopic> GetTopics([Service] ApplicationDbContext context)
        => context.CodeJamTopics;
}