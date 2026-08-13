# PR 8 Review Resolution Plan

## 1. Overview

Resolve the PR 8 review comments that either are explicitly marked `Ready for implementation: true` or have an open-question decision that requires a concrete change. The work keeps the existing UI behaviour and accessibility contracts intact while making component tests assert user-facing contracts rather than CSS implementation details, removing unneeded test-hook API surface, and splitting the shared icon implementation into composable components.

## 2. Existing Plan Context

Source: [`PR-8-review-comments.md`](./PR-8-review-comments.md).

This plan includes all `Firm Statements / Needs Addressing` entries marked `Ready for implementation: true` and these decided actions from `Open Ended / Questions`:

- remove the unused `data-shell-footer` host attribute;
- remove the unused `data-shell-header` host attribute;
- remove the unused button `$testId`/`data-testid` forwarding contract.

Open-question decisions to retain the current implementation, provide an explanation only, or defer work are recorded as non-goals.

## 3. Scope

- Correct the root README heading typo.
- Simplify UI tests so they assert rendered content, semantics, routing outcomes, and public component configuration rather than class names, DOM ordering, or prototype/internal details.
- Add a direct TypeScript alias for the shared accessibility test helpers and migrate every current helper importer to it.
- Inline the fixed theme-control template values instead of storing them as component fields.
- Remove unused public-header and public-footer host test hooks, plus the button `data-testid` forwarding API, associated tests, and documentation.
- Replace the name-switched `sbi-icon` implementation with a base icon wrapper that projects SVG content and separate concrete icon components for sun, moon-stars, and builder artwork.
- Update icon consumers, tests, and design-system documentation to the new public API while preserving decorative-icon accessibility and existing visual output.

## 4. Non-Goals

- Do not remove `check-localized-ssr-assets.mjs` or `run-a11y-tests.mjs`; their decisions retain the current test wrappers.
- Do not redesign the not-found page, seek its required human design sign-off, or add an Open Source Software footer page/link; both are deferred follow-up work.
- Do not change `overflow-wrap: anywhere`, native button click handling, or the `panelMedia` slot; the review decisions require explanation/retention rather than code changes.
- Do not change or remove `app.config.spec.ts`; its intended level of framework-wiring coverage remains undecided.
- Do not change route loading in `app.routes.ts`; the lazy-loading comment is not ready for implementation.
- Do not alter user-facing copy, routes, styling intent, or localisation keys as part of the test refactors.

## 5. Behaviour Scenarios

### Application and feature test coverage

- Given the application is navigated to home, under-construction, or an unmatched URL, when the app-level spec renders the route, then it verifies the expected route component/page is present inside the shared shell without duplicating the component-level copy and interaction assertions.
- Given the home route and hero render, when their specs run, then each spec groups assertions by intent and verifies only visible content and public semantic relationships: the hero is present, its heading labels the home article, introductory content and CTA are visible, and CTA activation navigates to `/under-construction`.
- Given not-found or under-construction renders, when their specs run, then they verify page heading, copy, labelled region, decorative icon semantics, and navigation behaviour without asserting styling class names or design-system internal markup.
- Given the public header renders, when its spec runs, then it verifies its visible brand and theme control configuration without relying on DOM sibling order or component prototypes.
- Given the theme mode changes through click, keyboard focus, persisted state, or system-preference changes, when theme-control tests run, then they preserve the existing mode, ARIA, persistence, and focus assertions but stop asserting implementation-specific CSS class names.

### Component contracts

- Given a consumer uses `sbi-button`, when it supplies supported disabled and ARIA inputs, then the native button retains its documented accessibility and click-output behaviour; no `$testId` input or `data-testid` forwarding is available.
- Given a consumer renders a concrete icon component, when it provides no accessible label, then the projected SVG is displayed and the base icon remains hidden from assistive technology; when it provides a label, then it exposes the same `role="img"` and accessible-name contract as today.
- Given the theme control and under-construction page render after the icon split, when their concrete sun, moon-stars, and builder icons are used, then they retain their current SVG artwork, CSS hooks, visual rendering, and decorative semantics.

## 6. Deliverables

1. **Repository documentation correction**
   - Change `Summer-born Infor` to `Summer-born Info` in `README.md`.

2. **Accessibility helper import boundary**
   - Add an exact `@a11y-test-helpers` path mapping in `UI/tsconfig.json` to `./src/testing/a11y/a11y-test-helpers`, alongside the existing `@design-system/*` alias. Do not introduce a broad `@testing/*` alias.
   - Replace relative helper imports with `@a11y-test-helpers` in every current importer: `a11y-browser.setup.ts` and the component accessibility specs for home, not-found, under-construction, root shell, and theme control.
   - Search `UI/` for `a11y-test-helpers` after the migration to confirm all imports use the direct alias and no relative helper imports remain.

3. **Test-contract refactor**
   - Reduce `UI/src/app/app.spec.ts` to app creation and high-level shell-plus-route registration assertions; retain one focused assertion per relevant route instead of repeating feature copy, controls, and component behaviour.
   - Reorganise `home.spec.ts` and `home-hero.spec.ts` into intent-focused scenarios. Remove layout-class and internal-wrapper assertions; retain visible hero content, heading/region relationship, CTA, image alternative text, and route navigation coverage in the most appropriate spec.
   - Remove style-class and design-system-internal assertions from `not-found.spec.ts` and `under-construction.spec.ts`, retaining their rendered page contracts, accessibility metadata relationship, decorative-icon outcome, and back/home navigation cases.
   - Simplify `public-header.spec.ts` to public configuration and rendered output assertions, removing prototype and brittle element-order checks.
   - Preserve theme-control behavioural coverage while removing class-name assertions from `theme-control.spec.ts`.

