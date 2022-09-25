using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.Account;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Server.Services.Contracts;

public interface IProfileService
{
    /// <summary>
    /// Add experience to a given user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<UserExperience>> AddUserExperience(AddUserExperienceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modify an existing experience item for a given user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf> EditUserExperience(AddUserExperienceRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove experience from a user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf> RemoveUserExperience(RemoveUserExperienceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a user goal
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf<UserGoal>> AddUserGoal(AddUserGoalRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edit a user goal
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf> EditUserGoal(EditUserGoalRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a goal from user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf> DeleteUserGoal(DeleteUserGoalRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Edit profile info
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResultOf> EditProfileInfo(EditProfileInfoRequest request, CancellationToken cancellationToken = default);
}