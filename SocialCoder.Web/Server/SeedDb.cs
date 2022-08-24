using Microsoft.AspNetCore.Identity;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.Models;

namespace SocialCoder.Web.Server;

public static class SeedDb
{
    /// <summary>
    /// Inject test data into our database!
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        #if RELEASE
        return;
        #endif

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // If our users table has at least 1 user we'll skip this process
        if (context.Users.Any())
            return;

        var adminName = "admin";
        var adminEmail = "admin@test.com";

        var user = new ApplicationUser
        {
            UserName = adminName,
            NormalizedEmail = adminEmail.ToUpper(),
            NormalizedUserName = adminName.ToUpper(),
            Email = adminEmail
        };

        var result = await userManager.CreateAsync(user, "Testing@123");

        if (!result.Succeeded)
        {
            Console.Error.WriteLine(string.Join("\n", result.Errors.Select(x => x.Description)));
            Environment.Exit(1);
        }
        
        await context.SaveChangesAsync();

        var gold = new Badge
        {
            Title = "Gold",
            Description = "Highest marks!",
            Requirement = 100,
            BadgeType = BadgeType.Badge,
            ImagePath = "../img/badge/gold-b",
            RewardExperience = 50
        };

        var bronze = new Badge
        {
            Title = "Bronze",
            Description = "Good job!",
            Requirement = 60,
            BadgeType = BadgeType.Badge,
            ImagePath = "../img/badge/bronzec-b",
            RewardExperience = 30
        };

        var platinum = new Badge
        {
            Title = "Platinum",
            Description = "You're on fire!",
            Requirement = 150,
            BadgeType = BadgeType.Badge,
            ImagePath = "../img/badge/platinum-b",
            RewardExperience = 75
        };

        var silver = new Badge
        {
            Title = "Silver",
            Description = "Not too bad!",
            Requirement = 25,
            BadgeType = BadgeType.Badge,
            ImagePath = "../img/badge/silver-b",
            RewardExperience = 10,
        };

        context.Badges.AddRange(gold, bronze, platinum, silver);
        await context.SaveChangesAsync();
        
        context.BadgeRequirements.AddRange(new BadgeRequirement
            {
                BadgeId = bronze.Id,
                RequiredBadgeId = silver.Id
            },
            new()
            {
              BadgeId  = gold.Id,
              RequiredBadgeId = bronze.Id
            },
            new()
            {
                BadgeId = platinum.Id,
                RequiredBadgeId = gold.Id
            });
        
        await context.SaveChangesAsync();
        
        context.BadgeProgress.AddRange(new BadgeProgress
            {
                BadgeId = silver.Id,
                Progress = silver.Requirement,
                IsCompleted = true,
                UserId = user.Id
            },
            new()
            {
                BadgeId = bronze.Id,
                UserId = user.Id,
                Progress = 22
            });
        await context.SaveChangesAsync();
    }
}