# Implementation Plan: Task Management Application with Real-Time Collaboration

**Branch**: `001-task-management` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-task-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a task management application with user authentication (ASP.NET Core Identity + JWT), real-time collaboration (SignalR WebSockets), and Kanban board interface. Backend uses .NET 10 Web API with Vertical Slice Architecture, PostgreSQL database via EFCore, FluentValidation for input validation, and Serilog for structured logging. Frontend uses React with Tailwind CSS for responsive UI. System supports project/team management with RBAC (Owner/Manager/Member roles), real-time task updates across clients, and mobile API endpoints. MVP targets 10K users, 1M tasks, with 100 concurrent connections and <2s Kanban board load times.

## Technical Context

**Language/Version**: .NET 10 (C# 12)  
**Primary Dependencies**: ASP.NET Core Identity, SignalR, EFCore (PostgreSQL provider), FluentValidation, Serilog, React 18+, Tailwind CSS 3+  
**Storage**: PostgreSQL database for primary data persistence, Redis for SignalR backplane (NEEDS CLARIFICATION: single server vs distributed)  
**Testing**: xUnit for backend (unit/integration tests), React Testing Library + Jest for frontend  
**Target Platform**: Linux/Windows server for backend API, modern web browsers for frontend (Chrome 90+, Firefox 88+, Safari 14+, Edge 90+)  
**Project Type**: Web application (backend API + frontend SPA)  
**Performance Goals**: 
- Kanban board loads in <2 seconds for 500+ tasks
- Real-time updates synchronize within 1 second (99th percentile)
- Support 100 concurrent users without degradation
- API response time <200ms p95 for CRUD operations

**Constraints**: 
- HTTPS/TLS required for all communication
- Async/await first for all I/O operations
- No blocking database calls
- JWT access tokens 15-min expiration, refresh tokens 7-day expiration
- Strong password requirements (12+ chars, complexity)
- All endpoints must have authorization attributes
- Nullable reference types enabled

**Scale/Scope**: 
- MVP: 10K users, 1M tasks, 100 concurrent connections
- 7 user stories (US1-US7): Authentication, Projects, Kanban, Assignment, Real-time, Mobile API, Reports
- 20 functional requirements (FR-001 to FR-020)
- 8 key entities (User, Project, ProjectMember, Task, TaskHistory, Comment, Notification, ProjectInvitation)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

This feature MUST comply with all constitutional principles and technology requirements:

**Principles Compliance**:
- [x] **I. TDD with xUnit**: Plan includes test-first approach using xUnit; tests will be written before implementation. Each feature slice will have corresponding xUnit test project with AAA pattern tests.
- [x] **II. Code Quality**: Architecture supports single responsibility via Vertical Slice Architecture; each feature is self-contained. Naming conventions documented in quickstart.md.
- [x] **III. Git & Commit Practices**: Feature commits will use format `<type>(<task-id>): <description>`; frequent small commits per task. Task IDs from tasks.md (T001, T002, etc.).
- [x] **IV. Security**: Input validation via FluentValidation for all requests; ASP.NET Core Identity for user management; JWT authentication with 15-min access tokens and 7-day refresh tokens; RBAC with Owner/Manager/Member roles on all protected endpoints; passwords hashed, tokens never logged; HTTPS enforced.
- [x] **V. Observability & Logging**: Serilog structured logging configured with correlation IDs; authentication events, task changes, API errors logged at appropriate levels; no sensitive data in logs; health check endpoints planned.

**Technology Stack Compliance**:
- [x] **.NET 10 Web API**: Using .NET 10 framework with Minimal APIs organized in static endpoint classes
- [x] **Vertical Slice Architecture**: Features organized by domain (Users/, Projects/, Tasks/, etc.) with co-located models, services, endpoints, validators
- [x] **FluentValidation**: All request DTOs have associated validators (CreateUserValidator, CreateTaskValidator, etc.)
- [x] **Dependency Injection**: Services registered in ServiceCollectionExtensions; constructor injection only
- [x] **RESTful API**: Endpoints follow REST principles - POST /api/v1/users (create), GET /api/v1/projects/{id} (read), PUT /api/v1/tasks/{id} (update), DELETE /api/v1/tasks/{id} (delete)
- [x] **Authentication & Authorization**: ASP.NET Core Identity + JWT configured; all protected endpoints use [Authorize] with roles (Owner, Manager, Member)
- [x] **EFCore Database**: Using EFCore with Npgsql PostgreSQL provider; Domain entities in Domain/, DbContext in Data/, Repositories in Data/Repositories/
- [x] **Serilog Logging**: Structured logging configured in Program.cs with JSON format; correlation IDs via middleware

**Engineering Guardrails Compliance**:
- [x] **Async/Await First**: All database operations (EFCore), HTTP calls (SignalR), and file I/O use async/await; no blocking calls (.Result, .Wait())
- [x] **Null Safety**: Nullable reference types enabled in .csproj (`<Nullable>enable</Nullable>`); explicit `?` annotations for nullable types (string?, User?)
- [x] **Error Handling**: Global Exception Handling Middleware implemented to catch unhandled exceptions; returns 500 with correlation ID; validation errors return 400; auth failures return 401/403

*All gates passed. No violations to justify.*

## Project Structure

### Documentation (this feature)

```text
specs/001-task-management/
├── spec.md              # Feature specification (already exists)
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── auth.yaml       # Authentication endpoints (register, login, refresh)
│   ├── projects.yaml   # Project CRUD and team management
│   ├── tasks.yaml      # Task CRUD, assignment, status updates
│   └── realtime.yaml   # SignalR hub methods and events
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── TaskManagement.Api/              # Main API project
│   │   ├── Program.cs                   # Application entry point, DI config, middleware pipeline
│   │   ├── appsettings.json             # Configuration (non-sensitive)
│   │   ├── appsettings.Development.json # Dev-specific config
│   │   ├── TaskManagement.Api.csproj    # Project file
│   │   │
│   │   ├── Domain/                      # Domain entities (aggregate roots, value objects)
│   │   │   ├── Users/
│   │   │   │   └── User.cs              # User entity (Identity integration)
│   │   │   ├── Projects/
│   │   │   │   ├── Project.cs           # Project aggregate root
│   │   │   │   ├── ProjectMember.cs     # Project membership with roles
│   │   │   │   └── ProjectInvitation.cs # Invitation entity
│   │   │   ├── Tasks/
│   │   │   │   ├── Task.cs              # Task entity
│   │   │   │   ├── TaskHistory.cs       # Audit trail
│   │   │   │   ├── Comment.cs           # Task comments
│   │   │   │   └── TaskStatus.cs        # Enum (ToDo, InProgress, InReview, Done)
│   │   │   └── Notifications/
│   │   │       └── Notification.cs      # User notifications
│   │   │
│   │   ├── Data/                        # EFCore DbContext, configurations, repositories
│   │   │   ├── TaskManagementDbContext.cs # Main DbContext
│   │   │   ├── Configurations/          # Fluent API entity configurations
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── ProjectConfiguration.cs
│   │   │   │   ├── TaskConfiguration.cs
│   │   │   │   └── ...
│   │   │   ├── Migrations/              # EFCore migrations (auto-generated)
│   │   │   └── Repositories/            # Repository implementations
│   │   │       ├── IUserRepository.cs
│   │   │       ├── UserRepository.cs
│   │   │       ├── IProjectRepository.cs
│   │   │       ├── ProjectRepository.cs
│   │   │       ├── ITaskRepository.cs
│   │   │       └── TaskRepository.cs
│   │   │
│   │   ├── Features/                    # Vertical slices - one folder per feature
│   │   │   ├── Auth/                    # Authentication feature
│   │   │   │   ├── Register/
│   │   │   │   │   ├── RegisterRequest.cs        # DTO
│   │   │   │   │   ├── RegisterResponse.cs       # DTO
│   │   │   │   │   ├── RegisterValidator.cs      # FluentValidation
│   │   │   │   │   ├── RegisterService.cs        # Business logic
│   │   │   │   │   └── RegisterEndpoint.cs       # Minimal API endpoint
│   │   │   │   ├── Login/
│   │   │   │   │   ├── LoginRequest.cs
│   │   │   │   │   ├── LoginResponse.cs
│   │   │   │   │   ├── LoginValidator.cs
│   │   │   │   │   ├── LoginService.cs
│   │   │   │   │   └── LoginEndpoint.cs
│   │   │   │   └── RefreshToken/
│   │   │   │       ├── RefreshTokenRequest.cs
│   │   │   │       ├── RefreshTokenResponse.cs
│   │   │   │       └── RefreshTokenEndpoint.cs
│   │   │   │
│   │   │   ├── Projects/                # Project management feature
│   │   │   │   ├── CreateProject/
│   │   │   │   │   ├── CreateProjectRequest.cs
│   │   │   │   │   ├── CreateProjectResponse.cs
│   │   │   │   │   ├── CreateProjectValidator.cs
│   │   │   │   │   ├── CreateProjectService.cs
│   │   │   │   │   └── CreateProjectEndpoint.cs
│   │   │   │   ├── GetProject/
│   │   │   │   ├── UpdateProject/
│   │   │   │   ├── DeleteProject/
│   │   │   │   ├── InviteMember/
│   │   │   │   └── RemoveMember/
│   │   │   │
│   │   │   ├── Tasks/                   # Task management feature
│   │   │   │   ├── CreateTask/
│   │   │   │   │   ├── CreateTaskRequest.cs
│   │   │   │   │   ├── CreateTaskResponse.cs
│   │   │   │   │   ├── CreateTaskValidator.cs
│   │   │   │   │   ├── CreateTaskService.cs
│   │   │   │   │   └── CreateTaskEndpoint.cs
│   │   │   │   ├── GetTask/
│   │   │   │   ├── UpdateTask/
│   │   │   │   ├── UpdateTaskStatus/     # Kanban drag-drop
│   │   │   │   ├── AssignTask/
│   │   │   │   ├── GetKanbanBoard/       # Get all tasks for project
│   │   │   │   └── AddComment/
│   │   │   │
│   │   │   ├── Notifications/           # Notification feature
│   │   │   │   ├── GetUserNotifications/
│   │   │   │   └── MarkAsRead/
│   │   │   │
│   │   │   └── Dashboard/               # Dashboard/Reports feature
│   │   │       ├── GetProjectMetrics/
│   │   │       └── GetTeamActivity/
│   │   │
│   │   ├── Hubs/                        # SignalR hubs for real-time communication
│   │   │   └── TaskManagementHub.cs     # Real-time updates hub
│   │   │
│   │   ├── Middleware/                  # Custom middleware
│   │   │   ├── ExceptionHandlingMiddleware.cs  # Global error handler
│   │   │   └── CorrelationIdMiddleware.cs      # Request tracing
│   │   │
│   │   ├── Services/                    # Shared services (not feature-specific)
│   │   │   ├── JwtTokenService.cs       # JWT generation and validation
│   │   │   ├── EmailService.cs          # Email sending (invitations, password reset)
│   │   │   └── NotificationService.cs   # Push notification orchestration
│   │   │
│   │   └── Extensions/                  # Extension methods
│   │       ├── ServiceCollectionExtensions.cs # DI registration
│   │       └── WebApplicationExtensions.cs    # Middleware registration
│   │
│   └── TaskManagement.Contracts/        # Shared contracts (optional separate project)
│       └── ... (DTOs if shared between projects)
│
└── tests/
    ├── TaskManagement.Api.Tests/        # Unit tests (xUnit)
    │   ├── Features/
    │   │   ├── Auth/
    │   │   │   ├── RegisterServiceTests.cs
    │   │   │   ├── RegisterValidatorTests.cs
    │   │   │   └── RegisterEndpointTests.cs
    │   │   ├── Projects/
    │   │   │   └── ...
    │   │   └── Tasks/
    │   │       └── ...
    │   └── Services/
    │       └── JwtTokenServiceTests.cs
    │
    └── TaskManagement.IntegrationTests/ # Integration tests
        ├── Auth/
        │   ├── RegisterIntegrationTests.cs
        │   └── LoginIntegrationTests.cs
        ├── Projects/
        │   └── ProjectCrudIntegrationTests.cs
        └── Tasks/
            └── TaskCrudIntegrationTests.cs

frontend/
├── public/                              # Static assets
│   ├── index.html
│   └── favicon.ico
│
├── src/
│   ├── main.tsx                         # React entry point
│   ├── App.tsx                          # Root component
│   ├── index.css                        # Global Tailwind styles
│   │
│   ├── components/                      # Reusable UI components
│   │   ├── ui/                         # Base UI components (buttons, inputs, cards)
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Card.tsx
│   │   │   └── Modal.tsx
│   │   ├── layout/                     # Layout components
│   │   │   ├── Header.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── MainLayout.tsx
│   │   ├── auth/                       # Auth-related components
│   │   │   ├── LoginForm.tsx
│   │   │   ├── RegisterForm.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   ├── projects/                   # Project components
│   │   │   ├── ProjectCard.tsx
│   │   │   ├── ProjectForm.tsx
│   │   │   └── TeamMemberList.tsx
│   │   ├── tasks/                      # Task components
│   │   │   ├── TaskCard.tsx
│   │   │   ├── TaskForm.tsx
│   │   │   ├── TaskDetail.tsx
│   │   │   └── CommentList.tsx
│   │   └── kanban/                     # Kanban board components
│   │       ├── KanbanBoard.tsx
│   │       ├── KanbanColumn.tsx
│   │       └── DraggableTask.tsx
│   │
│   ├── pages/                          # Page components (routes)
│   │   ├── auth/
│   │   │   ├── LoginPage.tsx
│   │   │   └── RegisterPage.tsx
│   │   ├── projects/
│   │   │   ├── ProjectsListPage.tsx
│   │   │   └── ProjectDetailPage.tsx
│   │   ├── tasks/
│   │   │   └── MyTasksPage.tsx
│   │   ├── dashboard/
│   │   │   └── DashboardPage.tsx
│   │   └── NotFoundPage.tsx
│   │
│   ├── services/                       # API clients and business logic
│   │   ├── api/
│   │   │   ├── apiClient.ts           # Axios instance with interceptors
│   │   │   ├── authApi.ts             # Authentication API calls
│   │   │   ├── projectsApi.ts         # Projects API calls
│   │   │   ├── tasksApi.ts            # Tasks API calls
│   │   │   └── notificationsApi.ts    # Notifications API calls
│   │   ├── signalr/
│   │   │   └── signalrService.ts      # SignalR connection management
│   │   └── auth/
│   │       └── authService.ts         # Auth state management
│   │
│   ├── hooks/                          # Custom React hooks
│   │   ├── useAuth.ts                 # Authentication hook
│   │   ├── useRealtime.ts             # SignalR real-time hook
│   │   ├── useProjects.ts             # Projects data hook
│   │   └── useTasks.ts                # Tasks data hook
│   │
│   ├── store/                          # State management (Context API or Zustand)
│   │   ├── authStore.ts
│   │   ├── projectsStore.ts
│   │   └── tasksStore.ts
│   │
│   ├── types/                          # TypeScript type definitions
│   │   ├── auth.types.ts
│   │   ├── project.types.ts
│   │   ├── task.types.ts
│   │   └── api.types.ts
│   │
│   └── utils/                          # Utility functions
│       ├── formatters.ts              # Date, currency formatters
│       ├── validators.ts              # Client-side validation
│       └── constants.ts               # App constants
│
├── tests/                              # Frontend tests
│   ├── components/
│   │   └── ...
│   └── services/
│       └── ...
│
├── package.json                        # Node dependencies
├── tsconfig.json                       # TypeScript config
├── vite.config.ts                      # Vite bundler config
├── tailwind.config.js                  # Tailwind CSS config
└── postcss.config.js                   # PostCSS config
```

**Structure Decision**: Selected Web application structure (Option 2) with backend API and frontend SPA. Backend uses Vertical Slice Architecture where each feature (Auth, Projects, Tasks) is self-contained with its own models, services, endpoints, and validators. Domain entities are shared across features but placed in Domain/ folder. Data access layer (EFCore) is centralized in Data/ folder with repositories. Frontend follows component-based architecture with reusable UI components, page components for routing, services for API integration, and hooks for state management. SignalR enables real-time collaboration between frontend and backend.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
