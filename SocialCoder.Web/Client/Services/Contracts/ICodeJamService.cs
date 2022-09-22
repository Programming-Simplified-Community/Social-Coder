using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Client.Services.Contracts;

public interface ICodeJamService
{
    /// <summary>
    /// As a privileged user, create a new <see cref="CodeJamTopic"/>
    /// </summary>
    /// <param name="topic">Topic information to create</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<CodeJamTopic>> AdminCreateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// As a privileged user, update an existing <see cref="CodeJamTopic"/>
    /// </summary>
    /// <param name="topic">Topic information to update</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<CodeJamTopic>> AdminUpdateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// As a privileged user, delete a <see cref="CodeJamTopic"/>
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf> Delete(int topicId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// As an authenticated user register for a code jam
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<CodeJamViewModel>> Register(CodeJamRegistrationRequest request, string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// As an authenticated user, abandon a code jam
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<CodeJamViewModel>> Abandon(CodeJamAbandonRequest request, string userId,
        CancellationToken cancellationToken = default);
}