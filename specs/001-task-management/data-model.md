# Data Model: Task Management Application

**Feature**: 001-task-management  
**Date**: 2026-02-03  
**Database**: PostgreSQL 14+  
**ORM**: Entity Framework Core 8.0  

## Entity Relationship Diagram

```
┌─────────────┐         ┌──────────────────┐         ┌─────────────┐
│    User     │────1:N──│  ProjectMember   │──N:1────│   Project   │
│ (Identity)  │         │  (Join Table)    │         │             │
└─────────────┘         └──────────────────┘         └─────────────┘
      │                                                      │
      │ 1:N                                               1:N│
      │                                                      │
      ├──────────────────┬───────────────────┬───────────────┤
      │                  │                   │               │
      ▼                  ▼                   ▼               ▼
┌─────────────┐   ┌──────────────┐   ┌──────────────┐  ┌────────┐
│ Notification│   │   Comment    │   │   TaskHistory│  │  Task  │
│             │   │              │   │   (Audit)    │  │        │
└─────────────┘   └──────────────┘   └──────────────┘  └────────┘
                         │                  │               │
                         │ N:1              │ N:1           │ 1:1
                         │                  │               │
                         └──────────────────┴───────────────┘
                                        Task

┌──────────────────┐
│ ProjectInvitation│
│                  │────────────────────────────────────────┐
└──────────────────┘                                        │
      │ N:1                                              N:1│
      ├─────────────────────────────────────────────────────┤
      ▼                                                     ▼
┌─────────────┐                                     ┌─────────────┐
│    User     │                                     │   Project   │
│  (Inviter)  │                                     │             │
└─────────────┘                                     └─────────────┘
```

---

## Core Entities

### 1. User (ASP.NET Core Identity)

**Purpose**: Represents a registered user with authentication credentials and profile information.

**Table**: `AspNetUsers` (Identity framework standard table)

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | string (PK) | NOT NULL | GUID-based user ID (Identity default) |
| Email | string | NOT NULL, UNIQUE | User email address (login credential) |
| EmailConfirmed | bool | NOT NULL | Whether email is verified |
| PasswordHash | string | NOT NULL | Hashed password (Identity handles hashing) |
| FirstName | string | NULL | User first name |
| LastName | string | NULL | User last name |
| ProfilePictureUrl | string | NULL | URL to profile image (blob storage) |
| RefreshToken | string | NULL | Current refresh token for JWT |
| RefreshTokenExpiry | datetime | NULL | Refresh token expiration timestamp (UTC) |
| CreatedTimestamp | datetime | NOT NULL | Account creation timestamp (UTC) |
| UpdatedTimestamp | datetime | NOT NULL | Last profile update timestamp (UTC) |

**Relationships**:
- One-to-Many with `ProjectMember` (projects user is member of)
- One-to-Many with `Task` as `Assignee` (tasks assigned to user)
- One-to-Many with `Task` as `CreatedBy` (tasks created by user)
- One-to-Many with `Comment` as `Author` (comments written by user)
- One-to-Many with `Notification` as `Recipient` (notifications for user)
- One-to-Many with `ProjectInvitation` as `Inviter` (invitations sent by user)

**Validation Rules**:
- Email must be valid format (RFC 5322)
- Password must be 12+ characters with uppercase, lowercase, digit, special character
- FirstName and LastName max 100 characters each
- ProfilePictureUrl must be valid URL if provided

**Indexes**:
- Unique index on `Email` (enforced by Identity)
- Index on `RefreshToken` for token lookup performance

**Domain Behavior**:
```csharp
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}
```

---

### 2. Project

**Purpose**: Container for organizing related tasks; represents a project workspace with team members.

**Table**: `Projects`

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | int (PK) | AUTO_INCREMENT | Project unique identifier |
| Name | string | NOT NULL | Project name (1-100 chars) |
| Description | string | NULL | Optional project description (max 1000 chars) |
| OwnerId | string (FK) | NOT NULL | User ID of project owner (creator) |
| IsArchived | bool | NOT NULL, DEFAULT false | Soft archive flag |
| IsDeleted | bool | NOT NULL, DEFAULT false | Soft delete flag (shadow property) |
| CreatedTimestamp | datetime | NOT NULL | Project creation timestamp (UTC) |
| UpdatedTimestamp | datetime | NOT NULL | Last update timestamp (UTC) |

