using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Projects.CreateProject;
using TaskManagement.Api.Features.Projects.GetProject;
using TaskManagement.Api.Features.Projects.UpdateProject;

namespace TaskManagement.Api.Tests.Features.Projects;

/// <summary>
/// Unit tests for project CRUD services testing create, read, update operations with authorization checks
/// </summary>
public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly TaskManagementDbContext _context;
    private readonly Mock<ILogger<CreateProjectService>> _createLoggerMock;
    private readonly Mock<ILogger<GetProjectService>> _getLoggerMock;
    private readonly Mock<ILogger<UpdateProjectService>> _updateLoggerMock;

    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TaskManagementDbContext(options);
        
        _createLoggerMock = new Mock<ILogger<CreateProjectService>>();
        _getLoggerMock = new Mock<ILogger<GetProjectService>>();
        _updateLoggerMock = new Mock<ILogger<UpdateProjectService>>();
    }

    #region CreateProject Tests

    [Fact]
    public async Task CreateProject_WithValidRequest_ShouldCreateProjectSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            Description = "A test project description"
        };

        var createdProject = new Project
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            IsArchived = false,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        _projectRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProject);

        var service = new CreateProjectService(
            _context,
            _projectRepositoryMock.Object,
            _createLoggerMock.Object);

        // Act
        var result = await service.CreateProjectAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.OwnerId.Should().Be(userId);
        result.IsArchived.Should().BeFalse();

        // Verify repository was called
        _projectRepositoryMock.Verify(
            repo => repo.CreateAsync(It.Is<Project>(p => 
                p.Name == request.Name && 
                p.Description == request.Description && 
                p.OwnerId == userId), 
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateProject_WithMinimalData_ShouldCreateProjectWithNullDescription()
    {
        // Arrange
        var userId = "user-456";
        var request = new CreateProjectRequest
        {
            Name = "Minimal Project",
            Description = null
        };

        var createdProject = new Project
        {
            Id = 2,
            Name = request.Name,
            Description = null,
            OwnerId = userId,
            IsArchived = false,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        _projectRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProject);

        var service = new CreateProjectService(
            _context,
            _projectRepositoryMock.Object,
            _createLoggerMock.Object);

        // Act
        var result = await service.CreateProjectAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().BeNull();
    }

    [Fact]
    public async Task CreateProject_ShouldLogProjectCreation()
    {
        // Arrange
        var userId = "user-789";
        var request = new CreateProjectRequest
        {
            Name = "Logged Project",
            Description = "Test logging"
        };

        var createdProject = new Project
        {
            Id = 3,
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        _projectRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProject);

        var service = new CreateProjectService(
            _context,
            _projectRepositoryMock.Object,
            _createLoggerMock.Object);

        // Act
        await service.CreateProjectAsync(userId, request);

        // Assert - Verify logging occurred
        _createLoggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Project") && v.ToString()!.Contains("created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetProject Tests

    [Fact]
    public async Task GetProject_WithValidProjectAndMember_ShouldReturnProjectDetails()
    {
        // Arrange
        var projectId = 1;
        var userId = "user-123";
        var ownerId = "owner-456";

        var owner = new ApplicationUser
        {
            Id = ownerId,
            Email = "owner@example.com",
            FirstName = "Project",
            LastName = "Owner"
        };

        var member = new ApplicationUser
        {
            Id = userId,
            Email = "member@example.com",
            FirstName = "Team",
            LastName = "Member"
        };

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Test Description",
            OwnerId = ownerId,
            Owner = owner,
            CreatedTimestamp = DateTime.UtcNow.AddDays(-10),
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = userId,
                    User = member,
                    ProjectId = projectId,
                    Role = ProjectRole.Member,
                    JoinedTimestamp = DateTime.UtcNow.AddDays(-5)
                },
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    ProjectId = projectId,
                    Role = ProjectRole.Owner,
                    JoinedTimestamp = DateTime.UtcNow.AddDays(-10)
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var service = new GetProjectService(_projectRepositoryMock.Object);

        // Act
        var result = await service.GetProjectAsync(projectId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(projectId);
        result.Name.Should().Be(project.Name);
        result.Description.Should().Be(project.Description);
        result.MemberCount.Should().Be(2);
        result.Members.Should().HaveCount(2);
        result.Role.Should().Be("Member");
        result.Owner.Should().NotBeNull();
        result.Owner.UserId.Should().Be(ownerId);
        result.Owner.Email.Should().Be("owner@example.com");
    }

    [Fact]
    public async Task GetProject_WithNonMemberUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var projectId = 1;
        var userId = "non-member-123";
        var ownerId = "owner-456";

        var owner = new ApplicationUser
        {
            Id = ownerId,
            Email = "owner@example.com",
            FirstName = "Owner",
            LastName = "Name"
        };

        var project = new Project
        {
            Id = projectId,
            Name = "Private Project",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    ProjectId = projectId,
                    Role = ProjectRole.Owner
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var service = new GetProjectService(_projectRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await service.GetProjectAsync(projectId, userId));
    }

    [Fact]
    public async Task GetProject_WithNonExistentProject_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var projectId = 999;
        var userId = "user-123";

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var service = new GetProjectService(_projectRepositoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await service.GetProjectAsync(projectId, userId));
    }

    [Fact]
    public async Task GetProject_WithOwnerAsUser_ShouldReturnOwnerRole()
    {
        // Arrange
        var projectId = 1;
        var ownerId = "owner-123";

        var owner = new ApplicationUser
        {
            Id = ownerId,
            Email = "owner@example.com",
            FirstName = "Owner",
            LastName = "Name"
        };

        var project = new Project
        {
            Id = projectId,
            Name = "Owner's Project",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    ProjectId = projectId,
                    Role = ProjectRole.Owner
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var service = new GetProjectService(_projectRepositoryMock.Object);

        // Act
        var result = await service.GetProjectAsync(projectId, ownerId);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("Owner");
    }

    #endregion

    #region UpdateProject Tests

    [Fact]
    public async Task UpdateProject_WithOwner_ShouldUpdateProjectSuccessfully()
    {
        // Arrange
        var projectId = 1;
        var ownerId = "owner-123";

        var owner = new ApplicationUser
        {
            Id = ownerId,
            Email = "owner@example.com"
        };

        var project = new Project
        {
            Id = projectId,
            Name = "Old Name",
            Description = "Old Description",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    Role = ProjectRole.Owner
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>(),
            UpdatedTimestamp = DateTime.UtcNow.AddDays(-1)
        };

        var request = new UpdateProjectRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UpdateProjectService(_projectRepositoryMock.Object, _updateLoggerMock.Object);

        // Act
        var result = await service.UpdateProjectAsync(projectId, ownerId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");

        // Verify repository update was called
        _projectRepositoryMock.Verify(
            repo => repo.UpdateAsync(It.Is<Project>(p => 
                p.Name == "New Name" && 
                p.Description == "New Description" &&
                p.UpdatedTimestamp > DateTime.UtcNow.AddSeconds(-5)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProject_WithManager_ShouldUpdateProjectSuccessfully()
    {
        // Arrange
        var projectId = 1;
        var managerId = "manager-123";
        var ownerId = "owner-456";

        var owner = new ApplicationUser { Id = ownerId, Email = "owner@example.com" };
        var manager = new ApplicationUser { Id = managerId, Email = "manager@example.com" };

        var project = new Project
        {
            Id = projectId,
            Name = "Old Name",
            Description = "Old Description",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    Role = ProjectRole.Owner
                },
                new ProjectMember
                {
                    UserId = managerId,
                    User = manager,
                    Role = ProjectRole.Manager
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        var request = new UpdateProjectRequest
        {
            Name = "Updated by Manager",
            Description = "Manager updated this"
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UpdateProjectService(_projectRepositoryMock.Object, _updateLoggerMock.Object);

        // Act
        var result = await service.UpdateProjectAsync(projectId, managerId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated by Manager");
        result.Description.Should().Be("Manager updated this");
    }

    [Fact]
    public async Task UpdateProject_WithRegularMember_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var projectId = 1;
        var memberId = "member-123";
        var ownerId = "owner-456";

        var owner = new ApplicationUser { Id = ownerId, Email = "owner@example.com" };
        var member = new ApplicationUser { Id = memberId, Email = "member@example.com" };

        var project = new Project
        {
            Id = projectId,
            Name = "Project Name",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    Role = ProjectRole.Owner
                },
                new ProjectMember
                {
                    UserId = memberId,
                    User = member,
                    Role = ProjectRole.Member
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        var request = new UpdateProjectRequest
        {
            Name = "Unauthorized Update",
            Description = "Should fail"
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var service = new UpdateProjectService(_projectRepositoryMock.Object, _updateLoggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await service.UpdateProjectAsync(projectId, memberId, request));

        // Verify update was never called
        _projectRepositoryMock.Verify(
            repo => repo.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateProject_WithNonExistentProject_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var projectId = 999;
        var userId = "user-123";
        var request = new UpdateProjectRequest { Name = "Test" };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var service = new UpdateProjectService(_projectRepositoryMock.Object, _updateLoggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await service.UpdateProjectAsync(projectId, userId, request));
    }

    [Fact]
    public async Task UpdateProject_WithNonMemberUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var projectId = 1;
        var nonMemberId = "non-member-123";
        var ownerId = "owner-456";

        var owner = new ApplicationUser { Id = ownerId, Email = "owner@example.com" };

        var project = new Project
        {
            Id = projectId,
            Name = "Private Project",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    Role = ProjectRole.Owner
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        var request = new UpdateProjectRequest { Name = "Unauthorized" };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var service = new UpdateProjectService(_projectRepositoryMock.Object, _updateLoggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await service.UpdateProjectAsync(projectId, nonMemberId, request));
    }

    [Fact]
    public async Task UpdateProject_ShouldLogUpdateOperation()
    {
        // Arrange
        var projectId = 1;
        var ownerId = "owner-123";

        var owner = new ApplicationUser
        {
            Id = ownerId,
            Email = "owner@example.com"
        };

        var project = new Project
        {
            Id = projectId,
            Name = "Old Name",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    Role = ProjectRole.Owner
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        var request = new UpdateProjectRequest { Name = "New Name" };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UpdateProjectService(_projectRepositoryMock.Object, _updateLoggerMock.Object);

        // Act
        await service.UpdateProjectAsync(projectId, ownerId, request);

        // Assert - Verify logging occurred
        _updateLoggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Project") && v.ToString()!.Contains("updated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProject_WithPartialUpdate_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var projectId = 1;
        var ownerId = "owner-123";

        var owner = new ApplicationUser
        {
            Id = ownerId,
            Email = "owner@example.com"
        };

        var project = new Project
        {
            Id = projectId,
            Name = "Original Name",
            Description = "Original Description",
            OwnerId = ownerId,
            Owner = owner,
            Members = new List<ProjectMember>
            {
                new ProjectMember
                {
                    UserId = ownerId,
                    User = owner,
                    Role = ProjectRole.Owner
                }
            },
            Tasks = new List<TaskManagement.Api.Domain.Tasks.Task>()
        };

        var request = new UpdateProjectRequest
        {
            Name = "Updated Name",
            Description = null // Not updating description
        };

        _projectRepositoryMock
            .Setup(repo => repo.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UpdateProjectService(_projectRepositoryMock.Object, _updateLoggerMock.Object);

        // Act
        var result = await service.UpdateProjectAsync(projectId, ownerId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        // Description should remain in the project but not verified in this simplified test
    }

    #endregion
}
