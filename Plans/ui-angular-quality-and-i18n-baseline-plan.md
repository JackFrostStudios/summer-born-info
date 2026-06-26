# UI Angular Quality And i18n Baseline Plan

## 1. Overview

Raise the new Angular workspace baseline in `UI/` from "works locally" to "policy-enforced and localization-ready" before more feature code lands.

This follow-up bundles four related improvements from [review.md](C:/Projects/summer-born-info/review.md): compiler strictness, stricter Angular and TypeScript linting, more useful UI CI reporting, and an Angular i18n baseline that is enforced in code, build scripts, CI, and contributor documentation.

The goal is to make the repository's stated UI standards executable now, while the codebase is still small enough for these guardrails to be added with limited churn.

## 2. Roadmap Source or Existing Plan Context

- Review source: [review.md](C:/Projects/summer-born-info/review.md)
  - P1: enable full TypeScript and Angular strictness.
  - P2: move ESLint to a stricter Angular and type-aware baseline, including i18n-related rules.
  - P3: stop running the UI test suite twice in CI and publish more actionable test and coverage feedback.
  - P4: establish an Angular i18n-ready baseline now, not after templates have grown.
- Existing baseline plan: [Plans/milestone-7-initial-angular-app-plan.md](C:/Projects/summer-born-info/Plans/milestone-7-initial-angular-app-plan.md)
  - That plan established the initial Angular workspace, UI CI split, and baseline lint/test/build scripts.
  - This follow-up plan strengthens those foundations rather than replacing them.
- Current repository state confirmed on 2026-06-26:
  - `UI/tsconfig.json` enables several targeted checks but not `strict`, `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`, or `angularCompilerOptions.strictTemplates`.
  - `UI/eslint.config.js` uses non-type-aware `typescript-eslint` presets plus Angular recommended presets, not the stricter "all"/type-checked posture described in the review.
  - `.github/workflows/ui-ci.yml` runs `npm run test:ci` and `npm run test:coverage`, causing duplicate unit-test execution and only uploading the coverage directory as an artifact.
  - `UI/package.json` and `UI/angular.json` do not yet expose a dedicated i18n extraction command or localized build command.
  - `UI/README.md` and `UI/AI_PROJECT_GUIDE.md` describe the current UI workflow but do not yet document localization conventions, extraction flow, or localized build expectations.

## 3. Scope

- Enable full TypeScript strict mode and Angular strict template checking for the UI workspace.
- Upgrade ESLint to a type-aware, Angular-strict baseline that enforces repo policy and translation readiness.
- Introduce Angular localize support and standardize the project's source-locale workflow, extraction path, and localized build commands.
- Improve UI CI so tests run once, test and coverage results are published in a reviewer-friendly format, and i18n-related checks are part of the enforced workflow.
- Document the resulting contributor workflow, including how to mark strings for translation, extract messages, and validate localized output.

## 4. Non-Goals

- Translating the application into non-English locales in this slice.
- Designing locale switching UX, language negotiation, or locale persistence.
- Adding frontend-to-backend localization contracts or API-delivered translation content.
- Reworking the UI architecture beyond what is necessary to support stricter checks and localization readiness.
- Replacing the current Angular builder, unit-test runner, or SSR setup unless a concrete compatibility issue forces a narrowly scoped change.

## 5. Behaviour Scenarios

### Scenario: Type and template mistakes are blocked during local development

Given a developer introduces a nullability error, unsafe indexed access, or template/property mismatch  
When they run `npm run build`, `npm test`, or the relevant editor diagnostics update  
Then the workspace should fail with strict TypeScript or Angular template errors before the change reaches runtime.

### Scenario: Angular and template best practices are enforced consistently

Given a developer adds a new component, template, or signal-based interaction  
When they run `npm run lint` or CI runs the UI validation workflow  
Then type-aware TypeScript rules, Angular strict rules, template accessibility rules, and i18n rules should all evaluate the change and fail clearly when repo policy is violated.

### Scenario: User-facing text is added to a template

Given a developer adds new visible text in an Angular template  
When they run `npm run lint`  
Then the change should fail unless the text is marked using the agreed Angular i18n convention so translation readiness is enforced from the start.

### Scenario: Contributors extract source messages for translators or review

Given the source locale remains English  
When a contributor runs the documented i18n extraction command from `UI/`  
Then Angular should generate or refresh the source messages file in the agreed location without requiring custom tooling or ad hoc manual steps.

### Scenario: CI validates the localization baseline

Given a pull request changes UI files or UI workflow files  
When `ui-ci` runs in GitHub Actions  
Then it should execute the single agreed test run, publish test-result and coverage outputs in a readable form, and fail if lint, strict build, coverage, or localization-related rules do not pass.

### Scenario: Contributors need to confirm localized builds still compile

Given Angular localize support has been added and English is the source locale  
When a contributor runs the documented localized build command  
Then the project should produce the expected build output using the standard Angular localization path, even if only the source locale is currently shipped.

### Scenario: A new contributor needs to follow the localization convention

