# UI Test Expert Findings

Review scope: `C:\Projects\summer-born-info\UI` only.

Validated guidance read: root `AI_PROJECT_GUIDE.md`, `UI/AGENTS.md`, `UI/AI_PROJECT_GUIDE.md`, `UI/package.json`, and all first-party UI spec files under `UI/src`.

Observed command: `npm run test:run` from `UI/` passes on 2026-07-02 with 10 test files and 38 passing tests. The run writes the configured JSON and JUnit reports under `UI/test-results/`. It also emits Node experimental warnings about `localStorage` not being available unless `--localstorage-file` is provided.

## Strengths

- The suite is colocated with the components and services it covers, matching the UI project guidance.
- Tests generally render Angular components through `TestBed` rather than testing class methods in isolation.
- Routing composition has useful smoke coverage in `src/app/app.spec.ts` and `src/app/shell/root-shell/root-shell.spec.ts`.
- The design-system button tests cover projection, disabled state, ARIA forwarding, click output, and focusability in `src/design-system/button/button.spec.ts`.
- Theme behavior has meaningful state tests around persistence, system preference, and root document attributes in `src/app/shell/theme-control/theme-control.service.spec.ts`.
- The under-construction page tests cover both browser-history and no-history navigation paths in `src/app/features/under-construction/under-construction.spec.ts`.

## High-Impact Testing Gaps

### 1. No coverage for invalid persisted theme values or storage failures

`ThemeControlService` intentionally handles malformed storage and unavailable storage in `src/app/shell/theme-control/theme-control.service.ts:56`, `:88`, `:100`, and `:112`, but the specs only cover normal `getItem`, `setItem`, and `removeItem` behavior in `src/app/shell/theme-control/theme-control.service.spec.ts:112`, `:122`, `:132`, and `:136`.

Add tests for:

- A stored value such as `sepia` is removed and the service falls back to `system`.
- `localStorage.getItem`, `setItem`, and `removeItem` throwing does not throw from service creation or mode changes.
- `setMode('system')` removes the root attribute and clears storage after an explicit override.

### 2. No server/platform fallback coverage for browser-only theme logic

The service has browser guards through `PLATFORM_ID` and `matchMedia` checks in `src/app/shell/theme-control/theme-control.service.ts:20`, `:125`, and `:132`, but all tests install browser-like globals. This leaves SSR safety unproven even though the Angular app has SSR configuration.

Add a focused service spec that provides `PLATFORM_ID` as `server` and asserts:

- Creation does not access browser-only APIs.
- Effective mode falls back to `light`.
- `setMode('dark')` updates the document attribute without trying to use `localStorage`.

### 3. Theme listener cleanup is not asserted

`ThemeControlService` registers and unregisters a media query listener using `DestroyRef` in `src/app/shell/theme-control/theme-control.service.ts:144` and `:146`. The current matchMedia stub stores one listener and supports removal, but no test proves cleanup happens.

Add an assertion that destroying the injector or resetting TestBed calls `removeEventListener('change', listener)` and that a later `setMatches` does not update the destroyed service.

### 4. Route tests do not cover unknown URL behavior

`src/app/app.routes.ts` only defines `/` and `/under-construction`. `src/app/app.spec.ts:25` and `:45` cover those happy paths, but no test documents what happens for an unknown route. That matters because users can land on mistyped or future links.

Either add a route fallback in product code and test it, or explicitly add a test that confirms the current behavior until the route policy is decided.

## Weak or Brittle Assertions

### 1. Several specs assert CSS classes and child counts rather than user-facing roles

Examples:

- `src/app/shell/public-header/public-header.spec.ts:32` asserts `header.children.length`.
- `src/app/features/home/home.spec.ts:159` and `src/app/features/home/home-hero/home-hero.spec.ts:57` assert implementation classes on the shared button.
- `src/app/shell/root-shell/root-shell.spec.ts:36` through `:40` assert component selectors and layout classes.

