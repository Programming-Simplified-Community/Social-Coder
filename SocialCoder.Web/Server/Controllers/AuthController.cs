using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Server.Attributes;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[DisabledInSetupMode]
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
        this._signInManager = signInManager;
        this._logger = logger;
        this._userService = userService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await this._signInManager.SignOutAsync();
        return this.Ok();
    }

    [HttpGet("{scheme?}")]
    public IActionResult Challenge([FromRoute] string scheme)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = this.Url.Action("ExternalCallback", "Auth")
        };

        return this.Challenge(props, scheme);
    }

    #region OAuth Callbacks (IDK Why but these were needed for this to work)
    [Route("/signin-discord")]
    public Task<IActionResult> SignInDiscord() => Task.FromResult<IActionResult>(this.Ok());

    [Route("/signin-google")]
    public Task<IActionResult> SignInGoogle() => Task.FromResult<IActionResult>(this.Ok());

    [Route("/signin-github")]
    public Task<IActionResult> SignInGithub() => Task.FromResult<IActionResult>(this.Ok());
    #endregion

    /// <summary>
    /// Obtain information about the user from HttpContext
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public UserInfo UserInfo()
        => new()
        {
            IsAuthenticated = this.User.Identity?.IsAuthenticated ?? false,
            UserName = this.User.Identity?.Name ?? string.Empty,
            Claims = this.User.Claims.ToDictionary(x => x.Type, x => x.Value)
        };

    public async Task<IActionResult> ExternalCallback()
    {
        try
        {
            var result = await this.HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            var response = await this._userService.GetUserFromOAuth(result);

            if (!response.Success || response.Data is null)
            {
                return this.BadRequest(response.Message);
            }

            await this._signInManager.SignInAsync(response.Data, isPersistent: false, CookieAuthenticationDefaults.AuthenticationScheme);
            return this.Redirect("~/");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex.ToString());
            return this.BadRequest();
        }
    }

    /// <summary>
    /// Retrieve external authentication providers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Providers()
         => this.Ok((await this._signInManager.GetExternalAuthenticationSchemesAsync()).Select(x => new AuthProvider
         {
             Name = x.Name,
             DisplayName = x.DisplayName
         }));
}