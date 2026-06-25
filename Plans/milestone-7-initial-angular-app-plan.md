# Milestone 7 Initial Angular App Plan

## 1. Overview

Turn the already-generated Angular application in `UI/` into a clean, intentional baseline for future delivery work.

The primary outcome of this milestone is not creating a new Angular app from scratch. It is removing Angular CLI placeholder content, keeping the project runnable with SSR and routing intact, and documenting a contributor workflow that makes UI development easy to start and consistent across the team.

## 2. Roadmap Source or Existing Plan Context

- Roadmap source: [Roadmap/initial-ui-roadmap.md](C:/Projects/summer-born-info/Roadmap/initial-ui-roadmap.md), Milestone 7 "Initial Angular App".
- User refinement for this plan:
  - the Angular app has already been initialized with Angular CLI,
  - remaining work should focus on clearing scaffold/template code,
  - the result should be a clean, empty project ready for Milestone 8 and later UI work,
  - UI documentation should include a practical development workflow with recommended editor and browser tooling.
- Current repository state:
  - `UI/` already contains an Angular 22 application with SSR support.
  - `UI/src/app/app.html` still contains the full generated Angular placeholder page.
  - `UI/src/styles.scss` is still the default comment-only scaffold.
  - `UI/src/app/app.routes.ts` exists but currently defines no routes.
  - `UI/README.md` documents basic setup and commands, but it does not yet provide a contributor-focused development workflow or recommended tooling setup.

## 3. Scope

- Remove generated placeholder UI content and starter styling from the Angular scaffold.
- Keep the app runnable locally through the current Angular scripts after cleanup.
- Preserve the existing SSR-capable project shape unless a simplification is clearly safe and explicitly justified during implementation.
- Establish a minimal application shell that is intentionally empty but structurally ready for later homepage and admin work.
- Document the recommended UI development workflow, including local commands, editor setup, and browser tooling for day-to-day work.
- Align UI guidance files if the cleanup introduces or clarifies baseline structural conventions.

## 4. Non-Goals

- Designing or implementing the Milestone 8 homepage.
- Choosing the final product theme, typography, branding, or layout system.
- Building admin authentication, protected routes, or API integration.
- Introducing a full design system, shared component library, or state-management framework.
- Reworking the Angular project into a materially different architecture unless an existing generated choice is actively blocking delivery.
- Defining a repository-wide API and UI combined startup workflow if one does not yet exist.

## 5. Behaviour Scenarios

### Scenario: Developer starts the UI locally after scaffold cleanup

Given the Angular project has had starter content removed  
When a contributor runs `npm start` from `UI/`  
Then the app should compile successfully and load a deliberately minimal shell instead of Angular demo content.

### Scenario: Visitor opens the application before feature work begins

Given no homepage or admin features have been implemented yet  
When the root route is rendered in the browser  
Then the page should show a simple baseline shell or placeholder state that is clearly project-owned, accessible, and free of Angular branding/demo links.

### Scenario: Routing baseline remains ready for future milestones

Given Milestone 8 and later milestones will add public and admin routes  
When a developer reviews the cleaned project structure  
Then they should find an obvious place for root shell concerns, route definitions, and future feature folders without first undoing generated template decisions.

### Scenario: Contributor follows the recommended UI workflow

Given a contributor is new to the UI project  
When they read the UI setup and workflow documentation  
Then they should be able to install dependencies, run the app, run tests, format code, and use the recommended editor/browser tooling without guessing at missing steps.

### Scenario: Contributor wants a productive debugging setup

Given a contributor is iterating on Angular UI work  
When they follow the documented recommendations  
Then they should have a supported editor setup in VS Code, Angular-aware language tooling, formatting support, and a recommended browser setup that enables Angular inspection and routine debugging.

## 6. Deliverables

1. Angular scaffold cleanup
   - Remove the generated Angular placeholder markup, inline styles, sample links, and demo branding from `UI/src/app/app.html`.
   - Remove root component state that only exists to support the starter template.
   - Replace starter shell output with a minimal, intentional application baseline suitable for follow-on UI work.

2. Minimal shell baseline
   - Keep the root `App` component focused on shell concerns.
   - Retain a clean route entry strategy, even if Milestone 7 still serves only a root placeholder experience.
   - Ensure the root shell and route structure do not need to be torn apart before Milestone 8 homepage work begins.

3. Style baseline cleanup
   - Remove generated sample styles and comment-only placeholders that no longer add value.
   - Leave behind only the smallest useful global and app-level style baseline needed for a neutral starting point.
   - Avoid introducing homepage-specific visual decisions in this milestone.

4. Validation baseline
   - Update or replace scaffold-level tests so they validate the cleaned shell rather than generated Angular starter content.
   - Confirm the current local commands remain accurate for run, build, and test workflows after cleanup.

