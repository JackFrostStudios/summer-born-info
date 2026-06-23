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

For an incremental development build:

```bash
npm run watch
```

The current Angular build uses the application builder and produces browser and server output in `dist/summer-born-info/`.

## Test

Run unit tests from the `UI` folder:

```bash
npm test
```

The current test target is Angular's unit-test builder and the project currently includes scaffold-level component tests only.

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
