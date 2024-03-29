﻿using SocialCoder.Web.Client.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;
using StrawberryShake;

namespace SocialCoder.Web.Client.Services.Implementations;

public class CodeJamService : ICodeJamService
{
    private readonly ILogger<CodeJamService> _logger;
    private readonly SocialCoderGraphQLClient _graph;
    
    public CodeJamService(ILogger<CodeJamService> logger, SocialCoderGraphQLClient graph)
    {
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
            Id = data!.TopicId,
            Title = data.Title,
            Description = data.Description,
            BackgroundImageUrl = data.BackgroundImageUrl,
            JamStartDate = data.JamStartDate.DateTime,
            JamEndDate = data.JamEndDate.DateTime,
            RegistrationStartDate = data.RegistrationStartDate.DateTime
        });
    }

    public async Task<ResultOf> Delete(int topicId, string userId, CancellationToken cancellationToken = default)
    {
        var response = await _graph.DeleteCodeJamTopic.ExecuteAsync(topicId, cancellationToken);

        if (!response.IsErrorResult() && response.Data is not null) return ResultOf.Pass();
        var errors = string.Join("\n", response.Errors.Select(x => x.Message));
        _logger.LogError("Error with GraphQL: {Error}", errors);
        return ResultOf.Fail(errors);
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
    public async Task<ResultOf<CodeJamViewModel>> Register(CodeJamRegistrationRequest request, string userId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _graph.RegisterCodeJam.ExecuteAsync(request.TopicId, request.PreferTeam, userId, cancellationToken);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            _logger.LogError("Error with GraphQL {Method}, {Error}", nameof(Register), errors);
            return ResultOf<CodeJamViewModel>.Fail(errors);
        }
        
        if(!response.Data.Register.Success)
            return ResultOf<CodeJamViewModel>.Fail(response.Data.Register.Message ?? "Error registering");

        var data = response.Data.Register.Data!;
        return ResultOf<CodeJamViewModel>.Pass(new()
        {
            TotalSoloApplicants = data.TotalSoloApplicants,
            TotalTeamApplicants = data.TotalTeamApplicants,
            IsRegistered = true
        });
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
    public async Task<ResultOf<CodeJamViewModel>> Abandon(CodeJamAbandonRequest request, string userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _graph.AbandonCodeJam.ExecuteAsync(request.TopicId, userId, cancellationToken);

        if (response.IsErrorResult() || response.Data is null)
        {
            var errors = string.Join("\n", response.Errors.Select(x => x.Message));
            _logger.LogError("Error with GraphQL {Method}, {Error}", nameof(Abandon), errors);
            return ResultOf<CodeJamViewModel>.Fail(errors);
        }
        
        if(!response.Data.Abandon.Success)
            return ResultOf<CodeJamViewModel>.Fail(response.Data.Abandon.Message ?? "Error abandoned");

        var data = response.Data.Abandon.Data!;
        return ResultOf<CodeJamViewModel>.Pass(new()
        {
            TotalSoloApplicants = data.TotalSoloApplicants,
            TotalTeamApplicants = data.TotalTeamApplicants,
            IsRegistered = false
        });
    }
}