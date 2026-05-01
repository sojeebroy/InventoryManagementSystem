using Microsoft.AspNetCore.Identity;
using Inventory_Management_System.Models;

namespace Inventory_Management_System.Middleware;

/// <summary>
/// Middleware to check if user is blocked and invalidate their session
/// </summary>
public class BlockedUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BlockedUserMiddleware> _logger;

    public BlockedUserMiddleware(RequestDelegate next, ILogger<BlockedUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user != null && user.IsBlocked)
            {
                _logger.LogWarning("Blocked user {UserId} attempted to access {Path}", user.Id, context.Request.Path);

                // Sign out the user
                await signInManager.SignOutAsync();

                // Redirect to blocked page
                context.Response.Redirect("/Account/BlockedUser");
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method for registering the middleware
/// </summary>
public static class BlockedUserMiddlewareExtensions
{
    public static IApplicationBuilder UseBlockedUserMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<BlockedUserMiddleware>();
    }
}
