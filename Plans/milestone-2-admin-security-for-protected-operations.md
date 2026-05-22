# Milestone 2 Admin Security for Protected Operations

## 1. Overview

This plan turns Milestone 2 from the roadmap into delivery-ready work for authenticated admin access and authorization across privileged API operations.

The implementation goal is to move the API from its current unauthenticated state to an ASP.NET Core Identity-backed admin security model that protects school import and review moderation operations, emits the expected `401 Unauthorized` and `403 Forbidden` responses, and produces OpenAPI metadata that downstream UI work can plan against.

Current implementation context on 2026-05-22:

- `API/SummerBornInfo.Web/Program.cs` wires OpenAPI, EF Core, and school import services, but does not currently register authentication or authorization.
- `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` currently exposes school import routes under `/api/schools`, including an unauthenticated `POST /api/schools/import`.
- The solution does not currently include ASP.NET Core Identity package references or identity persistence types.

## 2. Roadmap Source or Existing Plan Context

Roadmap source:

- [Roadmap/initial-api-roadmap.md](C:\Projects\summer-born-info\Roadmap\initial-api-roadmap.md), Milestone 2: `Admin Security for Protected Operations`

Milestone 1 baseline source:

- [Plans/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Plans\milestone-1-contract-baseline-and-delivery-decisions.md)

Milestone 1 defines the downstream expectations that this milestone must implement:

- protected admin operations use ASP.NET Core Identity;
- admin-only routes sit under `/api/admin`;
- unauthenticated callers receive `401 Unauthorized`;
- authenticated non-admin callers receive `403 Forbidden`;
- the protected endpoint inventory for this milestone includes:
  - `POST /api/admin/school-imports`
  - `POST /api/admin/csa-application-reviews/{reviewId}/moderation`

## 3. Scope

This milestone includes:

- adding ASP.NET Core Identity to the API solution;
- introducing an admin-capable identity model and persistence wiring;
- defining and implementing the initial admin authentication entry path used by volunteer administrators;
- enforcing admin authorization on protected operations;
- aligning the school import trigger route with the Milestone 1 admin contract;
- creating the baseline moderation endpoint shell with admin protection, even if the full review workflow lands in Milestone 5;
- exposing correct protected-operation metadata in generated OpenAPI;
- adding integration coverage for successful admin access plus `401` and `403` failure paths;
- documenting any bootstrap or seed steps required to create the first admin identity in non-development environments.

## 4. Non-Goals

- public user accounts or public sign-in;
- review submission, reporting, or moderation business rules beyond the protected endpoint shell needed for this milestone;
- audit trail implementation for moderation;
- school search, URN lookup, or radius query delivery;
- production-grade secrets rotation, MFA, or delegated external identity providers;
- final UI implementation for admin sign-in.

## 5. Behaviour Scenarios

### Scenario 1: Unauthenticated caller attempts a protected school import

Given the caller is not authenticated  
When `POST /api/admin/school-imports` is called  
Then the API returns `401 Unauthorized`  
And the import handler does not run  
And the OpenAPI contract marks the operation as protected.

### Scenario 2: Authenticated non-admin caller attempts a protected school import

Given the caller is authenticated but does not have the admin role or claim  
When `POST /api/admin/school-imports` is called  
Then the API returns `403 Forbidden`  
And no import request is created.

### Scenario 3: Admin caller triggers a school import

Given the caller is authenticated as an admin  
When `POST /api/admin/school-imports` is called  
Then the API queues or records the import request  
And the API returns the Milestone 1 contract shape with an import request identifier and queued status  
And the route is no longer available as an unauthenticated public endpoint under `/api/schools/import`.

### Scenario 4: Unauthenticated caller attempts review moderation

Given the caller is not authenticated  
When `POST /api/admin/csa-application-reviews/{reviewId}/moderation` is called  
Then the API returns `401 Unauthorized`.

### Scenario 5: Authenticated non-admin caller attempts review moderation

Given the caller is authenticated but is not an admin  
When `POST /api/admin/csa-application-reviews/{reviewId}/moderation` is called  
Then the API returns `403 Forbidden`.

### Scenario 6: Admin caller moderates a review

Given the caller is authenticated as an admin  
And the supplied review exists  
When `POST /api/admin/csa-application-reviews/{reviewId}/moderation` is called with a supported decision  
Then the API performs the moderation action  
And returns the Milestone 1 moderation response shape.

