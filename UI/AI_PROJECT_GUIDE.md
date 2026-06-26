# AI Project Guide

This document covers UI-internal structure and conventions. For top-level repository boundaries and API-vs-UI routing, start with [../AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md).

## Current UI Shape

The UI is currently an Angular application with server-side rendering support, a thin root app host, and a minimal routed shell baseline.

- `UI/src/main.ts` bootstraps the browser application.
- `UI/src/main.server.ts` bootstraps the server render entry point.
- `UI/src/server.ts` hosts the generated SSR server entry.
- `UI/src/app/app.ts` is the root app host and should stay focused on the top-level router outlet.
- `UI/src/app/app.config.ts` and `app.config.server.ts` hold browser and server configuration.
- `UI/src/app/app.routes.ts` is the route entry point for browser routes, with `app.routes.server.ts` holding the server route definitions.
- `UI/src/app/shell/root-shell.*` owns shell-level layout and nested route composition.
- `UI/src/app/features/home/home-placeholder.*` is the current baseline feature route rendered inside the shell.
- `UI/src/locale/messages.xlf` is the canonical extracted source-messages file for Angular i18n.
- `UI/src/styles.scss` is the shared global stylesheet entry point.
- `UI/public/` holds static assets copied by the Angular build.

## Ownership And Placement Rules

- Put browser-facing UI work in `UI/src/app/` unless there is a clear reason to create a new top-level folder under `src/`.
- Keep feature code close together: route definition, component, template, styles, and tests should stay near each other.
- Treat `UI/src/app/app.routes.ts` as the canonical place to grow the route tree until a feature area is large enough to justify a route-specific substructure.
- Keep shell-only layout concerns in `UI/src/app/shell/` and keep feature-specific rendering out of the shell.
- Add new routed user-facing areas under `UI/src/app/features/` so each feature keeps its component, template, styles, and tests together.
- Keep Angular localization artifacts owned by `UI/src/locale/`.
- Treat `UI/src/locale/messages.xlf` as generated project state that should stay in sync with user-facing template text marked for translation.
- Add shared static assets to `UI/public/`.
- If locale-specific static assets are introduced later, keep them under a locale-scoped folder in `UI/public/` such as `UI/public/i18n/<locale>/` instead of scattering them across feature folders.
- Keep repository-level guidance in `UI/README.md` and `UI/AGENTS.md`; do not duplicate that detail inside feature files.
- If the UI grows beyond the current baseline, prefer adding feature-focused subfolders under `UI/src/app/features/` rather than expanding `app.ts` or putting feature logic into `root-shell`.

## Conventions

### Application Structure

- Keep the root `App` component focused on app bootstrap concerns and router outlet composition.
- Keep `RootShell` focused on shell concerns such as app-level layout, navigation framing, and nested router outlet composition.
- Put feature-specific behaviour in feature components, routes, and services rather than in `App` or the root shell.
- Prefer standalone Angular APIs and keep module-era patterns out unless an existing dependency requires them.
- When adding a reusable abstraction, make sure at least two consumers justify it.

### Localization Ownership

- Mark visible user-facing template strings where they are rendered by using Angular template `i18n` metadata instead of moving string ownership into shared constants prematurely.
- Keep extraction and source-locale message files under `UI/src/locale/`, because they are part of the UI source tree rather than public static content.
- Keep localization workflow details, contributor commands, and CI reproduction steps in `UI/README.md`; this guide should only record where localization files live and which part of the UI owns them.
- If a future feature introduces locale selection, translation loading, or locale-specific routing, document the owning feature area here in the same change.

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
- For the current Angular app, prefer focused unit or shallow integration tests around rendered output, shell-plus-route composition, and component interaction.
- Assert on visible behaviour and state, not internal implementation details.
- When a UI change affects accessibility or semantics, include assertions or manual validation notes that cover the changed experience.

## Review Checklist

- Does the change belong in `UI/` rather than `API/`, `Plans/`, or `Roadmap/`?
- Is new UI code placed near the route, component, or feature it belongs to?
- Does the change preserve the current ownership split between `app.ts`, `app.routes.ts`, `shell/`, and `features/`?
- Are accessibility, responsive layout, and keyboard/focus behaviour considered for user-facing changes?
- Are tests or validation notes appropriate for the level of UI change?
- If a new structural convention was introduced, was this file updated to capture it?
