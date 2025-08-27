using System.Collections.Immutable;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Identity;
using SocialCoder.Web.Client;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.Account;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Server.GraphQL;

public partial class GraphQlMutations
{
    [UseMutationConvention, Authorize(Roles = [Roles.Owner, Roles.Administrator])]
    public async Task<ResultOf> AddRoleToUser(
        AddRoleToUserRequest request, string callingUser,
        [Service] ITopicEventSender sender,
        [Service] IUserService service,
        [Service] UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var result = await service.AddRoleToUser(request, callingUser, cancellationToken);

        if (result.Data is null || !result.Success)
        {
            return ResultOf.Fail(result.Message ?? "Error adding role to user");
        }

        var roles = await userManager.GetRolesAsync(result.Data);
        var user = new UserAccountItem
        {
            UserId = result.Data.Id,
            Username = result.Data.UserName!,
            Email = result.Data.Email!,
            Roles = roles.ToList()
        };
        await sender.SendAsync(nameof(UserSubscriptions.UserUpdated), user, cancellationToken);
        return ResultOf.Pass();
    }

    [UseMutationConvention, Authorize(Roles = [Roles.Owner, Roles.Administrator])]
    public async Task<ResultOf> RemoveRoleFromUser(RemoveRoleFromUserRequest request, string callingUser,
        [Service] IUserService service, [Service] ITopicEventSender sender, UserManager<ApplicationUser> userManager, CancellationToken cancellationToken)
    {
        var result = await service.RemoveRoleFromUser(request, callingUser, cancellationToken);

        if (result.Data is null || !result.Success)
        {
            return ResultOf.Fail(result.Message ?? "Error removing role from user");
        }

        var roles = await userManager.GetRolesAsync(result.Data);
        var user = new UserAccountItem
        {
            UserId = result.Data.Id,
            Username = result.Data.UserName!,
            Email = result.Data.Email!,
            Roles = roles.ToList()
        };
        await sender.SendAsync(nameof(UserSubscriptions.UserUpdated), user, cancellationToken);
        return ResultOf.Pass();
    }

    [UseMutationConvention, Authorize(Roles = [Roles.Owner, Roles.Administrator])]
    public async Task<ResultOf> BanUser(BanUserRequest request, string callingUser, [Service] IUserService service, [Service] ITopicEventSender sender,
        [Service] UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var result = await service.BanUser(request, callingUser, cancellationToken);

        if (result.Data is null || !result.Success)
        {
            return ResultOf.Fail(result.Message ?? "Error banning user");
        }

        var roles = await userManager.GetRolesAsync(result.Data);
        var user = new UserAccountItem
        {
            UserId = result.Data.Id,
            Username = result.Data.UserName!,
            Email = result.Data.Email!,
            Roles = roles.ToList()
        };
        await sender.SendAsync(nameof(UserSubscriptions.UserBanned), user, cancellationToken);
        return ResultOf.Pass();
    }

    [UseMutationConvention, Authorize]
    public async Task<ResultOf<UserExperience>> AddUserExperience(AddUserExperienceRequest request, [Service] IProfileService service,
        CancellationToken cancellationToken)
        => await service.AddUserExperience(request, cancellationToken);

    [UseMutationConvention, Authorize]
    public async Task<ResultOf> EditUserExperience(AddUserExperienceRequest request, [Service] IProfileService service,
        CancellationToken cancellationToken)
        => await service.EditUserExperience(request, cancellationToken);

    [UseMutationConvention, Authorize]
    public async Task<ResultOf> RemoveUserExperience(RemoveUserExperienceRequest request,
        [Service] IProfileService service,
        CancellationToken cancellationToken)
        => await service.RemoveUserExperience(request, cancellationToken);

    [UseMutationConvention, Authorize]
    public async Task<ResultOf<UserGoal>> AddUserGoal(AddUserGoalRequest request, [Service] IProfileService service,
        CancellationToken cancellationToken)
        => await service.AddUserGoal(request, cancellationToken);

    [UseMutationConvention, Authorize]
    public async Task<ResultOf> EditUserGoal(EditUserGoalRequest request, [Service] IProfileService service,
        CancellationToken cancellationToken)
        => await service.EditUserGoal(request, cancellationToken);

    [UseMutationConvention, Authorize]
    public async Task<ResultOf> DeleteUserGoal(DeleteUserGoalRequest request, [Service] IProfileService serivce,
        CancellationToken cancellationToken)
        => await serivce.DeleteUserGoal(request, cancellationToken);

    [UseMutationConvention, Authorize]
    public async Task<ResultOf> EditProfileInfo(EditProfileInfoRequest request, [Service] IProfileService service,
        CancellationToken cancellationToken)
        => await service.EditProfileInfo(request, cancellationToken);
}