Note:
If full review persistence is not yet implemented at the point Milestone 2 is delivered, this scenario may be satisfied by a thin protected endpoint shell backed by the agreed request and response contract, with Milestone 5 completing the business workflow behind it.

### Scenario 7: First admin bootstrap

Given a new environment with no existing admin users  
When the documented bootstrap flow is run  
Then at least one admin identity can be created safely  
And the resulting account can authenticate and access protected routes.

### Scenario 8: OpenAPI contract generation for protected operations

Given the API is running with OpenAPI enabled  
When the generated contract is inspected  
Then protected admin operations show the selected security scheme  
And the documented `401` and `403` responses remain visible to consumers.

## 6. Deliverables

1. Identity foundation in the API solution

- Add the required ASP.NET Core Identity packages and service registration.
- Extend infrastructure persistence so identity tables live alongside the existing application database.
- Introduce the application user type and any minimal role model required for admin authorization.

2. Admin authentication contract and implementation

- Implement the initial admin authentication path selected for this project.
- Define the request and response contract for project-specific sign-in and sign-out endpoints only under `/api/admin/auth/*`.
- Do not add a current-session endpoint as part of this milestone unless a later milestone introduces a new UI contract requirement.
- Ensure authentication works in local development, integration tests, and deployed environments.

3. Admin bootstrap and authorization model

- Define the single admin authorization rule used in Milestone 2, preferably role-based unless a stronger reason emerges during implementation.
- Implement a deterministic way to create the first admin account for development and non-development environments.
- Standardize the development bootstrap configuration on `AdminUserEmail` and `AdminUserPassword` values sourced from `dotnet user-secrets`.
- Prevent ordinary authenticated users from being treated as admins.

4. Protected route alignment

- Replace or redirect the current unauthenticated school import trigger route so the contract aligns with `POST /api/admin/school-imports`.
- Add the admin moderation endpoint at `POST /api/admin/csa-application-reviews/{reviewId}/moderation`.
- Apply authorization requirements at the route group or endpoint level so protected behaviour is hard to bypass.

5. Error and OpenAPI behaviour

- Return `401` for missing or invalid authentication and `403` for authenticated non-admin access.
- Ensure generated OpenAPI output includes the selected security scheme and protected-operation metadata.
- Add explicit response metadata for `401` and `403` on protected endpoints where the framework does not infer it clearly enough.

6. Automated verification

- Add HTTP-level integration tests for:
  - unauthenticated access to protected routes;
  - authenticated non-admin access to protected routes;
  - authenticated admin access to protected routes;
  - OpenAPI output including the chosen security scheme and protected endpoint annotations.
- Add focused tests for admin bootstrap or seeding logic if it contains custom behaviour.

7. Operational documentation

- Document the chosen auth flow, admin bootstrap process, required configuration, and any local-development shortcuts.
- Document the checked-in production bootstrap script location under the repo-root `ProductionScripts` folder and how operators are expected to run it.
- Record any intentional deferrals, especially if moderation business logic remains a Milestone 5 concern.

## 7. Technology Requirements and Decisions

Confirmed decisions:

- Use ASP.NET Core Identity as the authentication direction.
- Use cookie-based authentication for admin API access rather than bearer tokens.
- Keep protected operations under `/api/admin`.
- Do not expose framework-default Identity endpoints directly.
- Wrap auth operations in project-specific `/api/admin/auth/*` endpoints so the public contract only includes the intended admin sign-in and sign-out surface and does not leak registration or other unwanted framework endpoints.
- Use the existing PostgreSQL-backed `ApplicationDbContext` as the persistence boundary for identity data unless a strong implementation constraint appears.
- Limit the Milestone 2 admin auth endpoint surface to sign-in and sign-out only; do not add a current-session endpoint.
- In development, upsert the configured admin user at server start using `dotnet user-secrets` keys named `AdminUserEmail` and `AdminUserPassword`.
- For production, check in a SQL script artifact under the repo-root `ProductionScripts` folder that can be run to create the initial admin user and role assignment.

Implementation decisions:

- Use ASP.NET Core Identity with EF Core persistence in `SummerBornInfo.Infrastructure`.
- Use role-based authorization with a single initial `Admin` role for Milestone 2.
- Protect endpoint groups with authorization policies rather than ad hoc checks inside handlers.
- Implement project-specific auth endpoints under `/api/admin/auth/*`, with only the minimum contract needed for admin sign-in and sign-out.
- Verify auth behaviour primarily through integration tests in `API/SummerBornInfo.Web.Tests`.

