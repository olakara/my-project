using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Services;
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
using TaskManagement.Api.Features.Tasks.GetMyTasks;
using TaskManagement.Api.Features.Tasks.UpdateTask;
using TaskManagement.Api.Features.Tasks.AssignTask;

namespace TaskManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddScoped<TaskManagementDbContext>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddHttpContextAccessor();
        
        // Auth Feature Services
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ILogoutService, LogoutService>();

        // Project Feature Services
        services.AddScoped<ICreateProjectService, CreateProjectService>();
        services.AddScoped<IGetProjectsService, GetProjectsService>();
        services.AddScoped<IGetProjectService, GetProjectService>();
        services.AddScoped<IUpdateProjectService, UpdateProjectService>();
        services.AddScoped<IInviteMemberService, InviteMemberService>();
        services.AddScoped<IRemoveMemberService, RemoveMemberService>();
        services.AddScoped<IAcceptInvitationService, AcceptInvitationService>();
        services.AddScoped<ICreateTaskService, CreateTaskService>();
        services.AddScoped<IGetTaskService, GetTaskService>();
        services.AddScoped<IGetMyTasksService, GetMyTasksService>();
        services.AddScoped<IUpdateTaskService, UpdateTaskService>();
        services.AddScoped<IAssignTaskService, AssignTaskService>();

        // Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"];
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT Secret key is not configured");

        var key = Encoding.ASCII.GetBytes(secretKey);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(5)
                };
            });

        // Authorization
        services.AddAuthorization();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                var corsOrigins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:5173" };
                builder
                    .WithOrigins(corsOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
