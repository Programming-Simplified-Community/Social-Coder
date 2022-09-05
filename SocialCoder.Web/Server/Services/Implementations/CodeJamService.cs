using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared.Extensions;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.Requests;
using SocialCoder.Web.Shared.Services;

namespace SocialCoder.Web.Server.Services.Implementations;

public class CodeJamService : ICodeJamService
{
    private readonly ApplicationDbContext _context;

    public CodeJamService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<CodeJamTopic>> GetAllTopics(PaginationRequest? request,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamTopic> items;
        
        if (request is null || request.PageSize <= 0)
        {
            items = await _context.CodeJamTopics.ToListAsync(cancellationToken);
            return new PaginatedResponse<CodeJamTopic>()
            {
                PageSize = items.Count,
                PageNumber = 0,
                Items = items,
                IsDescending = false,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }

        items = await _context.CodeJamTopics.PaginatedQuery(request, x => x.JamStartDate)
            .ToListAsync(cancellationToken);
        
        var totalCount = await _context.CodeJamTopics.CountAsync(cancellationToken);
        return new PaginatedResponse<CodeJamTopic>
        {
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
            Items = items,
            IsDescending = request.IsDescending,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            TotalRecords = totalCount
        };
    }


    public async Task<PaginatedResponse<CodeJamTopic>> GetActiveTopics(SpecificDateQuery? request,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamTopic> items;
        if (request is null || request.PageSize <= 0)
        {
            var d = DateTime.UtcNow;
            items = await _context.CodeJamTopics.Where(x => d >= x.JamStartDate && d <= x.JamEndDate)
                .ToListAsync(cancellationToken);
            return new PaginatedResponse<CodeJamTopic>
            {
                Items = items,
                IsDescending = false,
                PageNumber = 0,
                PageSize = items.Count,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }

        items = await _context.CodeJamTopics
            .Where(x=>request.Date >= x.JamStartDate && request.Date <= x.JamEndDate)
            .PaginatedQuery(request, x => x.JamStartDate)
            .ToListAsync(cancellationToken);
        
        var total = await _context.CodeJamTopics
            .CountAsync(x => request.Date >= x.JamStartDate && request.Date <= x.JamEndDate, cancellationToken);

        return new PaginatedResponse<CodeJamTopic>
        {
            Items = items,
            IsDescending = request.IsDescending,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)total / request.PageSize),
            TotalRecords = total
        };
    }

    public async Task<PaginatedResponse<CodeJamTopic>> GetRegisterableTopics(SpecificDateQuery? request,
        CancellationToken cancellationToken = default)
    {
        List<CodeJamTopic> items;

        if (request is null)
        {
            var d = DateTime.UtcNow;
            items = await _context.CodeJamTopics
                .Where(x=>d >= x.RegistrationStartDate && d <= x.JamStartDate)
                .ToListAsync(cancellationToken);
            return new PaginatedResponse<CodeJamTopic>
            {
                Items = items,
                IsDescending = false,
                PageNumber = 0,
                PageSize = items.Count,
                TotalPages = 1,
                TotalRecords = items.Count
            };
        }

        items = await _context.CodeJamTopics
            .Where(x => request.Date >= x.RegistrationStartDate && request.Date <= x.JamStartDate)
            .PaginatedQuery(request, x => x.RegistrationStartDate)
            .ToListAsync(cancellationToken);

        var total = await _context.CodeJamTopics
            .CountAsync(x => request.Date >= x.RegistrationStartDate && request.Date <= x.JamStartDate, cancellationToken);

        return new PaginatedResponse<CodeJamTopic>
        {
            Items = items,
            IsDescending = request.IsDescending,
            PageSize = request.PageSize,
            PageNumber = request.PageNumber,
            TotalPages = (int)Math.Ceiling((double)total / request.PageSize),
            TotalRecords = total
        };
    }
}