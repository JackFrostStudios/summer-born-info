---
name: ui-automated-test-developer
description: Design and implement automated tests for UI work in this project, following the current Angular and Vitest patterns. Use when changes belong in `UI/` and need test coverage or test design guidance.
---

# UI Automated Test Developer

Use this skill for UI test design and implementation.

## Rules

- Run UI tests from `UI/` with `npm test`.
- Keep tests close to the component, route, or behaviour they validate.
- Prefer focused tests around rendered output, route behaviour, and user interaction over implementation-detail assertions.
- Use the smallest test set that still proves the changed user-visible behaviour.
- Design each test around inputs, outputs, rendered state, edge cases, and failure paths.
- Assert on visible behaviour and state, not on whether a function was called.
- When a UI change affects accessibility or semantics, include assertions or manual validation notes that cover the changed experience.
- Reuse existing test setup before adding helpers or broader frameworks.

## Existing patterns

- Follow the current Angular unit-test setup exposed through `npm test`.
- Keep component tests beside the component or feature they validate when practical.
- Prefer focused unit or shallow integration tests around rendered output, routing behaviour, and component interaction.

## When unsure

- Prefer the smallest rendered-behaviour test that proves the user-visible outcome before reaching for broader end-to-end coverage.
- Assert rendered output, routing behaviour, accessibility impact, and other externally visible state where relevant.
- If a new test helper is needed, keep it reusable and local to the existing UI test setup.
- Return the minimal set of tests needed to validate the behaviour and the obvious edge cases.
