using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.Services;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.Controllers;

[ApiController, Route("/api/[controller]/[action]")]
public class CodeJamController : ControllerBase
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

    [HttpPost, Authorize, Route("/api/[controller]/topics/{topicId}/register")]
    public async Task<IActionResult> Register([FromRoute] int topicId, [FromBody] CodeJamRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (topicId != request.TopicId)
            return BadRequest("Invalid Data");
        
        var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("An unknown user has attempted to register to topic {TopicId}", topicId);
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Was unable to locate user with Id: {UserId} - who wanted to register for topic: {TopicId}",
                userId,
                topicId);
            return BadRequest();
        }

        var response = await _cj.Register(request, userId, cancellationToken);

        return response.Success ? Ok() : BadRequest(response.Message);
    }

    [HttpPost, Authorize, Route("/api/[controller]/topics/{topicId}/withdraw")]
    public async Task<IActionResult> Withdraw([FromRoute] int topicId, [FromBody] CodeJamAbandonRequest request,
        CancellationToken cancellationToken)
    {
        if (topicId != request.TopicId)
            return BadRequest("Invalid Data");

        var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("An unknown user has attempted to abandon topic {TopicId}", topicId);
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Was unable to locate user with Id: {UserId} who wanted to abandon topic: {TopicId}",
                userId,
                topicId);
            return BadRequest();
        }

        var response = await _cj.Abandon(request, userId, cancellationToken);

        return response.Success ? Ok() : BadRequest(response.Message);
    }
    
    [HttpPost]
    public async Task<PaginatedResponse<CodeJamViewModel>> Topics(PaginationRequest pagination, CancellationToken cancellationToken)
        => await _cj.GetAllTopics(pagination, HttpContext.User.Claims.FirstOrDefault(x=>x.Type == ClaimTypes.NameIdentifier)?.Value, cancellationToken);

    [HttpPost, Route("/api/[controller]/Topics/active")] // TODO: Figure out how to capture user provided date times -- into their culture
    public async Task<PaginatedResponse<CodeJamViewModel>> ActiveTopics(SpecificDateQuery query, CancellationToken cancellationToken)
        => await _cj.GetActiveTopics(query, HttpContext.User.Claims.FirstOrDefault(x=>x.Type == ClaimTypes.NameIdentifier)?.Value,  cancellationToken);

    [HttpPost, Route("/api/[controller]/Topics/open")]  // TODO: Figure out how to capture user provided date times -- into their culture
    public async Task<PaginatedResponse<CodeJamViewModel>> OpenTopics(SpecificDateQuery query, CancellationToken cancellationToken)
        => await _cj.GetRegisterableTopics(query, HttpContext.User.Claims.FirstOrDefault(x=>x.Type == ClaimTypes.NameIdentifier)?.Value,  cancellationToken);
}