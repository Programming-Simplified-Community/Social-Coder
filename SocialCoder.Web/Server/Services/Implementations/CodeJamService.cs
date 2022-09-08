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
    
    #region Administrative

    public async Task<ResultOf> Delete(int topicId, CancellationToken cancellationToken = default)
    {
        var topic = await _context.CodeJamTopics.FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);

        if (topic is null)
            return ResultOf.Fail("Invalid Topic");

        _context.CodeJamTopics.Remove(topic);
        await _context.SaveChangesAsync(cancellationToken);
        
        return ResultOf.Pass();
    }

    public async Task<PaginatedResponse<CodeJamAdminViewModel>> AdminGetTopics(PaginationRequest? request,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamAdminViewModel> items;

        if (request is null || request.PageSize <= 0)
        {
            items = await (from topic in _context.CodeJamTopics
                let soloApps = (from reg in _context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id
                          && reg.AbandonedOn == null && !reg.PreferTeam
                    select reg).Count()
                let totalApps = (from reg in _context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id &&
                          reg.AbandonedOn == null
                    select reg).Count()

                select new CodeJamAdminViewModel
                {
                    Topic = topic,
                    TotalSoloApplicants = soloApps,
                    TotalTeamApplicants = totalApps - soloApps
                }).ToListAsync(cancellationToken);
            return new PaginatedResponse<CodeJamAdminViewModel>
            {
                Items = items,
                IsDescending = false,
                PageNumber = 1,
                PageSize = items.Count,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }

        var baseQuery = request.IsDescending
            ? _context.CodeJamTopics.OrderByDescending(x => x.JamStartDate)
            : _context.CodeJamTopics.OrderBy(x => x.JamStartDate);

        items = await (from topic in baseQuery
            let soloApps = (from reg in _context.CodeJamRegistrations
                where reg.CodeJamTopicId == topic.Id
                      && reg.AbandonedOn == null && !reg.PreferTeam
                select reg).Count()
            let total = (from reg in _context.CodeJamRegistrations
                where reg.CodeJamTopicId == topic.Id
                      && reg.AbandonedOn == null
                select reg).Count()
            select new CodeJamAdminViewModel
            {
                Topic = topic,
                TotalSoloApplicants = soloApps,
                TotalTeamApplicants = total - soloApps
            })
            .Skip((request.PageNumber-1)*request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var grandTotal = await _context.CodeJamTopics.CountAsync(cancellationToken);
        return new PaginatedResponse<CodeJamAdminViewModel>
        {
            Items = items,
            IsDescending = request.IsDescending,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)grandTotal / request.PageSize),
            TotalRecords = grandTotal
        };
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
                PageNumber = 1,
                Items = items,
                IsDescending = false,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }

        var baseQuery = request.IsDescending
            ? _context.CodeJamTopics.OrderByDescending(x => x.JamStartDate)
            : _context.CodeJamTopics.OrderBy(x => x.JamStartDate)
                .AsQueryable();

        items = await (from topic in baseQuery
                let isRegistered = (from reg in _context.CodeJamRegistrations
                    where reg.UserId == userId && reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                    select reg).Any()
                let soloApps = (from reg in _context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && !reg.PreferTeam && reg.AbandonedOn == null
                    select reg).Count()
                let total = (from reg in _context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                    select reg).Count()
                select new CodeJamViewModel
                {
                    Topic = topic,
                    IsRegistered = isRegistered,
                    TotalSoloApplicants = soloApps,
                    TotalTeamApplicants = total - soloApps
                })
            .Skip((request.PageNumber-1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var totalCount = await _context.CodeJamTopics.CountAsync(cancellationToken);
        return new PaginatedResponse<CodeJamViewModel>
        {
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
            Items = items,
            IsDescending = request.IsDescending,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            TotalRecords = totalCount
        };
    }


    public async Task<PaginatedResponse<CodeJamViewModel>> GetActiveTopics(SpecificDateQuery? request, string? userId,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamViewModel> items;

        var baseQuery = 
            from topic in _context.CodeJamTopics
            where request.Date >= topic.JamStartDate && request.Date <= topic.JamEndDate
            select topic;
        
        // Default query when request is null
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
                PageNumber = 1,
                PageSize = items.Count,
                TotalPages = 1,
                TotalRecords = items.Count // because this is the entire result set
            };
        }

        baseQuery = request.IsDescending 
            ? baseQuery.OrderByDescending(x => x.JamStartDate) 
            : baseQuery.OrderBy(x => x.JamStartDate);
        
        items = await (
            from topic in baseQuery
            let isRegistered = (from reg in _context.CodeJamRegistrations
                where reg.UserId == userId && reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                select reg).Any()

            let soloApps = (from reg in _context.CodeJamRegistrations
                where reg.AbandonedOn == null && reg.CodeJamTopicId == topic.Id && !reg.PreferTeam
                select reg).Count()
            let total = (from topic in baseQuery select topic).Count()
            select new CodeJamViewModel
            {
                Topic = topic,
                IsRegistered = isRegistered,
                TotalSoloApplicants = soloApps,
                TotalTeamApplicants = total - soloApps
            })
            .Skip((request.PageNumber-1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        // We need to ensure our count is based on the non-paginated version
        var totalCount = await _context.CodeJamTopics
            .CountAsync(x=>request.Date >= x.JamStartDate && 
                           request.Date <= x.JamEndDate, 
                cancellationToken);
        return new PaginatedResponse<CodeJamViewModel>
        {
            Items = items,
            IsDescending = request.IsDescending,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)1 / request.PageSize),
            TotalRecords = totalCount
        };
    }

    public async Task<PaginatedResponse<CodeJamViewModel>> GetRegisterableTopics(SpecificDateQuery? request, string? userId,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamViewModel> items;

        var baseQuery = (from topic in _context.CodeJamTopics
            where request.Date >= topic.RegistrationStartDate && request.Date <= topic.JamStartDate
            select topic);
        
        if (request is null)
        {
            items = await (
                from topic in baseQuery
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
                PageNumber = 1,
                PageSize = items.Count,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }

        baseQuery = request.IsDescending
            ? baseQuery.OrderByDescending(x => x.RegistrationStartDate)
            : baseQuery.OrderBy(x => x.RegistrationStartDate);

        items = await (from topic in baseQuery

            let isRegistered = (from reg in _context.CodeJamRegistrations
                where reg.UserId == userId && reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                select reg).Any()
            let soloApps = (from reg in _context.CodeJamRegistrations
                where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null && !reg.PreferTeam
                select reg).Count()
            let total = (from reg in _context.CodeJamRegistrations
                where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                select reg).Count()
            select new CodeJamViewModel
            {
                Topic = topic,
                IsRegistered = isRegistered,
                TotalSoloApplicants = soloApps,
                TotalTeamApplicants = total - soloApps
            })
            .Skip((request.PageNumber-1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Needs to be the total count of the non-paginated result set
        var totalCount = await _context.CodeJamTopics.CountAsync(
            x => request.Date >= x.RegistrationStartDate && 
                 request.Date <= x.JamStartDate, 
            cancellationToken);
        return new PaginatedResponse<CodeJamViewModel>
        {
            Items = items,
            IsDescending = request.IsDescending,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            TotalRecords = totalCount
        };
    }
}