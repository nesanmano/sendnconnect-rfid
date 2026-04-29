using System.Security.Claims;

namespace CardManagement.Api.Infrastructure;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    private static readonly string[] SkipPaths = ["/health", "/swagger", "/favicon.ico"];

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (SkipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var orgId = context.User.FindFirstValue("urn:zitadel:iam:org:id")
                 ?? context.User.FindFirstValue("org_id")
                 ?? context.Request.Headers["X-Org-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(orgId))
        {
            _logger.LogWarning("No org_id found in token or headers for path {Path}", path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Missing org_id. Provide org_id claim or X-Org-Id header." });
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub")
                  ?? "unknown";

        tenantContext.SetTenant(orgId, userId);
        await _next(context);
    }
}
