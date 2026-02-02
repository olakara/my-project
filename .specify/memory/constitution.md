<!--
SYNC IMPACT REPORT
==================
Version Change: 1.0.0 → 1.1.0
Created: 2026-02-02, Amended: 2026-02-02
Bump Rationale: MINOR version - Add Technology Stack section with .NET 10 Web API, FluentValidation, Vertical Slice architecture requirements; enhance TDD principle with xUnit specifics

Principles Defined:
- I. Test-Driven Development (TDD with AAA Pattern using xUnit) - NON-NEGOTIABLE
- II. Code Quality Standards
- III. Git & Commit Practices
- IV. Security-First Development
- V. Observability & Logging

Sections Added:
- Technology Stack: .NET 10, FluentValidation, Vertical Slice, Minimal API, DI, RESTful

Modified Principles:
- I. TDD: Enhanced with xUnit framework and C# testing patterns

Templates Requiring Updates:
✅ Updated: .specify/templates/plan-template.md - Constitution Check includes tech stack
✅ Updated: .specify/templates/spec-template.md - Aligned with tech stack
✅ Updated: .specify/templates/tasks-template.md - Tech stack tasks added to foundational phase

Follow-up TODOs: None - All requirements defined and propagated

Suggested Commit Message:
docs: amend constitution to v1.1.0 - add .NET 10 tech stack and xUnit testing requirements
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

**Rule**: All changes MUST follow standard git workflows with clear, atomic commits.

**Commit Standards**:
- Use conventional commit format: `type(scope): description`
  - **Types**: feat, fix, docs, style, refactor, test, chore, perf, ci, build
  - **Example**: `feat(auth): add JWT token validation`
  - **Example**: `test(auth): add AAA tests for JWT validation`
- First line ≤ 50 characters; body lines ≤ 72 characters
- Body explains "what" and "why," not "how"
- Reference related issues/tickets

**Branching Strategy**:
- Branch from `main` or `develop` depending on project workflow
- Branch naming: `###-feature-name` or `type/description`
- Feature branches MUST be short-lived (< 3 days ideal)
- MUST rebase/merge latest changes before creating PR

**Pull Request Requirements**:
- Each PR MUST be focused on a single feature or fix
- Include tests (written first per Principle I)
- Pass all CI/CD checks (tests, linting, security scans)
- Require at least one approval
- Squash commits if history is noisy; preserve if history is valuable

**Rationale**: Clear git history aids debugging, code archaeology, and collaboration. Conventional commits enable automated changelog generation and semantic versioning.

**Enforcement**: Git hooks and CI/CD MUST enforce commit message format. PRs violating standards MUST be rejected.

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

### Project Structure

```text
src/
├── YourProject.Api/
│   ├── Program.cs                      # Entry point, DI setup
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── ApplicationBuilderExtensions.cs
│   ├── Features/
│   │   ├── Users/
│   │   │   ├── GetUser/
│   │   │   │   ├── GetUserRequest.cs
│   │   │   │   ├── GetUserResponse.cs
│   │   │   │   ├── GetUserEndpoint.cs        # Minimal API endpoint
│   │   │   │   └── GetUserValidator.cs       # FluentValidation
│   │   │   ├── CreateUser/
│   │   │   │   ├── CreateUserRequest.cs
│   │   │   │   ├── CreateUserResponse.cs
│   │   │   │   ├── CreateUserEndpoint.cs
│   │   │   │   └── CreateUserValidator.cs
│   │   │   └── Services/
│   │   │       └── UserService.cs            # Shared by slice
│   │   └── Products/
│   │       └── [similar structure]
│   └── Common/
│       ├── Filters/
│       ├── Exceptions/
│       └── Utilities/
│
tests/
├── YourProject.Tests/                  # Unit tests
│   ├── Features/
│   │   ├── Users/
│   │   │   ├── GetUserEndpointTests.cs
│   │   │   └── UserServiceTests.cs
│   │   └── Common/
│   │
│   └── YourProject.IntegrationTests/   # Integration tests
        └── Features/
            └── Users/
                └── CreateUserIntegrationTests.cs
```

**Rationale**: .NET 10 is the latest stable LTS release with modern async patterns and performance improvements. Vertical Slice Architecture reduces coupling and accelerates feature development. Minimal APIs with static classes provide lightweight, clean endpoint definitions. FluentValidation offers fluent, maintainable validation rules. Dependency injection enables testability and loose coupling.

**Enforcement**: Architectural violations MUST be identified in code reviews. New features MUST follow the Vertical Slice structure. All validators MUST use FluentValidation. Endpoints MUST be organized in static classes.

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

**Version**: 1.1.0 | **Ratified**: 2026-02-02 | **Last Amended**: 2026-02-02
