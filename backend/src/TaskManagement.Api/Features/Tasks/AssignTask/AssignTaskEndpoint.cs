using System.Security.Claims;
using FluentValidation;

namespace TaskManagement.Api.Features.Tasks.AssignTask;

public static class AssignTaskEndpoint
{
    public static void MapAssignTaskEndpoint(this WebApplication app)
    {
        app.MapPatch("/api/v1/tasks/{taskId:int}/assign", AssignTaskAsync)
            .WithName("AssignTask")
            .WithOpenApi()
            .Produces<AssignTaskResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Assign a task to a team member")
            .WithDescription("Assigns a task to a specified team member or unassigns it if assigneeId is null");
    }

    private static async System.Threading.Tasks.Task<IResult> AssignTaskAsync(
        int taskId,
        AssignTaskRequest request,
        IAssignTaskService assignTaskService,
        IValidator<AssignTaskRequest> validator,
        HttpContext httpContext,
        ILogger<AssignTaskResponse> logger,
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
            var response = await assignTaskService.AssignTaskAsync(taskId, userId, request, ct);
            logger.LogInformation("Task {TaskId} assigned to user {AssigneeId}", taskId, request.AssigneeId);
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
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
