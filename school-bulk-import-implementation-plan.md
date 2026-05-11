# School Bulk Import Implementation Plan

## Goal

Complete the async school bulk import flow after upload so that:

- each CSV row produces a `SchoolImportResult`
- import progress is persisted after each processed row
- failed rows are recorded with line number and error message
- a background worker consumes `SchoolBulkImportUploaded` events and triggers processing

## Key findings from the current code

- Upload is already implemented in `API/SummerBornInfo.Features/Schools/Commands/Import/ImportSchoolsCommandHandler.cs`.
- Row processing already exists in `API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/SchoolsImporter.cs`.
- `SchoolBulkImportRequest` currently stores only `Id` and `ContentId`.
- The queue abstraction can read events, but the current `IEventReader` does not expose queue message ids, so a worker cannot acknowledge/delete processed messages yet.
- DI currently wires feature handlers and infrastructure services in `API/SummerBornInfo.Web/Program.cs`, and there is no hosted background worker pattern yet.

## Placement decisions

- Keep `SchoolImportResult`, `ProcessImportFileCommand`, and its handler in the existing `Schools/Commands/ProcessImportFile` feature slice.
- Keep CSV parsing and per-row import behaviour inside the existing `FileProcessing` folder.
- Extend domain entities in `API/SummerBornInfo.Domain` for persisted import progress and failure records.
- Add EF Core mapping and queue-consumer plumbing in `API/SummerBornInfo.Infrastructure`.
- Register the hosted worker and DI wiring in `API/SummerBornInfo.Web/Program.cs`.

## Implementation steps

### 1. Extend the import request persistence model

Status: complete

- Add progress fields to `SchoolBulkImportRequest`, at minimum `LinesProcessed`.
- Add persisted failed-line storage, preferably as a child entity such as `SchoolBulkImportFailure` with:
  - import request id
  - CSV line number
  - error message
- Consider adding a lightweight status field such as `Pending`, `Processing`, `Completed`, `CompletedWithFailures`, `Failed` to make retries and operations safer.
- Update EF configuration and add integration/domain tests for the new model shape.

### 2. Change the importer contract to return row results

Status: complete

- Add `SchoolImportResult` in the `ProcessImportFile` slice.
- Update `SchoolsImporter.ImportAsync` to yield `SchoolImportResult` instead of `int`.
- Include:
  - file line number
  - success flag
  - optional error message
- Wrap per-row processing so expected row-level failures become failed results rather than stopping the whole stream.
- Keep unexpected file-level failures available for the handler to surface or mark the request as failed.
- Update importer tests to assert both successful results and failure result behaviour.

### 3. Implement the process-import command and handler

Status: complete

- Add `ProcessImportFileCommand` carrying `SchoolBulkImportRequestId`.
- Add `ProcessImportFileCommandHandler` in the same feature folder.
- Handler flow:
  - load the `SchoolBulkImportRequest`
  - open the stored CSV via `ILargeObjectReader`
  - enumerate `SchoolsImporter.ImportAsync`
  - after each yielded result:
    - increment/persist processed line count
    - append failure rows when needed
    - save changes so progress is durable during long-running imports
- Decide and document idempotency behaviour before implementation:
  - simplest path is "do not reprocess completed imports"
  - if retries are needed, guard against duplicate failure rows and duplicate line counts
- Add feature tests for:
  - request not found
  - content missing
  - successful progress updates
  - failed line capture
  - repeat-processing guard

### 4. Fill the queue-consumer gap for the worker

Status: complete

- Extend the queue abstraction so the worker can both read and acknowledge messages.
- Smallest likely change:
  - add an infrastructure event-envelope/receipt type containing message id and payload
  - update or supplement `IEventReader` with a method that returns both
  - add a delete/ack method after successful handling
- Add infrastructure tests proving:
  - queued event can be read with metadata
  - acknowledged event is deleted and not re-read

### 5. Add the background service

Status: complete

- Add a hosted service in `API/SummerBornInfo.Web` because this is application startup/runtime wiring.
- Worker loop responsibilities:
  - poll `EventQueue.SchoolBulkImport`
  - resolve a scope
  - invoke `ProcessImportFileCommandHandler`
  - acknowledge/delete the queue message only after successful processing
- For handler failures, log and leave the message unacknowledged so queue visibility timeout can retry it.
- Register:
  - `ProcessImportFileCommandHandler`
  - `ILargeObjectReader`
  - `IEventReader` or new queue-consumer abstraction
  - hosted service

## Suggested delivery order

1. Data model and tests for import request progress/failures.
2. Importer contract change to `SchoolImportResult` and importer tests.
3. Process-import handler with integration tests.
4. Queue acknowledgement abstraction and infrastructure tests.
5. Hosted background worker and end-to-end integration coverage where practical.

## Commit plan

