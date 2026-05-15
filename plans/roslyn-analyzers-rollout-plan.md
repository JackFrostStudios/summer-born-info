# Roslyn Analyzers Rollout Plan

## Goal

Raise code quality and security signal in this solution with a small, maintainable analyzer set that fits the existing .NET 10 SDK-style projects and central package management setup.

## Recommended Analyzer Set

### 1. Built-in .NET SDK analyzers

Use the analyzers that already ship with the .NET SDK as the baseline and configure them centrally.

- Enable build enforcement through `AnalysisLevel`, `AnalysisMode`, and `EnforceCodeStyleInBuild`.
- Treat these as the primary source for:
  - correctness and reliability (`CA*`)
  - maintainability and performance (`CA*`)
  - code style and simplification (`IDE*`)
  - framework deprecation warnings (`SYSLIB*`)

Why this belongs in the plan:

- The repo already targets `net10.0`, so the SDK analyzers are the most natural baseline.
- They are first-party, actively maintained, and easy to tune per-rule in `.editorconfig`.

### 2. `Meziantou.Analyzer`

Add `Meziantou.Analyzer` as the single third-party analyzer package for extra correctness, usage, and security-focused checks that the SDK analyzers do not cover as well.

Why this is a good fit:

- It is current and actively maintained.
- It adds practical rules around API usage, cancellation, disposal, string handling, culture, and defensive coding without forcing a style-only workflow.
- It overlaps less noisily with the SDK analyzers than broader analyzer bundles.

### 3. xUnit analyzers

Rely on the xUnit analyzers already brought in by `xunit.v3` for test projects.

Why this is enough:

- The test projects already reference `xunit.v3`.
- No extra package is needed unless the test package structure changes later.

## Analyzer Packs To Avoid

### `SecurityCodeScan.VS2019`

Do not add this as part of the initial rollout.

Reason:

- The latest package on NuGet appears stale for this stack, with its latest update in September 2022.
- For a .NET 10 codebase, that is a poor maintenance signal for a central security gate.

### `Roslynator.Analyzers`

Do not add this in the first pass.

Reason:

- It is active and useful, but it overlaps heavily with SDK and Meziantou rules.
- Adding it now would increase issue volume and tuning cost before we establish a clean baseline.

### Style-heavy packs such as StyleCop

Do not add style-first analyzers during the quality and security rollout.

Reason:

- They would blur the goal by producing large formatting and naming churn that is not tightly connected to correctness or security.

## Central Configuration Shape

Use a three-part setup under the existing repository structure:

1. `API/Directory.Packages.props`
   - Add analyzer package versions here.
2. `API/Directory.Build.props`
   - Add shared analyzer `PackageReference` entries with `PrivateAssets="all"` and `IncludeAssets="runtime; build; native; contentfiles; analyzers"`.
   - Set shared analysis properties here.
3. `.editorconfig`
   - Keep rule severities and targeted suppressions here.

## Initial Severity Strategy

Start with a staged policy instead of making the whole solution warning-clean in one step.

### Phase 1 build blockers

Set these to `error` early:

- nullability and compiler safety warnings that already indicate likely bugs
- high-confidence reliability rules
- high-confidence security rules from SDK or Meziantou where the fix is straightforward

### Phase 2 cleanup warnings

Set these to `warning` first:

- disposal and async-usage rules
- cancellation-token propagation rules
- culture/globalization usage rules
- maintainability and simplification rules

### Phase 3 suggestions

Keep these as `suggestion` or `silent` initially:

- style and readability preferences
- low-signal refactor prompts

## Repo-Specific Hotspots Already Identified

These are likely to raise analyzer findings once enforcement is turned on:

### Cancellation propagation

- `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs`
  - Endpoint handlers currently pass `CancellationToken.None` instead of the request token.

### Synchronous blocking over async code

- `API/SummerBornInfo.Infrastructure/Persistence/LargeObjects/LargeObjectStream.cs`
  - Uses `GetAwaiter().GetResult()` in stream members.

### Disposable scope handling

- `API/SummerBornInfo.Web/Program.cs`
  - Startup scope creation in development setup should be reviewed for disposal-friendly patterns.
- `API/SummerBornInfo.Web/BackgroundServices/ProcessSchoolBulkImportBackgroundService.cs`
  - Scope lifetime and service resolution patterns should be checked under analyzer enforcement.

### Async method shape

- `API/SummerBornInfo.Infrastructure/Persistence/LargeObjects/LargeObjectStream.cs`
  - `async` methods that immediately throw should be simplified to non-async implementations returning faulted tasks/value tasks if analyzer noise appears.

## Implementation Plan

- [ ] Step 1: Add central analyzer configuration scaffolding.
  - Create `API/Directory.Build.props` if it does not exist.
  - Add `Meziantou.Analyzer` version to `API/Directory.Packages.props`.
  - Add or update repo-root `.editorconfig` with analyzer severity policy.

- [ ] Step 2: Enable analyzers in a non-breaking mode first.
  - Set `AnalysisLevel` to a fixed SDK band instead of floating `latest`.
  - Use `AnalysisMode` at a conservative starting level such as `Recommended`.
  - Keep broad `TreatWarningsAsErrors` off for the first pass.

- [ ] Step 3: Run restores/builds for the buildable subset of the solution.
  - Validate analyzer output on the main application projects first.
  - Exclude or separately handle the Aspire host project if workload restore remains a blocker.
  - Resolve current environment issues that prevent a clean analyzer run:
    - NuGet access is currently blocked in this workspace.
    - The Aspire AppHost SDK is not currently resolving in the attempted solution build.

- [ ] Step 4: Fix high-confidence issues first.
  - Propagate real request cancellation tokens through web endpoints.
  - Remove synchronous waits where practical.
  - Clean up disposal and scope-lifetime issues.

- [ ] Step 5: Tighten severity after the first cleanup pass.
  - Promote the most valuable rules from warning to error.
  - Add narrow suppressions only where there is a documented reason.

- [ ] Step 6: Repeat on test projects and infrastructure edges.
  - Review test-only suppressions separately from production code suppressions.
  - Keep xUnit analyzer fixes local to test projects.

- [ ] Step 7: Lock the policy into CI.
  - Add a build step that fails on the selected analyzer error set.
  - Keep warning growth visible without blocking all delivery immediately.

## Delivery Sequence

Recommended commit sequence once implementation starts:

1. `chore: add central roslyn analyzer configuration`
2. `chore: fix first-pass analyzer violations in web and infrastructure`
3. `chore: tighten analyzer severities and CI enforcement`

## Notes

- This plan intentionally favors a curated analyzer set over a maximal one.
- The fastest path to value here is fewer analyzers, clearer severities, and disciplined suppression rules.
