using FluentValidation;

namespace TaskManagement.Api.Configuration;

public static class ValidationConfiguration
{
    public static IServiceCollection AddApplicationValidation(this IServiceCollection services)
    {
        // Register all validators from the API assembly
        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }
}
