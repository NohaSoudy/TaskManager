# Story 01 — Create Task Management API

## Prerequisites

- The C#/.NET 9 API, task controllers, EF Core model, Npgsql provider, initial migration, and test project already exist. Preserve them; this plan completes reproducible local setup, Swagger/OpenAPI, testing safeguards, and documentation.
- This is the initial feature; no sibling story dependencies exist. Keep the current root solution and project layout: `TaskManager.sln`, `src/TaskManager.Api/`, and `tests/TaskManager.Api.Tests/`.

---

## Story Goal

Make the TaskManager API setup reproducible on a clean Windows development environment. Check PostgreSQL prerequisites, safely create `TaskManagerDb` without replacing an existing database, configure local credentials securely, apply and verify version-controlled EF Core migrations, use an isolated test database, expose Swagger/OpenAPI in Development, and document/build/test/smoke the complete workflow.

Keep the existing API implementation and the C#/.NET 9, PostgreSQL, and EF Core stack intact. Add only the missing setup safeguards, Swagger support, tests, and documentation.

---

## Context — Read These Files First

1. `.squad/stories/taskmanager/create-task-management-api/intake.md` — Follow the API, reproducible setup, isolation, Swagger, and acceptance criteria.
2. `.squad/config.yaml` — Preserve C# as the primary language, GitHub tracker configuration, and global story numbering.
3. `src/TaskManager.Api/Program.cs` — Lines 5–15 register services and start the app; lines 9–11 read `ConnectionStrings:TaskManager` or `TASKMANAGER_CONNECTION_STRING`, fail clearly when absent, and register Npgsql. Preserve this logic while adding Swagger.
4. `src/TaskManager.Api/TaskManager.Api.csproj` — Line 1 targets .NET 9 and references EF Core Design/Npgsql. Add a .NET 9-compatible Swagger package here.
5. `src/TaskManager.Api/Controllers/TasksController.cs` — Lines 7–14 contain the existing task routes. Preserve operations and add OpenAPI metadata.
6. `src/TaskManager.Api/Data/TaskDbContext.cs` — Lines 3–10 define the EF Core model and `tasks` table mapping used for schema checks.
7. `src/TaskManager.Api/Migrations/202610130001_InitialCreate.cs` and `src/TaskManager.Api/Migrations/TaskDbContextModelSnapshot.cs` — Preserve and maintain the initial migration/snapshot.
8. `tests/TaskManager.Api.Tests/PostgresIntegrationTests.cs` — Lines 10–14 currently skip when the test connection is missing, then migrate and clear task rows; line 25 configures the test host. Replace silent skip with an actionable integration-test failure and guard the database target before migration or row deletion.
9. `tests/TaskManager.Api.Tests/ValidationTests.cs` — Lines 8–15 hold database-independent validation tests; keep these independently runnable.
10. `README.md` — Lines 5–22 give brief connection/migration/run notes; lines 37–45 document the test variable and current skip. Replace with the full Windows PowerShell workflow and Swagger instructions.
11. Repository root — `TaskManager.sln` is at the root. No appsettings or launch settings currently provide a database connection or local URL. Never add real credentials to tracked configuration.

---

## Product rules (current and new behaviour)

- Preserve current task routes and contract: create/list/get/update/delete/status change; statuses are `Todo`, `InProgress`, `Done`; invalid requests return 400 problem details and absent tasks return 404.
- Preserve PostgreSQL/EF Core and configuration lookup at `ConnectionStrings:TaskManager` with fallback `TASKMANAGER_CONNECTION_STRING`. Missing configuration must be reported clearly.
- New local setup explicitly checks PostgreSQL tools, service, and readiness; it creates `TaskManagerDb` only if absent and never drops or overwrites an existing database.
- PostgreSQL integration tests use only a separate local `TaskManagerTestDb` from `TASKMANAGER_TEST_CONNECTION_STRING`. Never point destructive test cleanup at development or production.
- Swagger UI/OpenAPI are enabled only in Development and document task routes, schemas, HTTP outcomes, and validation. Never expose connection strings or secrets in the document.

---

## Implementation tasks

### 0 — Agent-run PostgreSQL discovery and secure provisioning (must finish before migration application)

