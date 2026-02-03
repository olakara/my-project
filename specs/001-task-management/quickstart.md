# Quickstart Guide: Task Management Application

**Feature**: 001-task-management  
**Branch**: `001-task-management`  
**Date**: 2026-02-03

This guide helps developers set up the development environment and run the task management application locally.

---

## Prerequisites

### Required Software

- **.NET 10 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 20+**: [Download](https://nodejs.org/)
- **PostgreSQL 14+**: [Download](https://www.postgresql.org/download/)
- **Git**: [Download](https://git-scm.com/downloads)
- **Visual Studio 2022** or **VS Code** with C# Dev Kit extension

### Optional Tools

- **Docker Desktop**: For containerized PostgreSQL (alternative to local install)
- **Postman** or **Thunder Client**: For API testing
- **Azure Data Studio** or **pgAdmin**: For database management

---

## Project Structure

```
my-project/
├── backend/
│   ├── src/
│   │   └── TaskManagement.Api/
│   └── tests/
│       ├── TaskManagement.Api.Tests/
│       └── TaskManagement.IntegrationTests/
├── frontend/
│   ├── src/
│   ├── public/
│   └── tests/
└── specs/
    └── 001-task-management/
```

---

## Setup Instructions

### 1. Clone Repository

```bash
git clone <repository-url>
cd my-project
git checkout 001-task-management
```

### 2. Database Setup

#### Option A: Local PostgreSQL Installation

1. Install PostgreSQL 14+ from official website
2. Create database and user:

```sql
CREATE DATABASE taskmanagement;
CREATE USER taskuser WITH PASSWORD 'YourSecurePassword123!';
GRANT ALL PRIVILEGES ON DATABASE taskmanagement TO taskuser;
```

3. Update connection string in `backend/src/TaskManagement.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taskmanagement;Username=taskuser;Password=YourSecurePassword123!"
  }
}
```

#### Option B: Docker PostgreSQL

```bash
docker run --name taskmanagement-postgres \
  -e POSTGRES_DB=taskmanagement \
  -e POSTGRES_USER=taskuser \
  -e POSTGRES_PASSWORD=YourSecurePassword123! \
  -p 5432:5432 \
  -d postgres:14
```

Connection string (already configured for Docker):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taskmanagement;Username=taskuser;Password=YourSecurePassword123!"
  }
}
```

### 3. Backend Setup (.NET API)

1. Navigate to backend directory:

```bash
cd backend/src/TaskManagement.Api
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Configure user secrets (never commit sensitive data):

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "YourSuperSecretJwtKeyThatIsAtLeast32CharactersLong!"
dotnet user-secrets set "Jwt:Issuer" "TaskManagementApp"
dotnet user-secrets set "Jwt:Audience" "TaskManagementClient"
```

4. Apply database migrations:

```bash
dotnet ef database update
```

If migrations don't exist yet, create initial migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5. Run backend API:

```bash
dotnet run
```

Backend API will be available at: `https://localhost:5001`

Swagger UI: `https://localhost:5001/swagger`

### 4. Frontend Setup (React + Vite)

1. Open new terminal, navigate to frontend:

```bash
cd frontend
```

2. Install dependencies:

```bash
npm install
```

3. Create `.env.local` file:

```bash
# .env.local
VITE_API_URL=https://localhost:5001/api/v1
VITE_HUB_URL=https://localhost:5001/hubs/taskmanagement
```

4. Run frontend development server:

```bash
npm run dev
```

Frontend will be available at: `http://localhost:5173`

---

## Verification

### Backend Health Check

1. Open browser to `https://localhost:5001/health` (should return "Healthy")
2. Navigate to Swagger UI: `https://localhost:5001/swagger`
3. Test `/auth/register` endpoint:

```json
POST /api/v1/auth/register
{
  "email": "test@example.com",
  "password": "TestPassword123!",
  "firstName": "Test",
  "lastName": "User"
}
```

Expected response: `201 Created` with JWT token and user details

### Frontend Verification

1. Open `http://localhost:5173` in browser
2. You should see the login/registration page
3. Register a new user account
4. Login and verify JWT token is stored in memory (check browser dev tools > Application > Storage)

### Database Verification

Connect to PostgreSQL and verify tables were created:

```sql
\c taskmanagement
\dt

-- Should show tables:
-- AspNetUsers, AspNetRoles, Projects, ProjectMembers, Tasks, 
-- TaskHistory, Comments, Notifications, ProjectInvitations
```

---

## Development Workflow

### Running Tests

**Backend (xUnit)**:

```bash
cd backend

# Run all tests
dotnet test

# Run unit tests only
dotnet test TaskManagement.Api.Tests/

# Run integration tests only
dotnet test TaskManagement.IntegrationTests/

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Frontend (Jest + React Testing Library)**:

```bash
cd frontend

# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with coverage
npm test -- --coverage
```

### Code Formatting

**Backend**:

```bash
# Format all C# files
dotnet format
```

**Frontend**:

```bash
# Format with Prettier
npm run format

# Lint with ESLint
npm run lint
```

### Database Migrations

When you modify entities in `Domain/`, create a new migration:

```bash
cd backend/src/TaskManagement.Api

# Create migration
dotnet ef migrations add AddTaskPriorityField

# Review generated migration in Data/Migrations/

# Apply migration
dotnet ef database update

# Rollback if needed
dotnet ef database update PreviousMigrationName
```

### Git Workflow

Follow commit format from constitution:

```bash
# Feature branch already exists: 001-task-management

# Make changes, write tests first (TDD)

# Commit with task ID reference
git add .
git commit -m "test(T001): add xUnit tests for user registration"

# Implement feature
git add .
git commit -m "feat(T001): implement user registration endpoint"

# Push changes
git push origin 001-task-management
```

Commit format: `<type>(<task-id>): <description>`
- Types: `feat`, `fix`, `test`, `docs`, `refactor`, `chore`
- Task IDs from `tasks.md` (T001, T002, etc.)

---

## Common Tasks

### Create New Feature (Vertical Slice)

Example: Add "GetUserProfile" feature

1. Create feature folder structure:

```
backend/src/TaskManagement.Api/Features/Users/GetUserProfile/
├── GetUserProfileRequest.cs
├── GetUserProfileResponse.cs
├── GetUserProfileValidator.cs
├── GetUserProfileService.cs
└── GetUserProfileEndpoint.cs
```

2. Write tests first (TDD):

```bash
cd backend/tests/TaskManagement.Api.Tests/Features/Users/
# Create GetUserProfileServiceTests.cs
```

3. Implement feature following AAA pattern in tests

4. Register endpoint in `Program.cs`:

```csharp
app.MapGroup("/api/v1/users")
   .MapGetUserProfileEndpoint()
   .RequireAuthorization();
```

### Add New Entity

1. Create entity in `Domain/`:

```csharp
// Domain/Tasks/Attachment.cs
public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;
}
```

2. Create EFCore configuration:

```csharp
// Data/Configurations/AttachmentConfiguration.cs
public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.HasOne(a => a.Task).WithMany(t => t.Attachments).HasForeignKey(a => a.TaskId);
    }
}
```

3. Add DbSet to `TaskManagementDbContext`:

```csharp
public DbSet<Attachment> Attachments { get; set; } = null!;
```

4. Create and apply migration:

```bash
dotnet ef migrations add AddAttachmentEntity
dotnet ef database update
```

### Debug Real-Time Updates (SignalR)

**Backend (C#)**:

Enable SignalR logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.SignalR": "Debug",
      "Microsoft.AspNetCore.Http.Connections": "Debug"
    }
  }
}
```

