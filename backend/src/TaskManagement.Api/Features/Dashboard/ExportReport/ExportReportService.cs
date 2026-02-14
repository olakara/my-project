using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Users;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;

namespace TaskManagement.Api.Features.Dashboard.ExportReport;

public record ExportReportResult(string FileName, string Content, string ContentType);

public interface IExportReportService
{
    System.Threading.Tasks.Task<ExportReportResult> ExportReportAsync(int projectId, string userId, CancellationToken ct = default);
}

public class ExportReportService : IExportReportService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;

    public ExportReportService(TaskManagementDbContext context, IProjectRepository projectRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<ExportReportResult> ExportReportAsync(int projectId, string userId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        if (!IsProjectMember(project, userId))
        {
            throw new UnauthorizedAccessException("User is not a member of this project");
        }

        var tasks = await _context.Tasks
            .AsNoTracking()
            .Include(task => task.Assignee)
            .Include(task => task.Creator)
            .Where(task => task.ProjectId == projectId)
            .OrderBy(task => task.CreatedTimestamp)
            .ToListAsync(ct);

        var csv = BuildCsv(project, tasks);
        var fileName = $"project-{projectId}-tasks-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return new ExportReportResult(fileName, csv, "text/csv");
    }

    private static string BuildCsv(Project project, IReadOnlyCollection<DomainTask> tasks)
    {
        var builder = new StringBuilder();
        builder.AppendLine("ProjectId,ProjectName,TaskId,Title,Description,Status,Priority,AssigneeName,AssigneeEmail,CreatedByName,CreatedByEmail,DueDate,CreatedAtUtc,UpdatedAtUtc");

        foreach (var task in tasks)
        {
            var assigneeName = task.Assignee != null ? BuildFullName(task.Assignee) : string.Empty;
            var assigneeEmail = task.Assignee?.Email ?? string.Empty;
            var creatorName = BuildFullName(task.Creator);
            var creatorEmail = task.Creator.Email ?? string.Empty;

            builder.Append(EscapeCsv(project.Id.ToString(CultureInfo.InvariantCulture))).Append(',');
            builder.Append(EscapeCsv(project.Name)).Append(',');
            builder.Append(EscapeCsv(task.Id.ToString(CultureInfo.InvariantCulture))).Append(',');
            builder.Append(EscapeCsv(task.Title)).Append(',');
            builder.Append(EscapeCsv(task.Description ?? string.Empty)).Append(',');
            builder.Append(EscapeCsv(task.Status.ToString())).Append(',');
            builder.Append(EscapeCsv(task.Priority.ToString())).Append(',');
            builder.Append(EscapeCsv(assigneeName)).Append(',');
            builder.Append(EscapeCsv(assigneeEmail)).Append(',');
            builder.Append(EscapeCsv(creatorName)).Append(',');
            builder.Append(EscapeCsv(creatorEmail)).Append(',');
            builder.Append(EscapeCsv(FormatDate(task.DueDate))).Append(',');
            builder.Append(EscapeCsv(FormatDate(task.CreatedTimestamp))).Append(',');
            builder.Append(EscapeCsv(FormatDate(task.UpdatedTimestamp)));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue
            ? value.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuotes)
        {
            return value;
        }

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static bool IsProjectMember(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        return project.Members.Any(member => member.UserId == userId);
    }

    private static string BuildFullName(ApplicationUser user)
    {
        var parts = new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join(" ", parts);
    }
}
