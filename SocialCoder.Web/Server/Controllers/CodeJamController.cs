using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Services;

namespace SocialCoder.Web.Server.Controllers;

[ApiController, Route("/api/[controller]/[action]/{id?}")]
public class CodeJamController : ControllerBase
{
    private readonly ICodeJamService _cj;

    public CodeJamController(ICodeJamService cj)
    {
        _cj = cj;
    }

    [HttpPost]
    public async Task<PaginatedResponse<CodeJamTopic>> Topics(PaginationRequest pagination, CancellationToken cancellationToken)
        => await _cj.GetAllTopics(pagination, cancellationToken);

    [HttpPost, Route("/api/[controller]/Topics/active")] // TODO: Figure out how to capture user provided date times -- into their culture
    public async Task<PaginatedResponse<CodeJamTopic>> ActiveTopics(SpecificDateQuery query, CancellationToken cancellationToken)
        => await _cj.GetActiveTopics(query, cancellationToken);

    [HttpPost, Route("/api/[controller]/Topics/open")]  // TODO: Figure out how to capture user provided date times -- into their culture
    public async Task<PaginatedResponse<CodeJamTopic>> OpenTopics(SpecificDateQuery query, CancellationToken cancellationToken)
        => await _cj.GetRegisterableTopics(query, cancellationToken);
}