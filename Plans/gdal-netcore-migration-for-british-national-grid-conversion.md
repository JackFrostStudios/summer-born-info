# GDAL.NetCore Migration For British National Grid Conversion

## 1. Overview

This plan defines the migration path from the current direct `GDAL` plus `GDAL.Native` package setup to `MaxRev-Dev/gdal.netcore` for British National Grid (`EPSG:27700`) to WGS84 (`EPSG:4326`) conversion during school import.

The implementation goal is to preserve the current import behavior and local OSTN15-backed grid shift support while replacing the custom runtime bootstrap with a cross-platform package strategy that works in both local Windows development and Linux container deployments.

Current implementation context on 2026-06-01:

- `API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/BritishNationalGridLocationConverter.cs` already uses GDAL/OSR for coordinate transformation.
- `API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/GdalRuntimeConfiguration.cs` currently configures native library discovery and GDAL/PROJ paths manually.
- The current bootstrap assumes Windows-native runtime layout under `gdal/x64` or `gdal/x86`, which is compatible with the current Windows-only `GDAL.Native` package but does not support Linux containers cleanly.
- `API/SummerBornInfo.Features/Resources/Gdal/share/uk_os_OSTN15_NTv2_OSGBtoETRS.tif` is already committed so the GB grid shift resource is available locally without relying on network access.
- `API/SummerBornInfo.Web/SummerBornInfo.Web.csproj` and `API/SummerBornInfo.Features.Tests/SummerBornInfo.Features.Tests.csproj` currently copy the OSTN15 grid into runtime output folders manually.

## 2. Roadmap Source Or Existing Plan Context

Existing plan context:

- [Plans/milestone-4-spatial-school-search-support.md](C:\Projects\summer-born-info\Plans\milestone-4-spatial-school-search-support.md)

Relevant delivered behavior from Milestone 4:

- School import parses `Easting` and `Northing` values from the source feed.
- Imported British National Grid coordinates are converted to WGS84 before persisting the canonical school point location.
- Invalid or blank coordinates do not produce malformed persisted points.
- Automated tests already cover successful import with valid coordinates and null-location behavior for invalid coordinates.

Confirmed user clarification:

- The project should migrate to `MaxRev-Dev/gdal.netcore` as the long-term GDAL runtime approach.
- The migration must preserve local grid shift support by bundling the OSTN15 resource instead of depending on remote grid downloads.
- The target runtime shape must support local Windows development and Linux containers.

## 3. Scope

This migration includes:

- replacing direct usage of `GDAL` and `GDAL.Native` with the relevant `MaxRev.Gdal.*` packages;
- introducing a cross-platform GDAL runtime initialization path based on `gdal.netcore`;
- preserving `OSGeo.OSR`-based British National Grid to WGS84 conversion behavior in the converter;
- preserving the locally bundled OSTN15 grid shift resource and wiring it into PROJ configuration for both Windows and Linux execution paths;
- ensuring the API host and test host both receive the runtime assets and bundled grid data required by `gdal.netcore`;
- updating automated tests so the migration is protected against runtime-path regressions and grid-resource omissions;
- verifying that Windows local development and Linux container deployment use compatible runtime packaging assumptions.

## 4. Non-Goals

- changing the public nearby-search API contract, school DTO shape, or persisted spatial model;
- changing the source coordinate parsing rules, null-on-invalid behavior, or school import domain logic;
- broadening the import pipeline to support additional coordinate systems beyond the current British National Grid use case;
- enabling remote PROJ grid downloads as part of normal runtime behavior;
- adopting GDAL command-line tools for application behavior that currently depends only on the managed GDAL bindings.

## 5. Behaviour Scenarios

### Scenario 1: Windows local development continues to convert source coordinates successfully

Given a developer runs the API or the feature tests on Windows  
When school import processes rows with valid `Easting` and `Northing` values  
Then `EPSG:27700` to `EPSG:4326` conversion succeeds  
And the converter still returns a `Point` with SRID `4326`  
And the runtime no longer depends on the custom Windows-only native loader logic.

### Scenario 2: Linux container runtime can initialize GDAL without Windows-specific assumptions

Given the API is published into a Linux container image  
When the first British National Grid conversion is requested  
Then GDAL and PROJ initialize without referencing `kernel32`, `gdal/x64`, or `gdal/x86`  
And the process can locate its native runtime libraries and projection resources through the `gdal.netcore` runtime packaging model.

### Scenario 3: OSTN15 grid shift support remains locally bundled

Given the migration removes the current direct `GDAL.Native` runtime package  
When the application or tests run without internet access  
Then the OSTN15 resource required for GB grid-based transformation remains present in the output  
And PROJ is configured to search the bundled local grid location  
And the conversion path does not depend on enabling PROJ network access.

