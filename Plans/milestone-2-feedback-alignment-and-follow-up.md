# Milestone 2 Feedback Alignment and Follow-up

## 1. Overview

This plan captures post-delivery feedback after Milestone 2 and turns it into a focused follow-up slice that aligns the documented API contract with the implementation already in the repository, while also closing the remaining behaviour and test-structure gaps.

As of 2026-05-23, much of the current implementation already uses a bare `Id` property in response DTOs such as `SchoolResponse`, `ImportSchoolsResponse`, and `GetSchoolBulkImportStatusResponse`, but the Milestone 1 plan and Milestone 1 contract documentation still describe prefixed response identifiers such as `schoolId`, `importRequestId`, and `reviewId` in multiple places. The codebase also still contains at least one API response shape that uses a prefixed identifier field today: `ModerateCsaApplicationReviewResponse` exposes `ReviewId`. In addition, the GET school import status endpoint remains publicly accessible under `/api/schools/import/{requestId}`, and admin-authenticated client setup is duplicated across HTTP integration test classes.

The goal of this plan is to make identifier naming consistent across all API responses and their supporting documentation, protect import-status retrieval with the same admin-only rules already applied to import creation, and remove avoidable duplication from the web test suite.

## 2. Roadmap Source or Existing Plan Context

Roadmap and milestone context:

- [Roadmap/initial-api-roadmap.md](C:\Projects\summer-born-info\Roadmap\initial-api-roadmap.md)
- [Plans/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Plans\milestone-1-contract-baseline-and-delivery-decisions.md)
- [Plans/milestone-2-admin-security-for-protected-operations.md](C:\Projects\summer-born-info\Plans\milestone-2-admin-security-for-protected-operations.md)
- [Documentation/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Documentation\milestone-1-contract-baseline-and-delivery-decisions.md)

Relevant implementation context confirmed from the current codebase:

- Most current response DTOs, including school and import payloads, already expose `Id`, not prefixed identifier field names.
- `API/SummerBornInfo.Features/CsaApplicationReviews/Commands/Moderate/ModerateCsaApplicationReviewResponse.cs` still exposes `ReviewId`, so the response-identifier feedback is not purely documentation-only.
- `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` still exposes `GET /api/schools/import/{requestId}` without admin authorization.
- `API/SummerBornInfo.Web/API/Admin/SchoolImports/AdminSchoolImportEndpoints.cs` already protects `POST /api/admin/school-imports` with the admin policy.
- `API/SummerBornInfo.Web.Tests/API/Schools/CreateSchoolsImportRequestTests.cs` and `API/SummerBornInfo.Web.Tests/API/CsaApplicationReviews/ModerateCsaApplicationReviewTests.cs` duplicate user seeding and authenticated-client creation logic that belongs in `API/SummerBornInfo.Web.Tests/TestFramework/WebIntegrationTestBase.cs`.

User feedback to implement:

1. All API responses should use a generic `Id` field rather than resource-prefixed identifier fields.
2. The get school import request API should also be admin only and require admin authentication.
3. The duplicated admin-user generation and authenticated-client setup in tests should move to the shared base test class.

## 3. Scope

This follow-up includes:

- auditing all currently implemented API response payloads and HTTP-level tests to confirm entity identifiers use `Id` rather than resource-prefixed names;
- updating code where any implemented response DTO still exposes a prefixed identifier field;
- updating Milestone 1 planning and contract documentation so response identifier guidance matches the cross-API `Id` convention;
- reviewing the currently documented route and payload language across school import and CSA application review contracts, correcting any examples that imply prefixed response identifiers rather than generic `Id`;
- protecting school import status retrieval behind admin authentication and authorization;
- aligning the import-status route contract and tests with the protected admin surface already established for school import creation;
- extracting shared identity-seeding and authenticated-client helper logic into the common web integration test base;
- updating or adding HTTP integration tests to cover `401`, `403`, and successful admin access for import-status retrieval after the authorization change.

## 4. Non-Goals

This follow-up does not include:

- redesigning the wider route structure for school lookup, school reviews, or moderation beyond the documented identifier terminology and the import-status authorization change;
- renaming internal domain entity properties, EF keys, or command/query request parameters that are not part of the public response contract;
- changing the already delivered admin sign-in implementation;
- introducing non-admin authenticated roles beyond what already exists;
- broad test-framework redesign outside the duplicated authentication helper extraction.

## 5. Behaviour Scenarios

### Scenario 1: Consumer reads the updated Milestone 1 artifacts

Given the Milestone 1 plan and contract documentation are used as the baseline reference  
When a contributor or downstream consumer reads the response schema guidance  
Then the documents consistently describe response identifiers as `Id`  
And the documentation no longer claims that school response payloads use `schoolId`  
And route parameters remain distinguished from response fields where that matters.

