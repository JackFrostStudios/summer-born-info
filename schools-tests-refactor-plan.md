# Schools Integration Test Refactor Plan

- [x] Step 1: Migrate SchoolsIntegrationTests.cs into API/Schools/GetAllSchoolsTests.cs, API/Schools/CreateSchoolImportRequestTests.cs, and API/Schools/GetSchoolImportRequestTests.cs so the folder structure mirrors the implementation slice without changing behavior.
- [x] Step 2: Run the schools-focused web integration tests to confirm the migration introduced no regressions.
- [ ] Step 3: Improve GetAllSchoolsTests to assert returned school data and cursor pagination behavior.
- [ ] Step 4: Re-run the affected tests to confirm the stronger assertions pass.