**Relationships**:
- Many-to-One with `User` as `Owner` (project creator)
- One-to-Many with `ProjectMember` (team members)
- One-to-Many with `Task` (tasks in project)
- One-to-Many with `ProjectInvitation` (pending invitations)

**Validation Rules**:
- Name required, 1-100 characters
- Description max 1000 characters
- OwnerId must reference existing user
- Name cannot be empty or whitespace

**Indexes**:
- Index on `OwnerId` (query projects by owner)
- Index on `IsArchived` (filter active projects)
- Index on `IsDeleted` (soft delete filter)

**Domain Behavior**:
```csharp
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
    public ICollection<ProjectInvitation> Invitations { get; set; } = new List<ProjectInvitation>();
    
    // Business methods
    public bool HasMember(string userId) => Members.Any(m => m.UserId == userId);
    public bool IsOwner(string userId) => OwnerId == userId;
    public ProjectRole? GetUserRole(string userId) => Members.FirstOrDefault(m => m.UserId == userId)?.Role;
}
```

---

### 3. ProjectMember (Join Table)

**Purpose**: Represents a user's membership in a project with associated role (Owner/Manager/Member).

**Table**: `ProjectMembers`

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | int (PK) | AUTO_INCREMENT | Membership unique identifier |
| UserId | string (FK) | NOT NULL | User ID (references AspNetUsers) |
| ProjectId | int (FK) | NOT NULL | Project ID (references Projects) |
| Role | string | NOT NULL | User role: Owner, Manager, Member |
| JoinedTimestamp | datetime | NOT NULL | When user joined/was added (UTC) |

**Relationships**:
- Many-to-One with `User` (member)
- Many-to-One with `Project` (project)

**Validation Rules**:
- UserId and ProjectId combination must be unique (user can only have one role per project)
- Role must be one of: Owner, Manager, Member
- ProjectId must reference existing project
- UserId must reference existing user

**Indexes**:
- Unique composite index on `(UserId, ProjectId)` (prevent duplicate memberships)
- Index on `ProjectId` (query all members of a project)
- Index on `UserId` (query all projects of a user)

**Domain Behavior**:
```csharp
public class ProjectMember
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public ProjectRole Role { get; set; }
    public DateTime JoinedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Project Project { get; set; } = null!;
    
    // Business methods
    public bool CanManageProject() => Role == ProjectRole.Owner || Role == ProjectRole.Manager;
    public bool CanDeleteProject() => Role == ProjectRole.Owner;
}

public enum ProjectRole
{
    Owner,      // Full control (delete project, manage all settings, transfer ownership)
    Manager,    // Manage tasks, invite/remove members, update settings (cannot delete project)
    Member      // Create/edit own tasks, view project, comment
}
```

---

### 4. Task

**Purpose**: Represents an individual work item within a project with status, assignment, and metadata.

**Table**: `Tasks`

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | int (PK) | AUTO_INCREMENT | Task unique identifier |
| Title | string | NOT NULL | Task title (1-200 chars) |
| Description | string | NULL | Optional task description (max 5000 chars) |
| Status | string | NOT NULL | To Do, In Progress, In Review, Done |
| Priority | string | NOT NULL | Low, Medium, High |
| ProjectId | int (FK) | NOT NULL | Project containing this task |
| AssigneeId | string (FK) | NULL | User assigned to task (nullable) |
| CreatedBy | string (FK) | NOT NULL | User who created task |
| DueDate | datetime | NULL | Task due date (UTC) |
| IsDeleted | bool | NOT NULL, DEFAULT false | Soft delete flag (shadow property) |
| CreatedTimestamp | datetime | NOT NULL | Task creation timestamp (UTC) |
| UpdatedTimestamp | datetime | NOT NULL | Last update timestamp (UTC) |

