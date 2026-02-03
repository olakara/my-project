using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Features.Auth.RefreshToken;

public interface IRefreshTokenService
{
    System.Threading.Tasks.Task<RefreshTokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<RefreshTokenService> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<RefreshTokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is required");
        }

        // Find user by refresh token
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken: ct);

        if (user == null)
        {
            _logger.LogWarning("Refresh attempted with invalid token");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh attempted with expired token for user {UserId}", user.Id);
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);
            throw new UnauthorizedAccessException("Refresh token has expired");
        }

        _logger.LogInformation("Refreshing token for user {UserId}", user.Id);

        // Generate new tokens (token rotation)
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        // Update user with new refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = newRefreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiry = newRefreshTokenExpiry
        };
    }
}
