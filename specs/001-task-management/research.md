# Research: Task Management Application

**Feature**: 001-task-management  
**Date**: 2026-02-03  
**Purpose**: Resolve technical unknowns and establish best practices for implementation

## Research Tasks

### 1. SignalR Real-Time Communication Strategy

**Decision**: Use SignalR with Azure SignalR Service or Redis backplane for scaling

**Rationale**:
- SignalR provides WebSocket-based real-time communication with automatic fallback to Server-Sent Events and Long Polling
- Built-in support for .NET, excellent integration with ASP.NET Core
- Supports both WebSocket and HTTP-based transports for broad browser compatibility
- Handles connection management, reconnection logic, and heartbeat automatically
- Provides strongly-typed hubs with C# async/await support

**Implementation Approach**:
- **Single Server (MVP)**: Use in-memory SignalR with no backplane - sufficient for 100 concurrent users
- **Multi-Server (Scale)**: Add Redis backplane via `Microsoft.AspNetCore.SignalR.StackExchangeRedis` package
- **Hub Design**: Create `TaskManagementHub` with methods for joining project rooms and broadcasting task updates
- **Connection Groups**: Use SignalR groups to map connections to project IDs (only broadcast to relevant users)
- **Authentication**: Require JWT token in SignalR connection via `[Authorize]` attribute on hub

**Architecture Pattern**:
```csharp
// Hub methods (server receives from client)
public async Task JoinProject(string projectId)
public async Task LeaveProject(string projectId)

// Hub events (server broadcasts to clients)
await Clients.Group(projectId).SendAsync("TaskUpdated", taskDto);
await Clients.Group(projectId).SendAsync("TaskCreated", taskDto);
await Clients.Group(projectId).SendAsync("CommentAdded", commentDto);
```

**Alternatives Considered**:
- **WebSockets directly**: More control but requires custom connection management, reconnection logic, and cross-browser compatibility - rejected as reinventing SignalR
- **Pusher/Ably (3rd party)**: SaaS real-time services with good SDKs but adds external dependency and cost - rejected for MVP, consider for scale
- **Server-Sent Events (SSE)**: Simpler but uni-directional (server to client only) - rejected as tasks require bi-directional communication
- **Polling**: Simple but inefficient, high latency - rejected as requirement is <1s sync

**Best Practices**:
- Implement connection lifecycle management in frontend (reconnect on disconnect)
- Use TypeScript client SDK (`@microsoft/signalr`) for type-safe communication
- Log all SignalR events (connections, disconnections, messages) with correlation IDs
- Implement rate limiting on hub methods to prevent abuse
- Use connection ID to track active users per project

---

### 2. React State Management Strategy

**Decision**: Use Zustand for global state + React Query for server state

**Rationale**:
- **Zustand**: Lightweight (1KB), simple API, no boilerplate, TypeScript-first, no provider hell
- **React Query**: Purpose-built for server state, handles caching, invalidation, background refetching, optimistic updates
- Clear separation: Zustand for client state (auth, UI), React Query for API data (projects, tasks, users)
- Both libraries work seamlessly together and with TypeScript
- Easier to test than Redux or Context API patterns

**State Architecture**:
```typescript
// Zustand stores (client state)
authStore: { user, token, login(), logout(), refreshToken() }
uiStore: { sidebarOpen, theme, notifications[] }

// React Query queries (server state)
useProjects() - GET /api/v1/projects
useProject(id) - GET /api/v1/projects/{id}
useTasks(projectId) - GET /api/v1/projects/{projectId}/tasks
useTask(id) - GET /api/v1/tasks/{id}

// React Query mutations (server mutations)
useCreateTask() - POST /api/v1/tasks
useUpdateTask() - PUT /api/v1/tasks/{id}
useDeleteTask() - DELETE /api/v1/tasks/{id}
```

**React Query Configuration**:
- Stale time: 30s for tasks (fresh for Kanban board interactions)
- Cache time: 5 minutes (keep data in memory)
- Retry: 3 attempts with exponential backoff
- Refetch on window focus: Enabled (sync when user returns to tab)
- Optimistic updates: Enabled for task drag-drop (instant UI feedback)

