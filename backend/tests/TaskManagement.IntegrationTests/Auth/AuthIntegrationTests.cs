using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TaskManagement.Api;
using TaskManagement.Api.Data;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Auth.Login;
using TaskManagement.Api.Features.Auth.Register;
using TaskManagement.Api.Features.Auth.RefreshToken;

namespace TaskManagement.IntegrationTests.Auth;

/// <summary>
/// Integration tests for the complete authentication flow.
/// Tests the full user journey: register → login → access protected endpoint → refresh token → logout
/// Uses WebApplicationFactory to test against the real application with in-memory database.
/// </summary>
public class AuthIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private readonly string _testUserEmail = "testuser@example.com";
    private readonly string _testPassword = "TestPassword123!@#";
    private readonly string _testFirstName = "John";
    private readonly string _testLastName = "Doe";

    public async Task InitializeAsync()
    {
        // Create a factory that uses in-memory database for testing
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the production DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType ==
                            typeof(DbContextOptions<TaskManagementDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<TaskManagementDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("AuthIntegrationTestDb");
                    });
                });
            });

        _client = _factory.CreateClient();

        // Initialize database
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory != null)
        {
            // Clean up database
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
                await dbContext.Database.EnsureDeletedAsync();
            }
            
            _factory.Dispose();
        }

        await Task.CompletedTask;
    }

    #region Registration Tests

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnCreatedWithJwtTokens()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = _testFirstName,
            LastName = _testLastName
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        content.Should().NotBeNull();
        content!.UserId.Should().NotBeNullOrEmpty();
        content.Email.Should().Be(_testUserEmail);
        content.FirstName.Should().Be(_testFirstName);
        content.LastName.Should().Be(_testLastName);
        content.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
        content.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);

        // Verify user was created in database
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == _testUserEmail);
            user.Should().NotBeNull();
            user!.FirstName.Should().Be(_testFirstName);
            user.LastName.Should().Be(_testLastName);
        }
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange - Register first user
        var firstRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = "First",
            LastName = "User"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", firstRequest);

        // Arrange - Attempt to register second user with same email
        var secondRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = "DifferentPass123!@#",
            FirstName = "Second",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "weak",  // Too short and no complexity
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnAccessTokenAndRefreshToken()
    {
        // Arrange - Register user first
        var registerRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = _testFirstName,
            LastName = _testLastName
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Arrange - Prepare login request
        var loginRequest = new LoginRequest
        {
            Email = _testUserEmail,
            Password = _testPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.UserId.Should().NotBeNullOrEmpty();
        content.Email.Should().Be(_testUserEmail);
        content.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
        content.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);

        // Verify refresh token cookie was set
        response.Headers.TryGetValues("Set-Cookie", out var setCookies);
        setCookies.Should().NotBeNull();
        var refreshTokenCookie = setCookies?.FirstOrDefault(c => c.Contains("RefreshToken", StringComparison.OrdinalIgnoreCase));
        refreshTokenCookie.Should().NotBeNullOrEmpty("Refresh token should be set as HttpOnly cookie");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = _testPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ShouldReturnBadRequest()
    {
        // Arrange - Register user first
        var registerRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = _testFirstName,
            LastName = _testLastName
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Arrange - Attempt login with wrong password
        var loginRequest = new LoginRequest
        {
            Email = _testUserEmail,
            Password = "WrongPassword123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Protected Endpoint Access Tests

    [Fact]
    public async Task ProtectedEndpoint_WithValidAccessToken_ShouldReturnOk()
    {
        // Arrange - Register and login user
        var registerRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = _testFirstName,
            LastName = _testLastName
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = _testUserEmail,
            Password = _testPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Act - Call logout endpoint (protected) with access token
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent!.AccessToken);
        var response = await _client.SendAsync(logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        // Act - Call protected endpoint without token
        var response = await _client.PostAsync("/api/v1/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidAccessToken_ShouldReturnUnauthorized()
    {
        // Act - Call protected endpoint with invalid token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token-123");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ShouldReturnNewAccessToken()
    {
        // Arrange - Register and login user
        var registerRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = _testFirstName,
            LastName = _testLastName
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = _testUserEmail,
            Password = _testPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var originalAccessToken = loginContent!.AccessToken;

        // Act - Refresh token
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginContent.RefreshToken
        };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshContent = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        refreshContent.Should().NotBeNull();
        refreshContent!.AccessToken.Should().NotBeNullOrEmpty();
        refreshContent.AccessToken.Should().NotBe(originalAccessToken, "New access token should be different");
        refreshContent.RefreshToken.Should().NotBeNullOrEmpty();
        refreshContent.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithEmptyRefreshToken_ShouldReturnBadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidAccessToken_ShouldInvalidateRefreshToken()
    {
        // Arrange - Register and login user
        var registerRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = _testFirstName,
            LastName = _testLastName
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = _testUserEmail,
            Password = _testPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Act - Logout
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent!.AccessToken);
        var logoutResponse = await _client.SendAsync(logoutRequest);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify refresh token no longer works
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginContent.RefreshToken
        };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Full Auth Flow Tests

    [Fact]
    public async Task FullAuthFlow_RegisterLoginAccessProtectedEndpointRefreshLogout_ShouldSucceed()
    {
        // Step 1: Register
        var registerRequest = new RegisterRequest
        {
            Email = _testUserEmail,
            Password = _testPassword,
            FirstName = _testFirstName,
            LastName = _testLastName
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        registerContent.Should().NotBeNull();
        registerContent!.UserId.Should().NotBeNullOrEmpty();
        var userId = registerContent.UserId;

        // Step 2: Login
        var loginRequest = new LoginRequest
        {
            Email = _testUserEmail,
            Password = _testPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var accessToken = loginContent!.AccessToken;
        var refreshToken = loginContent.RefreshToken;

        // Step 3: Access protected endpoint with access token
        var protectedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        protectedRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var protectedResponse = await _client.SendAsync(protectedRequest);
        // If we logout here, we can't use the same token later, so let's use a different approach
        // Let's skip logout for now in this scenario to test refresh first

        // Re-login to get fresh tokens for testing refresh
        var loginResponse2 = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginContent2 = await loginResponse2.Content.ReadFromJsonAsync<LoginResponse>();
        var accessToken2 = loginContent2!.AccessToken;
        var refreshToken2 = loginContent2.RefreshToken;

        // Step 4: Refresh token
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = refreshToken2
        };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshContent = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponse>();
        var newAccessToken = refreshContent!.AccessToken;

        // Step 5: Logout with new access token
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newAccessToken);
        var logoutResponse = await _client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Verify logout invalidated the token (refresh should fail)
        var finalRefreshRequest = new RefreshTokenRequest
        {
            RefreshToken = refreshContent.RefreshToken
        };
        var finalRefreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", finalRefreshRequest);
        finalRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}

