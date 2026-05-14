# School Bulk Import Status Endpoint Plan

## Goal
Add a `GET` endpoint to retrieve school bulk import status by request ID, returning:
- `schoolBulkImportRequestId`
- `status`
- `LinesProcessed`
- `failures` (objects with line number + message)

The response must **not** include `ContentId` (large object ID / oid).

## Proposed API Contract
- Route: `GET /api/schools/import/{requestId}`
- Success (`200 OK`) response shape:
  - `schoolBulkImportRequestId: Guid`
  - `status: string`
  - `LinesProcessed: int`
  - `failures: { lineNumber: int, message: string }[]` (mapped from failures)
  - `status` values should match the domain request status flags:
    - `Pending`
    - `Processing`
    - `Completed`
    - `CompletedWithFailures`
    - `Failed`
  - the endpoint must support polling and return `200 OK` for any existing request in any of the above states
  - `failures` must be returned as an empty array when there are none; never `null`
  - `failures` must be ordered ascending by `lineNumber`
- Not found (`404 Not Found`) when no import request exists for `requestId`.

## Delivery Steps
- [ ] Step 1: Add a failing web integration test in `API/SummerBornInfo.Web.IntegrationTests/SchoolsIntegrationTests.cs` for:
  - known request ID returns `200` with expected fields
  - missing request ID returns `404`
  - response does not expose `ContentId`
  - returned `failures` are ordered by `lineNumber` ascending
  - `failures` is `[]` when no failures exist
  - existing requests in any status return `200`
- [ ] Step 2: Add a new query slice under `API/SummerBornInfo.Features/Schools/Queries/GetSchoolBulkImportStatus/`:
  - query record (`GetSchoolBulkImportStatusQuery`)
  - response DTO (`GetSchoolBulkImportStatusResponse`)
  - handler (`GetSchoolBulkImportStatusQueryHandler`)
  - query should load failures, order by line number, and project to the API-safe DTO (no `ContentId`)
- [ ] Step 3: Wire endpoint in `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs`:
  - map `GET /import/{requestId:guid}`
  - call handler
  - return `404` when query result is null, otherwise `200`
- [ ] Step 4: Run focused tests:
  - `dotnet test API/SummerBornInfo.Web.IntegrationTests/SummerBornInfo.Web.Tests.csproj`
  - then run full solution tests if needed
- [ ] Step 5: Refactor only for clarity (if required), keep behavior unchanged, and re-run tests.

## Notes
- Keep implementation aligned with existing vertical-slice conventions from `AI_PROJECT_GUIDE.md`.
- Reuse current schools endpoint style and existing `SchoolBulkImportRequest` persistence model.
