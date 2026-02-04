using System.Security.Claims;

namespace TaskManagement.Api.Features.Projects.GetProject;

public static class GetProjectEndpoint
{
    public static void MapGetProjectEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/projects/{projectId:int}", GetProjectAsync)
            .WithName("GetProject")
            .WithOpenApi()
            .Produces<GetProjectResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Get project details")
            .WithDescription("Returns project details including members for the specified project");
    }

    private static async System.Threading.Tasks.Task<IResult> GetProjectAsync(
        int projectId,
        IGetProjectService getProjectService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var project = await getProjectService.GetProjectAsync(projectId, userId, ct);
            return Results.Ok(project);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Project not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
    }
}
