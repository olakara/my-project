using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Auth.Register;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Features.Auth;

public class RegisterServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<RegisterService>> _loggerMock;
    private readonly RegisterService _registerService;

    public RegisterServiceTests()
    {
        // Setup UserManager mock - using CreateMock helper pattern
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            null!,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

        // Setup JWT Token Service mock
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();

        // Setup Logger mock
        _loggerMock = new Mock<ILogger<RegisterService>>();

        // Create service instance
        _registerService = new RegisterService(
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object);
    }

    #region Valid Registration Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldSuccessfullyRegisterUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123!@#",
            FirstName = "John",
            LastName = "Doe"
        };

        var userId = "user-123";

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .Callback<ApplicationUser, string>((user, password) => user.Id = userId)
            .ReturnsAsync(IdentityResult.Success);

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("access-token-123");

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateRefreshToken())
            .Returns("refresh-token-123");

        _userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _registerService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.AccessToken.Should().Be("access-token-123");
        result.RefreshToken.Should().Be("refresh-token-123");
        result.RefreshTokenExpiry.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));

        // Verify interactions
        _userManagerMock.Verify(
            um => um.CreateAsync(It.Is<ApplicationUser>(u => u.Email == request.Email), request.Password),
            Times.Once);
        _jwtTokenServiceMock.Verify(jwt => jwt.GenerateAccessToken(It.IsAny<ApplicationUser>()), Times.Once);
        _jwtTokenServiceMock.Verify(jwt => jwt.GenerateRefreshToken(), Times.Once);
        _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldLogRegistrationSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123!@#",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var userId = "user-456";

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .Callback<ApplicationUser, string>((user, password) => user.Id = userId)
            .ReturnsAsync(IdentityResult.Success);

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("access-token");

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateRefreshToken())
            .Returns("refresh-token");

        _userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _registerService.RegisterAsync(request);

        // Assert - Verify that success was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"User {userId} registered successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Duplicate Email Validation Tests

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePass123!@#",
            FirstName = "John",
            LastName = "Duplicate"
        };

        var identityError = new IdentityError
        {
            Code = "DuplicateUserName",
            Description = "Username 'existing@example.com' is already taken."
        };

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _registerService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldLogWarning()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePass123!@#",
            FirstName = "John",
            LastName = "Duplicate"
        };

        var identityError = new IdentityError
        {
            Code = "DuplicateUserName",
            Description = "Username 'existing@example.com' is already taken."
        };

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act & Assert
        try
        {
            await _registerService.RegisterAsync(request);
        }
        catch { }

        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"User registration failed for email {request.Email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Password Complexity Validation Tests

    [Theory]
    [InlineData("short")]  // Too short, no complexity
    [InlineData("12345678")]  // 8 chars, no complexity
    [InlineData("password123")]  // No uppercase or special char
    [InlineData("PASSWORD123")]  // No lowercase or special char
    [InlineData("Password")]  // No number or special char
    public async Task RegisterAsync_WithWeakPassword_ShouldThrowInvalidOperationException(string weakPassword)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = weakPassword,
            FirstName = "Test",
            LastName = "User"
        };

        var identityError = new IdentityError
        {
            Code = "PasswordTooShort",
            Description = "Passwords must be at least 12 characters."
        };

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _registerService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WithValidStrongPassword_ShouldSucceed()
    {
        // Arrange
        var validStrongPasswords = new[]
        {
            "SecurePass123!@#",
            "MyPassword2024!",
            "Complex#Pass999",
            "Str0ng&Secure"
        };

        foreach (var password in validStrongPasswords)
        {
            var request = new RegisterRequest
            {
                Email = $"user-{Guid.NewGuid()}@example.com",
                Password = password,
                FirstName = "Test",
                LastName = "User"
            };

            _userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .Callback<ApplicationUser, string>((user, pwd) => user.Id = Guid.NewGuid().ToString())
                .ReturnsAsync(IdentityResult.Success);

            _jwtTokenServiceMock
                .Setup(jwt => jwt.GenerateAccessToken(It.IsAny<ApplicationUser>()))
                .Returns("access-token");

            _jwtTokenServiceMock
                .Setup(jwt => jwt.GenerateRefreshToken())
                .Returns("refresh-token");

            _userManagerMock
                .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _registerService.RegisterAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
        }
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordValidationError_ShouldIncludeErrorMessage()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "weak",
            FirstName = "Test",
            LastName = "User"
        };

        var errors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 12 characters." },
            new IdentityError { Code = "PasswordRequiresNonAlphanumeric", Description = "Passwords must have at least one non-alphanumeric character." }
        };

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _registerService.RegisterAsync(request));

        // Assert
        ex.Message.Should().Contain("Passwords must be at least 12 characters.");
        ex.Message.Should().Contain("Passwords must have at least one non-alphanumeric character.");
    }

    #endregion

    #region Additional Validation Tests

    [Fact]
    public async Task RegisterAsync_ShouldSetEmailConfirmedToTrue()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123!@#",
            FirstName = "John",
            LastName = "Doe"
        };

        ApplicationUser? capturedUser = null;

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, pwd) =>
            {
                capturedUser = user;
                user.Id = "user-123";
            })
            .ReturnsAsync(IdentityResult.Success);

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("access-token");

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateRefreshToken())
            .Returns("refresh-token");

        _userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _registerService.RegisterAsync(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.EmailConfirmed.Should().BeTrue();
        capturedUser.UserName.Should().Be(request.Email);
    }

    [Fact]
    public async Task RegisterAsync_ShouldStoreRefreshTokenWithExpiry()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePass123!@#",
            FirstName = "John",
            LastName = "Doe"
        };

        ApplicationUser? updatedUser = null;

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, pwd) => user.Id = "user-123")
            .ReturnsAsync(IdentityResult.Success);

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("access-token");

        _jwtTokenServiceMock
            .Setup(jwt => jwt.GenerateRefreshToken())
            .Returns("refresh-token-123");

        _userManagerMock
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(user => updatedUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var beforeRegistration = DateTime.UtcNow;

        // Act
        await _registerService.RegisterAsync(request);

        var afterRegistration = DateTime.UtcNow;

        // Assert
        updatedUser.Should().NotBeNull();
        updatedUser!.RefreshToken.Should().Be("refresh-token-123");
        updatedUser.RefreshTokenExpiry.Should().NotBeNull();
        updatedUser.RefreshTokenExpiry!.Value.Should().BeOnOrAfter(beforeRegistration.AddDays(7));
        updatedUser.RefreshTokenExpiry.Value.Should().BeOnOrBefore(afterRegistration.AddDays(7));
    }

    #endregion
}
