using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.GraphQL;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Server.Services.Implementations;

namespace SocialCoder.Web.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void SetupForAdmin(this IServiceCollection services, IConfiguration configuration)
    {

    }

    public static void SetupForProduction(this IServiceCollection services, IConfiguration configuration)
    {
        var dbHost = configuration.GetValue<string>("DB_HOST");
        var dbPassword = configuration.GetValue<string>("DB_PASSWORD");
        var dbName = configuration.GetValue<string>("DB_NAME");
        var dbUser = configuration.GetValue<string>("DB_USER");

        var connectionString =
            $"Server=${dbHost};port=5432;User Id=${dbUser};Password=${dbPassword};Database=${dbName}";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        }, ServiceLifetime.Transient);

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters =
                "1234567890-_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ";
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.LoginPath = "/login";
            options.LogoutPath = "/login";
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
        });

        var authBuilder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        if (configuration.GetValue<string>("Authentication:Google:ClientId") is not null)
        {
            authBuilder.AddGoogle(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            });
        }

        if (configuration.GetValue<string>("Authentication:Discord:ClientId") is not null)
        {
            authBuilder.AddDiscord(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = configuration["Authentication:Discord:ClientId"];
                options.ClientSecret = configuration["Authentication:Discord:ClientSecret"];
            });
        }

        if (configuration.GetValue<string>("Authentication:Github:ClientId") is not null)
        {
            authBuilder.AddGitHub(options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ClientId = configuration["Authentication:Github:ClientId"];
                options.ClientSecret = configuration["Authentication:Github:ClientSecret"];

                options.Scope.Add("public_repo");
                options.Scope.Add("read:org");
                options.Scope.Add("gist");
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.Scope.Add("user:follow");
                options.Scope.Add("read:project");
            });
        }

        authBuilder.AddCookie();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICodeJamService, CodeJamService>();
        services.AddScoped<IProfileService, ProfileService>();

        services.AddGraphQLServer()
            .AddQueryType<GraphQLQueries>()
            .AddTypeExtension<CodeJamTopicQueryExtensions>()
            .AddTypeExtension<UserManagementExtensions>()
            .AddMutationType<GraphQlMutations>()
            .AddProjections()
            .AddFiltering()
            .AddSorting();
    }

    public static void SetupForProduction(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGraphQL();
    }
}