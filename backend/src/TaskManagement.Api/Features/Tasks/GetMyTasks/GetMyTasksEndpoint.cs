using System.Security.Claims;
using TaskManagement.Api.Domain.Tasks;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.GetMyTasks;

public static class GetMyTasksEndpoint
{
    public static void MapGetMyTasksEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/tasks/my-tasks", GetMyTasksAsync)
            .WithName("GetMyTasks")
            .WithOpenApi()
            .Produces<List<GetMyTasksResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization()
            .WithSummary("Get tasks assigned to current user")
            .WithDescription("Returns tasks assigned to the authenticated user with optional filtering and pagination");
    }

    private static async System.Threading.Tasks.Task<IResult> GetMyTasksAsync(
        HttpContext httpContext,
        IGetMyTasksService getMyTasksService,
        string? status = null,
        string? priority = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        DomainTaskStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse(status, true, out DomainTaskStatus parsedStatus))
            {
                return Results.BadRequest(new { error = "Invalid status filter" });
            }

            statusFilter = parsedStatus;
        }

        TaskPriority? priorityFilter = null;
        if (!string.IsNullOrWhiteSpace(priority))
        {
            if (!Enum.TryParse(priority, true, out TaskPriority parsedPriority))
            {
                return Results.BadRequest(new { error = "Invalid priority filter" });
            }

            priorityFilter = parsedPriority;
        }

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 50;
        }

        var response = await getMyTasksService.GetMyTasksAsync(
            userId,
            page,
            pageSize,
            statusFilter,
            priorityFilter,
            ct);

        return Results.Ok(response);
    }
}
