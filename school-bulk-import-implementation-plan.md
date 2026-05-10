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

Status: in progress

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

- Extend the queue abstraction so the worker can both read and acknowledge messages.
- Smallest likely change:
  - add an infrastructure event-envelope/receipt type containing message id and payload
  - update or supplement `IEventReader` with a method that returns both
  - add a delete/ack method after successful handling
- Add infrastructure tests proving:
  - queued event can be read with metadata
  - acknowledged event is deleted and not re-read

### 5. Add the background service

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
