using Microsoft.AspNetCore.Identity;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Shared;
using SocialCoder.Web.Shared.Enums;
using SocialCoder.Web.Shared.Extensions;
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
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.ExperiencePools.Any())
        {
            var items = Enum.GetValues<ExperienceItem>();
            var succeeded = true;

            foreach (var item in items)
            {
                var asInt = (int)item;
                Console.WriteLine($"Trying to add {item} with id {asInt} for {item.GetDisplayName()}");
                try
                {
                    context.ExperiencePools.Add(new()
                    {
                        Name = item.GetDisplayName(),
                        Id = (int)item,
                        ImageUrl = item.GetIcon()
                    });
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    succeeded = false;
                    await Console.Error.WriteLineAsync($"An error occurred while adding {item}: {ex.Message}");
                }
            }

            if (succeeded)
            {
                throw new Exception("Error occurred when seeing database");
            }
        }

        // CREATE ROLES that our application requires to operate (at least base ones)
        if (!context.Roles.Any())
        {
            var roles = new[]
            {
                Roles.Owner,
                Roles.Administrator,
                Roles.EventCoordinator
            };

            List<string> errors = [];

            foreach (var roleName in roles)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    errors.Add(string.Join("\n", result.Errors.Select(x => x.Description)));
                }
            }

            if (errors.Any())
            {
                throw new Exception(string.Join("\n", errors));
            }
        }

        // At this point, anything after is for testing. So if we're in release mode -- don't add our test material
#if RELEASE
        return;
#endif

        // give us a couple starting topics to work with for testing
        if (!context.CodeJamTopics.Any())
        {
            Random random = new();
            for (var i = 0; i < 10; i++)
            {
                var registrationStart = DateTime.UtcNow.AddMonths(-random.Next(0, 3));
                var start = DateTime.UtcNow.AddDays(-random.Next(0, 31));
                var end = start.AddDays(random.Next(7, 30));

                context.CodeJamTopics.Add(new()
                {
                    Title = $"Topic {i}",
                    Description = "Just another code jam here",
                    JamStartDate = start,
                    JamEndDate = end,
                    RegistrationStartDate = registrationStart
                });
            }

            await context.SaveChangesAsync();
        }

        // GENERATE BADGES
        if (!context.Badges.Any())
        {
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
                    BadgeId = gold.Id,
                    RequiredBadgeId = bronze.Id
                },
                new()
                {
                    BadgeId = platinum.Id,
                    RequiredBadgeId = gold.Id
                });

            await context.SaveChangesAsync();
        }
    }
}