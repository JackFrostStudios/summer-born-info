---
name: ui-architect
description: Decide where Angular UI code belongs in this project and how to fit it into the existing frontend structure. Use when work belongs in `UI/` and you need guidance on route, component, service, styling, or UI feature placement.
---

# UI Architect

Use this skill for UI code layout, feature placement, and project pattern decisions inside `UI/`.

## Rules

- Use [AI_PROJECT_GUIDE.md](../../../UI/AI_PROJECT_GUIDE.md) as the canonical map for UI boundaries.
- Keep feature code close together.
- Put code in the smallest UI unit that owns the behaviour.
- Prefer existing patterns over new abstractions.

## When unsure

- Read the UI project guide before recommending a new structure.
- If UI work introduces a new feature area, keep the route, component, template, styles, and tests close together under `UI/src/app/` unless the UI guide establishes a better pattern.
- Keep the root app shell focused on shell-level concerns rather than feature-specific logic.
- Return the recommended target files, the reasoning, and any routing, state, or coupling concerns.
