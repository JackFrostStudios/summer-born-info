# UI Accessibility Remediation Plan

## 1. Overview

Implement the accessibility improvements identified in `UI/review-findings/accessibility-expert.md` so the Angular UI has predictable SPA navigation announcements, route-specific titles, a reusable skip-link mechanism, clearer toggle semantics, stronger shared button naming rules, and automated accessibility smoke coverage for both light and dark themes.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-03 to create an implementation plan for `UI/review-findings/accessibility-expert.md`.
- Review source: `UI/review-findings/accessibility-expert.md`.
- User-confirmed resolutions:
  - Theme toggle should keep a stable toggle name of `Dark mode`.
  - Route changes should clear carried focus instead of moving focus to the routed page `<h1>`, so the skip link remains the first keyboard entry point after navigation.
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
- Add shell-level route-change accessibility handling that updates the document title, announces the new page context, and clears carried focus so the skip link remains the next keyboard action after navigation.
- Add a reusable skip-link component that renders route-provided bypass links and works with the app shell.
- Normalize the theme toggle to the stable-name toggle-button pattern with `aria-pressed`.
- Update the base document metadata to use a readable title and `en-GB` language tag.
- Tighten the shared button accessible-name API and development diagnostics.
- Add dedicated automated accessibility smoke tests using Playwright-driven Chrome and `axe-core`, exposed through a dedicated npm command that loads the shared application stylesheet entry point for realistic rendering.
- Update existing component and route tests to cover the new accessibility behaviour.

## 4. Non-Goals

- Reworking site navigation structure beyond what is needed to support skip links and route metadata.
- Converting the header brand into a home link during this slice.
- Replacing manual accessibility checks such as keyboard flow, zoom/reflow, forced colors, or screen reader smoke tests with automation.
- Building a generalized live-region framework beyond what is needed for route-change context announcement.
- Expanding accessibility automation to every component in the app during the first pass if representative shell and route coverage is sufficient.

## 5. Behaviour Scenarios

### Scenario: A visitor navigates between SPA routes

Given a keyboard or screen-reader user activates navigation that changes the Angular route, when the new route finishes activation, then the document title should update to the route-specific title, any carried focus from the previous view should be cleared, and assistive technology should have a reliable cue that the main content changed without skipping over the first shell skip link.

### Scenario: A visitor navigates to a specific in-page anchor

Given navigation includes a fragment or is triggered by a skip link, when the destination route and target element are available, then the shell should focus the requested anchor target so the user lands on the content they explicitly asked for.

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
   - clearing carried focus so the next keyboard interaction reaches the shell skip link,
   - preserving shell-managed focus only for explicit fragment or skip-link anchor targets,
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
    - Playwright component/browser execution,
    - Chrome as the validated browser target,
    - `axe-core`.
12. Add a dedicated npm command for the accessibility suite, with a clear project-local name such as `npm run test:a11y`, so developers and CI can run the browser accessibility smoke coverage intentionally.
13. Ensure the accessibility smoke harness loads the real application styles by pulling in `UI/src/styles.scss` and therefore the shared CSS modules it composes, then exercises both light and dark mode.
14. Add or update existing unit/component tests for route titles, route-change focus, skip-link rendering and activation, theme-toggle semantics, and button naming behaviour.
15. Update `UI/AI_PROJECT_GUIDE.md` if the change introduces a new canonical ownership pattern for route accessibility metadata, shared skip-link components, or browser-based accessibility test placement.
16. Run the standard UI validation commands plus the new accessibility test command(s).

## 7. Technology Requirements and Decisions

- Route metadata ownership:
  Use Angular route metadata as the canonical ownership point for page titles and route-level skip-link declarations so each feature route owns its own accessibility context.
- Focus handling:
  The shell should clear carried focus after navigation instead of forcing focus to the routed page `<h1>`, because the skip link is intentionally the first keyboard action available after each route change.
- Anchor-target behavior:
  When navigation is intentionally directed to a fragment or skip-link target, that requested anchor should receive focus so the app respects the user's explicit destination.
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
- Browser target:
  The accessibility suite should execute against Playwright-launched Chrome rather than the default bundled browser so the validation matches the requested browser environment.
