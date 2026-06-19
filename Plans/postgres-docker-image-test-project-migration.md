# Postgres Docker Image Test Project Migration

## Overview

Move the Postgres Docker image tests out of `API/SummerBornInfo.Infrastructure.Tests` into a dedicated `API/SummerbornInfo.PostgresDockerImage.Tests` project, then remove references and friend assembly declarations that no longer match the new ownership.

## Source Context

- User request on 2026-06-19 to extract the tests into their own project.
- Additional requirement: confirm `InternalsVisibleTo` is configured at the correct project level after the move.

## Scope

- Create a new `SummerbornInfo.PostgresDockerImage.Tests` test project.
- Move the Postgres Docker image tests into the new project.
- Add the new test project to the API solution.
- Update project references and test assets so the new project builds independently.
- Review `InternalsVisibleTo` declarations affected by the split and align them with the owning test project.
- Remove references or configuration from `SummerBornInfo.Infrastructure.Tests` that are no longer needed because of the move.
- Update `API/AI_PROJECT_GUIDE.md` to reflect the new project.

## Non-Goals

- Refactoring unrelated infrastructure tests.
- Changing Docker image runtime behavior.
- Reorganising other test projects beyond the references touched by this migration.

## Behaviour Scenarios

### Scenario 1

Given the solution contains a dedicated Postgres Docker image library, when the test suite is organised by project ownership, then the Docker image tests live in `SummerbornInfo.PostgresDockerImage.Tests` instead of `SummerBornInfo.Infrastructure.Tests`.

### Scenario 2

Given internal APIs are exercised by tests, when the new test project builds, then only the owning production project grants internals access to the matching test project and stale friend assembly declarations are removed.

### Scenario 3

Given a developer runs the affected tests from the `API` folder, when the migration is complete, then the new test project is included in the solution and its tests pass without depending on the infrastructure test project.

## Deliverables

1. A new `API/SummerbornInfo.PostgresDockerImage.Tests` project with the required test SDK packages, runner configuration, and project references.
2. The migrated `PostgreSqlDockerImageVersionTests` file under the new test project namespace.
3. Updated solution and project metadata, including any required `InternalsVisibleTo` changes.
4. Cleanup of obsolete references or files in `SummerBornInfo.Infrastructure.Tests`.
5. Validation that the new test project runs successfully from the `API` folder.

## Technology Decisions

- Follow the existing `API/SummerBornInfo.*.Tests` project pattern for test project structure and xUnit runner configuration.
- Keep the change narrow: only add references that the migrated tests require.
- Treat `InternalsVisibleTo` as a per-production-project concern rather than a shared infrastructure test concern.

## Dependencies And Sequencing

1. Create the new test project and migrate the test file.
2. Update solution membership and friend assembly configuration.
3. Remove obsolete references and assets from the infrastructure test project.
4. Run focused test validation.

## Risks And Mitigations

- Risk: the moved tests may rely on transitive references currently supplied by `Infrastructure.Tests`.
  - Mitigation: add only the explicit project/package references required by the migrated test.
- Risk: stale `InternalsVisibleTo` declarations leave the new project unable to access internal members or grant unnecessary access to the old project.
  - Mitigation: inspect each affected production project assembly info file and align it with actual test ownership.

## Unknowns

- None currently identified for this narrow migration.

## Completion Checklist

- [x] `SummerbornInfo.PostgresDockerImage.Tests` exists and is added to the solution.
- [x] Postgres Docker image tests no longer live in `SummerBornInfo.Infrastructure.Tests`.
- [x] `InternalsVisibleTo` declarations are correct for all affected projects after the move.
- [x] Obsolete references/configuration are removed from `SummerBornInfo.Infrastructure.Tests`.
- [x] `API/AI_PROJECT_GUIDE.md` reflects the new project layout.
- [x] Focused test validation has been run from the `API` folder.
