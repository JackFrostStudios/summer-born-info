# Design System Review Remediation Plan

## 1. Overview

Resolve the issues captured in `UI/review-findings/design-system-expert.md` using the current `UI/` codebase as the source of truth for status. This plan keeps every reviewed issue visible for tracking, including items that are already partly addressed, and turns the remaining gaps into implementation-ready work.

This plan also captures three explicit product decisions from the request:

- Button styles should not be a consumer-overridable API. Remove consumer-facing button custom properties where practical, or clearly treat any remaining ones as internal implementation details.
- The repeated raised panel pattern should be considered for a reusable shared component in `UI/src/design-system/`.
- Form components are intentionally out of scope until a real feature needs them, so issue 6 should be resolved through minimum cleanup and documentation rather than by building a form system now.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-05 to create a remediation plan for `UI/review-findings/design-system-expert.md`.
- Review source: `UI/review-findings/design-system-expert.md`.
- Verification source: a dedicated codebase-check sub-agent plus local inspection verified each issue against the current repository state on 2026-07-05 before this plan was drafted.
- Relevant repository guidance:
  - `Plans/AGENTS.md`
  - `UI/AGENTS.md`
  - `AI_PROJECT_GUIDE.md`
  - `UI/AI_PROJECT_GUIDE.md`
  - `UI/src/styles/README.md`

### Verified Review Status Snapshot

1. Issue 1. Component-level design tokens are undocumented and informal: `Open`
2. Issue 2. `sbi-button` uses `buttonType` for visual semantics: `Open`
3. Issue 3. Reusable panel/surface patterns are reimplemented in feature styles: `Open`
4. Issue 4. Surface-token naming lacks a decision model: `Partially Resolved`
5. Issue 5. Typography governance allows undocumented local exceptions: `Partially Resolved`
6. Issue 6. Global base form styling exists before form primitives/components: `Open`
7. Issue 7. Interaction-state tokens are not first-class yet: `Partially Resolved`
8. Issue 8. There is no design-system component index or barrel: `Open`
9. Issue 9. British `colour` naming is mixed with CSS `color` terminology: `Open`
10. Issue 10. Visual regression coverage is not represented in the UI workflow: `Partially Resolved`

### Current-State Notes That Shape The Plan

- No reviewed issue is fully resolved yet as of 2026-07-05, so none are marked `Resolved` in the current snapshot.
- The button custom-property surface is already being consumed in feature code, especially in `UI/src/app/shell/theme-control/theme-control.scss`, which means issue 1 is now a live API-governance problem rather than a theoretical documentation gap.
- The repeated raised-panel recipe now exists in at least `under-construction` and `not-found`, which justifies a shared abstraction under the current UI guidance because there are already multiple consumers.
- Some documentation has improved since the original review, which is why issues 4, 5, 7, and 10 are `Partially Resolved`, but the missing decision models and governance rules are still meaningful gaps.

## 3. Scope

- Resolve the still-open and partially resolved design-system review findings in `UI/`.
- Preserve every review item in the plan with an explicit tracked status.
- Introduce only the minimum new abstractions justified by current repeated usage.
- Prefer documentation and API hardening where that fully addresses the issue, especially for low-priority governance gaps.
- Add or update UI tests where component APIs or shared rendering behavior change.
- Run the normal UI validation workflow for any implementation slices that change component behavior, templates, styles, or shared documentation links.

## 4. Non-Goals

- Building a full form component library before an actual feature requires it.
- Adding Storybook or a dedicated screenshot regression platform immediately.
- Reworking the whole token system or renaming broad CSS token families without a narrow need.
- Converting every bespoke visual treatment into a shared primitive if it still has only one legitimate consumer.
- Introducing breaking UI API changes without a clear migration path for existing consumers.

## 5. Behaviour Scenarios

### Scenario: A developer needs to use the shared button correctly

Given a developer reaches for `sbi-button`, when they look for usage guidance, then they should find a discoverable design-system index plus button-specific documentation covering visual variants, accessibility expectations, and which styling hooks are supported versus internal only.

### Scenario: A feature needs an icon-sized theme toggle button

