# Coordinate Conversion Runtime Abstraction For Tests

## 1. Overview

This plan introduces a test-safe abstraction around British National Grid to WGS84 conversion so that most feature and web tests no longer boot the real GDAL runtime. The goal is to keep the real GDAL-backed implementation for application runtime and for the direct coordinate-conversion test project, while allowing the rest of the solution to substitute a deterministic fake or stub converter during parallel test execution.

Current implementation context on 2026-06-11:

- `API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/SchoolsImporter.cs` calls a static `BritishNationalGridLocationConverter.TryConvertToWgs84Point(...)` directly.
- The same converter and `GdalRuntimeConfiguration` currently exist in both `API/SummerBornInfo.Features/.../FileProcessing/` and `API/SummerBornInfo.CoordinateConversion/`, which leaves two competing runtime entry points in the solution.
- `API/SummerBornInfo.Web/Program.cs` eagerly calls `GdalRuntimeConfiguration.Configure()` during host startup through the feature-layer global usings, so any web-hosted test process initializes GDAL before tests can substitute collaborators.
- `API/SummerBornInfo.Features.Tests` contains importer and command-handler tests that currently instantiate the real importer and, in some cases, explicitly call `GdalRuntimeConfiguration.Configure()`.
- `API/SummerBornInfo.CoordinateConversion.Tests` already exists as the dedicated place for real GDAL and PROJ runtime verification.

## 2. Roadmap Source Or Existing Plan Context

Existing plan context:

- [Plans/milestone-4-spatial-school-search-support.md](C:\Projects\summer-born-info\Plans\milestone-4-spatial-school-search-support.md)
- [Plans/gdal-netcore-migration-for-british-national-grid-conversion.md](C:\Projects\summer-born-info\Plans\gdal-netcore-migration-for-british-national-grid-conversion.md)

Relevant delivered behavior from those plans:

- School import converts source `Easting` and `Northing` values into a canonical WGS84 `Point`.
- The solution now uses the `SummerBornInfo.CoordinateConversion` project plus `gdal.netcore` runtime packaging.
- Dedicated coordinate conversion tests already validate the real GDAL runtime, bundled OSTN15 grid usage, and successful conversions.

New requirement from the user:

- Most tests must be able to mock or stub coordinate conversion instead of using the real GDAL configuration.
- Only production runtime and the direct coordinate conversion tests should rely on the real GDAL-backed implementation.

## 3. Scope

This change includes:

- introducing a DI-friendly abstraction for coordinate conversion;
- consolidating runtime conversion code so the GDAL-backed implementation lives in one canonical place;
- removing eager or static usage patterns that force GDAL initialization before tests can replace the converter;
- updating feature and web tests to use fake conversion behavior by default;
- preserving the real GDAL-backed implementation for runtime and `SummerBornInfo.CoordinateConversion.Tests`.

## 4. Non-Goals

- changing the coordinate parsing rules, valid-range checks, or null-on-invalid behavior of the real converter;
- changing nearby-search contracts or the persisted school geometry model;
- removing the dedicated real-runtime verification in `SummerBornInfo.CoordinateConversion.Tests`;
- introducing a new solution project unless implementation proves the existing project boundaries cannot support the abstraction cleanly.

## 5. Behaviour Scenarios

### Scenario 1: Runtime import still uses the real GDAL-backed converter

Given the application starts in production or development runtime  
When school import needs to convert valid `Easting` and `Northing` values  
Then the importer resolves the real coordinate conversion service from DI  
And that service performs the existing GDAL-backed conversion behavior  
And runtime configuration remains internal to the real implementation rather than being a separate eager startup requirement.

### Scenario 2: Feature tests can run importer logic without booting GDAL

Given a feature test exercises `SchoolsImporter` or `ProcessImportFileCommandHandler` behavior that is not specifically about GDAL  
When the test constructs the subject under test  
Then it can supply a fake coordinate conversion implementation  
And the test does not need to call `GdalRuntimeConfiguration.Configure()`  
And the test can run in parallel without competing over shared GDAL/PROJ process state.

### Scenario 3: Web-hosted tests can replace the real converter before import flows execute

Given a web integration test needs the application host but does not care about real coordinate transformation  
When the test overrides service registration in the test host  
Then the import pipeline uses the test double instead of the GDAL-backed implementation  
And host startup does not eagerly initialize GDAL before the override can take effect.

### Scenario 4: Direct coordinate conversion tests remain the only real-runtime verification path

Given the dedicated `SummerBornInfo.CoordinateConversion.Tests` project  
When those tests execute  
Then they still initialize the real GDAL runtime, verify offline PROJ/grid-shift configuration, and validate representative conversions  
And that project remains the authoritative regression suite for the real conversion implementation.

### Scenario 5: Import behavior tests remain meaningful with stubbed conversion results

