# PR 7 Review Comments Resolution Plan

## 1. Overview

Resolve the actionable PR #7 review comments captured in [PR-7-review-comments.md](../PR-7-review-comments.md) by applying the suggested solutions across GitHub Actions workflows, Angular workspace conventions, and UI documentation. The goal is to make the CI configuration leaner, align the UI workspace with the repository's preferred naming and locale conventions, and move cross-repository CI guidance into the existing root `Documentation/` area while preserving a concise UI-specific contributor guide.

## 2. Roadmap Source or Existing Plan Context

- Source review artifact: [PR-7-review-comments.md](../PR-7-review-comments.md)
- Affected repository areas:
  - [`.github/workflows/api-ci.yml`](../.github/workflows/api-ci.yml)
  - [`.github/workflows/ui-ci.yml`](../.github/workflows/ui-ci.yml)
  - [`UI/.vscode/extensions.json`](../UI/.vscode/extensions.json)
  - [`UI/eslint.config.js`](../UI/eslint.config.js)
  - [`UI/angular.json`](../UI/angular.json)
  - [`UI/package.json`](../UI/package.json)
  - [`UI/README.md`](../UI/README.md)
  - `UI/src/**` selectors and Angular bootstrap markup
  - [`README.md`](../README.md)
  - `Documentation/` root documentation
- Relevant repo guidance:
  - [Plans/AGENTS.md](./AGENTS.md)
  - [UI/AGENTS.md](../UI/AGENTS.md)
  - [AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md)

## 3. Scope

- Remove unnecessary full-history checkout from the API and UI validation jobs where the downstream steps do not require complete Git history.
- Align the Angular UI workspace with the requested `sbi` selector prefix.
- Add the Node version constraint requested in review via `package.json` `engines`.
- Switch the UI source locale guidance and configuration from `en-US` to `en-GB`.
- Add the missing VS Code Prettier recommendation.
- Move the long-form CI workflow documentation out of `UI/README.md` into the shared root documentation area, then leave a shorter UI summary that links to the shared document.

## 4. Non-Goals

- Redesigning the overall GitHub Actions workflow structure beyond the requested `fetch-depth` cleanup.
- Introducing a full Node version manager strategy such as Volta, `.nvmrc`, or `.node-version`.
- Changing UI behavior, styling, routing, or API integration beyond what is required to support the selector-prefix and locale convention updates.
- Reworking repository-wide documentation structure beyond the specific CI content extraction needed for this PR feedback.

## 5. Behaviour Scenarios

### Scenario: CI runs for API changes without full-history checkout in the validation job

Given a pull request or push that triggers `api-ci`, when the `api-ci` job checks out repository contents for restore/build/test/report steps, then the job should use the default shallow checkout because no later step depends on full Git history.

### Scenario: CI runs for UI changes without full-history checkout in the validation job

Given a pull request or push that triggers `ui-ci`, when the `ui-ci` job checks out repository contents for install/build/lint/test/report steps, then the job should use the default shallow checkout because the later steps operate only on the checked-out workspace contents.

### Scenario: Angular contributors generate or lint components after the prefix change

Given a contributor scaffolding or reviewing Angular components, when selector conventions are applied, then Angular configuration, ESLint rules, and existing component selectors should consistently use the `sbi` prefix rather than `app`, and the application bootstrap markup should reference the updated root selector.

### Scenario: Contributors follow local setup guidance

Given a developer opening the UI workspace in VS Code, when they inspect recommended extensions and setup instructions, then they should see both Angular Language Service and Prettier recommendations, along with a documented Node compatibility constraint that matches `package.json`.

### Scenario: Localization settings follow UK English

Given the Angular build and i18n extraction workflow, when source locale configuration is read from Angular config, extracted message metadata, and contributor documentation, then the source locale should be represented as `en-GB` consistently rather than `en-US` or the incorrect `en-UK`.

### Scenario: Contributors need CI troubleshooting guidance

Given a contributor reading repository docs to understand CI behavior, when they need the shared policy and command details for API/UI workflows, then the canonical long-form explanation should live in the root `Documentation/` folder, while `UI/README.md` should retain only UI-specific summary guidance and a link to the shared document.

## 6. Deliverables

