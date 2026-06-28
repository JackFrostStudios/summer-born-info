---
name: api-architect
description: Decide where API code belongs in this project and how to fit it into the existing vertical-slice .NET structure. Use when work belongs in `API/` and you need guidance on feature placement, layering, DI wiring, or API project boundaries.
---

# API Architect

Use this skill for API code layout, feature placement, and project pattern decisions inside `API/`.

## Rules

- Use [AI_PROJECT_GUIDE.md](../../../API/AI_PROJECT_GUIDE.md) as the canonical map for API boundaries.
- Keep the vertical-slice structure intact.
- Put code in the smallest API layer that owns the behaviour.
- Prefer existing patterns over new abstractions.

## When unsure

- Check project references before placing code.
- If API code needs DI, add it in `API/SummerBornInfo.Web/Program.cs`.
- If the feature is a new slice, mirror the existing `Schools` structure first.
- Return the recommended target files, the reasoning, and any DI or coupling concerns.

## Domain Logic

- Ensure complex state is managed in Domain entities; simple value updates do not require domain functions, but multi-value updates or domain actions should be tied to functions on Domain entities.
