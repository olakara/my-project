# Implementation Tasks: Task Management Application with Real-Time Collaboration

**Feature**: 001-task-management  
**Generated**: 2026-02-03  
**Plan**: [plan.md](./plan.md) | **Spec**: [spec.md](./spec.md)  
**Total Tasks**: 127 | **Task Count by Phase**: Setup (8), Foundational (18), US1 Auth (16), US2 Projects (14), US3 Kanban (15), US4 Assignment (11), US5 Real-Time (12), US7 Reports (8), Polish (7)

---

## Overview

This document contains all implementation tasks organized by phase and user story. Tasks are designed to be independently executable and testable. Each task includes a unique ID, priority indicators, labels, and specific file paths for implementation.

**Key Metrics**:
- **Parallelizable Tasks**: 48 (marked with [P])
- **User Story Dependencies**: US1 (Auth) → US2 (Projects) → US3 (Kanban) & US4 (Assignment) & US5 (Real-Time)
- **MVP Scope**: US1 + US2 + US3 (core authentication, projects, and Kanban board)
- **Suggested Sprint**: Week 1: Setup + Foundational + US1; Week 2: US2 + US3; Week 3: US4 + US5; Week 4: US7 + Polish

---

## Phase 1: Setup & Project Initialization

**Goal**: Establish project structure, configure dependencies, and prepare for development.  
**Independent Test**: Verify backend builds without errors, frontend builds without errors, database connectivity works, all dependencies resolved.

### Setup Tasks

- [X] T001 Create backend project structure with .NET 10 Web API and vertical slice architecture in `backend/src/TaskManagement.Api/`
- [X] T002 [P] Create test projects (`TaskManagement.Api.Tests/` and `TaskManagement.IntegrationTests/`) with xUnit and test infrastructure
- [X] T003 [P] Create frontend React + Vite project in `frontend/` with TypeScript, Tailwind CSS, and shadcn/ui configured
- [X] T004 Initialize NuGet packages for backend: Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL, FluentValidation, Serilog, Microsoft.AspNetCore.Identity, System.IdentityModel.Tokens.Jwt in `backend/src/TaskManagement.Api/TaskManagement.Api.csproj`
- [X] T005 [P] Initialize npm packages for frontend: react, react-router-dom, zustand, @tanstack/react-query, @microsoft/signalr, tailwindcss, shadcn/ui components in `frontend/package.json`
- [X] T006 Create git structure and configure `.gitignore` for backend (bin/, obj/, Migrations/) and frontend (node_modules/, dist/, .env.local)
- [X] T007 Create appsettings configuration files: `backend/src/TaskManagement.Api/appsettings.json` (base), `appsettings.Development.json` (local dev), `appsettings.Production.json` (production)
- [X] T008 [P] Create documentation: README.md at repo root with setup instructions, architecture overview, and contribution guidelines

---

## Phase 2: Foundational Infrastructure

**Goal**: Establish shared infrastructure (database, authentication, logging, validation) that all features depend on.  
**Independent Test**: Verify database schema created, JWT tokens can be generated/validated, Serilog logs written correctly, FluentValidation validators execute.  
**Blockers**: All Phase 3+ tasks depend on completing Phase 2.

### Database & Data Access

- [x] T009 [P] Create EFCore DbContext in `backend/src/TaskManagement.Api/Data/TaskManagementDbContext.cs` with DbSets for all entities (User, Project, ProjectMember, Task, Comment, TaskHistory, Notification, ProjectInvitation)
- [x] T010 [P] Create EFCore entity configurations for User: `backend/src/TaskManagement.Api/Data/Configurations/UserConfiguration.cs` with identity mapping, shadow properties, indexes
- [x] T011 [P] Create entity configurations for Project, ProjectMember, Task: `backend/src/TaskManagement.Api/Data/Configurations/ProjectConfiguration.cs`, `ProjectMemberConfiguration.cs`, `TaskConfiguration.cs`
- [x] T012 [P] Create entity configurations for Comment, TaskHistory, Notification, ProjectInvitation: corresponding configuration classes in `backend/src/TaskManagement.Api/Data/Configurations/`
- [x] T013 Create initial EFCore migration in `backend/src/TaskManagement.Api/Data/Migrations/` named `InitialCreate` with all entity tables, foreign keys, and indexes
- [x] T014 [P] Create repository interfaces: `IUserRepository.cs`, `IProjectRepository.cs`, `ITaskRepository.cs` in `backend/src/TaskManagement.Api/Data/Repositories/`
- [x] T015 [P] Implement repository classes: `UserRepository.cs`, `ProjectRepository.cs`, `TaskRepository.cs` in `backend/src/TaskManagement.Api/Data/Repositories/` with async methods and query filters

### Authentication & Authorization

