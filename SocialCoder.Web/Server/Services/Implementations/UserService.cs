using System.Security.Claims;
using System.Text;
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

    /// <summary>
    /// <para>
    ///     Checks to see if <paramref name="claimType"/> exists within <paramref name="claims"/>.
    /// </para>
    ///
    /// <para>
    ///     If it exists, <paramref name="claim"/> will be set to the found claim. Otherwise, it will be null
    /// </para>
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="claimType"></param>
    /// <param name="claim"></param>
    /// <returns>True if <paramref name="claimType"/> exists in <paramref name="claims"/></returns>
    bool HasClaim(ref List<Claim> claims, string claimType, out Claim? claim)
    {
        claim = claims.FirstOrDefault(x => x.Type == claimType);
        return claim is not null;
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

        #region Claims we want to capture from an OAuth Provider
        
        List<Claim> claimsToAdd = new();
        
        // Do we have a github url?
        if (HasClaim(ref claims, ClaimConstants.GITHUB_URL, out var githubUrl))
            claimsToAdd.Add(githubUrl!);
        
        // Do we have a github name?
        if(HasClaim(ref claims, ClaimConstants.GITHUB_NAME, out var githubUsername))
            claimsToAdd.Add(githubUsername!);

        if (claimsToAdd.Any())
            await _userManager.AddClaimsAsync(user, claimsToAdd);
        
        #endregion
        /*
           Our application requires at least 1 administrator. Considering the owner of this platform
           is going to be the FIRST to login - we'll make them administrator. 
           
           This does however mean that down the road we SHOULD NOT remove an admin role from a user if no other 
           user has the admin role. Otherwise a random user who signs up could potentially receive this
           elevated role...
        */

        var owners = await _userManager.GetUsersInRoleAsync(Roles.Owner);
        
        if(owners.Count > 0)  // we need to have an owner which is most likely the first person to login
            return ResultOf<ApplicationUser>.Pass(user);

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Owner);

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