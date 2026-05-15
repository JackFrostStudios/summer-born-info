# School Bulk Import Telemetry Relocation Plan

## Goal
Remove the long-running `ProcessSchoolBulkImport` activity from the background service and instead emit a short-lived activity for each processed CSV row during school bulk import handling.

## Delivery Steps
- [x] Step 1: Update tests to describe the new telemetry boundary.
  - Replace the background-service telemetry expectation with coverage that proves the background service no longer emits `ProcessSchoolBulkImport`.
  - Add focused coverage that proves row processing emits one `ProcessSchoolBulkImport` activity per processed row.
- [x] Step 2: Move the telemetry definition into the `Schools` feature and emit activities around each processed row.
  - Keep the background service free of custom activities.
  - Place the shared telemetry helper alongside the `GetSchoolBulkImportStatus` feature files.
  - Wrap each row result in `ProcessImportFileCommandHandler` with a short-lived activity tagged with request and row details.
- [x] Step 3: Run focused tests, then refine only if needed.
  - Run the relevant feature and web integration tests from `API/`.
  - Keep the change limited to telemetry orchestration.
