using FluentValidation;
using System.Security.Claims;

namespace TaskManagement.Api.Features.Projects.CreateProject;

public static class CreateProjectEndpoint
{
    public static void MapCreateProjectEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/projects", CreateProjectAsync)
            .WithName("CreateProject")
            .WithOpenApi()
            .Produces<CreateProjectResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization()
            .WithSummary("Create a new project")
            .WithDescription("Creates a new project and adds the creator as the owner");
    }

    private static async System.Threading.Tasks.Task<IResult> CreateProjectAsync(
        CreateProjectRequest request,
        ICreateProjectService createProjectService,
        IValidator<CreateProjectRequest> validator,
        HttpContext httpContext,
        ILogger<CreateProjectResponse> logger,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await createProjectService.CreateProjectAsync(userId, request, ct);
            logger.LogInformation("Project created successfully: {ProjectId}", response.Id);
            return Results.Created($"/api/v1/projects/{response.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Project creation failed: {Message}", ex.Message);
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
