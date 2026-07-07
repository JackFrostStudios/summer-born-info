# Styling Review Remediation Plan

## 1. Overview

Resolve the issues captured in `UI/review-findings/styling-expert.md`, using the current `UI/` codebase as the source of truth for each finding's status. This plan keeps all reviewed styling concerns visible for tracking, including items that have already been addressed since the review was written.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-07 to create a remediation plan for `UI/review-findings/styling-expert.md`.
- Review source: `UI/review-findings/styling-expert.md`.
- Verification source: a dedicated codebase-check sub-agent verified the review findings against the current repository state on 2026-07-07 before this plan was drafted.
- Relevant repository guidance:
  - `Plans/AGENTS.md`
  - `UI/AGENTS.md`
  - `AI_PROJECT_GUIDE.md`
  - `UI/AI_PROJECT_GUIDE.md`
  - `UI/src/styles/README.md`

### Verified Review Status Snapshot

1. Global readable-width constraint on `p` and `li` - `Open`
2. Homepage hero surface recipe is split between `.sbi-surface` and feature-local overrides - `Partially Resolved`
3. Under-construction page no longer owns a one-off panel surface - `Resolved`
4. Underline styling motif is duplicated between global links and the homepage hero highlight - `Open`
5. Repeated raw `clamp()` sizing and spacing formulas remain spread across feature and design-system styles - `Open`
6. Public-header negative letter-spacing remains a local exception instead of a named token - `Open`
7. Theme-control geometry still hard-codes icon sizing and transform math - `Open`
8. Button base padding still hard-codes rem values instead of spacing tokens - `Open`
9. Theme-control color-mode selectors still mix root selectors and `:host-context(...)` - `Open`
10. Global `:focus` reset still relies on transparent outlines plus `:focus-visible` recovery - `Open`
11. Cascade layers are still absent from the shared stylesheet stack - `Open`
12. Public-header responsive safeguards for narrow or localized content are still missing - `Open`
13. Homepage hero two-column breakpoint pressure remains - `Open`
14. Shared placeholder/panel two-column breakpoint pressure remains for under-construction and not-found pages - `Open`
15. Under-construction viewport-height rhythm still depends on `dvh` sizing - `Open`
16. Shared placeholder-panel abstraction now exists through `sbi-panel` and is reused across multiple routes - `Resolved`

## 3. Scope

- Fix the still-open styling-review findings in the Angular UI.
- Preserve resolved findings in the plan so the full review remains traceable.
- Prefer shared tokens, primitives, and design-system ownership where repetition already exists across features.
- Update tests, documentation, and validation steps when shared styling behavior or responsive structure changes.

## 4. Non-Goals

- Rebranding the visual system or replacing the current token palette.
- Reworking unrelated page copy, route structure, or feature behavior outside the styling findings.
- Introducing a large utility-class framework or broad CSS architecture reset.
- Forcing cascade layers immediately if implementation shows the current UI is still too small to justify the churn.

## 5. Behaviour Scenarios

### Scenario: Long-form copy should only be width-constrained intentionally

Given a paragraph or list appears inside a grid, card, footer, or compact shell area, when it is not explicitly marked as long-form reading content, then it should use the container width naturally instead of inheriting a global readable-width cap.

### Scenario: Shared surface styling should come from a deliberate shared abstraction

Given a routed page or feature panel needs a reusable raised surface treatment, when the component renders, then the shared primitive or shared design-system component should own the common border, radius, shadow, and background recipe while feature-local CSS owns only route-specific behavior.

### Scenario: Styling tokens should express repeated geometry decisions

Given multiple components use the same spacing, icon sizing, or responsive sizing patterns, when those patterns are tuned later, then the values should be editable through shared or clearly named local custom properties instead of repeated raw literals.

### Scenario: Narrow headers and localized text should remain usable

Given the public header renders on a narrow viewport or with longer translated copy, when the brand text and theme control compete for space, then the header should wrap or constrain gracefully without overlap, clipping, or unreadable compression.

### Scenario: Theme control styling should stay easy to reason about

Given the theme reel responds to explicit and implicit colour-mode changes, when the stylesheet applies the transformed state, then one clear selector strategy should control the state and shared geometry values should drive the reel movement.

### Scenario: Keyboard focus should remain visible in supported browsers

Given a keyboard or assistive-technology user tabs onto a focusable control, when the browser applies focus styling, then the UI should preserve a visible indicator without relying on a global reset that can suppress fallback outlines unexpectedly.

### Scenario: Responsive feature layouts should avoid abrupt pressure points

Given the homepage hero or shared placeholder-panel layouts cross their wide-screen breakpoint, when content grows through localization or future copy changes, then the layouts should still preserve readable columns, stable spacing, and non-jumpy vertical rhythm.

## 6. Deliverables

