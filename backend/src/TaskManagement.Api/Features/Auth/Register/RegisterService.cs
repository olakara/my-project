using Microsoft.AspNetCore.Identity;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Features.Auth.Register;

public interface IRegisterService
{
    System.Threading.Tasks.Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}

public class RegisterService : IRegisterService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterService> _logger;

    public RegisterService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService, ILogger<RegisterService> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User registration failed for email {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException($"User registration failed: {errors}");
        }

        _logger.LogInformation("User {UserId} registered successfully with email {Email}", user.Id, user.Email);

        // Generate JWT tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Store refresh token in user record
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        return new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiry = refreshTokenExpiry
        };
    }
}
