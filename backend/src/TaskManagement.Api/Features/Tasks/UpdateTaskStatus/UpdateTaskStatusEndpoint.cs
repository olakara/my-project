using FluentValidation;
using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Features.Tasks.UpdateTaskStatus;

public class UpdateTaskStatusEndpoint
{
    public static void MapUpdateTaskStatus(WebApplication app)
    {
        app.MapPatch("/api/v1/tasks/{taskId}/status", UpdateTaskStatus)
            .WithName("UpdateTaskStatus")
            .WithOpenApi()
            .Produces<UpdateTaskStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithDescription("Update the status of a task (e.g., drag-drop on Kanban board)");
    }

    private static async System.Threading.Tasks.Task UpdateTaskStatus(
        int taskId,
        HttpContext httpContext,
        IUpdateTaskStatusService service,
        IValidator<UpdateTaskStatusRequest> validator,
        UpdateTaskStatusRequest request,
        ILogger<UpdateTaskStatusEndpoint> logger,
        CancellationToken ct = default)
    {
        try
        {
            var userId = httpContext.User.FindFirst("sub")?.Value
                      ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                      ?? throw new UnauthorizedAccessException("User ID not found in token");

            // Validate request
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    new { message = "Validation failed", errors }, ct);
                return;
            }

            // Update task status
            var result = await service.UpdateTaskStatusAsync(taskId, userId, request, ct);

            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            await httpContext.Response.WriteAsJsonAsync(result, ct);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Task {TaskId} not found", taskId);
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized access attempt to update task {TaskId} by user", taskId);
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating task {TaskId} status", taskId);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Internal server error" }, CancellationToken.None);
        }
    }
}
