# UI Test Review Remediation Plan

## 1. Overview

Resolve the findings captured in `UI/review-findings/test-expert.md` using the current `UI/` codebase as the source of truth. Keep every reviewed finding visible for tracking, including items that are already fixed, and align the implementation with the user clarification that historical "this old thing is not implemented" assertions should be removed rather than preserved.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-07 to create a remediation plan for `UI/review-findings/test-expert.md`.
- Review source: `UI/review-findings/test-expert.md`.
- Verification source: the current repository state was inspected directly in `UI/src/**`, and a sub-agent was launched to independently verify the review findings against the current codebase before planning.
- Relevant repository guidance:
  - `Plans/AGENTS.md`
  - `UI/AGENTS.md`
  - `AI_PROJECT_GUIDE.md`
  - `UI/AI_PROJECT_GUIDE.md`

### Verified Review Status Snapshot

1. Missing `ThemeControlService` resilience coverage for malformed persisted values, storage failures, and `system` reset behavior - `Open`
2. Missing server/platform fallback coverage for browser-only theme logic - `Open`
3. Missing listener-cleanup assertion for `ThemeControlService` media-query subscription teardown - `Open`
4. Unknown-route behavior coverage - `Resolved`
5. Brittle structure/class assertions in shell and homepage specs - `Open`
6. `Home` and `HomeHero` spec duplication - `Open`
7. Historical negative-copy assertions in homepage tests - `Open`
8. Limited role/name-oriented query coverage in key specs - `Open`
9. Missing `Button` coverage for null ARIA inputs being removed - `Open`
10. Missing `Button` protection around unsupported variant usage - `Open`
11. Footer-link test expectations do not yet document accessible naming and link-safety intent - `Needs Decision`
12. Rendered `ThemeControl` response to system-preference changes - `Resolved`
13. `UnderConstruction` `document.defaultView === null` concern from the original review is obsolete in the current implementation - `Resolved as obsolete`
14. Theme test setup duplication across service and component specs - `Open`

## 3. Scope

- Fix the still-open UI testing review findings in `UI/`.
- Preserve already-resolved findings in the plan so their status stays traceable.
- Refocus tests on current supported behaviour and accessibility semantics rather than historical implementation cleanup.
- Remove negative assertions that merely prove old prototype copy or features are absent unless a concrete, durable regression risk is intentionally retained and documented.
- Run the normal UI validation commands after implementation.

## 4. Non-Goals

- Reworking product copy, route content, or shell layout beyond what is required to improve test coverage and assertion quality.
- Introducing a wholesale test-framework migration unless a very small helper is enough to unlock role/name-oriented assertions.
- Preserving long-term regression tests whose only purpose is to prove that previously removed prototype content is still absent.
- Changing production behaviour solely to make tests easier, except where a clear product or accessibility contract is missing and needs to be made explicit.

## 5. Behaviour Scenarios

### Scenario: Theme service starts with malformed persisted state

Given `localStorage` contains an invalid colour mode such as `sepia`, when `ThemeControlService` is created, then it should fall back to `system`, clear the invalid stored value, and avoid throwing.

### Scenario: Theme service interacts with unavailable browser storage

Given browser storage APIs throw on read, write, or remove, when `ThemeControlService` is created or `setMode()` is called, then the service should keep functioning without surfacing an exception and should still apply the correct effective mode to the document.

### Scenario: Theme service runs on a non-browser platform

Given the service is created with `PLATFORM_ID` set to `server`, when theme state is read or changed, then browser-only APIs should not be touched, the effective mode should fall back predictably, and explicit mode updates should still affect the document state that the service owns.

### Scenario: Theme service is destroyed while following system mode

Given the service has subscribed to system colour-scheme changes, when the injector is destroyed, then the media-query listener should be removed and later system changes should no longer update the destroyed service instance.

### Scenario: Homepage composition tests focus on ownership boundaries

Given `Home` composes `HomeHero`, when homepage tests run, then `HomeHero` should own detailed hero copy, image, and CTA behaviour assertions, while `Home` should verify page-level composition, landmark semantics, and route accessibility wiring only.

### Scenario: A user-facing semantic regression is introduced