**Relationships**:
- Many-to-One with `Project` (parent project)
- Many-to-One with `User` as `Assignee` (assigned user, nullable)
- Many-to-One with `User` as `CreatedBy` (task creator)
- One-to-Many with `Comment` (task comments)
- One-to-Many with `TaskHistory` (audit trail)

**Validation Rules**:
- Title required, 1-200 characters
- Description max 5000 characters
- Status must be one of: ToDo, InProgress, InReview, Done
- Priority must be one of: Low, Medium, High
- ProjectId must reference existing project
- AssigneeId must reference existing user (if provided)
- DueDate must be in future (if provided)

**Indexes**:
- Index on `ProjectId` (query all tasks in project)
- Index on `AssigneeId` (query tasks assigned to user)
- Index on `Status` (filter tasks by status for Kanban)
- Index on `CreatedTimestamp` (sort by creation date)
- Composite index on `(ProjectId, Status)` (Kanban board queries)

**State Transitions**:
```
ToDo → InProgress → InReview → Done
  ↑        ↓           ↓         ↓
  └────────┴───────────┴─────────┘
(Any status can transition back to ToDo for reopening)
```

**Domain Behavior**:
```csharp
public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int ProjectId { get; set; }
    public string? AssigneeId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ApplicationUser? Assignee { get; set; }
    public ApplicationUser Creator { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
    
    // Business methods
    public bool IsOverdue() => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != TaskStatus.Done;
    public bool IsAssignedTo(string userId) => AssigneeId == userId;
    public bool CanBeEditedBy(string userId, ProjectRole role) 
        => role == ProjectRole.Owner || role == ProjectRole.Manager || AssigneeId == userId;
}

public enum TaskStatus
{
    ToDo,
    InProgress,
    InReview,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}
```

---

### 5. TaskHistory (Audit Trail)

**Purpose**: Records all changes to tasks for audit trail and history tracking.

**Table**: `TaskHistory`

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | int (PK) | AUTO_INCREMENT | History record unique identifier |
| TaskId | int (FK) | NOT NULL | Task being changed |
| ChangedBy | string (FK) | NOT NULL | User who made the change |
| ChangeType | string | NOT NULL | StatusChanged, AssigneeChanged, etc. |
| OldValue | string | NULL | Previous value (JSON or string) |
| NewValue | string | NULL | New value (JSON or string) |
| ChangedTimestamp | datetime | NOT NULL | When change occurred (UTC) |

**Relationships**:
- Many-to-One with `Task` (task being tracked)
- Many-to-One with `User` as `ChangedBy` (user who made change)

**Validation Rules**:
- TaskId must reference existing task
- ChangedBy must reference existing user
- ChangeType must be one of: StatusChanged, AssigneeChanged, TitleChanged, DescriptionChanged, PriorityChanged, DueDateChanged
- ChangedTimestamp required

**Indexes**:
- Index on `TaskId` (query history for specific task)
- Index on `ChangedTimestamp` (sort history chronologically)
- Composite index on `(TaskId, ChangedTimestamp)` (task timeline queries)

**Domain Behavior**:
```csharp
public class TaskHistory
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public TaskChangeType ChangeType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Task Task { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    
    // Factory methods
    public static TaskHistory StatusChange(int taskId, string userId, TaskStatus oldStatus, TaskStatus newStatus)
        => new() { TaskId = taskId, ChangedBy = userId, ChangeType = TaskChangeType.StatusChanged, 
                   OldValue = oldStatus.ToString(), NewValue = newStatus.ToString() };
    
    public static TaskHistory AssigneeChange(int taskId, string userId, string? oldAssigneeId, string? newAssigneeId)
        => new() { TaskId = taskId, ChangedBy = userId, ChangeType = TaskChangeType.AssigneeChanged, 
                   OldValue = oldAssigneeId, NewValue = newAssigneeId };
}

public enum TaskChangeType
{
    StatusChanged,
    AssigneeChanged,
    TitleChanged,
    DescriptionChanged,
    PriorityChanged,
    DueDateChanged
}
```

---

### 6. Comment

**Purpose**: User comments/discussions on tasks for collaboration.

