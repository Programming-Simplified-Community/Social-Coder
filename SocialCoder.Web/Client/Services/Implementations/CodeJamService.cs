using System.Net.Http.Json;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.Services;
using SocialCoder.Web.Shared.ViewModels.CodeJam;
using StrawberryShake;

namespace SocialCoder.Web.Client.Services.Implementations;

public class CodeJamService : ICodeJamService
{
    private readonly HttpClient _client;
    private readonly ILogger<CodeJamService> _logger;
    private readonly SocialCoderGraphQLClient _graph;
    
    public CodeJamService(HttpClient client, ILogger<CodeJamService> logger, SocialCoderGraphQLClient graph)
    {
        _client = client;
        _logger = logger;
        _graph = graph;
    }

    #region Administrative

    public async Task<ResultOf<CodeJamTopic>> AdminCreateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default)
    {
        var response = await _graph.CreateCodeJamTopic.ExecuteAsync(topic.Title,
            topic.Description,
            topic.BackgroundImageUrl ?? string.Empty,
            topic.JamStartDate.ToUniversalTime(),
            topic.JamEndDate.ToUniversalTime(),
            topic.RegistrationStartDate.ToUniversalTime(),
            cancellationToken);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message)); 
            _logger.LogError("GraphQL Errors while trying to use {Method}. {Errors}",nameof(AdminCreateTopic),
                errors);
            return ResultOf<CodeJamTopic>.Fail(errors);
        }

        if (!response.Data.CreateTopic.Success)
        {
            _logger.LogError("Error trying to create CodeJamTopic: {Error}", response.Data.CreateTopic.Message);
            return ResultOf<CodeJamTopic>.Fail(
                response.Data.CreateTopic.Message ?? "Error trying to create admin topic");
        }

        var data = response.Data.CreateTopic.Data;
        
        return ResultOf<CodeJamTopic>.Pass(new CodeJamTopic
        {
            Id = data.TopicId,
            Title = data.Title,
            Description = data.Description,
            BackgroundImageUrl = data.BackgroundImageUrl,
            JamStartDate = data.JamStartDate.DateTime,
            JamEndDate = data.JamEndDate.DateTime,
            RegistrationStartDate = data.RegistrationStartDate.DateTime
        });
    }

    public async Task<ResultOf> Delete(int topicId, CancellationToken cancellationToken = default)
    {
        var response = await _graph.DeleteCodeJamTopic.ExecuteAsync(topicId, cancellationToken);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            _logger.LogError("Error with GraphQL: {Error}", errors);
            return ResultOf.Fail(errors);
        }
        
        return ResultOf.Pass();
    }

    public async Task<ResultOf<CodeJamTopic>> AdminUpdateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default)
    {
        var response = await _graph.UpdateCodeJamTopic.ExecuteAsync(topic.Id,
            topic.Title, topic.Description, 
            topic.BackgroundImageUrl ?? string.Empty, 
            topic.JamStartDate.ToUniversalTime(),
            topic.JamEndDate.ToUniversalTime(),
            topic.RegistrationStartDate.ToUniversalTime(), topic.IsActive, cancellationToken);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            _logger.LogError("GraphQL errors while trying to use {Method}. {Errors}", nameof(AdminUpdateTopic),
                errors);
            return ResultOf<CodeJamTopic>.Fail(errors);
        }

        if (!response.Data.UpdateTopic.Success)
        {
            _logger.LogError("Error trying to update CodeJamTopic: {Id}, {Title}. {Error}", topic.Id, topic.Title,
                response.Data.UpdateTopic.Message);
            return ResultOf<CodeJamTopic>.Fail(response.Data.UpdateTopic.Message ?? "Error trying to update topic");
        }

        var data = response.Data.UpdateTopic.Data;
        return ResultOf<CodeJamTopic>.Pass(new()
        {
            Id = data!.TopicId,
            Title = data.Title,
            Description = data.Description,
            BackgroundImageUrl = data.BackgroundImageUrl,
            JamStartDate = data.JamStartDate.DateTime,
            JamEndDate = data.JamEndDate.DateTime,
            RegistrationStartDate = data.RegistrationStartDate.DateTime,
            IsActive = topic.IsActive
        });
    }

    #endregion

    /// <summary>
    /// Here for now. Trying to keep API between server/client as similar as possible.
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<ResultOf<CodeJamViewModel>> GetTopic(int topicId, string? userId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

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
    public async Task<ResultOf<CodeJamViewModel>> Register(CodeJamRegistrationRequest request, string? userId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _client.PostAsJsonAsync(string.Format(Endpoints.CODE_JAM_POST_TOPIC_REGISTER, request.TopicId), request, cancellationToken);
        
        if(!response.IsSuccessStatusCode)
            return ResultOf<CodeJamViewModel>.Fail(response.ReasonPhrase!);
        
        return await response.Content.ReadFromJsonAsync<ResultOf<CodeJamViewModel>>(
            cancellationToken: cancellationToken) ?? ResultOf<CodeJamViewModel>.Fail("Failed to parse");
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
    public async Task<ResultOf<CodeJamViewModel>> Abandon(CodeJamAbandonRequest request, string? userId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _client.PostAsJsonAsync(string.Format(Endpoints.CODE_JAM_POST_TOPIC_WITHDRAW, request.TopicId), request, cancellationToken);
        
        if(!response.IsSuccessStatusCode)
            return ResultOf<CodeJamViewModel>.Fail(response.ReasonPhrase!);
        
        var item = await response.Content.ReadFromJsonAsync<ResultOf<CodeJamViewModel>>(cancellationToken: cancellationToken);

        return item ?? ResultOf<CodeJamViewModel>.Fail("Failed to parse");
    }
}