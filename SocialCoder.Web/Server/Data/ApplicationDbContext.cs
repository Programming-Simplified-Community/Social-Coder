using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.Models;
using SocialCoder.Web.Shared.Models.CodeJam;

// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace SocialCoder.Web.Server.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder e)
        {
            base.OnModelCreating(e);

            e.Entity<Badge>()
                .HasMany(x => x.Requirements)
                .WithOne(x => x.Badge)
                .HasForeignKey(x=>x.BadgeId);

            e.Entity<CodeJamTopic>()
                .HasMany(x => x.CodeJamRegistrations)
                .WithOne(x => x.CodeJamTopic)
                .HasForeignKey(x => x.CodeJamTopicId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Badge> Badges { get; set; }
        public DbSet<BadgeProgress> BadgeProgress { get; set; }
        public DbSet<BadgeRequirement> BadgeRequirements { get; set; }
        
        #region Code Jam

        public DbSet<CodeJamTopic> CodeJamTopics { get; set; }
        public DbSet<CodeJamRegistration> CodeJamRegistrations { get; set; }

        #endregion
    }
}