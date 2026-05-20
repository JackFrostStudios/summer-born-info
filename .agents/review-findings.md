# Review Findings

## Scope

This review covered:

- Production readiness, including any tight coupling from `SummerBornInfo.Web` to Aspire.
- Maintainability and simplicity.
- AI-assisted development guidance and context hygiene.
- Security and operations readiness.
- Testing, configuration, and dependency readiness.

## Highest Priority Improvements

### 1. Decouple the deployable web app from Aspire-only concerns

- `SummerBornInfo.Web` directly references `SummerBornInfo.AppHost.ServiceDefaults` and calls `builder.AddServiceDefaults()` in [API/SummerBornInfo.Web/Program.cs](../API/SummerBornInfo.Web/Program.cs), even though Aspire is intended only for local development and the project guide places Aspire composition in `AppHost`.
- `SummerBornInfo.Web` also depends on development-time bootstrap inside `Program.cs` for schema creation, `pgmq` initialization, and queue creation.

Suggestions:

- Remove the direct `SummerBornInfo.Web -> SummerBornInfo.AppHost.ServiceDefaults` dependency.
- Move shared production-safe hosting concerns into generic ASP.NET Core extensions that work with or without Aspire.
- Define a non-Aspire deployment contract for database migrations, `pgmq` extension availability, queue creation, and startup validation.

### 2. Fix import-worker failure handling and idempotency

- Failed queue messages are not acknowledged and can be retried indefinitely in [API/SummerBornInfo.Web/BackgroundServices/ProcessSchoolBulkImportBackgroundService.cs](../API/SummerBornInfo.Web/BackgroundServices/ProcessSchoolBulkImportBackgroundService.cs).
- `SchoolBulkImportRequest.ProcessingStarted()` allows a `Failed` request to move back to `Processing` in [API/SummerBornInfo.Domain/Entities/SchoolBulkImportRequest.cs](../API/SummerBornInfo.Domain/Entities/SchoolBulkImportRequest.cs), which makes poison-message loops and partial replays possible.
- Some failure paths in [API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/ProcessImportFileCommandHandler.cs](../API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/ProcessImportFileCommandHandler.cs) throw before the request is marked failed.

Suggestions:

- Add bounded retry behavior, retry tracking, and a dead-letter or terminal-failure path.
- Prevent automatic `Failed -> Processing` re-entry unless a reset is explicit.
- Ensure the full processing path records failure state consistently before surfacing exceptions.

### 3. Replace development-only database bootstrap with a production-safe setup path

- Outside `Development`, the app has no checked-in path for schema evolution or queue/bootstrap prerequisites.
- There are no EF migration files in the repo, and the current setup relies on `EnsureCreatedAsync`.
- Hosting is tightly coupled to the custom `ghcr.io/pgmq/pg18-pgmq:v1.10.0` image in [API/SummerBornInfo.AppHost/AppHost.cs](../API/SummerBornInfo.AppHost/AppHost.cs), which narrows future hosting options.

Suggestions:

- Introduce EF migrations instead of relying on `EnsureCreatedAsync`.
- Add startup validation for required database prerequisites, especially `pgmq`.
- Document supported production hosting assumptions clearly before infrastructure decisions solidify.

### 4. Tighten security and operational guardrails around the import API

- The import endpoint is currently unauthenticated, unthrottled, and accepts large uploads in [API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs](../API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs).
- Import status endpoints also appear unauthenticated.
- Raw exception messages are persisted and returned to clients through the status API.

Suggestions:

- Add authentication and authorization to import/status endpoints.
- Add rate limiting, tighter body-size controls, and content-type validation.
- Replace raw error exposure with sanitized user-facing messages while keeping detailed diagnostics in logs and traces.

### 5. Put tests into CI and close the highest-value behavior gaps

- `.github/workflows/ci.yml` currently restores and builds, but does not run the test suite.
- Existing tests are strong on happy-path import behavior, but there are still gaps around oversized uploads, empty uploads, worker retry/ack behavior, and startup/config failure modes.

Suggestions:

- Add a `dotnet test` step from the `API` directory to CI.
- Add small, high-value tests for:
  - empty upload rejection,
  - oversized upload rejection,
  - failed worker runs not acknowledging queue messages,
  - startup failure when required configuration or queue prerequisites are missing.

## Production Readiness

### High

