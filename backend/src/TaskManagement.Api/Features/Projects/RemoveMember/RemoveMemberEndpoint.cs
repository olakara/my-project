using System.Security.Claims;

namespace TaskManagement.Api.Features.Projects.RemoveMember;

public static class RemoveMemberEndpoint
{
    public static void MapRemoveMemberEndpoint(this WebApplication app)
    {
        app.MapDelete("/api/v1/projects/{projectId:int}/members/{userId}", RemoveMemberAsync)
            .WithName("RemoveProjectMember")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Remove member from project")
            .WithDescription("Removes a member from a project and unassigns their tasks");
    }

    private static async System.Threading.Tasks.Task<IResult> RemoveMemberAsync(
        int projectId,
        string userId,
        IRemoveMemberService removeMemberService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var requestedByUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(requestedByUserId))
        {
            return Results.Unauthorized();
        }

        try
        {
            await removeMemberService.RemoveMemberAsync(projectId, userId, requestedByUserId, ct);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex) when (ex.Message.Contains("Project"))
        {
            return Results.NotFound(new { error = "Project not found" });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Member not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
