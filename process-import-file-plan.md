# Process Import File Robustness Plan

- [x] Add import-specific exception types and sanitize user-facing file import errors.
- [x] Add and pass tests for resumable processing from the last processed row.
- [x] Add configurable max retry handling for poison messages in the background worker.

# Follow-up Error Handling Plan

- [x] Log unexpected wrapped import exceptions with ILogger before rethrowing.
- [x] Preserve the original CSV parse error text in row-level failure messages.
