using SocialCoder.Web.Shared.Models.CodeJam;

namespace SocialCoder.Web.Server.Data;

public class Query
{
    [UseProjection, UseSorting, UseFiltering]
    public IQueryable<CodeJamTopic> GetTopics([Service] ApplicationDbContext context)
        => context.CodeJamTopics;
}