- This is an executable setup story for the coding agent, not a checklist of commands to hand to the developer. Keep the work non-destructive and idempotent. Do not ask the developer to create either database or run setup commands.
- Inspect the repository's existing configuration and local machine state before choosing a connection method: find the PostgreSQL 18 installation/service and actual server host/port, inspect available client tools without assuming `psql` or `pg_isready` is on `PATH`, and inspect `ConnectionStrings:TaskManager`, `TASKMANAGER_CONNECTION_STRING`, relevant test configuration, and API User Secrets without printing secret values. Record only whether a source is configured and safe, never its contents.
- Use pgAdmin only if a supported automation interface (such as a documented CLI/API usable in this environment) is present. Otherwise use a safe scripted PostgreSQL client/driver available from the installed PostgreSQL/.NET tooling; do not install or download tools without authorization. Report the discovered method and PostgreSQL version.
- Resolve the configured server and credentials from existing local configuration, environment variables, or .NET User Secrets. For database creation, the credential must authenticate to maintenance database `postgres` and have `CREATEDB`; for migrations, the app credential must have the needed schema privileges. Reuse a suitable existing local credential where possible. Never request a secret in chat, put it in tracked files, command-line arguments, logs, plans, or generated output. If no safe credential/privilege source exists, stop before any mutation and state the exact missing secure prerequisite (for example, a local admin credential with `CREATEDB` or migration-role schema privileges). Do not report success for incomplete steps.
- Using the verified server and privileged local connection, query the catalog for `TaskManagerDb`. If absent, create only that database. If present, leave it wholly intact. Never drop, overwrite, reset, or choose a fallback database. Verify the resulting database identity by reconnecting and querying its catalog.
- Configure `ConnectionStrings:TaskManager` in API .NET User Secrets using a secret-safe mechanism that does not expose credentials in command arguments/output. Preserve the existing `TASKMANAGER_CONNECTION_STRING` fallback and never put secrets in tracked appsettings or docs. Re-read configuration without printing it; verify the application connection reaches exactly the expected local `TaskManagerDb` (database/user/server identity).
- Do not proceed to migration application until the server, database name, and credential target have all been verified. On any mismatch or uncertainty, stop without applying migrations and report what was observed without secrets.

### 0a — Provision the isolated integration-test database

- Independently inspect and query for `TaskManagerTestDb` using the verified local server. Create it only if absent; preserve an existing database and its contents. Never reuse `TaskManagerDb`.
- Configure `TASKMANAGER_TEST_CONNECTION_STRING` through a process environment setting or test-project User Secrets, using only the dedicated test database. Do not store test credentials in tracked files or output.
- Before any test migration or cleanup, require database name `TaskManagerTestDb` and loopback host; reject malformed/remote/development targets without connecting or echoing the connection string. Cleanup may clear only test rows after this guard succeeds.

### 1 — Preserve the API and add Swagger/OpenAPI

- File: `src/TaskManager.Api/TaskManager.Api.csproj`
  - Add `Swashbuckle.AspNetCore` at a release compatible with .NET 9 for OpenAPI generation and Swagger UI; retain `net9.0`, EF Core, and Npgsql.
- File: `src/TaskManager.Api/Program.cs`
  - Preserve current connection lookup/error, Npgsql registration, controllers, and exception handling. Register API explorer and Swagger generation. Add Swagger JSON/UI middleware only when `app.Environment.IsDevelopment()` is true; use `/swagger` for the UI.
  - Do not configure database credentials, connection strings, or secret-bearing security schemes in OpenAPI.
- File: `src/TaskManager.Api/Controllers/TasksController.cs`
  - Preserve action behavior and describe DTO schemas plus response codes: POST 201/400; GET collection 200; GET by ID 200/404; PUT 200/400/404; PATCH status 200/400/404; DELETE 204/404. Include validation problem details and allowed status values.
- File: `tests/TaskManager.Api.Tests/PostgresIntegrationTests.cs`
  - Assert `/swagger/v1/swagger.json` loads and contains every task path, request/response schema, expected success/error codes, and validation model. Assert the document does not contain a sentinel secret value.

### 2 — Document agent-run PostgreSQL discovery and safe provisioning

- File: `README.md`
  - Document that the coding agent discovers the installed PostgreSQL 18 service/version and connection configuration, locates client tools from the installation/service path when they are not on `PATH`, and uses a supported pgAdmin automation interface only if one is actually available. Otherwise it uses an available safe PostgreSQL driver/client. Do not require the developer to run this preflight or setup commands. Include the manual diagnostic commands as optional troubleshooting only:

