---
name: architect
description: Decide where code belongs in this project and how to fit it into the repository's API, UI, and planning structure.
---

# Architect

Use this skill for code layout, feature placement, and project pattern decisions.

## Rules

- Use [AI_PROJECT_GUIDE.md](../../../AI_PROJECT_GUIDE.md) as the canonical map for project boundaries.
- Route the task to the correct surface before making placement advice:
  - `API/` for backend HTTP, domain, persistence, queues, storage, and local infrastructure work.
  - `UI/` for Angular routes, components, styling, browser behaviour, and client-side state.
  - `Plans/` and `Roadmap/` for planning documents rather than product code.
- Keep the API vertical-slice structure intact for API feature work.
- Put code in the smallest layer that owns the behaviour.
- Prefer existing patterns over new abstractions.

## When unsure

- Check project references before placing code.
- Read the child project guide for the surface you are changing before recommending a new structure:
  - `API/AI_PROJECT_GUIDE.md` for API internals and slice placement.
  - `UI/AI_PROJECT_GUIDE.md` for UI structure, feature placement, and layout/testing expectations.
- If API code needs DI, add it in `API/SummerBornInfo.Web/Program.cs`.
- If the API feature is a new slice, mirror the existing `Schools` structure first.
- If UI work introduces a new feature area, keep the route, component, template, styles, and tests close together under `UI/src/app/` unless the UI guide establishes a better pattern.
- Return the recommended target files, the reasoning, and any DI or coupling concerns.


## Domain Logic
- For API domain work, ensure complex state is managed in Domain entities; simple value updates do not require domain functions, but multi-value updates or domain actions should be tied to functions on Domain entities.
