using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<JwtTokenService>> _loggerMock;
    private readonly JwtTokenService _jwtTokenService;
    private readonly string _testSecret = "this-is-a-very-secure-secret-key-for-testing-purposes-with-at-least-256-bits";
    private readonly string _testIssuer = "TestIssuer";
    private readonly string _testAudience = "TestAudience";

    public JwtTokenServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<JwtTokenService>>();

        // Setup configuration mocks with default values
        SetupConfiguration();

        _jwtTokenService = new JwtTokenService(_configurationMock.Object, _loggerMock.Object);
    }

    private void SetupConfiguration(int? expirationMinutes = null)
    {
        _configurationMock.Setup(c => c["Jwt:Secret"]).Returns(_testSecret);
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns(_testIssuer);
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns(_testAudience);
        _configurationMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"])
            .Returns((expirationMinutes ?? 15).ToString());
    }

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        // Validate token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.CanReadToken(token).Should().BeTrue();

        var jwtToken = tokenHandler.ReadJwtToken(token);
        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be(_testIssuer);
        jwtToken.Audiences.Should().Contain(_testAudience);
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeCorrectClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-456",
            Email = "jane@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var claims = jwtToken.Claims.ToList();
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
        claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "Jane Smith");
        claims.Should().Contain(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub && c.Value == user.Id);
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var expirationMinutes = 30;
        SetupConfiguration(expirationMinutes);
        var jwtTokenService = new JwtTokenService(_configurationMock.Object, _loggerMock.Object);

        var user = new ApplicationUser
        {
            Id = "user-789",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = jwtTokenService.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(expirationMinutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_WithDefaultExpiration_ShouldUse15Minutes()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-default",
            Email = "default@example.com",
            FirstName = "Default",
            LastName = "User"
        };

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(15);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_WithNullEmail_ShouldUseEmptyString()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-no-email",
            Email = null,
            FirstName = "No",
            LastName = "Email"
        };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void GenerateAccessToken_WithMissingSecret_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Secret"]).Returns((string?)null);
        configMock.Setup(c => c["Jwt:Issuer"]).Returns(_testIssuer);
        configMock.Setup(c => c["Jwt:Audience"]).Returns(_testAudience);

        var jwtTokenService = new JwtTokenService(configMock.Object, _loggerMock.Object);

        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        Action act = () => jwtTokenService.GenerateAccessToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT Secret not configured");
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        
        // Verify it's a valid Base64 string by trying to decode it
        Action act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturn64ByteToken()
    {
        // Act
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Assert
        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Length.Should().Be(64);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var token1 = _jwtTokenService.GenerateRefreshToken();
        var token2 = _jwtTokenService.GenerateRefreshToken();
        var token3 = _jwtTokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateRefreshToken_MultipleInvocations_ShouldAllReturnValidTokens()
    {
        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            var token = _jwtTokenService.GenerateRefreshToken();
            token.Should().NotBeNullOrEmpty();
            
            var bytes = Convert.FromBase64String(token);
            bytes.Length.Should().Be(64);
        }
    }

    #endregion

    #region GetPrincipalFromExpiredToken Tests

    [Fact]
    public void GetPrincipalFromExpiredToken_WithValidExpiredToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-expired",
            Email = "expired@example.com",
            FirstName = "Expired",
            LastName = "Token"
        };

        // Generate a token (even if not expired, this test validates the validation logic works)
        var token = _jwtTokenService.GenerateAccessToken(user);

        // Act
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal.Claims.Should().NotBeEmpty();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldExtractCorrectClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-claims-test",
            Email = "claims@example.com",
            FirstName = "Claims",
            LastName = "Test"
        };

        var token = _jwtTokenService.GenerateAccessToken(user);

        // Act
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        var claims = principal.Claims.ToList();
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
        claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "Claims Test");
        // Note: "sub" claim is mapped to NameIdentifier by JWT library, so we verify NameIdentifier instead
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithInvalidToken_ShouldThrowException()
    {
        // Arrange
        var invalidToken = "this.is.not.a.valid.jwt.token";

        // Act
        Action act = () => _jwtTokenService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithEmptyToken_ShouldThrowException()
    {
        // Arrange
        var emptyToken = string.Empty;

        // Act
        Action act = () => _jwtTokenService.GetPrincipalFromExpiredToken(emptyToken);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithTokenSignedByDifferentSecret_ShouldThrowSecurityTokenException()
    {
        // Arrange
        var differentConfigMock = new Mock<IConfiguration>();
        differentConfigMock.Setup(c => c["Jwt:Secret"]).Returns("different-secret-key-that-is-long-enough-to-be-valid-for-hmacsha256-algorithm");
        differentConfigMock.Setup(c => c["Jwt:Issuer"]).Returns(_testIssuer);
        differentConfigMock.Setup(c => c["Jwt:Audience"]).Returns(_testAudience);
        differentConfigMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("15");

        var differentJwtService = new JwtTokenService(differentConfigMock.Object, _loggerMock.Object);

        var user = new ApplicationUser
        {
            Id = "user-different-secret",
            Email = "different@example.com",
            FirstName = "Different",
            LastName = "Secret"
        };

        // Generate token with different secret
        var token = differentJwtService.GenerateAccessToken(user);

        // Act - Try to validate with original service (different secret)
        Action act = () => _jwtTokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        act.Should().Throw<Microsoft.IdentityModel.Tokens.SecurityTokenSignatureKeyNotFoundException>();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithWrongIssuer_ShouldThrowSecurityTokenException()
    {
        // Arrange
        var differentConfigMock = new Mock<IConfiguration>();
        differentConfigMock.Setup(c => c["Jwt:Secret"]).Returns(_testSecret);
        differentConfigMock.Setup(c => c["Jwt:Issuer"]).Returns("DifferentIssuer");
        differentConfigMock.Setup(c => c["Jwt:Audience"]).Returns(_testAudience);
        differentConfigMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("15");

        var differentJwtService = new JwtTokenService(differentConfigMock.Object, _loggerMock.Object);

        var user = new ApplicationUser
        {
            Id = "user-wrong-issuer",
            Email = "issuer@example.com",
            FirstName = "Wrong",
            LastName = "Issuer"
        };

        // Generate token with different issuer
        var token = differentJwtService.GenerateAccessToken(user);

        // Act - Try to validate with original service (expecting different issuer)
        Action act = () => _jwtTokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        act.Should().Throw<Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException>();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithWrongAudience_ShouldThrowSecurityTokenException()
    {
        // Arrange
        var differentConfigMock = new Mock<IConfiguration>();
        differentConfigMock.Setup(c => c["Jwt:Secret"]).Returns(_testSecret);
        differentConfigMock.Setup(c => c["Jwt:Issuer"]).Returns(_testIssuer);
        differentConfigMock.Setup(c => c["Jwt:Audience"]).Returns("DifferentAudience");
        differentConfigMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("15");

        var differentJwtService = new JwtTokenService(differentConfigMock.Object, _loggerMock.Object);

        var user = new ApplicationUser
        {
            Id = "user-wrong-audience",
            Email = "audience@example.com",
            FirstName = "Wrong",
            LastName = "Audience"
        };

        // Generate token with different audience
        var token = differentJwtService.GenerateAccessToken(user);

        // Act - Try to validate with original service (expecting different audience)
        Action act = () => _jwtTokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        act.Should().Throw<Microsoft.IdentityModel.Tokens.SecurityTokenInvalidAudienceException>();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithMissingSecret_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Secret"]).Returns((string?)null);
        configMock.Setup(c => c["Jwt:Issuer"]).Returns(_testIssuer);
        configMock.Setup(c => c["Jwt:Audience"]).Returns(_testAudience);

        var jwtTokenService = new JwtTokenService(configMock.Object, _loggerMock.Object);

        var token = "some.token.here";

        // Act
        Action act = () => jwtTokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT Secret not configured");
    }

    #endregion

    #region Token Rotation Integration Tests

    [Fact]
    public void TokenRotation_GenerateAndValidate_ShouldWorkCorrectly()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-rotation",
            Email = "rotation@example.com",
            FirstName = "Token",
            LastName = "Rotation"
        };

        // Act - Generate access token
        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        // Act - Generate refresh token
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Act - Validate access token (even if not expired)
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken);

        // Assert
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
        principal.Should().NotBeNull();
        
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void TokenRotation_MultipleRefreshTokens_ShouldBeUnique()
    {
        // Arrange
        var tokens = new List<string>();

        // Act - Generate multiple refresh tokens
        for (int i = 0; i < 10; i++)
        {
            tokens.Add(_jwtTokenService.GenerateRefreshToken());
        }

        // Assert - All tokens should be unique
        tokens.Distinct().Count().Should().Be(10);
    }

    [Fact]
    public void FullAuthFlow_GenerateAccessAndRefresh_ThenValidate_ShouldSucceed()
    {
        // Arrange
        var originalUser = new ApplicationUser
        {
            Id = "user-full-flow",
            Email = "fullflow@example.com",
            FirstName = "Full",
            LastName = "Flow"
        };

        // Act - Step 1: Generate tokens on login
        var accessToken1 = _jwtTokenService.GenerateAccessToken(originalUser);
        var refreshToken1 = _jwtTokenService.GenerateRefreshToken();

        // Act - Step 2: Extract claims from expired access token
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken1);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Act - Step 3: Wait a moment to ensure different timestamp, then generate new tokens after refresh
        System.Threading.Thread.Sleep(1000); // Wait 1 second to ensure different exp timestamp
        var newUser = new ApplicationUser
        {
            Id = userId!,
            Email = originalUser.Email,
            FirstName = originalUser.FirstName,
            LastName = originalUser.LastName
        };
        var accessToken2 = _jwtTokenService.GenerateAccessToken(newUser);
        var refreshToken2 = _jwtTokenService.GenerateRefreshToken();

        // Assert
        accessToken1.Should().NotBe(accessToken2, "new access token should be different");
        refreshToken1.Should().NotBe(refreshToken2, "refresh tokens should rotate");
        
        var principal2 = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken2);
        var userId2 = principal2.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        userId2.Should().Be(originalUser.Id, "user ID should remain consistent");
    }

    #endregion
}
