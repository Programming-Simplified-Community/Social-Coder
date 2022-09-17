using HotChocolate.AspNetCore.Authorization;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Server.GraphQL;

public partial class GraphQlMutations
{
    [UseMutationConvention, Authorize(Roles=new[]{Roles.Owner, Roles.Administrator})]
    public async Task<ResultOf> AddRoleToUser(AddRoleToUserRequest request, string callingUser,
        [Service] IUserService service, CancellationToken cancellationToken)
        => await service.AddRoleToUser(request, callingUser, cancellationToken);

    [UseMutationConvention, Authorize(Roles=new[]{Roles.Owner, Roles.Administrator})]
    public async Task<ResultOf> RemoveRoleFromUser(RemoveRoleFromUserRequest request, string callingUser,
        [Service] IUserService service, CancellationToken cancellationToken)
        => await service.RemoveRoleFromUser(request, callingUser, cancellationToken);

    [UseMutationConvention, Authorize(Roles=new[]{Roles.Owner, Roles.Administrator})]
    public async Task<ResultOf> BanUser(BanUserRequest request, string callingUser, [Service] IUserService service,
        CancellationToken cancellationToken)
        => await service.BanUser(request, callingUser, cancellationToken);
}