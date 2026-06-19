# API

This folder contains the Summer Born Information API solution and supporting projects.

## Project Aims

The API exists to support the initial Summer Born Information platform for UK parents and guardians of summer born children.

- Provide a stable backend for school discovery and delayed-start information experiences.
- Support the first user-facing journeys around finding schools and understanding summer born admissions choices.
- Provide secure admin capabilities for privileged operations such as school data import and moderation workflows.
- Give the follow-on UI project a dependable API contract, local development environment, and testable architecture.

## Developer Guidance

### Prerequisites

- .NET 10.0
- Visual Studio with Aspire support
- Docker Desktop, or another compatible container runtime

### Getting Started

1. Open `SummerBornInfo.sln` in Visual Studio.
2. Set `SummerBornInfo.AppHost` as the startup project.
3. Start debugging or run the solution.

The Aspire app host starts the local PostgreSQL environment and the `SummerBornInfo.Web` API together. In Development, the web app also performs startup tasks such as database creation and queue initialization from `SummerBornInfo.Web/Program.cs`.

If you prefer the command line, run the app host from the `API` folder:

```bash
dotnet run --project SummerBornInfo.AppHost/SummerBornInfo.AppHost.AppHost/SummerBornInfo.AppHost.csproj
```

### Local Configuration

Local admin bootstrap uses `dotnet user-secrets` against the web project:

```bash
dotnet user-secrets --project SummerBornInfo.Web/SummerBornInfo.Web.csproj set AdminUserEmail admin@example.com
dotnet user-secrets --project SummerBornInfo.Web/SummerBornInfo.Web.csproj set AdminUserPassword "Choose-a-strong-dev-password"
```

Both settings must be supplied together when either is configured.

### Architecture And Conventions

The solution uses a vertical slice architecture. Use [AI_PROJECT_GUIDE.md](./AI_PROJECT_GUIDE.md) as the compact source of truth for solution layout, code placement, and common .NET conventions.

In short:

- `SummerBornInfo.Domain` holds domain entities, value objects, and rules.
- `SummerBornInfo.Features` holds commands, queries, handlers, and feature DTOs.
- `SummerBornInfo.Infrastructure` holds EF Core, queue, storage, and external integrations.
- `SummerBornInfo.Web` holds HTTP wiring, endpoint registration, and startup configuration.
- `SummerBornInfo.AppHost` holds Aspire composition and local dependency startup.

### Testing

Run tests from the `API` folder so the local `global.json` is applied:

```bash
dotnet test
```

For a coverage run focused on repository code:

```bash
dotnet test --report-xunit-trx --coverlet --coverlet-exclude-assemblies-without-sources MissingAll --coverlet-output-format cobertura
```

The integration tests use Testcontainers to provision PostgreSQL when needed, so separate manual database setup should not be required for normal test runs.

### Additional Reference

Use these files when you need more detail than this onboarding README is intended to hold:

- [API_REFERENCE.md](./API_REFERENCE.md) for current API behavior, admin auth/bootstrap notes, and GDAL runtime notes.
- [API.http](./API.http) for simple local request scratch space.