**Table**: `Comments`

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | int (PK) | AUTO_INCREMENT | Comment unique identifier |
| Content | string | NOT NULL | Comment text (1-5000 chars) |
| TaskId | int (FK) | NOT NULL | Task being commented on |
| AuthorId | string (FK) | NOT NULL | User who wrote comment |
| IsDeleted | bool | NOT NULL, DEFAULT false | Soft delete flag (shadow property) |
| CreatedTimestamp | datetime | NOT NULL | Comment creation timestamp (UTC) |
| EditedTimestamp | datetime | NULL | Last edit timestamp (UTC, null if never edited) |

**Relationships**:
- Many-to-One with `Task` (task being commented on)
- Many-to-One with `User` as `Author` (comment author)

**Validation Rules**:
- Content required, 1-5000 characters
- TaskId must reference existing task
- AuthorId must reference existing user
- Content cannot be empty or whitespace

**Indexes**:
- Index on `TaskId` (query all comments for task)
- Index on `CreatedTimestamp` (sort comments chronologically)
- Composite index on `(TaskId, CreatedTimestamp)` (task comment timeline)

**Domain Behavior**:
```csharp
public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime? EditedTimestamp { get; set; }
    
    // Navigation properties
    public Task Task { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
    
    // Business methods
    public bool IsEdited() => EditedTimestamp.HasValue;
    public void Edit(string newContent)
    {
        Content = newContent;
        EditedTimestamp = DateTime.UtcNow;
    }
}
```

---

### 7. Notification

**Purpose**: User notifications for task assignments, status changes, comments, etc.

**Table**: `Notifications`

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | int (PK) | AUTO_INCREMENT | Notification unique identifier |
| RecipientId | string (FK) | NOT NULL | User receiving notification |
| Type | string | NOT NULL | TaskAssigned, StatusChanged, CommentAdded, etc. |
| Content | string | NOT NULL | Notification message text |
| RelatedTaskId | int (FK) | NULL | Related task (if applicable) |
| IsRead | bool | NOT NULL, DEFAULT false | Whether user has read notification |
| CreatedTimestamp | datetime | NOT NULL | Notification creation timestamp (UTC) |

**Relationships**:
- Many-to-One with `User` as `Recipient` (notification recipient)
- Many-to-One with `Task` as `RelatedTask` (optional related task)

**Validation Rules**:
- RecipientId must reference existing user
- Type must be one of: TaskAssigned, StatusChanged, CommentAdded, ProjectInvitation
- Content required, max 500 characters
- RelatedTaskId must reference existing task (if provided)

**Indexes**:
- Index on `RecipientId` (query all notifications for user)
- Index on `IsRead` (filter unread notifications)
- Index on `CreatedTimestamp` (sort notifications by recency)
- Composite index on `(RecipientId, IsRead, CreatedTimestamp)` (user notification inbox)

**Domain Behavior**:
```csharp
public class Notification
{
    public int Id { get; set; }
    public string RecipientId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? RelatedTaskId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser Recipient { get; set; } = null!;
    public Task? RelatedTask { get; set; }
    
    // Factory methods
    public static Notification TaskAssigned(string recipientId, int taskId, string taskTitle)
        => new() { RecipientId = recipientId, Type = NotificationType.TaskAssigned, 
                   Content = $"You have been assigned to task: {taskTitle}", RelatedTaskId = taskId };
    
    public static Notification StatusChanged(string recipientId, int taskId, string taskTitle, TaskStatus newStatus)
        => new() { RecipientId = recipientId, Type = NotificationType.StatusChanged, 
                   Content = $"Task '{taskTitle}' status changed to {newStatus}", RelatedTaskId = taskId };
}

public enum NotificationType
{
    TaskAssigned,
    StatusChanged,
    CommentAdded,
    ProjectInvitation,
    TaskDueSoon
}
```

---

### 8. ProjectInvitation

**Purpose**: Email-based project invitations with acceptance/decline tracking.

**Table**: `ProjectInvitations`

