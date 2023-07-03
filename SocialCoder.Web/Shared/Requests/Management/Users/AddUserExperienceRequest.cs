using SocialCoder.Web.Shared.Enums;

namespace SocialCoder.Web.Shared.Requests.Management.Users;

public record AddUserExperienceRequest(int ExperiencePoolId, ExperienceLevel Level, string UserId);
public record RemoveUserExperienceRequest(int ExperiencePoolId, string UserId);

/// <summary>
/// Add a user-goal
/// </summary>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="GoalType"></param>
/// <param name="TargetDate"></param>
public record AddUserGoalRequest(string Title, string Description, GoalType GoalType, DateTime TargetDate, string UserId);

/// <summary>
/// All entries are optional. Non-nullable values shall be used when modifying the existing record in the database
/// </summary>
/// <param name="GoalId">Required field for designating which record to modify</param>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="GoalType"></param>
/// <param name="TargetDate"></param>
/// <param name="CompletedOn"></param>
/// <param name="UserId"></param>
public record EditUserGoalRequest(int GoalId, string? Title, string? Description, GoalType? GoalType, DateTime? TargetDate, DateTime? CompletedOn, string UserId);

public record DeleteUserGoalRequest(int GoalId, string UserId);