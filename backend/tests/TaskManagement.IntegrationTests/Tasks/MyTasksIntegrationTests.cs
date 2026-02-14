using System.Net;
using System.Net.Http.Json;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskManagement.Api;
using TaskManagement.Api.Data;
using TaskManagement.Api.Features.Auth.Register;
using TaskManagement.Api.Features.Projects.AcceptInvitation;
using TaskManagement.Api.Features.Projects.CreateProject;
using TaskManagement.Api.Features.Projects.InviteMember;
using TaskManagement.Api.Features.Tasks.AssignTask;
using TaskManagement.Api.Features.Tasks.CreateTask;
using TaskManagement.Api.Features.Tasks.GetMyTasks;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.IntegrationTests.Tasks;

/// <summary>
/// Integration tests for assignment flow and my tasks query endpoint.
/// </summary>
public class MyTasksIntegrationTests : IAsyncLifetime
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
                        options.UseInMemoryDatabase("MyTasksIntegrationTestDb");
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
    public async System.Threading.Tasks.Task GetMyTasks_AssignedTasks_ShouldReturnOnlyAssignedTasks()
    {
        // Arrange: Owner creates project
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ownerAccessToken);

        var createProjectRequest = new CreateProjectRequest
        {
            Name = "My Tasks Project",
            Description = "My tasks integration test"
        };

        var createProjectResponse = await _client.PostAsJsonAsync("/api/v1/projects", createProjectRequest);
        createProjectResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdProject = await createProjectResponse.Content.ReadFromJsonAsync<CreateProjectResponse>();
        createdProject.Should().NotBeNull();
        var projectId = createdProject!.Id;

        // Arrange: Invite member and accept invitation
        var inviteRequest = new InviteMemberRequest
        {
            Email = "member@example.com",
            Role = ProjectRole.Member
        };

        var inviteResponse = await _client.PostAsJsonAsync($"/api/v1/projects/{projectId}/invitations", inviteRequest);
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var inviteData = await inviteResponse.Content.ReadFromJsonAsync<InviteMemberResponse>();
        inviteData.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _memberAccessToken);

        var acceptResponse = await _client.PostAsync($"/api/v1/invitations/{inviteData!.Id}/accept", null);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var acceptData = await acceptResponse.Content.ReadFromJsonAsync<AcceptInvitationResponse>();
        acceptData.Should().NotBeNull();
        acceptData!.ProjectId.Should().Be(projectId);

        // Arrange: Owner creates task and assigns to member
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _ownerAccessToken);

        var createTaskRequest = new CreateTaskRequest
        {
            Title = "Assigned Task",
            Description = "Task to be assigned"
        };

        var createTaskResponse = await _client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", createTaskRequest);
        createTaskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await createTaskResponse.Content.ReadFromJsonAsync<CreateTaskResponse>();
        createdTask.Should().NotBeNull();

        var assignRequest = new AssignTaskRequest
        {
            AssigneeId = _memberUserId
        };

        var assignResponse = await _client.PatchAsJsonAsync($"/api/v1/tasks/{createdTask!.Id}/assign", assignRequest);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: Member requests my tasks
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _memberAccessToken);

        var myTasksResponse = await _client.GetAsync("/api/v1/tasks/my-tasks");

        // Assert
        myTasksResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await myTasksResponse.Content.ReadFromJsonAsync<List<GetMyTasksResponse>>();
        tasks.Should().NotBeNull();
        tasks!.Should().ContainSingle(t => t.Id == createdTask.Id);
        tasks.First().ProjectId.Should().Be(projectId);
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
}
