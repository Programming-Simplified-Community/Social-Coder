using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SocialCoder.Web.Server;
using SocialCoder.Web.Server.Data;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConnections();

// Add services to the container.
var connectionString = builder.Configuration["DefaultConnection"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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

app.UseIdentityServer();
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
