using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginParameters parameters)
    {
        var user = await _userManager.FindByNameAsync(parameters.UserName);
        if (user is null) return BadRequest("Invalid login attempt");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password, false);
        if (!signInResult.Succeeded) return BadRequest("Invalid login attempt");

        await _signInManager.SignInAsync(user, parameters.RememberMe);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterParameters parameters)
    {
        var user = new ApplicationUser()
        {
            UserName = parameters.UserName,
            Email = parameters.Email,
            NormalizedEmail = parameters.Email.ToUpper(),
            NormalizedUserName = parameters.UserName.ToUpper()
        };

        var result = await _userManager.CreateAsync(user, parameters.Password);
        if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);
        
        return await Login(new()
        {
            UserName = parameters.UserName,
            Password = parameters.Password
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
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

    [Route("/signin-discord")]
    public async Task<IActionResult> SignInDiscord()
    {
        return Ok();
    }

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

            if (result.Ticket is null)
                _logger.LogError("ticket is not here");
            else
            {
                _logger.LogWarning("I have the ticket");
                StringBuilder sb = new();
                
                sb.AppendLine($"Auth Scheme: {result.Ticket.AuthenticationScheme}");
                sb.AppendLine(
                    $"Auth Items: {string.Join("\n\t", result.Ticket.Properties.Items.Select(x => $"{x.Key}: {x.Value}"))}");
                sb.AppendLine(
                    $"Auth Props: {string.Join("\n\t", result.Ticket.Properties.Parameters.Select(x => $"{x.Key}: {x.Value}"))}");
                sb.AppendLine($"Claims: {string.Join("\n\t",result.Ticket.Principal.Claims.Select(x => $"{x.Type}: {x.Value}"))}");

                _logger.LogInformation(sb.ToString());
            }
            
            if (!result.Succeeded)
            {
                _logger.LogError(result.Failure?.Message ?? "Error with external auth");
                throw new Exception("External authentication error");
            }

            var externalUser = result.Principal!;
            var claims = externalUser.Claims.ToList();
            var userIdClaim =
                claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject || x.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
                throw new Exception("Unknown User");
            _logger.LogInformation("{User} - {Claims}\n{Scheme}", externalUser.Identity?.Name,
                string.Join("\n", claims.Select(x=>$"{x.Type}: {x.Value}")),
                HttpContext.Request.Headers.Referer);
            
            // Does this user already exist within our application?
            var existingUser = await _userManager.FindByEmailAsync(claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ??
                                          string.Empty);

            if (existingUser is null)
            {
                var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value ?? string.Empty;
                var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? string.Empty;
                var createUserResult = await _userManager.CreateAsync(new ApplicationUser
                {
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    UserName = name,
                    NormalizedUserName = name.ToUpper()
                });

                if (!createUserResult.Succeeded)
                {
                    _logger.LogError("Was unable to create new user from {Scheme}\n{Error}",
                        result.Ticket.AuthenticationScheme,
                        string.Join("\n", createUserResult.Errors.Select(x=>x.Description)));
                    throw new Exception("Bad request");
                }
                
                existingUser = await _userManager.FindByNameAsync(name);
                
                var addLoginResult = await _userManager.AddLoginAsync(existingUser!, new UserLoginInfo(
                    result.Ticket.AuthenticationScheme,
                    userIdClaim.Value,
                    result.Ticket.AuthenticationScheme));

                if (!addLoginResult.Succeeded)
                    _logger.LogError("Was unable to add external login provider for {Scheme}\n{Error}",
                        result.Ticket.AuthenticationScheme,
                        string.Join("\n", addLoginResult.Errors.Select(x=>x.Description)));
                
            }

            await _signInManager.SignInAsync(existingUser, isPersistent: false);
            // await HttpContext.SignInAsync(externalUser);
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