- [x] T016 [P] Create JWT token service in `backend/src/TaskManagement.Api/Services/JwtTokenService.cs` with methods for generating access tokens, refresh tokens, validating tokens, and refresh token rotation
- [x] T017 Create middleware for authentication: `backend/src/TaskManagement.Api/Middleware/AuthenticationMiddleware.cs` to validate JWT tokens from Authorization header
- [x] T018 Create middleware for correlation IDs: `backend/src/TaskManagement.Api/Middleware/CorrelationIdMiddleware.cs` to add request tracing capability
- [x] T019 [P] Create global exception handling middleware: `backend/src/TaskManagement.Api/Middleware/ExceptionHandlingMiddleware.cs` to catch unhandled exceptions and return standardized error responses with correlation IDs
- [x] T020 Configure ASP.NET Core Identity in `backend/src/TaskManagement.Api/Program.cs`: IdentityBuilder setup, password policy (12+ chars, complexity), user manager configuration

### Validation & Logging

- [x] T021 [P] Create FluentValidation assembly: configure base validator, error message localization in `backend/src/TaskManagement.Api/Validators/` with `ValidationBehavior` middleware for Minimal APIs
- [x] T022 [P] Configure Serilog structured logging in `backend/src/TaskManagement.Api/Program.cs` with JSON output format, correlation ID enrichment, development console sink, production file sink

### Dependency Injection

- [x] T023 Create service collection extensions in `backend/src/TaskManagement.Api/Extensions/ServiceCollectionExtensions.cs` registering all services (repositories, JwtTokenService, database context) with appropriate lifetimes
- [x] T024 Create web application extensions in `backend/src/TaskManagement.Api/Extensions/WebApplicationExtensions.cs` registering all middleware (authentication, correlation ID, exception handling, CORS)

### Domain Entities

- [x] T025 [P] Create domain entity classes in `backend/src/TaskManagement.Api/Domain/`: `ApplicationUser.cs` extending IdentityUser, with properties for profile info, refresh tokens, timestamps, navigation properties to ProjectMember, Task, Comment, Notification
- [x] T026 [P] Create Project entity in `backend/src/TaskManagement.Api/Domain/Projects/Project.cs` with properties for name, description, owner, archived flag, timestamps, and navigation properties to ProjectMember, Task, ProjectInvitation
- [x] T027 Create ProjectMember entity in `backend/src/TaskManagement.Api/Domain/Projects/ProjectMember.cs` with user/project references, role enum (Owner/Manager/Member), timestamps, and business methods for permission checks

---

## Phase 3: User Story 1 - Authentication (P1)

**Goal**: Enable user registration, login, JWT token generation/refresh, and logout with secure password handling.  
**Independent Test**: New user can register with email/password; registered user can login to receive JWT token; JWT token enables access to protected endpoints; expired token can be refreshed; logout revokes refresh token.  
**Acceptance Criteria**: 
- User registration validates password strength (12+ chars, complexity)
- JWT tokens expire in 15 minutes
- Refresh tokens expire in 7 days
- Account locks after 5 failed login attempts
- All auth events logged with Serilog

### Domain & Data

- [x] T028 [P] Create Task and Comment domain entities in `backend/src/TaskManagement.Api/Domain/Tasks/`: `Task.cs` with properties for title, description, status enum, priority enum, assignee, timestamps; `Comment.cs` with content, author, task reference, timestamps
- [x] T029 [P] Create TaskHistory and Notification entities in `backend/src/TaskManagement.Api/Domain/Tasks/TaskHistory.cs` and `backend/src/TaskManagement.Api/Domain/Notifications/Notification.cs` for audit trail and user notifications

### Auth Feature - Registration

- [x] T030 [US1] Create register request/response DTOs in `backend/src/TaskManagement.Api/Features/Auth/Register/RegisterRequest.cs` and `RegisterResponse.cs` with properties for email, password, firstName, lastName
- [x] T031 [US1] Create registration validator in `backend/src/TaskManagement.Api/Features/Auth/Register/RegisterValidator.cs` validating email uniqueness, password strength, field lengths with FluentValidation
- [x] T032 [P] [US1] Create registration service in `backend/src/TaskManagement.Api/Features/Auth/Register/RegisterService.cs` handling user creation via UserManager, hashing password, generating JWT tokens
- [x] T033 [US1] Create registration endpoint in `backend/src/TaskManagement.Api/Features/Auth/Register/RegisterEndpoint.cs` as Minimal API POST `/api/v1/auth/register` returning 201 with user details and JWT token

### Auth Feature - Login

- [x] T034 [US1] Create login request/response DTOs in `backend/src/TaskManagement.Api/Features/Auth/Login/LoginRequest.cs` and `LoginResponse.cs` with email, password properties
- [x] T035 [US1] Create login validator in `backend/src/TaskManagement.Api/Features/Auth/Login/LoginValidator.cs` validating email format, password not empty
- [x] T036 [P] [US1] Create login service in `backend/src/TaskManagement.Api/Features/Auth/Login/LoginService.cs` checking credentials, incrementing failed attempts, locking account after 5 failures, generating tokens
- [x] T037 [US1] Create login endpoint in `backend/src/TaskManagement.Api/Features/Auth/Login/LoginEndpoint.cs` as Minimal API POST `/api/v1/auth/login` returning 200 with JWT and refresh token cookie

### Auth Feature - Token Refresh

