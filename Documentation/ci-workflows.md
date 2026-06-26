# CI Workflows

This document is the shared reference for repository CI workflow behavior.

## Workflow Split

Repository CI lives in two GitHub Actions workflow files under `.github/workflows/`:

- `api-ci.yml` validates the .NET backend from `API/`.
- `ui-ci.yml` validates the Angular frontend from `UI/`.

That split keeps API and UI validation independent while still surfacing both checks on pull requests and pushes to `main`.

## Trigger Behavior

- Both workflows run for pull requests targeting `main` and pushes to `main`.
- Each workflow has an initial `changes` job that checks whether its own area changed before the full validation job runs.
- `api-ci` performs full backend validation only when `API/**` or `.github/workflows/api-ci.yml` changed.
- `ui-ci` performs full frontend validation only when `UI/**` or `.github/workflows/ui-ci.yml` changed.
- When a workflow's area did not change, the workflow still completes with a skip message so the check remains visible instead of disappearing entirely.
- `ui-ci` also supports `workflow_dispatch` for an intentional full UI validation run.

## Contributor Expectations

- Treat the checked-in workflow files as the source of truth for CI behavior.
- Reproduce failures from the owning service folder so local commands match the workspace the workflow uses.
- Keep service-specific command details in the service README rather than duplicating them across multiple repository docs.

## Local Reproduction

- For UI validation and `ui-ci` troubleshooting, use [UI/README.md](../UI/README.md#ci-workflows).
- For API validation, use [API/README.md](../API/README.md).

## Coverage And Artifacts

- `ui-ci` uploads the `UI/coverage/` directory as the `ui-coverage` artifact and also emits machine-readable test results from `UI/test-results/`.
- `api-ci` uploads the merged Cobertura report from `API/coverage-results/coverage.xml`.

Use those workflow artifacts when a pull request needs deeper inspection than the job summary provides.
