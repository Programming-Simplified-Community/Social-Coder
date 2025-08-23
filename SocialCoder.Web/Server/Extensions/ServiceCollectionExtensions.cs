using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.GraphQL;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Server.Services.Implementations;

namespace SocialCoder.Web.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void SetupForAdmin(this IServiceCollection services, IConfiguration configuration)
    {

    }

    private static void SetupDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration.GetValue<string>("Postgres:Host"),
            Port = configuration.GetValue<int>("Postgres:Port"),
            Database = configuration.GetValue<string>("Postgres:Database"),
            Username = configuration.GetValue<string>("Postgres:UserId"),
            Password = configuration.GetValue<string>("Postgres:Password"),
            Timeout = 15,
            CommandTimeout = 15
        };

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionStringBuilder.ConnectionString);
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

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
    }

    public static void SetupForProduction(this IServiceCollection services, IConfiguration configuration)
    {
        services.SetupDatabase(configuration);

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

        if (configuration.GetValue<string>("OAuthSettings:Google:ClientId") is not null)
        {
            var clientId = configuration.GetValue<string>("OAuthSettings:Google:ClientId");
            var clientSecret = configuration.GetValue<string>("OAuthSettings:Google:ClientSecret");
            if (clientId is not null && clientSecret is not null)
            {
                authBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                });
            }
        }

        if (configuration.GetValue<string>("OAuthSettings:Discord:ClientId") is not null)
        {
            var clientId = configuration.GetValue<string>("OAuthSettings:Discord:ClientId");
            var clientSecret = configuration.GetValue<string>("OAuthSettings:Discord:ClientSecret");

            if (clientId is not null && clientSecret is not null)
            {
                authBuilder.AddDiscord(options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                });
            }
        }

        if (configuration.GetValue<string>("OAuthSettings:GitHub:ClientId") is not null)
        {
            var clientId = configuration.GetValue<string>("OAuthSettings:GitHub:ClientId");
            var clientSecret = configuration.GetValue<string>("OAuthSettings:GitHub:ClientSecret");

            if (clientId is not null && clientSecret is not null)
            {
                authBuilder.AddGitHub(options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;

                    options.Scope.Add("public_repo");
                    options.Scope.Add("read:org");
                    options.Scope.Add("gist");
                    options.Scope.Add("read:user");
                    options.Scope.Add("user:email");
                    options.Scope.Add("user:follow");
                    options.Scope.Add("read:project");
                });
            }
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