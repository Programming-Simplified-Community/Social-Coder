using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.Account;
using SocialCoder.Web.Shared.Requests.Management.Users;

namespace SocialCoder.Web.Server.Services.Implementations;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(ApplicationDbContext context, ILogger<ProfileService> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public async Task<ResultOf<UserExperience>> AddUserExperience(AddUserExperienceRequest request, CancellationToken cancellationToken = default)
    {
        // does this user already have an experience item in the database of this item?
        var existing = await this._context.UserExperiences.FirstOrDefaultAsync(
            x => x.UserId == request.UserId && x.ExperiencePoolId == request.ExperiencePoolId, cancellationToken);

        if (existing is not null)
        {
            return ResultOf<UserExperience>.Fail("Already have an entry for that");
        }

        var entry = new UserExperience
        {
            Level = request.Level,
            UserId = request.UserId,
            ExperiencePoolId = request.ExperiencePoolId
        };

        this._context.UserExperiences.Add(entry);
        await this._context.SaveChangesAsync(cancellationToken);

        return ResultOf<UserExperience>.Pass(entry);
    }

    public async Task<ResultOf> EditUserExperience(AddUserExperienceRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await this._context.UserExperiences.FirstOrDefaultAsync(
            x => x.UserId == request.UserId && x.ExperiencePoolId == request.ExperiencePoolId, cancellationToken);

        if (existing is null)
        {
            return ResultOf.Fail("Not Found");
        }

        existing.Level = request.Level;
        this._context.UserExperiences.Update(existing);
        await this._context.SaveChangesAsync(cancellationToken);

        return ResultOf.Pass();
    }

    public async Task<ResultOf> RemoveUserExperience(RemoveUserExperienceRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await this._context.UserExperiences.FirstOrDefaultAsync(
            x => x.UserId == request.UserId && x.ExperiencePoolId == request.ExperiencePoolId, cancellationToken);

        if (existing is null)
        {
            return ResultOf.Fail("Not Found");
        }

        this._context.UserExperiences.Remove(existing);
        await this._context.SaveChangesAsync(cancellationToken);
        return ResultOf.Pass();
    }

    public async Task<ResultOf<UserGoal>> AddUserGoal(AddUserGoalRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new UserGoal
        {
            Title = request.Title,
            Description = request.Description,
            UserId = request.UserId,
            GoalType = request.GoalType,
            TargetDate = request.TargetDate
        };

        this._context.UserGoals.Add(entry);
        await this._context.SaveChangesAsync(cancellationToken);

        return ResultOf<UserGoal>.Pass(entry);
    }

    public async Task<ResultOf> EditUserGoal(EditUserGoalRequest request, CancellationToken cancellationToken = default)
    {
        var existing =
            await this._context.UserGoals.FirstOrDefaultAsync(x => x.Id == request.GoalId && request.UserId == x.UserId,
                cancellationToken);

        if (existing is null)
        {
            return ResultOf.Fail("Not Found");
        }

        var modified = false;
        if (!string.IsNullOrEmpty(request.Title))
        {
            existing.Title = request.Title;
            modified = true;
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            existing.Description = request.Description;
            modified = true;
        }

        if (request.GoalType is not null)
        {
            existing.GoalType = request.GoalType.Value;
            modified = true;
        }

        if (request.CompletedOn is not null)
        {
            existing.CompletedOn = request.CompletedOn.Value;
            modified = true;
        }

        if (request.TargetDate is not null)
        {
            existing.TargetDate = request.TargetDate.Value;
            modified = true;
        }

        if (modified)
        {
            await this._context.SaveChangesAsync(cancellationToken);
        }

        return ResultOf.Pass();
    }

    public async Task<ResultOf> DeleteUserGoal(DeleteUserGoalRequest request, CancellationToken cancellationToken = default)
    {
        var existing =
            await this._context.UserGoals.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Id == request.GoalId,
                cancellationToken);

        if (existing is null)
        {
            return ResultOf.Fail("Not Found");
        }

        this._context.UserGoals.Remove(existing);
        await this._context.SaveChangesAsync(cancellationToken);

        return ResultOf.Pass();
    }

    public async Task<ResultOf> EditProfileInfo(EditProfileInfoRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await this._context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (existing is null)
        {
            return ResultOf.Fail("Not Found");
        }

        var changed = false;

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            existing.Country = request.Country;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            existing.Language = request.Language;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            changed = true;
            existing.DisplayName = request.DisplayName;
        }

        if (changed)
        {
            await this._context.SaveChangesAsync(cancellationToken);
        }

        return ResultOf.Pass();
    }
}