> **Source:** manual entry (tracker skipped via `--no-tracker`).
> Active tracker for this workspace: `github` — this story is not linked. 
> Run `squad tracker link <story-path> <tracker-id>` later if you want to attach one.

# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/taskmanager/create-task-management-api/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):Task Management API**
- **Feature slug (folder under `plans/`):** `taskmanager`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** None
- **Work item type:** None
- **Status:** None
- **Assignee:** None
- **Labels:** None

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

Create Task Management API


---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*


- Build a RESTful Task Management API using .NET 9 Web API, Entity Framework Core, and PostgreSQL.

- The API will allow users to create, retrieve, update, and delete tasks. Each task will have a title, description, status, creation date, and optional due date.

- The solution should follow a clean, maintainable architecture and use appropriate validation, error handling, and asynchronous database operations.

- This is a learning project for practicing Spec-Driven Development using Squad-Kit and Codex CLI.


---

## Acceptance criteria

*(Checklist, bullets, Gherkin, etc. Prefilled for Azure DevOps when the work item has acceptance criteria.)*

- Create a task with a required title, optional description, and optional due date.

- Retrieve all tasks.

- Retrieve a single task by its ID.

- Update an existing task.

- Delete an existing task.

- Change a task's status to Todo, InProgress, or Done.

- Reject invalid input with appropriate HTTP status codes and meaningful error messages.

- Return 404 Not Found when a requested task does not exist.

- Persist task data in PostgreSQL using Entity Framework Core.

- Use asynchronous operations for database access.

- Add database migrations and document how to apply them.

- Add automated tests for the main API scenarios.

- Provide instructions to configure the database and run the API locally.
- Make a clean Windows local setup reproducible: check the .NET 9 SDK and PostgreSQL tools/service/readiness, and explicitly report missing prerequisites.
- Create the TaskManagerDb development database only if absent using an administrative connection; preserve any existing database and document the DBA step when permissions are insufficient.
- Make local PostgreSQL provisioning and migration setup executable by the coding agent; the developer must not have to manually create a database or run setup commands. Before mutation, inspect the installed PostgreSQL service/version, available client/driver tooling (do not assume `psql` is on PATH), and effective connection configuration. Use pgAdmin only when a supported automation interface is available; otherwise use a safe scripted PostgreSQL client/driver.
- Obtain admin and migration credentials only from existing local configuration, process/user environment, or .NET User Secrets. Never ask for committed credentials or write secrets into tracked files, plans, logs, command output, or generated artifacts. If an authorized credential or required privilege is missing, stop and identify exactly what secure local prerequisite is needed; do not claim setup succeeded.
- Store `ConnectionStrings:TaskManager` in the API project's .NET User Secrets, verify a live connection to `TaskManagerDb`, apply the existing EF Core migrations only after validating the target, then verify the expected schema and migration history. Preserve all existing databases and migration files.
- Create or verify `TaskManagerTestDb` separately and configure integration tests to use only it. Guard against destructive cleanup ever targeting the development database.
- Configure ConnectionStrings:TaskManager or TASKMANAGER_CONNECTION_STRING through .NET User Secrets or a process environment variable; never commit credentials and return a clear error when configuration is missing.
- Keep EF Core migrations version-controlled, apply pending migrations, and verify the task schema and migration history.
- Use a distinct local TaskManagerTestDb via TASKMANAGER_TEST_CONNECTION_STRING; reject the development/production or remote target before tests migrate or clear data. Run PostgreSQL integration tests without silently skipping missing setup.
- Add .NET 9-compatible Swashbuckle OpenAPI and Swagger UI support for every task endpoint, including request/response schemas, success/error codes, and validation responses. Enable the UI in Development at `http://localhost:5080/swagger`; verify it loads and task operations can be exercised interactively; document generic server errors and validation details; do not expose secrets in the OpenAPI document.
- Document exact Windows PowerShell prerequisites, safe database creation, configuration, migrations, unit/integration tests, run commands, and Swagger UI URL.
- Report build, database-independent test, and integration-test outcomes separately; run each whose prerequisites are available and clearly identify any that could not run. Update README.md with the agent-driven setup flow and troubleshooting guidance.
- Build the solution, run unit and PostgreSQL integration tests, start the API, and verify task operations through Swagger UI against the configured development database.

- Keep the implementation consistent with the agreed project architecture.


---

## Attachments

None

---

## Dependencies

- **Blocked by / related ids:** None
- **Depends on code areas or other stories:** None. This is the initial feature.

## Extra notes (optional)

- Build the backend first. Angular frontend development is out of scope for this story.

- Authentication, authorization, user accounts, notifications, and deployment are out of scope.

- Do not introduce microservices or unnecessary infrastructure.

- Ask for clarification if a significant requirement is ambiguous instead of silently inventing a requirement.
- Preserve the already implemented C#/.NET 9 API, task endpoints, EF Core model, and initial migration while completing the local setup and Swagger work.

## Technical hints (optional)

- Backend: .NET 9 Web API.

- Database: PostgreSQL.

- ORM: Entity Framework Core.

- API style: REST.

- Use a clear, maintainable architecture with separation of concerns.

- Use the repository root: ..

- Primary language: C#.

## Out of scope

- Angular frontend.

- Authentication and authorization.

- Multi-user ownership and permissions.

- Email or push notifications.

- Cloud deployment and CI/CD.

- Microservices and message brokers.
