using Microsoft.AspNetCore.Identity;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Auth.Logout;

public interface ILogoutService
{
    System.Threading.Tasks.Task LogoutAsync(string userId, CancellationToken ct = default);
}

public class LogoutService : ILogoutService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LogoutService> _logger;

    public LogoutService(UserManager<ApplicationUser> userManager, ILogger<LogoutService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task LogoutAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Logout attempt for non-existent user {UserId}", userId);
            throw new InvalidOperationException("User not found");
        }

        // Clear refresh token to invalidate future refresh attempts
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to clear refresh token for user {UserId}", userId);
            throw new InvalidOperationException("Failed to logout");
        }

        _logger.LogInformation("User {UserId} logged out successfully", userId);
    }
}
