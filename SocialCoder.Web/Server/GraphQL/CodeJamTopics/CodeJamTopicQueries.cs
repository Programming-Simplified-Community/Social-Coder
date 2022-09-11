using HotChocolate.AspNetCore.Authorization;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared.Models.CodeJam;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace SocialCoder.Web.Server.GraphQL.CodeJamTopics;

public class CodeJamTopicQueries
{
    [UsePaging, UseOffsetPaging, UseProjection, UseFiltering, UseSorting]
    public IOrderedQueryable<CodeJamTopic> GetTopics([Service] ApplicationDbContext context)
        => context.CodeJamTopics.OrderBy(x=>x.JamStartDate);
}