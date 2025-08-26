using SocialCoder.Web.Server.Services;

namespace SocialCoder.Web.Server.Middleware;

/// <summary>
/// Enforces rules based on whether the application is in setup mode.
/// </summary>
public class SetupModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SetupModeMiddleware> _logger;

    public SetupModeMiddleware(RequestDelegate next, ILogger<SetupModeMiddleware> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppStateProvider stateProvider)
    {
        if (stateProvider.IsInSetupMode && !this.IsAllowedInSetupMode(context))
        {
            this._logger.LogWarning("Blocked request to {Path} due to setup mode.", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Application is currently in setup mode");
            return;
        }

        await this._next(context);
    }

    /// <summary>
    /// Limits the number of endpoints a user can reach while in setup mode.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool IsAllowedInSetupMode(HttpContext context)
    {
        var path = context.Request.Path;

        return path.StartsWithSegments("/api/configuration") ||
               path.StartsWithSegments("/_framework") ||
               path.StartsWithSegments("/css") ||
               path.StartsWithSegments("/js") ||
               path.Value == "/" ||
               path.Value == "/setup" ||
               !path.StartsWithSegments("/api");
    }
}