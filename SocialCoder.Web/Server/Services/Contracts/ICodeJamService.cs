using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.Services.Contracts;

public interface ICodeJamService
{
    #region Administrative
    /// <summary>
    /// Enable an administrator to delete topic
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf> Delete(int topicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a topic
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<CodeJamTopic>> AdminUpdateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new topic
    /// </summary>
    /// <param name="topic">Topic to create</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<CodeJamTopic>> AdminCreateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default);
    #endregion
    
    #region Topics
    /// <summary>
    /// Retrieve a <see cref="CodeJamTopic"/> with specified <paramref name="topicId"/>
    /// </summary>
    /// <param name="topicId">Primary Key of item we're after</param>
    /// <param name="userId">Optional user Id -- so we can populate user-specified information</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A hydrated instance of <see cref="CodeJamViewModel"/> based on the target <see cref="CodeJamTopic"/></returns>
    Task<ResultOf<CodeJamViewModel>> GetTopic(int topicId, string? userId,
        CancellationToken cancellationToken = default);
    #endregion
    
    #region Registration for Topic

    Task<ResultOf<CodeJamViewModel>> Register(CodeJamRegistrationRequest request, string? userId, CancellationToken cancellationToken = default);

    Task<ResultOf<CodeJamViewModel>> Abandon(CodeJamAbandonRequest request, string? userId, CancellationToken cancellationToken = default);

    #endregion
}