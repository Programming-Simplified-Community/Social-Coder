using System.Net.Http.Json;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.Services;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Client.Services.Implementations;

public class CodeJamService : ICodeJamService
{
    private readonly HttpClient _client;
    private readonly ILogger<CodeJamService> _logger;
    
    public CodeJamService(HttpClient client, ILogger<CodeJamService> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    ///     For now the UserId is not used on the client side, just server side (pulled from HttpContext)
    ///     In the future I could see a use case where admins might have to do something on a user's
    ///     behalf maybe? If something broke? For now this is fine?
    /// </remarks>
    /// <returns></returns>
    public async Task<ResultOf> Register(CodeJamRegistrationRequest request, string? userId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _client.PostAsJsonAsync(string.Format(Endpoints.CODE_JAM_POST_TOPIC_REGISTER, request.TopicId), request, cancellationToken);

        return response.IsSuccessStatusCode ? ResultOf.Pass() : ResultOf.Fail(response.ReasonPhrase ?? await response.Content.ReadAsStringAsync(cancellationToken));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    ///     Same remarks as <see cref="Register"/>
    /// </remarks>
    /// <returns></returns>
    public async Task<ResultOf> Abandon(CodeJamAbandonRequest request, string? userId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _client.PostAsJsonAsync(string.Format(Endpoints.CODE_JAM_POST_TOPIC_WITHDRAW, request.TopicId), request, cancellationToken);
        
        return response.IsSuccessStatusCode ? ResultOf.Pass() : ResultOf.Fail(response.ReasonPhrase ??
            await response.Content.ReadAsStringAsync(cancellationToken));
    }
    
    public async Task<PaginatedResponse<CodeJamViewModel>> GetAllTopics(PaginationRequest? request, string? userId, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(Endpoints.CODE_JAM_POST_TOPICS, request ?? new(), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve topics. {Code}\n{Message}", response.StatusCode, response.ReasonPhrase);
            return new();
        }

        return await response.Content.ReadFromJsonAsync<PaginatedResponse<CodeJamViewModel>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<PaginatedResponse<CodeJamViewModel>> GetActiveTopics(SpecificDateQuery? request, string? userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(Endpoints.CODE_JAM_POST_TOPICS_ACTIVE, request ?? new() { Date = DateTime.UtcNow}, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve topics. {Code}\n{Message}", response.StatusCode, response.ReasonPhrase);
            return new();
        }

        return await response.Content.ReadFromJsonAsync<PaginatedResponse<CodeJamViewModel>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<PaginatedResponse<CodeJamViewModel>> GetRegisterableTopics(SpecificDateQuery? request, string? userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(Endpoints.CODE_JAM_POST_TOPICS_OPEN, request ?? new() { Date = DateTime.UtcNow}, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve registrable topics. {Code}\n{Message}", response.StatusCode,
                response.ReasonPhrase);
            return new();
        }
        
        return await response.Content.ReadFromJsonAsync<PaginatedResponse<CodeJamViewModel>>(cancellationToken: cancellationToken) ?? new();
    }
}