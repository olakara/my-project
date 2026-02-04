using System.Security.Claims;
using FluentValidation;
using TaskManagement.Api.Features.Tasks.GetTask;

namespace TaskManagement.Api.Features.Tasks.UpdateTask;

public static class UpdateTaskEndpoint
{
    public static void MapUpdateTaskEndpoint(this WebApplication app)
    {
        app.MapPut("/api/v1/tasks/{taskId:int}", UpdateTaskAsync)
            .WithName("UpdateTask")
            .WithOpenApi()
            .Produces<GetTaskResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Update task")
            .WithDescription("Updates task details for the creator or project managers");
    }

    private static async System.Threading.Tasks.Task<IResult> UpdateTaskAsync(
        int taskId,
        UpdateTaskRequest request,
        IUpdateTaskService updateTaskService,
        IValidator<UpdateTaskRequest> validator,
        HttpContext httpContext,
        ILogger<UpdateTaskService> logger,
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
            var response = await updateTaskService.UpdateTaskAsync(taskId, userId, request, ct);
            logger.LogInformation("Task updated successfully: {TaskId}", response.Id);
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
