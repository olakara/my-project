using FluentValidation;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Features.Tasks.GetKanbanBoard;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.GetKanbanBoard;

public class GetKanbanBoardEndpoint
{
    public static void MapGetKanbanBoard(WebApplication app)
    {
        app.MapGet("/api/v1/projects/{projectId}/tasks", GetKanbanBoard)
            .WithName("GetKanbanBoard")
            .WithOpenApi()
            .Produces<KanbanBoardDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithDescription("Get tasks for a project organized in Kanban board format with optional filtering and pagination");
    }

    private static async System.Threading.Tasks.Task GetKanbanBoard(
        int projectId,
        HttpContext httpContext,
        IGetKanbanBoardService service,
        ILogger<GetKanbanBoardEndpoint> logger,
        int page = 1,
        int pageSize = 50,
        string? assigneeId = null,
        int? priority = null,
        DateTime? dueDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var userId = httpContext.User.FindFirst("sub")?.Value 
                      ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                      ?? throw new UnauthorizedAccessException("User ID not found in token");

            // Validate pagination parameters
            if (page < 1)
                page = 1;
            if (pageSize < 1 || pageSize > 100)
                pageSize = 50;

            // Parse priority filter if provided
            TaskPriority? priorityFilter = null;
            if (priority.HasValue && Enum.IsDefined(typeof(TaskPriority), priority.Value))
            {
                priorityFilter = (TaskPriority)priority.Value;
            }

            var result = await service.GetKanbanBoardAsync(
                projectId,
                userId,
                page,
                pageSize,
                assigneeId,
                priorityFilter,
                dueDate,
                ct);

            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            await httpContext.Response.WriteAsJsonAsync(result, ct);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Project {ProjectId} not found", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized access attempt to Kanban board for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving Kanban board for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Internal server error" }, CancellationToken.None);
        }
    }
}