Given a contributor is unfamiliar with the UI's i18n setup  
When they read `UI/README.md` and `UI/AI_PROJECT_GUIDE.md`  
Then they should understand where visible strings belong, how to mark them, how to extract messages, which commands CI enforces, and where future locale files and localized build settings live.

## 6. Deliverables

1. Strict compiler baseline
   - Update `UI/tsconfig.json` so `compilerOptions` enables `strict`, `noUncheckedIndexedAccess`, and `exactOptionalPropertyTypes`.
   - Enable `angularCompilerOptions.strictTemplates` while retaining the existing strict Angular compiler options already in place.
   - Fix any resulting code or test issues so the current app, route shell, and unit tests pass under the stricter compiler baseline.

2. Type-aware Angular ESLint baseline
   - Rework `UI/eslint.config.js` to use `typescript-eslint` type-checked presets and enable `parserOptions.projectService: true`.
   - Move the Angular rule baseline to the strict "all" posture for TypeScript and templates, while preserving `templateAccessibility`.
   - Keep only a small explicit denylist for repo-approved exceptions identified in the review:
     - `@angular-eslint/prefer-on-push-component-change-detection`
     - `@angular-eslint/use-component-view-encapsulation`
     - `@angular-eslint/component-class-suffix`
     - `@angular-eslint/directive-class-suffix`
     - `@angular-eslint/no-developer-preview`
     - `@angular-eslint/no-experimental`
   - Keep i18n-related Angular lint rules enabled, including `@angular-eslint/require-localize-metadata`, `@angular-eslint/runtime-localize`, and `@angular-eslint/template/i18n`.
   - Adjust existing code or tests to comply with the stricter lint rules rather than weakening the rules unless a documented repo-policy exception is required.

3. Angular i18n baseline implementation
   - Add Angular localization support to the workspace using the standard `@angular/localize` package and Angular CLI conventions.
   - Configure `UI/angular.json` with an explicit `i18n` section that treats English as the source locale and points to the initial extraction output file location.
   - Choose and document a canonical extraction target under `UI/src/locale/` or another nearby UI-owned path so the messages file has a stable home.
   - Add stable npm scripts for at least:
     - message extraction,
     - a localized production build path,
     - and any supporting validation command needed for CI or contributor workflow clarity.
   - Confirm SSR or server output still builds correctly after localize support is introduced, or document and address any Angular-localize-specific build adjustments required by the current SSR application builder.

4. CI reporting and enforcement improvements
   - Update `.github/workflows/ui-ci.yml` so the UI unit suite runs once in a coverage-producing mode instead of separate `test:ci` and `test:coverage` runs.
   - Publish machine-readable UI test results in GitHub Actions so failures are visible without downloading raw artifacts.
   - Publish a Cobertura coverage summary in the workflow, matching the API review experience as closely as the Angular toolchain allows.
   - Keep the existing path-filter behavior so the workflow still skips quickly when UI files have not changed.
   - Ensure the workflow enforces the i18n-ready lint/build contract and any new extraction or localized-build validation command the implementation adopts.
   - Increase workflow permissions only if a chosen reporting action actually requires them, and document that reason in the workflow comments or plan implementation notes.

5. Documentation and contributor guidance
   - Update `UI/README.md` with:
     - the strictness and lint expectations,
     - the message-marking convention for user-facing strings,
     - the extraction command,
     - the localized build command,
     - and the revised CI reproduction commands.
   - Update `UI/AI_PROJECT_GUIDE.md` with any new structural convention for locale files, localized assets, or localization-related ownership boundaries.
   - If the localized-build and extraction workflow affects repo-level expectations, add a short pointer from the root `README.md` or other canonical repo docs only where it improves discoverability without duplicating UI-internal detail.

6. Validation updates
   - Update tests or fixtures if stricter compiler/lint/i18n enforcement changes rendered text, template markers, or helper code.
   - Validate the final slice locally with the documented UI commands, including lint, build, single-pass CI test execution, coverage generation, extraction, and any localized build command added by the change.

## 7. Technology Requirements and Decisions

- Decision: keep this as one coordinated follow-up plan instead of four unrelated changes.
  - Rationale: strictness, Angular lint posture, CI enforcement, and i18n readiness overlap heavily; splitting them would create duplicated config churn and increase the risk of partial policy enforcement.

- Decision: use Angular's standard localization stack centered on `@angular/localize`, Angular template i18n markers, CLI extraction, and Angular build-time localization support.
  - Rationale: the review explicitly calls for Angular localization readiness, and the standard Angular path is the most supportable option for a new Angular 22 workspace.

- Decision: treat English as the source locale and ship only that locale in this slice unless implementation uncovers a technical reason to add a second locale for proof.
  - Rationale: the user's requirement is readiness, build enforcement, and documentation, not translated content delivery.

- Decision: enforce localization readiness in lint and build automation, not just documentation.
  - Rationale: the review's concern is future drift; documentation alone would not stop untranslated or unmarked strings from accumulating.

- Decision: prefer one coverage-producing CI test execution with published reports over separate fast-test and coverage jobs.
  - Rationale: the current workflow duplicates work, and the review specifically calls for a single-pass workflow with better feedback.