Given a heading, landmark, button name, or other accessible contract changes accidentally, when the relevant UI tests run, then the tests should fail based on missing semantics or accessible names rather than implementation classes or child counts alone.

### Scenario: The not-found route is visited

Given a visitor navigates to an unknown URL, when the router resolves the route, then the shared shell should render the not-found experience. This is already covered and should remain covered as a resolved tracked item.

## 6. Deliverables

1. Extend `ThemeControlService` tests to cover invalid persisted values being discarded and falling back to `system`.
2. Extend `ThemeControlService` tests to cover `localStorage.getItem`, `setItem`, and `removeItem` throwing without breaking service creation or mode changes.
3. Add a focused assertion that `setMode('system')` clears the root attribute and persisted override after an explicit mode had been set.
4. Add a server-platform `ThemeControlService` test configuration that provides `PLATFORM_ID` as `server` and verifies browser guards plus predictable fallback behaviour.
5. Add teardown coverage proving the media-query listener is removed on injector destruction and no longer drives state updates after disposal.
6. Keep the existing resolved `ThemeControl` rendered-system-change coverage visible in the plan without reopening it unnecessarily.
7. Reduce `Home` spec ownership to composition, article/heading labelling, route accessibility metadata, and intentional page structure.
8. Keep detailed homepage hero copy, image, and CTA behaviour assertions in `HomeHero` tests only.
9. Remove historical negative-copy assertions from `UI/src/app/features/home/home.spec.ts` that only verify old prototype claims or calls to action are absent.
10. Replace brittle structural assertions with more user-facing checks in key shell and route specs, especially:
11. `PublicHeader`: prefer banner/brand/theme-control semantics over `header.children.length`.
12. `Home` and `HomeHero`: prefer heading/button accessible-name assertions over shared-button class assertions where the class itself is not the contract under test.
13. `RootShell`: prefer shell landmark/content composition assertions over component-selector or layout-class dependency where practical.
14. Add small local DOM helpers if needed so semantic assertions stay readable without requiring a broad test-stack rewrite.
15. Extend `Button` tests to prove null ARIA inputs remove forwarded attributes, not just blank-string `aria-labelledby`.
16. Decide and document how unsupported button variants are protected:
17. Either add a compile-time test or another focused safeguard proving unsupported variants are not silently accepted through host templates.
18. Strengthen footer-link tests to document accessible naming and intended external-link behaviour once the current same-tab versus hardened external-link contract is confirmed.
19. Factor duplicated `localStorage` and `matchMedia` stubs into a small shared theme-test helper if the added resilience cases would otherwise increase duplication further.
20. Preserve the already-resolved unknown-route coverage as an explicit resolved item rather than silently dropping it from tracking.
21. Preserve the obsolete `UnderConstruction` review item in the implementation record as resolved by code evolution rather than by adding irrelevant new tests.
22. Run `npm run format`, `npm run lint`, and `npm run test:run` in `UI/`.

## 7. Technology Requirements and Decisions

- Test scope:
  Tests should assert supported current behaviour, semantics, and accessibility contracts. They should not carry historical cleanup assertions by default.
- Assertion style:
  Prefer visible behaviour, landmark semantics, heading levels/names, and button accessible names over CSS classes and DOM child counts unless the class is itself the contract being tested.
- Test helpers:
  Start with small local helpers around semantic queries before introducing a new testing library. If implementation finds that native DOM APIs make accessible-name assertions too awkward or too weak, pause and explicitly discuss whether `@testing-library/angular` is worth adding.
- Theme service coverage:
  Browser guard, malformed storage, and teardown cases should be proven through tests rather than assumed from implementation shape.
- Footer-link intent:
  The implementation should either document current same-tab behaviour through clear test naming or tighten the product contract if the link should become a safer external-link pattern. This is the only tracked item that may need a small product decision rather than pure test work.
- Theme test helpers:
  The current `localStorage` and `matchMedia` stubs are duplicated between theme specs. If the new resilience cases make that duplication worse, extract a tiny shared helper near the theme tests instead of copying more setup code.

## 8. Dependencies and Sequencing