Prefer accessible and semantic assertions where possible: one banner, one main landmark, one contentinfo landmark, a button with accessible name `Take the first step`, and a heading with the expected level/name. Keep class assertions only where a class is the actual contract being tested.

### 2. Homepage tests duplicate the hero component contract heavily

`src/app/features/home/home.spec.ts` repeats many of the same copy, image, button, and navigation assertions already covered by `src/app/features/home/home-hero/home-hero.spec.ts`. This makes harmless hero text changes touch two spec files and can obscure what `Home` itself owns.

Keep `HomeHero` as the detailed contract for hero content and behavior. Trim `Home` tests toward composition and page labelling: it renders the hero, owns the `article` landmark/labelling relationship, and does not add extra sections if that is still an intentional product constraint.

### 3. Negative copy assertions are historical and fragile

`src/app/features/home/home.spec.ts:117` through `:123` asserts that old prototype claims such as `success rate`, `book a call`, and `expert advocacy` are absent. These are valuable during a cleanup PR but become noisy long-term regression tests unless the phrases are a durable product risk.

Replace with a smaller assertion around the approved visible structure, or move historical exclusions into a single narrowly named test with a comment explaining the regression it prevents.

### 4. Query style hides accessibility regressions

Most specs use `querySelector` and `textContent` rather than role/name-oriented queries. For example, the CTA tests in `src/app/features/home/home-hero/home-hero.spec.ts:6` and `:84` locate a nested native button via selectors. This can pass even if the accessible name or role is accidentally broken.

Consider adding `@testing-library/angular` or small local helpers around DOM role queries if the project wants accessibility-oriented assertions without a full new testing style. At minimum, assert button accessible names and landmark/heading semantics explicitly in key shell and route tests.

## Missing Cases

- `Button` should be tested for null ARIA inputs removing attributes, not only forwarding populated values in `src/design-system/button/button.spec.ts:89` through `:92`.
- `Button` variant handling only covers `primary` and `secondary`; if the component remains a design-system primitive, add a compile-time or rendered-state test that unsupported variants are not silently accepted through host templates.
- `ThemeControl` component tests do not cover changing system preference while the component is rendered; service tests cover the signal update, but the rendered `aria-label` and `aria-pressed` update should be asserted once.
- `UnderConstruction` does not cover `document.defaultView` being `null`, even though the component uses optional chaining in `src/app/features/under-construction/under-construction.ts:20`.
- Footer link tests assert `href` but not external-link safety or accessible naming. If the attribution opens in the same tab intentionally, document that through the test name or product guidance.

## Test Setup Issues

- The Angular test target uses coverage thresholds and structured reporters in `UI/angular.json:97` through `:110`, which is good for CI, but `npm run test:run` still writes `UI/test-results/results.json` and `UI/test-results/junit.xml`. Keep these generated files out of review noise or reserve reporter output for CI if local churn becomes a problem.
- The localStorage and matchMedia stubs are duplicated almost exactly between `src/app/shell/theme-control/theme-control.spec.ts:11` and `src/app/shell/theme-control/theme-control.service.spec.ts:11`. Extract a tiny local test helper near the theme-control specs once a third browser-global case is added.
- Test output includes Node experimental warnings about missing `--localstorage-file`. The project stubs `globalThis.localStorage` in theme tests, but the warning can still distract reviewers. If it persists in CI, configure the test environment or add a setup file that establishes the needed browser globals consistently.

## Concrete Remediation Plan

1. Add the missing `ThemeControlService` resilience tests first: malformed stored values, storage exceptions, `system` mode clearing, SSR fallback, and listener cleanup.
2. Add one rendered `ThemeControl` test proving `aria-label` and `aria-pressed` update when the system preference changes.
3. Add route policy coverage for unknown URLs, after deciding whether the product should show the under-construction page, a dedicated not-found page, or Angular's current no-match behavior.
4. Reduce duplication between `Home` and `HomeHero` specs so each file tests its owning responsibility.
5. Introduce role/name-style assertions gradually on shell landmarks, headings, and buttons. This will catch accessibility regressions earlier than selector-only tests.
