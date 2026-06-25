# Task Management Backend API

## Project Overview

This repository contains a .NET Task Management Backend API built for the Anyware Software .NET backend technical task. The API supports user registration, login, current-user profile retrieval, admin user management, task management, JWT authentication, Redis caching, SQL Server persistence, domain events, and simple background task processing.

The implementation was verified against the provided technical task PDF and the current source code. Functionality that is not present in the codebase is explicitly called out in the compliance matrix and notes below.

## Architecture

The solution uses a DDD-style, layered architecture with four projects:

| Project | Responsibility |
|---|---|
| `API` | ASP.NET Core Web API controllers, OpenAPI/Swagger configuration, authorization attributes, global error handling, application startup |
| `Application` | CQRS request/handler logic with MediatR, DTOs, validation, mapping, contracts, domain event handlers |
| `Domain` | Entities, enums, auditable base types, and domain events |
| `Infrastructure` | EF Core persistence, SQL Server configuration, Redis cache implementation, JWT/password services, current-user service, background worker, database seeding |

Primary flow:

1. Controllers receive HTTP requests and send commands/queries through MediatR.
2. Application handlers enforce business rules and use application contracts.
3. Infrastructure implements persistence, authentication, caching, and background processing.
4. EF Core save interceptors update audit fields, soft-delete entities, and publish domain events after successful saves.

## Technology Stack

- .NET `10.0`
- ASP.NET Core Web API
- Entity Framework Core `10.0.9`
- SQL Server
- Redis via `StackExchange.Redis`
- JWT bearer authentication
- MediatR
- FluentValidation
- AutoMapper
- ASP.NET Core OpenAPI with Swagger UI
- Docker and Docker Compose

## Features

- User registration and login
- JWT access token generation
- Current authenticated user profile endpoint
- Seeded users, including a default admin
- Admin-only user list, user lookup, user creation, user update, and user deletion
- Authenticated task creation, listing, lookup, status update, and deletion
- Task ownership checks for task list, task lookup including Redis cache hits, status update, and deletion
- Duplicate task prevention for the same user, same title, and same UTC day
- Task ordering by priority descending, then creation date ascending
- Redis caching for `Get Task by ID`
- Cache invalidation when a task is updated or deleted
- In-memory task queue and hosted background worker
- Domain events for task creation, update, and deletion
- Soft delete support through EF Core save interception
- Global exception handling with Problem Details responses
- Request validation through FluentValidation
- Structured request logging through a MediatR pipeline behavior

## Database Design

The application uses SQL Server through EF Core. Migrations are stored in `Infrastructure/Persistance/Migrations`, and pending migrations are applied at application startup.

### Tables

#### `Users`

| Column | Notes |
|---|---|
| `Id` | Integer primary key, identity |
| `Name` | Required string |
| `Email` | Required string, unique index |
| `HashedPassword` | Required string, stored with ASP.NET Core password hashing |
| `Role` | Integer enum: `User = 1`, `Admin = 2` |
| `CreatedAt` | UTC creation timestamp |
| `CreatedBy` | Nullable creator user id |
| `UpdatedAt` | Nullable update timestamp |
| `UpdatedBy` | Nullable updater user id |
| `IsDeleted` | Soft-delete flag |
| `DeletedAt` | Nullable deletion timestamp |
| `DeletedBy` | Nullable deleter user id |

#### `TaskItems`

| Column | Notes |
|---|---|
| `Id` | Integer primary key, identity |
| `Title` | Required string |
| `Description` | Required string |
| `Status` | Integer enum: `Pending = 1`, `InProgress = 2`, `Done = 3` |
| `Priority` | Integer enum: `Low = 1`, `Medium = 2`, `High = 3` |
| `CreatedAt` | UTC creation timestamp |
| `OwnerId` | Required foreign key to `Users.Id` |
| `CreatedBy` | Nullable creator user id |
| `UpdatedAt` | Nullable update timestamp |
| `UpdatedBy` | Nullable updater user id |
| `IsDeleted` | Soft-delete flag |
| `DeletedAt` | Nullable deletion timestamp |
| `DeletedBy` | Nullable deleter user id |

