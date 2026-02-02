---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Constitutional Requirements**:
- **TDD (NON-NEGOTIABLE)**: Tests MUST be written first, fail, then implementation follows
- **AAA Pattern**: All tests MUST follow Arrange-Act-Assert structure
- **Security**: Input validation, secure data handling in every user story
- **Logging**: Structured logging with appropriate levels for all operations
- **Code Quality**: Single responsibility, clear naming, no duplication
- **Git Commits**: MUST follow format `<type>(<task-id>): <description>` - frequent, small commits per task

**Engineering Guardrails**:
- **Async/Await First**: ALL I/O operations (EFCore, HTTP, files) MUST use async/await; no blocking calls
- **Null Safety**: Nullable reference types enabled (`<Nullable>enable</Nullable>`); explicit nullable annotations
- **Error Handling**: Global Exception Handling Middleware; no try-catch in controllers/endpoints (except business logic)

**Git Commit Standards**:
- Commit format: `<type>(<task-id>): <description>` where task-id is the task ID from this list (e.g., T001, T015)
- Types: feat, fix, docs, refactor, test, chore
- Examples:
  - `test(T013): add xUnit tests for CreateUserValidator`
  - `feat(T020): implement CreateUserEndpoint with Minimal API`
  - `chore(T001): add structured logging to registration flow`
- **Commit Frequency**: One commit per task or sub-task (small and frequent)
- Each commit MUST represent one logical unit of work

**Tests**: Tests are MANDATORY per constitution Principle I. Write tests FIRST, verify they FAIL, then implement.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[ID]**: Task identifier (e.g., T001, T015, T020) - MUST be used in commit messages
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions
- Each task is a unit for one git commit

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

<!-- 
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.
  
  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  
  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment
  
  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure per Technology Stack requirements

- [ ] T001 Create .NET 10 Web API project structure per Vertical Slice Architecture
  - Create `src/[Project].Api/` with Program.cs, Extensions/, Features/, Domain/, Data/, Common/, Middleware/ folders
  - Create `tests/[Project].Tests/` and `tests/[Project].IntegrationTests/` projects with matching folder structure
- [ ] T002 [P] Setup .NET 10 dependencies: FluentValidation, EFCore, EFCore.SqlServer (or relevant DB provider), Serilog, Moq, xUnit
- [ ] T003 [P] Configure linting tools (StyleCop, Roslyn analyzers) and code formatting
- [ ] T004 [P] Configure Serilog structured logging in Program.cs with JSON output and enrichment
- [ ] T005 [P] Configure EFCore DbContext with appropriate DbSets and conventions
- [ ] T006 [P] Enable Nullable reference types in .csproj: `<Nullable>enable</Nullable>` with `<WarningsAsErrors>nullable</WarningsAsErrors>`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T007 [P] Create Global Exception Handling Middleware in `Middleware/GlobalExceptionHandlingMiddleware.cs`
  - Handle standard exceptions with appropriate HTTP status codes
  - Log exceptions via Serilog with full context
  - Return standardized error response format
- [ ] T008 [P] Implement ApiErrorResponse class for consistent error responses with TraceId
- [ ] T009 [P] Register Global Exception Handling Middleware in Program.cs before routing
- [ ] T010 [P] Create base Domain entities in `Domain/Shared/` folder (e.g., AggregateRoot base class with nullable safety)
- [ ] T011 [P] Create EFCore entity configurations in `Data/Configurations/` for base entities with fluent API
- [ ] T012 [P] Create EFCore initial migration and verify DbContext setup (`dotnet ef migrations add InitialCreate`)
- [ ] T013 [P] Create Repository base class and register repositories in DI (Data/Repositories/)
- [ ] T014 [P] Implement authentication/authorization framework with DI registration (Principle IV - Security)
- [ ] T015 [P] Setup Minimal API base extensions for endpoint registration and routing
- [ ] T016 Create base request/response models and common validators (FluentValidation)
- [ ] T017 Implement correlation ID middleware for request tracing (Principle V - Serilog logging)
- [ ] T018 Create Global Exception Handler that logs via Serilog with structured context (guardrail enforcement)
- [ ] T019 Configure input validation error handling to return 400 Bad Request with Serilog logging
- [ ] T020 Configure security scanning and dependency vulnerability checks

**Checkpoint**: Foundation ready - Vertical Slice feature implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 (MANDATORY per Constitution) ✅

> **CONSTITUTIONAL REQUIREMENT (Principle I - TDD with xUnit)**: 
> Write these tests FIRST using AAA pattern (Arrange-Act-Assert) in xUnit
> Run tests and verify they FAIL before any implementation
> Test projects: `[Project].Tests/Domain/[Story]/`, `.Tests/Data/[Story]/`, `.Tests/Features/[Story]/`

- [ ] T017 [P] [US1] Create Domain entity tests in `Tests/Domain/[Story]/[Entity]Tests.cs` - test business logic and validations
- [ ] T018 [P] [US1] Create Repository tests in `Tests/Data/[Story]/[Entity]RepositoryTests.cs` - test EFCore data access
- [ ] T019 [P] [US1] Create FluentValidator tests in `Tests/Features/[Story]/[RequestValidator]Tests.cs` with AAA pattern
- [ ] T020 [P] [US1] Create Service tests in `Tests/Features/[Story]/[Service]Tests.cs` with Moq for dependencies
- [ ] T021 [P] [US1] Create Endpoint tests in `Tests/Features/[Story]/[Endpoint]EndpointTests.cs` for Minimal API
- [ ] T022 [P] [US1] Create integration tests in `IntegrationTests/Features/[Story]/` testing full workflow with real DB

