using Microsoft.AspNetCore.Identity;
using TaskManagement.Api.Data;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Configuration;

public static class IdentityConfiguration
{
    public static IServiceCollection AddApplicationIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password Policy
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // User Policy
                options.User.RequireUniqueEmail = true;

                // SignIn Policy
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // Lockout Policy
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<TaskManagementDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)
    {
        // Configure password hasher for stronger security
        services.Configure<PasswordHasherOptions>(options =>
        {
            options.IterationCount = 12000;
        });

        return services;
    }
}
