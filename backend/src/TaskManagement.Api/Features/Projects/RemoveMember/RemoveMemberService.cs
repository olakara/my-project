using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Features.Projects.RemoveMember;

public interface IRemoveMemberService
{
    System.Threading.Tasks.Task RemoveMemberAsync(int projectId, string memberUserId, string requestedByUserId, CancellationToken ct = default);
}

public class RemoveMemberService : IRemoveMemberService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<RemoveMemberService> _logger;

    public RemoveMemberService(TaskManagementDbContext context, IProjectRepository projectRepository, ILogger<RemoveMemberService> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task RemoveMemberAsync(int projectId, string memberUserId, string requestedByUserId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        if (!CanManageMembers(project, requestedByUserId))
        {
            throw new UnauthorizedAccessException("User is not authorized to remove members");
        }

        if (project.OwnerId == memberUserId)
        {
            throw new InvalidOperationException("Project owner cannot be removed");
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == memberUserId);
        if (member == null)
        {
            throw new KeyNotFoundException("Member not found");
        }

        var assignedTasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.AssigneeId == memberUserId)
            .ToListAsync(ct);

        foreach (var task in assignedTasks)
        {
            task.AssigneeId = null;
            task.UpdatedTimestamp = DateTime.UtcNow;
        }

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {RequestedByUserId} removed member {MemberUserId} from project {ProjectId}", requestedByUserId, memberUserId, projectId);
    }

    private static bool CanManageMembers(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        var role = project.GetUserRole(userId);
        return role == ProjectRole.Manager;
    }
}