```powershell
$ErrorActionPreference = "Stop"
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "Install the .NET 9 SDK, then rerun setup." }
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) { throw "Install PostgreSQL client tools or add psql to PATH." }
if (-not (Get-Command pg_isready -ErrorAction SilentlyContinue)) { throw "Install PostgreSQL tools or add pg_isready to PATH." }
$pgServices = @(Get-Service | Where-Object { $_.Name -like "postgresql*" -or $_.DisplayName -like "*PostgreSQL*" })
if ($pgServices.Count -eq 0) { throw "No PostgreSQL Windows service found. Install PostgreSQL and rerun setup." }
if (-not ($pgServices | Where-Object Status -eq "Running")) { throw "PostgreSQL is installed but stopped. Start the discovered service and rerun setup." }
pg_isready -h localhost -p 5432
if ($LASTEXITCODE -ne 0) { throw "PostgreSQL is not accepting connections at localhost:5432; check service, host, and port." }
dotnet --list-sdks
```

  - Explain how the agent reports a missing/stopped service, unavailable client/driver, wrong PostgreSQL version, or unready server and stops before database mutation. Document that absent credentials/privileges are the secure prerequisite the developer must make available locally; never ask them to paste credentials into chat or tracked files.
- File: `README.md`
  - Describe the agent connecting to maintenance database `postgres` using a locally configured administrative role with `CREATEDB`. It queries `pg_database` first; if `TaskManagerDb` exists, leaves it unchanged; if absent, issues only `CREATE DATABASE "TaskManagerDb";`. Never drop, recreate, truncate, or overwrite the development DB.
  - If permission is denied, stop before database mutation and report that the local credential lacks `CREATEDB` (or identify the specific missing migration privilege). The implementation may document an optional DBA remedy, but must not imply that setup completed or require a manual command as the normal workflow. Do not select a fallback DB.
  - If including a diagnostic example, label it optional and never imply the developer must execute it:

```powershell
$PgAdminRole = "postgres"
$exists = psql -h localhost -p 5432 -U $PgAdminRole -d postgres -W -tAc "SELECT 1 FROM pg_database WHERE datname = 'TaskManagerDb';"
if ($LASTEXITCODE -ne 0) { throw "Could not check TaskManagerDb; verify server readiness and admin login." }
if (($exists | Out-String).Trim() -eq "1") {
    Write-Output "TaskManagerDb already exists; leaving it unchanged."
} else {
    psql -h localhost -p 5432 -U $PgAdminRole -d postgres -W -v ON_ERROR_STOP=1 -c 'CREATE DATABASE "TaskManagerDb";'
    if ($LASTEXITCODE -ne 0) { throw "Could not create TaskManagerDb. Ask a DBA to create it and grant migration privileges." }
}
```

### 3 — Configure the app safely for local development

- File: `src/TaskManager.Api/Program.cs`
  - Continue reading `ConnectionStrings:TaskManager` then `TASKMANAGER_CONNECTION_STRING`. Keep the actionable missing/blank-setting error; do not log the connection value.
- File: `src/TaskManager.Api/TaskManager.Api.csproj`
  - Initialize User Secrets with `dotnet user-secrets init --project src/TaskManager.Api/TaskManager.Api.csproj`, retaining only the generated `UserSecretsId` metadata in the project file. Secret values stay outside the repository.
- File: `README.md`
  - Document that the coding agent sets the secret after securely resolving a local credential; the developer does not need to run a setup command. The secret must not appear in agent output or tracked files. Do not show a real or copy-paste placeholder connection string containing a password:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet user-secrets init --project src/TaskManager.Api/TaskManager.Api.csproj
