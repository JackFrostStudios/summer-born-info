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

- Keep feature code vertical and self-contained unless a shared abstraction is clearly justified.
- Put business rules in `Domain`, application behaviour in `Features`, and external system access in `Infrastructure`.
- Add DI wiring in `API/SummerBornInfo.Web/Program.cs`.
- Mirror the existing `Schools` slice when adding a new feature slice.
- Reuse existing patterns before introducing new abstractions or helper frameworks.

## Testing Expectations

- Prefer integration tests for EF Core, HTTP, events, and storage behaviour.
- Prefer real database-backed tests over mocked repository tests.
- Use xUnit and the repository's `Given_When_Then` naming style.
- Assert on outputs and system state rather than internal method calls.
