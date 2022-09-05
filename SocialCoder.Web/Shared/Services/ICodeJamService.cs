using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;

namespace SocialCoder.Web.Shared.Services;

public interface ICodeJamService
{
    #region Topics
    /// <summary>
    /// Retrieve all topics
    /// </summary>
    /// <returns></returns>
    Task<PaginatedResponse<CodeJamTopic>> GetAllTopics(PaginationRequest? request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all topics that are currently active
    /// </summary>
    /// <param name="request"></param>
    /// <param name="date"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PaginatedResponse<CodeJamTopic>> GetActiveTopics(SpecificDateQuery? request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all topics that are currently accepting applicants
    /// </summary>
    /// <param name="request"></param>
    /// <param name="date"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PaginatedResponse<CodeJamTopic>> GetRegisterableTopics(SpecificDateQuery? request, CancellationToken cancellationToken = default);
    #endregion
}