5. Development workflow documentation
   - Add or expand contributor documentation with a dedicated "development workflow" section or child document.
   - Document the recommended local loop:
     - install dependencies,
     - start the dev server,
     - run tests,
     - format or check formatting,
     - inspect the app in the browser.
   - Recommend VS Code as the primary editor baseline for this project.
   - Recommend the minimum plugin set needed for productive Angular work, expected to include:
     - Angular Language Service,
     - Prettier,
     - and any repo-specific workspace recommendations already present in `UI/.vscode/`.
   - Recommend a Chromium-based browser with Angular DevTools for routine component and change-detection inspection, while keeping browser choice guidance practical rather than mandatory.

6. Guidance alignment
   - Update `UI/README.md`, `UI/AI_PROJECT_GUIDE.md`, or related docs if the cleanup establishes clearer conventions for app-shell ownership, route growth, or day-to-day developer workflow.

## 7. Technology Requirements and Decisions

- Decision: treat Angular project creation as complete.
  - Rationale: the repo already contains a generated Angular 22 workspace with dependencies installed and scripts defined.

- Decision: preserve the current SSR-capable application shape for this milestone.
  - Rationale: SSR support is already part of the generated project, and removing it now would create unnecessary scope and possible rework without a roadmap requirement to do so.

- Decision: keep Milestone 7 visually neutral.
  - Rationale: Milestone 8 owns the first intentional theme and homepage direction, so this milestone should avoid premature design choices.

- Decision: document a recommended contributor toolchain centered on VS Code.
  - Rationale: the user explicitly wants a clear, easy-to-follow UI development setup, and VS Code plus Angular-specific tooling is the lowest-friction baseline for Angular contributors.

- Decision: prefer built-in Angular/npm workflows before introducing extra task runners or tooling.
  - Rationale: the current project already has the necessary scripts for start, build, watch, and test, and Milestone 7 should reduce noise rather than add more moving parts.

## 8. Dependencies and Sequencing

1. Confirm the current project baseline
   - Review the generated Angular shell, routes, styles, tests, and documentation.

2. Clean the scaffold
   - Status: Completed on 2026-06-25 after validating with `npm run build` and `npm test -- --watch=false`.
   - Remove placeholder markup, styles, and sample component state.

3. Re-establish the minimal shell
   - Status: Completed on 2026-06-25 after validating with `npm run build` and `npm test -- --watch=false`.
   - Leave behind a deliberate root app structure that still runs cleanly and is ready for route growth.

4. Update validation
   - Status: Completed on 2026-06-25 with no additional code changes required after validating `npm start`, `npm run build`, and `npm test -- --watch=false`.
   - Adjust tests to assert the new baseline experience.

5. Write workflow documentation
   - Status: Completed on 2026-06-25 after documenting the contributor workflow in `UI/README.md` and validating the documented commands against the current project setup.
   - Capture the recommended editor, extensions, browser tooling, and daily commands after the cleaned app shape is in place.

6. Perform consistency review
   - Ensure docs, tests, and shell structure all describe the same baseline and do not reference removed scaffold content.

## 9. Risks and Mitigations

- Risk: cleanup accidentally removes structure needed for later milestones.
  - Mitigation: keep the root shell, route file, and SSR entry points intact unless there is a clear and documented reason to change them.

- Risk: Milestone 7 drifts into homepage design work.
  - Mitigation: keep visual output intentionally sparse and defer branding, richer layout, and public content to Milestone 8.

- Risk: documentation recommends tools that are helpful but not actually reflected in the repo workflow.
  - Mitigation: derive the documented workflow from the existing `package.json`, `.vscode` folder contents, and current Angular setup rather than generic Angular advice.

- Risk: scaffold tests become misleading or brittle after cleanup.
  - Mitigation: replace generated assertions with small, stable checks that verify the new project-owned shell.

- Risk: contributors still guess how to work efficiently in the UI.
  - Mitigation: make one canonical workflow document or section that covers the normal edit-run-test-debug loop end to end.

## 10. Unknowns and Required Clarifications

- No blocker is currently identified for producing an implementation-ready plan.
- Assumption: retaining SSR support is preferred for now because it already exists and there is no explicit request to remove it.
- Assumption: the recommended browser tooling should be advisory, not a hard project requirement.
- Follow-up to settle during implementation if needed:
  - whether the workflow guidance belongs entirely in `UI/README.md` or should live in a dedicated child document such as `UI/docs/development-workflow.md` with the README linking to it.

## 11. Completion Checklist

- [x] Angular placeholder markup, links, branding, and demo styles are removed from the root app shell.
- [x] The root app still runs locally with `npm start` from `UI/`.
- [x] The project retains a clean minimal shell and route baseline ready for Milestone 8 work.
- [x] Global and app-level styles are reduced to an intentional neutral baseline.
- [x] Unit tests no longer assert scaffold/demo behaviour and instead cover the cleaned baseline shell.
- [x] UI documentation includes a practical development workflow for daily UI work.
- [x] The workflow documentation recommends VS Code, the relevant extensions, and a browser debugging setup that matches Angular development needs.
- [ ] Any new or clarified UI structural conventions are reflected in the appropriate UI guidance file.
- [ ] Final review confirms the milestone delivers a clean starting point rather than unfinished scaffold content.
