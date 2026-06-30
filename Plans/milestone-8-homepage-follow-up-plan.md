# Milestone 8 Homepage Follow-Up Plan

## 1. Overview

Deliver a focused follow-up to the recently shipped homepage so the page fills the viewport cleanly, gains a reusable shell header, and replaces the current colour-mode `<select>` with a compact animated toggle control plus system-reset affordance. The work should preserve the approved visual direction while improving the base shell structure for future public-page expansion.

## 2. Source Context

- Existing delivery baseline: [milestone-8-homepage-plan.md](./milestone-8-homepage-plan.md)
- Approved design direction: [milestone-8-homepage-style-decision.md](./milestone-8-homepage-style-decision.md)
- Feedback to address:
  - The app shell padding creates an unwanted gap around the homepage and should allow the page to fill the browser window.
  - The shell should now include a header bar with only the `Summer born info` brand and the theme selector.
  - The light/dark selector should become a toggle button with animated sun and moon-stars icons plus a small reset-to-system label below.
  - The footer should include icon attribution: `Uicons by Flaticon`.
- Relevant UI areas:
  - `UI/src/app/shell/` owns app-level layout and colour-mode interaction.
  - `UI/src/app/features/home/` owns homepage-specific content.
  - `UI/public/` or equivalent static asset handling already serves local assets.
  - `UI/prototypes/sun.svg` and `UI/prototypes/moon-stars.svg` are the supplied icon sources.

## 3. Scope

- Remove shell-level framing that prevents the homepage from reaching the viewport edges.
- Introduce a reusable app header bar in the root shell.
- Replace the existing colour-mode `<select>` with a dedicated theme toggle control and reset affordance.
- Add the supplied theme icons to the production asset flow.
- Add the required icon attribution to the homepage footer.
- Update tests, localization artifacts, and UI validation to reflect the new shell structure and control behaviour.

## 4. Non-Goals

- Reworking the approved homepage content sections beyond the shell/header/footer adjustments needed for this feedback.
- Expanding the header into full navigation, search, or account controls.
- Changing the underlying colour token strategy or replacing the existing colour-mode persistence model unless needed to support the new toggle interaction cleanly.
- Introducing additional third-party icon or animation dependencies.

## 5. Behaviour Scenarios

### Scenario: Homepage fills the browser window without an outer shell gap

Given a visitor opens the homepage on desktop or mobile, when the root shell renders, then no generic shell padding should create a visible frame around the page and the header, main content, and footer should compose against the viewport edges as intended by the approved design.

### Scenario: The app shell provides a reusable public header

Given the homepage route renders inside the root shell, when the page loads, then the shell should show a header bar containing only the `Summer born info` brand and the theme control, leaving feature-specific hero and content sections inside the homepage route.

### Scenario: A visitor toggles between light and dark mode

Given the visitor is using the header theme control, when they activate the toggle button, then the control should switch between explicit light and dark modes, update the document colour-mode override, persist the explicit choice, and animate between the sun and moon-stars icon states.

### Scenario: A visitor resets to the system preference

Given the visitor previously selected an explicit light or dark mode, when they activate the reset affordance below the toggle, then the explicit override should be removed, persisted storage should be cleared, and the application should return to the operating-system preference.

### Scenario: Assistive technology users understand the theme controls

Given a keyboard or screen-reader user reaches the header controls, when they interact with the toggle and reset actions, then both controls should expose clear accessible names, state, focus styling, and predictable button semantics without relying on icon imagery alone.

### Scenario: Footer attribution remains visible in production

Given the homepage footer is rendered, when a visitor reaches the attribution area, then they should be able to find and activate the `Uicons by Flaticon` link without the attribution overpowering the main footer content.

## 6. Deliverables

1. Refactor the root shell layout in `UI/src/app/shell/` so it owns a full-width public-page frame without the current outer padding.
2. Add a dedicated shell header component or equivalent focused shell subcomponent for the public header bar, containing only branding and theme controls.
3. Extract the colour-mode UI into a distinct component with a single responsibility for rendering and handling the toggle interaction, while continuing to use the existing colour-mode service as the persistence and document-state owner.
4. Introduce a small reset affordance within the colour-mode control component that returns the app to system mode without conflating that action with the light/dark toggle itself.
5. Add the supplied `sun.svg` and `moon-stars.svg` assets to the production-served asset path and wire them into the theme control without adding a new icon dependency.
6. Implement motion for the icon transition that is lightweight, accessible, and compatible with reduced-motion expectations.
7. Update the homepage footer content so it includes the required icon attribution link while preserving the current project summary messaging.
8. Update or add tests that cover shell structure, theme toggle behaviour, reset behaviour, and footer attribution rendering.
9. Refresh i18n extraction outputs for any new visible text such as the reset label and attribution text.
10. Run the required UI validation commands before handoff.

