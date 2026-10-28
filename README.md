# TaskManager API

TaskManager is a .NET 9 controller-based REST API using PostgreSQL and Entity Framework Core 9. The solution is `TaskManager.sln`; the API is in `src/TaskManager.Api/`, and tests are in `tests/TaskManager.Api.Tests/`.

## Configure a local database connection

Install the .NET 9 SDK and a supported PostgreSQL server/client for your development environment. Configure a local PostgreSQL database named `TaskManagerDb`; keep any existing database intact. The examples below use placeholders for the host and local role. Substitute values from your own setup; do not commit credentials or paste them into project files, logs, or chat.

The API reads `ConnectionStrings:TaskManager` from .NET User Secrets first, then the `TASKMANAGER_CONNECTION_STRING` process environment variable. Configure User Secrets from a secure local terminal (the value is not shown here):

```powershell
dotnet user-secrets set "ConnectionStrings:TaskManager" "<local-connection-string>" --project src/TaskManager.Api/TaskManager.Api.csproj
```

Alternatively, set `TASKMANAGER_CONNECTION_STRING` in the current process using your local connection value. Do not put a real connection string in tracked configuration. A connection string generally contains a host, database, username, and—when required by local authentication—a password. Keep it in User Secrets or a protected environment setting.

Apply the version-controlled EF Core migrations to the intended local development database:

```powershell
dotnet ef database update --project src/TaskManager.Api/TaskManager.Api.csproj
```

The initial migration creates the `tasks` table. Never drop or recreate an existing database to apply migrations.

## Build and tests

```powershell
dotnet restore TaskManager.sln
dotnet build TaskManager.sln --no-restore
```

Database-independent validation, Swagger, and connection-safety tests can run without PostgreSQL:

```powershell
dotnet test tests/TaskManager.Api.Tests/TaskManager.Api.Tests.csproj --no-build --filter "FullyQualifiedName~ValidationTests|FullyQualifiedName~SwaggerDocumentTests|FullyQualifiedName~PostgresConnectionSafetyTests"
```

PostgreSQL integration tests require a separate local database named `TaskManagerTestDb`. Configure `TASKMANAGER_TEST_CONNECTION_STRING` in the current process or another secure local source. The tests validate that target before applying migrations or clearing rows. Never point it at the development or production database.

```powershell
dotnet test tests/TaskManager.Api.Tests/TaskManager.Api.Tests.csproj --filter "FullyQualifiedName~PostgresIntegrationTests"
```

## Run the API and use Swagger

After configuring the development connection and applying migrations:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj --urls http://localhost:5080
```

Swagger UI is at <http://localhost:5080/swagger> and OpenAPI JSON is at <http://localhost:5080/swagger/v1/swagger.json>. Swagger loading alone does not verify the database connection; a database-backed API request does.

Task routes:

- `POST /api/tasks`: create; title required, description and `dueDate` optional; returns `201` and a Location header.
- `GET /api/tasks`: list all tasks.
- `GET /api/tasks/{id}`: retrieve a task.
- `PUT /api/tasks/{id}`: update title, description, status, and due date.
- `PATCH /api/tasks/{id}/status`: change status to `Todo`, `InProgress`, or `Done`.
- `DELETE /api/tasks/{id}`: delete; returns `204`.

Validation failures return `400` validation problem details; missing tasks return `404`. Swagger is enabled only in Development and contains no database credentials.

## Development Workflow

Test changes locally before committing them and pushing them to GitHub.
