using System.Net.Http.Json;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Services;

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

    public async Task<PaginatedResponse<CodeJamTopic>> GetAllTopics(PaginationRequest? request, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(Endpoints.CODE_JAM_POST_TOPICS, request ?? new(), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve topics. {Code}\n{Message}", response.StatusCode, response.ReasonPhrase);
            return new();
        }

        return await response.Content.ReadFromJsonAsync<PaginatedResponse<CodeJamTopic>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<PaginatedResponse<CodeJamTopic>> GetActiveTopics(SpecificDateQuery? request,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(Endpoints.CODE_JAM_POST_TOPICS_ACTIVE, request ?? new() { Date = DateTime.UtcNow}, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve topics. {Code}\n{Message}", response.StatusCode, response.ReasonPhrase);
            return new();
        }

        return await response.Content.ReadFromJsonAsync<PaginatedResponse<CodeJamTopic>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<PaginatedResponse<CodeJamTopic>> GetRegisterableTopics(SpecificDateQuery? request,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(Endpoints.CODE_JAM_POST_TOPICS_OPEN, request ?? new() { Date = DateTime.UtcNow}, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve registrable topics. {Code}\n{Message}", response.StatusCode,
                response.ReasonPhrase);
            return new();
        }
        
        return await response.Content.ReadFromJsonAsync<PaginatedResponse<CodeJamTopic>>(cancellationToken: cancellationToken) ?? new();
    }
}