Given a feature needs a compact icon-only button such as the theme toggle, when it uses the shared button, then it should rely on an explicit supported API or sanctioned composition pattern instead of overriding private `--sbi-button-*` custom properties from feature styles.

### Scenario: A future form feature introduces submit/reset semantics

Given the shared button is used near real forms later, when a developer reads or uses the API, then the visual variant concept should be clearly separated from the native button `type` concept so submit/reset behavior can be added without semantic confusion.

### Scenario: A developer builds a new page-level status or placeholder panel

Given a new page needs the same raised gradient panel treatment already used by `under-construction` and `not-found`, when the developer implements it, then they should reuse a documented shared panel component or primitive instead of copying border, radius, gradient, shadow, and layout rules into another feature stylesheet.

### Scenario: A developer chooses a neutral surface token

Given a developer is styling shell chrome, a page panel, or an inset region, when they consult the shared styling guidance, then they should see a surface-stack decision model that explains which token to choose and why.

### Scenario: A developer introduces a typography exception

Given a component needs brand or display typography that departs from the default scale, when the developer implements it, then the exception policy should explain whether to use a named token, a documented exception, or feature-local styling.

### Scenario: A feature adds plain HTML form controls before a form component system exists

Given the application still has no shared field components, when a feature introduces `input`, `select`, or `textarea`, then base styles should behave as reset-level defaults and the documentation should make clear that labels, hint text, errors, validation, and spacing are not yet part of a shared form API.

### Scenario: A second interactive design-system component is introduced

Given another shared interactive component is added later, when it needs hover, active, disabled, focus-visible, selected, or motion behavior, then the design system should already have a documented state model that prevents each component from inventing different interaction rules.

### Scenario: A contributor searches for theme naming conventions

Given a contributor encounters both `color` tokens and `data-sbi-colour-mode`, when they consult the docs, then they should quickly learn that CSS/token naming follows CSS terminology while app-facing naming follows the chosen product-language convention.

### Scenario: A contributor evaluates UI quality tooling

Given a contributor wants to know whether visual regression tooling exists, when they review the design-system guidance, then they should see the current testing posture, understand that accessibility/browser checks exist today, and know the trigger for adding visual regression tooling later.

## 6. Deliverables

1. Add `UI/src/design-system/README.md` as the design-system entry point covering ownership rules, maturity expectations, import conventions, accessibility expectations, and documentation requirements for shared components.
2. Add `UI/src/design-system/button/README.md` documenting `sbi-button` inputs, outputs, variant guidance, accessibility expectations, disabled behavior, and the styling contract.
3. Reclassify button styling hooks so consumer overrides are no longer the default path:
   - Remove unnecessary consumer-facing `--sbi-button-*` custom properties where they are only supporting current internal implementation.
   - Where a custom property must remain, document it as internal-only unless there is a deliberate supported public use case.
4. Replace the current theme-toggle styling dependency on private button custom properties with a supported shared-button composition approach.
5. Decide and implement the supported compact-button approach for current consumers:
   - Prefer an explicit shared-button API or sanctioned modifier for size/icon-only layout.
   - Avoid keeping raw feature-level overrides as the long-term contract.
6. Rename the visual button API from `buttonType` to `variant` and update consuming components, tests, and documentation.
7. Decide whether to add a separate native `type` input now or document it as the next planned addition when a form feature requires it.
8. Add a reusable shared panel abstraction in `UI/src/design-system/` for the raised gradient panel pattern already shared by `under-construction` and `not-found`.
9. Refactor `under-construction` and `not-found` to consume the shared panel abstraction instead of repeating the full panel recipe in feature styles.
10. Reassess whether the homepage hero art should consume the new shared panel abstraction or remain feature-specific, and document that decision explicitly.
11. Extend `UI/src/styles/README.md` with a surface-stack decision model that maps current tokens to typical usage such as page canvas, shell, section band, card/panel, inset container, and strongest neutral separation.
12. Extend `UI/src/styles/README.md` with a typography exception policy covering when local `clamp()` sizing or custom letter spacing is acceptable and when a named token should be introduced.
13. Resolve issue 6 with minimum scope only:
   - Keep base form styles at reset/default level.
   - Update docs to state that the app does not yet provide shared field primitives or form components.
   - Tighten any obviously misleading wording or styling that implies a full form system exists.