**Alternatives Considered**:
- **Redux Toolkit**: Feature-rich but heavyweight (10KB+ gzipped), more boilerplate - rejected as overkill for MVP
- **Context API + useReducer**: Built-in but causes re-render issues, no dev tools - rejected for performance concerns
- **Jotai/Recoil**: Atomic state, good for complex derived state but learning curve - rejected as Zustand is simpler
- **SWR**: Similar to React Query but less features, smaller ecosystem - React Query chosen for richer feature set

**Best Practices**:
- Co-locate Zustand stores with feature folders (`src/store/authStore.ts`)
- Use React Query's `queryClient.invalidateQueries()` after mutations
- Implement optimistic updates for drag-drop to meet <1s sync requirement
- Use React Query DevTools in development for debugging
- Persist auth token in localStorage via Zustand middleware

---

### 3. JWT Token Management & Refresh Strategy

**Decision**: Short-lived access tokens (15 min) with rotating refresh tokens (7 days) stored in HTTP-only cookies

**Rationale**:
- Access tokens in memory (React state) prevent XSS token theft - cleared on page refresh
- Refresh tokens in HTTP-only cookies prevent JavaScript access (XSS protection) - still vulnerable to CSRF
- Implement CSRF protection via SameSite cookie attribute and optional CSRF token
- Token rotation on refresh reduces window of refresh token theft
- Silent refresh before expiration prevents user logout during active sessions

**Token Flow**:
1. User logs in → Server returns access token (15 min) + refresh token (7 days) in HTTP-only cookie
2. Frontend stores access token in Zustand store (memory only, not localStorage)
3. API requests include access token in `Authorization: Bearer {token}` header
4. Before access token expires (13 min), frontend silently calls `/auth/refresh` with refresh token cookie
5. Server validates refresh token, rotates it (new refresh token), returns new access token
6. Frontend updates access token in memory and continues operations
7. On logout, server blacklists refresh token (in Redis or database) and clears cookie

**Frontend Implementation**:
```typescript
// Axios interceptor for token refresh
axiosInstance.interceptors.response.use(
  response => response,
  async error => {
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      const newToken = await authStore.refreshToken(); // Calls /auth/refresh
      originalRequest.headers['Authorization'] = `Bearer ${newToken}`;
      return axiosInstance(originalRequest);
    }
    return Promise.reject(error);
  }
);
```

**Backend Implementation**:
```csharp
// JWT generation with claims
var claims = new[] {
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Role, user.Role)
};
var accessToken = new JwtSecurityToken(
    issuer: _config["Jwt:Issuer"],
    audience: _config["Jwt:Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(15),
    signingCredentials: credentials
);

// Refresh token stored with user
user.RefreshToken = GenerateRefreshToken(); // Cryptographically random string
user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
await _userManager.UpdateAsync(user);
```

**Security Considerations**:
- **XSS**: Access tokens in memory (not localStorage) prevent theft via script injection
- **CSRF**: SameSite=Strict cookie attribute + HTTPS prevent cross-site attacks
- **Token Rotation**: New refresh token on each refresh reduces replay attack window
- **Blacklist**: Maintain revoked token list in Redis with TTL matching token expiration
- **HTTPS Only**: All tokens transmitted over TLS; SecureFlag on cookies
- **Token Claims**: Include minimal data (user ID, email, role) - no sensitive info

**Alternatives Considered**:
- **Access token in localStorage**: Vulnerable to XSS (rejected)
- **Long-lived access tokens (24h+)**: Increases security risk if stolen (rejected)
- **Refresh token in localStorage**: Vulnerable to XSS, defeats purpose (rejected)
- **Session-based auth**: Requires server-side session storage, doesn't scale horizontally - rejected for stateless API

**Best Practices**:
- Implement automatic refresh 2 minutes before access token expiration
- Clear tokens from memory on logout and page close
- Add jti (JWT ID) claim for token tracking and revocation
- Log all authentication events (login, logout, refresh, failures) with Serilog
- Use `ValidateLifetime = true` in JWT validation to enforce expiration

