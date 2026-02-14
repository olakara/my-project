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
using TaskManagement.Api.Features.Tasks.UpdateTaskStatus;
using TaskManagement.Api.Features.Tasks.AssignTask;
using TaskManagement.Api.Features.Tasks.AddComment;

namespace TaskManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Database
        services.AddScoped<TaskManagementDbContext>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

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
        services.AddScoped<IUpdateTaskStatusService, UpdateTaskStatusService>();
        services.AddScoped<IAssignTaskService, AssignTaskService>();
        services.AddScoped<IAddCommentService, AddCommentService>();

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

                // Support JWT tokens in query string for SignalR WebSocket connections
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        // If the request is for the SignalR hub and has a token, use it
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        
                        return Task.CompletedTask;
                    }
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

        // SignalR
        var signalRBuilder = services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = environment.IsDevelopment();
            options.MaximumReceiveMessageSize = 102400; // 100 KB
            options.StreamBufferCapacity = 10;
        });

        // Optional: Redis backplane for scaling (requires StackExchange.Redis NuGet package)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            // signalRBuilder.AddStackExchangeRedis(redisConnectionString);
            // Note: Uncomment the above line and add StackExchange.Redis package when scaling to multiple servers
        }

        return services;
    }
}
