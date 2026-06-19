# Milestone 4 Progress Memory

Date: 2026-05-29
Repo: `C:\Projects\summer-born-info`
Branch: `feat/milestone-4`

## Current State

- Tasks 1-9 from [Plans/milestone-4-spatial-school-search-support.md](C:\Projects\summer-born-info\Plans\milestone-4-spatial-school-search-support.md) are complete.
- Task 10 `Documentation alignment` is the next active task.
- The working tree should be clean immediately after committing the current checkpoint.

## Commit History For This Milestone

- `462ddae` `Add school spatial model foundation`
- `03652e1` `Add PostGIS bootstrap support`
- `94923b5` `Add school spatial schema indexing`
- `bbe4986` `Persist school locations during import`
- `dea0b02` `Add nearby school route contract`
- `6f63670` `Implement nearby school first-page query`
- `6c5896d` `Add nearby school cursor continuation`
- `a209ba5` `Document nearby school OpenAPI contract`
- Pending current commit: Task 9 verification slice

## What Task 9 Added

- Nearby API coverage now explicitly checks:
  - required-query validation for missing `latitude`, `longitude`, and `radiusMiles`
  - incompatible cursor replay when `longitude` changes
  - full `SchoolResponse` shape assertions on nearby result payloads
- OpenAPI schema verification now asserts `latitude` and `longitude` are present on `SchoolResponse`.
- Infrastructure tests now verify:
  - `Location` persists and round-trips as a PostGIS-backed point with SRID `4326`
  - generated/bootstrap-ready schema includes the `location` geography column metadata
  - `ix_school_location` exists and uses `gist`

## Files Changed In The Current Task 9 Checkpoint

- `API/SummerBornInfo.Web.Tests/API/Schools/GetNearbySchoolsTests.cs`
- `API/SummerBornInfo.Web.Tests/OpenApi/SchoolEndpointsOpenApiDocumentTests.cs`
- `API/SummerBornInfo.Infrastructure.Tests/Persistence/ApplicationDbContextSchoolTests.cs`
- `API/SummerBornInfo.Infrastructure.Tests/Persistence/PostgreSqlDatabaseBootstrapperTests.cs`
- `Plans/milestone-4-spatial-school-search-support.md`

## Validation That Passed

Run from `C:\Projects\summer-born-info\API`:

- `dotnet test SummerBornInfo.Web.Tests/SummerBornInfo.Web.Tests.csproj -- --filter-class SummerBornInfo.Web.Tests.API.Schools.GetNearbySchoolsTests --filter-class SummerBornInfo.Web.Tests.OpenApi.SchoolEndpointsOpenApiDocumentTests`
  - Passed: 27 tests
- `dotnet test SummerBornInfo.Infrastructure.Tests/SummerBornInfo.Infrastructure.Tests.csproj -- --filter-class SummerBornInfo.Infrastructure.Tests.Persistence.ApplicationDbContextSchoolTests --filter-class SummerBornInfo.Infrastructure.Tests.Persistence.PostgreSqlDatabaseBootstrapperTests`
  - Passed: 9 tests

Additional Task 8 validation already passed:

- `dotnet build SummerBornInfo.Web.Tests\SummerBornInfo.Web.Tests.csproj`
- `dotnet test SummerBornInfo.Web.Tests\SummerBornInfo.Web.Tests.csproj --no-build -- --filter-class *SchoolEndpointsOpenApiDocumentTests`

Important note:

- `dotnet test` must be run from the `API` directory so `API/global.json` applies the Microsoft Testing Platform runner.
- Avoid running `dotnet build` and `dotnet test` for `SummerBornInfo.Web.Tests` in parallel; that self-created file locks on `API/SummerBornInfo.Web.Tests/bin/Debug/net10.0/SummerBornInfo.Web.dll`.

## Next Session Resume Point

1. Confirm the Task 9 commit landed cleanly.
2. Start Task 10 from the plan using one worker.
3. Update API-facing documentation for:
   - `GET /api/schools/nearby`
   - the `PostGIS` plus `NetTopologySuite` choice
   - missing-location behaviour for nearby search
4. After Task 10, run the plan completion checklist and then perform the required peer review step from the `implement-plan` skill.
