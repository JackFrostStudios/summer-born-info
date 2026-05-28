# API

## Table of Contents

- [Introduction](#introduction)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Public School Discovery](#public-school-discovery)
- [Admin Authentication and Bootstrap](#admin-authentication-and-bootstrap)
- [Testing](#testing)
- [Development](#development)

## Introduction

This folder contains the Summer Born Information API solution and related projects.

## Architecture

This project follows a vertical slice architecture. The compact AI-friendly summary of the solution layout and common conventions lives in [AI_PROJECT_GUIDE.md](./AI_PROJECT_GUIDE.md).

In short:

- **Domain**: domain entities, value objects, and rules.
- **Features**: commands, queries, handlers, and feature DTOs.
- **Infrastructure**: database access, queue integration, large object or file storage, and other external integrations.
- **Web**: ASP.NET Core HTTP wiring and endpoint registration.
- **AppHost**: Aspire composition and local dependency startup.

## Getting Started

### Prerequisites

- .NET 10.0
- Visual Studio with Aspire support
- Docker Desktop or another compatible container runtime

### Running the Application

1. Open `SummerBornInfo.sln` in Visual Studio.
2. Set `SummerBornInfo.AppHost` as the startup project.
3. Start debugging or run the solution.

Visual Studio will start the Aspire app host, which in turn launches the PostgreSQL environment and the `SummerBornInfo.Web` API. The API also performs its development startup work in `Web/Program.cs`, including database creation and queue initialisation when running in Development.

If you prefer the command line, you can run the app host from the `API` folder with:

```bash
dotnet run --project SummerBornInfo.AppHost/SummerBornInfo.AppHost.AppHost/SummerBornInfo.AppHost.csproj
```

## Public School Discovery

Milestone 3 keeps the public school surface grouped under `/api/schools`, but it does not overload the collection route with search modes.

### `GET /api/schools`

Use `GET /api/schools` for plain collection traversal only. It returns the shared collection wrapper:

```json
{
  "schools": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "urn": 100001,
      "name": "Northbridge Primary"
    }
  ],
  "nextCursor": "00000000-0000-0000-0000-000000000001"
}
```

- `pageSize` is optional and defaults to `100`.
- Large `pageSize` values are capped to `200` on this collection route.
- `cursor` continues traversal by school `id`, so the collection cursor is not opaque.

### `GET /api/schools/search?q=...`

Use `GET /api/schools/search` with `q` for free-text school discovery across school name plus address and postcode fields. Successful responses use the same `{ schools, nextCursor }` wrapper as the collection route.

- `q` is required for free-text search, trimmed server-side, and must contain at least 4 non-whitespace characters.
- `pageSize` is optional and defaults to `100`; values outside `1` to `200` return `400 Bad Request`.
- `cursor`, when supplied, must be the opaque `nextCursor` returned by an earlier search for the same normalized query text.
- Search cursors are versioned, query-bound continuation tokens. Callers should treat them as opaque values and not infer ordering from their contents.
- A valid search with no matches returns `200 OK` with `"schools": []` and `"nextCursor": null`.
- Supplying blank `q`, a short query, an invalid cursor, or both `q` and `urn` returns `400 Bad Request`.

### `GET /api/schools/search?urn=...`

Use `GET /api/schools/search?urn=...` for exact URN lookup. This is a distinct query mode on the discovery route and returns a single full `SchoolResponse` object rather than the collection wrapper.

- `urn` must be a positive integer or the API returns `400 Bad Request`.
- Unknown URNs return `404 Not Found`.
- Do not combine `urn` with `q`; the route requires exactly one discovery mode.

### Search implementation notes

The current search implementation uses PostgreSQL full-text search together with `pg_trgm`:

- full-text ranking uses PostgreSQL's `simple` configuration with `plainto_tsquery`, which keeps school and place-name tokens closer to their source values than stemmed language dictionaries would;
- trigram matching uses `word_similarity` to support partial-name, address-fragment, postcode-fragment, and mild typo-tolerant discovery that plain full-text search would miss.

This hybrid approach was chosen over plain SQL `LIKE` matching because it gives materially better ranking and fragment matching, and over a separate search service because Milestone 3 can meet its discovery needs within the existing PostgreSQL stack.

## Admin Authentication and Bootstrap

Milestone 2 introduces cookie-based admin authentication for protected API operations. The supported admin auth contract is intentionally limited to project-specific endpoints under `/api/admin/auth/*`:

- `POST /api/admin/auth/sign-in`
- `POST /api/admin/auth/sign-out`

The sign-in endpoint accepts an email/password payload, validates that the user is in the `Admin` role, and issues the ASP.NET Core Identity application cookie on success. Invalid credentials return `401 Unauthorized`. Valid credentials for a non-admin user return `403 Forbidden`. Sign-out clears the same cookie and returns `204 No Content`.

Protected Milestone 2 admin operations remain under `/api/admin`, including:

- `POST /api/admin/school-imports`
- `POST /api/admin/csa-application-reviews/{reviewId}/moderation`

The moderation route is intentionally only a protected contract shell at this stage. The deeper moderation business workflow remains deferred to Milestone 5.

### Local development bootstrap

Local development uses startup-time admin upsert in Development. Configure the initial admin account with `dotnet user-secrets` against the web project:

```bash
dotnet user-secrets --project SummerBornInfo.Web/SummerBornInfo.Web.csproj set AdminUserEmail admin@example.com
dotnet user-secrets --project SummerBornInfo.Web/SummerBornInfo.Web.csproj set AdminUserPassword "Choose-a-strong-dev-password"
```

When the app starts in Development, it upserts that user, ensures the password matches the configured value, and assigns the `Admin` role. Both `AdminUserEmail` and `AdminUserPassword` must be supplied together when either is configured.

After the app is running, sign in by posting JSON to `/api/admin/auth/sign-in`:

```json
{
  "email": "admin@example.com",
  "password": "Choose-a-strong-dev-password"
}
```

Use a client that preserves cookies if you want to call protected admin endpoints in the same session.

### Production bootstrap

For non-development environments, the checked-in bootstrap artifact is [ProductionScripts/bootstrap-initial-admin.sql](../ProductionScripts/bootstrap-initial-admin.sql). Operators should run it with `psql` after the Milestone 2 identity schema exists.

The script requires:

- `admin_email`
- `admin_password_hash`

Optional:

- `admin_role_name` which defaults to `Admin`

Example:

```bash
psql "$DATABASE_URL" -v admin_email='admin@example.com' -v admin_password_hash='AQAAAAIAAYagAAAA...' -f ProductionScripts/bootstrap-initial-admin.sql
```

Important operational note: the script expects a precomputed ASP.NET Core Identity password hash. Operators should generate that hash from trusted application code or another trusted .NET environment and supply the resulting hash value to the script. The SQL script does not generate passwords or hashes itself.

The script is designed to be safely re-run. It creates missing role, user, and membership rows, normalizes the relevant key fields, and does not overwrite an existing non-null password hash for a matched user.

## Testing

The solution includes domain, infrastructure, feature, and web integration test projects. Tests are written with xUnit and are intended to be run through Visual Studio's Test Explorer / test runner.

Run all tests from Visual Studio, or from the `API` folder on the command line with:

```bash
dotnet test
```

For a coverage run that excludes third-party assemblies and keeps the report focused on repository code, use:

```bash
dotnet test --report-xunit-trx --coverlet --coverlet-exclude-assemblies-without-sources MissingAll --coverlet-output-format cobertura
```

The integration test projects use Testcontainers to provision PostgreSQL when required, so no separate manual database setup should be necessary for test runs.

## Development

Development is centered around Visual Studio and Aspire:

- Open the solution in Visual Studio and run `SummerBornInfo.AppHost` to start the local environment.
- Use the Solution Explorer and project references to work feature by feature.
- Keep changes vertical: feature code should stay in `Features` unless it truly belongs in `Domain` or `Infrastructure`.
- Use the Visual Studio test runner to execute and debug tests while iterating.

The app host brings up PostgreSQL and the web app together, which is the fastest way to get a consistent local stack running.