---

### 4. PostgreSQL Schema Design & EFCore Patterns

**Decision**: Use EFCore Code-First migrations with Repository pattern and explicit FK relationships

**Rationale**:
- Code-First allows defining domain entities in C# and generating database schema via migrations
- Repository pattern abstracts data access and enables unit testing with mocked repositories
- Explicit foreign keys enforce referential integrity at database level
- Indexes on frequently queried fields (user_id, project_id, status) improve performance
- Soft deletes via `IsDeleted` shadow property maintain audit trail

**EFCore Configuration Patterns**:
```csharp
// Entity Configuration (Fluent API)
public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(5000);
        
        // Foreign key relationships
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull); // Unassign if user deleted
        
        // Indexes for performance
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedTimestamp);
        
        // Soft delete
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
```

**Repository Pattern**:
```csharp
public interface ITaskRepository
{
    Task<Task?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Task>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
    Task<List<Task>> GetByAssigneeIdAsync(string userId, CancellationToken ct = default);
    Task<Task> CreateAsync(Task task, CancellationToken ct = default);
    Task UpdateAsync(Task task, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default); // Soft delete
}

public class TaskRepository : ITaskRepository
{
    private readonly TaskManagementDbContext _context;
    
    public TaskRepository(TaskManagementDbContext context) => _context = context;
    
    public async Task<Task?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
    
    // ... other methods with async/await and Include() for navigation properties
}
```

**Migration Strategy**:
- Generate migrations locally: `dotnet ef migrations add InitialCreate`
- Apply migrations on startup in Development: `context.Database.MigrateAsync()`
- Apply migrations via CI/CD in Production: Use migration scripts or `dotnet ef database update`
- Name migrations descriptively: `AddTaskStatusIndex`, `AddProjectMemberRole`, etc.

**Performance Optimizations**:
- Use `.AsNoTracking()` for read-only queries (Kanban board, reports)
- Implement pagination with `Skip()` and `Take()` for large result sets
- Use `Select()` projections to fetch only needed columns (DTOs)
- Add composite indexes for multi-column WHERE clauses
- Use compiled queries for frequently executed queries

**Alternatives Considered**:
- **Dapper (micro-ORM)**: Faster than EFCore but requires manual SQL and mapping - rejected for development speed
- **Database-First**: Existing database drives code - rejected as greenfield project
- **Direct DbContext injection**: Simpler but tight coupling, hard to test - rejected for testability

**Best Practices**:
- Use async methods exclusively: `ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`
- Enable nullable reference types to catch null reference issues at compile time
- Use shadow properties for audit fields (CreatedBy, CreatedAt, UpdatedAt, IsDeleted)
- Implement `IEntityTypeConfiguration` for each entity (separate configuration from entity)
- Use value converters for enums to store as strings in database (more readable)

---

### 5. React + Tailwind CSS Component Architecture

**Decision**: Use shadcn/ui component library with Tailwind CSS for consistent, accessible UI

**Rationale**:
- shadcn/ui provides pre-built, accessible components (Button, Input, Dialog, etc.) compatible with Tailwind
- Components are copied to your project (not npm dependency), allowing full customization
- Built with Radix UI primitives (WAI-ARIA compliant, keyboard navigation, focus management)
- Tailwind utility-first approach enables rapid UI development and consistent design system
- TypeScript-first with excellent DX

**Component Organization**:
```typescript
// Base UI components (shadcn/ui)
src/components/ui/
  - button.tsx       // Variants: primary, secondary, ghost, link
  - input.tsx        // Styled input with validation states
  - card.tsx         // Container with header, content, footer
  - dialog.tsx       // Accessible modal with overlay
  - select.tsx       // Dropdown with search
  - badge.tsx        // Status badges (priority, role)

// Feature-specific components
src/components/kanban/
  - KanbanBoard.tsx          // Container with columns
  - KanbanColumn.tsx         // Column with header, task list
  - DraggableTask.tsx        // Task card with react-beautiful-dnd

src/components/tasks/
  - TaskCard.tsx             // Compact task card for Kanban
  - TaskForm.tsx             // Create/edit task form with validation
  - TaskDetail.tsx           // Full task detail modal
  - CommentList.tsx          // Comments with real-time updates
```

