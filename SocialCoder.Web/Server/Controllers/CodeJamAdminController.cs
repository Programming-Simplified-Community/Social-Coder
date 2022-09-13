using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.Controllers;

/*
   This partial class is tailored for administrative actions on code jam items
   This separation will help us find actions which are "admin" or "authenticated"
 */

public partial class CodeJamController
{
    [Authorize(Roles = Roles.Administrator),
     Route("/api/[controller]/admin/topics/{topicId:int}"),
     HttpDelete]
    public async Task<ResultOf> DeleteTopic([FromRoute] int topicId, CancellationToken cancellationToken)
        => await _cj.Delete(topicId, cancellationToken);

    [Authorize(Roles = Roles.Administrator),
     HttpPut,
     Route("/api/[controller]/admin/topics/{topicId:int}")]
    public async Task<ResultOf<CodeJamTopic>> AdminUpdateTopic([FromRoute] int topicId, [FromBody] CodeJamTopic topic,
        CancellationToken cancellationToken = default)
    {
        if (topicId != topic.Id) 
            return ResultOf<CodeJamTopic>.Fail("Invalid Request");
        
        return await _cj.AdminUpdateTopic(topic, cancellationToken);
    }

    [Authorize(Roles = Roles.Administrator),
     HttpPost,
     Route("/api/[controller]/admin/topics/create")]
    public async Task<ResultOf<CodeJamTopic>> AdminCreateTopic([FromBody] CodeJamTopic topic,
        CancellationToken cancellationToken = default)
        => await _cj.AdminCreateTopic(topic, cancellationToken);
}