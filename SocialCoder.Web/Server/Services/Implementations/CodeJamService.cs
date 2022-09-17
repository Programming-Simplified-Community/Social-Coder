using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.Services.Implementations;

public class CodeJamService : ICodeJamService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CodeJamService> _logger;
    
    public CodeJamService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<CodeJamService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }
    
    #region Administrative

    public async Task<ResultOf<CodeJamTopic>> AdminCreateTopic(CodeJamTopic topic,
        CancellationToken cancellationToken = default)
    {
        if (topic.Id > 0)
            return ResultOf<CodeJamTopic>.Fail("This topic already exists");
        topic.RegistrationStartDate = topic.RegistrationStartDate.ToUniversalTime();
        topic.JamStartDate = topic.JamStartDate.ToUniversalTime();
        topic.JamEndDate = topic.JamEndDate.ToUniversalTime();
        _context.CodeJamTopics.Add(topic);
        await _context.SaveChangesAsync(cancellationToken);

        return ResultOf<CodeJamTopic>.Pass(topic);
    }

    public async Task<ResultOf> Delete(int topicId, CancellationToken cancellationToken = default)
    {
        var topic = await _context.CodeJamTopics
            .FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);

        if (topic is null)
            return ResultOf.Fail("Invalid Topic");
        
        _context.CodeJamTopics.Remove(topic);
        await _context.SaveChangesAsync(cancellationToken);
        
        return ResultOf.Pass();
    }

    public async Task<ResultOf<CodeJamTopic>> AdminUpdateTopic(CodeJamTopic topic, CancellationToken cancellationToken = default)
    {
        // Just to validate the item does actually exist
        var item = await _context.CodeJamTopics.FirstOrDefaultAsync(x => x.Id == topic.Id, cancellationToken);

        if (item is null)
            return ResultOf<CodeJamTopic>.Fail("Invalid Topic");

        item.Title = topic.Title;
        item.Description = topic.Description;
        item.JamStartDate = topic.JamStartDate.ToUniversalTime();
        item.JamEndDate = topic.JamEndDate.ToUniversalTime();
        item.RegistrationStartDate = topic.RegistrationStartDate.ToUniversalTime();
        item.BackgroundImageUrl = topic.BackgroundImageUrl;
        item.IsActive = topic.IsActive;

        _context.CodeJamTopics.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return ResultOf<CodeJamTopic>.Pass(item);
    }
    #endregion

    public async Task<ResultOf<CodeJamViewModel>> Register(CodeJamRegistrationRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return ResultOf<CodeJamViewModel>.Fail("Invalid Request");

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return ResultOf<CodeJamViewModel>.Fail("Invalid Request");

        var check = await (from topic in _context.CodeJamTopics
            where topic.Id == request.TopicId && topic.IsActive

            let reg = (from reg in _context.CodeJamRegistrations
                where reg.UserId == userId && reg.CodeJamTopicId == request.TopicId
                select reg).Any()

            select new
            {
                Topic = topic,
                IsRegistered = reg
            }).FirstOrDefaultAsync(cancellationToken);

        if (check is null) return ResultOf<CodeJamViewModel>.Fail("Invalid Request");
        if (check.IsRegistered) return ResultOf<CodeJamViewModel>.Fail("Already registered");

        _context.CodeJamRegistrations.Add(new()
        {
            UserId = userId,
            CodeJamTopicId = request.TopicId,
            PreferTeam = request.PreferTeam
        });

        await _context.SaveChangesAsync(cancellationToken);

        return await GetTopic(request.TopicId, userId, cancellationToken);
    }

    public async Task<ResultOf<CodeJamViewModel>> Abandon(CodeJamAbandonRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return ResultOf<CodeJamViewModel>.Fail("Invalid Request");

        var user = await _userManager.FindByIdAsync(userId);
        
        // Must be a valid user
        if (user is null)
            return ResultOf<CodeJamViewModel>.Fail("Invalid Request");

        var resultSet = await (from reg in _context.CodeJamRegistrations
            join topic in _context.CodeJamTopics
                on reg.CodeJamTopicId equals topic.Id
            where topic.Id == request.TopicId && reg.UserId == userId
            select new
            {
                Registration = reg,
                Topic = topic
            }).FirstOrDefaultAsync(cancellationToken);

        if (resultSet is null)
            return ResultOf<CodeJamViewModel>.Fail("You weren't registered for this topic");

        // If a user abandons a code jam PRIOR to it actually starting we will hard delete the record
        // Allowing the user to re-register if they choose.
        if (DateTime.UtcNow <= resultSet.Topic.JamStartDate)
        {
            _context.CodeJamRegistrations.Remove(resultSet.Registration);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetTopic(request.TopicId, userId, cancellationToken);
        }
        
        resultSet.Registration.AbandonedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetTopic(request.TopicId, userId, cancellationToken);
    }

    public async Task<ResultOf<CodeJamViewModel>> GetTopic(int topicId, string? userId,
        CancellationToken cancellationToken = default)
    {
        var item = await (from topic in _context.CodeJamTopics
                                            where topic.Id == topicId
            
            let isRegistered = (from reg in _context.CodeJamRegistrations
                where reg.CodeJamTopicId == topicId && reg.UserId == userId && reg.AbandonedOn == null
                select reg).Any()
            let soloApps = (from reg in _context.CodeJamRegistrations
                where reg.CodeJamTopicId == topicId && !reg.PreferTeam && reg.AbandonedOn == null
                select reg).Count()

            let total = (from reg in _context.CodeJamRegistrations
                where reg.CodeJamTopicId == topicId && reg.AbandonedOn == null
                select reg).Count()
            
            select new CodeJamViewModel
            {
                Topic = topic,
                IsRegistered = isRegistered,
                TotalSoloApplicants = soloApps,
                TotalTeamApplicants = total - soloApps
            }).FirstOrDefaultAsync(cancellationToken);

        return item is not null
            ? ResultOf<CodeJamViewModel>.Pass(item)
            : ResultOf<CodeJamViewModel>.Fail("Item not found");
    }
}