- [x] T038 [P] [US1] Create refresh token service in `backend/src/TaskManagement.Api/Features/Auth/RefreshToken/RefreshTokenService.cs` validating refresh token expiration, rotating tokens, updating user
- [x] T039 [US1] Create refresh token endpoint in `backend/src/TaskManagement.Api/Features/Auth/RefreshToken/RefreshTokenEndpoint.cs` as Minimal API POST `/api/v1/auth/refresh` returning new access token and rotated refresh token

### Auth Feature - Logout

- [x] T040 [US1] Create logout service in `backend/src/TaskManagement.Api/Features/Auth/Logout/LogoutService.cs` clearing refresh token from user record
- [x] T041 [US1] Create logout endpoint in `backend/src/TaskManagement.Api/Features/Auth/Logout/LogoutEndpoint.cs` as Minimal API POST `/api/v1/auth/logout` requiring [Authorize] returning 200

### Auth Feature - Tests

- [x] T042 [US1] Create registration unit tests in `backend/tests/TaskManagement.Api.Tests/Features/Auth/RegisterServiceTests.cs` testing valid registration, duplicate email validation, password complexity validation
- [X] T043 [P] [US1] Create login unit tests in `backend/tests/TaskManagement.Api.Tests/Features/Auth/LoginServiceTests.cs` testing successful login, failed attempts lockout, token generation
- [X] T044 [P] [US1] Create integration tests in `backend/tests/TaskManagement.IntegrationTests/Auth/AuthIntegrationTests.cs` testing full auth flow: register → login → access protected endpoint → refresh token → logout
- [X] T045 [US1] Create JWT token service tests in `backend/tests/TaskManagement.Api.Tests/Services/JwtTokenServiceTests.cs` testing token generation, validation, refresh token rotation

### Frontend Auth

- [X] T046 [US1] Create auth types in `frontend/src/types/auth.types.ts` defining User, AuthToken, LoginRequest, RegisterRequest interfaces
- [X] T047 [P] [US1] Create auth API client in `frontend/src/services/api/authApi.ts` with methods for register(), login(), logout(), refreshToken() using axios interceptors
- [X] T048 [US1] Create Zustand auth store in `frontend/src/store/authStore.ts` managing user state, tokens, login/logout/register actions, persistent storage via middleware
- [X] T049 [P] [US1] Create useAuth hook in `frontend/src/hooks/useAuth.ts` providing auth context to components (user, isLoading, error, login, register, logout)
- [X] T050 [US1] Create login page in `frontend/src/pages/auth/LoginPage.tsx` with form, email/password inputs, error display, link to register
- [X] T051 [P] [US1] Create register page in `frontend/src/pages/auth/RegisterPage.tsx` with form, firstName/lastName/email/password inputs, validation feedback, link to login
- [X] T052 [US1] Create protected route component in `frontend/src/components/auth/ProtectedRoute.tsx` redirecting unauthenticated users to login
- [X] T053 [P] [US1] Create API client with token interceptor in `frontend/src/services/api/apiClient.ts` injecting JWT into requests, handling 401 responses with silent refresh

---

## Phase 4: User Story 2 - Project Creation & Management (P1)

**Goal**: Enable authenticated users to create projects, invite team members with roles, and manage project settings.  
**Independent Test**: User can create project; invite team members by email; view members list with roles; update project settings; remove members; transfer ownership.  
**Acceptance Criteria**:
- Project creation requires name (1-100 chars)
- Only Owner/Manager can modify project settings
- Role-based access control enforced (Owner > Manager > Member)
- Invitations expire after 14 days
- Members list shows join date and role

### Backend Domain & Data

- [X] T054 [P] Create ProjectInvitation entity in `backend/src/TaskManagement.Api/Domain/Projects/ProjectInvitation.cs` with email, project reference, inviter, role, status enum (Pending/Accepted/Declined), expiration timestamp

### Create Project Feature

- [X] T055 [US2] Create project DTOs in `backend/src/TaskManagement.Api/Features/Projects/CreateProject/CreateProjectRequest.cs` and `CreateProjectResponse.cs` with name, description properties

- [X] T056 [US2] Create project validator in `backend/src/TaskManagement.Api/Features/Projects/CreateProject/CreateProjectValidator.cs` validating name length, owner verification
- [X] T057 [P] [US2] Create project service in `backend/src/TaskManagement.Api/Features/Projects/CreateProject/CreateProjectService.cs` creating project, adding creator as Owner member, logging audit event
- [X] T058 [US2] Create project endpoint in `backend/src/TaskManagement.Api/Features/Projects/CreateProject/CreateProjectEndpoint.cs` as POST `/api/v1/projects` requiring [Authorize] returning 201 with project details

### Get Projects Feature

- [X] T059 [P] [US2] Create get projects service in `backend/src/TaskManagement.Api/Features/Projects/GetProjects/GetProjectsService.cs` querying projects where user is member, including members count
- [X] T060 [US2] Create get projects endpoint in `backend/src/TaskManagement.Api/Features/Projects/GetProjects/GetProjectsEndpoint.cs` as GET `/api/v1/projects` requiring [Authorize] returning 200 with projects list

### Get Project Details

