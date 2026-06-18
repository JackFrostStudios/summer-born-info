# Optimize BNG Coordinate Conversion Caching

## 1. Overview

Reduce the per-row overhead in school imports by reusing the expensive GDAL/PROJ coordinate system setup while avoiding shared `CoordinateTransformation` instances across concurrent callers. The implementation should cache the source and target `SpatialReference` objects in the singleton converter, create one `CoordinateTransformation` per thread, and preserve the current conversion results and null-on-invalid-input behavior.

## 2. Roadmap Source or Existing Plan Context

This plan addresses the performance finding captured in [peer-review-feat-milestone-4.md](../Documentation/peer-review-feat-milestone-4.md):

- `SchoolsImporter` calls `IBritishNationalGridLocationConverter.TryConvertToWgs84Point(...)` once per imported row.
- `BritishNationalGridLocationConverter` currently rebuilds the full transform pipeline on every call by creating two `SpatialReference` instances, importing `EPSG:27700` and `EPSG:4326`, constructing a new `CoordinateTransformation`, and allocating a fresh coordinate buffer.
- The converter is already registered as a singleton in `API/SummerBornInfo.Web/Program.cs`, so the application has a natural lifetime for reusable conversion infrastructure.

Current state:

- `API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/SchoolsImporter.cs` converts coordinates inside the row-processing path.
- `API/SummerBornInfo.CoordinateConversion/BritishNationalGridLocationConverter.cs` performs runtime bootstrap lazily and rebuilds GDAL/PROJ objects per conversion.
- `API/SummerBornInfo.CoordinateConversion.Tests/BritishNationalGridLocationConverterTests.cs` verifies runtime bootstrap, valid conversions, and invalid-input handling, but does not currently assert transform reuse or concurrent safety expectations.

## 3. Scope

- Refactor `BritishNationalGridLocationConverter` so the expensive EPSG spatial reference setup is created once per converter lifetime instead of once per conversion call.
- Introduce thread-local or equivalent isolated ownership for `CoordinateTransformation` instances so concurrent callers do not share the same native transform object.
- Add tests that verify the converter still returns the same coordinate results and that the new caching lifecycle behaves as intended.
- Preserve the existing importer contract and DI registration pattern so callers continue using `IBritishNationalGridLocationConverter` unchanged.

## 4. Non-Goals

- Changing the importer flow, batching strategy, or `SaveChangesAsync` frequency in `SchoolsImporter`.
- Replacing GDAL/PROJ with a different coordinate conversion library.
- Changing the accepted input validation rules, null-return behavior, or WGS84 output shape.
- Adding wall-clock performance benchmarks as a required correctness gate for CI.
- Optimizing unrelated allocations in the import pipeline unless they are required to support the thread-local transformation design.

## 5. Behaviour Scenarios

### Repeated conversion on one import thread

Given a single importer execution processing many valid school rows on the same thread,
when `TryConvertToWgs84Point` is called repeatedly,
then the converter should reuse the cached `SpatialReference` objects and the same thread-local `CoordinateTransformation` instead of rebuilding the full transform pipeline for every row.

### Concurrent conversion from multiple callers

Given multiple callers using the singleton converter from different threads,
when each thread requests valid BNG-to-WGS84 conversion,
then each thread should use its own `CoordinateTransformation` instance and receive correct WGS84 coordinates without cross-thread mutation or disposal hazards from a shared transformation object.

### Invalid coordinate input

Given invalid, non-finite, or out-of-range easting/northing values,
when `TryConvertToWgs84Point` is called,
then the converter should still return `null` without creating unnecessary transformation work beyond the existing input parsing and range validation.

### Converter disposal at host shutdown

Given the converter is registered as a singleton in the ASP.NET DI container,
when the application host disposes singleton services,
then any cached `SpatialReference`, thread-local `CoordinateTransformation`, and related native resources created by the converter should be disposed cleanly.

## 6. Deliverables

1. Refactor `API/SummerBornInfo.CoordinateConversion/BritishNationalGridLocationConverter.cs` to cache the `EPSG:27700` and `EPSG:4326` `SpatialReference` instances for the lifetime of the converter.
2. Add isolated transformation reuse inside the converter.
   Recommended decision: use `ThreadLocal<CoordinateTransformation>` so each thread lazily creates one transformation from the cached spatial references and reuses it across repeated calls on that thread.
3. Update the converter lifecycle to dispose native GDAL objects correctly.
   Recommended decision: make `BritishNationalGridLocationConverter` implement `IDisposable`, dispose the cached `SpatialReference` instances, and dispose all created thread-local transformations when the singleton is torn down.
4. Keep `GdalRuntimeConfiguration.Configure()` as the lazy runtime bootstrap entry point, but move reusable transform object creation out of the hot per-call path.
5. Add a minimal test seam if needed so converter tests can verify caching behavior deterministically without relying on timing-based assertions.
   Recommended decision: introduce an internal factory or internal creation delegates for `SpatialReference` and `CoordinateTransformation` creation that production uses directly and tests can instrument.