**Verification**: Run `dotnet test`, confirm all xUnit tests FAIL (red state) before proceeding to implementation

### Implementation for User Story 1 (Vertical Slice with Domain, Data, Features)

**Domain Layer**:
- [ ] T023 [P] [US1] Create Domain entity `Domain/[Story]/[Entity].cs` with business logic, value objects, nullable safety
  - Use explicit `?` annotations for nullable properties
  - Never null collections/required properties without `?`
  - Include domain validations
- [ ] T024 [P] [US1] Create domain validations and business rules in entity (no null reference exceptions)

**Data Layer (EFCore)**:
- [ ] T025 [P] [US1] Create EFCore entity configuration in `Data/Configurations/[Entity]Configuration.cs` (fluent API)
- [ ] T026 [P] [US1] Create EFCore migration: `dotnet ef migrations add Add[Entity]` in `Data/Migrations/`
- [ ] T027 [P] [US1] Create Repository implementation in `Data/Repositories/[Entity]Repository.cs` with **async methods**
  - ALL repository methods MUST be async: `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`
  - Use `.ConfigureAwait(false)` in library code
  - Never use `.Result` or `.Wait()` on tasks

**Features Layer (API)**:
- [ ] T028 [P] [US1] Create request/response models: `Features/[Story]/[Request].cs` and `[Response].cs`
  - Use explicit `?` for nullable properties
  - Non-nullable properties must be initialized or required
- [ ] T029 [P] [US1] Create `Features/[Story]/[RequestValidator].cs` using FluentValidation (semantic rules)
- [ ] T030 [P] [US1] Create `Features/[Story]/Services/[Service].cs` with **async business logic** (uses Domain + Repository)
  - Service methods MUST be async: `CreateAsync`, `UpdateAsync`, etc.
  - Inject `ILogger<T>` and use for all operations
- [ ] T031 [US1] Create `Features/[Story]/Endpoints/[Endpoint]Endpoint.cs` static class with **async Minimal API**
  - Handler MUST be async: `private static async Task<IResult> Handle(...)`
  - NO try-catch blocks (Global Exception Handling Middleware handles errors)
  - Validate requests; let middleware handle exceptions
  - Use null-safe navigation: `user?.Address?.City`
- [ ] T032 [US1] Register endpoints and services in Program.cs using extension methods (DI)
- [ ] T033 [US1] Add structured logging with correlation IDs to all operations (Serilog - Principle V)
  - Log method entry/exit at Debug level
  - Log business events at Info level
  - NEVER log sensitive data
- [ ] T034 [US1] Run `dotnet test` - verify all tests pass (green state), refactor if needed
  - Verify no nullable warnings
  - Verify all async methods tested with proper await patterns

**Checkpoint**: At this point, User Story 1 (complete Vertical Slice from Domain through API) should be fully functional, tested, secure, observable, and async-first with null safety

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 (MANDATORY per Constitution) ✅

- [ ] T035 [P] [US2] Create Domain entity tests in `Tests/Domain/[Story]/[Entity]Tests.cs`
- [ ] T036 [P] [US2] Create Repository tests in `Tests/Data/[Story]/[Entity]RepositoryTests.cs`
- [ ] T037 [P] [US2] Create validator and service tests in `Tests/Features/[Story]/`
- [ ] T038 [P] [US2] Create endpoint and integration tests

### Implementation for User Story 2

- [ ] T020 [P] [US2] Create [Entity] model in src/models/[entity].py
- [ ] T021 [US2] Implement [Service] in src/services/[service].py
- [ ] T022 [US2] Implement [endpoint/feature] in src/[location]/[file].py
- [ ] T023 [US2] Integrate with User Story 1 components (if needed)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 (OPTIONAL - only if tests requested) ⚠️

- [ ] T024 [P] [US3] Contract test for [endpoint] in tests/contract/test_[name].py
- [ ] T025 [P] [US3] Integration test for [user journey] in tests/integration/test_[name].py

### Implementation for User Story 3

- [ ] T026 [P] [US3] Create [Entity] model in src/models/[entity].py
- [ ] T027 [US3] Implement [Service] in src/services/[service].py
- [ ] T028 [US3] Implement [endpoint/feature] in src/[location]/[file].py

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in docs/
- [ ] TXXX Code cleanup and refactoring
- [ ] TXXX Performance optimization across all stories
- [ ] TXXX [P] Additional unit tests (if requested) in tests/unit/
- [ ] TXXX Security hardening
- [ ] TXXX Run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Contract test for [endpoint] in tests/contract/test_[name].py"
Task: "Integration test for [user journey] in tests/integration/test_[name].py"

# Launch all models for User Story 1 together:
Task: "Create [Entity1] model in src/models/[entity1].py"
Task: "Create [Entity2] model in src/models/[entity2].py"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
