# Coverage Exclusion Plan

- [ ] Add shared coverlet configuration under `API` so `dotnet test` excludes third-party assemblies without maintaining a per-project inclusion list.
- [x] Add shared coverlet configuration under `API` so `dotnet test` excludes third-party assemblies without maintaining a per-project inclusion list.
- [x] Validate the existing API test command still passes and that the generated Cobertura report only contains repository assemblies.

Validation notes:
- `dotnet test --results-directory TestResults --report-xunit-trx --coverlet --coverlet-output-format cobertura` still passes, but under the current MTP `dotnet test` path it continues to include third-party assemblies in coverage output.
- `dotnet test --results-directory TestResults --report-xunit-trx --coverlet --coverlet-exclude-assemblies-without-sources MissingAny --coverlet-output-format cobertura` passes and limits the Cobertura report to repository assemblies.
