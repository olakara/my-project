using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagement.Api.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static void MapLogoutEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/auth/logout", LogoutAsync)
            .WithName("Logout")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .RequireAuthorization()
            .WithSummary("Logout user")
            .WithDescription("Invalidates the user's refresh token, requiring re-authentication for future access");
    }

    [Authorize]
    private static async System.Threading.Tasks.Task<IResult> LogoutAsync(
        ClaimsPrincipal user,
        ILogoutService logoutService,
        ILogger<LogoutService> logger,
        CancellationToken ct)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            await logoutService.LogoutAsync(userId, ct);
            logger.LogInformation("User {UserId} logged out", userId);
            return Results.Ok(new { message = "Logged out successfully" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Logout failed: {Message}", ex.Message);
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
