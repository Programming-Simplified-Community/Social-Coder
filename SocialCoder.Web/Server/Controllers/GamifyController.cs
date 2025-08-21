using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Attributes;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared.Models;
using SocialCoder.Web.Shared.ViewModels;

namespace SocialCoder.Web.Server.Controllers;

[Route("[controller]")]
[DisabledInSetupMode]
public class GamifyController : ControllerBase
{
    private readonly ILogger<GamifyController> _logger;
    private readonly ApplicationDbContext _context;
    
    public GamifyController(ILogger<GamifyController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Retrieve all badges in our database.
    /// </summary>
    /// <param name="cancellationToken">
    ///     Cancellation Token so if connection is interrupted between client and server we can
    ///     appropriately cleanup resources that are no longer needed
    /// </param>
    /// <returns>Collection of <see cref="Badge"/>s from database</returns>
    [HttpGet]
    public async Task<IEnumerable<Badge>> Badges(CancellationToken cancellationToken)
    {
        return await _context.Badges
            .Include(x => x.Requirements)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieve all badges obtained by user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{userId}/badges")]
    public async Task<IEnumerable<UserBadgeViewModel>> GetUserBadges([FromRoute] string userId, CancellationToken cancellationToken)
    {
        var results = await _context.BadgeProgress
            .Join(_context.Badges,                                          // join on badges
                progress => progress.BadgeId,                   // where progress.BadgeId equals badge.Id
                badge => badge.Id,
                (progress, badge) => new UserBadgeViewModel       // translate the two records into our view model
                {
                    Id = badge.Id,
                    Title = badge.Title,
                    Description = badge.Description,
                    Requirement = badge.Requirement,
                    Progress = progress.Progress,
                    ImagePath = badge.ImagePath,
                    Type = badge.BadgeType,
                })
            .Where(x=>x.Type == BadgeType.Badge)
            .ToListAsync(cancellationToken);

        return results;
    }
    
    /// <summary>
    /// Similar to <see cref="GetUserBadges"/> except for <see cref="BadgeType.Quests"/>
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{userId}/quests")]
    public async Task<IEnumerable<UserBadgeViewModel>> GetUserQuests([FromRoute] string userId,
        CancellationToken cancellationToken)
    {
        var results = await _context.BadgeProgress
            .Join(_context.Badges,                                          // join on badges
                progress => progress.BadgeId,                   // where progress.BadgeId equals badge.Id
                badge => badge.Id,
                (progress, badge) => new UserBadgeViewModel       // translate the two records into our view model
                {
                    Id = badge.Id,
                    Title = badge.Title,
                    Description = badge.Description,
                    Requirement = badge.Requirement,
                    Progress = progress.Progress,
                    ImagePath = badge.ImagePath,
                    Type = badge.BadgeType,
                })
            .Where(x=>x.Type == BadgeType.Quest)
            .ToListAsync(cancellationToken);

        return results;
    }
}