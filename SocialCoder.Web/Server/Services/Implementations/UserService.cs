using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.Management;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Server.Services.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger, ApplicationDbContext context)
    {
        _userManager = userManager;
        _logger = logger;
        _context = context;
    }
    
    #region Management of users

    public async Task<ResultOf> BanUser(BanUserRequest request, string callingUserId, CancellationToken cancellationToken = default)
    {
        if (request.Reason.Length < 10)
            return ResultOf.Fail("Need some type of reason that's at least 10 characters in length");

        var banner = await _userManager.FindByIdAsync(callingUserId);
        
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (banner is null || user is null)
            return ResultOf.Fail("Invalid Request");
        
        var userRoles = await _userManager.GetRolesAsync(user);

        if (userRoles is not null && userRoles.Any(x => x is Roles.Administrator or Roles.Owner))
            return ResultOf.Fail("Cannot ban an admin or owner");
     
        // Find this user
        var existingBan = await _context.PlatformBans
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (existingBan is null)
            _context.PlatformBans.Add(new PlatformUserBan
            {
                Reason = request.Reason,
                CreatedByUserId = callingUserId,
                UserId = request.UserId
            });
        else
        {
            existingBan.Reason = request.Reason;
            existingBan.EditedByUserId = callingUserId;
            existingBan.EditedOn = DateTime.UtcNow;
            _context.PlatformBans.Update(existingBan);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Username} was banned by {Banner} for '{Reason}'",
                user.UserName,
                banner.UserName,
                request.Reason);
            return ResultOf.Pass();
        }
        catch (Exception ex)
        {
            _logger.LogError("Was unable to ban {Username} by {Banner} for '{Reason}' due to an exception: {Error}",
                user.UserName,
                banner.UserName,
                request.Reason,
                ex);
            return ResultOf.Fail("Server error");
        }
    }

    public async Task<ResultOf> AddRoleToUser(AddRoleToUserRequest request, string callingUserId,
        CancellationToken cancellationToken = default)
    {
        var callingUser = await _userManager.FindByIdAsync(callingUserId);
        var callingRoles = await _userManager.GetRolesAsync(callingUser);

        var isOwner = callingRoles.Any(x => x == Roles.Owner);
        
        var user = await _userManager.FindByIdAsync(request.UserId);
        var userRoles = await _userManager.GetRolesAsync(user);

        if (userRoles.Any(x => x == Roles.Administrator) && !isOwner)
            return ResultOf.Fail("Unable to add role");
        
        if(user is null || callingUser is null)
            return ResultOf.Fail("Invalid Request");

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);

        if (result.Succeeded)
        {
            _logger.LogInformation("{User} was granted {Role} by {Admin}",
                user.UserName,
                callingUser.UserName,
                request.RoleName);
            return ResultOf.Pass();
        }
        
        _logger.LogError("Was unable to grant {Role} to {User} by {Admin}. {Error}",
            request.RoleName,
            user.UserName,
            callingUser.UserName,
            string.Join("\n", result.Errors.Select(x=>x.Description)));
        return ResultOf.Fail("Error trying to add role");
    }

    public async Task<ResultOf> RemoveRoleFromUser(RemoveRoleFromUserRequest request, string callingUserId,
        CancellationToken cancellationToken = default)
    {
        var callingUser = await _userManager.FindByIdAsync(callingUserId);
        var callingRoles = await _userManager.GetRolesAsync(callingUser);

        var isOwner = callingRoles.Any(x => x == Roles.Owner);
        
        var user = await _userManager.FindByIdAsync(request.UserId);
        var userRoles = await _userManager.GetRolesAsync(user);
        
        if(userRoles.Any(x=>x is Roles.Administrator or Roles.Owner) && !isOwner)
            return ResultOf.Fail("Unable to remove roles from another admin");
        
        if (callingUser is null || user is null)
            return ResultOf.Fail("Invalid Request");

        var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);

        if (result.Succeeded)
        {
            _logger.LogInformation("Removed {RoleName} from {User} by {Admin}: '{Reason}'",
                request.RoleName,
                user.UserName,
                callingUser.UserName,
                request.Reason);
            return ResultOf.Pass();
        }
        
        _logger.LogInformation("Unable to remove {RoleName} from {User} by {Admin}: {Reason}\n{Error}",
            request.RoleName,
            user.UserName,
            callingUser.UserName,
            request.Reason,
            string.Join("\n\n", result.Errors.Select(x=>x.Description)));
        return ResultOf.Fail("Error trying to remove role");
    }
    #endregion

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
        if (HasClaim(ref claims, ClaimConstants.GithubUrl, out var githubUrl))
            claimsToAdd.Add(githubUrl!);
        
        // Do we have a github name?
        if(HasClaim(ref claims, ClaimConstants.GithubName, out var githubUsername))
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