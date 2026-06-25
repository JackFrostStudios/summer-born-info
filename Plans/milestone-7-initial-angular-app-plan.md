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
  - `.github/workflows/ci.yml` currently runs API-only checks from `API/`; this should be split into a dedicated `api-ci` workflow while Milestone 7 adds a separate `ui-ci` workflow for Angular quality gates.
  - `UI/package.json` currently exposes `start`, `build`, `watch`, and `test` scripts, but it does not yet define a dedicated lint script, formatting script, or coverage-oriented CI script.

## 3. Scope

- Remove generated placeholder UI content and starter styling from the Angular scaffold.
- Keep the app runnable locally through the current Angular scripts after cleanup.
- Preserve the existing SSR-capable project shape unless a simplification is clearly safe and explicitly justified during implementation.
- Establish a minimal application shell that is intentionally empty but structurally ready for later homepage and admin work.
- Document the recommended UI development workflow, including local commands, editor setup, and browser tooling for day-to-day work.
- Define and document the repository-level CI expectations Milestone 7 now owns for the Angular app, including the checks GitHub Actions must run for UI changes.
- Align UI guidance files if the cleanup introduces or clarifies baseline structural conventions.

## 4. Non-Goals

- Designing or implementing the Milestone 8 homepage.
- Choosing the final product theme, typography, branding, or layout system.
- Building admin authentication, protected routes, or API integration.
- Introducing a full design system, shared component library, or state-management framework.
- Reworking the Angular project into a materially different architecture unless an existing generated choice is actively blocking delivery.
- Defining a repository-wide API and UI combined startup workflow if one does not yet exist.
- Expanding CI to cover broader API and UI orchestration beyond the UI quality gates required for this milestone.
- Quietly choosing long-term linting or coverage tooling without recording the decision and rationale in the plan and implementation.

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

### Scenario: Pull request validation runs for UI changes

Given a contributor opens or updates a pull request that changes files under `UI/` or the `ui-ci` workflow  
When GitHub Actions runs the Milestone 7 UI quality gates  
Then the `ui-ci` workflow should execute the agreed UI formatting, lint, build, test, and coverage checks and fail clearly when any gate does not pass.

### Scenario: Coverage expectations are visible in CI

Given the UI test suite runs in GitHub Actions  
When coverage is collected for the Angular project  
Then the workflow should publish or expose the resulting coverage output in a repeatable way so reviewers can tell whether the configured threshold or expected baseline was met.

### Scenario: API and UI feedback can arrive independently

Given a pull request changes both API and UI code  
When GitHub Actions evaluates the repository checks  
Then `api-ci` and `ui-ci` should run as separate workflows so one area can report success or failure without waiting for the other to finish.

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

7. UI CI quality gates
   - Add a dedicated `ui-ci` GitHub Actions workflow so UI changes are validated in CI instead of relying only on local commands.
   - Ensure `ui-ci` enforces the agreed formatting, lint, build, test, and coverage checks for the Angular app.
   - Ensure the formatting gate is a non-mutating check that fails when any tracked UI file would be reformatted, rather than a command that rewrites files in CI.
   - Scope `ui-ci` triggers and working directories so the UI checks run from `UI/` and do not accidentally depend on API-only defaults in the existing workflow.

8. API workflow split
   - Rename or replace the current root API-focused workflow so it becomes an explicit `api-ci` workflow rather than a generic shared `ci` workflow.
   - Preserve the existing API validation behaviour while isolating it from UI concerns so API and UI checks can run and report independently.
   - Keep workflow naming, triggers, and branch-protection intent clear enough that contributors can tell which check failed without opening unrelated jobs.

9. UI validation script baseline
   - Add or normalize the npm scripts needed for CI so the workflow can invoke stable, named commands for format checking, linting, build, test execution, and coverage generation.
   - Distinguish between a contributor convenience formatting command such as `npm run format` and a CI-safe check command such as `npm run format:check` that fails if any file is not already correctly formatted.
   - Ensure the chosen scripts are documented in `UI/README.md` and are practical for contributors to run locally before pushing changes.
   - If the current Angular toolchain does not already provide one of the required checks, add the minimum necessary project configuration and document why it was introduced.

10. UI CI documentation and review expectations
   - Document where `api-ci` and `ui-ci` live, what each workflow enforces, and how a contributor can reproduce UI failures locally.
   - Clarify any coverage threshold, artifact, or reporting expectation introduced for UI checks so maintainers know how to interpret CI output.

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

- Decision: Milestone 7 must treat UI CI as a repository-level concern, not just a local developer workflow concern.
  - Rationale: the roadmap now makes GitHub Actions enforcement part of the milestone exit criteria, so the implementation plan must include automation and not stop at documented local commands.

- Decision: split CI into separate `api-ci` and `ui-ci` workflows.
  - Rationale: the repo already has API-only automation and now needs UI-specific quality gates; separate workflows let both areas run in parallel and return faster, clearer feedback.