- Decision: keep the TypeScript and Angular strictness posture high unless a concrete framework or generated-code issue forces a narrow exception.
  - Rationale: the workspace is still small, making this the cheapest time to adopt strong compiler guarantees.

## 8. Dependencies and Sequencing

1. Confirm the current baseline
   - Re-read the review findings, current UI config files, and current CI workflow so implementation starts from the observed repository state.

2. Enable strict compiler settings
   - Turn on the TypeScript and Angular strict options first.
   - Fix the resulting code, tests, or template issues before tightening lint rules so compiler feedback is handled in a controlled way.

3. Introduce the stricter ESLint baseline
   - Move ESLint to the type-aware and Angular-strict presets.
   - Apply only the documented repo-policy exceptions and fix any remaining violations in the current codebase.

4. Add Angular localize support and CLI conventions
   - Install and wire the standard localization package and config.
   - Define the source locale, extraction output location, and npm command surface before updating CI or docs.

5. Align build and validation scripts
   - Normalize `package.json` scripts so contributors and CI share the same commands for lint, extraction, localized build validation, and test/coverage execution.

6. Upgrade UI CI reporting
   - Replace the duplicated test steps with a single coverage-producing run.
   - Add test-result publication and coverage-summary publication.
   - Add any required extraction or localized-build validation step once the scripts are stable.

7. Update documentation
   - Document the localization workflow and stricter validation expectations after the commands and file locations are settled.

8. Perform end-to-end validation
   - Run the final local command set and confirm the plan's deliverables match both repository guidance and the review's recommended follow-up.

## 9. Risks and Mitigations

- Risk: strict mode or strict templates expose issues in generated SSR or test scaffolding that are not obvious from the current minimal app.
  - Mitigation: enable compiler strictness before lint escalation so failures are isolated and easier to fix.

- Risk: moving to Angular "all" lint presets produces noisy failures or conflicts with the repo's documented Angular conventions.
  - Mitigation: keep the exception list explicit and minimal, and record every rule disabled for policy reasons rather than weakening the overall preset selection.

- Risk: Angular localize support may require builder or SSR-specific adjustments that are easy to miss if only the default browser build is exercised.
  - Mitigation: include localized-build validation and SSR-aware verification in the implementation acceptance criteria.

- Risk: CI reporting improvements depend on third-party GitHub Actions that need extra permissions or produce brittle formatting.
  - Mitigation: choose maintained actions with minimal permissions, mirror the API workflow only where the toolchain supports it cleanly, and keep raw artifacts as a fallback rather than the only output.

- Risk: contributors may start treating extraction output as optional or forget when to refresh it.
  - Mitigation: document when extraction should be rerun, keep the command stable, and decide during implementation whether CI should validate extraction freshness directly or through a reproducible contributor workflow.

- Risk: i18n lint rules can create friction if the app mixes intentionally non-translatable strings, attribute values, or diagnostic text.
  - Mitigation: document the allowed exceptions and encode them as narrow lint suppressions or conventions instead of broad rule disablement.

## 10. Unknowns and Required Clarifications

- Assumption: one implementation plan should cover P1 through P4 because the user asked for "the recommended follow-up" from the review and specifically highlighted i18n readiness.
- Assumption: shipping English as the only locale in this slice is sufficient as long as the Angular localize workflow, extraction path, build steps, and CI enforcement are all in place.
- Clarification to settle during implementation if needed:
  - whether CI should fail when extracted message files are stale compared with templates, or whether extraction should remain a documented/manual contributor step until a second locale exists.
  - which GitHub Actions the repository wants to standardize on for UI test-result and Cobertura-summary publication, if the API workflow already establishes a preferred reporting action.

## 11. Completion Checklist

- [x] `UI/tsconfig.json` enables `strict`, `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`, and `angularCompilerOptions.strictTemplates`.
- [x] The existing UI app, templates, and tests pass under the stricter compiler configuration.
- [x] `UI/eslint.config.js` uses type-aware `typescript-eslint` presets with `projectService: true`.
- [x] Angular TypeScript, template, accessibility, and i18n lint rules are enforced from a strict baseline with only the documented repo-policy exceptions disabled.
- [x] The UI workspace includes standard Angular localize support with English configured as the source locale.
- [x] `UI/angular.json` and related files define a stable extraction output location and localized build path.
- [x] `UI/package.json` exposes stable scripts for extraction, localized build validation, and the revised single-pass CI test workflow.
- [x] `.github/workflows/ui-ci.yml` runs the UI tests once, publishes test results, and publishes a readable coverage summary in addition to retaining any useful artifacts.
- [x] The UI CI workflow enforces the finalized i18n-ready lint/build contract.
- [x] `UI/README.md` documents the localization workflow, revised CI reproduction commands, and the contributor expectations for marking translatable strings.
- [x] `UI/AI_PROJECT_GUIDE.md` records any new ownership rule for locale files or localization-related structure.
- [x] Final validation confirms lint, build, extraction, localized build, tests, and coverage all pass from the `UI/` workspace.
