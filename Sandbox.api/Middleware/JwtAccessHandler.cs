using Microsoft.AspNetCore.Authorization;
using W.Ind.Core.Middleware;

namespace Sandbox.api.Middleware;

// JwtAccessBase (abstract): Implements base methods for processing JWT authenticated Bearer requests
    // ShouldSkip(context): override required for use
public class JwtAccessHandler : JwtAccessBase
{
    public JwtAccessHandler(RequestDelegate next) : base(next) { }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            // Ignores token processing & continues request
            await _next(context);
            return;
        }

        await ProcessTokenAsync(context);
    }

    // ShouldSkip(context): Only method not implemented in base class
    protected override bool ShouldSkip(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint != null && endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
    }
}