1. Remove the global `p, li` readable-width rule from `UI/src/styles/_base.scss`.
2. Apply `.sbi-readable` or component-local readable-width constraints only in templates or component styles that intentionally present long-form reading content.
3. Review the homepage hero surface styling and decide whether the gradient treatment should remain feature-owned or move into a documented shared variation; in either case, remove unnecessary duplication with `.sbi-surface`.
4. Preserve the already-resolved under-construction surface finding in the implementation record by keeping `sbi-panel` as the shared placeholder/status panel abstraction.
5. Decide whether the repeated hero-emphasis underline treatment warrants a token or primitive; if not, simplify the duplication so the visual recipe is defined in one deliberate place.
6. Introduce shared or clearly named local custom properties for repeated theme-control geometry values, including icon size and reel offset math.
7. Replace hard-coded button base padding values with spacing-token-based equivalents in `UI/src/design-system/button/_button-styles.scss`.
8. Evaluate repeated raw `clamp()` formulas across header, hero, placeholder, and panel styles; extract shared semantic tokens only where the same sizing rule is genuinely part of the design language.
9. Decide whether the public-header negative letter-spacing should become a named brand token or remain a documented exception, then align the code with that decision.
10. Add public-header responsive safeguards such as `min-inline-size: 0`, wrapping, truncation, or an intentional narrow-layout pattern so the shell remains stable with longer text.
11. Simplify theme-control colour-mode selectors to one intentional strategy, preferably the most maintainable component-scoped option supported by the Angular styling model.
12. Revisit the global focus reset so fallback focus visibility is preserved across the supported browser matrix.
13. Review the homepage hero breakpoint and shared panel breakpoint behavior, then adjust breakpoint timing, column minima, gaps, or layout strategy where necessary to reduce translation-pressure risk.
14. Reassess `min-block-size: min(68dvh, 48rem)` on the under-construction route and move page-height rhythm to a more stable owner if mobile viewport behavior remains fragile.
15. Decide whether cascade layers should be introduced now or explicitly deferred; record that decision in the implementation outcome rather than leaving it implicit.
16. Add or update focused UI tests, browser accessibility coverage, or manual validation notes where responsive shell, shared panel, focus, or shared styling behavior changes materially.
17. Run `npm run format`, `npm run lint`, `npm run test:run`, and `npm run validate:i18n` in `UI/`, plus `npm run extract:i18n` if any user-facing template text or extraction metadata changes during implementation.

## 7. Technology Requirements and Decisions

- Shared styling ownership:
  Continue using the existing split between global tokens/primitives in `UI/src/styles/`, feature-local styles in `UI/src/app/`, and reusable shared component styling in `UI/src/design-system/`.
- Surface abstractions:
  Keep `sbi-panel` as the shared route-level placeholder/status panel abstraction. Do not regress to duplicated feature-local gradient-panel shells.
- Token extraction:
  Only promote repeated style values into shared tokens when they represent durable design language rather than a one-off feature treatment.
- Theme selector strategy:
  Prefer a single understandable theming approach per component instead of mixing root-level selectors and `:host-context(...)` for the same state.
- Focus styling:
  Treat keyboard-visible focus behavior as a supported accessibility contract, not a cosmetic preference.
- Cascade layers:
  This is a strategic CSS architecture decision. If implementation shows the current app is still too small for the added complexity, explicitly defer the change while still closing the higher-priority concrete findings.
- Validation:
  Reuse the existing UI validation workflow and extend automated coverage only where the styling changes introduce behavior that is likely to regress silently.

## 8. Dependencies and Sequencing

1. Start with shared-foundation decisions: readable-width ownership, button padding tokens, theme-control geometry variables, and the public-header letter-spacing decision.
2. Tackle the public-header responsive safeguards and theme-control selector simplification together so shell-level styling remains coherent.
3. Resolve homepage hero duplication and underline-pattern ownership once the shared-style direction is settled.
4. Revisit shared placeholder and responsive breakpoint behavior for `sbi-panel`, under-construction, and not-found after the shell-level rules are stable.
5. Rework the global focus reset only after confirming the supported browser and accessibility expectations for the current UI test matrix.
6. Make an explicit keep-or-defer decision on cascade layers after the concrete remediation work is complete and the remaining stylesheet complexity is visible.
7. Finish with formatting, linting, tests, and i18n validation.

## 8a. Ordered Implementation Steps

### Step 1. Shared Foundation And Shell Styling

Status: Completed on 2026-07-07. Validation passed with `npm run format`, `npm run lint`, `npm run test:run`, `npm run extract:i18n`, `npm run build:localize`, and `npm run check:localized-ssr-assets`. `npm run validate:i18n` hit the expected extraction-drift guard before the extracted file was refreshed.

- Remove the global `p, li` readable-width rule from `UI/src/styles/_base.scss`.
- Apply explicit readable-width constraints only where long-form reading is intentional.
- Replace hard-coded button base padding with spacing-token-based values.
- Decide and implement the public-header letter-spacing direction, then add responsive safeguards for narrow or localized header content.
- Refactor the theme control to use named geometry custom properties and one intentional colour-mode selector strategy.
- Revisit the global focus reset so fallback focus visibility is preserved.
- Add or update focused tests for shell-level and theme-control behavior where practical.

### Step 2. Feature Surface And Responsive Layout Remediation

