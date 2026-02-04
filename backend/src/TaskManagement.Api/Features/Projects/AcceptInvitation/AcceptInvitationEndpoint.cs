using System.Security.Claims;
using FluentValidation;

namespace TaskManagement.Api.Features.Projects.AcceptInvitation;

public static class AcceptInvitationEndpoint
{
    public static void MapAcceptInvitationEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/invitations/{invitationId:int}/accept", AcceptInvitationAsync)
            .WithName("AcceptInvitation")
            .WithOpenApi()
            .Produces<AcceptInvitationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Accept project invitation")
            .WithDescription("Accepts a project invitation and adds the user as a project member");
    }

    private static async Task<IResult> AcceptInvitationAsync(
        int invitationId,
        IAcceptInvitationService acceptInvitationService,
        HttpContext httpContext,
        ILogger<AcceptInvitationService> logger,
        CancellationToken ct)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var response = await acceptInvitationService.AcceptInvitationAsync(invitationId, userId, ct);
            logger.LogInformation("User {UserId} accepted invitation {InvitationId}", userId, invitationId);
            return Results.Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Invitation or user not found: {Message}", ex.Message);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("User {UserId} unauthorized to accept invitation {InvitationId}: {Message}",
                userId, invitationId, ex.Message);
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Invalid operation for invitation {InvitationId}: {Message}",
                invitationId, ex.Message);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error accepting invitation {InvitationId} for user {UserId}",
                invitationId, userId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
