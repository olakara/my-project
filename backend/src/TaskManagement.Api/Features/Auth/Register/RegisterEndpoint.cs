using FluentValidation;

namespace TaskManagement.Api.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this WebApplication app)
    {
        app.MapPost("/api/v1/auth/register", RegisterAsync)
            .WithName("Register")
            .WithOpenApi()
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account and returns JWT tokens");
    }

    private static async System.Threading.Tasks.Task<IResult> RegisterAsync(
        RegisterRequest request,
        IRegisterService registerService,
        IValidator<RegisterRequest> validator,
        ILogger<RegisterResponse> logger,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var response = await registerService.RegisterAsync(request, ct);
            logger.LogInformation("User registered successfully: {UserId}", response.UserId);
            return Results.Created($"/api/v1/users/{response.UserId}", response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