| Column Name | Type | Constraints | Description |
|-------------|------|-------------|-------------|
| Id | int (PK) | AUTO_INCREMENT | Invitation unique identifier |
| Email | string | NOT NULL | Invitee email address |
| ProjectId | int (FK) | NOT NULL | Project being invited to |
| InviterId | string (FK) | NOT NULL | User who sent invitation |
| Role | string | NOT NULL | Invited role: Manager or Member |
| Status | string | NOT NULL | Pending, Accepted, Declined, Expired |
| Token | string | NOT NULL, UNIQUE | Unique invitation token (for acceptance link) |
| CreatedTimestamp | datetime | NOT NULL | Invitation sent timestamp (UTC) |
| ExpiresTimestamp | datetime | NOT NULL | Invitation expiration (14 days from creation) |
| RespondedTimestamp | datetime | NULL | When invitation was accepted/declined |

**Relationships**:
- Many-to-One with `Project` (project for invitation)
- Many-to-One with `User` as `Inviter` (user who sent invitation)

**Validation Rules**:
- Email required, valid format
- ProjectId must reference existing project
- InviterId must reference existing user
- Role must be one of: Manager, Member (cannot invite as Owner)
- Token must be unique, cryptographically random (GUID)
- ExpiresTimestamp must be after CreatedTimestamp

**Indexes**:
- Unique index on `Token` (lookup invitation by token)
- Index on `Email` (query invitations for email)
- Index on `ProjectId` (query all invitations for project)
- Index on `Status` (filter by invitation status)
- Composite index on `(Email, ProjectId, Status)` (prevent duplicate pending invitations)

**Domain Behavior**:
```csharp
public class ProjectInvitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string InviterId { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string Token { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresTimestamp { get; set; } = DateTime.UtcNow.AddDays(14);
    public DateTime? RespondedTimestamp { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ApplicationUser Inviter { get; set; } = null!;
    
    // Business methods
    public bool IsExpired() => DateTime.UtcNow > ExpiresTimestamp;
    public bool IsPending() => Status == InvitationStatus.Pending && !IsExpired();
    
    public void Accept()
    {
        if (!IsPending()) throw new InvalidOperationException("Invitation is not pending");
        Status = InvitationStatus.Accepted;
        RespondedTimestamp = DateTime.UtcNow;
    }
    
    public void Decline()
    {
        if (!IsPending()) throw new InvalidOperationException("Invitation is not pending");
        Status = InvitationStatus.Declined;
        RespondedTimestamp = DateTime.UtcNow;
    }
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Declined,
    Expired
}
```

---

## Database Schema Summary

| Table | Rows (Est. MVP) | Primary Access Pattern |
|-------|-----------------|------------------------|
| AspNetUsers | 10,000 | Query by ID, Email, RefreshToken |
| Projects | 5,000 | Query by ID, OwnerId, IsArchived |
| ProjectMembers | 20,000 | Query by UserId, ProjectId (composite) |
| Tasks | 1,000,000 | Query by ProjectId + Status (Kanban), AssigneeId (My Tasks) |
| TaskHistory | 5,000,000 | Query by TaskId + timestamp (audit trail) |
| Comments | 500,000 | Query by TaskId + timestamp (task discussions) |
| Notifications | 2,000,000 | Query by RecipientId + IsRead + timestamp (notification inbox) |
| ProjectInvitations | 50,000 | Query by Token (acceptance), Email + ProjectId (duplicates) |

---

## Soft Delete Strategy

Entities with soft delete (via `IsDeleted` shadow property):
- **Project**: Maintain history, prevent accidental data loss
- **Task**: Keep task data for reporting, prevent breaking TaskHistory references
- **Comment**: Preserve discussion context

Soft delete implementation in EFCore:
```csharp
// DbContext configuration
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);
    builder.Entity<Task>().HasQueryFilter(t => !t.IsDeleted);
    builder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
}

// Soft delete method in repository
public async Task SoftDeleteAsync<T>(T entity) where T : class
{
    _context.Entry(entity).Property("IsDeleted").CurrentValue = true;
    await _context.SaveChangesAsync();
}
```

---

## Data Integrity & Constraints