Given importer tests need to verify persistence of geometry values or location clearing behavior  
When the fake converter returns a known `Point` or `null`  
Then the importer persists or clears school geometry accordingly  
And the tests verify import behavior without depending on GDAL internals or coordinate math.

## 6. Deliverables

1. Canonical coordinate conversion abstraction

- Introduce an interface in `API/SummerBornInfo.CoordinateConversion/` that owns the conversion contract used by features, for example a `TryConvertToWgs84Point`-style API returning `Point?`.
- Replace the static-only consumption model with an instance-based GDAL implementation that satisfies that interface.
- Keep parsing, validation, and conversion behavior aligned with the current real converter.

2. Runtime implementation consolidation

- Remove the duplicate `BritishNationalGridLocationConverter` and `GdalRuntimeConfiguration` implementation from `API/SummerBornInfo.Features/.../FileProcessing/`.
- Update feature code to depend on the canonical abstraction from `SummerBornInfo.CoordinateConversion` instead of the feature-local static classes.
- Clean up global usings and namespaces so `SummerBornInfo.Web` and tests no longer accidentally bind to the feature-local runtime classes.

3. Lazy runtime configuration boundary

- Move GDAL runtime initialization behind the real converter implementation so it occurs only when that implementation is actually used.
- Remove the eager top-level `GdalRuntimeConfiguration.Configure();` call from `API/SummerBornInfo.Web/Program.cs`.
- Register the real converter implementation in DI in `API/SummerBornInfo.Web/Program.cs` with a lifetime appropriate for a stateless service and idempotent runtime initialization.

4. Test seam adoption across feature and web tests

- Update `SchoolsImporter<TContext>` to receive the converter abstraction through constructor injection.
- Update direct importer and command-handler tests in `API/SummerBornInfo.Features.Tests/` to use a fake or stub converter by default.
- Where tests currently assert geometry ranges derived from real coordinate conversion, replace those assertions with deterministic fake-returned points unless the test's purpose is specifically to validate GDAL conversion.
- Add a simple reusable fake converter helper for tests so geometry success, invalid-coordinate nulls, and exception paths can be expressed without custom setup in every test.
- If any web integration tests execute import flows, ensure `CustomWebApplicationFactory` can override the converter registration cleanly.

5. Real-runtime verification boundary

- Keep the real GDAL bootstrap and conversion assertions in `API/SummerBornInfo.CoordinateConversion.Tests/`.
- Remove redundant real-runtime converter tests from `API/SummerBornInfo.Features.Tests/` once equivalent coverage exists in `CoordinateConversion.Tests`.
- Preserve at least one direct test in the coordinate-conversion test project for valid conversion, invalid input, out-of-range input, and runtime/grid-shift configuration.

6. Documentation and plan alignment

- Update the relevant plan document checklists if implementation changes the completion state of this work.
- Add or adjust contributor documentation where needed so the intended testing split is explicit: fake converter in most tests, real GDAL only in runtime and dedicated coordinate-conversion tests.

## 7. Technology Requirements And Decisions

Confirmed decisions:

- The canonical real implementation should remain in `API/SummerBornInfo.CoordinateConversion/`.
- Most tests should substitute a fake or stubbed converter instead of using real GDAL.
- The real GDAL-backed implementation should remain in use for runtime and for `API/SummerBornInfo.CoordinateConversion.Tests/`.

Planning decisions:

- Put the abstraction in `SummerBornInfo.CoordinateConversion` rather than `SummerBornInfo.Features` so the project that owns the behavior also owns its contract and implementation.
- Prefer constructor injection into `SchoolsImporter<TContext>` over static calls so tests can control conversion behavior per test case.
- Treat GDAL runtime configuration as an internal implementation concern of the real converter rather than a separate startup concern in `Program.cs`.
- Use DI overrides in test hosts instead of environment flags or conditional compilation to avoid hidden test-only runtime branches.

Rationale:

- Keeping the abstraction with the coordinate conversion project avoids creating a reverse dependency from the implementation project back into features.
- Constructor injection is the smallest change that opens a clean seam for both feature tests and web-hosted tests.
- Removing eager startup configuration is necessary because service overrides in tests happen after `Program` is loaded; otherwise GDAL initializes even when tests do not need it.
- Consolidating the duplicate feature-local and coordinate-conversion-local implementations prevents future drift and keeps runtime ownership clear.

## 8. Dependencies And Sequencing

1. Confirm the canonical abstraction shape and implementation location inside `SummerBornInfo.CoordinateConversion`.
2. Consolidate duplicate runtime/converter classes so there is one real implementation path.
3. Update importer construction and DI registration to use the abstraction.
4. Remove eager startup configuration so tests can override the service safely.
5. Refactor feature and web tests to use fake conversion behavior by default.
6. Remove redundant GDAL-backed tests outside `CoordinateConversion.Tests`.
7. Update documentation and plan completion markers.

### Sequential Task Breakdown

