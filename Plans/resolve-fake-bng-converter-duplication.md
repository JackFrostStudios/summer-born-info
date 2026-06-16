# Resolve Fake BNG Converter Duplication

## 1. Overview

Centralize the fake British National Grid converter in `SummerBornInfo.TestFramework` so the shared test helper lives in one place and can be reused by `SummerBornInfo.Features.Tests` and `SummerBornInfo.Web.Tests` without keeping duplicate implementations in sync.

## 2. Roadmap Source or Existing Plan Context

This plan resolves the maintainability and code-quality PR feedback captured in [peer-review-feat-milestone-4.md](../Documentation/peer-review-feat-milestone-4.md):

- The fake British National Grid converter is duplicated across the feature and web test projects.
- The review expectation is that the shared test framework owns the reusable fake converter for all test projects.

Current state:

- `API/SummerBornInfo.Features.Tests/TestFramework/FakeBritishNationalGridLocationConverter.cs` contains one copy.
- `API/SummerBornInfo.Web.Tests/TestFramework/FakeBritishNationalGridLocationConverter.cs` contains a second, verbatim copy.
- Both test projects already reference `API/SummerBornInfo.TestFramework/SummerBornInfo.TestFramework.csproj`.

## 3. Scope

- Add a single reusable fake BNG converter to `API/SummerBornInfo.TestFramework`.
- Update the feature and web test projects to consume the shared helper from the test framework.
- Remove the duplicated per-project fake converter files once the shared helper is wired in.
- Verify the affected tests still compile and exercise the same canned coordinate behavior.

## 4. Non-Goals

- Changing the production `BritishNationalGridLocationConverter` implementation or its runtime behavior.
- Refactoring unrelated test helpers, factories, or integration-test infrastructure.
- Expanding the fake converter API beyond the behavior already relied on by the current tests.
- Resolving the other milestone 4 PR comments in the same implementation slice.

## 5. Behaviour Scenarios

### Shared fake usage from feature tests

Given a feature test that needs deterministic BNG-to-WGS84 conversion results,
when the test creates `FakeBritishNationalGridLocationConverter.ForExampleImportFile()` or configures custom return/throw behavior,
then the test should compile and behave exactly as it does today without owning a local fake implementation.

### Shared fake usage from web tests

Given a web integration test that passes a fake converter into `CustomWebApplicationFactory`,
when the test injects the shared fake through the existing `IBritishNationalGridLocationConverter` override path,
then the application factory should continue to use the fake singleton for test execution without any per-project converter copy.

### Single-source future maintenance

Given a future change to canned coordinates, cloning semantics, or error behavior in the fake converter,
when the helper is updated in `SummerBornInfo.TestFramework`,
then all consuming test projects should pick up the same behavior without needing manual synchronization across duplicate files.

## 6. Deliverables

1. Add a shared `FakeBritishNationalGridLocationConverter` implementation under `API/SummerBornInfo.TestFramework`.
2. Update `API/SummerBornInfo.TestFramework/SummerBornInfo.TestFramework.csproj` so the shared helper has direct access to:
   - `IBritishNationalGridLocationConverter` from `SummerBornInfo.CoordinateConversion`
   - `Point` from `NetTopologySuite`
3. Decide and apply the shared helper namespace so consuming tests can use the type with minimal churn.
   Recommended decision: keep the type in the `SummerBornInfo.TestFramework` namespace so both test projects can reuse their existing global using for the test framework.
4. Update `API/SummerBornInfo.Features.Tests` to consume the shared fake and remove any obsolete namespace/usings tied to the local copy.
5. Update `API/SummerBornInfo.Web.Tests` to consume the shared fake and remove any obsolete namespace/usings tied to the local copy.
6. Delete the duplicate converter files from:
   - `API/SummerBornInfo.Features.Tests/TestFramework/FakeBritishNationalGridLocationConverter.cs`
   - `API/SummerBornInfo.Web.Tests/TestFramework/FakeBritishNationalGridLocationConverter.cs`