- Style loading:
  The accessibility harness should load the same `UI/src/styles.scss` entry point used by the app so the shared style modules, tokens, resets, and primitives participate in axe validation and visual semantics.
- Validation command:
  Provide a dedicated npm entry point for the accessibility suite, preferably `npm run test:a11y`, so local development and CI have an unambiguous command for the Chrome + Playwright + `axe-core` checks.

## 8. Dependencies and Sequencing

1. Define the route accessibility metadata contract and decide where shared types/helpers live.
2. Update router configuration to support route titles, skip-link metadata consumption, and `withInMemoryScrolling`.
3. Add shell-level route accessibility orchestration for title updates, default `<h1>` focus management, fragment-target focus overrides, and any lightweight announcement behaviour.
4. Update the homepage and under-construction route components/templates so their `<h1>` ids and skip-link targets match the new contract.
5. Add the reusable skip-link component and render it in the shell as the first focusable element.
6. Finish the router/skip-link integration so `withInMemoryScrolling` preserves fragment scrolling and explicit anchor focus for shell skip links across forward/back navigation.
7. Normalize theme-toggle semantics and update its tests.
8. Tighten the shared button naming API and add development-only warning coverage.
9. Update base document metadata (`title`, `lang`) and any related bootstrap/title wiring.
10. Introduce Playwright, Chrome-based accessibility smoke setup and `axe-core` integration in separate `component.a11y-spec.ts` files, with `UI/src/styles.scss` loaded into the browser test harness.
11. Add the dedicated npm accessibility command and any supporting browser-test configuration so `npm run test:a11y` executes the Chrome coverage path consistently.
12. Add or update focused validation for route titles, route-change focus, skip-link activation, theme semantics, and button naming behaviour.
13. Run formatting, linting, unit/component tests, i18n validation if user-facing template text changes, and the new accessibility suite.

## 9. Risks and Mitigations

- Risk: Shell-managed focus changes may fight Angular navigation timing and skip-link expectations.
  - Mitigation: keep the shell behavior to title/announcement updates plus clearing carried focus, and cover the behavior with deterministic tests.
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

- No blocking clarification remains for this implementation plan after the user confirmed the route-change focus strategy should preserve the skip link as the first keyboard action.
- The implementation should document the exact route metadata shape and where it lives once the code change chooses the final TypeScript ownership pattern.
- The Playwright-based accessibility coverage should use a dedicated npm script named `npm run test:a11y` unless implementation constraints inside the Angular/Vitest browser runner force a repository-consistent variant with the same intent.

## 11. Completion Checklist

