using HotChocolate.Authorization;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.Account;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Server.GraphQL;

public partial class GraphQlMutations
{
    [UseMutationConvention, Authorize(Roles= [Roles.Owner, Roles.Administrator])]
    public async Task<ResultOf> AddRoleToUser(AddRoleToUserRequest request, string callingUser,
        [Service] IUserService service, CancellationToken cancellationToken)
        => await service.AddRoleToUser(request, callingUser, cancellationToken);

    [UseMutationConvention, Authorize(Roles= [Roles.Owner, Roles.Administrator])]
    public async Task<ResultOf> RemoveRoleFromUser(RemoveRoleFromUserRequest request, string callingUser,
        [Service] IUserService service, CancellationToken cancellationToken)
        => await service.RemoveRoleFromUser(request, callingUser, cancellationToken);

    [UseMutationConvention, Authorize(Roles= [Roles.Owner, Roles.Administrator])]
    public async Task<ResultOf> BanUser(BanUserRequest request, string callingUser, [Service] IUserService service,
        CancellationToken cancellationToken)
        => await service.BanUser(request, callingUser, cancellationToken);

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