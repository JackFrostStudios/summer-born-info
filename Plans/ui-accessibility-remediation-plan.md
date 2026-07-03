# UI Accessibility Remediation Plan

## 1. Overview

Implement the accessibility improvements identified in `UI/review-findings/accessibility-expert.md` so the Angular UI has predictable SPA navigation announcements, route-specific titles, a reusable skip-link mechanism, clearer toggle semantics, stronger shared button naming rules, and automated accessibility smoke coverage for both light and dark themes.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-03 to create an implementation plan for `UI/review-findings/accessibility-expert.md`.
- Review source: `UI/review-findings/accessibility-expert.md`.
- User-confirmed resolutions:
  - Theme toggle should keep a stable toggle name of `Dark mode`.
  - Route-change focus should move to the routed page `<h1>`.
  - Skip links should be implemented as a common component, with route components providing human-readable skip-link names and anchor targets.
  - Skip-link behaviour should account for Angular `withInMemoryScrolling`.
  - Accessibility automation should live in separate `component.a11y-spec.ts` tests using Playwright, Chrome, and `axe-core`, and should verify both light and dark mode with styles loaded correctly.
  - The shared button should only apply `aria-labelledby` when both related inputs are provided, and should log a development-only warning if conflicting accessible-name inputs are supplied.
- Relevant repository guidance:
  - [Plans/AGENTS.md](./AGENTS.md)
  - [UI/AGENTS.md](../UI/AGENTS.md)
  - [AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md)
  - [UI/AI_PROJECT_GUIDE.md](../UI/AI_PROJECT_GUIDE.md)

## 3. Scope

- Add route-owned accessibility metadata for document titles, focus targets, and skip links.
- Add shell-level route-change accessibility handling that updates the document title, announces the new page context, and moves focus to the routed page `<h1>`.
- Add a reusable skip-link component that renders route-provided bypass links and works with the app shell.
- Normalize the theme toggle to the stable-name toggle-button pattern with `aria-pressed`.
- Update the base document metadata to use a readable title and `en-GB` language tag.
- Tighten the shared button accessible-name API and development diagnostics.
- Add dedicated automated accessibility smoke tests using Playwright, Chrome, and `axe-core`.
- Update existing component and route tests to cover the new accessibility behaviour.

## 4. Non-Goals

- Reworking site navigation structure beyond what is needed to support skip links and route metadata.
- Converting the header brand into a home link during this slice.
- Replacing manual accessibility checks such as keyboard flow, zoom/reflow, forced colors, or screen reader smoke tests with automation.
- Building a generalized live-region framework beyond what is needed for route-change context announcement.
- Expanding accessibility automation to every component in the app during the first pass if representative shell and route coverage is sufficient.

## 5. Behaviour Scenarios

### Scenario: A visitor navigates between SPA routes

Given a keyboard or screen-reader user activates navigation that changes the Angular route, when the new route finishes activation without a specific fragment target, then the document title should update to the route-specific title, the routed page `<h1>` should receive programmatic focus, and assistive technology should have a reliable cue that the main content changed.

### Scenario: A visitor navigates to a specific in-page anchor

Given navigation includes a fragment or is triggered by a skip link, when the destination route and target element are available, then focus should move to the requested anchor target instead of the default page `<h1>` so the user lands on the content they explicitly asked for.

### Scenario: A visitor lands on the homepage

Given the homepage route is active, when the page renders, then the browser title should read `Summer-born Info - Home`, the page should expose a single focusable `<h1>` target for route-change focus, and the route should publish any skip-link destinations needed by the shell.

### Scenario: A visitor lands on the under-construction route

Given the under-construction route is active, when the page renders, then the browser title should read `Summer-born Info - Page coming soon`, the page should expose its `<h1>` as the route-change focus target, and the route should publish any skip-link destinations needed by the shell.

### Scenario: A keyboard user wants to bypass repeated shell content

Given the user tabs from the top of the page, when focus reaches the first shell focusable element, then a reusable skip-link component should reveal one or more route-provided bypass links such as `Skip to main content` as an overlay at the top of the screen, and activating one of those links should move the user to the declared in-page destination without fighting router scroll restoration.

### Scenario: The skip-link UI stays out of the way when not in use

Given the page is rendered and the skip-link control is not focused, when the user is interacting with the rest of the page, then the skip-link should remain visually hidden but still become available as the first keyboard-focusable shell control. When the skip link receives focus it should appear prominently as a top-of-screen overlay, and when focus leaves it should hide again.

### Scenario: The theme toggle announces a stable name and explicit state

Given the theme toggle is rendered, when assistive technology reads the control, then the accessible name should remain `Dark mode` while `aria-pressed="true|false"` communicates whether dark mode is currently active.

### Scenario: A developer supplies conflicting button naming inputs

