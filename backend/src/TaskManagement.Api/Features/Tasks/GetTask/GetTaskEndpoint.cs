using System.Security.Claims;

namespace TaskManagement.Api.Features.Tasks.GetTask;

public static class GetTaskEndpoint
{
    public static void MapGetTaskEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/tasks/{taskId:int}", GetTaskAsync)
            .WithName("GetTask")
            .WithOpenApi()
            .Produces<GetTaskResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Get task details")
            .WithDescription("Returns detailed task information including comments and history preview");
    }

    private static async System.Threading.Tasks.Task<IResult> GetTaskAsync(
        int taskId,
        IGetTaskService getTaskService,
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
            var response = await getTaskService.GetTaskAsync(taskId, userId, ct);
            return Results.Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Task not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
    }
}