Rationale:

- cookie auth matches the intended admin access model and avoids introducing token management work that is not needed for this milestone;
- project-specific auth endpoints preserve a clean long-term API contract and avoid leaking framework routes such as registration, while keeping the Milestone 2 UI surface intentionally small;
- development startup upsert keeps local setup lightweight and repeatable, and fixed `AdminUserEmail` and `AdminUserPassword` keys remove ambiguity across contributors;
- a checked-in production SQL script under `ProductionScripts` supports controlled first-user provisioning without requiring a public bootstrap endpoint or a separate generation tool in deployment workflows.

## 8. Dependencies and Sequencing

1. Add Identity packages, application user type, role model, and persistence wiring.
2. Register cookie authentication and authorization in `API/SummerBornInfo.Web/Program.cs`.
3. Implement project-specific `/api/admin/auth/*` endpoints over the required Identity operations.
4. Implement development admin upsert from `dotnet user-secrets`.
5. Add the checked-in production SQL bootstrap script under `ProductionScripts` for the first admin user and role assignment.
6. Align school import routes to `/api/admin/school-imports`.
7. Add the protected moderation endpoint contract shell.
8. Update OpenAPI metadata and error response declarations.
9. Add integration tests covering `401`, `403`, successful admin access, and auth endpoint behaviour.
10. Update documentation for local setup, secrets configuration, and production bootstrap.

Dependency notes:

- Milestone 5 depends on the moderation endpoint protection added here, but not necessarily on full moderation workflow implementation.
- The custom `/api/admin/auth/*` wrapper should be in place before OpenAPI verification so the generated contract reflects the intended public surface rather than framework internals.

### Sequential Task Breakdown

Deliver the milestone as the following one-task-at-a-time sequence, with one git commit after each completed task:

1. Task 1: Identity persistence foundation

- Add ASP.NET Core Identity package references, user and role types, and persistence wiring in the existing database context.
- Outcome: the solution can store identity users and roles without yet exposing auth routes.
- Commit boundary: persistence and schema wiring only.

2. Task 2: Authentication and authorization registration

- Register cookie authentication, authorization, and the milestone admin policy or role rule in the API startup path.
- Outcome: the application can evaluate authenticated versus admin-only access once protected endpoints are added.
- Commit boundary: service registration and middleware pipeline only.

3. Task 3: Project-specific admin auth endpoints

- Implement `/api/admin/auth/sign-in` and `/api/admin/auth/sign-out` using ASP.NET Core Identity without exposing framework-default Identity endpoints.
- Outcome: admins have the minimum contract needed to establish and end an authenticated session.
- Commit boundary: auth endpoint contract and handler implementation only.

4. Task 4: Development admin bootstrap

- Add startup-time development upsert for the configured admin account using `AdminUserEmail` and `AdminUserPassword` from `dotnet user-secrets`.
- Outcome: local and test environments have a deterministic first admin path.
- Commit boundary: development bootstrap behaviour only.

5. Task 5: Production bootstrap artifact

- Add the checked-in SQL bootstrap script under the repo-root `ProductionScripts` folder for initial admin creation and role assignment.
- Outcome: non-development environments have a documented first-admin provisioning path.
- Commit boundary: production bootstrap artifact and any directly supporting documentation only.

6. Task 6: School import route protection and alignment

- Move the import trigger contract to `POST /api/admin/school-imports` and remove or retire the old public route from the supported contract.
- Outcome: school import is admin-protected and aligned with the Milestone 1 route contract.
- Commit boundary: school import route alignment and protection only.

7. Task 7: Moderation endpoint protection shell

- Add `POST /api/admin/csa-application-reviews/{reviewId}/moderation` as an admin-protected endpoint shell that matches the agreed contract.
- Outcome: the protected moderation surface exists even if deeper workflow behaviour is deferred to Milestone 5.
- Commit boundary: moderation endpoint shell and authorization only.

8. Task 8: OpenAPI and error metadata

- Add or refine security scheme configuration plus explicit `401` and `403` response metadata for protected admin operations.
- Outcome: generated OpenAPI clearly describes protected routes and failure modes.
- Commit boundary: OpenAPI and response metadata only.

9. Task 9: Integration test coverage

- Add integration tests for unauthenticated, non-admin, and admin access paths, plus auth endpoint and OpenAPI coverage.
- Outcome: the milestone behaviour is regression-protected at the HTTP contract level.
- Commit boundary: automated verification only, unless a minimal production fix is required to make the tests pass.

