using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Api;
using TaskManagement.Api.Data;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Features.Auth.Register;
using TaskManagement.Api.Features.Dashboard.GetBurndown;
using TaskManagement.Api.Features.Dashboard.GetProjectMetrics;
using TaskManagement.Api.Features.Dashboard.GetTeamActivity;
using TaskManagement.Api.Features.Projects.AcceptInvitation;
using TaskManagement.Api.Features.Projects.CreateProject;
using TaskManagement.Api.Features.Projects.InviteMember;
using TaskManagement.Api.Features.Tasks.CreateTask;
using TaskManagement.Api.Features.Tasks.UpdateTaskStatus;
using Xunit;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.IntegrationTests.Dashboard;

/// <summary>
/// Integration tests for dashboard metrics, burndown, and team activity endpoints.
/// </summary>
public class DashboardIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _ownerAccessToken = string.Empty;
    private string _ownerUserId = string.Empty;
    private string _memberAccessToken = string.Empty;
    private string _memberUserId = string.Empty;

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TaskManagementDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<TaskManagementDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("DashboardIntegrationTestDb");
                    });
                });
            });

        _client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        await RegisterTestUsersAsync();
    }

    public async System.Threading.Tasks.Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory != null)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
                await dbContext.Database.EnsureDeletedAsync();
            }

            _factory.Dispose();
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjectMetrics_ShouldReturnCountsAndTeamStats()
    {
        // Arrange
        var projectId = await CreateProjectWithMemberAsync();
        var (memberTaskId, ownerTaskId) = await CreateTasksAsync(projectId);

        await MarkTaskDoneAsync(memberTaskId, _memberAccessToken);
        await MarkTaskDoneAsync(ownerTaskId, _ownerAccessToken);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ownerAccessToken);

        // Act
        var response = await _client.GetAsync($"/api/v1/projects/{projectId}/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var metrics = await response.Content.ReadFromJsonAsync<ProjectMetricsResponse>();
        metrics.Should().NotBeNull();
        metrics!.ProjectId.Should().Be(projectId);
        metrics.TotalTasks.Should().Be(2);
        metrics.CompletedTasks.Should().Be(2);
        metrics.CompletionPercentage.Should().Be(100m);

        var memberStats = metrics.TeamMembers.Single(member => member.UserId == _memberUserId);
        memberStats.AssignedTasks.Should().Be(1);
        memberStats.CompletedTasks.Should().Be(1);

        var ownerStats = metrics.TeamMembers.Single(member => member.UserId == _ownerUserId);
        ownerStats.AssignedTasks.Should().Be(1);
        ownerStats.CompletedTasks.Should().Be(1);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetBurndown_ShouldReturnCompletionSeries()
    {
        // Arrange
        var projectId = await CreateProjectWithMemberAsync();
        var (memberTaskId, ownerTaskId) = await CreateTasksAsync(projectId);

        await MarkTaskDoneAsync(memberTaskId, _memberAccessToken);
        await MarkTaskDoneAsync(ownerTaskId, _ownerAccessToken);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ownerAccessToken);

        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.Date;

        // Act
        var response = await _client.GetAsync(
            $"/api/v1/projects/{projectId}/burndown?startDate={startDate:O}&endDate={endDate:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var burndown = await response.Content.ReadFromJsonAsync<BurndownResponse>();
        burndown.Should().NotBeNull();
        burndown!.ProjectId.Should().Be(projectId);
        burndown.TotalCompleted.Should().Be(2);
        burndown.Days.Should().HaveCount(1);
        burndown.Days.Single().CompletedTasks.Should().Be(2);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTeamActivity_ShouldReturnMemberActivity()
    {
        // Arrange
        var projectId = await CreateProjectWithMemberAsync();
        var (memberTaskId, ownerTaskId) = await CreateTasksAsync(projectId);

        await MarkTaskDoneAsync(memberTaskId, _memberAccessToken);
        await MarkTaskDoneAsync(ownerTaskId, _ownerAccessToken);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ownerAccessToken);

        // Act
        var response = await _client.GetAsync($"/api/v1/projects/{projectId}/team-activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activity = await response.Content.ReadFromJsonAsync<TeamActivityResponse>();
        activity.Should().NotBeNull();
        activity!.ProjectId.Should().Be(projectId);
        activity.TotalCompletedTasks.Should().Be(2);
        activity.Members.Should().Contain(member => member.UserId == _memberUserId && member.CompletedTasks == 1);
        activity.Members.Should().Contain(member => member.UserId == _ownerUserId && member.CompletedTasks == 1);
    }

    private async System.Threading.Tasks.Task RegisterTestUsersAsync()
    {
        var ownerRegister = new RegisterRequest
        {
            Email = "owner@example.com",
            Password = "OwnerPass123!@#",
            FirstName = "Project",
            LastName = "Owner"
        };

        var ownerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", ownerRegister);
        ownerResponse.EnsureSuccessStatusCode();
        var ownerData = await ownerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        _ownerAccessToken = ownerData!.AccessToken;
        _ownerUserId = ownerData.UserId;

        var memberRegister = new RegisterRequest
        {
            Email = "member@example.com",
            Password = "MemberPass123!@#",
            FirstName = "Team",
            LastName = "Member"
        };

        var memberResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", memberRegister);
        memberResponse.EnsureSuccessStatusCode();
        var memberData = await memberResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        _memberAccessToken = memberData!.AccessToken;
        _memberUserId = memberData.UserId;
    }

    private async System.Threading.Tasks.Task<int> CreateProjectWithMemberAsync()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ownerAccessToken);

        var createProjectRequest = new CreateProjectRequest
        {
            Name = "Dashboard Project",
            Description = "Dashboard integration test"
        };

        var createProjectResponse = await _client.PostAsJsonAsync("/api/v1/projects", createProjectRequest);
        createProjectResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>();
        project.Should().NotBeNull();

        var inviteRequest = new InviteMemberRequest
        {
            Email = "member@example.com",
            Role = ProjectRole.Member
        };

        var inviteResponse = await _client.PostAsJsonAsync(
            $"/api/v1/projects/{project!.Id}/invitations", inviteRequest);
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var inviteData = await inviteResponse.Content.ReadFromJsonAsync<InviteMemberResponse>();
        inviteData.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _memberAccessToken);

        var acceptResponse = await _client.PostAsync($"/api/v1/invitations/{inviteData!.Id}/accept", null);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var acceptData = await acceptResponse.Content.ReadFromJsonAsync<AcceptInvitationResponse>();
        acceptData.Should().NotBeNull();
        acceptData!.ProjectId.Should().Be(project.Id);

        return project.Id;
    }

    private async System.Threading.Tasks.Task<(int MemberTaskId, int OwnerTaskId)> CreateTasksAsync(int projectId)
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ownerAccessToken);

        var memberTaskRequest = new CreateTaskRequest
        {
            Title = "Member Task",
            Description = "Assigned to member",
            AssigneeId = _memberUserId,
            Priority = TaskPriority.Medium
        };

        var memberTaskResponse = await _client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/tasks", memberTaskRequest);
        memberTaskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var memberTask = await memberTaskResponse.Content.ReadFromJsonAsync<CreateTaskResponse>();
        memberTask.Should().NotBeNull();

        var ownerTaskRequest = new CreateTaskRequest
        {
            Title = "Owner Task",
            Description = "Assigned to owner",
            AssigneeId = _ownerUserId,
            Priority = TaskPriority.High
        };

        var ownerTaskResponse = await _client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/tasks", ownerTaskRequest);
        ownerTaskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var ownerTask = await ownerTaskResponse.Content.ReadFromJsonAsync<CreateTaskResponse>();
        ownerTask.Should().NotBeNull();

        return (memberTask!.Id, ownerTask!.Id);
    }

    private async System.Threading.Tasks.Task MarkTaskDoneAsync(int taskId, string accessToken)
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var request = new UpdateTaskStatusRequest
        {
            NewStatus = DomainTaskStatus.Done
        };

        var response = await _client.PatchAsJsonAsync($"/api/v1/tasks/{taskId}/status", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