4. **Theme-control simplification**
   - In `UI/src/app/shell/theme-control/theme-control.ts` and its template, replace `sunIconName`, `moonStarsIconName`, `variant`, and `layout` fields with their literal, type-safe template values (or the new concrete icon components where applicable).
   - Retain signal-backed mode state and the `$localize` label because they represent behaviour and localization, not fixed presentation values.

5. **Remove unused test hooks**
   - Remove `componentId` and the `data-shell-header`/`data-shell-footer` host bindings from the public header and footer components; update any tests that mention them.
   - Remove `$testId` from `Button`, the `[attr.data-testid]` binding from `button.html`, the test-host state/binding and forwarding assertion from `button.spec.ts`, and the `$testId` contract entry from `UI/src/design-system/button/README.md`.
   - Confirm no production or test consumer remains through a repository search before completing the removal.

6. **Composable icon design-system API**
   - Refactor `UI/src/design-system/icons/` so the base `sbi-icon` component owns shared host styling and `$label`-driven accessibility semantics, and renders projected content with `ng-content` instead of selecting artwork by `$name`.
   - Add separate standalone components for the sun, moon-stars, and builder SVGs. Each concrete component imports the base icon and projects its own unchanged SVG path markup into it; expose clear, selector-specific public components such as `sbi-sun-icon`, `sbi-moon-stars-icon`, and `sbi-builder-icon`.
   - Move the existing SVG selection test into focused base-icon accessibility tests and concrete-icon rendering tests. Do not treat a concrete SVG's internal DOM shape as a consumer contract beyond confirming the expected artwork renders.
   - Update `icons/index.ts`, `icons/README.md`, `UI/src/design-system/README.md`, theme-control, under-construction, and their affected tests to import and render the concrete components rather than pass `IconName` values.
   - Retain current consumer CSS classes on the concrete icon element or an equivalent styled host so existing scoped styles continue to apply without selector changes.

## 7. Technology Requirements and Decisions

- Use the existing Angular standalone-component, `input()`, host-metadata, native control-flow, Vitest, and TestBed patterns. No new dependency is required.
- The new `@a11y-test-helpers` alias is a compile-time import convenience only; it maps directly to the existing helper module and does not change runtime bundling.
- `sbi-icon` becomes a reusable base wrapper with content projection. Specific artwork becomes separate components, removing the expanding `IconName` union and `@switch` template while keeping the public accessibility API (`$label`) on the base wrapper.
- Preserve inline SVG and `currentColor` styling. The review request explicitly favours projected SVG content, so external asset delivery or a third-party icon library is out of scope.
- Tests must follow the UI guide: assert visible behaviour, semantics, and state rather than CSS classes or private implementation details.

## 8. Dependencies and Sequencing

1. Correct the README and add the direct `@a11y-test-helpers` alias; migrate every helper import and run the focused test/type check to validate resolution.
2. Remove header/footer and button test hooks, updating their tests and documentation before broader test cleanup so no stale API contract remains.
3. Introduce the base and concrete icon components, update the public barrel/documentation, then migrate theme-control and under-construction consumers and their tests. This is the only structural change and should complete before the theme-control simplification.
4. Refactor app, feature, header, and theme-control tests around their public contracts after the affected component APIs are stable.
5. Run formatting, linting, unit tests, accessibility tests, and localization validation; inspect the generated localisation diff to confirm the refactor did not change source messages.

## 9. Risks and Mitigations

- **Icon migration can silently change accessibility or CSS targeting.** Preserve the base host ARIA conditions and consumer classes; run the existing accessibility suite and inspect focused theme/placeholder tests.
- **Test simplification can remove meaningful route or semantic coverage.** Keep explicit scenario-level coverage for shell composition, region-to-heading relationships, CTA/back navigation, and theme state changes before deleting class/internal assertions.
- **The new alias may compile differently in app and spec builds.** Place the exact mapping in the shared `tsconfig.json`, retain the current `tsconfig.spec.json` include rules, and validate every migrated accessibility file with the normal test command.
- **Removing unused hooks may miss a consumer.** Search all UI source, tests, documentation, and build scripts for `$testId`, `data-testid`, `data-shell-header`, and `data-shell-footer` before finalising.

## 10. Unknowns and Required Clarifications

None block this delivery. The source document records explicit decisions for every item in scope.

The following comments remain intentionally deferred and require separate decisions or design work: app configuration test policy, route lazy-loading scope, not-found design review, and an Open Source Software footer page/link.

## 11. Completion Checklist

- [x] `README.md` header typo is corrected.
- [x] `@a11y-test-helpers` is configured and every current accessibility-test/setup importer uses it.
- [ ] App, home, hero, not-found, under-construction, public-header, and theme-control tests assert public behaviour/semantics without the flagged class, DOM-order, or prototype assertions.
- [ ] Theme-control fixed values are inline and no longer stored as fields.
- [x] Header/footer host test hooks and all button `$testId` API, bindings, tests, and documentation are removed with no remaining consumers.
- [x] `sbi-icon` is a content-projecting base component and separate sun, moon-stars, and builder components provide the existing inline SVG artwork.
- [x] All icon consumers, exports, documentation, and tests use the new concrete icon API and retain current accessibility behaviour.
- [ ] `npm run format`, `npm run lint`, `npm run test:run`, and `npm run test:a11y` pass from `UI/`.
- [ ] `npm run validate:i18n` passes from `UI/`, including no unintended `messages.xlf` drift.
- [ ] No open-question item is changed unless its decision explicitly requires the included removal work.