**Frontend (React)**:

Enable SignalR client logging:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/taskmanagement', {
    accessTokenFactory: () => authStore.getAccessToken()
  })
  .configureLogging(signalR.LogLevel.Debug)  // Add this line
  .withAutomaticReconnect()
  .build();
```

Check browser console for WebSocket connection logs.

---

## Troubleshooting

### Issue: Database connection fails

**Solution**:
- Verify PostgreSQL is running: `pg_isready` (Linux/Mac) or check Windows Services
- Check connection string in `appsettings.Development.json`
- Ensure database and user exist
- Test connection: `psql -h localhost -U taskuser -d taskmanagement`

### Issue: Migration fails with "relation already exists"

**Solution**:
```bash
# Drop database and recreate
dotnet ef database drop --force
dotnet ef database update
```

### Issue: Frontend can't connect to backend API (CORS error)

**Solution**:
Backend CORS is configured in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

app.UseCors("AllowFrontend");
```

Verify frontend URL matches CORS policy.

### Issue: JWT token authentication fails

**Solution**:
- Verify JWT secret is set in user secrets: `dotnet user-secrets list`
- Check token expiration (15 minutes)
- Ensure `Authorization: Bearer <token>` header is included in requests
- Check Swagger UI authorize button is configured

### Issue: SignalR connection fails

**Solution**:
- Verify WebSocket support: `https://localhost:5001/hubs/taskmanagement` should upgrade to WebSocket
- Check JWT token is passed in `accessTokenFactory`
- Ensure `[Authorize]` attribute on Hub class
- Check browser console for connection errors

---

## Next Steps

1. Review [spec.md](./spec.md) for full feature requirements
2. Review [plan.md](./plan.md) for architecture decisions
3. Review [data-model.md](./data-model.md) for entity relationships
4. Review [contracts/](./contracts/) for API specifications
5. Review [tasks.md](./tasks.md) for implementation task breakdown (generated via `/speckit.tasks`)

---

## Development Tools

### Recommended VS Code Extensions

- **C# Dev Kit**: C# language support and debugging
- **REST Client**: Test API endpoints directly from `.http` files
- **Tailwind CSS IntelliSense**: Autocomplete for Tailwind classes
- **ESLint**: JavaScript/TypeScript linting
- **Prettier**: Code formatting
- **GitLens**: Enhanced git integration

### Recommended VS 2022 Extensions

- **ReSharper**: Advanced C# refactoring and analysis
- **EF Core Power Tools**: Visual EFCore tooling
- **CodeMaid**: Code cleanup and formatting

### Useful Commands

```bash
# Backend: Watch for file changes and auto-reload
dotnet watch run

# Frontend: Check bundle size
npm run build
npm run preview

# Backend: Generate OpenAPI spec
dotnet tool install -g Swashbuckle.AspNetCore.Cli
swagger tofile --output openapi.json bin/Debug/net10.0/TaskManagement.Api.dll v1

# Check for outdated packages
dotnet list package --outdated  # Backend
npm outdated                    # Frontend
```

---

## Environment Variables

### Backend (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taskmanagement;Username=taskuser;Password=<secret>"
  },
  "Jwt": {
    "Secret": "<from user-secrets>",
    "Issuer": "TaskManagementApp",
    "Audience": "TaskManagementClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

### Frontend (`.env.local`)

```bash
VITE_API_URL=https://localhost:5001/api/v1
VITE_HUB_URL=https://localhost:5001/hubs/taskmanagement
VITE_ENV=development
```

---

## Support

For issues or questions:
1. Check [troubleshooting](#troubleshooting) section above
2. Review [constitution.md](../../.specify/memory/constitution.md) for coding standards
3. Consult feature specification in [spec.md](./spec.md)
4. Review research decisions in [research.md](./research.md)