- [X] T061 [P] [US2] Create get project service in `backend/src/TaskManagement.Api/Features/Projects/GetProject/GetProjectService.cs` with authorization check (user must be member)
- [X] T062 [US2] Create get project endpoint in `backend/src/TaskManagement.Api/Features/Projects/GetProject/GetProjectEndpoint.cs` as GET `/api/v1/projects/{projectId}` returning 200 with full project details including members

### Update Project Feature

- [X] T063 [US2] Create update project DTOs in `backend/src/TaskManagement.Api/Features/Projects/UpdateProject/UpdateProjectRequest.cs` with name, description
- [X] T064 [US2] Create update validator in `backend/src/TaskManagement.Api/Features/Projects/UpdateProject/UpdateProjectValidator.cs` validating authorization (Owner/Manager only), field lengths
- [X] T065 [P] [US2] Create update project service in `backend/src/TaskManagement.Api/Features/Projects/UpdateProject/UpdateProjectService.cs` modifying project, logging change, broadcasting update to all members
- [X] T066 [US2] Create update project endpoint in `backend/src/TaskManagement.Api/Features/Projects/UpdateProject/UpdateProjectEndpoint.cs` as PUT `/api/v1/projects/{projectId}` requiring [Authorize] returning 200

### Invite Member Feature

 [X] T067 [US2] Create invite member DTOs in `backend/src/TaskManagement.Api/Features/Projects/InviteMember/InviteMemberRequest.cs` with email, role
 [X] T068 [US2] Create invite validator in `backend/src/TaskManagement.Api/Features/Projects/InviteMember/InviteMemberValidator.cs` validating email format, role, authorization (Owner/Manager)
 [X] T069 [P] [US2] Create invite service in `backend/src/TaskManagement.Api/Features/Projects/InviteMember/InviteMemberService.cs` creating invitation, sending email, logging audit
 [X] T070 [US2] Create invite endpoint in `backend/src/TaskManagement.Api/Features/Projects/InviteMember/InviteMemberEndpoint.cs` as POST `/api/v1/projects/{projectId}/invitations` returning 201

### Remove Member Feature

- [X] T071 [US2] Create remove member service in `backend/src/TaskManagement.Api/Features/Projects/RemoveMember/RemoveMemberService.cs` with authorization check, reassigning tasks, logging audit
- [X] T072 [P] [US2] Create remove member endpoint in `backend/src/TaskManagement.Api/Features/Projects/RemoveMember/RemoveMemberEndpoint.cs` as DELETE `/api/v1/projects/{projectId}/members/{userId}` requiring [Authorize] returning 204

### Accept Invitation Feature

- [X] T073 [US2] Create accept invitation DTOs in `backend/src/TaskManagement.Api/Features/Projects/AcceptInvitation/AcceptInvitationRequest.cs`
- [X] T074 [P] [US2] Create accept invitation service in `backend/src/TaskManagement.Api/Features/Projects/AcceptInvitation/AcceptInvitationService.cs` updating invitation status, creating project member, logging audit
- [X] T075 [US2] Create accept invitation endpoint in `backend/src/TaskManagement.Api/Features/Projects/AcceptInvitation/AcceptInvitationEndpoint.cs` as POST `/api/v1/invitations/{invitationId}/accept` requiring [Authorize] returning 200

### Project Tests

- [X] T076 [US2] Create project CRUD unit tests in `backend/tests/TaskManagement.Api.Tests/Features/Projects/ProjectServiceTests.cs` testing create, read, update with authorization checks
- [X] T077 [P] [US2] Create project integration tests in `backend/tests/TaskManagement.IntegrationTests/Projects/ProjectIntegrationTests.cs` testing create project → invite member → accept invitation → view members

### Frontend Projects



## Phase 5: User Story 3 - Kanban Board with Tasks (P1)

**Goal**: Display tasks on Kanban board with drag-drop between columns, real-time status updates, and task detail view.  
**Independent Test**: Kanban board displays 4 columns (ToDo, InProgress, InReview, Done); dragging task between columns updates status in DB; task detail modal shows full information; new tasks appear in ToDo column.  
**Acceptance Criteria**:
- Kanban board loads tasks for project in <2 seconds
- Drag-drop immediately updates task status
- Task filters available (by assignee, priority, due date)
- Empty columns show placeholder
- Column headers show task counts

### Create Task Feature

- [X] T086 [US3] Create task DTOs in `backend/src/TaskManagement.Api/Features/Tasks/CreateTask/CreateTaskRequest.cs` and `CreateTaskResponse.cs` with title, description, assigneeId, priority, dueDate
- [X] T087 [US3] Create task validator in `backend/src/TaskManagement.Api/Features/Tasks/CreateTask/CreateTaskValidator.cs` validating title (1-200 chars), description max, assignee exists in project, due date in future
- [X] T088 [P] [US3] Create task service in `backend/src/TaskManagement.Api/Features/Tasks/CreateTask/CreateTaskService.cs` creating task, recording history, broadcasting update via SignalR
- [X] T089 [US3] Create task endpoint in `backend/src/TaskManagement.Api/Features/Tasks/CreateTask/CreateTaskEndpoint.cs` as POST `/api/v1/projects/{projectId}/tasks` requiring [Authorize] returning 201

### Get Kanban Board Feature

