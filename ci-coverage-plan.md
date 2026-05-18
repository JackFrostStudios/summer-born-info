- [x] Review the current CI workflow inputs and map the example coverage flow onto this repo's API-only test setup
- [x] Update `.github/workflows/ci.yml` to publish coverage artifacts, generate a sticky PR coverage comment, and keep the 90% coverage gate with simpler reporting
- [x] Run the relevant validation commands locally and capture any follow-up constraints

Validation notes:
- `dotnet test` must continue running from the `API` directory so Microsoft Testing Platform picks up the repo's `global.json`.
- `--report-xunit-trx` emits one `.trx` file per test assembly into `API/TestResults`, and coverage remains available as `API/TestResults/coverage.cobertura.xml`.