- Decision: enforce UI linting with `angular-eslint`, integrated through Angular CLI and exposed via a stable `npm run lint` command that delegates to `ng lint`.
  - Rationale: Angular 22 does not ship a built-in lint ruleset by default, while Angular CLI still supports `ng lint` when a lint provider is added. `angular-eslint` is the Angular ecosystem's best-supported path, brings both TypeScript and Angular template linting, fits the planned VS Code workflow, and coexists cleanly with Prettier as a separate formatting concern.

- Decision: enforce UI coverage with a dedicated `npm run test:coverage` command that runs `ng test --watch=false --coverage`, using Angular's supported coverage configuration surface in `angular.json`.
  - Rationale: Angular 22's default unit-test path is Vitest-backed and Angular documents coverage through `ng test --coverage` plus builder-level coverage settings. Keeping the configuration in `angular.json` is more supportable than introducing an early custom Vitest config, and `--watch=false` gives contributors and CI the same deterministic command shape.

- Decision: start the UI coverage gate with global 90% thresholds for statements, functions, lines, and branches, then lower them later only if real implementation or tooling friction proves that necessary.
  - Rationale: the team prefers to set a high initial quality bar and relax it only if the baseline proves impractical. Using global thresholds keeps the first CI contract simpler than per-file enforcement while still making "passing" explicit from the start.

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
   - Status: Completed on 2026-06-25 after aligning `UI/AI_PROJECT_GUIDE.md` with the routed shell baseline and updated contributor workflow.
   - Ensure docs, tests, and shell structure all describe the same baseline and do not reference removed scaffold content.

7. Confirm the UI CI baseline and decision points
   - Status: Completed on 2026-06-25 after reviewing the existing root GitHub Actions workflow, current UI scripts, and the Angular/Vitest linting and coverage options available to the project.
   - Confirm the implementation starts from an API-only root workflow, a UI workspace without lint scripts, and a UI workspace without a working coverage command until the required coverage provider is installed.

8. Define the UI quality-gate contract
   - Status: Completed on 2026-06-25 after settling the linting and coverage decisions for Milestone 7.
   - Enforce formatting separately from linting, require CI to run a non-mutating format-check command that fails when files would be reformatted, use `angular-eslint` through `npm run lint`, keep build and test commands aligned with Angular CLI defaults, and add a dedicated `npm run test:coverage` command with initial global 90% thresholds for statements, functions, lines, and branches.

9. Split the repository workflows
   - Status: Completed on 2026-06-25 after replacing the generic root workflow with explicit `api-ci` and `ui-ci` workflow files and validating the workflow diff with `git diff --check -- .github/workflows`.
   - Replace the generic root workflow arrangement with explicit `api-ci` and `ui-ci` workflows that can run independently.

10. Implement the UI workflow automation
   - Status: Completed on 2026-06-25 after adding the `ui-ci` GitHub Actions workflow, wiring path-based `push` and `pull_request` triggers for `UI/`, adding stable UI validation scripts, and validating `npm run format:check`, `npm run lint`, `npm run build`, `npm run test:ci`, and `npm run test:coverage`.
   - Update the repository workflow configuration so pull requests and relevant pushes run the agreed UI checks from the `UI/` workspace.

11. Document CI reproduction and expectations
   - Status: Completed on 2026-06-25 after documenting the `api-ci` and `ui-ci` workflow split, the local `ui-ci` reproduction commands, the non-mutating `format:check` gate, and the initial 90% global UI coverage thresholds in `UI/README.md` with a repository-level pointer in `README.md`.
   - Update contributor-facing docs so CI failures can be reproduced locally and the coverage/lint expectations are discoverable.

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

- Risk: the roadmap now requires CI quality gates, but the implementation could stop after local scripts and docs.
  - Mitigation: treat GitHub Actions workflow changes as an explicit deliverable and keep the completion checklist tied to automation, not only local validation.

- Risk: lint and coverage gates could be added inconsistently or with unclear thresholds, causing noisy CI failures and team churn.
  - Mitigation: record the chosen commands, thresholds, and rationale in the implementation itself and in the contributor docs at the same time.

- Risk: extending the existing root workflow could unintentionally inherit API-specific defaults such as `working-directory: API`.
  - Mitigation: explicitly scope UI jobs or steps to `UI/` and validate that the workflow behaves correctly for UI-only changes.

- Risk: splitting the workflows could confuse branch protection or check naming if the new workflow names are not stable.
  - Mitigation: standardize on explicit `api-ci` and `ui-ci` workflow names in both implementation and documentation.

## 10. Unknowns and Required Clarifications

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
- [x] Any new or clarified UI structural conventions are reflected in the appropriate UI guidance file.
- [x] The repository CI is split into explicit `api-ci` and `ui-ci` workflows that can run and report independently.
- [x] The `ui-ci` workflow runs the agreed format, lint, build, test, and coverage gates from the `UI/` workspace.
- [x] The UI project exposes stable npm commands for each CI gate, including a non-mutating `format:check` command plus any newly required lint and coverage commands.
- [x] Contributor documentation explains how to reproduce the UI CI checks locally and how to interpret the initial 90% global UI coverage thresholds and any related reporting output.
- [x] Final review confirms the milestone delivers both a clean Angular starting point and the required repository-level UI quality gates.
