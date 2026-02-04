using System.Security.Claims;
using FluentValidation;
using TaskManagement.Api.Features.Projects.GetProject;

namespace TaskManagement.Api.Features.Projects.UpdateProject;

public static class UpdateProjectEndpoint
{
    public static void MapUpdateProjectEndpoint(this WebApplication app)
    {
        app.MapPut("/api/v1/projects/{projectId:int}", UpdateProjectAsync)
            .WithName("UpdateProject")
            .WithOpenApi()
            .Produces<GetProjectResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Update project")
            .WithDescription("Updates project details for Owner or Manager");
    }

    private static async System.Threading.Tasks.Task<IResult> UpdateProjectAsync(
        int projectId,
        UpdateProjectRequest request,
        IUpdateProjectService updateProjectService,
        IValidator<UpdateProjectRequest> validator,
        HttpContext httpContext,
        ILogger<UpdateProjectService> logger,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var response = await updateProjectService.UpdateProjectAsync(projectId, userId, request, ct);
            logger.LogInformation("Project updated successfully: {ProjectId}", response.Id);
            return Results.Ok(response);
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