- `SummerBornInfo.Web` is tightly coupled to Aspire service defaults through the project reference and startup wiring in [API/SummerBornInfo.Web/SummerBornInfo.Web.csproj](../API/SummerBornInfo.Web/SummerBornInfo.Web.csproj) and [API/SummerBornInfo.Web/Program.cs](../API/SummerBornInfo.Web/Program.cs).
- Non-Aspire deployment is underdefined because database schema creation, `pgmq` initialization, and queue creation only happen in the development block in [API/SummerBornInfo.Web/Program.cs](../API/SummerBornInfo.Web/Program.cs).
- The queue worker can retry failed messages forever and replay partially processed work.

### Medium

- The import path is very database-chatty: the importer saves each row, and the handler saves progress each row as well.
- `School.URN` does not appear to have a uniqueness constraint in [API/SummerBornInfo.Infrastructure/Persistence/Configuration/SchoolConfiguration.cs](../API/SummerBornInfo.Infrastructure/Persistence/Configuration/SchoolConfiguration.cs), so retries or parallel workers could create duplicates.
- `pageSize` is accepted without an upper bound and flows directly into `Take(...)`.
- Health checks are registered in service defaults, but `Program.cs` never maps them.

## Maintainability

### High

- The import implementation pushes EF Core persistence, tracking cleanup, and infrastructure mechanics into `Features`, especially in [API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/SchoolsImporter.cs](../API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/SchoolsImporter.cs).
- `SummerBornInfo.Features` directly depends on `Infrastructure`, which weakens the intended layer boundaries in `AI_PROJECT_GUIDE.md`.

### Medium

- `SchoolsImporter<TContext>` is generic even though only `ApplicationDbContext` is used.
- `LookupImporterBase` and its subclasses add abstraction overhead without much behavioral payoff for the current scope.
- Queue and large-object access are split into several thin one-method interfaces, adding indirection without much apparent substitution value.

### Low

- The import upload size limit is duplicated in the endpoint implementation.
- The global-using convention in `AI_PROJECT_GUIDE.md` does not match the AppHost projects, which increases churn risk.

## AI-Assisted Development Workflow

### High

- `README.md` contains a broken AppHost CLI path and a confusing testing section, so future AI contributors following it literally will be misled.
- `AI_PROJECT_GUIDE.md` does not fully describe supporting projects like `SummerBornInfo.AppHost.ServiceDefaults` and `SummerBornInfo.IntegrationTests`.
- `AGENTS.md` applies plan-file and per-step commit rules too broadly; those rules fit implementation work better than reviews or no-edit investigations.

### Medium

- Testing expectations are described, but not enforced clearly enough to tell an AI what must be run before handoff.
- The global-using rule is stricter than the actual repo usage.
- There is no explicit instruction to ignore generated output such as `bin/`, `obj/`, and `TestResults/` during repo search and review.

## Security And Operations

### High

- Import/status endpoints need authentication, authorization, and abuse protection before production use.
- Queue failure handling currently lacks dead-lettering or bounded retry behavior.

### Medium

- Health/readiness endpoints are not currently exposed.
- Connection string and worker option validation are weak; invalid production config is likely to fail late.
- Raw exception text is returned to clients through the import status API.

### Notes

- No obvious checked-in secrets were found.
- Swagger/OpenAPI is currently restricted to development, which is appropriate.
- `AppHost` enabling PgAdmin and a root-running local Postgres container appears acceptable as local-only tooling, but it should remain clearly separate from production hosting guidance.

## Testing, Configuration, And Dependency Readiness

### High

- CI does not run tests.
- There is no production-ready, checked-in contract for schema evolution and queue prerequisites.
- The `pgmq` dependency narrows hosting options and should be treated as an explicit platform decision.

### Medium

- Health checks are registered but not mapped.
- Worker options are bound but not validated on startup.
- The highest-value operational failure cases still need targeted tests.

## Suggested Follow-Up Order

1. Decouple `SummerBornInfo.Web` from Aspire-only service defaults and define a non-Aspire deployment path.
2. Fix queue retry/idempotency behavior and protect against poison-message loops.
3. Introduce migrations and startup validation for database and queue prerequisites.
4. Add auth, rate limiting, and safer error handling to import/status endpoints.
5. Add `dotnet test` to CI and fill the focused failure-mode test gaps.
6. Simplify the import architecture by moving EF-heavy mechanics out of `Features`.
7. Update `README.md`, `AI_PROJECT_GUIDE.md`, and `AGENTS.md` so future AI contributors get accurate, lower-noise guidance.
