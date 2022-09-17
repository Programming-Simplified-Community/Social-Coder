using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SocialCoder.Web.Client;
using SocialCoder.Web.Client.Services.Contracts;
using SocialCoder.Web.Client.Services.Implementations;
using System.Globalization;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ICodeJamService, CodeJamService>();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSocialCoderGraphQLClient()
    .ConfigureHttpClient(c=>c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "graphql"));

var host = builder.Build();

#region Setup Application Culture By User Browser
CultureInfo culture;
var js = host.Services.GetRequiredService<IJSRuntime>();
var result = await js.InvokeAsync<string>("blazorCulture.get");

if (!string.IsNullOrEmpty(result))
    culture = new CultureInfo(result);
else
{
    culture = new CultureInfo("en-US");
    await js.InvokeVoidAsync("blazorCulture.set", "en-US");
}

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

TimeUtil.TimezoneOffset = await js.InvokeAsync<int>("eval", "-new Date().getTimezoneOffset()");

#endregion

await host.RunAsync();