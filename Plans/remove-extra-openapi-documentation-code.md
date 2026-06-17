# Remove Extra OpenAPI Documentation Code

## 1. Overview

Remove the custom OpenAPI documentation code that adds school endpoint-specific metadata and keep only the authorization-focused OpenAPI extensions. The result should be a simpler web project where OpenAPI generation remains available for the admin authorization metadata, but the extra school documentation transformers, related tests, and obsolete references are deleted.

## 2. Roadmap Source or Existing Plan Context

This plan is based on the requested cleanup to:

- remove the extra OpenAPI and Swagger documentation code that is considered low-value and confusing,
- keep only the authorization extensions,
- clean up tests, and
- delete references to the removed OpenAPI extension methods.

Current state:

- `API/SummerBornInfo.Web/Program.cs` registers OpenAPI and Swagger UI in development and applies `AddAdminSecurityMetadata()`.
- `API/SummerBornInfo.Web/OpenApi/AdminSecurityOpenApiOptionsExtensions.cs` adds the cookie security scheme and protected-admin operation metadata. This is the portion to retain.
- `API/SummerBornInfo.Web/OpenApi/SchoolEndpointOpenApiExtensions.cs` adds custom response, parameter, and schema metadata for school endpoints. This is the primary code to remove.
- `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` still calls `AddSchoolCollectionOpenApiMetadata()`, `AddSchoolSearchOpenApiMetadata()`, and `AddNearbySchoolOpenApiMetadata()`.
- `API/SummerBornInfo.Web.Tests/OpenApi/OpenApiSecurityDocumentTests.cs` validates the retained authorization metadata.
- `API/SummerBornInfo.Web.Tests/OpenApi/SchoolEndpointsOpenApiDocumentTests.cs` validates the custom school OpenAPI metadata that should be removed.
- `API/SummerBornInfo.Web/GlobalUsings.cs` and `API/SummerBornInfo.Web.Tests/GlobalUsings.cs` include OpenAPI namespace imports that may no longer all be necessary after the cleanup.

Existing behavioral coverage already lives in the web integration tests for school endpoints, including:

- `API/SummerBornInfo.Web.Tests/API/Schools/GetAllSchoolsTests.cs`
- `API/SummerBornInfo.Web.Tests/API/Schools/SearchSchoolsTests.cs`
- `API/SummerBornInfo.Web.Tests/API/Schools/GetNearbySchoolsTests.cs`
- `API/SummerBornInfo.Web.Tests/API/Schools/GetSchoolByUrnTests.cs`

## 3. Scope

- Remove the custom school endpoint OpenAPI extension methods and their call sites.
- Retain the admin authorization OpenAPI extension path.
- Delete OpenAPI tests that only verify the removed school documentation customizations.
- Clean up any now-unused usings, namespaces, and references tied to the removed extension methods.
- Revalidate that the remaining authorization OpenAPI document behavior still works and that school endpoint behavioral tests remain the source of truth for endpoint contracts.

## 4. Non-Goals

- Reworking the behavior of the school endpoints themselves.
- Changing admin authorization behavior, cookie authentication behavior, or the protected endpoint policy model.
- Replacing the removed school OpenAPI metadata with a new custom documentation layer.
- Redesigning the overall API test strategy beyond removing obsolete document-focused coverage and keeping meaningful endpoint behavior coverage.
- Removing OpenAPI generation entirely from the application.

## 5. Behaviour Scenarios

### Protected admin operations still advertise auth requirements

Given the application is running in development and an OpenAPI document is requested,
when `/openapi/v1.json` is generated for protected `/api/admin/*` endpoints,
then the document should still include the identity cookie security scheme and the `401` / `403` responses added by `AdminSecurityOpenApiOptionsExtensions`.

### Anonymous admin auth endpoints stay exempt from security requirements

Given an admin authentication endpoint that allows anonymous access,
when its OpenAPI operation is generated,
then the authorization extension should not add a security requirement to that operation.

### School endpoints rely on default metadata only

Given the school discovery endpoints are mapped,
when the application builds its endpoints,
then those endpoints should no longer call custom OpenAPI extension methods and should rely only on default ASP.NET metadata plus their runtime behavior.

### School endpoint correctness is proven by behavior tests instead of document tests

Given a school collection, search, nearby, or URN lookup request,
when the existing web integration tests exercise those endpoints,
then response shapes, validation failures, pagination, and not-found behavior should still be verified without depending on custom OpenAPI document assertions.

## 6. Deliverables

1. Delete `API/SummerBornInfo.Web/OpenApi/SchoolEndpointOpenApiExtensions.cs`.
2. Update `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` to remove:
   - `AddSchoolCollectionOpenApiMetadata()`
   - `AddSchoolSearchOpenApiMetadata()`
   - `AddNearbySchoolOpenApiMetadata()`
3. Keep `API/SummerBornInfo.Web/OpenApi/AdminSecurityOpenApiOptionsExtensions.cs` in place and preserve its registration from `API/SummerBornInfo.Web/Program.cs`.
4. Review `API/SummerBornInfo.Web/GlobalUsings.cs` and remove only the OpenAPI-related global usings that are no longer required after deleting the school extension file, while keeping the imports needed by `AdminSecurityOpenApiOptionsExtensions.cs` and `Program.cs`.
5. Delete `API/SummerBornInfo.Web.Tests/OpenApi/SchoolEndpointsOpenApiDocumentTests.cs`.
6. Review `API/SummerBornInfo.Web.Tests/GlobalUsings.cs` and remove the `SummerBornInfo.Web.OpenApi` global using only if the remaining security document tests no longer need it.
   Recommended decision: keep the global using if `OpenApiSecurityDocumentTests` still references `AdminSecurityOpenApiOptionsExtensions.IdentityApplicationCookieSecuritySchemeName`; otherwise replace it with a local using in the test file.