### Relationships and Filters

- `User` has many `TaskItems`.
- `TaskItem.OwnerId` references `User.Id`.
- The EF model configures cascade delete at the relationship level.
- The save interceptor converts deletes into soft deletes.
- A global query filter excludes `AuditableEntity` records where `IsDeleted = true`.

## API Endpoints

Swagger UI is enabled in the Development environment at:

- `/swagger`

The OpenAPI document is mapped at:

- `/openapi/v1.json`

### Authentication

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/api/Auth/register` | Anonymous | Register a user and return a JWT |
| `POST` | `/api/Auth/login` | Anonymous | Login and return a JWT |
| `GET` | `/api/Auth/me` | Authenticated | Return the current user's profile |

### Users

All user-management endpoints require the `Admin` role.

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/api/Users` | Admin | List users |
| `GET` | `/api/Users/{id}` | Admin | Get user by id |
| `POST` | `/api/Users` | Admin | Create user |
| `PUT` | `/api/Users/{id}` | Admin | Update user |
| `DELETE` | `/api/Users/{id}` | Admin | Soft-delete user |

### Tasks

All task endpoints require authentication.

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/api/Tasks` | Authenticated | List tasks owned by the current user |
| `GET` | `/api/Tasks/{id}` | Authenticated | Get a task by id, using Redis cache |
| `POST` | `/api/Tasks` | Authenticated | Create a task for the current user |
| `PUT` | `/api/Tasks/{id}/status` | Authenticated | Update owned task status |
| `DELETE` | `/api/Tasks/{id}` | Authenticated | Soft-delete owned task |

## Authentication & Authorization

Authentication uses JWT bearer tokens.

JWT claims include:

- `jti`
- `ClaimTypes.NameIdentifier`
- `ClaimTypes.Email`
- `ClaimTypes.Role`

Authorization is implemented with:

- `[Authorize]` for protected authenticated endpoints.
- `HasRoleAttribute`, which sets the ASP.NET Core authorization `Roles` property from `UserRole` enum values.
- `[HasRole(UserRole.Admin)]` on `UsersController`.

Passwords are hashed with `Microsoft.AspNetCore.Identity.PasswordHasher<User>`.

There is no refresh token implementation in the current codebase.

## Caching Strategy

Redis is used for the `GET /api/Tasks/{id}` endpoint.

Implementation details:

- Cache abstraction: `Application.Common.Contracts.ICache`
- Redis implementation: `Infrastructure.Caching.RedisCacheService`
- Cache key format: `TaskItem_{taskId}`
- Expiration: 5 minutes
- Cache invalidation: `InvalidateTaskItemCacheEventHandler` removes the task cache entry when a `TaskItemUpdatedEvent` or `TaskItemDeletedEvent` is published.
- Ownership check: the task returned from Redis is still validated against the current user before the response is returned.

Verified behavior:

1. On cache miss, the task is loaded from SQL Server and stored in Redis.
2. On cache hit, the cached task is returned.
3. On task update or delete, the cache entry is removed.

## Background Processing

Background processing uses an in-memory queue and a hosted service.

Components:

- `ITaskQueue`
- `TaskQueue`, backed by `ConcurrentQueue<int>`
- `TaskProcessingWorker`, implemented as `BackgroundService`
- `TaskWorkerSettings`

When a task is created:

1. `CreateTaskCommandHandler` creates the task and adds a `TaskItemCreatedEvent`.
2. `PublishDomainEventsInterceptor` publishes the event after `SaveChangesAsync`.
3. `QueueTaskItemForProcessingEventHandler` enqueues the task id.
4. `TaskProcessingWorker` dequeues task ids on a configured interval.
5. The worker sets the task status to `InProgress`, waits for the configured processing time, then sets the task status to `Done`.

When a task is manually updated back to `Pending`, `ReQueueTaskItemForProcessingEventHandler` queues it again.

## Domain Events

The domain event infrastructure is implemented in `Domain.Shared.EntityBase` and `Domain.Shared.EventBase`.

Implemented events:

- `TaskItemCreatedEvent`
- `TaskItemUpdatedEvent`
- `TaskItemDeletedEvent`

Events are captured and cleared during EF Core `SavingChangesAsync`, then published through MediatR after successful save in `SavedChangesAsync`.

Implemented handlers:

- Queue task for processing after creation
- Re-queue task for processing when updated to `Pending`
- Invalidate Redis cache after task update or deletion

## Configuration

Configuration is read from `API/appsettings.json`, environment variables, and standard ASP.NET Core configuration sources.

### Connection Strings

```json
{
  "ConnectionStrings": {
    "SqlConnection": "Server=localhost\\SQLEXPRESS;Database=TaskManagement;Trusted_Connection=True;TrustServerCertificate=True;",
    "RedisConnection": "localhost:6379"
  }
}
```

### JWT

```json
{
  "Jwt": {
    "Key": "SECRETTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT",
    "Issuer": "Ibrahim-Moawad",
    "Audience": "Ibrahim-Moawad",
    "ExpiryMinutes": 60
  }
}
```

For production, replace the configured JWT key with a secure secret provided through environment variables or a secret manager.

### Task Worker

```json
{
  "TaskWorker": {
    "PollInterval": "00:00:10",
    "ProcessingTime": "00:00:05"
  }
}
```

### Seeded Users

The application seeds users only when the `Users` table is empty.

| Role | Email | Password |
|---|---|---|
| Admin | `admin@example.com` | `Admin123!` |
| User | `user@example.com` | `User123!` |

## Local Development Setup

### Prerequisites

- .NET SDK compatible with `net10.0`
- SQL Server or SQL Server Express
- Redis

### Run Locally

1. Update `API/appsettings.json` connection strings if needed.
2. Start SQL Server and Redis.
3. Restore and build:

```bash
dotnet restore AnywareSoftwareTechnicalTask.slnx
dotnet build AnywareSoftwareTechnicalTask.slnx
```

4. Run the API:

```bash
dotnet run --project API/API.csproj
```

5. Open Swagger UI in Development mode:

```text
https://localhost:<port>/swagger
```

The application applies EF Core migrations and seeds configured users during startup.

## Assumptions

- The assignment allows JWT authentication without refresh tokens; refresh tokens are not implemented in the current codebase.
- The assignment allows a simple background worker, so the queue is in memory rather than a durable external queue.
- Task duplicate detection is based on the current UTC date because the application stores audit dates with `DateTime.UtcNow`.
- Redis is used only for the `Get Task by ID` endpoint, matching the PDF requirement.
- Swagger/OpenAPI is enabled only in the Development environment.

## Docker Setup

The repository includes:

- `API/Dockerfile`
- `docker-compose.yml`

Docker Compose starts:

- SQL Server 2022 Developer container
- Redis container
- API container exposed on host port `8080`

Run:

```bash
docker compose up --build
```

Docker Compose configures the API with:

```text
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__SqlConnection=server=sqlserver;Database=TaskManagerDB;User Id=sa;Password=StrongPassw0rd!;TrustServerCertificate=True;
ConnectionStrings__RedisConnection=redis:6379
```

After startup, Swagger should be available at:

```text
http://localhost:8080/swagger
```

## Testing

No automated test project is present in the repository.

Verified locally:

```bash
dotnet build AnywareSoftwareTechnicalTask.slnx
```

Result:

- Build succeeded.
- 13 compiler warnings were reported, mostly nullable-reference warnings, one obsolete EF Core API warning, and possible null Redis connection configuration warnings.

## Technical Task Compliance Matrix

| Requirement | Status | Implementation |
|------------|---------|----------------|
| Build a Task Management Backend API using .NET | ✅ Implemented | ASP.NET Core Web API solution targeting `net10.0` |
| Clean DDD-style project structure | ✅ Implemented | Separate `API`, `Application`, `Domain`, and `Infrastructure` projects |
| RESTful API development | ✅ Implemented | Controllers expose auth, user, and task endpoints under `/api/*` |
| Swagger documentation | ✅ Implemented | OpenAPI and Swagger UI enabled in Development; JWT bearer scheme added |
| Redis caching | ✅ Implemented | `GET /api/Tasks/{id}` uses Redis via `RedisCacheService` |
| Simple background processing / queue handling | ✅ Implemented | `TaskQueue` and `TaskProcessingWorker` process created tasks |
| Basic authentication | ✅ Implemented | JWT login/register flow and bearer authentication |
| Basic authorization | ✅ Implemented | Protected task/current-user endpoints and admin-only user-management endpoints |
| Database seeding | ✅ Implemented | Startup initializer migrates DB and seeds configured users when `Users` is empty |
| User registration | ✅ Implemented | `POST /api/Auth/register` |
| User login | ✅ Implemented | `POST /api/Auth/login` |
| Get current user profile | ✅ Implemented | `GET /api/Auth/me` |
| Passwords stored securely using hashing | ✅ Implemented | `PasswordHasher<User>` stores hashed passwords |
| Seed default admin user | ✅ Implemented | `admin@example.com` / `Admin123!` configured in `appsettings.json` |
| Admin can create/add users | ✅ Implemented | `POST /api/Users`, restricted to `Admin` |
| Admin can delete users | ✅ Implemented | `DELETE /api/Users/{id}`, restricted to `Admin`; implemented as soft delete |
| Admin can view users list | ✅ Implemented | `GET /api/Users`, restricted to `Admin` |
| Create task | ✅ Implemented | `POST /api/Tasks` |
| Get task by ID | ✅ Implemented | `GET /api/Tasks/{id}` |
| Get all tasks | ✅ Implemented | `GET /api/Tasks`, scoped to current user |
| Update task status | ✅ Implemented | `PUT /api/Tasks/{id}/status` |
| Task has Id, Title, Description, Status, Priority, CreatedAt, UserId | ✅ Implemented | `TaskItem` has `Id`, `Title`, `Description`, `Status`, `Priority`, `CreatedAt`, and `OwnerId` |
| Task status values Pending, InProgress, Done | ✅ Implemented | `TaskItemStatus` enum |
| Logged-in user can create tasks | ✅ Implemented | `TasksController` requires `[Authorize]`; owner comes from current user |
| Each task belongs to a specific user | ✅ Implemented | `TaskItem.OwnerId` foreign key |
| Logged-in user can only view own tasks | ✅ Implemented | List query filters by owner; task-by-id validates owner on database reads and cached Redis reads |
| Logged-in user can only update own tasks | ✅ Implemented | Status update queries by task id and current user id |
| Only admin can add/delete users | ✅ Implemented | `UsersController` is restricted with `[HasRole(UserRole.Admin)]` |
| Created task is saved to database | ✅ Implemented | `CreateTaskCommandHandler` saves task through EF Core |
| Created task is sent to background processing | ✅ Implemented | `TaskItemCreatedEvent` queues task id after save |
| Background worker can simulate processing and update task | ✅ Implemented | Worker sets `InProgress`, delays, then sets `Done` |
| Cache first `Get Task by ID` from database | ✅ Implemented | Query reads DB and caches task on cache miss |
| Subsequent `Get Task by ID` from Redis | ✅ Implemented | Query returns Redis value when present |
| Invalidate or refresh cache when task is updated | ✅ Implemented | Update event removes `TaskItem_{id}` cache key |
| Business logic: sort by priority first, then creation date | ✅ Implemented | `OrderByDescending(Priority).ThenBy(CreatedAt)` |
| Business logic: prevent duplicate title on same day for same user | ✅ Implemented | Create handler checks same owner, title, and UTC date |
| SQL Server or PostgreSQL relational database | ✅ Implemented | SQL Server via EF Core |
| Global exception handling bonus | ✅ Implemented | `GlobalExceptionHandler` and `UseStatusCodePages` |
| Docker support bonus | ✅ Implemented | Dockerfile and Compose for API, SQL Server, Redis |
| Unit tests bonus | ❌ Not Implemented | No test project or automated tests found |
| Clean logging bonus | ✅ Implemented | MediatR logging behavior and worker/cache logging |
| Refresh token implementation bonus | ❌ Not Implemented | No refresh token model, endpoint, or service found in the current codebase |
| Soft delete for users bonus | ✅ Implemented | Save interceptor converts deletes to soft deletes for auditable entities |
| Short video walkthrough deliverable | ❌ Not Implemented | No video artifact is present in the repository |

## Design Decisions

- CQRS with MediatR keeps controllers thin and moves business rules into application handlers.
- Infrastructure concerns are accessed through application contracts, keeping the application layer independent from Redis, EF Core implementation details, JWT generation, and password hashing.
- EF Core interceptors centralize audit-field updates, soft-delete behavior, and domain-event publishing.
- Domain events decouple task lifecycle side effects from command handlers.
- The background queue is intentionally in-memory because the assignment allowed a simple `BackgroundService` and did not require an external queue.
- Redis caching is limited to task-by-id reads, matching the technical task requirement.
- Swagger is only enabled in Development, which matches common ASP.NET Core production hardening practices.

## Future Improvements

- Add automated tests for authentication, authorization, task ownership, duplicate-task validation, cache invalidation, and background processing.
- Replace development JWT secrets and SQL credentials with secret-manager or environment-specific production secrets.
- Add the planned refresh token flow if longer-lived sessions are required.
- Add pagination and filtering for user and task list endpoints.
- Initialize non-nullable entity properties or mark them as `required` to remove nullable-reference build warnings.
- Update the EF Core query filter helper to avoid the obsolete `GetQueryFilter()` API warning.
- Replace the in-memory queue with a durable queue if task processing must survive application restarts.

## Notes for Reviewers

- The source of truth for assignment requirements was the provided `NET Backend Developer Technical Task.pdf`.
- The seeded admin credentials are `admin@example.com` / `Admin123!`.
- The application also seeds a normal user: `user@example.com` / `User123!`.
- Database migrations and seeding run automatically during startup.
- Task deletion is implemented even though the PDF only explicitly required create, get, list, and status update.
- User deletion and task deletion are soft deletes through the shared auditable-entity interceptor.
- Ownership is enforced for task reads from both SQL Server and Redis.
- The solution currently has no automated test project.
- The solution was verified with `dotnet build AnywareSoftwareTechnicalTask.slnx`; the build succeeded with warnings.

## Walkthrough Guide

This section maps the implementation to the topics requested for the task walkthrough.

| Topic | What to Show |
|---|---|
| Project structure | Four projects: `API`, `Application`, `Domain`, and `Infrastructure` |
| Architecture approach | DDD-style layering, CQRS with MediatR, application contracts, infrastructure implementations, EF Core interceptors |
| Authentication and authorization flow | Register/login returns JWT; protected routes use `[Authorize]`; admin routes use `[HasRole(UserRole.Admin)]` |
| Seeded admin user | `admin@example.com` / `Admin123!`, inserted on startup when the users table is empty |
| Implemented user and admin APIs | Auth endpoints plus admin-only user list, lookup, create, update, and delete |
| Implemented task APIs | Authenticated task list, lookup, create, status update, and delete |
| How Redis is used | `GET /api/Tasks/{id}` reads from Redis by `TaskItem_{id}`, falls back to SQL Server, caches for 5 minutes, validates ownership, and invalidates on update/delete |
| How background processing works | `TaskItemCreatedEvent` queues the task; `TaskProcessingWorker` marks it `InProgress`, waits, then marks it `Done` |
| Business logic added | Priority/date sorting and duplicate title prevention for the same user on the same UTC day |
