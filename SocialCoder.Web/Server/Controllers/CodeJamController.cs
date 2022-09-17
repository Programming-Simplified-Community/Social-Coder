using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.Controllers;

[ApiController, Route("/api/[controller]/[action]")]
public partial class CodeJamController : ControllerBase
{
    private readonly ICodeJamService _cj;

    public CodeJamController(ICodeJamService cj)
    {
        _cj = cj;
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

}