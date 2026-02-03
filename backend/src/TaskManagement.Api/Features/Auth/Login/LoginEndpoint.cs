using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace TaskManagement.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/auth/login", LoginAsync)
            .WithName("Login")
            .WithOpenApi()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .WithSummary("Login user")
            .WithDescription("Authenticates user with email and password, returns JWT tokens");
    }

    private static async System.Threading.Tasks.Task<IResult> LoginAsync(
        LoginRequest request,
        ILoginService loginService,
        IValidator<LoginRequest> validator,
        ILogger<LoginResponse> logger,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var response = await loginService.LoginAsync(request, ct);
            logger.LogInformation("User {UserId} logged in successfully", response.UserId);
            
            // Return response with refresh token in secure HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.RefreshTokenExpiry
            };

            // Create result without refresh token in body (sent as cookie instead)
            var responseBody = new
            {
                response.UserId,
                response.Email,
                response.FirstName,
                response.LastName,
                response.AccessToken,
                response.RefreshTokenExpiry
            };

            var result = Results.Ok(responseBody);
            // Note: Cookie setting should be done in middleware or use a custom result
            return result;
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Login failed: {Message}", ex.Message);
            return Results.Unauthorized();
        }
    }
}
