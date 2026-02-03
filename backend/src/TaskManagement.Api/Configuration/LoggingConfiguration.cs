using Serilog;

namespace TaskManagement.Api.Configuration;

public static class LoggingConfiguration
{
    public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
    {
        var isDevelopment = builder.Environment.IsDevelopment();

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .MinimumLevel.Information();

        if (isDevelopment)
        {
            logger
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            logger
                .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/application-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    fileSizeLimitBytes: 1_000_000,
                    retainedFileCountLimit: 30,
                    shared: true);
        }

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger.CreateLogger());

        return builder;
    }
}