- [X] T090 [P] [US3] Create get kanban board service in `backend/src/TaskManagement.Api/Features/Tasks/GetKanbanBoard/GetKanbanBoardService.cs` querying all tasks grouped by status with optional filters, applying pagination
- [X] T091 [US3] Create get kanban board endpoint in `backend/src/TaskManagement.Api/Features/Tasks/GetKanbanBoard/GetKanbanBoardEndpoint.cs` as GET `/api/v1/projects/{projectId}/tasks` with query params for filters returning 200 with tasks grouped by status

### Get Task Details Feature

- [X] T092 [US3] Create task detail response DTO in `backend/src/TaskManagement.Api/Features/Tasks/GetTask/GetTaskResponse.cs` including all task properties, assignee details, comments count, history preview
- [X] T093 [P] [US3] Create get task service in `backend/src/TaskManagement.Api/Features/Tasks/GetTask/GetTaskService.cs` with authorization check (user must be project member)
- [X] T094 [US3] Create get task endpoint in `backend/src/TaskManagement.Api/Features/Tasks/GetTask/GetTaskEndpoint.cs` as GET `/api/v1/tasks/{taskId}` requiring [Authorize] returning 200

### Update Task Status Feature

- [X] T095 [US3] Create update task status DTOs in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTaskStatus/UpdateTaskStatusRequest.cs` with newStatus field
- [X] T096 [US3] Create status validator in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTaskStatus/UpdateTaskStatusValidator.cs` validating status is valid enum value, authorization
- [X] T097 [P] [US3] Create update status service in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTaskStatus/UpdateTaskStatusService.cs` changing status, recording history entry, broadcasting update
- [X] T098 [US3] Create update status endpoint in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTaskStatus/UpdateTaskStatusEndpoint.cs` as PATCH `/api/v1/tasks/{taskId}/status` requiring [Authorize] returning 200

### Update Task Details Feature