7. Confirm there are no remaining references to the deleted extension methods or the removed school OpenAPI test fixture helpers anywhere in the solution.
8. Run targeted validation from `API/`:
   - `dotnet test --project SummerBornInfo.Web.Tests/SummerBornInfo.Web.Tests.csproj -- --filter-class "SummerBornInfo.Web.Tests.OpenApi.OpenApiSecurityDocumentTests"`
   - `dotnet test --project SummerBornInfo.Web.Tests/SummerBornInfo.Web.Tests.csproj -- --filter-class "SummerBornInfo.Web.Tests.API.Schools.GetAllSchoolsTests" --filter-class "SummerBornInfo.Web.Tests.API.Schools.SearchSchoolsTests" --filter-class "SummerBornInfo.Web.Tests.API.Schools.GetNearbySchoolsTests" --filter-class "SummerBornInfo.Web.Tests.API.Schools.GetSchoolByUrnTests"`
9. Run a focused build if needed to catch stale compile-time references after file deletion:
   - `dotnet build SummerBornInfo.Web.Tests/SummerBornInfo.Web.Tests.csproj`

## 7. Technology Requirements and Decisions

- Keep `Microsoft.AspNetCore.OpenApi` because the retained authorization extension still depends on the OpenAPI transformer pipeline.
- Keep `Swashbuckle.AspNetCore.SwaggerUI` and the existing development-time Swagger UI mapping unless the user separately decides to remove the entire UI surface. This plan assumes the requested cleanup targets the extra custom documentation code, not the remaining development document viewer.
- Do not introduce replacement transformer classes, filters, or new OpenAPI customization packages.
- Prefer relying on minimal API runtime metadata and endpoint integration tests over hand-maintained OpenAPI transformer logic for school endpoint documentation.

## 8. Dependencies and Sequencing

1. Remove the school endpoint OpenAPI extension file and the three call sites in `SchoolEndpoints.cs`.
2. Clean up web-project usings and confirm only the authorization OpenAPI code remains under `API/SummerBornInfo.Web/OpenApi/`.
3. Remove the obsolete school OpenAPI document tests and tidy any leftover test usings.
4. Search the solution for deleted extension method references and remove any stragglers.
5. Run the retained authorization OpenAPI tests.
6. Run the school endpoint behavioral tests to confirm coverage remains where it matters.
7. Build the web test project if the targeted tests do not already prove all compile-time cleanup paths.

## 9. Risks and Mitigations

- Risk: Removing the school OpenAPI transformer file could leave stale call sites or global usings that break compilation.
  Mitigation: Delete the file together with all three `SchoolEndpoints` call sites, then run a solution search and a focused build.

- Risk: Removing document tests could unintentionally drop meaningful contract coverage.
  Mitigation: Lean on the existing school endpoint integration tests that already verify live request/response behavior, validation rules, pagination, and not-found outcomes.

- Risk: Cleanup might accidentally strip imports or package references needed by the remaining authorization OpenAPI extension.
  Mitigation: Preserve `Program.cs` OpenAPI registration, keep `AdminSecurityOpenApiOptionsExtensions.cs`, and run the retained `OpenApiSecurityDocumentTests`.

- Risk: The user may actually want all Swagger UI and OpenAPI document publication removed, not just the extra school metadata.
  Mitigation: Treat this plan as scoped to removing custom school documentation code while retaining the authorization extension path, and confirm broader removal separately before implementation if that goal changes.

## 10. Unknowns and Required Clarifications

No blocking unknowns for this plan.

Assumption captured for implementation: the application should continue exposing the development OpenAPI document and Swagger UI so the remaining authorization extension still has a document surface to enrich. If the intended end state is to remove the OpenAPI endpoint and Swagger UI entirely, the implementation plan should be revised before coding because the retained authorization extension would no longer provide practical value.

## 11. Completion Checklist

- [x] `API/SummerBornInfo.Web/OpenApi/SchoolEndpointOpenApiExtensions.cs` is deleted.
- [x] `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` no longer references any school OpenAPI extension methods.
- [x] `API/SummerBornInfo.Web/OpenApi/AdminSecurityOpenApiOptionsExtensions.cs` remains intact and registered.
- [x] Obsolete OpenAPI-related global usings and references are removed only where they are no longer needed.
- [x] `API/SummerBornInfo.Web.Tests/OpenApi/SchoolEndpointsOpenApiDocumentTests.cs` is deleted.
- [x] No solution files reference `AddSchoolCollectionOpenApiMetadata`, `AddSchoolSearchOpenApiMetadata`, or `AddNearbySchoolOpenApiMetadata`.
- [x] Authorization OpenAPI document tests pass.
- [ ] School endpoint behavioral integration tests pass after the cleanup.

## 12. Implementation Progress

- [x] Step 1: Remove the school endpoint OpenAPI extension file and the three call sites in `SchoolEndpoints.cs`.
- [x] Step 2: Clean up web-project usings and confirm only the authorization OpenAPI code remains under `API/SummerBornInfo.Web/OpenApi/`.
- [x] Step 3: Remove the obsolete school OpenAPI document tests and tidy any leftover test usings.
- [x] Step 4: Search the solution for deleted extension method references and remove any stragglers.
- [x] Step 5: Run the retained authorization OpenAPI tests.
- [ ] Step 6: Run the school endpoint behavioral tests to confirm coverage remains where it matters.
- [ ] Step 7: Build the web test project if needed to catch stale compile-time references after the cleanup.
