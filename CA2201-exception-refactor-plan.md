# CA2201 Exception Refactor Plan

## Goal

Replace base `Exception` usage in the following files with more specific exception types:

- `API/SummerBornInfo.Infrastructure/Persistence/LargeObjects/LargeObjectWriter.cs`
- `API/SummerBornInfo.IntegrationTests/TestData/ExampleImportFile.cs`

Co-locate the new exception classes beside their owning code in an `Exceptions` folder.

## Exception Grouping

### Large object persistence

Use one exception type per high-level failure concept, even when the exact message or trigger differs.

- `LargeObjectCreationException`
  - Covers both `CreateLargeObjectAsync` failure paths in `LargeObjectWriter`:
    - `ExecuteScalarAsync(...)` returns `null`
    - `ExecuteScalarAsync(...)` returns a value that is not a `uint`
- `LargeObjectOpenException`
  - Covers the `OpenLargeObjectAsync` failure path when the result is not a valid writable file descriptor

Target folder:

- `API/SummerBornInfo.Infrastructure/Persistence/LargeObjects/Exceptions/`

Target namespace:

- `SummerBornInfo.Infrastructure.Persistence.LargeObjects.Exceptions`

### Integration test resource loading

Use a resource-specific exception for missing embedded test data.

- `ExampleImportFileResourceNotFoundException`
  - Covers the `GetManifestResourceStream(...)` null case in `ExampleImportFile`

Target folder:

- `API/SummerBornInfo.IntegrationTests/TestData/Exceptions/`

Target namespace:

- `SummerBornInfo.TestFramework.TestData.Exceptions`

## Delivery Steps

- [x] Add `LargeObjectCreationException` and `LargeObjectOpenException` under the large object `Exceptions` folder.
- [x] Update `LargeObjectWriter` to throw those specific exceptions for each current CA2201 site.
- [x] Add `ExampleImportFileResourceNotFoundException` under the test data `Exceptions` folder.
- [x] Update `ExampleImportFile` to throw the new resource-specific exception.
- [ ] Run the relevant test/build validation to confirm the warning is resolved and no namespace issues were introduced.

## Notes

- Reuse `LargeObjectCreationException` for both create-failure branches because they represent the same high-level concept.
- Keep these exceptions local to their owning areas rather than introducing a shared cross-project exception bucket.
- If implementation work follows, the validation step should be run from `API/` per `AI_PROJECT_GUIDE.md`.
