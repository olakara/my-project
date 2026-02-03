# Task Management Application with Real-Time Collaboration

A full-stack task management application built with .NET 8 Web API and React 18, featuring real-time collaboration via SignalR, role-based access control, and a Kanban board interface.

## 🚀 Features

- **User Authentication** - Secure registration and login with JWT tokens and refresh token rotation
- **Project Management** - Create projects, invite team members with roles (Owner/Manager/Member)
- **Kanban Board** - Drag-and-drop task management across columns (To Do, In Progress, In Review, Done)
- **Task Assignment** - Assign tasks to team members with notifications
- **Real-Time Collaboration** - Live updates via SignalR WebSockets for instant sync
- **Dashboard & Reports** - Task metrics, burndown charts, and team activity tracking
- **Mobile API Support** - RESTful API endpoints for mobile clients

## 🏗️ Architecture

### Backend (.NET 8 Web API)
- **Vertical Slice Architecture** - Features organized by domain (Auth, Projects, Tasks)
- **Entity Framework Core** - PostgreSQL database with code-first migrations
- **ASP.NET Core Identity** - User management with JWT authentication
- **FluentValidation** - Input validation with semantic rules
- **Serilog** - Structured logging with correlation IDs
- **SignalR** - Real-time WebSocket communication

### Frontend (React 18 + TypeScript)
- **Vite** - Fast build tool and dev server
- **Tailwind CSS** - Utility-first CSS framework
- **shadcn/ui** - Accessible component library with Radix UI
- **Zustand** - Lightweight state management
- **TanStack Query** - Server state management with caching
- **React Router** - Client-side routing

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/downloads)

## 🛠️ Setup Instructions

### 1. Clone Repository

```bash
git clone <repository-url>
cd my-project
git checkout 001-task-management
```

### 2. Database Setup

**Option A: Local PostgreSQL**
```sql
CREATE DATABASE taskmanagement;
CREATE USER taskuser WITH PASSWORD 'YourSecurePassword123!';
GRANT ALL PRIVILEGES ON DATABASE taskmanagement TO taskuser;
```

**Option B: Docker PostgreSQL**
```bash
docker run --name taskmanagement-postgres \
  -e POSTGRES_DB=taskmanagement \
  -e POSTGRES_USER=taskuser \
  -e POSTGRES_PASSWORD=YourSecurePassword123! \
  -p 5432:5432 \
  -d postgres:14
```

### 3. Backend Setup

```bash
cd backend/src/TaskManagement.Api

# Restore dependencies
dotnet restore

# Configure user secrets (never commit sensitive data)
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "YourSuperSecretJwtKeyThatIsAtLeast32CharactersLong!"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=taskmanagement;Username=taskuser;Password=YourSecurePassword123!"

# Apply database migrations
dotnet ef database update

# Run backend API
dotnet run
```

Backend will be available at: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`

### 4. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Create environment file
cp .env.example .env.local

# Run frontend development server
npm run dev
```

Frontend will be available at: `http://localhost:5173`

## 🧪 Running Tests

### Backend (xUnit)

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

### Frontend (Vitest + React Testing Library)

```bash
cd frontend

# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with UI
npm run test:ui
```

## 📁 Project Structure

```
my-project/
├── backend/
│   ├── src/
│   │   └── TaskManagement.Api/
│   │       ├── Domain/           # Domain entities (User, Project, Task, etc.)
│   │       ├── Data/             # EFCore DbContext, Configurations, Repositories
│   │       ├── Features/         # Vertical slices (Auth, Projects, Tasks, etc.)
│   │       ├── Hubs/             # SignalR hubs for real-time
│   │       ├── Middleware/       # Custom middleware (exception handling, correlation ID)
│   │       ├── Services/         # Shared services (JWT, email, notifications)
│   │       └── Program.cs        # Application entry point
│   └── tests/
│       ├── TaskManagement.Api.Tests/
│       └── TaskManagement.IntegrationTests/
│
├── frontend/
│   ├── src/
│   │   ├── components/           # Reusable UI components
│   │   ├── pages/                # Page components (routes)
│   │   ├── services/             # API clients, SignalR, auth
│   │   ├── hooks/                # Custom React hooks
│   │   ├── store/                # Zustand state management
│   │   ├── types/                # TypeScript type definitions
│   │   └── utils/                # Utility functions
│   └── tests/
│
└── specs/
    └── 001-task-management/
        ├── spec.md               # Feature specification
        ├── plan.md               # Technical implementation plan
        ├── tasks.md              # Implementation tasks
        ├── data-model.md         # Database schema and entities
        ├── quickstart.md         # Developer quickstart guide
        └── contracts/            # OpenAPI specifications
```

## 🔒 Security

- **Authentication**: JWT access tokens (15 min expiration) + refresh tokens (7 days)
- **Password Policy**: 12+ characters with uppercase, lowercase, digit, special character
- **Input Validation**: FluentValidation on all API requests
- **RBAC**: Role-based access control (Owner, Manager, Member)
- **HTTPS**: TLS required for all communication
- **Logging**: No sensitive data logged (passwords, tokens)
- **CORS**: Configured for frontend origin only

## 📊 Performance Targets

- Kanban board loads in **<2 seconds** for 500+ tasks
- Real-time updates sync within **<1 second** (99th percentile)
- Support **100 concurrent users** without degradation
- API response time **<200ms** p95 for CRUD operations
- **85%+ code coverage** with xUnit and React Testing Library tests

## 🤝 Contributing

### Git Workflow

Commits follow the format: `<type>(<task-id>): <description>`

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

Example:
```bash
git commit -m "feat(T030): implement user registration endpoint"
```

### Development Guidelines

1. **TDD Approach**: Write tests before implementation (AAA pattern with xUnit)
2. **Async/Await First**: All I/O operations must be async
3. **Null Safety**: Nullable reference types enabled (`string?` for nullable)
4. **Vertical Slices**: Each feature is self-contained in `Features/` folder
5. **Code Quality**: Use `dotnet format` and `npm run format` before committing

## 📚 Documentation

- [Feature Specification](./specs/001-task-management/spec.md) - User stories and requirements
- [Implementation Plan](./specs/001-task-management/plan.md) - Technical architecture and design
- [Tasks Breakdown](./specs/001-task-management/tasks.md) - Detailed implementation tasks
- [Data Model](./specs/001-task-management/data-model.md) - Database schema and entities
- [API Contracts](./specs/001-task-management/contracts/) - OpenAPI specifications

## 🛠️ Technology Stack

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- PostgreSQL 14+
- ASP.NET Core Identity
- FluentValidation 11.3
- Serilog 8.0
- SignalR (built-in)
- xUnit + Moq + FluentAssertions

### Frontend
- React 18.2
- TypeScript 5.2
- Vite 5.0
- Tailwind CSS 3.4
- shadcn/ui (Radix UI components)
- Zustand 4.4
- TanStack Query 5.14
- React Router 6.21
- SignalR Client 8.0
- Vitest + React Testing Library

## 📄 License

[MIT License](LICENSE)

## 👥 Team

- **Architecture**: Vertical Slice Architecture with CQRS principles
- **Status**: ✅ Phase 1 (Setup) Complete
- **Branch**: `001-task-management`
- **Target MVP**: US1 (Auth) + US2 (Projects) + US3 (Kanban Board)

---

**Built with ❤️ using .NET 8, React 18, and TypeScript**
