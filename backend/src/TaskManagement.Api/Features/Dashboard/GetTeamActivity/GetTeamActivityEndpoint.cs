using System.Security.Claims;

namespace TaskManagement.Api.Features.Dashboard.GetTeamActivity;

public static class GetTeamActivityEndpoint
{
    public static void MapGetTeamActivityEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/projects/{projectId}/team-activity", GetTeamActivityAsync)
            .WithName("GetTeamActivity")
            .WithOpenApi()
            .Produces<TeamActivityResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Get project team activity")
            .WithDescription("Returns team member activity metrics sorted by completed tasks");
    }

    private static async System.Threading.Tasks.Task GetTeamActivityAsync(
        int projectId,
        HttpContext httpContext,
        IGetTeamActivityService service,
        ILogger<GetTeamActivityService> logger,
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

            var response = await service.GetTeamActivityAsync(projectId, userId, ct);

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
            logger.LogWarning("Unauthorized access to team activity for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving team activity for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Internal server error" }, CancellationToken.None);
        }
    }
}
