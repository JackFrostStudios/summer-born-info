# Shared Button Component Plan

## 1. Overview

Create a reusable Angular button component under `UI/src/design-system/` so button behaviour and baseline styling are owned in one place instead of being duplicated in feature and shell templates. The component should support the existing shared button variants (`primary` and `secondary`), expose a click/tap output for consumers, and render projected template content so both text-only and icon-driven buttons can use the same abstraction.

## 2. Roadmap Source or Existing Plan Context

- Request source: create a shared button component that can replace the current button primitive usage in:
  - `UI/src/app/features/home/home-hero/`
  - `UI/src/app/shell/theme-control/theme-control.html`
- Relevant repository guidance:
  - [Plans/AGENTS.md](./AGENTS.md)
  - [UI/AGENTS.md](../UI/AGENTS.md)
  - [AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md)
  - [UI/AI_PROJECT_GUIDE.md](../UI/AI_PROJECT_GUIDE.md)
  - `UI/src/styles/README.md`
- Current implementation context:
  - `UI/src/design-system/` exists but does not yet contain shared Angular UI components.
  - `UI/src/styles/_primitives.scss` currently defines the shared `.sbi-button` and `.sbi-button--secondary` styling, but those primitives are temporary and should be removed once the reusable component becomes the single source of truth.
  - `home-hero.html` currently renders a text CTA using a native `<button>` with `.sbi-button`.
  - `theme-control.html` currently renders a native toggle button with custom projected icon markup, component-specific ARIA state, and custom interaction styling.
  - The current `theme-control` transition behaviour is not the target for reuse because it emphasises click-state animation rather than the hover-driven interaction pattern already established by the shared button primitive.

## 3. Scope

- Add a reusable standalone button component in `UI/src/design-system/` with colocated TypeScript, template, stylesheet, and tests.
- Support a `buttonType` input that maps to the shared visual variants `primary` and `secondary`.
- Support projected template content so consumers can pass plain text or richer markup such as animated icon content.
- Expose a component output for click activation so consumers can handle button presses through the shared component instead of binding directly to a native button.
- Preserve native button semantics, keyboard activation, and disabled-safe interaction expectations.
- Migrate the homepage hero CTA and the shell theme control to consume the new button component.
- Update tests and shared styling ownership as needed to reflect the new reusable component boundary.

## 4. Non-Goals

- Converting every existing or future button in the app during this slice.
- Creating a polymorphic button-or-link abstraction.
- Introducing loading states, icon slots with separate APIs, or a full compound button design system in the same change.
- Reworking the theme-control behaviour, colour-mode service logic, or animation concept beyond what is needed to fit the shared button shell.
- Redesigning homepage copy, CTA wording, or navigation behaviour.

## 5. Behaviour Scenarios

### Scenario: A standard text CTA uses the shared component

Given a feature needs a primary call to action with text content, when it renders the shared button with projected text and `buttonType="primary"`, then the component should render a native `<button type="button">` with the shared primary styling and emit its activation event when clicked or tapped.

### Scenario: A lower-priority action uses a variant style

Given a consumer needs an action that should not visually outrank the primary CTA, when it sets `buttonType="secondary"`, then the component should apply the matching shared variant styling without the consumer having to duplicate primitive class names.

### Scenario: A consumer provides custom button content

Given a consumer needs richer content than plain text, when it places custom template content inside the shared button component, then the component should project and render that content inside the native button while preserving button semantics and interaction behaviour.

### Scenario: Theme control keeps its accessible toggle experience

Given the theme control uses animated icon content and screen-reader-only text, when it migrates to the shared button component, then it should still expose the correct `aria-pressed` state, keep the projected viewport/reel/icon structure, and toggle colour mode from the button activation output without losing keyboard accessibility.

### Scenario: Shared interaction motion follows the button primitive

Given the design-system button is intended to replace direct primitive usage, when a consumer uses the shared component, then its transition and interaction behaviour should align with the existing primitive hover feedback rather than preserving the current theme-control click-driven transition treatment.

### Scenario: Consumers keep explicit ownership of specialised ARIA attributes

