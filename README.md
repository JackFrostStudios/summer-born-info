# Summer-born Infor

This repository contains the Summer-born Info platform split into separate API and UI services.

## Repository map

- [`API/`](./API/) - .NET backend, local infrastructure, and automated tests. Start with [API/README.md](./API/README.md).
- [`UI/`](./UI/) - Angular frontend application. Start with [UI/README.md](./UI/README.md).
- [`Documentation/`](./Documentation/) - shared repository documentation, including [CI workflow guidance](./Documentation/ci-workflows.md).
- [`Plans/`](./Plans/) - delivery-ready implementation plans.
- [`Roadmap/`](./Roadmap/) - higher-level roadmap and initiative planning.
- [`AI_PROJECT_GUIDE.md`](./AI_PROJECT_GUIDE.md) - top-level repository boundaries for AI helpers and contributors.
- [`AGENTS.md`](./AGENTS.md) - routing instructions for repo-aware agents.

## Where to go next

- Working on backend behaviour or infrastructure: [API/README.md](./API/README.md)
- Working on frontend behaviour or styling: [UI/README.md](./UI/README.md)
- Figuring out which part of the repo owns a change: [AI_PROJECT_GUIDE.md](./AI_PROJECT_GUIDE.md)
- Updating implementation plans: [Plans/AGENTS.md](./Plans/AGENTS.md)
- Updating roadmap documents: [Roadmap/AGENTS.md](./Roadmap/AGENTS.md)

## CI Workflows

Repository checks are split across `.github/workflows/api-ci.yml` for API validation and `.github/workflows/ui-ci.yml` for UI validation. For the shared workflow overview, trigger behavior, and artifact expectations, use [Documentation/ci-workflows.md](./Documentation/ci-workflows.md). For the exact UI commands and coverage expectations needed to reproduce a `ui-ci` failure locally, use [UI/README.md](./UI/README.md).

The UI also documents its Angular localization workflow there, including how to mark user-facing template strings, refresh `UI/src/locale/messages.xlf`, and run the localized-build validation commands.

## License

This project is licensed under the GNU General Public License. See [LICENSE](./LICENSE).
