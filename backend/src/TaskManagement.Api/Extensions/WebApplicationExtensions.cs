using TaskManagement.Api.Middleware;
using TaskManagement.Api.Features.Auth.Register;
using TaskManagement.Api.Features.Auth.Login;
using TaskManagement.Api.Features.Auth.RefreshToken;
using TaskManagement.Api.Features.Auth.Logout;
using TaskManagement.Api.Features.Projects.CreateProject;
using TaskManagement.Api.Features.Projects.GetProjects;
using TaskManagement.Api.Features.Projects.GetProject;
using TaskManagement.Api.Features.Projects.UpdateProject;
using TaskManagement.Api.Features.Projects.InviteMember;
using TaskManagement.Api.Features.Projects.RemoveMember;
using TaskManagement.Api.Features.Projects.AcceptInvitation;
using TaskManagement.Api.Features.Tasks.CreateTask;
using TaskManagement.Api.Features.Tasks.GetTask;
using TaskManagement.Api.Features.Tasks.UpdateTask;

namespace TaskManagement.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        // Exception handling - should be first
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Correlation ID middleware
        app.UseMiddleware<CorrelationIdMiddleware>();

        // HTTPS redirect in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // CORS
        app.UseCors("AllowFrontend");

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Swagger in development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        return app;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // Auth endpoints
        app.MapRegisterEndpoint();
        app.MapLoginEndpoint();
        app.MapRefreshTokenEndpoint();
        app.MapLogoutEndpoint();
        app.MapCreateProjectEndpoint();
        app.MapGetProjectsEndpoint();
        app.MapGetProjectEndpoint();
        app.MapUpdateProjectEndpoint();
        app.MapInviteMemberEndpoint();
        app.MapRemoveMemberEndpoint();
        app.MapAcceptInvitationEndpoint();
        app.MapCreateTaskEndpoint();
        app.MapGetTaskEndpoint();
        app.MapUpdateTaskEndpoint();

        return app;
    }
}