Given different buttons may need different accessibility metadata, when a consumer uses the shared button component, then it should still be able to provide attributes such as `aria-pressed`, `aria-label`, or test hooks without the shared component inventing feature-specific semantics.

### Scenario: Styling stays consistent across multiple consumers

Given the hero CTA and theme control both use the shared component, when shared button interaction styles evolve later, then both consumers should inherit those changes from the design-system component instead of diverging through copied native button markup.

## 6. Deliverables

1. Create a reusable button component under `UI/src/design-system/` with a stable selector such as `sbi-button`.
2. Implement a typed `buttonType` input with the supported values `primary` and `secondary`, and define the default variant explicitly.
3. Implement an activation output for native button presses. The implementation should document whether it re-emits the native click event or emits a simpler void-style signal, and tests should match that contract.
4. Use content projection so consumers can pass plain text or richer markup as the button body without special-case APIs for icons.
5. Preserve native `<button>` semantics internally, including an explicit `type="button"` default so form submission behaviour is not introduced accidentally.
6. Move the canonical button styling into the new shared component so it becomes the only source of truth for button presentation, while keeping tokens and truly global non-button primitives in `UI/src/styles/`.
7. Add support for forwarding essential attributes needed by current consumers, including at minimum disabled state and ARIA/test attributes required by the theme control tests.
8. Migrate `home-hero` to use the shared component for its CTA while preserving the current text, visual priority, and layout alignment.
9. Migrate `theme-control` to use the shared component while preserving:
   - `aria-pressed` behaviour,
   - screen-reader-only label rendering,
   - projected icon animation markup,
   - existing toggle behaviour, and
   - the visual footprint expected by its current SCSS and tests.
   Update its outer button-shell interaction styling so it follows the primitive-aligned hover transition behaviour instead of the current click-emphasised pattern.
10. Add or update automated tests for the shared component and both migrated consumers.
11. Run the standard UI validation commands for a reusable component and template migration change, including i18n extraction if message locations or strings change.

## 7. Technology Requirements and Decisions

- Placement:
  Put the new component in `UI/src/design-system/` because it is intended for cross-app reuse and already has at least two consumers that justify the abstraction.
- Angular API shape:
  Build it as a standalone Angular component using `input()` and `output()` APIs, matching current UI guidance.
- Content projection:
  Use Angular content projection (`<ng-content>`) for the button body so both simple text and animated icon markup are supported without extra structural inputs.
- Event contract:
  Prefer a semantic output such as `pressed` or `buttonClick` over asking each consumer to bind directly to an internal native button. The chosen name should be consistent with project conventions and easy to read in templates.
- Attribute forwarding:
  Because `theme-control` needs `aria-pressed` and may later need other host-level attributes, the plan should treat host-to-inner-button attribute forwarding as an explicit design point rather than assuming Angular will copy arbitrary attributes automatically. The implementation may use targeted inputs for required attributes or a deliberate forwarding pattern, but it should avoid a brittle one-off solution.
- Styling source of truth:
  Reuse the existing design tokens from `UI/src/styles/_tokens.scss`, but move the canonical button styling out of `UI/src/styles/_primitives.scss` and into the shared component implementation. Once consumers are migrated, remove the old button primitive classes so buttons have one source of truth.
- Interaction behaviour:
  Treat the existing shared button primitive as the source of truth for transition timing and hover feedback. Do not preserve the current `theme-control` outer-button behaviour where the main interaction animation emphasis happens on click.
- Accessibility:
  The shared component must preserve native keyboard behaviour, focus visibility, and disabled semantics. The theme-control migration must keep its toggle semantics intact instead of flattening everything into a generic icon button with no state.
- Testing approach:
  Add focused tests for the design-system button component and update consumer tests to assert visible behaviour through the new abstraction rather than internal implementation details.

## 8. Dependencies and Sequencing

1. [x] Inspect the existing shared button primitive styles and use them as the migration reference for the new component-owned styling.
2. [x] Create the new design-system button component files in `UI/src/design-system/`.
3. [x] Implement the core API:
   - typed variant input,
   - projected content,
   - explicit native button type,
   - activation output,
   - any required disabled/ARIA forwarding support.
