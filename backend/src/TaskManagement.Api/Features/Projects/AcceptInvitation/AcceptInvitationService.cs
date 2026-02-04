using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Projects.AcceptInvitation;

public interface IAcceptInvitationService
{
    Task<AcceptInvitationResponse> AcceptInvitationAsync(int invitationId, string userId, CancellationToken ct = default);
}

public class AcceptInvitationService : IAcceptInvitationService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AcceptInvitationService> _logger;

    public AcceptInvitationService(
        TaskManagementDbContext context,
        IProjectRepository projectRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<AcceptInvitationService> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AcceptInvitationResponse> AcceptInvitationAsync(int invitationId, string userId, CancellationToken ct = default)
    {
        // Find the invitation
        var invitation = await _context.ProjectInvitations
            .Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Id == invitationId, ct);

        if (invitation == null)
        {
            throw new KeyNotFoundException("Invitation not found");
        }

        // Verify invitation is valid
        if (invitation.IsExpired)
        {
            throw new InvalidOperationException("This invitation has expired");
        }

        if (invitation.Status != ProjectInvitationStatus.Pending)
        {
            throw new InvalidOperationException($"This invitation has already been {invitation.Status.ToString().ToLower()}");
        }

        // Get the user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Verify user email matches invitation
        if (!string.Equals(user.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("This invitation is not for your email address");
        }

        // Check if user is already a member
        var existingMember = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == invitation.ProjectId && pm.UserId == userId, ct);

        if (existingMember != null)
        {
            throw new InvalidOperationException("You are already a member of this project");
        }

        // Create project member
        var projectMember = new ProjectMember
        {
            UserId = userId,
            ProjectId = invitation.ProjectId,
            Role = invitation.Role,
            JoinedTimestamp = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(projectMember);

        // Update invitation status
        invitation.Status = ProjectInvitationStatus.Accepted;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} accepted invitation {InvitationId} and joined project {ProjectId} as {Role}",
            userId,
            invitationId,
            invitation.ProjectId,
            invitation.Role);

        return new AcceptInvitationResponse
        {
            ProjectId = invitation.ProjectId,
            ProjectName = invitation.Project.Name,
            Role = invitation.Role.ToString(),
            Message = $"Successfully joined project '{invitation.Project.Name}' as {invitation.Role}"
        };
    }
}
