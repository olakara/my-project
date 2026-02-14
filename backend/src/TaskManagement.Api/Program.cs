using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Configuration;
using TaskManagement.Api.Data;
using TaskManagement.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.AddApplicationLogging();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseApplicationMiddleware();

// Map endpoints
app.MapApplicationEndpoints();

app.Run();

public partial class Program { }
