using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.Services;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.Controllers;

[ApiController, Route("/api/[controller]/[action]")]
public partial class CodeJamController : ControllerBase
{
    private readonly ICodeJamService _cj;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CodeJamController> _logger;
    
    public CodeJamController(ICodeJamService cj, UserManager<ApplicationUser> userManager, ILogger<CodeJamController> logger)
    {
        _cj = cj;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost, Authorize, Route("/api/[controller]/topics/{id:int}")]
    public async Task<ResultOf<CodeJamViewModel>> GetTopic([FromRoute] int id, [FromBody] CodeJamTopicRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.TopicId)
            return ResultOf<CodeJamViewModel>.Fail("Invalid request");

        return await _cj.GetTopic(request.TopicId,
            HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
            cancellationToken);
    }

    [HttpPost, Authorize, Route("/api/[controller]/topics/{topicId:int}/register")]
    public async Task<ResultOf<CodeJamViewModel>> Register([FromRoute] int topicId, [FromBody] CodeJamRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (topicId != request.TopicId)
            return ResultOf<CodeJamViewModel>.Fail("Bad Data");
        
        var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("An unknown user has attempted to register to topic {TopicId}", topicId);
            return ResultOf<CodeJamViewModel>.Fail("Bad Data");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Was unable to locate user with Id: {UserId} - who wanted to register for topic: {TopicId}",
                userId,
                topicId);
            return ResultOf<CodeJamViewModel>.Fail("Not Found");
        }

        return await _cj.Register(request, userId, cancellationToken);
    }

    [HttpPost, Authorize, Route("/api/[controller]/topics/{topicId:int}/withdraw")]
    public async Task<ResultOf<CodeJamViewModel>> Withdraw([FromRoute] int topicId, [FromBody] CodeJamAbandonRequest request,
        CancellationToken cancellationToken)
    {
        if (topicId != request.TopicId)
            return ResultOf<CodeJamViewModel>.Fail("Invalid Data");

        var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("An unknown user has attempted to abandon topic {TopicId}", topicId);
            return ResultOf<CodeJamViewModel>.Fail("Invalid Data");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Was unable to locate user with Id: {UserId} who wanted to abandon topic: {TopicId}",
                userId,
                topicId);
            return ResultOf<CodeJamViewModel>.Fail("Not Found");
        }

        var response = await _cj.Abandon(request, userId, cancellationToken);

        return response;
    }
}