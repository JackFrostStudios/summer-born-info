# AI Project Guide

This document is the compact source of truth for the repository layout and the conventions that AI helpers should assume.

## Solution Layout

- `API/SummerBornInfo.Domain`: domain entities, value objects, and rules.
- `API/SummerBornInfo.Features`: commands, queries, handlers, and feature DTOs.
- `API/SummerBornInfo.Infrastructure`: EF Core, event queue, and storage implementations.
- `API/SummerBornInfo.Web`: HTTP wiring, route registration, and startup configuration.
- `API/SummerBornInfo.AppHost`: Aspire composition and local dependency startup.
- `API/SummerBornInfo.*.Tests`: the matching test projects for each layer.

## Conventions

### General Guidance

- Keep feature code vertical and self-contained unless a shared abstraction is clearly justified.
- Put business rules in `Domain`, application behaviour in `Features`, and external system access in `Infrastructure`.
- Features orchestrate use-cases and persistence; domain entities enforce invariants and own state mutation.
- Add DI wiring in `API/SummerBornInfo.Web/Program.cs`.
- Mirror the existing `Schools` slice when adding a new feature slice.
- Reuse existing patterns before introducing new abstractions or helper frameworks.

### Domain State Management

- Encapsulate complex entity state transitions inside domain entities. Feature handlers must call domain methods rather than setting entity state directly when there is a domain action linked to the change.
- When a state change is for a specific  domain action or has invariants, model it as a domain method and keep entity setters private.
- Example pattern: for school import processing, use `SchoolBulkImportRequest.ProcessingStarted()`, `UpdateProgress(...)`, and `ProcessingComplete()` instead of direct status mutation in feature handlers.
- Anti-pattern: directly assigning entity status (for example `request.Status = ...`) in feature code when a domain method exists for that transition.

### Using Style

- Imports should be managed via "global using" in a single GlobalUsings.cs file per project to reduce noise in classes. The imports in these files should be sorted to reduce merge conflicts.

## Review Checklist

- If there is a complex entity state change, does the change happen through a domain method that encapsulates the complex rules?
- If there is a entity state change related to a specific domain action, does the change happen through a domain method with a name reflecting the action?
- Are there any imports in individual files? If they are not overloading the global using with a named import then refactor the using to "GlobalUsings.cs" in that project.

## Testing Expectations

- Prefer integration tests for EF Core, HTTP, events, and storage behaviour.
- Prefer real database-backed tests over mocked repository tests.
- Use xUnit and the repository's `Given_When_Then` naming style.
- Assert on outputs and system state rather than internal method calls.

## Running tests

- `dotnet test` commands must be run from the `API` folder as the working directory, otherwise `global.json` configuration is not loaded and tests aren't identified.
