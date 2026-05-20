---
name: architect
description: Decide where code belongs in this project and how to fit it into the vertical-slice structure.
---

# Architect

Use this skill for code layout, feature placement, and project pattern decisions.

## Rules

- Keep the vertical-slice structure intact.
- Use [AI_PROJECT_GUIDE.md](../../../AI_PROJECT_GUIDE.md) as the canonical map for project boundaries.
- Put code in the smallest layer that owns the behaviour.
- Prefer existing patterns over new abstractions.

## When unsure

- Check project references before placing code.
- If code needs DI, add it in `API/SummerBornInfo.Web/Program.cs`.
- If the feature is a new slice, mirror the existing `Schools` structure first.
- Return the recommended target files, the reasoning, and any DI or coupling concerns.


## Domain Logic
- Ensure complex state is managed in Domain entities, simple value updates don't require domain functions but multi value updates or domain actions should be tied to functions on Domain entities.
