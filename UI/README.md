# UI

This folder contains the Summer Born Information Angular frontend application.

## Project Aims

The UI is the browser-facing surface for the Summer Born Information platform.

- Present user journeys for parents and guardians.
- Consume backend API capabilities once those contracts are wired into the frontend.
- Provide a clear place for future client-side routing, state, styling, and accessibility work.

## Prerequisites

- Node.js supported by the current Angular 22 toolchain. At the time of writing, Angular 22.0.x supports `^22.22.3 || ^24.15.0 || ^26.0.0`.
- npm 11.x (the project is pinned to `npm@11.1.0` in `package.json`)

## Install

Run from the `UI` folder:

```bash
npm install
```

## Run Locally

Start the Angular development server from the `UI` folder:

```bash
npm start
```

This runs `ng serve` with the development configuration from `angular.json`. By default the app is available at `http://localhost:4200/`.

To run the generated SSR output after a build:

```bash
npm run serve:ssr:summer-born-info
```

Run that only after `npm run build`, because it serves the built server bundle from `dist/summer-born-info/server/`.

## Build

Create a production build:

```bash
npm run build
```

Validate the localized production build path that CI enforces:

```bash
npm run build:localize
```

For an incremental development build:

```bash
npm run watch
```

The current Angular build uses the application builder and produces browser and server output in `dist/summer-born-info/`.

## Validation Baseline

The UI workspace now treats strict compilation and strict Angular linting as the default contributor baseline.

- `npm run build` enforces strict TypeScript and strict Angular template validation.
- `npm run lint` runs the type-aware Angular ESLint configuration for TypeScript and templates.
- Visible user-facing strings in Angular templates must be marked with Angular `i18n` metadata so new UI text enters the localization workflow when it is introduced.

For example:

```html
<h1 i18n="Home page heading|Introduces the page to parents and guardians@@homePageHeading">
  Find summer born information
</h1>
```

Use a meaningful description when it helps translators or reviewers, and add a custom message ID only when the template already has a stable reason to do so.

## Localization Workflow

The UI uses the standard Angular `@angular/localize` workflow with `en-GB` as the source locale.

- Source-locale extraction output lives at `UI/src/locale/messages.xlf`.
- Refresh extracted messages after adding, removing, or changing marked user-facing template strings.
- Use Angular template `i18n` metadata for visible text in templates instead of inventing a separate project-specific translation layer.

Commands from the `UI/` folder:

```bash
npm run extract:i18n
npm run build:localize
npm run validate:i18n
```

What they do:

- `npm run extract:i18n` refreshes `src/locale/messages.xlf`.
- `npm run build:localize` runs the production localized build path with missing translations treated as errors.
- `npm run validate:i18n` runs extraction, fails if that would leave `src/locale/messages.xlf` changed compared with Git, and then runs the localized build, which matches the dedicated i18n validation step in CI.

If `npm run validate:i18n` fails because `src/locale/messages.xlf` changed, rerun `npm run extract:i18n`, review the updated message file, and include it with your UI change.

## Test

Run unit tests from the `UI` folder:

```bash
npm test
```

The current test target is Angular's unit-test builder and the project currently includes focused tests around the baseline app shell and routing setup.

For single-pass local validation commands:

```bash
npm run test:run
npm run test:coverage
npm run test:ci
```

- `npm run test:run` runs the unit tests once without watch mode.
- `npm run test:coverage` runs the unit tests once with coverage and report output.
- `npm run test:ci` is the CI-facing alias and currently matches `npm run test:coverage`.

## Development Workflow

Use VS Code as the baseline editor for day-to-day UI work in this repository. It lines up with the checked-in workspace tasks and launch settings under `UI/.vscode/`, which already include `npm start` and `npm test` tasks plus Chrome launch profiles for `ng serve` and `ng test`.

Recommended minimum extensions:

- Angular Language Service (`angular.ng-template`) to match the workspace recommendation in `UI/.vscode/extensions.json`.
- Prettier (`esbenp.prettier-vscode`) for consistent formatting against `UI/.prettierrc`.

Recommended local loop from the `UI` folder:

1. Install dependencies when you first clone the repo or when `package-lock.json` changes.

   ```bash
   npm install
   ```

