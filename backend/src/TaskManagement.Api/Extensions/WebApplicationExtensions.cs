using TaskManagement.Api.Middleware;
using TaskManagement.Api.Features.Auth.Register;
using TaskManagement.Api.Features.Auth.Login;
using TaskManagement.Api.Features.Auth.RefreshToken;
using TaskManagement.Api.Features.Auth.Logout;

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

        return app;
    }
}