dotnet user-secrets set "ConnectionStrings:TaskManager" "<local-connection-string-from-secure-configuration>" --project src/TaskManager.Api/TaskManager.Api.csproj
```

The value above is an explanatory placeholder, not a literal command argument. Use the connection value already configured securely for the local PostgreSQL instance; never put its password in tracked documentation or command output.

  - Document `TASKMANAGER_CONNECTION_STRING` only as an existing supported fallback, not as a required manual setup step. Never put credentials in appsettings, launch settings, code, docs, plans, command arguments/output, logs, or test output.

### 4 — Apply and verify version-controlled migrations

- File: `src/TaskManager.Api/Migrations/202610130001_InitialCreate.cs` and `src/TaskManager.Api/Migrations/TaskDbContextModelSnapshot.cs`
  - Preserve the initial migration and snapshot. For model changes, add a new named migration and updated snapshot; do not rewrite an already-applied migration.
- File: `README.md`
  - Have the agent inspect available EF tooling and restore/build as needed. Apply the existing migrations only after verifying the configured target is exactly the local `TaskManagerDb` and the connection succeeds. The developer must not run setup commands:

```powershell
dotnet tool list --global
dotnet tool install --global dotnet-ef --version 9.0.4
dotnet restore TaskManager.sln
dotnet ef migrations list --project src/TaskManager.Api/TaskManager.Api.csproj
dotnet ef database update --project src/TaskManager.Api/TaskManager.Api.csproj
```

  - Verify the expected columns and applied migration using read-only PostgreSQL queries. Include these PowerShell commands in README and confirm the result lists all six columns plus the initial migration ID:

```powershell
psql -h localhost -p 5432 -U postgres -d TaskManagerDb -W -c "SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'tasks' AND column_name IN ('Id', 'Title', 'Description', 'Status', 'CreatedAt', 'DueDate') ORDER BY ordinal_position;"
if ($LASTEXITCODE -ne 0) { throw "Could not verify the TaskManagerDb task schema." }
psql -h localhost -p 5432 -U postgres -d TaskManagerDb -W -c 'SELECT "MigrationId" FROM public."__EFMigrationsHistory" ORDER BY "MigrationId";'
if ($LASTEXITCODE -ne 0) { throw "Could not verify EF migration history." }
```

  - If migration or schema verification fails, report the exact failure and fix setup/permissions; do not drop the database or claim success.

### 5 — Isolate and require PostgreSQL integration testing

- File: `tests/TaskManager.Api.Tests/PostgresIntegrationTests.cs`
  - Replace the current silent skip on missing `TASKMANAGER_TEST_CONNECTION_STRING` with a clear failure for the explicit integration run. Keep `ValidationTests` independently runnable without PostgreSQL.
  - Before migration or `ExecuteDeleteAsync`, parse the test string with `NpgsqlConnectionStringBuilder`. Require database `TaskManagerTestDb` and loopback host `localhost`, `127.0.0.1`, or `::1`; reject `TaskManagerDb`, remote hosts, malformed/missing values, and do not echo the string. The test host must use only the test variable.
  - Preserve migration-before-test behavior and clear task rows only after the safety guard accepts the dedicated test DB. Extend tests for Swagger JSON and task CRUD/status. Never use development or production data.
- File: `README.md`
  - Document that the agent creates/verifies `TaskManagerTestDb` separately with a catalog check and no destructive database operations, then configures the test connection safely. Warn that integration testing applies migrations and clears task rows only after the test-target guard accepts `TaskManagerTestDb`. If creation cannot proceed, state the exact missing local credential/privilege prerequisite and report tests as not run.

### 6 — Complete end-to-end verification and README workflow

- File: `README.md`
  - Document unit, integration, and full-suite commands; integration tests must fail clearly when setup is missing rather than silently skip.
  - Document starting in Development at a stable local URL:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5080"
dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj
```

  - Give Swagger URLs `http://localhost:5080/swagger` and `http://localhost:5080/swagger/v1/swagger.json`. Explain that the app connection must be configured and migrations applied before database-backed operations work.
  - Give a repeatable Swagger UI flow to create a uniquely named task, list, retrieve by ID, update, change status, and delete only that newly created task. State expected HTTP statuses and do not clear existing development data.
- Execute the ordered workflow: inspect and verify local PostgreSQL/configuration; create-or-preserve the two databases; set User Secrets; verify the application connection to `TaskManagerDb`; apply existing migrations; verify task schema and `__EFMigrationsHistory`; configure and guard the test-only connection; then verify Swagger, build, run database-independent tests, and run integration tests when prerequisites are available. Report build, database-independent test, and integration test outcomes separately, including every blocked/unrun check. Never claim a database was created, a connection verified, or a migration applied unless that action actually succeeded.

---

## Edge Cases & Failure Modes

- **PostgreSQL tool not on PATH:** discover the installation/service path and check for supported automation or an available driver; do not assume `psql`/`pg_isready` is installed. If no safe client method is available, stop and report it.
- **Service stopped or server unready:** report the discovered service and readiness failure; stop before database mutation and report the exact server/service/host/port prerequisite. Enforce in agent discovery and describe remediation in README troubleshooting.
- **Development DB already exists:** leave it intact. Never drop/recreate it or use a fallback DB. Enforce in the existence-check/create flow in README from Implementation task 2.
- **Credentials unavailable or admin lacks CREATEDB:** do not prompt for secrets in chat, attempt a weaker/fallback database, or imply setup succeeded. Stop before mutation and identify the secure local credential source or exact privilege missing. A DBA may provision/authorize credentials outside this automated run.
- **Missing/invalid connection value:** preserve the `Program.cs` check; stop setup without printing the value and identify which secure local configuration entry is missing.
- **Migration history/schema mismatch:** report applied IDs and actual columns; do not rewrite applied migration or fake verification. Enforce with the migration artifacts and read-only verification in `README.md` from Implementation task 4.
- **Test string absent/unsafe:** fail before DB access; reject development DB name and remote hosts before migration/row clearing. Do not skip explicit integration verification. Replace the skip and guard preceding migration/cleanup at `tests/TaskManager.Api.Tests/PostgresIntegrationTests.cs:10–14`.
- **Existing test DB:** keep it, migrate pending changes, and clear rows only after the local test DB guard succeeds.
- **Swagger outside Development:** keep docs disabled outside Development. Swagger loading alone is not evidence of a DB connection; verify database-backed operations separately.
- **Swagger document security:** expose task models/status/errors only; never include credentials, connection strings, or secret fields. Enforce through Swagger registration in `src/TaskManager.Api/Program.cs` and DTO/response metadata in `src/TaskManager.Api/Controllers/TasksController.cs:7–14`.
- **Database/API smoke failure:** report config, permission, migration, or connection failure rather than reporting setup complete.

