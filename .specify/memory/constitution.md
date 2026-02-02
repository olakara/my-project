<!--
SYNC IMPACT REPORT
==================
Version Change: TEMPLATE → 1.0.0
Created: 2026-02-02
Bump Rationale: Initial constitution establishing core project governance and engineering principles

Principles Defined:
- I. Test-Driven Development (TDD with AAA Pattern) - NON-NEGOTIABLE
- II. Code Quality Standards
- III. Git & Commit Practices
- IV. Security-First Development
- V. Observability & Logging

Templates Requiring Updates:
✅ Updated: .specify/templates/plan-template.md - Constitution Check section aligned
✅ Updated: .specify/templates/spec-template.md - Acceptance criteria align with TDD/AAA
✅ Updated: .specify/templates/tasks-template.md - Test-first task ordering, security/logging tasks

Follow-up TODOs: None - All core principles defined and propagated

Suggested Commit Message:
docs: establish constitution v1.0.0 with TDD, code quality, git, security & logging principles
-->

# my-project Constitution

## Core Principles

### I. Test-Driven Development (TDD with AAA Pattern) - NON-NEGOTIABLE

**Rule**: All production code MUST be preceded by failing tests. No implementation before tests exist and fail.

**TDD Cycle (Mandatory)**:
1. Write test(s) that define the desired behavior
2. Run tests and verify they FAIL (red state)
3. Write minimal code to make tests pass (green state)
4. Refactor while keeping tests green
5. Commit only when tests pass

**AAA Pattern (Mandatory)**:
Every test MUST follow the Arrange-Act-Assert structure:
- **Arrange**: Set up test data, mocks, and preconditions
- **Act**: Execute the behavior being tested (single action)
- **Assert**: Verify expected outcomes and side effects

**Rationale**: TDD with AAA ensures testable design, prevents regressions, and creates living documentation. The AAA pattern enforces clear, maintainable tests with single responsibilities.

**Enforcement**: Code reviews MUST reject any production code lacking corresponding tests written first. Test commits MUST be separate from or precede implementation commits.

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

**Version**: 1.0.0 | **Ratified**: 2026-02-02 | **Last Amended**: 2026-02-02
