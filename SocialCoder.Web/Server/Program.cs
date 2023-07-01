using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.GraphQL;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Server.Services.Implementations;
using SocialCoder.Web.Shared.Extensions;
using GraphQLQueries = SocialCoder.Web.Server.GraphQL.GraphQLQueries;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonConfigurationFiles();
builder.Configuration.AddConnections();

// Add services to the container.
var connectionString = builder.Configuration["DefaultConnection"];

/*
 *  Having the database as transient prevents multiple queries happening on the
 *  same DbContext -- which throws an exception.
 *
 *  Especially when using GraphQL pulling from multiple endpoints.
 */

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
}, ServiceLifetime.Transient);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "1234567890-_ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ";
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddGoogle(options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddDiscord(options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"];
        options.Scope.Add("identify");
        options.Scope.Add("email");
    })
    .AddGitHub(options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = builder.Configuration["Authentication:Github:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Github:ClientSecret"];

        options.Scope.Add("public_repo");
        options.Scope.Add("read:org");
        options.Scope.Add("gist");
        options.Scope.Add("read:user");
        options.Scope.Add("user:email");
        options.Scope.Add("user:follow");
        options.Scope.Add("read:project");
    })
    .AddCookie();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICodeJamService, CodeJamService>();

builder.Services.AddGraphQLServer()
    .AddQueryType<GraphQLQueries>()
    .AddTypeExtension<CodeJamTopicQueryExtensions>()
    .AddTypeExtension<UserManagementExtensions>()
    .AddMutationType<GraphQlMutations>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

#if DEBUG
try
{
    // We have services that are scoped. Therefore we need to create a new scope 
    // for the things we'll use in our SeedDb.
    var serviceProvider = app.Services.CreateScope().ServiceProvider;
    await SeedDb.Seed(serviceProvider);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    await app.DisposeAsync();
    Environment.Exit(1);
}
#endif

app.Run();