4. [x] Move the existing primary and secondary button styling into the shared component and remove duplication with the old primitives.
5. [x] Add focused unit tests for the shared button component covering variant mapping, projected content, and output emission.
6. [x] Migrate `home-hero` to the shared button component and update its tests for the new rendered structure.
7. [x] Migrate `theme-control` to the shared button component and update its styles/tests to keep toggle semantics, animation markup, and accessibility intact while aligning the outer button transitions with the primitive hover behaviour.
8. [x] Delete the legacy button primitive rules from `UI/src/styles/_primitives.scss` once all current consumers in scope have been migrated to the component.
9. [x] If the change establishes `UI/src/design-system/` as a canonical home for shared Angular UI components, update `UI/AI_PROJECT_GUIDE.md` to document that ownership convention.
10. [x] Run `npm run format`, `npm run lint`, `npm run test:run`, and the relevant i18n validation commands from `UI/`.

## 9. Risks and Mitigations

- Risk: The shared component API is too narrow for `theme-control`, forcing awkward workarounds.
  - Mitigation: treat projected content and attribute forwarding as first-class requirements, not follow-up polish.
- Risk: The migration accidentally carries forward the wrong motion pattern from `theme-control`, leaving shared buttons inconsistent across the app.
  - Mitigation: explicitly treat primitive hover behaviour as the interaction baseline and update theme-control tests/styles to verify that contract.
- Risk: Moving from direct native buttons to a wrapped component breaks ARIA state or keyboard semantics.
  - Mitigation: keep a native `<button>` inside the component, test activation and accessibility attributes explicitly, and preserve `type="button"` as the default.
- Risk: Shared button CSS becomes duplicated between global primitives and component styles.
  - Mitigation: move button styling fully into the component and delete the old primitive rules in the same slice instead of leaving both paths active.
- Risk: Theme-control tests become brittle if they rely on the exact old CSS class attached to the native button.
  - Mitigation: update tests to assert the accessible behaviour and projected icon structure that matter, while only checking structural classes that remain part of the agreed contract.
- Risk: Refactoring projected template content changes i18n extraction locations or message IDs.
  - Mitigation: keep existing template `i18n` metadata in the consuming templates and run extraction validation if tracked message locations change.

## 10. Unknowns and Required Clarifications

- No blocking clarification is required to prepare this implementation plan.
- The implementation should make one explicit decision about the public output name (`pressed` versus another clear equivalent) and keep that decision consistent across the shared component and migrated consumers.
- If the team expects the component to support native attributes beyond the current migration needs, a follow-up enhancement may be warranted after this slice. That broader API is not required to complete the current work.

## 11. Completion Checklist

- [x] A reusable standalone button component exists under `UI/src/design-system/`.
- [x] The component supports `primary` and `secondary` visual variants through a typed input.
- [x] The component projects arbitrary inner content through Angular content projection.
- [x] The component emits a documented activation output when users click or tap it.
- [x] The component preserves native button semantics with an explicit `type="button"` default.
- [x] Shared button styling has one canonical source of truth in the component, and the legacy button primitive rules have been removed.
- [x] `home-hero` uses the shared button component for its CTA without changing copy or intended emphasis.
- [x] `theme-control` uses the shared button component while preserving toggle semantics and projected icon content, and its outer button transitions align with the shared primitive hover behaviour instead of the previous click-emphasised pattern.
- [x] Shared button component tests cover variant selection, projected content, and activation output.
- [x] Consumer tests are updated to reflect the new abstraction without losing accessibility assertions.
- [x] `UI/AI_PROJECT_GUIDE.md` is updated if `UI/src/design-system/` becomes an explicit documented convention for shared Angular components.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run test:run` has been run in `UI/`.
- [x] `npm run extract:i18n` has been run in `UI/` if template message locations or text changed.
- [x] `npm run validate:i18n` has been run in `UI/` if extraction output changed.