- Commit 1: import request model and persistence tests
- Commit 2: importer result contract and importer tests
- Commit 3: process-import command/handler and feature tests
- Commit 4: queue acknowledgement support and infrastructure tests
- Commit 5: background worker registration and integration verification

## Open decisions to confirm before coding

- Whether failed rows should be stored in a separate table or a JSON column. Separate table is the safer fit with the current EF style.
- Whether we want an explicit import status field now. I recommend yes, because it simplifies worker retries and prevents accidental reprocessing.

## Phase 2

## Goal

Refactor the completed bulk import implementation so that queue concerns, domain behaviour, and worker timing configuration are cleaner and more maintainable without changing the user-visible workflow.

## Key findings for the refactor

- `API/SummerBornInfo.Infrastructure/Events/EventReader.cs` currently handles both reading and deleting queue messages.
- `API/SummerBornInfo.Domain/Entities/SchoolBulkImportRequest.cs` contains persisted progress state, but the row-processing update behaviour should move into the entity so the rules live in one place.
- `SchoolBulkImportRequest` currently exposes mutable state directly, which allows callers to bypass domain rules when changing progress, failures, or status.
- `API/SummerBornInfo.Web/BackgroundServices/ProcessSchoolBulkImportBackgroundService.cs` currently hard-codes both the empty-queue delay and the event-read timeout.
- The row error rule needs to be explicit: one failure record per row, with later processing for the same row overwriting the stored error details instead of creating duplicates.

## Placement decisions

- Keep queue read behaviour in `API/SummerBornInfo.Infrastructure/Events/EventReader.cs`.
- Extract queue delete/acknowledgement into a separate infrastructure class and abstraction in the same events area so read and delete responsibilities are isolated.
- Put row progress and failure-recording rules on `SchoolBulkImportRequest` in `API/SummerBornInfo.Domain`.
- Restrict external mutation of import state so progress counters, status, and failure collections are updated only through domain methods on `SchoolBulkImportRequest`.
- Keep polling and timeout settings in `API/SummerBornInfo.Web/appsettings.json`, with binding in the web host for the background service.

## Implementation steps

### 6. Separate queue delete behaviour from the event reader

Status: pending

- Refactor `EventReader` so it is responsible only for reading queue messages.
- Introduce a separate class and interface for delete/acknowledgement behaviour.
- Update background-service DI and usage so message deletion goes through the new abstraction.
- Add or update infrastructure tests to cover the split responsibilities.

### 7. Move bulk import progress rules into the domain entity

Status: pending

- Add an `UpdateProgress` method on `SchoolBulkImportRequest`.
- Add explicit status-transition methods on `SchoolBulkImportRequest`, for example:
  - `ProcessingStarted`
  - `ProcessingComplete`
  - `ProcessingFailed`
- Make import state externally read-only where possible so callers cannot set progress, status, or failure entries directly.
- Method responsibilities:
  - update the number of rows processed
  - record a row error when one exists
  - overwrite the existing error details when the same row already has a recorded failure
- Status-transition responsibilities:
  - `ProcessingStarted` sets the status to `Processing`
  - `ProcessingComplete` sets the status to `Completed` or `CompletedWithFailures` based on recorded row failures
  - `ProcessingFailed` sets the status to `Failed`
- Refactor application/handler code so row-processing logic calls the entity method instead of manipulating persistence state directly.
- Add domain and handler tests covering:
  - processed-row count updates
  - new failure creation
  - duplicate-row failure overwrite behaviour
  - valid status transitions for started, completed, completed with failures, and failed paths
  - external callers cannot bypass the entity behaviour by mutating state directly

### 8. Make worker polling delay configurable

Status: pending

- Move the delay between iterations in `ProcessSchoolBulkImportBackgroundService` into configuration.
- Add the setting to `API/SummerBornInfo.Web/appsettings.json`.
- Bind the value through options or equivalent configuration wiring in the web project.
- Update tests to cover configuration-backed delay behaviour where practical.

### 9. Make event read timeout configurable

Status: pending

- Move the timeout passed to `ReadEventAsync` in `ProcessSchoolBulkImportBackgroundService` into configuration.
- Add the setting to `API/SummerBornInfo.Web/appsettings.json`.
- Bind and inject the value through the same worker settings model used for the polling delay where possible.
- Update tests to verify the configured timeout is used when polling the queue.

## Suggested delivery order

1. Extract queue delete/acknowledgement responsibilities and update infrastructure coverage.
2. Move row progress and failure rules into `SchoolBulkImportRequest` with failing tests first.
3. Wire worker polling delay and event read timeout through configuration.
4. Run regression coverage across the background worker and import-processing flow.

## Commit plan

- Commit 6: split queue read/delete responsibilities and update infrastructure tests
- Commit 7: move progress/failure rules into `SchoolBulkImportRequest` with domain/handler tests
- Commit 8: configure worker delay and read timeout via appsettings and update worker tests
