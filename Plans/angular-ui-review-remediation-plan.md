# Angular UI Review Remediation Plan

## 1. Overview

Resolve the findings captured in `UI/review-findings/angular-expert.md`, using the current `UI/` codebase as the source of truth for issue status. This plan keeps every reviewed issue visible for tracking, including findings that have already been fixed since the review was written.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-05 to create a remediation plan for `UI/review-findings/angular-expert.md`.
- Review source: `UI/review-findings/angular-expert.md`.
- Verification source: a dedicated codebase-check sub-agent verified each issue against the current repository state on 2026-07-05 before this plan was drafted.
- Relevant repository guidance:
  - `Plans/AGENTS.md`
  - `UI/AGENTS.md`
  - `AI_PROJECT_GUIDE.md`
  - `UI/AI_PROJECT_GUIDE.md`

### Verified Review Status Snapshot

1. Issue 1: SSR hydration missing `withI18nSupport()` and `withEventReplay()` - `Open`
2. Issue 2: Secondary route components are still eagerly loaded - `Open`
3. Issue 3: Routed pages missing Angular route titles - `Resolved`
4. Issue 4: Template linting does not cover design-system HTML - `Open`
5. Issue 5: Source locale and document shell language differ - `Resolved`
6. Issue 6: Technical ARIA ID reference is incorrectly marked for translation - `Open`
7. Issue 7: `ThemeControlService` still uses `@Injectable` instead of Angular v22 `@Service` - `Open`
8. Issue 8: No wildcard / not-found client route exists - `Open`

## 3. Scope

- Fix the still-open Angular review findings in the UI application.
- Preserve resolved findings in the plan so their status remains traceable.
- Add or update tests where route, hydration, i18n, or linting behavior changes.
- Refresh localization and run the normal UI validation workflow for any implementation slice that changes templates, routes, or Angular configuration.

## 4. Non-Goals

- Reworking homepage content or shell layout beyond what is needed to resolve the tracked review findings.
- Broad performance optimization outside the specific lazy-loading change identified in the review.
- Introducing new routing architecture beyond what is necessary for a lazy secondary route and a not-found experience.
- Reopening issues 3 and 5 unless regression work shows they are no longer actually fixed.

## 5. Behaviour Scenarios

### Scenario: A visitor interacts before Angular hydration completes

Given the server-rendered homepage is visible, when a visitor clicks the CTA or theme toggle before client hydration finishes, then Angular should replay the interaction after hydration and preserve hydration support for i18n-rendered DOM.

### Scenario: A visitor opens the primary homepage route

Given the homepage is the primary landing route, when the application loads, then the route may remain eagerly loaded and continue exposing the existing route title and accessibility metadata.

### Scenario: A visitor navigates to the secondary under-construction route

Given the visitor navigates to `/under-construction`, when the router resolves the route, then the page should load correctly through a lazy route configuration without regressing the current title and accessibility metadata.

### Scenario: Shared design-system templates are linted

Given a developer runs the UI lint workflow, when ESLint evaluates Angular templates, then `src/design-system/**/*.html` should receive the same Angular template and accessibility rules already applied to `src/app/**/*.html`.

### Scenario: The homepage article exposes ARIA relationships

Given the homepage article references `home-heading`, when Angular extracts translations, then the technical ID reference should remain fixed in source code and should not be exposed for translator modification.

### Scenario: A singleton service follows the current Angular convention

Given `ThemeControlService` remains a root singleton, when the service metadata is updated to the Angular v22-preferred form, then existing behavior and tests for theme persistence, system theme sync, and DOM updates should still pass unchanged.

### Scenario: A visitor lands on an unknown client route

Given a visitor navigates to a path that does not match a known client route, when the router handles the URL in the browser, then the app should render a not-found route inside the existing shell with a route title and predictable content instead of leaving the view unmatched.

## 6. Deliverables

1. Update `UI/src/app/app.config.ts` hydration setup to use `provideClientHydration(withI18nSupport(), withEventReplay())`.
2. Add focused verification for hydration-related configuration so the app config tests fail if either hydration option is removed.
3. Keep the homepage route eagerly loaded, but convert the secondary `under-construction` route to lazy loading via `loadComponent`.
4. Update route tests so they assert the lazy-loading contract without regressing existing route-title and route-accessibility metadata expectations.
5. Expand Angular template lint coverage in `UI/eslint.config.js` from `src/app/**/*.html` to `src/**/*.html`.
6. Resolve any new lint findings surfaced in `UI/src/design-system/**` or other shared template locations after the lint scope expands.
7. Remove `i18n-aria-labelledby` from `UI/src/app/features/home/home.html` so the technical ID reference is no longer translatable.
8. Update homepage tests and refresh i18n artifacts if the ARIA metadata change affects extracted output.
9. Migrate `UI/src/app/shell/theme-control/theme-control.service.ts` from `@Injectable({ providedIn: 'root' })` to Angular v22 `@Service()` if the project version and tooling support it cleanly.
10. Keep the existing theme-control service test coverage green so the migration remains convention-only and does not alter runtime behavior.
11. Add a dedicated not-found route feature under `UI/src/app/features/` and register a final `**` route in `UI/src/app/app.routes.ts`.
12. Ensure the not-found route renders inside the existing public shell, has a route title, and follows current i18n and accessibility expectations.
13. Add or update router tests for unmatched URLs and not-found route registration.
14. Preserve the already-fixed route-title and `lang="en-GB"` findings as explicit resolved items in the implementation record rather than silently omitting them.
15. Run `npm run format`, `npm run lint`, `npm run test:run`, and `npm run validate:i18n` in `UI/`, plus `npm run extract:i18n` if the ARIA metadata or not-found route introduces user-facing template or extraction changes.

