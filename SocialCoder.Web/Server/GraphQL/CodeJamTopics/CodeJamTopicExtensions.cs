using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared.Models.CodeJam;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace SocialCoder.Web.Server.GraphQL.CodeJamTopics;

[ExtendObjectType(typeof(CodeJamTopic))]
public sealed class CodeJamTopicQueryExtensions
{
    public int TopicId([Parent] CodeJamTopic topic) => topic.Id;
    
    /// <summary>
    /// In GraphQL allow us to query how many people signed up for the topic
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<int> TotalApplicants(
        [Parent] CodeJamTopic parent, 
        CancellationToken cancellationToken,
        [Service] ApplicationDbContext context)
        => await context.CodeJamRegistrations.CountAsync(x => x.CodeJamTopicId == parent.Id && x.AbandonedOn == null,
            cancellationToken);
    
    /// <summary>
    /// In GraphQL allow us to query how many people signed up to be on a team
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<int> TeamApplicants([Parent] CodeJamTopic parent, CancellationToken cancellationToken,
        [Service] ApplicationDbContext context)
        => await context.CodeJamRegistrations.CountAsync(
            x => x.CodeJamTopicId == parent.Id && x.AbandonedOn == null & x.PreferTeam, cancellationToken);
    
    /// <summary>
    /// In GraphQL allow us to query whether or not a specified user is registered with the code-jam
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="db"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> IsRegistered([Parent] CodeJamTopic parent, CancellationToken cancellationToken,
        [Service] ApplicationDbContext db,
        string? userId)
    {
        return await db.CodeJamRegistrations.AnyAsync(
            x => x.CodeJamTopicId == parent.Id && x.AbandonedOn == null && x.UserId == userId, cancellationToken);
    }
    
    /// <summary>
    /// In GraphQL allow us to query how many people signed up to as a lone-wolf/solo
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [BindMember(nameof(CodeJamTopic.Id))]
    public async Task<int> SoloApplicants(
        [Parent] CodeJamTopic parent, 
        CancellationToken cancellationToken,
        [Service] ApplicationDbContext context
    )
        => await context.CodeJamRegistrations.CountAsync(x =>
            x.CodeJamTopicId == parent.Id && x.AbandonedOn == null && !x.PreferTeam, cancellationToken);
}