using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Projects.InviteMember;

public interface IInviteMemberService
{
    System.Threading.Tasks.Task<InviteMemberResponse> InviteMemberAsync(int projectId, string inviterId, InviteMemberRequest request, CancellationToken ct = default);
}

public class InviteMemberService : IInviteMemberService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<InviteMemberService> _logger;

    public InviteMemberService(
        TaskManagementDbContext context,
        IProjectRepository projectRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<InviteMemberService> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<InviteMemberResponse> InviteMemberAsync(int projectId, string inviterId, InviteMemberRequest request, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        if (!CanInvite(project, inviterId))
        {
            throw new UnauthorizedAccessException("User is not authorized to invite members");
        }

        var email = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null && project.Members.Any(m => m.UserId == existingUser.Id))
        {
            throw new InvalidOperationException("User is already a member of this project");
        }

        var existingInvitation = await _context.ProjectInvitations
            .Where(i => i.ProjectId == projectId && i.Email == email)
            .OrderByDescending(i => i.CreatedTimestamp)
            .FirstOrDefaultAsync(ct);

        if (existingInvitation != null && !existingInvitation.IsExpired && existingInvitation.Status == ProjectInvitationStatus.Pending)
        {
            throw new InvalidOperationException("An active invitation already exists for this email");
        }

        var invitation = new ProjectInvitation
        {
            Email = email,
            ProjectId = projectId,
            InviterId = inviterId,
            Role = request.Role,
            Status = ProjectInvitationStatus.Pending,
            CreatedTimestamp = DateTime.UtcNow,
            ExpiresTimestamp = DateTime.UtcNow.AddDays(14)
        };

        _context.ProjectInvitations.Add(invitation);
        await _context.SaveChangesAsync(ct);

        await SendInvitationEmailAsync(email, project.Name, ct);

        var inviter = await _userManager.FindByIdAsync(inviterId);
        if (inviter == null)
        {
            throw new InvalidOperationException("Inviter not found");
        }

        _logger.LogInformation("User {InviterId} invited {Email} to project {ProjectId} as {Role}", inviterId, email, projectId, request.Role);

        return new InviteMemberResponse
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role.ToString(),
            Status = invitation.IsExpired ? "Expired" : invitation.Status.ToString(),
            InvitedBy = new UserSummaryResponse
            {
                UserId = inviter.Id,
                FullName = inviter.FullName,
                Email = inviter.Email ?? string.Empty,
                ProfilePictureUrl = inviter.ProfilePictureUrl
            },
            CreatedAt = invitation.CreatedTimestamp,
            ExpiresAt = invitation.ExpiresTimestamp
        };
    }

    private System.Threading.Tasks.Task SendInvitationEmailAsync(string email, string projectName, CancellationToken ct)
    {
        _logger.LogInformation("Invitation email queued for {Email} to join project {ProjectName}", email, projectName);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    private static bool CanInvite(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        var role = project.GetUserRole(userId);
        return role == ProjectRole.Manager;
    }
}
