using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Shared.Services;

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
    /// As an administrator -- 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PaginatedResponse<CodeJamAdminViewModel>> AdminGetTopics(PaginationRequest? request, CancellationToken cancellationToken = default);
    #endregion
    
    #region Topics
    /// <summary>
    /// Retrieve all topics
    /// </summary>
    /// <returns></returns>
    Task<PaginatedResponse<CodeJamViewModel>> GetAllTopics(PaginationRequest? request, string? userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all topics that are currently active
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PaginatedResponse<CodeJamViewModel>> GetActiveTopics(SpecificDateQuery? request, string? userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieve all topics that are currently accepting applicants
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PaginatedResponse<CodeJamViewModel>> GetRegisterableTopics(SpecificDateQuery? request, string? userId, CancellationToken cancellationToken = default);

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