Given a consumer passes both `ariaLabel` and `ariaLabelledBy` inputs to the shared button, when the component renders in development mode, then the component should log a warning describing the conflict and should only emit `aria-labelledby` when the required related inputs are present according to the agreed contract.

### Scenario: Accessibility smoke tests run in CI or local validation

Given the dedicated accessibility test suite executes, when it mounts the shell, homepage, under-construction route, and theme toggle in both light and dark mode, then Playwright and `axe-core` should report no known automated accessibility violations for the tested states with the application styles loaded.

## 6. Deliverables

1. Define a route accessibility metadata contract that each routed page can own, covering at minimum:
   - route title,
   - page focus target id for the `<h1>`,
   - skip-link entries with human-readable label and anchor target.
2. Update `UI/src/app/app.routes.ts` route definitions so the homepage and under-construction route declare their accessibility metadata and readable route titles.
3. Implement shell-level route accessibility handling that responds to navigation completion by:
   - applying the route title,
   - moving focus to the routed page `<h1>` by default,
   - overriding the default focus target when navigation includes a fragment or skip-link anchor target,
   - providing any required announcement mechanism for SPA context changes.
4. Update routed feature components in scope so each page exposes a stable `<h1 id="...">` target that matches its route metadata and is safe to focus programmatically.
5. Add a reusable skip-link component in a common shared location appropriate for shell reuse, and render it as the first focusable shell element.
   The component should be visually hidden until focused, then appear as a prominent overlay at the top of the screen, and hide again once focus moves away.
6. Configure the shell and router setup so skip-link anchors and focus behaviour work with Angular `withInMemoryScrolling` instead of being undermined by scroll restoration or fragment handling.
7. Update the theme-control component to use the stable toggle name `Dark mode` while preserving `aria-pressed` for state.
8. Update theme-control tests to assert the stable accessible name and pressed-state behaviour.
9. Update `UI/src/index.html` and any title bootstrap logic so the base title reads `Summer-born Info` and the document language is `en-GB`.
10. Tighten the shared button component API so:
    - the agreed `aria-labelledby` forwarding rule is applied,
    - conflicting accessible-name inputs trigger a development-only warning,
    - tests cover the supported naming strategies and the warning path.
11. Add dedicated accessibility smoke tests using separate `component.a11y-spec.ts` files and the required tooling:
    - Playwright,
    - Chrome,
    - `axe-core`.
12. Ensure the accessibility smoke harness loads the real application styles and exercises both light and dark mode.
13. Add or update existing unit/component tests for route titles, route-change focus, skip-link rendering and activation, and button naming behaviour.
14. Update `UI/AI_PROJECT_GUIDE.md` if the change introduces a new canonical ownership pattern for route accessibility metadata or shared skip-link components.
15. Run the standard UI validation commands plus the new accessibility test command(s).

## 7. Technology Requirements and Decisions

- Route metadata ownership:
  Use Angular route metadata as the canonical ownership point for page titles and route-level skip-link declarations so each feature route owns its own accessibility context.
- Focus target:
  The shell should move focus to the routed page `<h1>` after navigation by default, not to `<main>` or the document root, because that gives screen-reader users the clearest page-context announcement.
- Anchor-target override:
  When navigation is intentionally directed to a fragment or skip-link target, that requested anchor should receive focus instead of the default `<h1>` so the app respects the user's explicit destination.
- Skip-link ownership:
  Implement the skip-link UI as a reusable shared component rather than embedding raw markup in the shell template so future routed areas can reuse the same pattern.
- Skip-link presentation:
  The skip-link should follow the conventional accessible pattern of being visually hidden until focused, then appearing as a clear overlay at the top of the viewport, and hiding again when focus leaves.
- Skip-link data contract:
  Route components that already own page title metadata should also own their skip-link descriptors, including visible label text and the destination anchor id or fragment.
- Router behaviour:
  Enable or extend Angular router `withInMemoryScrolling` behaviour so skip-link activation and back/forward navigation do not conflict with fragment scrolling expectations.
- Title handling:
  Prefer Angular route `title` metadata or a small title strategy integrated with the route accessibility metadata instead of ad hoc per-component document writes.
- Announcement strategy:
  Keep the SPA route-change announcement lightweight and tied to the same shell-level navigation listener used for focus and title updates. If focus-to-`<h1>` provides sufficient announcement in practice, avoid inventing a heavier live-region abstraction unless testing shows it is needed.
- Shared button API:
  Preserve the existing button abstraction but make its accessible-name precedence explicit. The implementation should avoid silently applying conflicting naming attributes.
- Development diagnostics:
  Use Angular development-mode checks for the shared button warning so production output stays clean.
- Accessibility automation dependencies:
  Adding Playwright and `axe-core` is a material tooling change and should be recorded in the implementation. The plan assumes Chrome-based execution because the user explicitly requested Chrome coverage.
- Test harness:
  Keep the new accessibility tests separate from existing Vitest component specs if that separation is required to load browser styles and Playwright reliably.

