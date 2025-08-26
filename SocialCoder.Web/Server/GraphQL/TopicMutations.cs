using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.GraphQL;

public partial class GraphQlMutations
{
    [UseMutationConvention, Authorize]
    public async Task<ResultOf<CodeJamViewModel>> Register(CodeJamRegistrationRequest request, string userId,
        [Service] ICodeJamService cj, CancellationToken cancellationToken)
        => await cj.Register(request, userId, cancellationToken);

    [UseMutationConvention, Authorize]
    public async Task<ResultOf<CodeJamViewModel>> Abandon(CodeJamAbandonRequest request, string userId,
        [Service] ICodeJamService cj, CancellationToken cancellationToken)
        => await cj.Abandon(request, userId, cancellationToken);

    [UseMutationConvention, Authorize(Roles = [Roles.Administrator, Roles.Owner])]
    public async Task<ResultOf<CodeJamTopic>> UpdateTopic(UpdateCodeJamTopicRequest request,
        [Service] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var existing = await context.CodeJamTopics.FirstOrDefaultAsync(x => x.Id == request.TopicId, cancellationToken);

        if (existing is null)
        {
            return ResultOf<CodeJamTopic>.Fail("Not Found");
        }

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.JamStartDate = request.JamStartDate.ToUniversalTime();
        existing.JamEndDate = request.JamEndDate.ToUniversalTime();
        existing.RegistrationStartDate = request.JamRegistrationStart.ToUniversalTime();
        existing.BackgroundImageUrl = request.BackgroundImageUrl;
        existing.IsActive = request.IsActive;

        context.Update(existing);
        await context.SaveChangesAsync(cancellationToken);

        return ResultOf<CodeJamTopic>.Pass(existing);
    }

    [UseMutationConvention, Authorize(Roles = [Roles.Administrator, Roles.Owner])]
    public async Task<ResultOf> DeleteTopic(int topicId,
        [Service] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var existing = await context.CodeJamTopics.FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);

        if (existing is null)
        {
            return ResultOf.Fail("Not Found");
        }

        context.CodeJamTopics.Remove(existing);
        await context.SaveChangesAsync(cancellationToken);

        return ResultOf.Pass();
    }

    [UseMutationConvention, Authorize(Roles = [Roles.Administrator, Roles.Owner])]
    public async Task<ResultOf<CodeJamTopic>> CreateTopic(CreateCodeJamTopicRequest request,
        [Service] ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var topic = new CodeJamTopic
        {
            Title = request.Title,
            Description = request.Description,
            BackgroundImageUrl = request.BackgroundImageUrl,
            JamEndDate = request.JamEndDate,
            JamStartDate = request.JamStartDate,
            RegistrationStartDate = request.JamRegistrationStart
        };

        context.CodeJamTopics.Add(topic);
        await context.SaveChangesAsync(cancellationToken);

        return ResultOf<CodeJamTopic>.Pass(topic);
    }
}