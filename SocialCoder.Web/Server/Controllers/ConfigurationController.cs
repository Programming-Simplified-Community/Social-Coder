using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Server.Services;

namespace SocialCoder.Web.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly AppStateProvider _appStateProvider;

    public ConfigurationController(AppStateProvider provider)
    {
        _appStateProvider = provider;
    }

    [HttpGet("is-in-setup-mode")]
    public async Task<IActionResult> IsInSetupMode()
    {
        return Ok(_appStateProvider.IsInSetupMode);
    }
}