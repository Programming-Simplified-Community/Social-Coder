using SocialCoder.Web.Server;
using SocialCoder.Web.Server.Extensions;
using SocialCoder.Web.Server.Middleware;
using SocialCoder.Web.Server.Models;
using SocialCoder.Web.Server.Services;
using SocialCoder.Web.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConnections();
builder.Configuration.AddJsonConfigurationFiles();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton<AppStateProvider>();

var isSetupComplete = builder.Configuration.GetValue<bool>("IsSetupComplete");

builder.Services.AddRazorPages();
builder.Services.AddRazorComponents();

builder.Services.AddControllersWithViews().ConfigureApplicationPartManager(apm =>
{
    apm.FeatureProviders.Add(new SetupModeControllerProvider(!isSetupComplete));
});

if (isSetupComplete)
    builder.Services.SetupForProduction(builder.Configuration);
else
    builder.Services.SetupForAdmin(builder.Configuration);


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

app.UseMiddleware<SetupModeMiddleware>();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

if (isSetupComplete)
    app.SetupForProduction();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

#if DEBUG
if (isSetupComplete)
{
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
}
#endif

app.Run();