## 7. Technology Requirements and Decisions

- Hydration:
  Use Angular hydration features from `@angular/platform-browser` rather than custom client boot logic. The remediation should explicitly enable both `withI18nSupport()` and `withEventReplay()`.
- Route-loading strategy:
  Keep the homepage eager because it is the primary landing route, but move secondary routed UI to `loadComponent` so the app grows in the right direction without over-optimizing the first screen.
- Lint coverage:
  Align ESLint's flat-config template target with the broader `angular.json` lint scope so Angular template accessibility rules apply consistently across app and design-system templates.
- ARIA and i18n:
  Treat ID references such as `aria-labelledby="home-heading"` as technical wiring, not translatable content.
- Service convention:
  Migrate the theme-control singleton to `@Service()` only if the current Angular version and local tooling support it cleanly; if implementation uncovers compatibility issues, keep the plan item but document the blocker explicitly instead of forcing churn.
- Not-found route:
  Implement a real client-side `**` route rather than relying on server prerender fallbacks alone, so SSR and browser navigation behavior stay aligned.
- Validation:
  Reuse the existing UI validation commands and existing route/service test suites rather than adding separate one-off checks unless a hydration-specific smoke test becomes clearly necessary during implementation.

## 8. Dependencies and Sequencing

1. Confirm the route-loading and not-found route shape so routing tests can be updated once instead of repeatedly.
2. Update hydration configuration and app-config tests because that work is self-contained and low-conflict.
3. Convert the secondary route to `loadComponent` and update route tests.
4. Expand ESLint template coverage and fix any newly surfaced shared-template violations.
5. Remove the translatable ARIA ID metadata and refresh related tests and i18n artifacts.
6. Migrate the theme-control singleton decorator while keeping the current behavior tests intact.
7. Add the not-found feature route, route title, and wildcard registration, then add unmatched-route coverage.
8. Finish with formatting, linting, tests, and i18n validation.

### Execution Progress

- [x] Hydration configuration and app-config tests updated on 2026-07-05.
- [x] Secondary `under-construction` route converted to lazy loading with focused route-test coverage on 2026-07-05.
- [x] Angular template lint coverage expanded to `src/**/*.html` and newly surfaced shared-template findings resolved on 2026-07-05.
- [x] Technical homepage ARIA ID translation metadata removed and source extraction refreshed on 2026-07-05.

## 9. Risks and Mitigations

- Risk: Hydration changes can be easy to misconfigure without obvious local failures.
  - Mitigation: add direct config-level test coverage and include a manual SSR/hydration verification note during implementation if no automated hydration smoke test exists yet.
- Risk: Expanding lint coverage may uncover unrelated design-system template violations.
  - Mitigation: treat the lint expansion and resulting fixes as one slice so the repo does not stay red between commits.
- Risk: Switching to `loadComponent` can break route tests that currently expect direct component references.
  - Mitigation: update route specs in the same slice and keep route titles/accessibility assertions intact.
- Risk: `@Service()` support could differ between Angular packages, tooling, or local lint rules.
  - Mitigation: verify framework/tooling support during implementation and document the item as blocked or intentionally deferred if the convention is not yet mechanically supported.
- Risk: A not-found route can drift from the shell's accessibility and title conventions.
  - Mitigation: build it as a normal routed feature under the existing shell and cover its route title plus unmatched-path registration in tests.

## 10. Unknowns and Required Clarifications

- No blocking product clarification is currently required to produce this plan.
- The only implementation-time checkpoint to re-evaluate is issue 7: if `@Service()` support proves incomplete in the active Angular/tooling stack, the implementation should record that constraint and keep the item visible rather than forcing a speculative migration.
- If the user wants a custom UX/content review for the not-found page before implementation, that review should happen before writing final route copy.

## 11. Completion Checklist

- [x] Issue 1 open finding is resolved by enabling hydration i18n support and event replay.
- [x] Hydration configuration has direct automated coverage or a documented manual verification step.
- [x] Issue 2 open finding is resolved by keeping the homepage eager and lazy-loading the secondary route.
- [x] Issue 3 remains resolved: route titles exist and stay covered by tests.
- [x] Issue 4 open finding is resolved by linting all Angular HTML templates under `src/**/*.html`.
- [x] Any newly surfaced shared-template lint violations are fixed.
- [x] Issue 5 remains resolved: the document shell language matches the `en-GB` source locale.
- [x] Issue 6 open finding is resolved by removing translation metadata from the technical ARIA ID reference.
- [ ] Issue 7 open finding is resolved by migrating the singleton service to `@Service()`, or the implementation records a concrete framework/tooling blocker if that is not currently safe.
- [ ] Issue 8 open finding is resolved by adding a client-side wildcard route and not-found experience.
- [ ] Router tests cover lazy secondary routing and unmatched-route handling.
- [ ] Any affected service, route, or template tests are updated and passing.
- [ ] `npm run format` has been run in `UI/`.
- [ ] `npm run lint` has been run in `UI/`.
- [ ] `npm run test:run` has been run in `UI/`.
- [ ] `npm run extract:i18n` has been run in `UI/` if extraction-relevant template metadata changed.
- [ ] `npm run validate:i18n` has been run in `UI/` if localization output changed.
