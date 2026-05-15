# Roslyn Warning Triage (Small/Non-Controversial vs Controversial)

## Safe, mostly mechanical fixes (recommended next)

- `IDE0008` (use explicit type instead of `var`): mechanical but noisy; safe where clarity improves.
- `MA0165` (prefer interpolated strings): safe text-only transform.
- `MA0007` (trailing comma): safe formatting-only.
- `IDE0055` (formatting): safe formatting-only.
- `IDE0011` (add braces): safe readability improvement.
- `CA1305` / `MA0011` for numeric/date parsing: safe when using `CultureInfo.InvariantCulture` for persisted/imported data.

## Potentially controversial / no easy fix

- `IDE0058` (expression value never used): often tied to fluent APIs; “fixes” can reduce readability by forcing discards/temporary variables.
- `MA0004` (`ConfigureAwait(false)`): broad cross-cutting style choice; applying everywhere can hurt readability and may conflict with team conventions in app-layer code.
- `CA1711` (type name ends with `Queue`): rename is breaking and cross-cutting.
- `CA1016` (assembly version): policy/packaging decision across projects.
- `CA2201` (throwing `Exception`): should be fixed, but requires semantic exception design and tests.
- `MA0042` async recommendations in tests/IO: often desirable but can change test flow and complexity.
- `CA1051`, `CA1707`: naming/visibility conventions that may require API shape changes.

## Change implemented in this pass

- File changed: `API/SummerBornInfo.Features/Schools/Commands/ProcessImportFile/FileProcessing/SchoolCsvFields.cs`
- Warning count in that file: `93 -> 23`
- Warnings removed from that file include `CA1305`, `MA0011`, `MA0001`, plus many `MA0165` from repeated string manipulation.

## Suggested next low-risk batch

- Apply `IDE0011`, `MA0007`, and selected `IDE0055` across source projects.
- Apply `CA1305`/`MA0011` invariant-culture parsing fixes in import and persistence boundaries.
- Defer `IDE0058` and `MA0004` to an explicit style decision.
