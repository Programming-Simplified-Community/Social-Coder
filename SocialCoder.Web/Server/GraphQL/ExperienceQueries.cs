using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared.Models;
using SocialCoder.Web.Shared.Models.Account;
using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Server.GraphQL;

public partial class GraphQLQueries
{
    [UseProjection, Authorize]
    public IOrderedQueryable<ExperiencePool> GetExperiencePool([Service] ApplicationDbContext context)
        => context.ExperiencePools.OrderBy(x => x.Name);

    [UseProjection, Authorize]
    public IQueryable<UserExperienceViewModel> GetUserExperience(string userId, [Service] ApplicationDbContext context)
        => from experience in context.UserExperiences
           join item in context.ExperiencePools
               on experience.ExperiencePoolId equals item.Id
           where experience.UserId == userId
           orderby item.Name
           select new UserExperienceViewModel
           {
               Experience = experience.Level,
               Name = item.Name,
               ImageUrl = item.ImageUrl,
               UserId = userId,
               ExperiencePoolId = item.Id
           };

    [UseProjection, Authorize]
    public async Task<List<UserGoal>> GetGoals(string userId, [Service] ApplicationDbContext context,
        CancellationToken cancellationToken)
        => await context.UserGoals.Where(x => x.UserId == userId).ToListAsync(cancellationToken);

    [UseProjection, Authorize]
    public async Task<MyProfileInfo?> GetMyInfo(string userId, [Service] ApplicationDbContext context, CancellationToken cancellationToken)
        => await context.Users.Where(x => x.Id == userId)
            .Select(x => new MyProfileInfo
            {
                Country = x.Country,
                Language = x.Language,
                DisplayName = x.DisplayName,
                Username = x.UserName,
                Email = x.Email
            }).FirstOrDefaultAsync(cancellationToken);
}