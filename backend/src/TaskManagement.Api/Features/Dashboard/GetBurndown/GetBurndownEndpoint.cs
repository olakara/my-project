using System.Security.Claims;

namespace TaskManagement.Api.Features.Dashboard.GetBurndown;

public static class GetBurndownEndpoint
{
    public static void MapGetBurndownEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/projects/{projectId}/burndown", GetBurndownAsync)
            .WithName("GetBurndown")
            .WithOpenApi()
            .Produces<BurndownResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Get project burndown")
            .WithDescription("Returns daily task completion counts for a project within a date range");
    }

    private static async System.Threading.Tasks.Task GetBurndownAsync(
        int projectId,
        HttpContext httpContext,
        IGetBurndownService service,
        ILogger<GetBurndownEndpoint> logger,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var userId = httpContext.User.FindFirst("sub")?.Value
                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var resolvedEnd = (endDate ?? DateTime.UtcNow).Date;
            var resolvedStart = (startDate ?? resolvedEnd.AddDays(-29)).Date;

            if (resolvedEnd < resolvedStart)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { message = "End date must be on or after start date." }, ct);
                return;
            }

            var response = await service.GetBurndownAsync(projectId, userId, resolvedStart, resolvedEnd, ct);

            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            await httpContext.Response.WriteAsJsonAsync(response, ct);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Project {ProjectId} not found", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized access to burndown for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving burndown for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Internal server error" }, CancellationToken.None);
        }
    }
}
