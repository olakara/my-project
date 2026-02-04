using System.Security.Claims;

namespace TaskManagement.Api.Features.Projects.GetProjects;

public static class GetProjectsEndpoint
{
    public static void MapGetProjectsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/projects", GetProjectsAsync)
            .WithName("GetProjects")
            .WithOpenApi()
            .Produces<IReadOnlyList<ProjectSummaryResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization()
            .WithSummary("Get projects for current user")
            .WithDescription("Returns a list of projects where the current user is a member or owner");
    }

    private static async System.Threading.Tasks.Task<IResult> GetProjectsAsync(
        IGetProjectsService getProjectsService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var projects = await getProjectsService.GetProjectsAsync(userId, ct);
        return Results.Ok(projects);
    }
}