- Resolve homepage hero shared-surface ownership so `.sbi-surface` owns the shared shell and feature styles own only the hero-specific treatment.
- Make the underline motif deliberate and non-duplicative.
- Review repeated raw `clamp()` formulas in the hero, header, and placeholder surfaces and extract only the semantic tokens or named local properties that represent true shared patterns.
- Reduce breakpoint pressure in the homepage hero and the shared placeholder/panel layouts.
- Reassess the under-construction viewport-height rhythm and move it to a more stable owner if the current feature-owned `dvh` strategy remains fragile.
- Preserve the existing `sbi-panel` shared placeholder/status abstraction while aligning route-level styles to it cleanly.
- Add or update focused tests and accessibility coverage where responsive layout behavior changes materially.

### Step 3. Documentation, Architecture Decision, And Full Validation

- Update styling or structure documentation if the supported shared styling contract changes.
- Make an explicit cascade-layer decision and record the implementation outcome in this plan rather than leaving the finding implicit.
- Run `npm run format`, `npm run lint`, `npm run test:run`, and `npm run validate:i18n` in `UI/`, plus `npm run extract:i18n` if extraction-relevant template metadata changed.
- Update this plan's completion checklist to reflect the delivered state and any explicit deferral decision.

## 9. Risks and Mitigations

- Risk: Removing the global readable-width rule could unintentionally widen copy in places that currently depend on it.
  - Mitigation: update content-heavy templates in the same slice and verify line length visually on the main routes.
- Risk: Extracting too many new tokens can overfit the current app and make the shared style layer noisier.
  - Mitigation: only introduce shared tokens for genuinely repeated cross-feature patterns; keep one-off values local and documented.
- Risk: Theme-control selector or geometry refactors can break dark-mode motion without obvious compile failures.
  - Mitigation: keep focused component tests green and include manual browser verification for explicit light, explicit dark, and system-theme modes.
- Risk: Header and panel breakpoint changes can improve one screen width while regressing another.
  - Mitigation: verify around the current `48rem` breakpoint and on narrow mobile widths with longer sample strings.
- Risk: Focus-style changes can create browser-specific regressions.
  - Mitigation: pair the CSS change with explicit accessibility smoke checks and document any intentionally supported fallback behavior.
- Risk: Cascade layers could introduce unnecessary churn relative to the current stylesheet size.
  - Mitigation: treat layers as a decision checkpoint, not an automatic implementation requirement.

## 10. Unknowns and Required Clarifications

- No blocking product clarification is required to draft this plan.
- The main implementation-time decision checkpoint is cascade layers: they should only be introduced if the remaining stylesheet complexity justifies the extra architecture.
- The other notable decision checkpoint is hero-surface ownership: if the homepage gradient treatment is intended as a reusable pattern, it should become a documented shared variation rather than staying as an undocumented partial override.

## 11. Completion Checklist

- [ ] Issue 1 open finding is resolved by removing the global readable-width rule and applying readable-width constraints intentionally.
- [ ] Issue 2 partial finding is resolved by clearly separating homepage hero shared-surface ownership from feature-specific styling.
- [x] Issue 3 remains resolved: the under-construction route no longer owns a one-off panel surface.
- [ ] Issue 4 open finding is resolved by making the underline motif deliberate and non-duplicative.
- [ ] Issue 5 open finding is resolved by reducing repeated raw geometry formulas where they represent shared patterns.
- [x] Issue 6 open finding is resolved by aligning public-header letter-spacing with either a named token or an intentional documented exception.
- [x] Issue 7 open finding is resolved by extracting theme-control geometry values into named custom properties.
- [x] Issue 8 open finding is resolved by replacing hard-coded button padding with spacing-token-based values.
- [x] Issue 9 open finding is resolved by using one intentional theme-control colour-mode selector strategy.
- [x] Issue 10 open finding is resolved by preserving robust focus visibility without the current global transparent-outline risk.
- [ ] Issue 11 cascade-layer finding is closed either by implementation or by an explicit documented deferral decision.
- [x] Issue 12 open finding is resolved by adding responsive safeguards to the public header.
- [ ] Issue 13 open finding is resolved by reducing homepage hero breakpoint pressure.
- [ ] Issue 14 open finding is resolved by reducing shared placeholder-panel breakpoint pressure for localized or expanded content.
- [ ] Issue 15 open finding is resolved by confirming or revising the under-construction viewport-height strategy.
- [x] Issue 16 remains resolved: `sbi-panel` is now the shared placeholder/status panel abstraction used across multiple routes.
- [ ] Any affected documentation in `UI/src/styles/README.md`, `UI/AI_PROJECT_GUIDE.md`, or design-system component READMEs is updated if the supported styling contract changes.
- [ ] Relevant UI tests and accessibility smoke coverage are updated or supplemented where behavior meaningfully changes.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run test:run` has been run in `UI/`.
- [x] `npm run extract:i18n` has been run in `UI/` if extraction-relevant template metadata changed.
- [ ] `npm run validate:i18n` has been run in `UI/`.
