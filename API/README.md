# API

## Table of Contents

- [Introduction](#introduction)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
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