### Scenario 4: Runtime output contains all required cross-platform data

Given the API host and automated test host are built or published  
When their output directories are inspected  
Then they contain the `gdal.netcore` runtime assets required for the current platform  
And they contain `proj.db`, GDAL data, and the bundled OSTN15 resource  
And the output structure matches the expectations documented by `gdal.netcore` for successful runtime initialization.

### Scenario 5: Existing import behavior remains stable

Given the existing school import tests for valid coordinates, invalid coordinates, and location clearing already pass  
When the migration is complete  
Then those behaviors still pass unchanged  
And any newly added runtime-initialization assertions confirm that the migration did not silently fall back to a less-capable projection setup.

## 6. Deliverables

1. Dependency migration definition

- Replace the direct `GDAL` and `GDAL.Native` package references with the appropriate `MaxRev.Gdal.Core` and runtime packages for this solution.
- Decide whether to use:
  - a universal package reference, or
  - explicit Windows and Linux runtime package references on executable projects.
- Record the chosen package layout and why it fits the solution’s Windows-plus-Linux deployment model.

2. Runtime bootstrap replacement

- Remove or substantially simplify `GdalRuntimeConfiguration`.
- Introduce a runtime initialization path based on `MaxRev.Gdal.Core.GdalBase.ConfigureAll()` and any companion configuration calls required for local grid resources.
- Ensure initialization remains idempotent and safe to call from the existing converter entry point.

3. Local OSTN15 resource preservation

- Keep `uk_os_OSTN15_NTv2_OSGBtoETRS.tif` in version control.
- Ensure the resource is copied into the output path expected by the `gdal.netcore` runtime layout.
- Configure PROJ to search the bundled grid location explicitly if the default `gdal.netcore` setup does not automatically include it.

4. Cross-platform executable packaging alignment

- Update the web host project so Windows and Linux runtime assets are available in publish/build output according to the chosen package strategy.
- Update the feature test project so automated tests use the same runtime assumptions as production wherever practical.
- If needed, add runtime identifiers, content-copy rules, or publish notes to make Linux container output deterministic.

5. Automated verification upgrades

- Preserve the existing converter and importer behavior tests.
- Add tests or assertions that prove the bundled OSTN15 resource is present in runtime output after the migration.
- Add at least one verification path that protects against regressions in cross-platform initialization expectations.

6. Delivery and operational documentation

- Document the new package/runtime model for contributors.
- Record how the locally bundled OSTN15 resource is expected to be copied and discovered.
- Document any Linux container publish or Dockerfile expectations that the runtime packages require.

## 7. Technology Requirements And Decisions

Confirmed decisions:

- The migration target is `MaxRev-Dev/gdal.netcore`.
- The project must continue to use a locally bundled OSTN15 grid resource rather than depending on remote downloads.
- The runtime model must support local Windows development and Linux containers.

Implementation decisions to confirm in delivery:

- Prefer `MaxRev.Gdal.Core` plus explicit runtime packages over the broad universal package unless implementation evidence shows the universal package meaningfully simplifies publish behavior without introducing unnecessary runtime baggage.
- Task 1 selected explicit runtime packages:
  - `MaxRev.Gdal.Core` in the features library;
  - `MaxRev.Gdal.WindowsRuntime.Minimal` in executable projects that must support local Windows development;
  - `MaxRev.Gdal.LinuxRuntime.Minimal` in executable projects that must support Linux containers.
- Keep the existing GDAL/OSR API surface in application code where possible so the migration is primarily a runtime-packaging change rather than a conversion-logic rewrite.
- Continue to treat PROJ network access as disabled by default so conversion quality does not depend on outbound connectivity.
- Explicitly configure the local grid resource path because `gdal.netcore` documentation states that `proj.db` is bundled but `proj-data` grid shifts may still need additional configuration.

Rationale:

- `gdal.netcore` directly addresses the current cross-platform runtime packaging problem that the current Windows-oriented `GDAL.Native` package does not solve cleanly.
- Keeping the `OSGeo.OSR` usage model stable reduces migration risk and keeps the implementation focused on dependency/runtime concerns.
- Explicit local grid configuration protects the national-standard GB transformation path from environment drift, publish-layout changes, or network assumptions.

## 8. Dependencies And Sequencing

1. Confirm the final `gdal.netcore` package selection strategy for executable and library projects.
2. Swap package references and restore the runtime initialization path in a compileable state.
3. Replace the custom runtime bootstrap with `gdal.netcore` initialization and local PROJ grid configuration.
4. Align executable project outputs so the runtime packages and OSTN15 resource are copied consistently.
5. Re-run existing converter/importer tests and add migration-specific coverage.
6. Verify the web host build or publish path reflects the intended Linux-container-ready runtime layout.
7. Update contributor-facing documentation for the new runtime model.