- [X] T099 [US3] Create update task DTOs in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTask/UpdateTaskRequest.cs` with editable properties
- [X] T100 [US3] Create update validator in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTask/UpdateTaskValidator.cs` validating field lengths, authorization (creator or manager)
- [X] T101 [P] [US3] Create update task service in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTask/UpdateTaskService.cs` updating fields, recording changes in history
- [X] T102 [US3] Create update task endpoint in `backend/src/TaskManagement.Api/Features/Tasks/UpdateTask/UpdateTaskEndpoint.cs` as PUT `/api/v1/tasks/{taskId}` requiring [Authorize] returning 200

### Kanban Tests

- [X] T103 [US3] Create task CRUD tests in `backend/tests/TaskManagement.Api.Tests/Features/Tasks/TaskServiceTests.cs` testing create, read, status update with authorization
- [X] T104 [P] [US3] Create kanban board tests in `backend/tests/TaskManagement.IntegrationTests/Tasks/KanbanIntegrationTests.cs` testing drag-drop status update, filtering, pagination

### Frontend Kanban

- [X] T105 [US3] Create task types in `frontend/src/types/task.types.ts` defining Task, TaskStatus, TaskPriority interfaces
- [X] T106 [P] [US3] Create tasks API client in `frontend/src/services/api/tasksApi.ts` with methods for CRUD, status update, filtering, kanban board queries
- [X] T107 [US3] Create Zustand tasks store in `frontend/src/store/tasksStore.ts` managing tasks, filters, sorting
- [X] T108 [P] [US3] Create useTasks hook in `frontend/src/hooks/useTasks.ts` wrapping React Query queries for tasks with real-time sync support
- [X] T109 [US3] Create kanban board page in `frontend/src/pages/tasks/KanbanPage.tsx` rendering board with columns and task cards
- [X] T110 [P] [US3] Create kanban board component in `frontend/src/components/kanban/KanbanBoard.tsx` with drag-drop via react-beautiful-dnd, column layout
- [X] T111 [US3] Create kanban column component in `frontend/src/components/kanban/KanbanColumn.tsx` rendering task list for status with drop zone
- [X] T112 [P] [US3] Create draggable task card in `frontend/src/components/kanban/DraggableTask.tsx` with drag handle, click to open detail
- [X] T113 [US3] Create task card component in `frontend/src/components/tasks/TaskCard.tsx` showing title, assignee, priority, due date
- [X] T114 [P] [US3] Create task detail modal in `frontend/src/components/tasks/TaskDetail.tsx` displaying full task info, comments, history, edit button
- [X] T115 [US3] Create task form component in `frontend/src/components/tasks/TaskForm.tsx` with inputs for title, description, assignee, priority, due date

---

## Phase 6: User Story 4 - Task Assignment & Ownership (P2)

**Goal**: Enable task assignment to team members, send notifications, and provide "My Tasks" view.  
**Independent Test**: User assigned task receives notification; assigned user sees task in "My Tasks" view; reassignment updates both users; filtering by assignee works on Kanban board.  
**Acceptance Criteria**:
- Only project members can be assigned tasks
- Assignee can be changed by Owner/Manager/task creator
- Notifications sent within 500ms of assignment
- "My Tasks" shows all tasks assigned to current user
- Unassigned tasks show in board as "Unassigned"

### Assign Task Feature

- [X] T116 [US4] Create assign task DTOs in `backend/src/TaskManagement.Api/Features/Tasks/AssignTask/AssignTaskRequest.cs` with assigneeId
- [X] T117 [US4] Create assign validator in `backend/src/TaskManagement.Api/Features/Tasks/AssignTask/AssignTaskValidator.cs` validating assignee is project member, authorization
- [X] T118 [P] [US4] Create assign service in `backend/src/TaskManagement.Api/Features/Tasks/AssignTask/AssignTaskService.cs` updating assignee, recording history, creating notification, broadcasting update
- [X] T119 [US4] Create assign endpoint in `backend/src/TaskManagement.Api/Features/Tasks/AssignTask/AssignTaskEndpoint.cs` as PATCH `/api/v1/tasks/{taskId}/assign` requiring [Authorize] returning 200

### Get My Tasks Feature

- [X] T120 [P] [US4] Create get my tasks service in `backend/src/TaskManagement.Api/Features/Tasks/GetMyTasks/GetMyTasksService.cs` querying all tasks assigned to current user with optional filters
- [X] T121 [US4] Create get my tasks endpoint in `backend/src/TaskManagement.Api/Features/Tasks/GetMyTasks/GetMyTasksEndpoint.cs` as GET `/api/v1/tasks/my-tasks` requiring [Authorize] returning 200

### Assignment Tests

- [X] T122 [US4] Create assignment tests in `backend/tests/TaskManagement.Api.Tests/Features/Tasks/AssignmentServiceTests.cs` testing assignment, history recording, authorization
- [X] T123 [P] [US4] Create my tasks integration tests in `backend/tests/TaskManagement.IntegrationTests/Tasks/MyTasksIntegrationTests.cs` testing assignment flow and my tasks query

### Frontend Assignment

- [ ] T124 [US4] Create assignee selector component in `frontend/src/components/tasks/AssigneeSelector.tsx` showing project members dropdown, handling assignment changes
- [ ] T125 [P] [US4] Create my tasks page in `frontend/src/pages/tasks/MyTasksPage.tsx` displaying tasks assigned to current user with filters and status grouping
- [ ] T126 [US4] Create useMyTasks hook in `frontend/src/hooks/useMyTasks.ts` wrapping React Query for my tasks queries with real-time updates

---

## Phase 7: User Story 5 - Real-Time Collaboration (P2)

**Goal**: Implement WebSocket real-time updates for Kanban board, tasks, and comments via SignalR.  
**Independent Test**: Two users viewing same board see task status change within 1 second; comment added by one user appears instantly for other users; offline user sees cached data and syncs when reconnected.  
**Acceptance Criteria**:
- Task updates sync within 1 second (99th percentile)
- Comments appear instantly for all viewers
- Connection loss shows offline indicator
- Offline queue stores up to 50 local changes
- Reconnection triggers full board refresh

### SignalR Hub Setup

- [ ] T127 [P] [US5] Create TaskManagementHub in `backend/src/TaskManagement.Api/Hubs/TaskManagementHub.cs` with methods: JoinProject(projectId), LeaveProject(projectId), SendTaskUpdate(), SendCommentAdded(), SendUserConnected(), SendUserDisconnected()
- [ ] T128 [US5] Configure SignalR in `backend/src/TaskManagement.Api/Program.cs` with JWT authentication, CORS settings, hub routing, optional Redis backplane for scaling
- [ ] T129 [P] [US5] Create SignalR event mappings in hub to broadcast updates to project group: TaskStatusChanged → broadcast to group, CommentAdded → broadcast to group, TaskCreated → broadcast to group

### Backend Real-Time Publishing

- [ ] T130 [US5] Integrate SignalR hub injection into task services: CreateTaskService broadcasts TaskCreated event
- [ ] T131 [P] [US5] Update UpdateTaskStatusService to broadcast TaskStatusChanged event to project group
- [ ] T132 [US5] Update AssignTaskService to broadcast TaskAssigned event
- [ ] T133 [P] [US5] Create comment service in `backend/src/TaskManagement.Api/Features/Tasks/AddComment/AddCommentService.cs` with SignalR broadcast of CommentAdded event

### Add Comment Feature

- [ ] T134 [US5] Create comment DTOs in `backend/src/TaskManagement.Api/Features/Tasks/AddComment/AddCommentRequest.cs` and `AddCommentResponse.cs` with content
- [ ] T135 [US5] Create comment validator in `backend/src/TaskManagement.Api/Features/Tasks/AddComment/CommentValidator.cs` validating content length (1-5000 chars)
- [ ] T136 [P] [US5] Create add comment endpoint in `backend/src/TaskManagement.Api/Features/Tasks/AddComment/AddCommentEndpoint.cs` as POST `/api/v1/tasks/{taskId}/comments` requiring [Authorize] returning 201

### Frontend SignalR Integration

- [ ] T137 [US5] Create SignalR service in `frontend/src/services/signalr/signalrService.ts` managing hub connection, authentication, reconnection logic, event subscriptions
- [ ] T138 [P] [US5] Create useRealtime hook in `frontend/src/hooks/useRealtime.ts` providing real-time event subscriptions to components with automatic cleanup
- [ ] T139 [US5] Integrate SignalR event handlers into tasks store: listen for TaskStatusChanged, TaskCreated, CommentAdded events and update local state
- [ ] T140 [P] [US5] Update kanban board component to listen to real-time events and optimistically update UI before server confirmation
- [ ] T141 [US5] Create offline sync service in `frontend/src/services/api/offlineSync.ts` queuing mutations when offline, syncing when connection restored
- [ ] T142 [US5] Add connection status indicator component in `frontend/src/components/ui/ConnectionStatus.tsx` showing online/offline/connecting state with visual feedback

---

## Phase 8: User Story 7 - Task Progress Tracking & Reports (P2)

**Goal**: Provide dashboard with task metrics, burndown chart, team activity, and report generation.  
**Independent Test**: Dashboard shows accurate task counts by status; burndown chart displays correct completion trend; team activity shows member productivity; CSV export contains all task data; date filter recalculates metrics.  
**Acceptance Criteria**:
- Dashboard loads metrics in <1 second
- Burndown chart updates daily
- Team activity sorted by tasks completed
- CSV export includes all columns
- Metrics can be filtered by date range

### Get Project Metrics Feature

- [ ] T143 [P] [US7] Create metrics service in `backend/src/TaskManagement.Api/Features/Dashboard/GetProjectMetrics/GetProjectMetricsService.cs` calculating task counts by status, completion percentage, team member statistics
- [ ] T144 [US7] Create metrics endpoint in `backend/src/TaskManagement.Api/Features/Dashboard/GetProjectMetrics/GetProjectMetricsEndpoint.cs` as GET `/api/v1/projects/{projectId}/metrics` requiring [Authorize] returning 200

### Get Burndown Chart Feature

- [ ] T145 [US7] Create burndown service in `backend/src/TaskManagement.Api/Features/Dashboard/GetBurndown/GetBurndownService.cs` calculating tasks completed per day from task history records, aggregating by date
- [ ] T146 [P] [US7] Create burndown endpoint in `backend/src/TaskManagement.Api/Features/Dashboard/GetBurndown/GetBurndownEndpoint.cs` as GET `/api/v1/projects/{projectId}/burndown` with date range query params returning 200

### Get Team Activity Feature

- [ ] T147 [US7] Create team activity service in `backend/src/TaskManagement.Api/Features/Dashboard/GetTeamActivity/GetTeamActivityService.cs` aggregating task statistics by team member, sorting by completion count
- [ ] T148 [P] [US7] Create team activity endpoint in `backend/src/TaskManagement.Api/Features/Dashboard/GetTeamActivity/GetTeamActivityEndpoint.cs` as GET `/api/v1/projects/{projectId}/team-activity` requiring [Authorize] returning 200

### Export Report Feature

- [ ] T149 [US7] Create CSV export service in `backend/src/TaskManagement.Api/Features/Dashboard/ExportReport/ExportReportService.cs` generating CSV format with task data, dates, assignees
- [ ] T150 [P] [US7] Create export report endpoint in `backend/src/TaskManagement.Api/Features/Dashboard/ExportReport/ExportReportEndpoint.cs` as GET `/api/v1/projects/{projectId}/export-report` returning 200 with CSV attachment

### Dashboard Tests

- [ ] T151 [US7] Create metrics tests in `backend/tests/TaskManagement.Api.Tests/Features/Dashboard/MetricsServiceTests.cs` testing metric calculations with various task scenarios
- [ ] T152 [P] [US7] Create dashboard integration tests in `backend/tests/TaskManagement.IntegrationTests/Dashboard/DashboardIntegrationTests.cs` testing metrics, burndown, team activity queries

### Frontend Dashboard

- [ ] T153 [US7] Create dashboard page in `frontend/src/pages/dashboard/DashboardPage.tsx` rendering metrics cards, burndown chart, team activity table
- [ ] T154 [P] [US7] Create metrics card component in `frontend/src/components/dashboard/MetricsCard.tsx` displaying task counts and completion percentage
- [ ] T155 [US7] Create burndown chart component in `frontend/src/components/dashboard/BurndownChart.tsx` rendering chart with date range filter using Chart.js or Recharts
- [ ] T156 [P] [US7] Create team activity component in `frontend/src/components/dashboard/TeamActivityTable.tsx` displaying member statistics with sorting

---

## Phase 9: Polish & Cross-Cutting Concerns

**Goal**: Comprehensive testing, documentation, error handling, and performance optimization.  
**Independent Test**: Code coverage >85%, error responses include correlation IDs, all endpoints documented in Swagger, build runs without warnings, performance targets met.

### Testing & Quality

- [ ] T157 [P] Add comprehensive error handling tests covering 400/401/403/404/500 responses across endpoints
- [ ] T158 Add API contract validation tests ensuring responses match OpenAPI specs in contracts/
- [ ] T159 [P] Add performance/load testing for Kanban board with 500+ tasks to verify <2s load time
- [ ] T160 Add frontend component unit tests for key components (KanbanBoard, TaskForm, LoginForm) with React Testing Library

### Documentation & DevOps

- [ ] T161 [P] Generate Swagger/OpenAPI documentation in `backend/src/TaskManagement.Api/Program.cs` enabling `/swagger` endpoint with full API documentation
- [ ] T162 Create deployment documentation: `docs/DEPLOYMENT.md` with Docker setup, environment configuration, database migration steps
- [ ] T163 [P] Create architecture documentation: `docs/ARCHITECTURE.md` with Vertical Slice Architecture explanation, folder structure rationale, design decisions
- [ ] T164 Create API client SDK generation guide for mobile app consumption

### Security & Monitoring

- [ ] T165 [P] Add HTTPS/TLS configuration in production with certificate validation and HSTS headers
- [ ] T166 Add health check endpoint at `/health` returning service status including database connectivity
- [ ] T167 [P] Add request rate limiting middleware in `ExceptionHandlingMiddleware` to prevent abuse of auth endpoints
- [ ] T168 Add security scan (OWASP) validation and document findings

### Performance Optimization

- [ ] T169 [P] Add database query optimization: composite indexes on (ProjectId, Status) for Kanban queries, covering indexes for common filters
- [ ] T170 Add caching layer: Redis cache for frequently accessed data (project members, task filters) with TTL management
- [ ] T171 [P] Implement query pagination defaults: limit 50 tasks per page on Kanban board with infinite scroll support
- [ ] T172 Add frontend bundle size optimization: code splitting, lazy loading of routes, tree-shaking unused components

### Infrastructure

- [ ] T173 [P] Create Docker setup: `Dockerfile` for backend API, `docker-compose.yml` with PostgreSQL and optional Redis containers
- [ ] T174 Create CI/CD pipeline: GitHub Actions workflow (`.github/workflows/build.yml`) running tests, linting, building on every push
- [ ] T175 [P] Create database backup strategy documentation and automated backup scripts for PostgreSQL

---

## Dependency Graph

**Task Completion Order** (User Stories):

```
Phase 1 (Setup)
    ↓