1. Start with `ThemeControlService` resilience and teardown coverage because it is the highest-signal open testing gap and largely isolated from the rest of the suite.
2. Add the focused server-platform coverage while the theme test helpers are already in hand.
3. Refactor `Home` and `HomeHero` tests together so ownership boundaries and removed negative assertions land in one coherent slice.
4. Tighten assertion style in shell and route-adjacent tests once the homepage test scope is clean, reusing any local semantic helpers added in the previous step.
5. Fill the smaller remaining gaps in `Button` and theme-test setup, then handle footer-link intent once the expected link contract is confirmed.
6. Finish with formatting, linting, and the full UI test run.

### Execution Progress

- [x] Theme-control test remediation completed in `UI/src/app/shell/theme-control/`, including malformed storage handling, storage failure resilience, explicit `system` reset coverage, server-platform guards, listener teardown assertions, and shared stub extraction.
- [ ] Homepage ownership and historical negative-assertion cleanup is still pending.
- [ ] Shell semantics, button gaps, and footer-link contract coverage are still pending.
- [ ] Final cross-suite validation and peer review are still pending.

## 9. Risks and Mitigations

- Risk: Test refactors can accidentally reduce coverage while making assertions look cleaner.
  - Mitigation: keep or improve coverage of actual user-facing behaviour while removing only redundant or historical assertions.
- Risk: Theme-service server fallback expectations may be ambiguous if the implementation mixes DOM access and browser API guards.
  - Mitigation: write the expected behaviour down in the new tests first and adjust only if a concrete framework constraint appears.
- Risk: Replacing class-based assertions too aggressively could erase intentional design-system contract coverage.
  - Mitigation: keep class assertions only where styling class mapping is the product of the unit under test, such as focused design-system component variants.
- Risk: Footer-link expectations may expose a small product-policy ambiguity around same-tab versus hardened external-link behaviour.
  - Mitigation: treat that case as a scoped implementation checkpoint and document the chosen contract clearly in the test names and assertions.
- Risk: Adding more theme-service coverage without cleaning up setup duplication could make the specs harder to maintain.
  - Mitigation: extract a minimal shared helper only if the new cases materially increase repeated stub code.

## 10. Unknowns and Required Clarifications

- No blocking clarification is required to begin the remediation plan.
- If implementation shows that role/name-oriented assertions are too cumbersome with the current test stack, pause and align before adding a new testing dependency.
- Footer external-link behaviour may need a tiny product-contract clarification during implementation if the current markup does not clearly express whether same-tab navigation is intentional.
- No work should be planned for the old `UnderConstruction` `document.defaultView` review note unless that browser API reappears in the component, because the current code no longer uses that path.

## 11. Completion Checklist

- [x] Issue 1 open finding is resolved with direct tests for malformed persisted values, storage failures, and explicit `system` reset behaviour.
- [x] Issue 2 open finding is resolved with server-platform coverage for `ThemeControlService`.
- [x] Issue 3 open finding is resolved with listener teardown assertions tied to injector destruction.
- [x] Issue 4 remains resolved: unknown-route behaviour is covered through the current not-found route tests.
- [ ] Issue 5 open finding is resolved by replacing brittle structure/class assertions where semantics are the real contract.
- [ ] Issue 6 open finding is resolved by narrowing `Home` versus `HomeHero` test ownership.
- [ ] Issue 7 open finding is resolved by removing historical negative-copy assertions instead of preserving them.
- [ ] Issue 8 open finding is resolved by strengthening semantic and accessible-name assertions in key specs.
- [ ] Issue 9 open finding is resolved with null-ARIA attribute coverage in `Button` tests.
- [ ] Issue 10 open finding is resolved with focused unsupported-variant protection coverage or another documented safeguard.
- [ ] Issue 11 needs-decision finding is resolved with footer-link accessibility coverage plus a documented navigation-intent contract.
- [x] Issue 12 remains resolved: rendered `ThemeControl` updates on system-preference changes are already covered.
- [x] Issue 13 remains resolved as obsolete: `UnderConstruction` no longer uses the reviewed `document.defaultView` path.
- [x] Issue 14 open finding is resolved by extracting or otherwise reducing duplicated theme-test setup where the added coverage would make the duplication materially worse.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run test:run` has been run in `UI/`.
