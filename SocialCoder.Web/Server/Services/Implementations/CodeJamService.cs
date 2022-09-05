using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Requests.CodeJam;
using SocialCoder.Web.Shared.Services;
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

    public async Task<ResultOf<CodeJamViewModel>> Register(CodeJamRegistrationRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return ResultOf<CodeJamViewModel>.Fail("Invalid Request");

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return ResultOf<CodeJamViewModel>.Fail("Invalid Request");

        // TODO: Can we combine these into 1 database call
        // Must be a valid topic AND active
        if(!await _context.CodeJamTopics.AnyAsync(x=>x.Id == request.TopicId && x.IsActive, cancellationToken))
            return ResultOf<CodeJamViewModel>.Fail("Invalid Request");
        
        // Cannot have registered already
        if (await _context.CodeJamRegistrations.AnyAsync(x => x.UserId == userId && x.CodeJamTopicId == request.TopicId,
                cancellationToken))
            return ResultOf<CodeJamViewModel>.Fail("Already registered");
        
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
        
        // TODO: Can we combine these into 1 database call
        // Must have an existing registration for the selected topic
        var registration =
            await _context.CodeJamRegistrations.FirstOrDefaultAsync(
                x => x.CodeJamTopicId == request.TopicId && x.UserId == userId, cancellationToken);

        if (registration is null)
            return ResultOf<CodeJamViewModel>.Fail("You weren't registered for that!");

        registration.AbandonedOn = DateTime.UtcNow;
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

    public async Task<PaginatedResponse<CodeJamViewModel>> GetAllTopics(PaginationRequest? request,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamViewModel> items;
        
        if (request is null || request.PageSize <= 0)
        {
            items = await (from topic in _context.CodeJamTopics

                let isRegistered = (from reg in _context.CodeJamRegistrations
                    where reg.UserId == userId && reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                    select reg).Any()

                let soloApps = (from reg in _context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && !reg.PreferTeam && reg.AbandonedOn == null
                    select reg).Count()

                let total = (from reg in _context.CodeJamRegistrations where reg.CodeJamTopicId == topic.Id  && reg.AbandonedOn == null select reg)
                    .Count()
                    
                select new CodeJamViewModel
                {
                    Topic = topic,
                    IsRegistered = isRegistered,
                    TotalSoloApplicants = soloApps,
                    TotalTeamApplicants = total - soloApps
                }).ToListAsync(cancellationToken);
                        
            return new PaginatedResponse<CodeJamViewModel>()
            {
                PageSize = items.Count,
                PageNumber = 0,
                Items = items,
                IsDescending = false,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }
        
        var totalCount = await _context.CodeJamTopics.CountAsync(cancellationToken);
        return new PaginatedResponse<CodeJamViewModel>
        {
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
            Items = new List<CodeJamViewModel>(),
            IsDescending = request.IsDescending,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            TotalRecords = totalCount
        };
    }


    public async Task<PaginatedResponse<CodeJamViewModel>> GetActiveTopics(SpecificDateQuery? request, string? userId,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamViewModel> items;
        if (request is null || request.PageSize <= 0)
        {
            items = await (
                from topic in _context.CodeJamTopics
                where request.Date >= topic.JamStartDate && request.Date <= topic.JamEndDate
                
                let isRegistered = (from reg in _context.CodeJamRegistrations
                    where reg.UserId == userId && reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                    select reg).Any()

                let soloApps = (from reg in _context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && !reg.PreferTeam && reg.AbandonedOn == null
                    select reg).Count()

                let total = (from reg in _context.CodeJamRegistrations where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null select reg)
                    .Count()
                    
                select new CodeJamViewModel
                {
                    Topic = topic,
                    IsRegistered = isRegistered,
                    TotalSoloApplicants = soloApps,
                    TotalTeamApplicants = total - soloApps
                }).ToListAsync(cancellationToken);
            
            return new PaginatedResponse<CodeJamViewModel>
            {
                Items = items,
                IsDescending = false,
                PageNumber = 0,
                PageSize = items.Count,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }
        
        return new PaginatedResponse<CodeJamViewModel>
        {
            Items = new List<CodeJamViewModel>(),
            IsDescending = request.IsDescending,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)1 / request.PageSize),
            TotalRecords = 1
        };
    }

    public async Task<PaginatedResponse<CodeJamViewModel>> GetRegisterableTopics(SpecificDateQuery? request, string? userId,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamViewModel> items;

        if (request is null)
        {
            var d = DateTime.UtcNow;
            items = await (
                from topic in _context.CodeJamTopics
                where request.Date >= topic.JamStartDate && request.Date <= topic.JamEndDate
                
                let isRegistered = (from reg in _context.CodeJamRegistrations
                    where reg.UserId == userId && reg.CodeJamTopicId == topic.Id
                    select reg).Any()

                let soloApps = (from reg in _context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && !reg.PreferTeam  && reg.AbandonedOn == null
                    select reg).Count()

                let total = (from reg in _context.CodeJamRegistrations where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null select reg)
                    .Count()
                    
                select new CodeJamViewModel
                {
                    Topic = topic,
                    IsRegistered = isRegistered,
                    TotalSoloApplicants = soloApps,
                    TotalTeamApplicants = total - soloApps
                }).ToListAsync(cancellationToken);
            return new PaginatedResponse<CodeJamViewModel>
            {
                Items = items,
                IsDescending = false,
                PageNumber = 0,
                PageSize = items.Count,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }

        return new PaginatedResponse<CodeJamViewModel>
        {
            Items = new List<CodeJamViewModel>(),
            IsDescending = request.IsDescending,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
            TotalPages = (int)Math.Ceiling((double)1 / request.PageSize),
            TotalRecords = 1
        };
    }
}