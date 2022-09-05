using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Shared.Services;

public interface ICodeJamService
{
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
    #endregion
    
    #region Registration for Topic

    Task<ResultOf> Register(CodeJamRegistrationRequest request, string? userId, CancellationToken cancellationToken = default);

    Task<ResultOf> Abandon(CodeJamAbandonRequest request, string? userId, CancellationToken cancellationToken = default);

    #endregion
}