# AI Project Guide

This document covers UI-internal structure and conventions. For top-level repository boundaries and API-vs-UI routing, start with [../AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md).

## Current UI Shape

The UI is currently a newly generated Angular application with server-side rendering support and a minimal app shell.

- `UI/src/main.ts` bootstraps the browser application.
- `UI/src/main.server.ts` bootstraps the server render entry point.
- `UI/src/server.ts` hosts the generated SSR server entry.
- `UI/src/app/app.ts` is the current root standalone component.
- `UI/src/app/app.config.ts` and `app.config.server.ts` hold browser and server configuration.
- `UI/src/app/app.routes.ts` and `app.routes.server.ts` hold route definitions.
- `UI/src/styles.scss` is the shared global stylesheet entry point.
- `UI/public/` holds static assets copied by the Angular build.

## Ownership And Placement Rules

- Put browser-facing UI work in `UI/src/app/` unless there is a clear reason to create a new top-level folder under `src/`.
- Keep feature code close together: route definition, component, template, styles, and tests should stay near each other.
- Add shared static assets to `UI/public/`.
- Keep repository-level guidance in `UI/README.md` and `UI/AGENTS.md`; do not duplicate that detail inside feature files.
- If the UI grows beyond the single generated shell, prefer adding feature-focused subfolders under `UI/src/app/` rather than expanding the root app component indefinitely.

## Conventions

### Application Structure

- Keep the root `App` component focused on shell concerns such as app-level layout and router outlet composition.
- Put feature-specific behaviour in feature components, routes, and services rather than in the root shell.
- Prefer standalone Angular APIs and keep module-era patterns out unless an existing dependency requires them.
- When adding a reusable abstraction, make sure at least two consumers justify it.

### Styling And Layout

- Keep global styling in `UI/src/styles.scss` minimal and intentional.
- Prefer component-scoped styles for feature-specific presentation.
- Build layouts with semantic HTML first, then layer styling on top.
- Treat responsive behaviour, keyboard flow, and focus visibility as part of the feature, not polish to defer.

### API Integration

- The repository does not yet define a canonical frontend API client or local proxy setup.
- If you introduce UI-to-API integration, place the first concrete pattern where future features can reuse it and document the convention in this file.
- Do not invent environment or endpoint conventions silently; document them in the same change that introduces them.

## Testing Expectations

- Run UI tests from the `UI` folder with `npm test`.
- Keep automated tests close to the component or behaviour they cover.
- For the current Angular app, prefer focused unit or shallow integration tests around rendered output, routing behaviour, and component interaction.
- Assert on visible behaviour and state, not internal implementation details.
- When a UI change affects accessibility or semantics, include assertions or manual validation notes that cover the changed experience.

## Review Checklist

- Does the change belong in `UI/` rather than `API/`, `Plans/`, or `Roadmap/`?
- Is new UI code placed near the route, component, or feature it belongs to?
- Are accessibility, responsive layout, and keyboard/focus behaviour considered for user-facing changes?
- Are tests or validation notes appropriate for the level of UI change?
- If a new structural convention was introduced, was this file updated to capture it?
