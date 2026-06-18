# Derive Postgres Test Image Tag From Dockerfile Version

## 1. Overview

Add explicit version metadata to the shared PostgreSQL Dockerfile and make the integration test fixture derive its Docker image tag and build mutex name from that version instead of a hardcoded task tag. The Dockerfile should start with a pinned version label of `1.0.0`, and future image-affecting Dockerfile changes should bump that version so local Docker caches invalidate through a new tag.

## 2. Roadmap Source or Existing Plan Context

This plan addresses the maintainability PR feedback captured in [peer-review-feat-milestone-4.md](../Documentation/peer-review-feat-milestone-4.md):

- The shared Postgres test image currently uses the fixed tag `summerborninfo-postgres-postgis-pgmq:task-2`.
- `IntegrationTestDatabaseServerFixture` builds that image with `WithDeleteIfExists(deleteIfExists: false)`, so a previously built local image is silently reused.
- The shared Dockerfile currently has no explicit version metadata that the test infrastructure can read.

Current relevant code:

- `API/SummerbornInfo.PostgresDockerImage/Dockerfile` defines the shared image contents for PostGIS and PGMQ.
- `API/SummerbornInfo.PostgresDockerImage/PostgresDockerfilePath.cs` already provides the canonical Dockerfile directory used by other projects.
- `API/SummerBornInfo.TestFramework/IntegrationTestDatabaseServerFixture.cs` hardcodes both the image tag and mutex name.
- `API/SummerBornInfo.AppHost/AppHost.cs` also uses the same Dockerfile project, so any helper introduced in `SummerbornInfo.PostgresDockerImage` should preserve that shared ownership model.

Requested direction from the user:

- Add version metadata directly in the Dockerfile.
- Pin the initial version label to `1.0.0`.
- Derive the test fixture tag from that Dockerfile version so changing the Dockerfile and bumping the version triggers image-change detection.

## 3. Scope

- Add a Docker image version label to the shared PostgreSQL Dockerfile.
- Expose that version through the `SummerbornInfo.PostgresDockerImage` project so consuming code has one source of truth.
- Replace the hardcoded image tag and global mutex name in `IntegrationTestDatabaseServerFixture` with values derived from the Dockerfile version.
- Add clear failure behavior when the Dockerfile version label is missing, empty, duplicated, or not usable in a Docker tag.
- Add lightweight documentation in or near the Dockerfile that image-affecting changes require a version bump.
- Validate the shared fixture still builds and runs in representative test paths.

## 4. Non-Goals

- Automatically hashing the Docker build context or Dockerfile contents to generate tags.
- Changing the AppHost runtime container naming, volume naming, or local development workflow beyond any compile-time impact from shared helper changes.
- Reworking the PostgreSQL image contents, base image, or root-user behavior.
- Deleting existing local Docker images as part of normal test execution.
- Introducing a new dedicated test project unless the implementation proves it is the only practical way to validate the helper logic.

## 5. Behaviour Scenarios

### Initial versioned image build

Given the shared Dockerfile declares version label `1.0.0`,
when `IntegrationTestDatabaseServerFixture` initializes on a machine with no cached image for that version,
then it should build and start the image tagged from version `1.0.0` and continue using `WithDeleteIfExists(false)` for normal reuse.

### Reuse when the image definition version is unchanged

Given the Dockerfile version label remains `1.0.0`,
when tests run repeatedly on the same machine,
then the fixture should keep reusing the existing local image tag derived from `1.0.0` without forcing a rebuild.

### Cache invalidation after a version bump

Given a maintainer changes the shared Dockerfile in an image-affecting way and updates the Dockerfile version label from `1.0.0` to a new value,
when tests next initialize the fixture,
then the derived image tag and mutex name should change automatically so Docker builds and uses the new image instead of silently reusing the prior one.

### Fast failure for invalid metadata

Given the Dockerfile version label is missing, malformed, duplicated, or blank,
when the fixture or shared helper reads the version,
then startup should fail with a clear exception that explains the expected label format rather than falling back to a stale or implicit tag.

## 6. Deliverables

1. Update `API/SummerbornInfo.PostgresDockerImage/Dockerfile` to include a version label pinned to `1.0.0`.
   Recommended decision: use the standard OCI label `org.opencontainers.image.version="1.0.0"`.
2. Add a short adjacent Dockerfile comment stating that any image-affecting Dockerfile change must also bump that version label.
3. Add a shared helper in `API/SummerbornInfo.PostgresDockerImage` that:
   - locates the shared Dockerfile via `PostgresDockerfilePath`,
   - reads the Dockerfile,
   - extracts the configured version label,
   - validates the result for Docker tag usage, and
   - exposes the parsed version to consumers.
4. Decide and document the helper contract.
   Recommended decision: expose a single public property or method such as `PostgreSqlDockerImageVersion.Version` so callers do not parse Dockerfile text themselves.
