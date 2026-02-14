using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskManagement.Api.Features.Tasks.AddComment;

public static class AddCommentEndpoint
{
    public static IEndpointRouteBuilder MapAddCommentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/tasks/{taskId:int}/comments", AddCommentAsync)
            .RequireAuthorization()
            .WithName("AddComment")
            .WithTags("Tasks")
            .Produces<AddCommentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> AddCommentAsync(
        [FromRoute] int taskId,
        [FromBody] AddCommentRequest request,
        [FromServices] IAddCommentService service,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var response = await service.AddCommentAsync(taskId, userId, request, ct);
            return Results.Created($"/api/v1/tasks/{taskId}/comments/{response.Id}", response);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: ex.Message);
        }
    }
}
