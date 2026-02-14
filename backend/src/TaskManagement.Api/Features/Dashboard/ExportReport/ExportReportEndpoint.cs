using System.Security.Claims;

namespace TaskManagement.Api.Features.Dashboard.ExportReport;

public static class ExportReportEndpoint
{
    public static void MapExportReportEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/projects/{projectId}/export-report", ExportReportAsync)
            .WithName("ExportProjectReport")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization()
            .WithSummary("Export project report")
            .WithDescription("Returns a CSV export of project tasks");
    }

    private static async System.Threading.Tasks.Task ExportReportAsync(
        int projectId,
        HttpContext httpContext,
        IExportReportService service,
        ILogger<ExportReportService> logger,
        CancellationToken ct)
    {
        try
        {
            var userId = httpContext.User.FindFirst("sub")?.Value
                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var result = await service.ExportReportAsync(projectId, userId, ct);

            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            httpContext.Response.ContentType = result.ContentType;
            httpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{result.FileName}\"");
            await httpContext.Response.WriteAsync(result.Content, ct);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Project {ProjectId} not found", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized access to export report for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting report for project {ProjectId}", projectId);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = "Internal server error" }, CancellationToken.None);
        }
    }
}
