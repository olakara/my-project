<!--
SYNC IMPACT REPORT
==================
Version Change: 1.1.1 → 1.1.2
Created: 2026-02-02, Amended: 2026-02-02
Bump Rationale: PATCH version - Clarification and refinement of Technology Stack section with EFCore database patterns, Domain/Data folder structure, and Serilog logging framework

Principles Defined:
- I. Test-Driven Development (TDD with AAA Pattern using xUnit) - NON-NEGOTIABLE
- II. Code Quality Standards
- III. Git & Commit Practices (Task-based format)
- IV. Security-First Development
- V. Observability & Logging (Serilog structured logging)

Sections Enhanced:
- Technology Stack: Added EFCore for ORM, Domain/Data folder organization, Serilog for structured logging

Modified Components:
- Database: EFCore with Domain entities and Data contexts
- Logging: Serilog for structured JSON logging with correlation IDs
- Project Structure: Added Domain/ and Data/ folders

Templates Requiring Updates:
✅ Updated: .specify/templates/plan-template.md - Tech stack check includes EFCore/Serilog
✅ Updated: .specify/templates/tasks-template.md - EFCore/Serilog setup tasks
✅ Updated: .specify/templates/spec-template.md - Reference to EFCore/Serilog

Follow-up TODOs: None - All requirements defined and propagated

Suggested Commit Message:
docs(v1.1.2): amend constitution tech stack - add EFCore, Domain/Data folders, Serilog logging
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

**Secure Coding Practices**:
- Always validate and sanitize inputs
- Use secure defaults
- Fail securely (deny by default)
- Encrypt sensitive data in transit (TLS) and at rest
- Implement rate limiting and request throttling
- Regular security audits and penetration testing

**Rationale**: Security breaches damage user trust, expose legal liability, and can be catastrophic. Building security in from the start is far more effective than retrofitting.

**Enforcement**: Automated security scanning in CI/CD pipeline. Security-related PRs MUST be prioritized. Security code reviews MUST be performed by designated security champions or senior engineers.

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

**Version**: 1.1.2 | **Ratified**: 2026-02-02 | **Last Amended**: 2026-02-02
