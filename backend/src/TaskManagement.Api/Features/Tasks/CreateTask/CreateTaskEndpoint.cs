using System.Security.Claims;
using FluentValidation;

namespace TaskManagement.Api.Features.Tasks.CreateTask;

public static class CreateTaskEndpoint
{
    public static void MapCreateTaskEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/projects/{projectId:int}/tasks", CreateTaskAsync)
            .WithName("CreateTask")
            .WithOpenApi()
            .Produces<CreateTaskResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Create a new task")
            .WithDescription("Creates a new task in the specified project");
    }

    private static async System.Threading.Tasks.Task<IResult> CreateTaskAsync(
        int projectId,
        CreateTaskRequest request,
        ICreateTaskService createTaskService,
        IValidator<CreateTaskRequest> validator,
        HttpContext httpContext,
        ILogger<CreateTaskResponse> logger,
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
            var response = await createTaskService.CreateTaskAsync(projectId, userId, request, ct);
            logger.LogInformation("Task created successfully: {TaskId}", response.Id);
            return Results.Created($"/api/v1/tasks/{response.Id}", response);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Project not found" });
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