### Scenario 2: Public school response payloads use unprefixed identifiers

Given a caller requests school data from the existing API surface  
When the response contains a school or related lookup resource  
Then the payload uses `Id` for the entity identifier  
And no response contract documentation or automated verification expects `schoolId`, `requestId`, or other prefixed identifier fields in response bodies unless there is a separate non-identifier business field.

### Scenario 3: Non-school API response payloads also use unprefixed identifiers

Given a caller uses another implemented API such as admin moderation or school import operations  
When the response body includes the identifier of the primary returned entity  
Then that identifier is exposed as `Id` rather than `ReviewId`, `ImportRequestId`, or another resource-prefixed response field  
And tests and documentation assert the same convention consistently across APIs.

### Scenario 4: Unauthenticated caller requests school import status

Given the caller is not authenticated  
When `GET /api/admin/school-imports/{requestId}` is called  
Then the API returns `401 Unauthorized`  
And no import-status payload is returned.

### Scenario 5: Authenticated non-admin caller requests school import status

Given the caller is authenticated but does not satisfy the admin policy  
When the protected import-status endpoint is called  
Then the API returns `403 Forbidden`.

### Scenario 6: Authenticated admin caller requests school import status

Given the caller is authenticated as an admin  
And the requested import exists  
When the protected import-status endpoint is called  
Then the API returns `200 OK`  
And the response body includes the current import state using the existing `Id`, `Status`, `LinesProcessed`, and `Failures` payload shape  
And internal-only fields such as `ContentId` remain excluded.

### Scenario 7: Web integration tests need an authenticated client

Given a web integration test needs an authenticated user  
When the test creates an admin or non-admin client through the shared test framework  
Then the user creation, optional admin role assignment, cookie generation, and client setup happen through reusable base-class helpers  
And individual test classes only specify the user characteristics relevant to the scenario they are covering.

## 6. Deliverables

1. Milestone 1 planning alignment

- Update [Plans/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Plans\milestone-1-contract-baseline-and-delivery-decisions.md) so it no longer defines `schoolId` or any other prefixed name as the canonical response identifier.
- Replace response-schema guidance that conflicts with the shipped `Id` DTO convention, while keeping route-parameter intent explicit where school-specific routes are discussed.

2. Milestone 1 contract-document alignment

- Update [Documentation/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Documentation\milestone-1-contract-baseline-and-delivery-decisions.md) so response examples and schema tables use `Id` for entity identifiers in payloads.
- Review school, review, moderation, report, and import examples for stale prefixed identifier names such as `schoolId`, `reviewId`, and `importRequestId`, correcting them where they describe response bodies rather than route parameters.

3. Cross-API response identifier audit and code alignment

- Review all currently implemented API response DTOs for prefixed identifier fields.
- Update any implemented response contracts that still return fields such as `ReviewId` to use `Id` instead.
- Update affected endpoint tests and OpenAPI-facing expectations so the `Id` convention is verified consistently across APIs.

4. Protected import-status endpoint

- Move or remap the school import status GET operation onto the admin-protected import route group rather than the public `/api/schools` group.
- Support the import-status endpoint under `GET /api/admin/school-imports/{requestId}` so it sits alongside `POST /api/admin/school-imports`.
- Apply the existing admin authorization policy so import-status retrieval follows the same protection model as import creation.
- Preserve the current successful response shape unless a concrete compatibility issue is discovered during implementation.

5. HTTP integration coverage for import-status authorization

- Update existing import-status tests so successful retrieval uses an authenticated admin client.
- Add or update tests covering unauthenticated `401 Unauthorized` and authenticated non-admin `403 Forbidden` outcomes for the import-status endpoint.
- Preserve existing verification that successful responses exclude `ContentId` and return the expected status/failure data.

6. Shared web-test authentication helpers

- Add reusable helper methods to `WebIntegrationTestBase` for:
  - creating seeded users,
  - optionally assigning the admin role,
  - generating an authenticated client with the correct application cookie.
- Refactor existing test classes that currently duplicate this logic to use the shared helpers.

## 7. Technology Requirements and Decisions

Confirmed decisions:

- Public API response payloads should expose generic `Id` fields rather than resource-prefixed identifier names across all implemented APIs, not only school endpoints.
- Admin-only protection for school import operations applies to both creation and status retrieval.
- The supported school import status route is `GET /api/admin/school-imports/{requestId}` so it sits alongside the create route under the same admin path family.
- Shared authentication test helpers belong in `API/SummerBornInfo.Web.Tests/TestFramework/WebIntegrationTestBase.cs` unless a narrower abstraction becomes necessary during refactoring.

Implementation decisions for this slice:

- Prefer relocating the import-status GET route into the existing admin school-import endpoint group so creation and retrieval share one authorization boundary and route family.
- Update any currently implemented response DTOs that still expose prefixed identifiers, including moderation responses, as part of this slice rather than deferring them to a later cleanup.
- Keep query/handler parameter names such as `requestId` internally where they improve readability for route binding; the feedback applies to response payload fields, not every code identifier.
- Refactor test duplication by moving the current user/role/cookie setup code into protected base-class helpers rather than introducing a separate test utility project or fixture layer.

Constraints:

- The follow-up should not create a second conflicting contract story alongside Milestone 1; the original milestone artifacts must be corrected rather than left stale.
- The authorization change must preserve current admin success behaviour and only tighten access for callers who should not see import-status details.
- Test refactoring should reduce duplication without making individual endpoint tests harder to read.

## 8. Dependencies and Sequencing

1. Audit implemented API responses and tests for prefixed identifier fields.
2. Extract duplicated authenticated-client setup into `WebIntegrationTestBase`.
3. Update any implemented response DTOs and tests that still expose prefixed identifier names.
4. Move the import-status GET endpoint onto the admin-protected route group and apply authorization.
5. Update and extend web integration tests for the new import-status auth behaviour.
6. Review and correct the Milestone 1 plan language around response identifiers.
7. Update the Milestone 1 contract documentation examples and schema notes so they match the implemented `Id` payload convention.

Recommended delivery order:

- Start with the response-contract audit so any non-school API drift is identified before implementation begins.
- Extract the shared test-helper logic early so both identifier-alignment tests and authorization tests can use the final reusable pattern.
- Apply response DTO fixes and import-status authorization changes next, updating tests in the same slice.
- Finish by correcting the Milestone 1 plan and documentation so the written baseline matches the delivered behaviour across the full API surface.

## 9. Risks and Mitigations

- Risk:
  Documentation is corrected only partially, leaving some Milestone 1 examples or statements still referring to `schoolId` as a response field.
  Mitigation:
  Perform a focused terminology sweep across both the plan and the milestone documentation, distinguishing response payload fields from route parameters and checking non-school examples too.

- Risk:
  The team updates school responses but misses other implemented API payloads such as moderation responses, leaving the naming convention inconsistent across endpoints.
  Mitigation:
  Treat the work as a cross-API response audit with explicit DTO and HTTP-test verification rather than a school-only cleanup.

- Risk:
  Protecting import-status retrieval changes the route path or auth boundary in a way that breaks the existing integration tests without clearly expressing the intended supported contract.
  Mitigation:
  Update the endpoint and tests together, and keep the plan explicit about which route family is intended to remain supported.

- Risk:
  Moving authentication helpers to the base class could centralize too much logic and make tests opaque.
  Mitigation:
  Keep the helpers small and scenario-oriented, for example separate seeded-user and authenticated-client methods, so tests remain readable.

- Risk:
  Response identifier standardization could be misinterpreted as requiring internal renames across commands, domain entities, and route parameter names.
  Mitigation:
  State clearly in the plan that the feedback applies to public response payloads and supporting documentation, not every internal identifier variable.

## 10. Unknowns and Required Clarifications

There are no blocking clarifications required to implement this follow-up plan.

Resolved route clarification:

- The protected import-status endpoint should be implemented as `GET /api/admin/school-imports/{requestId}` so it sits alongside the create endpoint under the `api/admin/...` path family.

## 11. Completion Checklist

- [ ] All currently implemented API response DTOs use `Id` rather than prefixed identifier field names.
- [ ] HTTP-level tests for implemented APIs assert the `Id` convention consistently.
- [ ] Milestone 1 plan language no longer describes `schoolId` or any other prefixed name as the canonical response identifier.
- [ ] Milestone 1 contract documentation uses `Id` in response schema tables and examples where the payload represents an entity identifier.
- [ ] Any remaining `schoolId`, `reviewId`, or `importRequestId` references in milestone documentation are limited to route parameters or other cases where they are intentionally not response fields.
- [ ] School import status retrieval is protected by admin authentication and authorization.
- [ ] School import status retrieval is supported at `GET /api/admin/school-imports/{requestId}`.
- [ ] Unauthenticated import-status requests return `401 Unauthorized`.
- [ ] Authenticated non-admin import-status requests return `403 Forbidden`.
- [ ] Authenticated admin import-status requests still return the expected success payload and omit `ContentId`.
- [ ] Reusable seeded-user and authenticated-client helpers exist in `WebIntegrationTestBase`.
- [ ] Duplicated authentication helper code is removed from the affected web test classes.
- [ ] Automated tests cover the protected import-status behaviour and pass with the refactored shared helper approach.
