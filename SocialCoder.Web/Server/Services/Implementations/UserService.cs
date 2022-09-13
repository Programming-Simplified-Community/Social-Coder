using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;

namespace SocialCoder.Web.Server.Services.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ResultOf<ApplicationUser>> GetUserFromOAuth(AuthenticateResult authResult)
    {
        if (!authResult.Succeeded)
            return ResultOf<ApplicationUser>.Fail("Unable to authenticate");
        
        var userPrincipal = authResult.Principal!;
        var claims = userPrincipal.Claims.ToList();
        var userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier ||
                                                     x.Type == JwtClaimTypes.Subject);
        var authProvider = authResult.Ticket!.Properties.Items[".AuthScheme"];
        
        if (userIdClaim is null)
        {
            _logger.LogError("Unable to determine user from external login provider: {AuthScheme}", authProvider);
            return ResultOf<ApplicationUser>.Fail("Unknown User");
        }
        
        var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value ?? email;
        
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(name))
        {
            _logger.LogError("Unable to determine user from external login provider: {AuthScheme}. Could not locate email or name",
                authProvider);
            return ResultOf<ApplicationUser>.Fail("No email or name provided");
        }
        
        #region Attempt To Locate User given OAuth claims
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null && !string.IsNullOrEmpty(name))
            user = await _userManager.FindByNameAsync(name);
        #endregion
        
        // Early exit if user was found
        if (user is not null)
        {
            var existingProviders = await _userManager.GetLoginsAsync(user);
            
            if(existingProviders.Any(x=>x.LoginProvider == authProvider))
                return ResultOf<ApplicationUser>.Pass(user);
            
            return ResultOf<ApplicationUser>.Fail("Provider not associated with this account");
        }
            
        user = new ApplicationUser
        {
            Email = email,
            UserName = name,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = name.ToUpper()
        };

        var createResult = await _userManager.CreateAsync(user);

        if (!createResult.Succeeded)
        {
            _logger.LogError("Was unable to create new user from {Scheme}\n{Error}",
                authProvider,
                string.Join("\n", createResult.Errors.Select(x => x.Description)));
            return ResultOf<ApplicationUser>.Fail("Was unable to create user");
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(
            authProvider,
            userIdClaim.Value,
            authProvider));

        if (!addLoginResult.Succeeded)
        {
            _logger.LogError("Was unable to add external login provider for {Scheme}\n{Error}",
                authProvider,
                string.Join("\n", addLoginResult.Errors.Select(x=>x.Description)));
            return ResultOf<ApplicationUser>.Fail("Unable to add external login provided");
        }

        /*
           Our application requires at least 1 administrator. Considering the owner of this platform
           is going to be the FIRST to login - we'll make them administrator. 
           
           This does however mean that down the road we SHOULD NOT remove an admin role from a user if no other 
           user has the admin role. Otherwise a random user who signs up could potentially receive this
           elevated role...
        */

        var admins = await _userManager.GetUsersInRoleAsync(Roles.Administrator);
        
        if(admins.Count > 0)  // If we have admins don't add this user to admin role
            return ResultOf<ApplicationUser>.Pass(user);

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Administrator);

        if (!roleResult.Succeeded)
            _logger.LogError("Was unable to add {Role} to {User}. {Error}",
                Roles.Administrator,
                name,
                string.Join("\n", roleResult.Errors.Select(x => x.Description)));
        else 
            _logger.LogInformation("Added {User} to {Role}", name, Roles.Administrator);
        return ResultOf<ApplicationUser>.Pass(user);
    }
}