using SocialCoder.Web.Server.Services;

namespace SocialCoder.Web.Server.Middleware;

public class SetupModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SetupModeMiddleware> _logger;
    
    public SetupModeMiddleware(RequestDelegate next, ILogger<SetupModeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppStateProvider stateProvider)
    {
        if (stateProvider.IsInSetupMode && !IsAllowedInSetupMode(context))
        {
            _logger.LogWarning("Blocked request to {Path} due to setup mode.", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Application is currently in setup mode");
            return;
        }

        await _next(context);
    }

    private bool IsAllowedInSetupMode(HttpContext context)
    {
        var path = context.Request.Path;

        return path.StartsWithSegments("/api/configuration/is-in-setup-mode") ||
               path.StartsWithSegments("/_framework") ||
               path.StartsWithSegments("/css") ||
               path.StartsWithSegments("/js") ||
               path.Value == "/" ||
               path.Value == "/setup" ||
               !path.StartsWithSegments("/api");
    }
}