**Drag-and-Drop Implementation**:
- Use `@hello-pangea/dnd` (maintained fork of react-beautiful-dnd) for Kanban drag-drop
- Smooth animations, touch support, accessibility built-in
- OnDragEnd event triggers optimistic update + API call

```typescript
const onDragEnd = async (result: DropResult) => {
  const { source, destination, draggableId } = result;
  if (!destination) return;
  
  // Optimistic update (instant UI feedback)
  const newTasks = moveTask(tasks, source, destination);
  setTasks(newTasks);
  
  // API call (async)
  try {
    await updateTaskStatus(draggableId, destination.droppableId);
    queryClient.invalidateQueries(['tasks']);
  } catch (error) {
    // Rollback on error
    setTasks(tasks);
    toast.error('Failed to move task');
  }
};
```

**Tailwind Configuration**:
```javascript
// tailwind.config.js
module.exports = {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: { ... },    // Brand colors
        secondary: { ... },
        accent: { ... },
      },
      fontSize: {
        xs: '0.75rem',       // 12px
        sm: '0.875rem',      // 14px
        base: '1rem',        // 16px
        lg: '1.125rem',      // 18px
        xl: '1.25rem',       // 20px
      },
      spacing: {
        '128': '32rem',      // Custom spacing if needed
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),      // Better form styles
    require('@tailwindcss/typography'), // Rich text formatting
  ],
};
```

**Responsive Design**:
- Mobile-first approach: Default styles for mobile, use `md:`, `lg:` prefixes for larger screens
- Kanban board: Stacked columns on mobile, horizontal scroll on tablet+
- Sidebar: Hidden on mobile (hamburger menu), visible on desktop

**Alternatives Considered**:
- **Material-UI**: Feature-rich but large bundle size (500KB+), opinionated styles - rejected for performance
- **Ant Design**: Comprehensive but heavy, less customizable - rejected for Tailwind flexibility
- **Chakra UI**: Good DX but adds runtime cost - rejected for static Tailwind approach
- **Headless UI**: Similar to Radix but less features - shadcn/ui chosen for batteries-included approach

**Best Practices**:
- Create design tokens in Tailwind config (colors, spacing, typography) for consistency
- Use Tailwind's `@apply` directive sparingly (prefer utility classes in JSX for clarity)
- Implement dark mode via Tailwind's `dark:` variant and CSS variables
- Use `clsx` or `cn` utility for conditional class names
- Optimize bundle size with PurgeCSS (built into Tailwind)

---

### 6. Testing Strategy (xUnit + React Testing Library)

**Decision**: TDD approach with xUnit for backend, React Testing Library for frontend, Playwright for E2E

**Backend Testing (xUnit)**:
```csharp
// Unit test example (AAA pattern)
public class CreateTaskServiceTests
{
    [Fact]
    public async Task CreateTaskAsync_ValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var mockLogger = new Mock<ILogger<CreateTaskService>>();
        var service = new CreateTaskService(mockRepo.Object, mockLogger.Object);
        var request = new CreateTaskRequest { Title = "Test Task", ProjectId = 1 };
        
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<Task>(), default))
            .ReturnsAsync(new Task { Id = 1, Title = "Test Task" });
        
        // Act
        var result = await service.CreateTaskAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<Task>(), default), Times.Once);
    }
    
    [Theory]
    [InlineData("")]      // Empty title
    [InlineData(null)]    // Null title
    public async Task CreateTaskAsync_InvalidTitle_ThrowsValidationException(string title)
    {
        // Arrange, Act, Assert...
    }
}

// Integration test example
public class TaskEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public TaskEndpointIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateTask_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateTaskRequest { Title = "Integration Test", ProjectId = 1 };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/tasks", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(task);
        Assert.Equal("Integration Test", task.Title);
    }
}
```