7. Run targeted validation covering the affected consumers:
   - From `API/`: `dotnet test --project SummerBornInfo.Features.Tests/SummerBornInfo.Features.Tests.csproj -- --filter-class "SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.ProcessImportFileCommandHandlerTests" --filter-class "SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.ProcessImportFileTelemetryTests" --filter-class "SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.FileProcessing.SchoolsImporterTests"`
   - From `API/`: `dotnet test --project SummerBornInfo.Web.Tests/SummerBornInfo.Web.Tests.csproj -- --filter-class "SummerBornInfo.Web.Tests.API.Schools.CreateSchoolsImportRequestTests"`

## 7. Technology Requirements and Decisions

- `SummerBornInfo.TestFramework` should own the fake because it is the common dependency already shared by the affected test projects.
- The test framework should take a direct project reference on `SummerBornInfo.CoordinateConversion` rather than relying on transitive access through another project. This keeps the fake's dependency on `IBritishNationalGridLocationConverter` explicit.
- The test framework should also reference `NetTopologySuite` directly if the converter implementation needs `Point` at compile time and that dependency is not already guaranteed through a direct reference.
- Preserve the fake's existing API surface:
  - `Returns`
  - `Throws`
  - `ReturnsByDefault`
  - `ThrowsByDefault`
  - `ForExampleImportFile`
  - `CreateExampleAldgatePoint`
  - `CreateExampleSherbornePoint`
  - `CreatePoint`
- Preserve the current cloning behavior where configured points are recreated before return so tests do not accidentally share mutable `Point` instances.

## 8. Dependencies and Sequencing

1. Add the shared helper and supporting project references in `SummerBornInfo.TestFramework`.
2. Confirm the helper builds cleanly from the test framework project boundary.
3. Update feature tests to consume the shared type.
4. Update web tests to consume the shared type.
5. Remove duplicate files only after both consuming projects reference the shared helper successfully.
6. Run targeted tests for the feature and web suites to verify no behavioral regressions were introduced by the move.

## 8A. Implementation Progress

- [x] Add the shared helper and supporting project references in `SummerBornInfo.TestFramework`.
- [x] Confirm the helper builds cleanly from the test framework project boundary.
- [x] Update feature tests to consume the shared type.
- [x] Update web tests to consume the shared type.
- [x] Remove duplicate files only after both consuming projects reference the shared helper successfully.
- [x] Run targeted tests for the feature and web suites to verify no behavioral regressions were introduced by the move.

## 9. Risks and Mitigations

- Risk: Adding a new direct dependency from `SummerBornInfo.TestFramework` to `SummerBornInfo.CoordinateConversion` could widen the shared test framework surface unexpectedly.
  Mitigation: Keep the dependency limited to the test framework project and document it in the project file as the explicit home of the fake converter.

- Risk: Namespace or global-using changes could break test compilation in one project while fixing the other.
  Mitigation: Prefer the existing `SummerBornInfo.TestFramework` namespace to minimize consuming changes, and validate both test projects before considering the work complete.

- Risk: Subtle behavior drift could be introduced if the shared helper is rewritten instead of moved faithfully.
  Mitigation: Copy the existing implementation verbatim into the shared location first, then delete duplicates only after tests pass.

## 10. Unknowns and Required Clarifications

No blocking unknowns identified for planning. The implementation can proceed without additional product or architecture decisions because the desired ownership is explicit in the PR comment.

## 11. Completion Checklist

- [x] `SummerBornInfo.TestFramework` contains the only `FakeBritishNationalGridLocationConverter` implementation.
- [x] The test framework project file explicitly references any compile-time dependencies required by the shared fake.
- [x] `SummerBornInfo.Features.Tests` compiles against the shared fake with no local duplicate remaining.
- [x] `SummerBornInfo.Web.Tests` compiles against the shared fake with no local duplicate remaining.
- [x] The duplicated fake converter files have been removed from both test projects.
- [x] Targeted feature and web tests pass after the move.
- [x] The resulting changes directly satisfy the PR feedback that the fake converter should live in the shared test framework.