5. Update `API/SummerBornInfo.TestFramework/IntegrationTestDatabaseServerFixture.cs` so:
   - the image name is derived from the Dockerfile version instead of the fixed `:task-2` tag,
   - the mutex name is derived from the same versioned identifier, and
   - the fixture continues to serialize image creation for matching versions only.
6. Replace any remaining hardcoded tag-specific strings tied to `task-2` in the test framework.
7. Add focused validation for the shared helper and fixture behavior without creating unnecessary new project structure.
   Recommended decision: keep validation in existing test/build surfaces rather than introducing a new dedicated test project for this slice.
8. Run targeted verification from `API/`:
   - `dotnet build SummerBornInfo.AppHost/SummerBornInfo.AppHost.csproj`
   - `dotnet test SummerBornInfo.Infrastructure.Tests/SummerBornInfo.Infrastructure.Tests.csproj --filter-class "*PostgreSqlDatabaseBootstrapperTests"`
   - `dotnet test SummerBornInfo.Web.Tests/SummerBornInfo.Web.Tests.csproj --filter-class "*OpenApiSecurityDocumentTests"`

## 7. Technology Requirements and Decisions

- The Dockerfile version label should be the single human-maintained source of truth for the shared image version.
- Prefer the standard OCI label key `org.opencontainers.image.version` over a custom key unless implementation constraints make parsing that label impractical.
- The version-reading logic should live in `SummerbornInfo.PostgresDockerImage`, alongside `PostgresDockerfilePath`, so the Dockerfile project owns both path discovery and metadata access.
- `IntegrationTestDatabaseServerFixture` should consume the shared helper rather than re-reading the Dockerfile itself.
- Keep `.WithDeleteIfExists(deleteIfExists: false)` because the versioned tag becomes the intended cache invalidation mechanism.
- Sanitize or validate the label value against Docker tag expectations before composing the final image name and mutex name.
- This approach intentionally uses manual semantic versioning, not automatic content hashing. The implementation should make that maintenance rule explicit in code or comments instead of implying automatic change detection.

## 8. Dependencies and Sequencing

1. Add the version label and maintenance comment to the shared Dockerfile.
2. Introduce the shared Dockerfile-version helper in `SummerbornInfo.PostgresDockerImage`.
3. Update the integration test fixture to derive its image tag and mutex name from the shared helper.
4. Remove the remaining `task-2` literals once the derived path is in place.
5. Run compile-time validation for `SummerBornInfo.AppHost` to confirm the shared helper changes do not break other Dockerfile consumers.
6. Run representative Docker-backed tests that exercise the shared fixture from both infrastructure and web test paths.

## 9. Risks and Mitigations

- Risk: Manual version bumping can still be forgotten, which would preserve the stale-image problem under a new label-based design.
  Mitigation: make the version label prominent in the Dockerfile, add a nearby comment that version bumps are required for image-affecting changes, and ensure the fixture derives all cache keys directly from that value so the rule is explicit rather than hidden.

- Risk: Dockerfile parsing can become brittle if formatting changes or multiple labels are added later.
  Mitigation: keep the parser narrow, validate for exactly one expected version label, and fail fast with a descriptive exception when the format drifts.

- Risk: A version string valid for human readability may contain characters unsuitable for Docker tags or Windows mutex names.
  Mitigation: validate or normalize the version before composing the final image and mutex identifiers, and document the accepted format.

- Risk: Shared helper changes could unintentionally affect other consumers of `SummerbornInfo.PostgresDockerImage`.
  Mitigation: keep the new helper additive, preserve `PostgresDockerfilePath`, and run an AppHost build plus representative fixture-backed tests.

## 10. Unknowns and Required Clarifications

No blocking product or architecture unknowns remain for this plan.

Implementation assumption captured for delivery: the team accepts a manual version-bump workflow where Dockerfile changes are expected to update the label explicitly. If the desired end state later becomes automatic invalidation for any Dockerfile-content change, this plan should be revised toward a hash-based tagging strategy instead of a manually maintained label.

## 11. Completion Checklist

- [x] `API/SummerbornInfo.PostgresDockerImage/Dockerfile` contains a version label pinned to `1.0.0`.
- [x] The Dockerfile includes a clear maintenance note that image-affecting changes must bump the version label.
- [x] `SummerbornInfo.PostgresDockerImage` exposes the Dockerfile version through a shared helper instead of requiring downstream parsing.
- [ ] `IntegrationTestDatabaseServerFixture` no longer hardcodes the `:task-2` image tag or task-specific mutex name.
- [ ] The fixture derives both the Docker image tag and the mutex name from the Dockerfile version metadata.
- [ ] Invalid or missing Dockerfile version metadata causes a clear failure instead of falling back to a hidden default.
- [ ] `SummerBornInfo.AppHost` still builds after the shared helper is added.
- [ ] Representative infrastructure and web tests that use `IntegrationTestDatabaseServerFixture` pass with the new version-derived tag flow.