1. Update `.github/workflows/api-ci.yml` so the `api-ci` job checkout step no longer sets `fetch-depth: 0`, while leaving the `changes` job untouched because path filtering may still require history.
2. Update `.github/workflows/ui-ci.yml` so the `ui-ci` job checkout step no longer sets `fetch-depth: 0`, while leaving the `changes` job untouched for the same path-filter reason.
3. Update `UI/.vscode/extensions.json` to recommend `esbenp.prettier-vscode` alongside `angular.ng-template`.
4. Update `UI/angular.json`, `UI/eslint.config.js`, and all existing Angular selectors/bootstrap references under `UI/src/` so the workspace convention is `sbi` instead of `app`.
5. Update `UI/package.json` to declare the supported Node range in `engines`, matching the documented Angular 22 compatibility already described in the README unless implementation uncovers a stricter tested range already enforced elsewhere.
6. Update Angular localization configuration and generated/localized metadata from `en-US` to `en-GB`, including `UI/angular.json`, `UI/README.md`, and any tracked generated files such as `UI/src/locale/messages.xlf` that embed the source locale.
7. Create or update a root documentation page in `Documentation/` for shared CI workflow guidance, then trim `UI/README.md` to a shorter UI-focused CI section that links to the shared document instead of duplicating the full explanation.
8. Update root-level repository documentation only as needed so the new shared CI document is discoverable from `README.md` if the extracted content would otherwise be hard to find.

## 7. Technology Requirements and Decisions

- Documentation destination: use the existing root `Documentation/` folder rather than inventing a new top-level docs location, because the repository already has a shared documentation surface there.
- Node version pinning approach: implement the review request as a `package.json` `engines.node` constraint rather than exact binary pinning, consistent with the suggested solution and the current toolchain setup.
- Angular prefix convention: treat the `sbi` change as a workspace convention update, which requires touching configuration and existing selectors together to avoid lint/build inconsistency.
- Locale convention: use `en-GB` as the valid UK English locale identifier. Do not implement the review comment literally as `en-UK`.
- Workflow history retention: keep `fetch-depth: 0` only where Git history is demonstrably required. Based on current workflow contents, that applies to the path-filter `changes` jobs, not the build/test jobs.

## 8. Dependencies and Sequencing

1. Confirm all `app` selector usages and locale references in `UI/src/` and tracked config files so the convention changes are complete.
2. Apply workflow checkout cleanup first because it is isolated and low risk.
3. Apply Angular prefix changes across config and source files in one slice to avoid temporary lint/build failures caused by mixed selector conventions.
4. Apply locale and Node metadata updates next, including regeneration or refresh of any tracked localization artifact affected by the locale change.
5. Move CI documentation last so the final docs can reference the already-updated workflow names, commands, locale wording, and contributor expectations.
6. Run targeted verification after code/config changes, then perform documentation consistency review before closing the work.

## 9. Risks and Mitigations

- Risk: Removing `fetch-depth: 0` from the wrong checkout step could break path filtering.
  - Mitigation: limit the change to the second checkout in each workflow (`api-ci` and `ui-ci` jobs), and leave the `changes` jobs as-is unless validation proves shallow history is still sufficient there.
- Risk: The `sbi` prefix change could miss root bootstrap markup or component selectors, causing runtime bootstrap or lint failures.
  - Mitigation: update selectors, `index.html`, and workspace config together, then run lint/build tests.
- Risk: Changing source locale may invalidate the tracked extracted messages file.
  - Mitigation: rerun the i18n validation flow and include any required `messages.xlf` metadata updates in the same change.
- Risk: Moving CI docs could make contributor guidance harder to discover.
  - Mitigation: leave a concise summary and explicit link in `UI/README.md`, and add discoverability from root docs if needed.
- Risk: `engines.node` could be set to a range inconsistent with the currently tested CI runtime.
  - Mitigation: align the range with the documented Angular compatibility and confirm it does not contradict the `actions/setup-node` configuration.

## 10. Unknowns and Required Clarifications

- No blocking product decisions are currently open because the review artifact already includes suggested solutions for each actionable comment.
- During implementation, confirm whether the shared CI document should be a new dedicated file such as `Documentation/ci-workflows.md` or an update to an existing root documentation page. Either option is acceptable as long as the content lives under `Documentation/` and is clearly linked.

## 11. Completion Checklist

- [x] `api-ci` validation job checkout no longer uses `fetch-depth: 0`.
- [x] `ui-ci` validation job checkout no longer uses `fetch-depth: 0`.
- [x] `UI/.vscode/extensions.json` recommends both Angular Language Service and Prettier.
- [x] Angular prefix configuration is updated from `app` to `sbi` in workspace config and lint rules.
- [x] Existing Angular selectors/bootstrap references are updated to match the `sbi` prefix.
- [x] `UI/package.json` includes a Node `engines` constraint.
- [x] UI locale configuration and documentation consistently use `en-GB`.
- [x] Any tracked localization artifact impacted by the locale change is refreshed and committed.
- [ ] Shared CI documentation lives under `Documentation/`.
- [ ] `UI/README.md` keeps a shorter UI-specific CI summary with a link to the shared doc.
- [ ] Documentation links remain accurate from root and UI entry points.
- [ ] Validation is completed for the touched surfaces.
- [ ] `npm run lint` passes in `UI/`.
- [ ] `npm run build` passes in `UI/`.
- [ ] `npm run validate:i18n` passes in `UI/`.
- [ ] `npm run test:ci` passes in `UI/`.