14. Add a lightweight documented interaction-state model covering at least default, hover, active, focus-visible, disabled, and selected/current states.
15. Apply only the minimum interaction-token work justified today:
   - Prefer documenting the shared state model first.
   - Only promote motion/state values into shared tokens if the new panel work or button hardening reveals a second real consumer.
16. Add naming-convention guidance that explains why CSS tokens use `color` while app-facing theme mode uses `colour`, and clarify that the mixed spelling is intentional unless the project later chooses to standardize it.
17. Document the current UI quality-tooling posture:
   - Note that accessibility and browser-level testing already exist.
   - Record that dedicated visual regression tooling is intentionally deferred.
   - Define the trigger for revisiting that decision, such as when multiple stable shared components or theme-variant regressions create meaningful risk.
18. Update any affected UI architecture guidance if the new shared panel component changes canonical ownership or shared-component expectations.
19. Run `npm run format`, `npm run lint`, `npm run test:run`, and `npm run validate:i18n` from `UI/` for implementation slices that change code or user-facing/shared documentation references. Run `npm run extract:i18n` if template text changes.

## 7. Technology Requirements and Decisions

- Design-system documentation:
  Component-level docs should live beside the shared component they describe, with an index at `UI/src/design-system/README.md` so global style docs and component docs form one discoverable system.
- Button styling contract:
  Treat button visuals as owned by the component, not by downstream CSS overrides. Internal custom properties are acceptable as implementation detail, but they should not be presented or relied on as a supported consumer API.
- Button API naming:
  `variant` is the intended public design-system term for visual style; native button semantics should remain distinct from that concept.
- Shared panel abstraction:
  A shared panel component is justified because at least two current features repeat the same raised-panel structure and styling. This should live in `UI/src/design-system/` rather than as another global primitive because it combines structure and styling, not just a raw utility class.
- Forms posture:
  Do not introduce form components, validation primitives, or a field API in this plan. Resolve the current issue by clarifying the baseline and keeping global form styling appropriately modest.
- Interaction-state governance:
  Do not create a large token taxonomy pre-emptively. Document the state model first, and only promote state values into reusable tokens when a second consumer clearly benefits.
- Visual regression posture:
  Do not add Storybook or screenshot infrastructure in this remediation plan. Resolve the review finding by documenting the current coverage and the threshold for future investment.

## 8. Dependencies and Sequencing

1. Start with documentation and contract decisions for the button because they determine whether current CSS-variable consumers must be removed, replaced, or formalized.
2. Implement the shared-button API cleanup and consumer migration before broadening design-system docs further, so the docs describe the actual supported contract.
3. Introduce the shared panel abstraction once the design-system entry docs exist, because it becomes the second major documented component pattern.
4. Refactor repeated feature panels to the shared abstraction and explicitly decide whether the homepage hero is part of that pattern or a documented exception.
5. Update the shared styling README with the surface-stack and typography-exception guidance after the component abstractions are clearer.
6. Resolve the low-priority governance items with targeted documentation:
   - base-form posture,
   - interaction-state model,
   - `colour` versus `color` naming convention,
   - current testing posture and visual-regression deferral trigger.
7. Finish each implementation slice with formatting, linting, tests, and i18n validation appropriate to the touched files.

### Suggested Implementation Slices

1. Design-system documentation foundation:
   - add `UI/src/design-system/README.md`
   - add `UI/src/design-system/button/README.md`
   - define supported versus internal styling contracts
2. Shared button API hardening:
   - migrate `buttonType` to `variant`
   - remove or internalize consumer CSS-variable overrides
   - replace theme-control customization with a sanctioned pattern
3. Shared panel component:
   - introduce reusable panel component
   - refactor `under-construction` and `not-found`
   - decide whether hero treatment stays custom
4. Shared style governance updates:
   - surface-stack guidance
   - typography exception policy
   - minimum form-baseline documentation
   - interaction-state model
   - spelling convention note
   - current visual-quality posture note

### Implementation Progress