1. Task 1: Define the converter abstraction and canonical GDAL-backed implementation

- Add the interface and convert the real implementation from static-only usage to an injectable service.
- Outcome: the coordinate conversion project exposes one runtime-safe contract and one real implementation.
- Commit boundary: coordinate conversion project only.

2. Task 2: Rewire feature code to the abstraction and remove duplicates

- Inject the abstraction into `SchoolsImporter<TContext>` and delete the feature-local converter/runtime duplicates.
- Outcome: feature code no longer depends on static GDAL classes in its own layer.
- Commit boundary: features project only.

3. Task 3: Update runtime composition

- Register the real converter implementation in `Program.cs` and remove eager `GdalRuntimeConfiguration.Configure()` startup behavior.
- Outcome: runtime still resolves the real converter, but tests can replace it before use.
- Commit boundary: web host composition only.

4. Task 4: Refactor tests to the fake-by-default model

- Replace direct GDAL setup in `Features.Tests` with fake converter injection and helper doubles.
- Outcome: most tests stop competing over real GDAL runtime state and become more deterministic in parallel runs.
- Commit boundary: feature and web test projects only.

5. Task 5: Preserve and tighten real-runtime verification

- Keep real GDAL assertions only in `CoordinateConversion.Tests` and remove redundant coverage elsewhere.
- Outcome: there is one authoritative suite for runtime conversion behavior.
- Commit boundary: coordinate-conversion tests only.

### Task State Checklist

- [x] Task 1 complete: converter abstraction and canonical implementation committed.
- [x] Task 2 complete: feature code rewired and duplicates removed.
- [x] Task 3 complete: runtime composition updated.
- [ ] Task 4 complete: fake-by-default test refactor committed.
- [ ] Task 5 complete: real-runtime verification boundary cleaned up.

## 9. Risks And Mitigations

- Hidden startup coupling risk:
  Web-hosted tests may still trigger GDAL initialization if any remaining static initializer or top-level startup path touches runtime configuration too early.
  Mitigation: remove the explicit startup configure call, search for remaining direct `GdalRuntimeConfiguration.Configure()` usage, and verify tests can construct the host with a fake converter only.

- Duplicate implementation drift risk:
  Leaving both feature-local and coordinate-conversion-local classes in place would preserve ambiguity and allow future regressions.
  Mitigation: delete the feature-local runtime/converter classes as part of the rewiring task rather than leaving shims behind indefinitely.

- Over-mocking risk:
  Refactoring tests away from real GDAL could accidentally remove confidence in actual conversion behavior.
  Mitigation: keep and, where useful, strengthen the dedicated `CoordinateConversion.Tests` suite as the single real-runtime regression boundary.

- Constructor churn risk:
  Injecting the converter into `SchoolsImporter<TContext>` may require touching several test constructors and helper methods.
  Mitigation: add a small reusable fake converter and centralize default construction helpers in tests to keep the churn mechanical and low-risk.

- Runtime regression risk:
  Moving initialization behind the service boundary could break first-use conversion if configuration stops being idempotent.
  Mitigation: keep runtime setup idempotent inside the real implementation and preserve direct tests that exercise first-use conversion successfully.

## 10. Unknowns And Required Clarifications

No user decision is required to make this plan implementation-ready.

Implementation details still to settle during delivery:

- Final interface name and whether it should expose only `TryConvertToWgs84Point(...)` or also encapsulate any future conversion operations.
- Exact DI lifetime (`Singleton` or `Scoped`) for the real converter service, provided the implementation remains stateless and thread-safe around idempotent GDAL initialization.
- Whether any web integration tests currently exercising school import need targeted factory helpers for converter overrides, or whether feature-test coverage is sufficient after the fake seam is added.

These are delivery-level choices, not blockers, because they do not materially change scope or architecture direction.

## 11. Completion Checklist

- [ ] There is exactly one canonical real coordinate conversion implementation in `API/SummerBornInfo.CoordinateConversion/`.
- [ ] Feature code no longer contains its own duplicate `BritishNationalGridLocationConverter` or `GdalRuntimeConfiguration`.
- [ ] `SchoolsImporter<TContext>` depends on an injectable coordinate conversion abstraction instead of a static converter call.
- [ ] `API/SummerBornInfo.Web/Program.cs` registers the real converter in DI and no longer eagerly configures GDAL at startup.
- [ ] Most tests in `API/SummerBornInfo.Features.Tests/` no longer call `GdalRuntimeConfiguration.Configure()` or depend on real GDAL conversion.
- [ ] Any remaining real-runtime conversion assertions outside runtime have been consolidated into `API/SummerBornInfo.CoordinateConversion.Tests/`.
- [ ] Dedicated coordinate-conversion tests still verify offline runtime configuration, valid conversions, invalid inputs, and out-of-range handling.
- [ ] Parallel test execution no longer depends on shared real GDAL process state for ordinary feature and web tests.
