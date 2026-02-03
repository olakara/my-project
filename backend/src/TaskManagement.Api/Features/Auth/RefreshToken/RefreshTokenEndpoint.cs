namespace TaskManagement.Api.Features.Auth.RefreshToken;

public static class RefreshTokenEndpoint
{
    public static void MapRefreshTokenEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/auth/refresh", RefreshAsync)
            .WithName("RefreshToken")
            .WithOpenApi()
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .WithSummary("Refresh access token")
            .WithDescription("Exchanges a valid refresh token for a new access token and rotated refresh token");
    }

    private static async System.Threading.Tasks.Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        IRefreshTokenService refreshTokenService,
        ILogger<RefreshTokenResponse> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.BadRequest(new { error = "Refresh token is required" });
        }

        try
        {
            var response = await refreshTokenService.RefreshAsync(request.RefreshToken, ct);
            logger.LogInformation("Token refreshed successfully");
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Token refresh failed: {Message}", ex.Message);
            return Results.Unauthorized();
        }
    }
}