2. Start the Angular dev server for normal UI iteration.

   ```bash
   npm start
   ```

3. Run the unit tests while you work or before you hand changes over.

   ```bash
   npm run test:run
   ```

4. Check formatting before you commit. If Prettier reports changes, rerun the fix-up command.

   ```bash
   npm run format:check
   ```

5. Run lint and i18n validation before you hand over UI changes that touched TypeScript, templates, or user-facing text.

   ```bash
   npm run lint
   npm run validate:i18n
   ```

6. Inspect the running app in a browser at `http://localhost:4200/`.

   Use any browser you prefer, but a Chromium-based browser such as Chrome or Edge is the most practical default here because the Angular DevTools extension is readily available there for routine component-tree and change-detection inspection. If you use VS Code, the checked-in launch profiles are also set up around that browser family.

## CI Workflows

Repository CI now lives in two separate GitHub Actions workflow files under `.github/workflows/`:

- `api-ci.yml` validates the .NET backend from `API/`.
- `ui-ci.yml` validates the Angular frontend from `UI/`.

That split lets API and UI changes report independently on the same pull request while still keeping both checks visible for branch protection on pull requests and pushes to `main`.

Trigger behavior:

- Both workflows trigger for pull requests targeting `main` and pushes to `main`, so `api-ci` and `ui-ci` still appear predictably as separate checks.
- Each workflow detects whether its own area changed before doing the expensive work.
- `api-ci` runs the full backend validation only when `API/**` or `.github/workflows/api-ci.yml` changed.
- `ui-ci` runs the full frontend validation only when `UI/**` or `.github/workflows/ui-ci.yml` changed.
- When a workflow's area did not change, the workflow exits quickly with a skip message instead of disappearing entirely.
- `ui-ci` still supports `workflow_dispatch` for a deliberate full UI validation run.

If `ui-ci` runs its full validation and fails, reproduce it from the `UI/` folder with the same commands CI runs:

```bash
npm ci
npm run format:check
npm run lint
npm run build
npm run validate:i18n
npm run test:ci
```

What each UI command enforces:

- `npm run format:check` is the CI-safe, non-mutating Prettier gate. It fails when files are not formatted but does not rewrite them.
- `npm run format` is the local fix-up command that applies Prettier changes.
- `npm run lint` runs the type-aware Angular ESLint rules for TypeScript and templates, including the stricter Angular baseline and template i18n checks.
- `npm run build` verifies the application still compiles under strict TypeScript and strict Angular template validation.
- `npm run validate:i18n` refreshes the extracted source messages file, fails if the tracked `src/locale/messages.xlf` file was stale, and verifies the localized production build path.
- `npm run test:ci` runs the unit tests once with coverage enabled and enforces the current global 90% thresholds for statements, branches, functions, and lines.

Coverage reporting expectations:

- Local coverage output is written under `UI/coverage/summer-born-info/`.
- The coverage run emits a text summary in the terminal and also writes HTML, `lcov`, and `cobertura` outputs under `UI/coverage/summer-born-info/` for deeper inspection.
- The coverage-producing CI run also writes machine-readable test outputs to `UI/test-results/junit.xml` and `UI/test-results/results.json`.
- In GitHub Actions, `ui-ci` uploads the full `UI/coverage/` directory as the `ui-coverage` artifact, so reviewers can inspect the generated report when the workflow completes.

## Current API Relationship

The UI and API live in the same repository, but they are not yet wired into a documented end-to-end local startup flow.

- There is no shared root command that starts both services together.
- The UI does not yet define a repository-standard API client, proxy, or environment-based backend URL convention.
- Treat current frontend-to-backend integration as a gap to be implemented explicitly when UI features begin consuming live API endpoints.

Until that wiring exists, document and build UI changes as a standalone Angular application and describe any new API assumptions in the related plan or feature change.

## Architecture And Workflow Guidance

Keep this README focused on setup and commands. Use the deeper UI guidance files when you need structure or delivery rules:

- [AGENTS.md](./AGENTS.md) for UI workflow expectations and coding guidance.
- [AI_PROJECT_GUIDE.md](./AI_PROJECT_GUIDE.md) for UI structure, file ownership, and testing/layout conventions.
- [../AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md) for top-level repository boundaries.
