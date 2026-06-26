# Task Management Backend API

## Quick Access

- Live Demo URL: [http://142.132.210.239:8080/swagger/index.html](http://142.132.210.239:8080/swagger/index.html)
- GitHub Repository URL: [https://github.com/hema325/AnywareSoftwareTechnicalTask](https://github.com/hema325/AnywareSoftwareTechnicalTask)
- Docker Hub Repository URL: [https://hub.docker.com/r/hemamoawad/task-manager-software-anywhere-api](https://hub.docker.com/r/hemamoawad/task-manager-software-anywhere-api)

```bash
docker pull hemamoawad/task-manager-software-anywhere-api:latest
```

## Project Overview

This repository contains a .NET Task Management Backend API for the Anyware Software technical task. It implements JWT authentication, refresh-token rotation and revocation, seeded users, admin-only user management, authenticated task management, SQL Server persistence, Redis caching for task lookup, domain events, and a simple background worker that processes created tasks.

Main implemented features:

- Register, login, refresh token, revoke refresh token, and current-user profile APIs.
- Admin-only user list, lookup, create, update, and delete APIs.
- Authenticated task list, lookup, create, status update, and delete APIs.
- Task ownership checks so users can access only their own tasks.
- Duplicate task prevention for the same user, title, and UTC day.
- Task sorting by priority descending, then creation date ascending.
- Redis cache for `GET /api/Tasks/{id}` with invalidation on update/delete.
- Startup database migration and seeding.
- Docker Compose setup for API, SQL Server, and Redis.

## Setup Instructions

### Prerequisites

- .NET SDK compatible with `net10.0`.
- SQL Server or SQL Server Express.
- Redis.
- Docker and Docker Compose, if running the containerized setup.

### Required Software

- ASP.NET Core / .NET `10.0`.
- SQL Server 2022 or local SQL Server Express.
- Redis.
- Docker Desktop or Docker Engine with Compose support.

### Configuration Requirements

Configuration is read from `API/appsettings.json`, environment variables, and standard ASP.NET Core configuration sources.

Key settings:

- `ConnectionStrings:SqlConnection`
- `ConnectionStrings:RedisConnection`
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiryMinutes`
- `Jwt:RefreshTokenExpiryDays`
- `TaskWorker:PollInterval`
- `TaskWorker:ProcessingTime`
- `Seeds:Users`

For local non-Docker runs, update the SQL Server and Redis connection strings in `API/appsettings.json` if your machine uses different values.

### Database Setup

The API uses EF Core migrations stored under `Infrastructure/Persistance/Migrations`. On startup, the application checks for pending migrations, applies them, and seeds users when the `Users` table is empty.

For Docker runs, `docker-compose.yml` starts SQL Server, Redis, and the API with container-ready connection strings.

### Dependency Restoration

Restore dependencies from the repository root:

```bash
dotnet restore
```

## How to Run the Project

### Run Locally

Start SQL Server and Redis first, then run:

```bash
dotnet restore
dotnet build
dotnet run --project API
```

Swagger is enabled in the Development environment. Local launch settings expose:

- `http://localhost:5114/swagger`
- `https://localhost:7173/swagger`

### Run with Docker

```bash
docker compose up -d
```

Docker Compose exposes the API on host port `8080`. After startup, open:

```text
http://localhost:8080/swagger
```

The API container runs with `ASPNETCORE_ENVIRONMENT=Development`, so Swagger is available in the Docker Compose setup.

## Seeded Credentials

The application seeds these users from `API/appsettings.json` when the `Users` table is empty:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@example.com` | `Admin123!` |
| User | `user@example.com` | `User123!` |

## CI/CD

The workflow in `.github/workflows/CI-CD.yml` runs on `push` to `master` and can also be started manually with `workflow_dispatch`.

- Build process: GitHub Actions checks out the repository, installs .NET `10.0`, restores `API/API.csproj`, and builds it in Release mode.
- Docker image publishing: after a successful build, the workflow logs in to Docker Hub using `DOCKER_USERNAME` and `DOCKER_PASSWORD` secrets, then runs `docker compose build` and `docker compose push`.
- Deployment process: after publishing, the workflow connects to a server over SSH using repository secrets, changes to `/home/hema/Projects/TaskManager`, then runs `docker compose down`, `docker compose pull`, and `docker compose up -d`.
- How changes reach production: a push to `master` triggers build, Docker image publish, and server redeployment using the latest Compose image.

## Assumptions

- A simple in-memory background queue is acceptable for the task processing requirement.
- Duplicate task detection uses the current UTC date because audit timestamps are stored with `DateTime.UtcNow`.
- Redis caching is scoped to `GET /api/Tasks/{id}`.

## Deliverables

- ✅ Source Code Repository
- ✅ Docker Image
- ✅ Live Deployment
- ✅ Swagger Documentation
- ✅ CI/CD Pipeline