**Frontend Testing (React Testing Library)**:
```typescript
// Component test example
describe('TaskCard', () => {
  it('renders task title and description', () => {
    const task = { id: 1, title: 'Test Task', description: 'Test description' };
    render(<TaskCard task={task} />);
    
    expect(screen.getByText('Test Task')).toBeInTheDocument();
    expect(screen.getByText('Test description')).toBeInTheDocument();
  });
  
  it('calls onEdit when edit button clicked', async () => {
    const task = { id: 1, title: 'Test Task' };
    const onEdit = jest.fn();
    render(<TaskCard task={task} onEdit={onEdit} />);
    
    const editButton = screen.getByRole('button', { name: /edit/i });
    await userEvent.click(editButton);
    
    expect(onEdit).toHaveBeenCalledWith(task);
  });
});

// Hook test example
describe('useTasks', () => {
  it('fetches tasks on mount', async () => {
    const mockTasks = [{ id: 1, title: 'Task 1' }];
    server.use(
      rest.get('/api/v1/tasks', (req, res, ctx) => res(ctx.json(mockTasks)))
    );
    
    const { result } = renderHook(() => useTasks(1));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockTasks);
  });
});
```

**E2E Testing (Playwright)**:
```typescript
test('user can create and move task on Kanban board', async ({ page }) => {
  // Login
  await page.goto('/login');
  await page.fill('[name="email"]', 'test@example.com');
  await page.fill('[name="password"]', 'Password123!');
  await page.click('button[type="submit"]');
  
  // Navigate to project
  await page.goto('/projects/1');
  
  // Create task
  await page.click('button:has-text("Create Task")');
  await page.fill('[name="title"]', 'E2E Test Task');
  await page.click('button:has-text("Save")');
  
  // Verify task appears in To Do column
  await expect(page.locator('.kanban-column-todo').getByText('E2E Test Task')).toBeVisible();
  
  // Drag task to In Progress
  await page.dragAndDrop('.task-card:has-text("E2E Test Task")', '.kanban-column-in-progress');
  
  // Verify task moved
  await expect(page.locator('.kanban-column-in-progress').getByText('E2E Test Task')).toBeVisible();
});
```

**Test Coverage Targets**:
- Backend: 85%+ code coverage (business logic, validators, services)
- Frontend: 70%+ coverage (components, hooks, utilities)
- Critical paths: 95%+ (authentication, task CRUD, real-time updates)

**Best Practices**:
- Write tests first (TDD) to drive implementation
- Use AAA pattern consistently (Arrange, Act, Assert)
- Mock external dependencies (repositories, HTTP clients, SignalR)
- Test user behavior, not implementation details
- Use test fixtures for shared setup (database seeding, test users)
- Run tests in CI/CD pipeline; fail build on test failures

---

## Summary of Key Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| **Real-Time** | SignalR with Redis backplane | Native .NET support, WebSocket + fallbacks, auto-reconnect |
| **State Management** | Zustand + React Query | Lightweight, TypeScript-first, separates client/server state |
| **Auth Strategy** | JWT (15 min) + Refresh Token (7 days) in HTTP-only cookie | XSS protection, silent refresh, token rotation |
| **Database** | PostgreSQL + EFCore Code-First | Strong typing, migrations, async support, repository pattern |
| **UI Framework** | React + Tailwind + shadcn/ui | Utility-first CSS, accessible components, rapid development |
| **Drag-Drop** | @hello-pangea/dnd | Smooth animations, accessibility, touch support |
| **Testing** | xUnit + React Testing Library + Playwright | TDD-friendly, behavior-focused, E2E coverage |
| **Logging** | Serilog with correlation IDs | Structured logs, request tracing, production debugging |

---

## Next Steps (Phase 1)

1. Generate `data-model.md` with entity definitions, relationships, and validations
2. Generate API contracts in `contracts/` (OpenAPI YAML for auth, projects, tasks, realtime)
3. Generate `quickstart.md` with setup instructions for developers
4. Update agent context with new technologies (SignalR, React Query, Zustand, shadcn/ui, etc.)
5. Re-evaluate Constitution Check to ensure all design decisions comply
