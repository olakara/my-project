using System.Security.Claims;

namespace TaskManagement.Api.Features.Dashboard.GetProjectMetrics;

public static class GetProjectMetricsEndpoint
{
    public static void MapGetProjectMetricsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/projects/{projectId}/metrics", GetProjectMetricsAsync)
            .WithName("GetProjectMetrics")
            .WithOpenApi()
            .Produces<ProjectMetricsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Get project metrics")
            .WithDescription("Returns task counts by status, completion percentage, and team member statistics for a project");
    }

    private static async System.Threading.Tasks.Task GetProjectMetricsAsync(
        int projectId,
        HttpContext httpContext,
        IGetProjectMetricsService service,
        ILogger<GetProjectMetricsService> logger,
        CancellationToken ct)
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

            var response = await service.GetProjectMetricsAsync(projectId, userId, ct);

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
            logger.LogWarning("Unauthorized access to project metrics for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving project metrics for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Internal server error" }, CancellationToken.None);
        }
    }
}