**Foreign Key Cascade Behavior**:
- `Project.OwnerId` → `User.Id`: **RESTRICT** (cannot delete user who owns projects)
- `ProjectMember.UserId` → `User.Id`: **CASCADE** (remove memberships if user deleted)
- `ProjectMember.ProjectId` → `Project.Id`: **CASCADE** (remove memberships if project deleted)
- `Task.ProjectId` → `Project.Id`: **CASCADE** (delete tasks if project deleted)
- `Task.AssigneeId` → `User.Id`: **SET NULL** (unassign task if assignee deleted)
- `Comment.TaskId` → `Task.Id`: **CASCADE** (delete comments if task deleted)
- `TaskHistory.TaskId` → `Task.Id`: **CASCADE** (delete history if task deleted)
- `Notification.RelatedTaskId` → `Task.Id`: **SET NULL** (keep notification, remove task reference)

**Unique Constraints**:
- `AspNetUsers.Email`: Unique (enforced by Identity)
- `ProjectMembers.(UserId, ProjectId)`: Unique composite (one role per user per project)
- `ProjectInvitations.Token`: Unique (secure invitation acceptance)

**Check Constraints** (EFCore 7+ or raw SQL):
- `Project.Name`: Length between 1 and 100
- `Task.Title`: Length between 1 and 200
- `Comment.Content`: Length between 1 and 5000
- `Task.DueDate`: Must be in future (if provided)
- `ProjectInvitation.ExpiresTimestamp`: Must be after CreatedTimestamp

---

## Performance Considerations

**Indexed Queries** (aligned with user stories):
1. **Kanban Board**: `SELECT * FROM Tasks WHERE ProjectId = ? AND Status IN (?, ?, ?, ?) ORDER BY CreatedTimestamp` → Index on `(ProjectId, Status)`
2. **My Tasks**: `SELECT * FROM Tasks WHERE AssigneeId = ? ORDER BY DueDate` → Index on `AssigneeId`
3. **User Projects**: `SELECT * FROM Projects JOIN ProjectMembers ON ... WHERE UserId = ?` → Index on `ProjectMembers.UserId`
4. **Task History**: `SELECT * FROM TaskHistory WHERE TaskId = ? ORDER BY ChangedTimestamp DESC` → Index on `(TaskId, ChangedTimestamp)`
5. **User Notifications**: `SELECT * FROM Notifications WHERE RecipientId = ? AND IsRead = false ORDER BY CreatedTimestamp DESC LIMIT 50` → Index on `(RecipientId, IsRead, CreatedTimestamp)`

**Pagination**:
- Use `Skip()` and `Take()` with `ORDER BY` for large result sets (task lists, history, comments)
- Default page size: 50 items
- Max page size: 100 items (prevent large payloads)

**Query Optimization**:
- Use `.AsNoTracking()` for read-only queries (Kanban board, reports)
- Use `.Include()` for eager loading navigation properties (prevent N+1 queries)
- Use `.Select()` projections to fetch only needed columns (DTOs)
- Consider materialized views for complex aggregate queries (dashboard metrics)

---

## Migration Strategy

**Initial Migration**: `dotnet ef migrations add InitialCreate`

Creates all tables with indexes and foreign keys.

**Seed Data** (for development):
```csharp
// Add seed data in DbContext.OnModelCreating
modelBuilder.Entity<ApplicationUser>().HasData(
    new ApplicationUser { Id = "user1", Email = "admin@example.com", ... }
);

modelBuilder.Entity<Project>().HasData(
    new Project { Id = 1, Name = "Demo Project", OwnerId = "user1" }
);
```

**Production Migrations**:
- Generate migration: `dotnet ef migrations add AddTaskPriorityField`
- Review generated SQL: `dotnet ef migrations script`
- Apply in CI/CD: `dotnet ef database update --connection "..."`
- Rollback if needed: `dotnet ef database update PreviousMigration`

---

## Next Steps

1. Generate API contracts (OpenAPI) for all endpoints interacting with these entities
2. Implement EFCore configurations in `Data/Configurations/` folder
3. Create repositories with typed methods for each entity
4. Implement domain business logic in entity classes
5. Create DTOs for request/response models aligned with these entities
6. Write xUnit tests for repository methods and entity business logic