### Sequential Task Breakdown

1. Task 1: Package strategy migration

- Replace the current direct GDAL package references with `MaxRev.Gdal.*` package references.
- Outcome: the solution restores with the new dependency model.
- Commit boundary: package references only.

2. Task 2: Runtime initialization migration

- Replace `GdalRuntimeConfiguration` with `gdal.netcore` initialization plus any required local PROJ configuration hooks.
- Outcome: converter initialization no longer depends on Windows-specific native-loader logic.
- Commit boundary: runtime bootstrap only.

3. Task 3: OSTN15 local resource alignment

- Ensure the bundled grid resource lands in the runtime output expected by the new package model.
- Outcome: the migration preserves offline GB grid shift support.
- Commit boundary: content/output configuration only.

4. Task 4: Cross-platform output verification

- Update executable-project packaging rules for web and test hosts as needed.
- Outcome: Windows local runs and Linux-targeted publish output both contain the required runtime assets and projection resources.
- Commit boundary: project output and publish configuration only.

5. Task 5: Automated verification and documentation

- Add or adjust tests and contributor documentation to reflect the new runtime model and bundled grid expectations.
- Outcome: the migration is regression-protected and understandable for future contributors.
- Commit boundary: tests and documentation only.

### Task State Checklist

- [x] Task 1 complete: Package strategy migration committed.
- [x] Task 2 complete: Runtime initialization migration committed.
- [x] Task 3 complete: OSTN15 local resource alignment committed.
- [ ] Task 4 complete: Cross-platform output verification committed.
- [ ] Task 5 complete: Automated verification and documentation committed.

## 9. Risks And Mitigations

- Runtime-layout risk:
  `gdal.netcore` uses a different output layout than the current direct GDAL packages, and the application may silently miss native libraries or data files if the output assumptions are wrong.
  Mitigation: make runtime-output assertions explicit in tests and validate both test-host and web-host outputs during delivery.

- Grid-shift regression risk:
  The migration could accidentally preserve `proj.db` but lose the locally bundled OSTN15 resource or fail to point PROJ at it.
  Mitigation: keep the OSTN15 file in version control, assert that it exists in output, and document the configuration call used to register the local path.

- Cross-platform packaging risk:
  A package combination that works on Windows may still fail in Linux containers if the wrong runtime package set is chosen or publish output is incomplete.
  Mitigation: prefer explicit runtime packages, verify Linux-oriented output expectations during delivery, and document any required runtime identifiers or publish settings.

- Behavior-preservation risk:
  A runtime migration may be mistaken for a behavior change and introduce subtle coordinate differences or fallback transformations.
  Mitigation: keep current importer behavior tests as regression checks and retain a dedicated converter test around the expected WGS84 point range.

- Maintenance risk:
  Moving to a community runtime wrapper changes dependency ownership and release cadence relative to the direct GDAL packages.
  Mitigation: document the rationale, keep the integration surface small, and record the package versions chosen during implementation.

## 10. Unknowns And Required Clarifications

Resolved for planning:

- The project should migrate toward `MaxRev-Dev/gdal.netcore`.
- Local OSTN15 support must remain bundled and offline-capable.
- Windows local development and Linux containers are both first-class runtime targets.

Implementation-time checks still required:

- Confirm whether explicit `Proj.Configure(...)` usage is required for the bundled OSTN15 resource path in addition to `GdalBase.ConfigureAll()`.
- Confirm whether the project should reference both Windows and Linux runtime packages directly in the same executable project or rely on runtime-conditional publish behavior.
- Confirm whether any container Dockerfile or publish-profile adjustments are needed beyond package references and content-copy configuration.

These are implementation details rather than blockers because the migration direction and runtime goals are already decided.

## 11. Completion Checklist

- [ ] The solution no longer depends on the direct `GDAL` / `GDAL.Native` package combination.
- [ ] `BritishNationalGridLocationConverter` still converts valid `EPSG:27700` coordinates into WGS84 `Point` values with SRID `4326`.
- [ ] The runtime bootstrap no longer assumes Windows-only native library layout.
- [ ] The chosen `gdal.netcore` packages support both local Windows development and Linux containers.
- [ ] The OSTN15 grid resource remains checked into the repository.
- [x] The OSTN15 grid resource is copied into runtime output where PROJ can discover it.
- [ ] PROJ remains configured to work without relying on network-downloaded grid files.
- [ ] Existing importer behavior tests for valid, invalid, and cleared coordinates still pass.
- [ ] Additional verification protects against losing the bundled grid resource in build or publish output.
- [ ] Contributor-facing documentation explains the new package/runtime model and local grid expectations.