---

## Test Plan

1. **Build and unit:** Build the solution and run database-independent validation tests; report if .NET SDK/tooling prevents execution.
2. **Integration:** Run `PostgresIntegrationTests` only against local `TaskManagerTestDb`; verify missing, development, malformed, and remote connection targets fail before migration or data deletion.
3. **Integration:** Apply version-controlled migrations to the test DB; verify migration history, expected `public.tasks` columns, and create/list/get/update/status/delete against PostgreSQL.
4. **OpenAPI:** Fetch `/swagger/v1/swagger.json`; verify all task paths/schemas, 201/200/204/400/404/500 outcomes, validation responses, and absence of a sentinel secret.
5. **Smoke:** Start Development at port 5080, load Swagger UI, and execute task CRUD/status operations against migrated `TaskManagerDb`. Delete only the uniquely created smoke task.

---

## Verification Steps

1. **Frontend runs:** No frontend changes are required; Swagger UI is the local API client.
2. **Backend builds:** From repository root, run `dotnet restore TaskManager.sln` and `dotnet build TaskManager.sln`.
3. **Regression:** Run `dotnet test TaskManager.sln --filter FullyQualifiedName~ValidationTests`, then with the test DB configured run `dotnet test TaskManager.sln --filter FullyQualifiedName~PostgresIntegrationTests` and finally `dotnet test TaskManager.sln`.
4. **Database:** Agent discovers PostgreSQL and local credentials, safely creates/reuses both databases, sets the app User Secret, verifies the exact development connection, applies existing migrations, and verifies `public.tasks` plus `__EFMigrationsHistory` with read-only queries. The test database is separately configured and guarded.
5. **Swagger/E2E:** Start in Development at `http://localhost:5080`; verify `/swagger` and `/swagger/v1/swagger.json`; perform task CRUD/status and confirm responses. Report every unmet prerequisite.

---

## Done Criteria

- [ ] Existing C#/.NET 9 API, endpoints, EF Core model, and initial migration remain intact; no TypeScript backend or unrelated frontend work is added.
- [ ] Agent inspects PostgreSQL 18 installation/service, available client automation without assuming PATH tools, and existing connection configuration before any mutation; unavailable prerequisites are reported.
- [ ] `TaskManagerDb` is created only if absent using an administrative connection; an existing DB remains untouched and permission denial gives an actionable DBA step.
- [ ] Agent obtains credentials only from existing local config, environment, or .NET User Secrets; initializes/stores the app connection in .NET User Secrets without exposing it; missing credentials/privileges stop setup with the exact secure prerequisite.
- [ ] Agent verifies the application connects to exactly `TaskManagerDb` before applying migrations.
- [ ] Version-controlled EF migrations apply to the configured development DB and read-only checks confirm the expected task table/columns and migration history.
- [ ] `TASKMANAGER_TEST_CONNECTION_STRING` points to local `TaskManagerTestDb`; tests reject dev/production/remote targets before migration/row deletion and do not silently skip required integration checks.
- [ ] Swagger UI at `http://localhost:5080/swagger` and its OpenAPI JSON load in Development, document every task operation/schema/status/validation response including generic 500 errors, contain no secrets, and let a developer exercise task operations interactively.
- [ ] README explains agent-driven setup, the non-destructive database/migration workflow, secure prerequisite/failure handling, migration/schema verification, isolated test DB, build/test/run, Swagger, and troubleshooting; no manual setup commands are required.
- [ ] Build and unit tests pass; PostgreSQL integration, migration/schema, and Swagger-driven CRUD/status checks pass against local PostgreSQL.
- [ ] Any unavailable prerequisite or unrun verification is reported; setup is not marked complete when a required check was skipped.