10. Task 10: Operational documentation

- Update local and deployment-facing documentation for auth setup, development secrets, and production bootstrap usage.
- Outcome: contributors and operators can execute the supported bootstrap and sign-in flows without tribal knowledge.
- Commit boundary: documentation only.

### Task State Checklist

- [x] Task 1 complete: Identity persistence foundation committed.
- [x] Task 2 complete: Authentication and authorization registration committed.
- [x] Task 3 complete: Project-specific admin auth endpoints committed.
- [x] Task 4 complete: Development admin bootstrap committed.
- [x] Task 5 complete: Production bootstrap artifact committed.
- [ ] Task 6 complete: School import route protection and alignment committed.
- [ ] Task 7 complete: Moderation endpoint protection shell committed.
- [ ] Task 8 complete: OpenAPI and error metadata committed.
- [ ] Task 9 complete: Integration test coverage committed.
- [ ] Task 10 complete: Operational documentation committed.

## 9. Risks and Mitigations

- Cookie contract risk:
  Cookie auth requires correct secure-cookie settings, CSRF posture, and same-site behaviour for the intended admin client.
  Mitigation: make cookie configuration explicit in implementation and test the end-to-end sign-in flow with the expected client behaviour.

- Route migration risk:
  The current import route is public and uses a different path from the Milestone 1 baseline.
  Mitigation: treat route realignment as part of this milestone rather than a later cleanup.

- Bootstrap risk:
  Identity can be wired correctly but still be unusable if the development upsert or production SQL bootstrap path is incomplete or inconsistent.
  Mitigation: make both bootstrap paths first-class deliverables, standardize the development secrets keys as `AdminUserEmail` and `AdminUserPassword`, and verify the role assignment and login outcome for each path.

- Production script drift risk:
  A checked-in SQL bootstrap artifact can fall out of sync with the live Identity schema or password-hashing expectations if it is treated as a one-off document.
  Mitigation: keep the script adjacent to the implementation work in the repo-root `ProductionScripts` folder, document its assumptions, and verify it against the shipped schema during milestone completion.

- False-security risk:
  Handlers may appear protected locally while OpenAPI or endpoint metadata remains ambiguous for consumers.
  Mitigation: verify both runtime behaviour and generated OpenAPI output.

- Partial moderation risk:
  Milestone 2 needs a protected moderation contract before Milestone 5 delivers the full review workflow.
  Mitigation: explicitly allow a thin contract shell now and document what Milestone 5 must complete.

## 10. Resolved Clarifications

- The project-specific admin auth endpoint surface for Milestone 2 is sign-in and sign-out only.
- The development admin upsert flow must read `dotnet user-secrets` values from `AdminUserEmail` and `AdminUserPassword`.
- The production SQL bootstrap script must be a checked-in artifact stored under a repo-root `ProductionScripts` folder.

This plan is implementation-ready for Milestone 2 with the core contract, bootstrap configuration, and production script delivery format now explicitly fixed.

## 11. Completion Checklist

- [x] ASP.NET Core Identity packages and persistence are added to the API solution.
- [x] The application database can store identity users and roles.
- [x] Cookie authentication and authorization are registered in `API/SummerBornInfo.Web/Program.cs`.
- [x] Project-specific admin auth endpoints exist under `/api/admin/auth/*`, are limited to sign-in and sign-out, and do not expose registration or other unintended framework routes.
- [x] A development admin user is upserted at server start from `dotnet user-secrets` keys `AdminUserEmail` and `AdminUserPassword`.
- [x] A checked-in production SQL script exists under the repo-root `ProductionScripts` folder to create the initial admin user and role assignment.
- [ ] `POST /api/admin/school-imports` exists and requires admin authorization.
- [ ] The unauthenticated public school import trigger route is removed, redirected, or otherwise no longer part of the supported contract.
- [ ] `POST /api/admin/csa-application-reviews/{reviewId}/moderation` exists and requires admin authorization.
- [ ] Protected endpoints return `401` for unauthenticated callers.
- [ ] Protected endpoints return `403` for authenticated non-admin callers.
- [ ] Generated OpenAPI output shows the selected security scheme and protected-operation metadata.
- [ ] Integration tests cover unauthenticated, non-admin, and admin access paths.
- [ ] Local/deployment documentation explains auth setup and first-admin bootstrap.