- [x] Route definitions in `UI/src/app/app.routes.ts` declare readable route titles and accessibility metadata for in-scope routes.
- [x] SPA route changes update the document title and clear carried focus so the skip link remains the first keyboard action after navigation.
- [x] Fragment and skip-link navigation focus the requested anchor element instead of clearing focus back to the skip-link flow.
- [x] The homepage and under-construction page expose stable focusable `<h1>` targets that match the route metadata contract.
- [x] A reusable skip-link component exists and renders as the first focusable shell element.
- [x] The skip-link component is visually hidden until focused, appears as a top-of-screen overlay while focused, and hides again when focus leaves.
- [x] Skip-link activation works with the configured Angular `withInMemoryScrolling` behaviour.
- [x] The theme toggle exposes the stable accessible name `Dark mode` and uses `aria-pressed` for state.
- [x] `UI/src/index.html` uses the readable base title `Summer-born Info`.
- [x] `UI/src/index.html` uses `lang="en-GB"` or equivalent locale-correct output for the source locale.
- [x] The shared button only applies the agreed `aria-labelledby` behaviour and emits a development-only warning for conflicting naming inputs.
- [x] Shared button tests cover supported accessible-name strategies and the warning path.
- [x] Separate `component.a11y-spec.ts` coverage exists for representative shell and route states using Playwright, Chrome, and `axe-core`.
- [x] Accessibility smoke coverage verifies both light mode and dark mode with styles loaded correctly.
- [x] Existing unit/component tests are updated for route titles, route-change focus, skip links, theme-toggle semantics, and button naming behaviour.
- [x] `UI/AI_PROJECT_GUIDE.md` is updated if new shared ownership conventions are introduced.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run test:run` has been run in `UI/`.
- [x] `npm run extract:i18n` has been run in `UI/` if template text or i18n metadata changed.
- [x] `npm run validate:i18n` has been run in `UI/` if localization output changed.
- [x] `npm run test:a11y` has been added for the browser accessibility smoke suite and run successfully in `UI/`.

## 12. Implementation Progress

- [x] Step 1 completed in commit `3096001`: defined the shared route accessibility contract, wired readable route titles and metadata into `UI/src/app/app.routes.ts`, added route contract coverage, and documented the ownership pattern in `UI/AI_PROJECT_GUIDE.md`.
- [x] Step 2 completed in progress: enabled Angular `withInMemoryScrolling` in `UI/src/app/app.config.ts`, added route accessibility metadata lookup helpers for router-state consumption, and covered router title plus scrolling configuration in `UI/src/app/app.config.spec.ts`.
- [x] Step 3 completed in progress: added shell-level route accessibility orchestration in `UI/src/app/shell/root-shell/` for route announcements, carried-focus clearing on default navigation, and explicit fragment-target focus behavior with focused shell tests.
- [x] Step 4 completed in progress: confirmed the homepage and under-construction routes render `<h1>` targets that match their route accessibility metadata and added focused specs to prevent future drift between route focus/skip-link ids and the DOM.
- [x] Step 5 completed in progress: added a reusable shell-owned skip-link component, rendered it before the header as the first focusable shell control, and covered route-driven skip-link rendering plus focus-triggered reveal behaviour with focused specs.
- [x] Step 6 completed in progress: finished the shell/router skip-link path so repeated skip-link activation on the same fragment re-focuses correctly, explicit fragment navigation still focuses the requested anchor, and browser back/forward restoration no longer fights Angular `withInMemoryScrolling`, with focused shell and route-accessibility coverage.
- [x] Step 7 completed in progress: normalized `UI/src/app/shell/theme-control/` so the toggle keeps the stable accessible name `Dark mode`, continues to expose `aria-pressed` for state, adds focused semantics coverage, and refreshes the source locale catalog for the new label contract.
- [x] Step 8 completed in progress: updated `UI/src/index.html` to use the readable base title `Summer-born Info` and `lang="en-GB"`, and tightened the route accessibility service so routes without metadata fall back to the base document title instead of leaving stale route titles behind.
- [x] Step 9 completed in progress: tightened the shared button accessible-name contract in `UI/src/design-system/button/`, only forward `aria-labelledby` when `ariaLabel` is absent and the supplied label reference is non-blank, added a development-only warning for conflicting explicit naming inputs, and expanded button tests to cover visible text, `aria-label`, `aria-labelledby`, blank references, and the warning path.
- [x] Step 10 completed in progress: added a dedicated browser accessibility harness with Playwright-backed Angular browser tests, shared `axe-core` helpers that load the real `UI/src/styles.scss` entry point, and separate `component.a11y-spec.ts` smoke coverage for the root shell, homepage, under-construction route, and theme toggle in both light and dark mode.
- [x] Step 11 completed in progress: added the dedicated `npm run test:a11y` command, documented the browser accessibility smoke workflow in `UI/README.md`, made the Angular `test-a11y` target explicitly single-run so the Playwright browser accessibility path behaves consistently in local development and CI, and added repo-local Chrome executable resolution so the command truthfully validates the requested browser target.
- [x] Step 12 completed in progress: ran `npm run lint` (pass), `npm run test:run` (pass: 14 test files, 62 tests), `npm run test:a11y` (pass: 4 test files, 8 tests), and `npm run validate:i18n` (pass, including extraction drift check plus localized production build) from `UI/`.
