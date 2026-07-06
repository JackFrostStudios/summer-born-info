# Front End Architecture Review Remediation Plan

## 1. Overview

Resolve the actionable findings captured in `UI/review-findings/front-end-architect.md`, using the current `UI/` codebase as the source of truth for issue status. This plan keeps every reviewed issue visible for tracking, including findings that have already been resolved and findings the user has explicitly directed us not to pursue.

This plan records four explicit product decisions from the request:

- `Homepage CTA Is Coupled To A Temporary Placeholder Route` is `WON'T DO`.
- `Prototype Assets Live Beside Production Assets Without A Retirement Signal` is `WON'T DO`.
- `The Under-Construction Feature Is Generic But Lives As A Full Feature Route` is `WON'T DO`.
- `API Integration Gap Is Correctly Documented But Will Become A Scaling Blocker` is `WON'T DO`.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-06 to create a remediation plan for `UI/review-findings/front-end-architect.md`.
- Review source: `UI/review-findings/front-end-architect.md`.
- Verification source: a dedicated codebase-check sub-agent plus local inspection verified each issue against the current repository state on 2026-07-06 before this plan was drafted.
- Relevant repository guidance:
  - `Plans/AGENTS.md`
  - `UI/AGENTS.md`
  - `AI_PROJECT_GUIDE.md`
  - `UI/AI_PROJECT_GUIDE.md`
  - `UI/src/design-system/README.md`
  - `UI/src/styles/README.md`

### Verified Review Status Snapshot

1. Issue 1: Homepage CTA Is Coupled To A Temporary Placeholder Route - `WON'T DO`
2. Issue 2: Shared Design-System Component Has No Barrel Or Public API Boundary - `Open`
3. Issue 3: Styling Governance Depends On Guidance More Than Enforcement - `Resolved`
4. Issue 4: Prototype Assets Live Beside Production Assets Without A Retirement Signal - `WON'T DO`
5. Issue 5: The Under-Construction Feature Is Generic But Lives As A Full Feature Route - `WON'T DO`
6. Issue 6: Theme State Is Well Encapsulated, But Its App-Level Ownership Should Be Documented - `Resolved`
7. Issue 7: API Integration Gap Is Correctly Documented But Will Become A Scaling Blocker - `WON'T DO`

### Current-State Notes That Shape The Plan

- The only still-actionable architecture finding from this review is the missing design-system public API boundary.
- The styling-governance finding is now addressed by the shared governance guidance added to `UI/src/styles/README.md`, including the typography-exception policy that explains the current shell typography exception.
- The theme-state ownership finding is now sufficiently covered by the current shell ownership guidance in `UI/AI_PROJECT_GUIDE.md` plus the still-contained implementation under `UI/src/app/shell/theme-control/`.
- The four `WON'T DO` items remain intentionally visible in this plan for tracking, but they should not generate implementation tasks until product direction changes.

## 3. Scope

- Resolve the still-open design-system public API boundary finding in `UI/`.
- Preserve every review item in the plan with an explicit tracked status.
- Update design-system import conventions, in-repo consumers, and related documentation if the public API boundary is introduced.
- Add or update focused tests only where import-boundary or shared-component behavior changes require it.

## 4. Non-Goals

- Reworking the homepage CTA flow while its future destination is still unknown.
- Adding retirement policy work for prototype folders in this remediation slice.
- Rehoming or generalizing the `under-construction` route while its future ownership is still unsettled.
- Defining the first frontend API integration architecture before a live API-consuming feature needs it.
- Reopening the resolved styling-governance or theme-ownership findings unless implementation uncovers a real regression.
- Introducing a top-level monolithic design-system export surface if a narrow per-component boundary is sufficient.

## 5. Behaviour Scenarios

### Scenario: A feature imports a shared design-system button

Given a feature or shell component needs `sbi-button`, when a developer imports the shared component, then the import should come from a stable folder-level public API such as `@design-system/button` rather than from an implementation file path like `@design-system/button/button`.

### Scenario: A feature imports another shared design-system component

Given another shared design-system component such as `sbi-panel` is reused, when a developer imports it, then the import should follow the same documented public-boundary convention so consumers do not depend on internal file names.

### Scenario: Design-system internals change later

Given the implementation files inside a shared component folder change names or internal structure, when the refactor happens, then consumer imports should remain stable because they depend on the folder-level public API rather than on the component's internal file layout.

### Scenario: Review tracking remains explicit

Given a contributor revisits this plan later, when they review issue status, then they should be able to see which review findings were intentionally left alone as `WON'T DO`, which are already `Resolved`, and which still require delivery work.

## 6. Deliverables

1. Introduce a narrow public export boundary for current shared design-system component folders, starting with `UI/src/design-system/button/` and `UI/src/design-system/panel/`.
2. Choose the cleanest implementation shape for that boundary:
   - Prefer `index.ts` inside each component folder if Angular tooling and current path aliases support it cleanly.
   - If folder-level imports need config support, update the relevant TypeScript path mapping or workspace configuration in the same slice.