- [x] Slice 1 complete on 2026-07-05: added `UI/src/design-system/README.md` and `UI/src/design-system/button/README.md` to document design-system ownership, the current `sbi-button` contract, and the supported-versus-internal styling boundary.
- [x] Slice 1 validation: `npx prettier --write src/design-system/README.md src/design-system/button/README.md` from `UI/`.
- [ ] Slice 2 pending: shared button API hardening.
- [ ] Slice 3 pending: shared panel component.
- [ ] Slice 4 pending: shared style governance updates.

## 9. Risks and Mitigations

- Risk: Removing button CSS custom-property overrides could break current layouts such as the theme toggle.
  - Mitigation: replace those overrides with an explicit supported API or composition pattern in the same implementation slice.
- Risk: Renaming `buttonType` to `variant` could create a breaking change for consumers and tests.
  - Mitigation: update all in-repo consumers together and consider a short-lived compatibility alias only if implementation needs to land in multiple slices.
- Risk: A new shared panel component could become too specific to placeholder pages.
  - Mitigation: design it around the repeated shell/panel responsibilities that already exist, and keep feature-specific content or artwork outside the shared API.
- Risk: Over-documenting low-priority findings could create noise without real value.
  - Mitigation: keep issue 6, issue 9, and issue 10 fixes narrow and explicitly tied to current project constraints.
- Risk: Converting too much into tokens too early could ossify weak abstractions.
  - Mitigation: document the interaction model first and only promote shared tokens when multiple consumers require them.
- Risk: The homepage hero may look similar to the shared panel but still be intentionally bespoke.
  - Mitigation: require an explicit keep-custom versus reuse decision in the implementation record instead of forcing consistency for its own sake.

## 10. Unknowns and Required Clarifications

- No blocking clarification is required to draft this plan.
- During implementation, the main decision to confirm in code is the supported replacement for current theme-toggle button overrides:
  - a compact/shared-button size variant,
  - an icon-only treatment,
  - or a narrowly scoped internal host-context pattern.
- If implementation shows that a reusable panel component would be awkward for the current placeholder pages, the fallback is a documented shared modifier or primitive. The implementation should record that choice explicitly if it departs from the preferred component direction.

## 11. Completion Checklist

- [ ] Issue 1 is resolved by documented component-level design-system guidance and a clear public-versus-internal button styling contract.
- [ ] Button styling is no longer treated as a consumer-overridable API by default.
- [ ] Any remaining `--sbi-button-*` properties are either removed or explicitly documented as internal-only.
- [ ] Issue 2 is resolved by renaming the visual button API from `buttonType` to `variant`.
- [ ] The implementation records whether a separate native button `type` input was added now or intentionally deferred until real form usage.
- [ ] Issue 3 is resolved by introducing a shared reusable panel abstraction and removing duplicated panel recipes from repeated feature styles.
- [ ] The implementation records whether the homepage hero is part of that reusable panel family or an intentional exception.
- [ ] Issue 4 is resolved by adding a surface-stack decision model to `UI/src/styles/README.md`.
- [ ] Issue 5 is resolved by documenting the typography exception policy and either formalizing or explicitly documenting the current brand exception approach.
- [ ] Issue 6 is resolved with minimum scope only: base form styles are documented as defaults/reset-level, and no premature form component system is introduced.
- [ ] Issue 7 is resolved by documenting a shared interaction-state model, with new tokens added only if current implementation reveals a justified second consumer.
- [ ] Issue 8 is resolved by adding a discoverable design-system index and clear import/documentation conventions.
- [ ] Issue 9 is resolved by documenting the `colour` versus `color` naming convention or intentionally standardizing it if the implementation chooses to do so.
- [ ] Issue 10 is resolved by documenting the current UI quality-tooling posture and the trigger for future visual-regression tooling, without adding heavy tooling prematurely.
- [ ] Any affected shared-component tests are updated and passing.
- [ ] `npm run format` has been run in `UI/` for implementation slices that edit UI files.
- [ ] `npm run lint` has been run in `UI/` for implementation slices that edit UI files.
- [ ] `npm run test:run` has been run in `UI/` for implementation slices that change component or shared behavior.
- [ ] `npm run extract:i18n` has been run in `UI/` if template text changed.
- [ ] `npm run validate:i18n` has been run in `UI/` when localization-relevant output changed.
