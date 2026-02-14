using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Configuration;
using TaskManagement.Api.Data;
using TaskManagement.Api.Extensions;
using TaskManagement.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.AddApplicationLogging();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TaskManagement.Api")));

// Identity
builder.Services.AddApplicationIdentity(builder.Configuration);
builder.Services.ConfigureIdentityOptions();

// Validation
builder.Services.AddApplicationValidation();

// Application services
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "https://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Build
var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
    // Only run migrations if using a relational database (not in-memory for tests)
    try
    {
        dbContext.Database.Migrate();
    }
    catch (InvalidOperationException)
    {
        // In-memory database doesn't support migrations, just ensure created
        dbContext.Database.EnsureCreated();
    }
}

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseApplicationMiddleware();


if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Map endpoints
app.MapApplicationEndpoints();

app.Run();

public partial class Program { }