3. Migrate in-repo consumers away from deep implementation-file imports such as `@design-system/button/button` and `@design-system/panel/panel`.
4. Keep Sass partials and other internal implementation files private to their component folders unless a real shared dependency requires exposing something more.
5. Update `UI/src/design-system/README.md` and any affected component README files so the documented import convention matches the new public API boundary.
6. Preserve issue 3 and issue 6 as explicit `Resolved` items in the implementation record without reopening them unless regression evidence appears.
7. Preserve issues 1, 4, 5, and 7 as explicit `WON'T DO` items with no code changes attached in this remediation slice.
8. Run `npm run format`, `npm run lint`, and `npm run test:run` in `UI/` for any implementation slice that changes TypeScript, templates, or shared component imports.

## 7. Technology Requirements and Decisions

- Public API shape:
  Prefer a per-component folder public API such as `@design-system/button` over a broad top-level `UI/src/design-system/index.ts` export surface, because the review specifically calls for a narrow shared-component boundary and the current design system is still small.
- Alias ownership:
  Reuse the existing `@design-system/*` alias strategy rather than introducing a second alias family.
- Internal boundaries:
  Treat files such as `_button-styles.scss`, implementation-only selectors, and component-internal DOM structure as private details even after folder-level barrels are added.
- Dependency posture:
  Do not add new packages for this work. The change should be handled with current Angular, TypeScript, and workspace configuration.
- Validation posture:
  Use the existing UI validation commands and focused component or route tests rather than adding one-off tooling for import-boundary enforcement at this stage.

## 8. Dependencies and Sequencing

1. Confirm the current shared-component consumers and choose the public export shape for `button` and `panel`.
2. Add the folder-level public export files and any required alias/configuration support.
3. Migrate button consumers first because that import path appears across homepage, shell, and placeholder routes.
4. Migrate panel consumers next so the design-system convention is consistent across the currently shared component set.
5. Update design-system documentation after the code convention is real, so the docs reflect the implemented import boundary rather than a future intention.
6. Finish with formatting, linting, and tests from `UI/`.

### Suggested Implementation Slice

1. Design-system public API boundary:
   - add folder-level public exports for `button` and `panel`
   - update current consumers
   - update design-system documentation
   - run the standard UI validation commands for touched files

## 9. Risks and Mitigations

- Risk: Changing import paths can break Angular compilation or tests if one consumer is missed.
  - Mitigation: migrate all in-repo consumers in the same slice and finish with `npm run lint` plus `npm run test:run`.
- Risk: A public barrel could accidentally expose internal styling or helper files.
  - Mitigation: keep exports narrow and limited to the actual component types intended for reuse.
- Risk: Adding a top-level design-system index too early could create a broader public surface than the project needs.
  - Mitigation: prefer per-folder public APIs only, and add a top-level aggregate export later only if the component set grows enough to justify it.
- Risk: Future contributors may misread this review as requiring work on the four user-deferred items.
  - Mitigation: keep those findings explicitly marked `WON'T DO` in both the status snapshot and the completion checklist.

## 10. Unknowns and Required Clarifications

- No blocking clarification is required to draft or execute this plan.
- During implementation, the only technical checkpoint to confirm is the cleanest folder-level import shape supported by the current path alias setup.
- If implementation reveals that one current shared component should intentionally remain on a deep path for tooling reasons, document that exception explicitly instead of silently abandoning the public-boundary goal.

## 11. Completion Checklist

- [ ] Issue 1 remains explicitly tracked as `WON'T DO` with no remediation work performed in this slice.
- [ ] Issue 2 is resolved by introducing a documented folder-level public API boundary for the current shared design-system components and migrating in-repo consumers away from deep implementation-file imports.
- [ ] Button and panel consumers no longer import from implementation-file paths such as `@design-system/button/button` or `@design-system/panel/panel`.
- [ ] Design-system documentation reflects the implemented public import convention and continues treating internal styling files as private.
- [ ] Issue 3 remains explicitly tracked as `Resolved`.
- [ ] Issue 4 remains explicitly tracked as `WON'T DO` with no remediation work performed in this slice.
- [ ] Issue 5 remains explicitly tracked as `WON'T DO` with no remediation work performed in this slice.
- [ ] Issue 6 remains explicitly tracked as `Resolved`.
- [ ] Issue 7 remains explicitly tracked as `WON'T DO` with no remediation work performed in this slice.
- [ ] `npm run format` has been run in `UI/` if implementation edits UI files.
- [ ] `npm run lint` has been run in `UI/` if implementation edits UI files.
- [ ] `npm run test:run` has been run in `UI/` if implementation edits UI files.