## 7. Component Boundaries

- `RootShell`
  Owns app-level page composition only: header placement, routed main content, and any shell container rules.
- `PublicHeader` or shell header subcomponent
  Owns the header bar structure and brand presentation, but not colour-mode persistence logic.
- `ThemeToggle`
  Owns the interactive UI for explicit light/dark switching, icon rendering, animation state, and the reset affordance.
- `ColourModeService`
  Continues to own mode state, persistence, and document-root attribute updates.
- `Home`
  Continues to own homepage feature content and footer content, including the added attribution copy.

This split keeps future public-page growth straightforward: the shell can evolve independently from homepage content, and the colour-mode interaction can be reused or restyled without reworking feature templates.

## 8. Technology Requirements And Decisions

- Keep the implementation inside the existing Angular standalone component structure under `UI/src/app/`.
- Prefer local SVG asset usage from the Angular public asset pipeline rather than inlining large SVG markup into templates unless there is a clear accessibility or animation reason to do so.
- Use standard Angular template bindings and component-scoped SCSS for the toggle UI; do not introduce an animation library for this interaction.
- Preserve the existing `ColourModeService` contract unless a small extension is needed to support simpler toggle logic such as deriving the next explicit mode from the current effective state.
- Keep shell-level CSS responsible for viewport fill and header framing, while homepage SCSS remains responsible for homepage sections and footer styling.
- Ensure any new visible strings participate in the existing Angular i18n workflow.

## 9. Dependencies And Sequencing

1. Confirm the current shell structure, colour-mode service behaviour, and homepage footer ownership.
2. Move the shell from padded framing to full-viewport composition so the layout baseline is correct before styling the new header.
3. Introduce the shell header structure and extract the theme control into its own component boundary.
4. Add and wire the theme icons through the production asset path.
5. Implement toggle interaction, icon animation, and reset behaviour on top of the existing colour-mode service.
6. Update the homepage footer with attribution once the icon usage is final.
7. Refresh tests, localization output, and validation commands.

## 10. Risks And Mitigations

- Risk: Removing shell padding unintentionally breaks spacing on small screens.
  - Mitigation: move spacing responsibility to the header and homepage sections instead of relying on a global shell inset.
- Risk: A two-state toggle makes the current `system` mode harder to understand.
  - Mitigation: keep reset-to-system as a clearly labeled secondary action directly beneath the toggle and cover the behaviour in tests.
- Risk: Icon-only controls reduce accessibility.
  - Mitigation: keep text labels or accessible names on the control, expose state with ARIA where appropriate, and verify keyboard focus behaviour.
- Risk: Theme animation feels distracting or conflicts with reduced-motion users.
  - Mitigation: keep the animation subtle and disable or simplify it under `prefers-reduced-motion`.
- Risk: Footer attribution becomes visually noisy.
  - Mitigation: style attribution as secondary footer metadata while preserving link contrast and readability.

## 11. Unknowns And Clarifications

- No blocking clarification is required to plan this slice.
- Assumption: the reset affordance should be implemented as a small secondary button or link-style button directly below the main toggle, because the feedback describes it as a label below rather than a third toggle state in the primary control.
- Assumption: the header brand text should use the requested casing `Summer born info` in the header, even though other existing homepage copy uses `Summer Born Info`.
- If the user wants the header brand to navigate somewhere, or wants the reset affordance phrased differently from `Reset to system`, that can be handled during implementation without changing the overall plan.

## 12. Completion Checklist

- [x] Shell-level outer padding no longer creates a visible frame around the homepage.
- [x] The root shell renders a reusable public header bar.
- [x] The header contains only `Summer born info` and the theme control.
- [x] The theme control is implemented as a dedicated component separate from homepage content.
- [x] The primary theme control toggles between explicit light and dark mode.
- [x] The control visually animates between the supplied sun and moon-stars icons.
- [x] A small reset-to-system affordance is rendered below the toggle and clears the explicit override.
- [x] The supplied icon assets are available through the production UI asset path.
- [x] Footer attribution for `Uicons by Flaticon` is present and linked correctly.
- [x] Accessibility expectations are covered for semantics, keyboard access, focus states, and reduced-motion handling.
- [x] Automated tests are updated for shell structure and theme behaviour.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run extract:i18n` has been run in `UI/` if tracked messages changed.
- [x] `npm run validate:i18n` has been run in `UI/` if localization output changed.
- [x] `npm run test:run` has been run in `UI/`.
