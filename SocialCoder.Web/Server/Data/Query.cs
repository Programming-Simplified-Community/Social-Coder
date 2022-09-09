using HotChocolate.AspNetCore.Authorization;
using SocialCoder.Web.Shared.Models.CodeJam;
using SocialCoder.Web.Shared.ViewModels.CodeJam;

namespace SocialCoder.Web.Server.Data;

public class Query
{
    [UseProjection, UseSorting, UseFiltering, Authorize]
    public IQueryable<CodeJamTopic> GetTopics([Service] ApplicationDbContext context)
        => context.CodeJamTopics;

    [UseProjection, UseSorting, UseFiltering, Authorize]
    public IQueryable<CodeJamViewModel> GetTopicCards([Service] ApplicationDbContext context, string? userId)
        => (from topic in context.CodeJamTopics
            
            let isRegistered = (from reg in context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && reg.UserId == userId && reg.AbandonedOn == null
                    select reg).Any()
            
            let soloApplicants = (from reg in context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && !reg.PreferTeam && reg.AbandonedOn == null
                        select reg).Count()
            let total = (from reg in context.CodeJamRegistrations
                    where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null select reg).Count()
            
            select new CodeJamViewModel
            {
                Topic = topic,
                TotalSoloApplicants = soloApplicants,
                TotalTeamApplicants = total - soloApplicants,
                IsRegistered = isRegistered
            }).AsQueryable();

    public IQueryable<CodeJamAdminViewModel> GetAdminTopicCards([Service] ApplicationDbContext context)
        => (from topic in context.CodeJamTopics
            let soloApps = (from reg in context.CodeJamRegistrations
                where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null && !reg.PreferTeam
                select reg).Count()
            let totalApps = (from reg in context.CodeJamRegistrations
                where reg.CodeJamTopicId == topic.Id && reg.AbandonedOn == null
                select reg).Count()
            select new CodeJamAdminViewModel
            {
                Topic = topic,
                TotalSoloApplicants = soloApps,
                TotalTeamApplicants = totalApps - soloApps
            }).AsQueryable();
}