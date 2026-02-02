<!--
SYNC IMPACT REPORT
==================
Version Change: 1.2.0 → 1.2.1
Created: 2026-02-02, Amended: 2026-02-02
Bump Rationale: PATCH version - Enhanced Security-First Development (Principle IV) with ASP.NET Core Identity, JWT authentication, and mandatory RBAC

Principles Updated:
- IV. Security-First Development: ENHANCED with ASP.NET Core Identity, JWT, and RBAC requirements

Sections Enhanced:
- Authentication & Authorization subsection: Added Identity, JWT, and RBAC implementation details
- Token Lifecycle Management: Refresh token patterns and token revocation
- Code examples: JWT configuration, authorization policies, refresh token flow

New Requirements:
1. ASP.NET Core Identity MUST be used for user account management
2. JWT (JSON Web Tokens) MUST be used for API authentication
3. Role-Based Access Control (RBAC) is MANDATORY for all endpoints
4. All protected endpoints MUST verify authentication and role membership
5. Strong password policies and account lockout configured
6. Token expiration and refresh token rotation implemented

Templates Updated:
✅ .specify/templates/plan-template.md - Added RBAC/JWT security compliance checks
✅ .specify/templates/spec-template.md - Added authentication/authorization metadata (Identity + JWT + RBAC)
✅ .specify/templates/tasks-template.md - Added Identity setup (T014), JWT config (T015), Token service (T016), Authorization policies (T017), Auth endpoints (T018)

Follow-up TODOs: None - all templates synchronized

Suggested Commit Message:
docs(v1.2.1): enhance security principle - add ASP.NET Core Identity, JWT, and RBAC requirements
-->

# my-project Constitution

## Core Principles

### I. Test-Driven Development (TDD with AAA Pattern using xUnit) - NON-NEGOTIABLE

**Rule**: All production code MUST be preceded by failing tests. No implementation before tests exist and fail.

**TDD Cycle (Mandatory)**:
1. Write test(s) that define the desired behavior
2. Run tests and verify they FAIL (red state)
3. Write minimal code to make tests pass (green state)
4. Refactor while keeping tests green
5. Commit only when tests pass

**Testing Framework (Mandatory)**:
- **Unit & Integration Tests**: xUnit framework MUST be used for all test projects
- Project naming convention: `[Feature].Tests` for unit tests, `[Feature].IntegrationTests` for integration tests
- xUnit Facts for single test cases, Theories with InlineData for parameterized tests
- Use xUnit's Fixtures for shared test setup and IDisposable for cleanup

**AAA Pattern (Mandatory)**:
Every test MUST follow the Arrange-Act-Assert structure:
- **Arrange**: Set up test data, mocks, and preconditions (use builders or factories)
- **Act**: Execute the behavior being tested (single action)
- **Assert**: Verify expected outcomes using xUnit's Assert class methods

**C# Testing Standards**:
- Test method names MUST be descriptive: `[MethodName]_[Scenario]_[ExpectedResult]`
- Use xUnit's `Assert.Throws<T>` or `Assert.ThrowsAsync<T>` for exception testing
- Use `IFixture` (xUnit fixture) for dependency injection in tests
- Mock external dependencies using Moq or similar frameworks
- Aim for 80%+ code coverage (excluding trivial getters/setters)

**Rationale**: TDD with AAA ensures testable design, prevents regressions, and creates living documentation. xUnit provides a modern, flexible testing framework aligned with .NET best practices. The AAA pattern enforces clear, maintainable tests with single responsibilities.

**Enforcement**: Code reviews MUST reject any production code lacking corresponding xUnit tests written first. Test commits MUST be separate from or precede implementation commits. CI/CD MUST run all xUnit tests and fail on low coverage.

---

### II. Code Quality Standards

**Rule**: All code MUST be clean, readable, and maintainable. Quality is non-negotiable.

**Requirements**:
- Functions/methods MUST have single, clear responsibilities (Single Responsibility Principle)
- Code MUST be self-documenting; comments explain "why," not "what"
- Magic numbers and hardcoded values MUST be named constants
- Cyclomatic complexity MUST be kept low (max 10 per function recommended)
- Code duplication MUST be eliminated through abstraction
- All public APIs MUST have documentation
- Linting and formatting rules MUST pass with zero warnings

**Naming Conventions**:
- Use descriptive names that reveal intent
- Boolean variables/functions MUST use is/has/can prefixes
- Avoid abbreviations unless industry-standard
- Consistent naming patterns within modules

