# Roslyn Warning Cleanup Plan

- [x] Capture all current analyzer warnings from a full build.
- [x] Classify warnings into: quick/non-controversial fixes vs controversial/no-easy-fix.
- [x] Implement quick fixes in small, safe commits.
- [x] Re-run build/tests to verify no regressions.
- [x] Document remaining warnings with rationale and suggested approach.

## Completed in this pass

- Refactored `SchoolCsvFields` parsing to centralize quote trimming and use invariant culture integer parsing.
- Reduced warnings in `SchoolCsvFields.cs` from 93 to 23 (removed CA1305, MA0011, MA0001, and many MA0165 warnings).
- Rebuilt solution with `--no-incremental` to regenerate accurate warning inventory.