## 8. Dependencies and Sequencing

1. Define the route accessibility metadata contract and decide where shared types/helpers live.
2. Update router configuration to support route titles, skip-link metadata consumption, and `withInMemoryScrolling`.
3. Add shell-level route accessibility orchestration for title updates, default `<h1>` focus management, fragment-target focus overrides, and any lightweight announcement behaviour.
4. Update the homepage and under-construction route components/templates so their `<h1>` ids and skip-link targets match the new contract.
5. Add the reusable skip-link component and render it in the shell as the first focusable element.
6. Normalize theme-toggle semantics and update its tests.
7. Tighten the shared button naming API and add development-only warning coverage.
8. Update base document metadata (`title`, `lang`) and any related bootstrap/title wiring.
9. Introduce Playwright, Chrome-based accessibility smoke setup, and `axe-core` integration in separate `component.a11y-spec.ts` files with real styles loaded.
10. Add or update validation scripts so the new accessibility suite has a clear execution path.
11. Run formatting, linting, unit/component tests, i18n validation if user-facing template text changes, and the new accessibility suite.

## 9. Risks and Mitigations

- Risk: Programmatic focus to the `<h1>` may race route rendering and fail intermittently.
  - Mitigation: trigger focus only after navigation completion and rendered content availability, and cover it with deterministic tests.
- Risk: Skip-link fragment navigation may conflict with Angular scroll restoration.
  - Mitigation: explicitly configure and test `withInMemoryScrolling` behaviour with shell skip links and route transitions.
- Risk: Route metadata and component markup can drift out of sync if ids are hand-maintained.
  - Mitigation: keep the metadata contract small, typed, and close to the route definition, and test the rendered focus target ids for in-scope routes.
- Risk: The shared button warning could become noisy or ambiguous for developers.
  - Mitigation: warn only in development mode and make the message specific about which naming inputs conflict and which one takes precedence.
- Risk: Browser-based accessibility tests may be flaky if styles or app bootstrapping are incomplete.
  - Mitigation: use a dedicated harness that loads the real styles intentionally, limit the first slice to representative scenarios, and keep dark-mode setup explicit.
- Risk: New tooling increases CI/runtime cost.
  - Mitigation: keep the accessibility suite focused on shell and critical routes first, and wire it as a distinct test target so teams can run it intentionally.

## 10. Unknowns and Required Clarifications

- No blocking clarification remains for this implementation plan after the user confirmed the `<h1>` focus target.
- The implementation should document the exact route metadata shape and where it lives once the code change chooses the final TypeScript ownership pattern.
- If Playwright-based accessibility coverage needs a new npm script, the implementation should choose a clear naming convention consistent with existing UI scripts.

## 11. Completion Checklist

- [x] Route definitions in `UI/src/app/app.routes.ts` declare readable route titles and accessibility metadata for in-scope routes.
- [ ] SPA route changes update the document title and move focus to the routed page `<h1>` by default.
- [ ] Fragment and skip-link navigation override the default focus target and focus the requested anchor element instead.
- [ ] The homepage and under-construction page expose stable focusable `<h1>` targets that match the route metadata contract.
- [ ] A reusable skip-link component exists and renders as the first focusable shell element.
- [ ] The skip-link component is visually hidden until focused, appears as a top-of-screen overlay while focused, and hides again when focus leaves.
- [ ] Skip-link activation works with the configured Angular `withInMemoryScrolling` behaviour.
- [ ] The theme toggle exposes the stable accessible name `Dark mode` and uses `aria-pressed` for state.
- [ ] `UI/src/index.html` uses the readable base title `Summer-born Info`.
- [ ] `UI/src/index.html` uses `lang="en-GB"` or equivalent locale-correct output for the source locale.
- [ ] The shared button only applies the agreed `aria-labelledby` behaviour and emits a development-only warning for conflicting naming inputs.
- [ ] Shared button tests cover supported accessible-name strategies and the warning path.
- [ ] Separate `component.a11y-spec.ts` coverage exists for representative shell and route states using Playwright, Chrome, and `axe-core`.
- [ ] Accessibility smoke coverage verifies both light mode and dark mode with styles loaded correctly.
- [ ] Existing unit/component tests are updated for route titles, route-change focus, skip links, theme-toggle semantics, and button naming behaviour.
- [ ] `UI/AI_PROJECT_GUIDE.md` is updated if new shared ownership conventions are introduced.
- [ ] `npm run format` has been run in `UI/`.
- [ ] `npm run lint` has been run in `UI/`.
- [ ] `npm run test:run` has been run in `UI/`.
- [ ] `npm run extract:i18n` has been run in `UI/` if template text or i18n metadata changed.
- [ ] `npm run validate:i18n` has been run in `UI/` if localization output changed.
- [ ] The new accessibility smoke test command has been run successfully in `UI/`.