**Rationale**: High-quality code reduces bugs, accelerates onboarding, and decreases maintenance costs. Clean code is an investment in the project's future.

**Enforcement**: Automated linters and formatters MUST run in CI/CD. Code reviews MUST verify adherence to quality standards before approval.

---

### III. Git & Commit Practices

**Rule**: All changes MUST follow standard git workflows with clear, atomic commits linked to tasks.

**Commit Format (Mandatory)**:
```
<type>(<task-id/issue-id>): <description>
```

**Examples**:
- `feat(T001): implement user registration endpoint`
- `test(GITHUB-42): add AAA tests for email validation`
- `fix(T015): resolve null reference in UserService`
- `docs(T020): update API documentation for auth flow`
- `refactor(GITHUB-88): extract validation logic to service`
- `chore(T005): update FluentValidation dependency`

**Commit Types (Mandatory)**:
- **feat**: New feature or capability
- **fix**: Bug fix or issue resolution
- **docs**: Documentation updates (README, API docs, comments)
- **refactor**: Code restructuring without behavior change
- **test**: Test additions, fixes, or improvements
- **chore**: Maintenance tasks, dependency updates, tooling

**Task-ID / Issue-ID Reference (Mandatory)**:
- Use task ID from `tasks.md` (e.g., T001, T015, T020)
- OR GitHub issue ID (e.g., GITHUB-42, GITHUB-88, #123)
- Task-ID enables traceability across feature development
- Links commits to specification and implementation planning
- Facilitates automated changelog generation and release notes

**Commit Message Structure**:
- **First line** (type + task-id + description): ≤ 50 characters
- **Blank line**: Separate subject from body
- **Body**: Explain "what" and "why", not "how" (≤ 72 characters per line)
- **References**: Include related task-ids or GitHub issues

**Example Detailed Commit**:
```
feat(T001): implement user registration endpoint

Adds POST /api/v1/users endpoint to allow account creation.

Changes:
- Create CreateUserRequest and CreateUserResponse DTOs
- Create CreateUserValidator using FluentValidation
- Implement UserService.CreateUserAsync method
- Create CreateUserEndpoint static class with Minimal API

Related tasks: T001, T002 (validation)
Closes: GITHUB-15
```

**Commit Frequency & Granularity (Mandatory)**:
- Commits MUST be **frequent and small** - ideally one commit per task or sub-task
- Each commit SHOULD represent one logical unit of work
- Avoid multi-task or "kitchen sink" commits
- Example progression for a feature:
  ```
  T001 - Write tests → [commit: test(T001): add xUnit tests for user registration]
  T001 - Implement request validators → [commit: feat(T001): add FluentValidation validators]
  T001 - Implement service → [commit: feat(T001): implement UserService.CreateUserAsync]
  T001 - Implement endpoint → [commit: feat(T001): create CreateUserEndpoint with Minimal API]
  T001 - Add logging → [commit: chore(T001): add structured logging to registration flow]
  ```
- Small commits enable:
  - Precise code archaeology and debugging
  - Easy cherry-picking and reverts
  - Clear attribution of changes
  - Meaningful code review feedback

**Branching Strategy**:
- Branch from `main` or `develop` depending on project workflow
- Branch naming: `<task-id>-<description>` or `<type>/<description>`
  - Example: `T001-user-registration` or `feat/user-registration`
- Feature branches MUST be short-lived (< 3 days ideal)
- Rebase onto latest upstream before creating PR

**Pull Request Requirements**:
- Each PR MUST be focused on single feature/epic (multiple related tasks OK)
- Include all commits with proper format and task-id references
- All xUnit tests MUST pass (Principle I)
- Pass all CI/CD checks (tests, linting, security scans)
- Require at least one approval from code reviewer
- Verify commit history is clean and meaningful
  - Squash only if commits are noise or duplicative
  - Preserve if commit history aids understanding

**Git History Standards**:
- History MUST be searchable by task-id and issue-id
- Rebase preferred over merge for linear history (when safe)
- Merge commits acceptable for feature branches to maintain branch identity
- Use `git log --grep=T001` to find all commits related to a task
- Use `git log --grep=#123` to find GitHub issue-related commits

**Rationale**: Task-based commits enable precise traceability from feature specification through implementation. Frequent, small commits reduce cognitive load in code reviews, simplify debugging, and create meaningful history. Linked task-ids bridge specification, implementation, testing, and deployment phases. Clear git history is essential for team collaboration and future maintenance.

**Enforcement**: Git hooks MUST validate commit format before allowing commits to local repository. CI/CD pipeline MUST reject PRs with non-compliant commit messages. Code reviews MUST verify commit granularity and relevance to referenced tasks.

---

### IV. Security-First Development

**Rule**: Security MUST be considered at every stage of development. Security flaws are critical bugs.

**Requirements**:
- Input validation MUST be performed on all external data
- Sensitive data (passwords, tokens, keys) MUST NEVER be logged or committed
- Use parameterized queries/prepared statements to prevent injection attacks
- Implement principle of least privilege for all access controls
- Dependencies MUST be scanned for vulnerabilities; high/critical CVEs MUST be addressed immediately
- Authentication and authorization MUST be centralized and consistently applied
- Error messages MUST NOT leak sensitive information or system details

**Authentication & Authorization (Mandatory)**:
- Use **ASP.NET Core Identity** for user account management and identity services
- Implement **JWT (JSON Web Tokens)** for API authentication
- **Role-Based Access Control (RBAC)** is MANDATORY for all endpoints
- All protected endpoints MUST verify user authentication and appropriate role membership
- Use `[Authorize]` and `[Authorize(Roles = "Admin")]` attributes consistently
- Implement centralized authorization policies for complex permission logic
- All authentication and authorization logic MUST be testable and auditable

**ASP.NET Core Identity Setup (Mandatory)**:
- Register Identity services in DI container with secure defaults
- Use strong password policies (minimum length, complexity requirements)
- Example registration in Program.cs:
  ```csharp
  builder.Services
      .AddIdentity<IdentityUser, IdentityRole>(options => {
          options.Password.RequiredLength = 12;
          options.Password.RequireDigit = true;
          options.Password.RequireUppercase = true;
          options.Password.RequireLowercase = true;
          options.Password.RequireNonAlphanumeric = true;
          options.Lockout.MaxFailedAccessAttempts = 5;
          options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
      })
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddDefaultTokenProviders();
  ```

**JWT Authentication (Mandatory)**:
- Configure JWT bearer authentication scheme
- Issue tokens with appropriate expiration (15-60 minutes recommended)
- Implement refresh token rotation for extended sessions
- Store JWT signing keys securely in configuration/secrets (never in code)
- Example authentication configuration:
  ```csharp
  builder.Services.AddAuthentication(options => {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  })
  .AddJwtBearer(options => {
      options.TokenValidationParameters = new TokenValidationParameters {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = builder.Configuration["Jwt:Issuer"],
          ValidAudience = builder.Configuration["Jwt:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
      };
  });
  ```

**Role-Based Access Control (RBAC) - Mandatory**:
- Define all application roles in a centralized location (Enum or database)
- Example roles: Admin, Manager, User, Guest
- All protected endpoints MUST declare required roles explicitly
- Example minimal API with authorization:
  ```csharp
  app.MapGet("/api/users/{id}", GetUserHandler)
      .WithName("GetUser")
      .WithOpenApi()
      .RequireAuthorization()
      .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));
  ```
- Implement authorization policies for complex rules:
  ```csharp
  builder.Services.AddAuthorizationBuilder()
      .AddPolicy("AdminOnly", policy => 
          policy.RequireRole("Admin"))
      .AddPolicy("ContentCreators", policy =>
          policy.RequireRole("Admin", "Editor", "Writer"));
  ```

**Token Lifecycle Management**:
- Access tokens MUST have short expiration (15-60 minutes)
- Implement refresh tokens for obtaining new access tokens
- Refresh tokens MUST be stored securely (secure HTTP-only cookies or secure storage)
- Implement token revocation mechanism (blacklist for logout)
- Example refresh token flow:
  ```csharp
  // POST /auth/refresh
  public async Task<IResult> RefreshToken(RefreshTokenRequest request) {
      var user = await _userManager.FindByIdAsync(request.UserId);
      if (user is null)
          return Results.Unauthorized();
      
      var newAccessToken = GenerateAccessToken(user);
      var newRefreshToken = GenerateRefreshToken();
      
      // Store new refresh token securely
      user.RefreshToken = newRefreshToken;
      user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
      await _userManager.UpdateAsync(user);
      
      return Results.Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
  }
  ```

**Secure Coding Practices**:
- Always validate and sanitize inputs
- Use secure defaults (deny by default)
- Fail securely (return appropriate 401/403 responses)
- Encrypt sensitive data in transit (TLS/HTTPS enforced)
- Encrypt sensitive data at rest (use EF Core encryption or DPAPI)
- Implement rate limiting and request throttling (prevent brute-force attacks)
- Log authentication/authorization events (user login, permission denied, etc.)
- Regular security audits and penetration testing

**Rationale**: ASP.NET Core Identity provides battle-tested user management. JWT enables stateless, scalable authentication. RBAC enforces principle of least privilege at the endpoint level. This combination prevents unauthorized access and enables audit trails.

**Enforcement**: Code reviews MUST verify all protected endpoints have appropriate `[Authorize]` attributes with roles/policies. Security scanning tools MUST detect missing authorization. CI/CD MUST fail on hardcoded JWT secrets or credentials. Authentication/authorization test coverage MUST exceed 95%.

---

### V. Observability & Logging

**Rule**: All systems MUST be observable. Logging MUST enable debugging and monitoring in production.

**Logging Standards**:
- Use structured logging (JSON format preferred)
- Include correlation IDs for request tracing across services
- Log levels MUST be used appropriately:
  - **ERROR**: Failures requiring immediate attention
  - **WARN**: Potential issues or degraded functionality
  - **INFO**: Significant business events (user actions, state changes)
  - **DEBUG**: Detailed technical information for troubleshooting
- NEVER log sensitive data (PII, passwords, tokens, API keys)
- Include relevant context (user ID, request ID, timestamp, service name)

**Observability Requirements**:
- Key operations MUST emit metrics (counters, gauges, histograms)
- Performance-critical paths MUST be instrumented with timing/tracing
- Health check endpoints MUST be implemented
- Errors MUST include stack traces and contextual information
- Implement distributed tracing for multi-service architectures

**Production Debugging**:
- Logs MUST be centralized and searchable
- Dashboards MUST track key business and technical metrics
- Alerting MUST be configured for critical errors and SLO violations
- Log retention policies MUST balance cost and compliance requirements

**Rationale**: Without observability, debugging production issues is impossible. Structured logging enables automated analysis, alerting, and insights into system behavior.

**Enforcement**: Code reviews MUST verify appropriate logging is present. Missing logging in critical paths MUST block PR approval. Log security scans MUST detect sensitive data leakage.

---

## Technology Stack

### Architecture & Framework

**Web Framework (Mandatory)**:
- Application MUST be built with **.NET 10 Web API**
- Use **Minimal APIs** with static classes for endpoint organization
- Each endpoint group MUST be organized in separate static classes within an `Endpoints` folder structure
- Implement **Vertical Slice Architecture** for feature organization
  - Each feature folder contains: models, services, endpoints, validators, and tests
  - Example: `Features/Users/GetUser/` contains all related classes for that specific operation
  - Dependencies flow inward; no cross-slice dependencies without service abstraction

**Dependency Injection (Mandatory)**:
- Use .NET's built-in dependency injection container (Microsoft.Extensions.DependencyInjection)
- All services MUST be registered in a centralized extension method (e.g., `ServiceCollectionExtensions`)
- Constructor injection MUST be used; never use service locator pattern
- Register with appropriate lifetime: Singleton, Scoped (default for HTTP requests), or Transient

**API Style (Mandatory)**:
- Build **RESTful APIs** following REST principles
- HTTP methods MUST be used correctly: GET (read), POST (create), PUT (full update), PATCH (partial update), DELETE (delete)
- Use appropriate HTTP status codes: 200 (OK), 201 (Created), 400 (Bad Request), 404 (Not Found), 500 (Server Error)
- Use consistent naming conventions for endpoints: `/api/v1/[resource]` pattern
- Request/response bodies MUST use JSON format

### Validation

**Input Validation (Mandatory)**:
- Use **FluentValidation** for all input validation
- Create validators for each request/command model (e.g., `CreateUserValidator`)
- Validators MUST be registered in the dependency injection container
- Use validation in minimal API endpoints via ValidationFilter or direct validator call
- Never allow unvalidated input to reach business logic
- Validation errors MUST return 400 Bad Request with detailed error messages

**FluentValidation Standards**:
- One validator class per request/command model
- Use semantic validation rules: `RuleFor(x => x.Email).EmailAddress()`, `RuleFor(x => x.Age).GreaterThan(0)`
- Combine multiple rules using chaining and collections
- Create custom validators for domain-specific validations
- Include meaningful error messages for each rule

### Database Access & Entities

**EFCore (Entity Framework Core) - Mandatory**:
- Use **Entity Framework Core** for all database interactions and ORM
- Never use raw SQL or stored procedures unless absolutely necessary (and justified)
- Use async/await patterns with `async`/`await` keywords
- Implement the Repository pattern to abstract data access

**Domain Entities (Mandatory)**:
- Domain model classes MUST be placed in `Domain/` folder
- Example: `Domain/Users/User.cs`, `Domain/Products/Product.cs`
- Domain entities represent core business concepts (Aggregate Roots, Value Objects)
- Keep domain entities free of infrastructure/persistence concerns
- Use property initialization with backing fields where needed
- Include validation logic in domain entities (immutable where possible)

**Data Access & Contexts (Mandatory)**:
- All EFCore `DbContext` classes MUST be placed in `Data/` folder
- `Data/YourProjectDbContext.cs` - Main DbContext class
- `Data/Configurations/` - Entity configurations (fluent API)
  - Example: `Data/Configurations/UserConfiguration.cs` for User entity mapping
- `Data/Migrations/` - EFCore-generated migration files
- `Data/Repositories/` - Repository implementations for data access
  - Example: `Data/Repositories/UserRepository.cs`
- DbContext MUST be registered in DI with appropriate lifetime (Scoped for HTTP requests)

**EFCore Standards**:
- Use fluent API in Configuration classes, not data annotations where possible
- Implement soft deletes via Shadow properties for audit trails (IsDeleted)
- Use value converters for domain-driven design patterns
- Leverage EFCore's change tracking for audit/logging
- Configure appropriate cascade delete behaviors

**Rationale**: EFCore provides type-safe, LINQ-based database access with async support. Separating Domain and Data folders enforces clear separation of concerns. Domain entities focus on business logic; Data layer handles persistence mechanics.

### Logging & Observability

**Serilog (Structured Logging) - Mandatory**:
- Use **Serilog** for all structured logging throughout the application
- Configure in `Program.cs` with appropriate sinks (Console, File, Cloud services)
- Serilog MUST output structured logs in JSON format for production environments
- Use `.UseSerilog()` in `WebApplicationBuilder` configuration

**Serilog Configuration (Mandatory)**:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()          // Enable context enrichment
    .Enrich.WithProperty("Environment", environment)
    .Enrich.WithProperty("Application", "YourProjectName")
    .WriteTo.Console(outputTemplate:  // Structured JSON in production
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

**Logging in Features & Services (Mandatory)**:
- Inject `ILogger<T>` via dependency injection in all services
- Log at appropriate levels:
  - **Error**: Use `logger.LogError()` for exceptions and failures
  - **Warning**: Use `logger.LogWarning()` for unusual but handled conditions
  - **Information**: Use `logger.LogInformation()` for business events (user actions, state changes)
  - **Debug**: Use `logger.LogDebug()` for technical troubleshooting
- Include structured data with log messages:
  ```csharp
  logger.LogInformation("User {UserId} registered with email {Email}", userId, email);
  ```
- NEVER log sensitive data (passwords, tokens, API keys, PII)

**Correlation IDs & Request Tracking (Mandatory)**:
- Implement correlation ID middleware to track requests across services
- Add correlation ID to `LogContext` for all subsequent logs in the request
- Example middleware:
  ```csharp
  app.Use(async (context, next) => {
      var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
          ?? Guid.NewGuid().ToString();
      using (LogContext.PushProperty("CorrelationId", correlationId)) {
          await next.Invoke();
      }
  });
  ```
- All logs in a request MUST include the correlation ID for traceability

**Performance & Diagnostics Logging**:
- Log method entry/exit for critical operations at Debug level
- Measure and log execution time for data access operations
- Log external service calls (duration, status code, errors)
- Example:
  ```csharp
  var stopwatch = Stopwatch.StartNew();
  var user = await userRepository.GetByIdAsync(userId);
  stopwatch.Stop();
  logger.LogInformation("Retrieved user {UserId} in {ElapsedMs}ms", userId, stopwatch.ElapsedMilliseconds);
  ```

**Rationale**: Serilog provides industry-standard structured logging with rich enrichment. JSON-formatted logs enable machine parsing, centralized logging, and analytics. Correlation IDs bridge distributed traces. Structured data enables powerful filtering and alerting.

### Project Structure (Updated)

```text
src/
├── YourProject.Api/
│   ├── Program.cs                      # Entry point, DI setup, Serilog config
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── ApplicationBuilderExtensions.cs
│   ├── Domain/                         # Domain models (business entities)
│   │   ├── Users/
│   │   │   ├── User.cs                 # Domain entity
│   │   │   ├── UserId.cs               # Value Object
│   │   │   └── UserEvents.cs           # Domain events (if using)
│   │   ├── Products/
│   │   │   └── Product.cs
│   │   └── Shared/
│   │       └── AggregateRoot.cs        # Base class for entities
│   ├── Data/                           # Data access layer (EFCore)
│   │   ├── YourProjectDbContext.cs     # DbContext
│   │   ├── Configurations/             # EFCore fluent API configs
│   │   │   ├── UserConfiguration.cs
│   │   │   └── ProductConfiguration.cs
│   │   ├── Repositories/               # Repository pattern implementations
│   │   │   ├── UserRepository.cs
│   │   │   └── ProductRepository.cs
│   │   └── Migrations/                 # EFCore migrations (auto-generated)
│   │       ├── 20260202_InitialCreate.cs
│   │       └── YourProjectDbContextModelSnapshot.cs
│   ├── Features/                       # Vertical slices (Endpoints)
│   │   ├── Users/
│   │   │   ├── GetUser/
│   │   │   │   ├── GetUserRequest.cs
│   │   │   │   ├── GetUserResponse.cs
│   │   │   │   ├── GetUserEndpoint.cs
│   │   │   │   └── GetUserValidator.cs
│   │   │   ├── CreateUser/
│   │   │   │   ├── CreateUserRequest.cs
│   │   │   │   ├── CreateUserResponse.cs
│   │   │   │   ├── CreateUserEndpoint.cs
│   │   │   │   └── CreateUserValidator.cs
│   │   │   └── Services/
│   │   │       └── UserService.cs      # Business logic
│   │   └── Products/
│   │       └── [similar structure]
│   ├── Middleware/                     # HTTP middleware
│   │   └── CorrelationIdMiddleware.cs  # Correlation ID for request tracing
│   ├── Common/
│   │   ├── Filters/
│   │   ├── Exceptions/
│   │   └── Utilities/
│   └── appsettings.json                # Serilog configuration
│
tests/
├── YourProject.Tests/                  # Unit tests
│   ├── Features/
│   │   ├── Users/
│   │   │   ├── GetUserEndpointTests.cs
│   │   │   ├── UserServiceTests.cs
│   │   │   └── CreateUserValidatorTests.cs
│   │   └── Common/
│   ├── Domain/
│   │   ├── Users/
│   │   │   └── UserTests.cs            # Domain entity tests
│   │   └── Products/
│   │       └── ProductTests.cs
│   └── Data/
│       ├── Users/
│       │   └── UserRepositoryTests.cs
│       └── Products/
│           └── ProductRepositoryTests.cs
│
└── YourProject.IntegrationTests/       # Integration tests
    ├── Features/
    │   ├── Users/
    │   │   ├── CreateUserIntegrationTests.cs
    │   │   └── GetUserIntegrationTests.cs
    │   └── Products/
    └── Data/
        └── [EFCore integration tests]
```

**Rationale**: Layered structure with Domain (entities) and Data (access) provides clear separation. Domain focuses on business rules; Data handles persistence. This enables testing domain logic independently from infrastructure. Serilog enrichment with application context improves observability.

---

## Engineering Guardrails

These guardrails enforce critical .NET best practices that complement the core principles. Violations MUST be identified and corrected in code review.

### Asynchronous-First Operations

**Rule**: All I/O-bound operations MUST use async/await patterns. Synchronous approaches are ONLY acceptable when async alternatives are unavailable (with documented justification).

**Mandatory Async/Await Patterns**:
- **Database Operations**: ALL EFCore queries MUST use async methods
  - `await _context.Users.FirstOrDefaultAsync()`
  - `await repository.GetByIdAsync(id)`
  - `await _context.SaveChangesAsync()`
  - NEVER use: `.FirstOrDefault()`, `.ToList()`, `.SaveChanges()`

- **HTTP Operations**: All external API calls MUST use async HTTP clients
  - `await _httpClient.GetAsync(url)`
  - `await _httpClient.PostAsJsonAsync(url, data)`
  - NEVER use: `WebClient`, synchronous `HttpClient` methods

- **File Operations**: All file I/O MUST use async methods
  - `await File.ReadAllTextAsync(path)`
  - `await File.WriteAllTextAsync(path, content)`
  - NEVER use: `File.ReadAllText()`, `File.WriteAllText()`

- **Method Signatures**: All methods performing I/O operations MUST return `Task` or `Task<T>`
  - `public async Task<User> GetUserAsync(int id)`
  - `public async Task SaveChangesAsync()`
  - NEVER return `void` from async methods (except event handlers)

**ConfigureAwait Pattern**:
- Use `ConfigureAwait(false)` in library code (avoids context switching)
- Example: `await _context.Users.FirstOrDefaultAsync().ConfigureAwait(false)`
- Not required in ASP.NET Core endpoints (where context is already correct)

**Rationale**: Async/await enables efficient resource utilization, allowing the thread pool to handle other requests while waiting for I/O. Synchronous blocking reduces application throughput and can cause thread starvation under load.

**Enforcement**: Code reviews MUST reject any I/O-bound synchronous calls. Linters MUST flag `Result` properties on `Task` and similar anti-patterns. CI/CD MUST fail on detected blocking calls.

### Null Safety with Nullable Reference Types

**Rule**: Nullable reference types MUST be enabled at project level. Reference types MUST be explicitly marked as nullable where applicable. The compiler MUST guide null-safety validation.

**Project Configuration (Mandatory)**:
Each .csproj file MUST include:
```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

This enables:
- Non-nullable reference types by default
- Compiler warnings for potential null reference exceptions
- Explicit `?` annotation required for nullable types

**Null-Safe Coding Patterns**:

**✅ CORRECT - Nullable Annotation**:
```csharp
public User? GetUserById(int id)              // May return null
public string Name { get; set; } = string.Empty;  // Never null
public string? Email { get; set; }            // Can be null
public List<string>? Tags { get; set; }       // Can be null or empty

// Null-checking
if (user is not null) {                       // Guard clause
    logger.LogInformation("User: {Name}", user.Name);
}

// Null coalescing
var email = user?.Email ?? "unknown@example.com";

// Safe navigation
var city = user?.Address?.City;
```

**❌ INCORRECT - Missing Nullable Markers**:
```csharp
public User GetUserById(int id)      // Implies never null, but might be!
public string Email { get; set; }    // Compiler warning if null possible
public List<string> Tags { get; set; }  // Can be null without marker
```

**Null Validation at Entry Points**:
- ALL controller/endpoint parameters MUST be validated for null
- Use guard clauses or argument validation
```csharp
public async Task<IResult> Create(CreateUserRequest? request, IValidator<CreateUserRequest> validator) {
    if (request is null)
        return Results.BadRequest("Request required");
    
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);
}
```

**Null-Safety in Domain Entities**:
```csharp
public class User {
    public int Id { get; set; }
    public string Email { get; private set; }    // Non-nullable, must be set
    public string? MiddleName { get; set; }      // Nullable, can be absent
    public List<Order> Orders { get; } = new();  // Collections never null, initialized
    
    public static Result<User> Create(string email, string? middleName) {
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure("Email required");
        
        return Result<User>.Success(new User { Email = email, MiddleName = middleName });
    }
}
```

**Compiler Warning Resolution**:
- NEVER use `#pragma disable` or `null!` force-cast as avoidance
- Instead, fix the underlying null-safety issue
- Use `ArgumentNullException.ThrowIfNull(parameter)` for explicit validation

**Rationale**: Nullable reference types shift null-safety from runtime exceptions to compile-time warnings. This eliminates entire categories of `NullReferenceException` bugs. Explicit annotations make intent clear and enable better tooling.

**Enforcement**: Compiler warnings MUST be treated as errors (`<WarningsAsErrors>nullable</WarningsAsErrors>`). Code reviews MUST verify explicit nullable annotations. No `#pragma` suppressions allowed without justification.

### Global Exception Handling Middleware

**Rule**: Implement Global Exception Handling Middleware. Controllers/endpoints MUST NOT contain try-catch blocks except for specific, recoverable business logic. Infrastructure errors (DB, HTTP) MUST be handled globally.

**Global Exception Handler Implementation (Mandatory)**:
```csharp
// Middleware/GlobalExceptionHandlingMiddleware.cs
public class GlobalExceptionHandlingMiddleware {
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception) {
        context.Response.ContentType = "application/json";
        
        var response = new ApiErrorResponse {
            Message = "An error occurred processing your request",
            TraceId = context.TraceIdentifier
        };

        context.Response.StatusCode = exception switch {
            ValidationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

**Middleware Registration** (in Program.cs):
```csharp
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.MapControllers();
```

**Exception Types & HTTP Status Mapping**:
| Exception Type | HTTP Status | Use Case |
|---|---|---|
| `ValidationException` | 400 Bad Request | FluentValidation errors |
| `KeyNotFoundException` | 404 Not Found | Resource not found |
| `UnauthorizedAccessException` | 401 Unauthorized | Authentication failure |
| `ForbiddenAccessException` | 403 Forbidden | Authorization failure |
| `InvalidOperationException` | 409 Conflict | Business rule violation |
| `Exception` | 500 Internal Server Error | Unexpected errors |

**When to Use Try-Catch (Limited Cases)**:

✅ **ONLY for specific, recoverable business logic**:
```csharp
public async Task<bool> DeleteUserAsync(int userId) {
    try {
        // Business-specific: attempt deletion, but continue if optional
        await _auditService.LogDeletionAsync(userId);
    }
    catch (AuditServiceException ex) {
        // Log but don't fail - deletion still proceeds
        _logger.LogWarning(ex, "Failed to log deletion for user {UserId}", userId);
    }
    
    // Database operation handled by global middleware
    await _userRepository.DeleteAsync(userId);
    return true;
}
```

**❌ NEVER use try-catch for infrastructure errors**:
```csharp
// WRONG: Global middleware handles this
try {
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex) {
    return Results.InternalServerError();
}
```

**Error Response Format (Mandatory)**:
```csharp
public class ApiErrorResponse {
    public string Message { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string TraceId { get; set; }
}
```

**Logging in Exception Middleware**:
- Log FULL exception details at ERROR level (stack trace, inner exceptions)
- Include request context (path, method, correlation ID)
- NEVER log sensitive data (passwords, tokens)
```csharp
_logger.LogError(
    ex,
    "Exception in {Path} {Method}. CorrelationId: {CorrelationId}",
    context.Request.Path,
    context.Request.Method,
    context.TraceIdentifier
);
```

**Rationale**: Centralized exception handling ensures consistent error responses, prevents information leakage, and simplifies maintenance. Controllers remain focused on business logic. Serilog logs all exceptions with full context for production debugging.

**Enforcement**: Code reviews MUST reject try-catch blocks in endpoints/controllers (except documented business logic). Linters MUST flag catch blocks catching base `Exception` in endpoints. All unhandled exceptions MUST be logged via Serilog.

---

## Development Workflow

### Code Review Process

All code changes MUST go through peer review before merging to main branches.

**Review Checklist**:
- [ ] Tests written first and demonstrate AAA pattern (Principle I)
- [ ] All tests pass in CI/CD
- [ ] Code quality standards met (Principle II)
- [ ] Commit messages follow conventional format (Principle III)
- [ ] Security considerations addressed (Principle IV)
- [ ] Appropriate logging and instrumentation added (Principle V)
- [ ] Documentation updated if APIs/behavior changed
- [ ] No sensitive data exposed in code or logs

**Review Focus**:
- Correctness and completeness
- Design and architecture alignment
- Performance implications
- Security vulnerabilities
- Maintainability and readability

---

## Quality Gates

The following gates MUST pass before code can be merged:

1. **Test Gate**: All tests pass; coverage meets minimum threshold (recommended 80%+)
2. **Linting Gate**: Zero linting errors or warnings
3. **Security Gate**: No high/critical vulnerabilities in dependencies or code
4. **Review Gate**: At least one approved review from qualified reviewer
5. **Build Gate**: Successful build on target platforms

CI/CD pipeline MUST enforce these gates automatically.

---

## Governance

### Constitutional Authority

This constitution supersedes all other development practices, coding standards, and workflow guidelines. When conflicts arise, constitutional principles take precedence.

### Amendment Process

Amendments to this constitution require:
1. Documented proposal with rationale
2. Discussion period with team (minimum 3 days)
3. Approval from project maintainers or technical lead
4. Version increment following semantic versioning
5. Migration plan if changes affect existing code
6. Update to all dependent documentation and templates

### Versioning Policy

Constitution versions follow semantic versioning (MAJOR.MINOR.PATCH):
- **MAJOR**: Backward-incompatible changes (principle removal/redefinition)
- **MINOR**: New principles or material expansions
- **PATCH**: Clarifications, typos, non-semantic refinements

### Compliance

All code reviews, architectural decisions, and technical discussions MUST reference and verify compliance with constitutional principles. Violations MUST be justified in writing and approved by technical leadership.

For runtime development guidance and detailed workflows, refer to the command prompt files in `.github/prompts/speckit.*.prompt.md`.

**Version**: 1.2.1 | **Ratified**: 2026-02-02 | **Last Amended**: 2026-02-02
