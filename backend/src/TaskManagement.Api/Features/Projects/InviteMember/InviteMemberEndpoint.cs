using System.Security.Claims;
using FluentValidation;

namespace TaskManagement.Api.Features.Projects.InviteMember;

public static class InviteMemberEndpoint
{
    public static void MapInviteMemberEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/projects/{projectId:int}/invitations", InviteMemberAsync)
            .WithName("InviteMember")
            .WithOpenApi()
            .Produces<InviteMemberResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Invite member to project")
            .WithDescription("Creates an invitation for a user to join a project");
    }

    private static async System.Threading.Tasks.Task<IResult> InviteMemberAsync(
        int projectId,
        InviteMemberRequest request,
        IInviteMemberService inviteMemberService,
        IValidator<InviteMemberRequest> validator,
        HttpContext httpContext,
        ILogger<InviteMemberService> logger,
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
            var response = await inviteMemberService.InviteMemberAsync(projectId, userId, request, ct);
            logger.LogInformation("Invitation created for project {ProjectId} and email {Email}", projectId, request.Email);
            return Results.Created($"/api/v1/projects/{projectId}/invitations/{response.Id}", response);
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
