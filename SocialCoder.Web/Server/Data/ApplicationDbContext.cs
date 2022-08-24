using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.Models;

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
        }

        public DbSet<Badge> Badges { get; set; }
        public DbSet<BadgeProgress> BadgeProgress { get; set; }
        public DbSet<BadgeRequirement> BadgeRequirements { get; set; }
    }
}