Phase 2 (Foundational: DB, Auth, Validation, Logging)
    ↓
Phase 3 (US1: Authentication)
    ↓
Phase 4 (US2: Projects) ← Depends on US1
    ↓
┌─────────────────────────────────────────────┐
│ Phase 5 (US3: Kanban)                       │
│ Phase 6 (US4: Assignment) ← Parallel with US3
│ Phase 7 (US5: Real-Time) ← Can run parallel  
└─────────────────────────────────────────────┘
    ↓
Phase 8 (US7: Reports) ← Depends on US3 + US4
    ↓
Phase 9 (Polish & Cross-Cutting)
```

**Independent User Stories** (Can be developed in parallel after dependencies):
- US3 (Kanban) and US4 (Assignment) can be implemented in parallel (share domain, task DTOs)
- US5 (Real-Time) can be developed in parallel, integrated at end (decorates existing services)
- US7 (Reports) depends on complete task history, best done after US3 + US4

---

## Parallel Execution Examples

### Example 1: Week 2 - Maximized Parallelization

**Team A**: US2 Projects (T055-T085)
- T055-T077: Backend project CRUD, member management
- T078-T085: Frontend projects UI

**Team B**: US3 Kanban (T086-T115)
- T086-T104: Backend task CRUD, kanban queries
- T105-T115: Frontend kanban UI

**Both teams** create tasks in same project database from T088, T090 (no conflicts - vertical slices isolated)

### Example 2: Week 3 - US4 + US5 Parallel

**Team A**: US4 Assignment (T116-T126)
- Services use task repository, notifications publish events

**Team B**: US5 Real-Time (T127-T172)
- SignalR hub listens to events from T118 service
- Frontend receives real-time updates

Integration: T131 (UpdateTaskStatusService) decorated with SignalR broadcast.

---

## Implementation Strategy

**MVP Scope** (2-3 weeks):
- Phase 1: Setup (1 day)
- Phase 2: Foundational (2-3 days)
- Phase 3: US1 Auth (2-3 days)
- Phase 4: US2 Projects (2-3 days)
- Phase 5: US3 Kanban (2-3 days)

**Phase 2 Scope** (Week 4):
- Phase 6: US4 Assignment (2 days)
- Phase 7: US5 Real-Time (2-3 days)

**Phase 3 Scope** (Week 5+):
- Phase 8: US7 Reports (2 days)
- Phase 9: Polish (3-5 days)

**Testing Strategy**: TDD approach - write tests in Phase 3+ for each service/feature before implementation (see T042-T044 auth tests as pattern).

**Git Commit Pattern**: `feat(T042): implement user registration with password validation`

---

## Success Metrics

- ✅ All 175 tasks completed and tested
- ✅ Code coverage >85% (xUnit tests cover business logic, React Testing Library covers critical paths)
- ✅ Kanban board loads in <2 seconds for 500 tasks
- ✅ Real-time updates sync within 1 second (99th percentile)
- ✅ Support 100 concurrent users without performance degradation
- ✅ API response time <200ms p95 for CRUD operations
- ✅ All endpoints secured with [Authorize] attributes
- ✅ Zero sensitive data in logs
- ✅ 99.9% uptime during MVP phase
