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
    [Authorize(Roles = Roles.ELEVATED_ROLES),
     Route("/api/[controller]/admin/topics/{topicId:int}"),
     HttpDelete]
    public async Task<ResultOf> DeleteTopic([FromRoute] int topicId, CancellationToken cancellationToken)
        => await _cj.Delete(topicId, cancellationToken);
}