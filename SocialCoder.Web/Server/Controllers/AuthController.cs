using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        SignInManager<ApplicationUser> signInManager, 
        ILogger<AuthController> logger, 
        IUserService userService)
    {
        _signInManager = signInManager;
        _logger = logger;
        _userService = userService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        //await _signInManager.SignOutAsync();
        await HttpContext.SignOutAsync();
        return Ok();
    }

    [HttpGet("{scheme?}")]
    public IActionResult Challenge([FromRoute] string scheme)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action("ExternalCallback", "Auth")
        };

        return Challenge(props, scheme);
    }

    [HttpGet, Authorize]
    public IActionResult TestAuth() => Redirect("~/");

    #region OAuth Callbacks (IDK Why but these were needed for this to work)
    [Route("/signin-discord")]
    public Task<IActionResult> SignInDiscord() => Task.FromResult<IActionResult>(Ok());

    [Route("/signin-google")]
    public Task<IActionResult> SignInGoogle() => Task.FromResult<IActionResult>(Ok());
    #endregion

    /// <summary>
    /// Obtain information about the user from HttpContext
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public UserInfo UserInfo()
        => new()
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name ?? string.Empty,
            Claims = User.Claims.ToDictionary(x => x.Type, x => x.Value)
        };
    
    public async Task<IActionResult> ExternalCallback()
    {
        try
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            
            var response = await _userService.GetUserFromOAuth(result);

            if (!response.Success || response.Data is null)
                return BadRequest(response.Message);
    
            //await HttpContext.SignInAsync(response.Data);
            await _signInManager.SignInAsync(response.Data, isPersistent: false, CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return BadRequest();
        }
    }
    
    /// <summary>
    /// Retrieve external authentication providers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Providers()
         => Ok((await _signInManager.GetExternalAuthenticationSchemesAsync()).Select(x=>new AuthProvider
             {
                 Name = x.Name,
                 DisplayName = x.DisplayName
             }));
}