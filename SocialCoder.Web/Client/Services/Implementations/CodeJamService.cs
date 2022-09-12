﻿using System.Net.Http.Json;
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

    #region Administrative

    public async Task<ResultOf> Delete(int topicId, CancellationToken cancellationToken = default)
    {
        var response =
            await _client.DeleteAsync(string.Format(Endpoints.CODEJAM_DELETE_TOPIC, topicId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            return ResultOf.Fail(response.ReasonPhrase ?? "Was unable to delete");

        return await response.Content.ReadFromJsonAsync<ResultOf>(cancellationToken: cancellationToken)
               ?? ResultOf.Fail("Failed to parse");
    }

    public async Task<ResultOf<CodeJamTopic>> AdminUpdateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default)
    {
        var response = await _client.PutAsJsonAsync(string.Format(Endpoints.CODE_JAM_PUT_TOPIC, topic.Id), topic, cancellationToken);
        
        if(!response.IsSuccessStatusCode)
            return ResultOf<CodeJamTopic>.Fail(response.ReasonPhrase ?? "Failed to update");

        return await response.Content.ReadFromJsonAsync<ResultOf<CodeJamTopic>>(cancellationToken: cancellationToken) ??
               ResultOf<CodeJamTopic>.Fail("Unable to parse");
    }

    #endregion
    
    public async Task<ResultOf<CodeJamViewModel>> GetTopic(int topicId, string? userId, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(string.Format(Endpoints.CODE_JAM_POST_GET_TOPIC, topicId),
            new CodeJamTopicRequest
            {
                TopicId = topicId
            }, cancellationToken);

        if (!response.IsSuccessStatusCode)
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