6. Expand `API/SummerBornInfo.CoordinateConversion.Tests/BritishNationalGridLocationConverterTests.cs` to cover:
   - existing valid conversion behavior,
   - existing invalid input behavior,
   - reuse behavior for repeated conversions on the same thread,
   - isolated transformation creation for concurrent threads or equivalent multi-threaded access,
   - disposal of cached native resources if a test seam is introduced.
7. Keep `API/SummerBornInfo.Web/Program.cs` registration as `AddSingleton<IBritishNationalGridLocationConverter, BritishNationalGridLocationConverter>()` unless the refactor reveals a lifetime issue that contradicts the plan assumptions.
8. Validate the implementation from `API/` with at least:
   - `dotnet test SummerBornInfo.CoordinateConversion.Tests/SummerBornInfo.CoordinateConversion.Tests.csproj`
   - `dotnet build SummerBornInfo.sln`

## 7. Technology Requirements and Decisions

- Keep the implementation inside `SummerBornInfo.CoordinateConversion`; callers in `Features` and `Web` should continue consuming only `IBritishNationalGridLocationConverter`.
- Prefer thread-local transformations over an object pool for this change.
  Rationale: the converter is a singleton, conversion calls are short-lived, and thread-local ownership avoids rent/return discipline, pool exhaustion concerns, and accidental cross-thread reuse of native transform state.
- Cache only the expensive, stable CRS setup globally in the first iteration:
  - one source `SpatialReference` for `EPSG:27700`
  - one target `SpatialReference` for `EPSG:4326`
- Do not add timing-based tests or benchmark thresholds to CI because they are environment-sensitive and brittle. Verify the optimization by asserting creation/reuse semantics instead.
- Preserve the current lazy GDAL runtime bootstrap behavior so tests and host startup do not need manual configuration changes.

## 8. Dependencies and Sequencing

1. [x] Confirm the converter remains the sole owner of real GDAL/PROJ conversion objects and decide the exact disposal model for cached resources.
2. [x] Introduce the reusable spatial reference fields and thread-local transformation creation path in `BritishNationalGridLocationConverter`.
3. [x] Add any required internal test seam for observing transform creation counts and disposal behavior.
4. [x] Extend the coordinate conversion tests to cover same-thread reuse and concurrent-thread isolation without changing the public interface.
5. [x] Run the coordinate conversion test project.
6. [x] Run a solution build to catch any DI, disposal, or compile-time regressions outside the converter project.

## 9. Risks and Mitigations

- Risk: Shared cached `SpatialReference` instances may still prove unsafe if GDAL interop mutates them during transformation creation.
  Mitigation: keep the plan scoped so the implementation can pivot to thread-local transform bundles that also own thread-local spatial references if tests or runtime behavior show shared references are not safe.

- Risk: Introducing `IDisposable` on the singleton converter could leak or double-dispose native resources if the thread-local tracking strategy is incomplete.
  Mitigation: choose a disposal approach that can enumerate created thread-local values, cover it with a focused unit test seam, and rely on DI-managed singleton disposal rather than ad hoc caller disposal.

- Risk: A pooling abstraction could add complexity without enough benefit for the current workload.
  Mitigation: implement the recommended thread-local design first and reserve pooling as a fallback only if thread-local storage proves unsuitable.

- Risk: Implementation might accidentally change conversion results or error behavior while refactoring the hot path.
  Mitigation: preserve and extend the existing authoritative converter tests for valid coordinates, invalid values, and runtime bootstrap behavior before considering the change complete.

## 10. Unknowns and Required Clarifications

No blocking product or roadmap clarifications are required for this plan.

Assumption captured for implementation: shared cached `SpatialReference` instances are acceptable to use as read-only inputs when constructing one `CoordinateTransformation` per thread. If that assumption fails during implementation or test validation, revise the implementation to keep the same public contract while moving the spatial references into the same thread-local ownership boundary as the transformation.

## 11. Completion Checklist

- [x] `BritishNationalGridLocationConverter` no longer creates fresh `SpatialReference` instances on every conversion call.
- [x] The converter no longer creates a fresh `CoordinateTransformation` for every conversion call.
- [x] Repeated conversions on the same thread reuse the same transformation instance or equivalent cached per-thread transform state.
- [x] Concurrent callers do not share a single `CoordinateTransformation` instance across threads.
- [x] Native GDAL resources created by the converter are disposed through a defined singleton lifecycle.
- [x] Existing conversion accuracy and invalid-input behavior remain covered by tests.
- [x] New tests prove caching/reuse semantics without relying on timing-based assertions.
- [x] `dotnet test SummerBornInfo.CoordinateConversion.Tests/SummerBornInfo.CoordinateConversion.Tests.csproj` passes from `API/`.
- [x] `dotnet build SummerBornInfo.sln` passes from `API/`.
