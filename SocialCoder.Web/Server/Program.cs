using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services.Contracts;
using SocialCoder.Web.Server.Services.Implementations;
using SocialCoder.Web.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConnections();
builder.Configuration.AddJsonConfigurationFiles();

// Add services to the container.
var connectionString = builder.Configuration["DefaultConnection"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;

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

builder.Services.AddAuthentication("MainCookie")
    .AddDiscord(options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"];
        options.Scope.Add("identify");
        options.Scope.Add("email");
    });


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IUserService, UserService>();

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
