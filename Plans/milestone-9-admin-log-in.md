# Milestone 9: Admin Log In Implementation Plan

## 1. Overview

Deliver the first protected admin access flow in the Angular UI so authorized volunteer administrators can sign in, sign out, and reach admin-only UI areas. This milestone should also establish the reusable frontend authentication pattern that Milestone 10 can use for the admin bulk school import workflow.

The implementation should stay intentionally small: a dedicated admin login route, an authenticated admin route boundary, an auth API service, session state handling, sign-out behaviour, and tests that prove the guard and failure paths are reliable.

## 2. Roadmap Source or Existing Plan Context

Source roadmap: `Roadmap/initial-ui-roadmap.md`, Milestone 9: Admin Log In.

Roadmap objective:

> Deliver the first protected admin access flow in the UI so authorized volunteers can sign in and reach privileged workflows.

Roadmap dependencies:

- Milestone 7 application baseline.
- API authentication contract from the API roadmap.
- Agreement on session and sign-out behaviour.

Roadmap exit criteria:

- Admin users can sign in through the UI.
- Authentication failures are handled clearly.
- Protected UI areas can be gated.
- The login flow is ready to support follow-on admin features.

Confirmed API contract:

- Admin auth endpoints are project-specific routes under `/api/admin/auth/*`.
- `POST /api/admin/auth/sign-in` accepts JSON `{ "email": string, "password": string }`.
- Successful admin sign-in returns `204 No Content` and sets the ASP.NET Core Identity application cookie.
- Invalid credentials return `401 Unauthorized`.
- Valid credentials for a non-admin user return `403 Forbidden`.
- `POST /api/admin/auth/sign-out` clears the same cookie and returns `204 No Content`.
- Protected admin operations live under `/api/admin/*` and return `401 Unauthorized` for unauthenticated callers and `403 Forbidden` for authenticated non-admin callers.

Current UI context:

- Routes are composed in `UI/src/app/app.routes.ts` under `RootShell`.
- Secondary routes should lazy-load by default.
- Route accessibility metadata is declared alongside routes through `defineRouteAccessibility`.
- Feature code belongs under `UI/src/app/features/`.
- The UI API URL convention is same-origin root-relative `/api/...` requests, documented in `UI/AI_PROJECT_GUIDE.md`.
- The UI does not currently include a local Angular dev proxy; plain `ng serve` does not provide live same-origin API calls unless co-hosted or proxied.
- User-facing template text must use Angular i18n metadata and refresh `UI/src/locale/messages.xlf`.

## 3. Scope

- Add a dedicated admin login page at `/admin/login`.
- Add a protected admin landing route at `/admin` that proves route gating and gives Milestone 10 a stable place to add import navigation.
- Add client-side authentication state and API integration for admin sign-in and sign-out.
- Add a route guard that prevents unauthenticated users from reaching protected admin routes.
- Add clear, accessible feedback for invalid credentials, non-admin access, request failures, loading, and successful sign-in redirection.
- Add a visible sign-out affordance on authenticated admin screens.
- Establish the first documented UI-to-API integration convention needed for authenticated cookie requests.
- Add focused unit, routing, and accessibility coverage for the login flow and guard behaviour.

## 4. Non-Goals

- Password reset, account creation, invite management, email verification, or profile editing.
- A full admin dashboard beyond a minimal authenticated landing page.
- Role-management UI.
- Long-lived "remember me" sessions.
- Refresh-token handling or JWT storage.
- Public user authentication.
- Milestone 10 bulk school import controls, upload handling, status polling, or import result display.
- Production admin bootstrap tooling, which remains owned by the API and operations documentation.

## 5. Behaviour Scenarios

### 5.1 Admin Opens the Login Page

Given a visitor navigates to `/admin/login`, when the route loads, then the UI shows a focused admin sign-in form with email and password fields, a submit button, and concise contextual copy for volunteer administrators.

Acceptance conditions:

- The page has route accessibility metadata with an appropriate document title, focus target, and skip link target.
- The form uses semantic labels, native input types, autocomplete hints, and visible keyboard focus states.
- The initial submit action is disabled or blocked until required fields contain non-empty values.
- The layout is responsive across mobile and desktop viewports.

### 5.2 Admin Signs In Successfully

Given an authorized admin enters valid credentials, when they submit the login form, then the UI posts to `/api/admin/auth/sign-in` with `credentials: 'include'`, receives `204 No Content`, marks the admin session as authenticated for the current browser runtime, and redirects them to the protected admin landing route.

Acceptance conditions:

- The submit button communicates pending state and prevents duplicate submissions while the request is in flight.
- No password value is logged, persisted to local storage, copied into route state, or displayed after submission.
- The authenticated route is reachable without immediately sending the user back to login.
- The user lands on the originally requested protected route when they were redirected to login from a guard.

### 5.3 Invalid Credentials Are Rejected

Given an admin enters an unknown email or incorrect password, when the API returns `401 Unauthorized`, then the UI keeps them on `/admin/login`, clears only the password field, and shows a clear form-level error.

Acceptance conditions:

- The error message does not reveal whether the email or password was wrong.
- Focus moves to the form-level error or the error is announced through an accessible live region.
- The email value remains available for correction.
- The user can edit the fields and submit again.

### 5.4 Non-Admin Credentials Are Rejected

Given a valid non-admin user enters credentials, when the API returns `403 Forbidden`, then the UI keeps them on `/admin/login` and shows a clear access-denied message.

Acceptance conditions:

- The message distinguishes lack of admin access from ordinary invalid credentials without exposing sensitive account details.
- No authenticated admin session is recorded client-side.
- The user can attempt a different account.

### 5.5 Network or Unexpected API Failure

Given the sign-in request cannot complete or the API returns an unexpected status, when the user submits the form, then the UI keeps the form available and shows a recoverable failure message.

Acceptance conditions:

- The pending state always clears after success or failure.
- The user can retry without refreshing.
- Unexpected errors use a generic message and do not expose raw stack traces or transport details.

### 5.6 Protected Admin Route Without a Known Session

Given a user navigates directly to `/admin` without a known authenticated admin session, when the guard evaluates, then the UI redirects them to `/admin/login` and preserves the attempted URL as the post-login destination.

Acceptance conditions:

- Protected route content is not rendered before the redirect.
- The redirect works for direct navigation and in-app navigation.
- The guard has automated tests for allowed and denied states.

### 5.7 Existing Session During Browser Runtime

Given an admin has signed in successfully during the current browser runtime, when they navigate between protected admin routes, then the UI allows navigation without another login prompt.

Acceptance conditions:

- The session state is held in an auth service using Angular signals.
- The state is not stored in local storage or session storage.
- A browser refresh may require re-authentication until a future authenticated-session probe endpoint exists.
- This refresh limitation is documented as an explicit follow-up, not hidden behaviour.

### 5.8 Sign Out

Given a signed-in admin is viewing a protected admin route, when they activate sign out, then the UI posts to `/api/admin/auth/sign-out` with `credentials: 'include'`, clears the client auth state, and returns them to `/admin/login`.

Acceptance conditions:

- Sign-out is available from authenticated admin UI.
- Sign-out remains idempotent from the user's perspective if the server session has already expired.
- A failed sign-out request leaves a visible recoverable error and does not pretend the server cookie has been cleared.

### 5.9 Expired or Missing Server Cookie on a Future Admin API Call

Given the UI believes the admin is signed in but a protected API call returns `401 Unauthorized`, when a future admin workflow handles that response, then the shared auth pattern should clear client auth state and route the user back to login.

Acceptance conditions:

- The auth service exposes a reusable method or event path for future API clients to mark the session unauthenticated.
- Milestone 10 can consume the pattern without reimplementing session-expiry handling.

## 6. Deliverables

### 6.1 Admin Auth API Client

- Add a focused auth service under `UI/src/app/` using the placement convention chosen for reusable API integration, for example `UI/src/app/core/auth/` or `UI/src/app/features/admin/auth/` if the project prefers admin-owned auth to stay feature-local at first.
- Implement `signIn(email, password)`, `signOut()`, `$isAuthenticated`, `$isSigningIn`, and a way to clear auth state after a later `401`.
- Use `HttpClient` or a small fetch wrapper consistently with Angular patterns; all auth calls must include cookies.
- Map API statuses into typed UI outcomes instead of spreading raw status checks through components.

### 6.2 API Base URL Convention

- Introduce the minimum reusable convention for building API URLs from the Angular app.
- Prefer same-origin `/api/...` requests if the deployed UI and API are intended to share an origin.
- If cross-origin local development is needed, document the proxy or environment decision in `UI/AI_PROJECT_GUIDE.md` and `UI/README.md` in the same change.
- Do not add a new third-party API client dependency for this milestone.

### 6.3 Admin Login Feature

- Add `UI/src/app/features/admin/login/` with standalone component, template, styles, and tests.
- Use reactive forms or Angular Signal Forms if available and already practical in the project.
- Include required validation for email and password fields.
- Provide accessible form-level and field-level error handling.
- Redirect authenticated users away from the login page when appropriate.

### 6.4 Protected Admin Landing Feature

- Add a minimal protected `/admin` landing route under `UI/src/app/features/admin/`.
- Render a simple authenticated admin page that confirms access and reserves space for follow-on admin workflows.
- Include a sign-out action.
- Avoid building dashboard cards or import controls before Milestone 10.

### 6.5 Route Guard and Routing

- Extend `UI/src/app/app.routes.ts` with lazy-loaded admin routes.
- Add route accessibility metadata for `/admin/login` and `/admin`.
- Implement an admin auth guard that allows known authenticated sessions and redirects unauthenticated users to `/admin/login` with a return URL.
- Ensure wildcard and public routes continue to behave as they do today.

### 6.6 Shell and Navigation Integration

- Add only the navigation affordances needed for users to discover admin login or leave authenticated admin space.
- Keep admin-specific feature logic out of `RootShell`.
- If public header navigation is changed, keep copy parent-friendly and mark visible strings for i18n extraction.

### 6.7 Documentation Updates

- Update `UI/AI_PROJECT_GUIDE.md` with the selected API integration and auth state ownership convention.
- Update `UI/README.md` if a local API proxy, same-origin assumption, or end-to-end startup note becomes necessary.
- Capture the browser-refresh limitation if no authenticated-session probe endpoint is added.

### 6.8 Tests and Validation

- Add or update unit tests for:
  - successful login outcome and redirect;
  - `401` invalid credentials;
  - `403` non-admin credentials;
  - unexpected or network failure;
  - sign-out success and failure;
  - guard redirect and allow paths.
- Add accessibility smoke coverage for the login and protected admin landing pages.
- Refresh localization artifacts for all new visible UI strings.

## 7. Technology Requirements and Decisions

- Use the existing Angular 22 standalone component style.
- Use Angular signals for client auth state.
- Prefer lazy-loaded admin feature routes because admin screens are secondary to the public homepage.
- Use cookie-based authentication exactly as the API provides it; frontend code must not store tokens.
- Send authenticated API requests with cookies included.
- Do not introduce NgRx, external auth SDKs, or a generated OpenAPI client for this milestone.
- Use existing design tokens, primitives, and design-system components where they fit.
- Maintain SSR compatibility by avoiding direct unguarded access to browser-only globals in services, guards, and components.
- Treat the first API integration pattern as a repo convention and document it when implemented.

## 8. Dependencies and Sequencing

1. [x] Confirm current API auth endpoints still match `API/API_REFERENCE.md` and `API/SummerBornInfo.Web/API/Admin/Auth/AdminAuthEndpoints.cs`.
2. [x] Decide and document the UI API URL convention before coding auth calls.
3. [x] Build the auth service and typed result mapping.
4. [x] Add route guard tests before or alongside guard implementation.
5. [x] Add `/admin/login` route and component.
6. [x] Add protected `/admin` landing route and sign-out action.
7. [x] Wire route accessibility metadata, header or admin navigation affordances, and post-login return URL handling.
8. [x] Add component, guard, service, and accessibility tests.
9. Run formatting, linting, build, i18n validation, unit tests, and accessibility smoke tests from `UI/`.

## 9. Implementation Progress

- Step 1 completed by sub-agent `01a087e5-a409-7081-99b5-a17efa1af3f6`.
  - Confirmed `API/API_REFERENCE.md`, `API/SummerBornInfo.Web/API/Admin/Auth/AdminAuthEndpoints.cs`, `API/SummerBornInfo.Web/Program.cs`, and focused authentication/OpenAPI tests match the admin auth contract in this plan.
  - Validation: `dotnet test .\SummerBornInfo.Web.Tests\SummerBornInfo.Web.Tests.csproj -- --filter-class "*Authentication*" --filter-class "*OpenApiSecurityDocumentTests"` passed with 14 tests.
  - Residual risk: the API was not manually exercised over HTTP outside the integration test harness.
- Step 2 completed by sub-agent `01a087e9-bdfb-7622-b67d-49ba03adf924`.
  - Decision: Milestone 9 UI auth calls will use same-origin root-relative `/api/...` URLs, with cookies included on authenticated requests.
  - Documented the convention in `UI/AI_PROJECT_GUIDE.md` and the local `ng serve` limitation in `UI/README.md`.
  - Validation: `npm run format:check` passed from `UI/`.
  - No helper or API client code was added because no auth call sites exist yet and the documented convention is sufficient for the next step.
- Step 3 completed by sub-agent `01a087eb-cc2b-7383-a5ca-c1e8e84ba74a`.
  - Added `AdminAuthService` under `UI/src/app/features/admin/auth/` with root-provided Angular `HttpClient` integration, same-origin `/api/admin/auth/*` calls, cookie credentials, signal-backed in-memory auth state, typed sign-in/sign-out outcomes, and `markUnauthenticated()` for future protected API `401` handling.
  - Added focused service tests covering successful sign-in, `401` invalid credentials, `403` non-admin access, network and unexpected failures, sign-out success and failure, pending/auth signals, and explicit auth-state clearing.
  - Added `provideHttpClient()` to the application config.
  - Validation: `npm run format`, `npm run lint`, `npm run test:run -- --include=src/app/features/admin/auth/admin-auth.service.spec.ts`, and `npm run test:run -- --include=src/app/app.config.spec.ts` passed from `UI/`.
  - Residual risk: the service has not been exercised against a live co-hosted API; route guard, login component, and sign-out UI remain later plan steps.
- Step 4 completed by sub-agent `01a087f1-f28a-7b43-a1c5-4898504553a6`.
  - Added reusable `adminAuthGuard` under `UI/src/app/features/admin/auth/` beside the admin auth service.
  - Added focused router-level guard tests covering authenticated activation, unauthenticated redirect to `/admin/login`, preserved `returnUrl` values for protected URLs with query and fragment, and prevention of protected component construction before redirect.
  - Validation: `npm run format`, `npm run lint`, and `npm run test:run -- --include=src/app/features/admin/auth/admin-auth.guard.spec.ts` passed from `UI/`.
  - Residual risk: the guard is validated against spec-only routes until later steps add the real `/admin` and `/admin/login` route tree.
- Step 5 completed by sub-agent `01a087f7-2c67-7d73-96c7-127ef072d0a7`, with a top-level follow-up fix for login-return URL sanitisation.
  - Added lazy `/admin/login` routing under `RootShell` with route accessibility metadata for title, focus target, and skip-link target.
  - Added `UI/src/app/features/admin/login/` with a standalone admin login component, semantic email/password form, required-field blocking, autocomplete hints, pending duplicate-submit prevention, accessible form-level errors, safe `returnUrl` handling, already-authenticated runtime redirect, and current-runtime-only auth behaviour.
  - Added focused component, route metadata, and login accessibility smoke tests covering successful redirects, invalid credentials, non-admin access, network/unexpected failures, pending state, required-field blocking, password clearing, return URL sanitisation, already-authenticated redirect, and route metadata.
  - Refreshed `UI/src/locale/messages.xlf` for the new login copy and localized error messages.
  - Kept the login form on native inputs plus signals and raised the `allScript` production budget from `325kB`/`350kB` to `350kB`/`365kB`, preserving a warning at the new aggregate script size while allowing the added lazy login route to build.
  - Validation: `npm run test:run -- --include=src/app/features/admin/login/admin-login.spec.ts --include=src/app/app.routes.spec.ts`, `npm run test:run -- --include=src/app/features/admin/login/component.a11y-spec.ts`, `npm run test:run`, `npm run format`, `npm run lint`, `npm run extract:i18n`, `npm run build`, `npm run build:localize`, `npm run check:localized-ssr-assets`, and `npm run test:a11y` passed from `UI/`.
  - `npm run validate:i18n` was attempted, but it stops at the expected `git diff --exit-code -- src/locale/messages.xlf` gate because this step intentionally adds uncommitted extracted messages. The underlying extraction, localized build, and localized SSR asset checks were run separately and passed.
  - Residual risk: the login redirects to `/admin`, but the protected landing route remains a later step, so direct browser success-path validation depends on Step 6 adding that route.
- Step 6 completed by sub-agent `01a08808-c538-72f2-8e74-aa4a262505ed`.
  - Added lazy protected `/admin` routing under `RootShell` with `adminAuthGuard` and route accessibility metadata for title, focus target, and skip-link target.
  - Added `UI/src/app/features/admin/admin-home/` with a standalone minimal protected admin landing page, authenticated-access confirmation copy, reserved follow-on workflow space, visible sign-out action, pending duplicate-submit prevention, and accessible recoverable sign-out failure messages.
  - Sign-out success uses `AdminAuthService.signOut()` and redirects to `/admin/login`; failed sign-out leaves the admin on `/admin` and relies on the service behaviour that only clears client auth state after a `signed-out` outcome.
  - Added focused component, route metadata, real route-tree protection, sign-out success/failure, pending duplicate-prevention, minimal-content, and accessibility smoke tests for the admin landing page.
  - Refreshed `UI/src/locale/messages.xlf` for the new admin landing and sign-out copy.
  - Validation: `npm run format`, `npm run lint`, `npm run test:run -- --include=src/app/features/admin/admin-home/admin-home.spec.ts --include=src/app/app.routes.spec.ts`, `npm run test:run -- --include=src/app/features/admin/admin-home/component.a11y-spec.ts`, `npm run extract:i18n`, and `npm run test:a11y` passed from `UI/`.
  - Residual risk: live browser/API sign-out has not been exercised against a co-hosted backend in this step.
- Step 7 completed by sub-agent `01a08810-d34d-79e0-a9ac-50ce1746bc6f` after inspection confirmed steps 5 and 6 already satisfied the route, navigation, and return URL requirements.
  - Confirmed `/admin/login` and `/admin` declare route accessibility metadata in `UI/src/app/app.routes.ts` and are covered by `UI/src/app/app.routes.spec.ts`.
  - Confirmed `adminAuthGuard` preserves guarded destinations by redirecting unknown sessions to `/admin/login?returnUrl=...`, with coverage for protected URLs that include query strings and fragments.
  - Confirmed `AdminLogin` redirects successful sign-in to the preserved safe app-relative `returnUrl` or `/admin`, and rejects unsafe, external, relative, and login-loop return URLs.
  - Confirmed `AdminHome` provides the authenticated admin navigation affordance for leaving admin space through its visible sign-out action, without adding broad public navigation or admin-specific logic to `RootShell`.
  - No UI code or copy changes were needed.
  - Validation: focused route, login, admin-home, and guard tests were rerun from `UI/`; `git diff --check` passed.
  - Residual risk: live browser/API return URL and sign-out behavior has not been exercised against a co-hosted backend in this step.
- Step 8 completed by sub-agent `01a08812-bd27-7300-89bf-874a35c1ec9e` after auditing the existing admin auth, guard, login, admin landing, route metadata, and accessibility specs against the plan scenarios.
  - Confirmed `AdminAuthService` tests cover typed sign-in/sign-out outcomes, cookie-backed API requests, pending/auth signals, sign-out failure state retention, and the reusable `markUnauthenticated()` path for future protected API `401` handling.
  - Confirmed `adminAuthGuard` and real route-tree tests cover allow/deny behaviour, protected-content suppression before redirect, and preserved protected return URLs with query strings and fragments.
  - Confirmed `AdminLogin` tests cover required-field blocking, successful redirect to `/admin` and safe return URLs, unsafe return URL fallback, already-authenticated redirect, invalid credentials, non-admin access, network/unexpected failures, pending duplicate prevention, and password clearing after success or invalid credentials.
  - Confirmed `AdminHome` tests cover minimal protected landing content, sign-out success redirect, sign-out network/unexpected failure messaging, retained authenticated state on failed sign-out, focused error feedback, and pending duplicate prevention.
  - Confirmed `app.routes.spec.ts` covers `/admin/login` and `/admin` lazy loading plus route accessibility metadata, and the dedicated accessibility smoke specs cover login and admin landing pages.
  - Added one focused success-path login assertion for password clearing; no production code changes were needed.
  - Validation: focused admin auth, guard, login, admin-home, route metadata, and accessibility specs were rerun from `UI/`; `npm run lint` and `git diff --check` passed.
  - Residual risk: live browser/API auth and sign-out behaviour remains unexercised against a co-hosted backend in this step.

## 10. Risks and Mitigations

- API/UI local-origin mismatch:
  - Risk: same-origin `/api/...` calls fail during local Angular development if the API runs on a different origin.
  - Mitigation: use same-origin `/api/...` URLs for Milestone 9 and validate live browser/API calls only when the UI is co-hosted with the API or served behind a local proxy that provides `/api` on the Angular origin.
- No session probe endpoint:
  - Risk: after a browser refresh, the cookie may still be valid but the UI cannot know that before rendering protected routes.
  - Mitigation: treat in-memory auth as the Milestone 9 baseline and document a future `/api/admin/auth/session` or equivalent if persistent refresh recognition becomes required.
- CSRF and cookie policy assumptions:
  - Risk: cookie auth has deployment-sensitive security settings.
  - Mitigation: rely on the existing API auth implementation for cookie issuance and avoid adding frontend token storage; confirm any cross-origin configuration before enabling cross-origin credentials.
- Confusing volunteer admin feedback:
  - Risk: generic failures may leave a small admin team unsure what to do.
  - Mitigation: use clear, non-sensitive messages for invalid credentials, forbidden access, and retryable service failures.
- Overbuilding admin dashboard:
  - Risk: Milestone 9 expands into Milestone 10 or later admin workflows.
  - Mitigation: keep `/admin` to route protection, session controls, and a minimal landing surface.

## 11. Unknowns and Required Clarifications

- API URL convention:
  - Resolved for Milestone 9: the UI will call same-origin root-relative `/api/...` endpoints. No environment-configured API origin or proxy file is introduced in this step.
- Session refresh behaviour:
  - Assumption for this plan: client auth state is in-memory only, so a browser refresh returns the user to login unless a later API session probe is added.
- Login discoverability:
  - Assumption for this plan: `/admin/login` can exist without prominent public homepage promotion; a small footer or header link is optional if maintainers want discoverability.
- Post-login destination:
  - Assumption for this plan: a guarded route redirect stores a return URL and successful login returns there; direct login navigations default to `/admin`.

The remaining unknowns do not block implementation, but service and guard work should keep the refresh limitation visible until a session probe endpoint exists.

## 12. Completion Checklist

- [x] `/admin/login` route exists, lazy-loads, and has route accessibility metadata.
- [x] `/admin` route exists, is protected, lazy-loads, and has route accessibility metadata.
- [x] Auth service can sign in, sign out, expose signal-backed auth state, and clear state after later `401` handling.
- [x] Sign-in posts `{ email, password }` to `/api/admin/auth/sign-in` with cookies included.
- [x] Sign-out posts to `/api/admin/auth/sign-out` with cookies included.
- [x] `204`, `401`, `403`, network failure, and unexpected failure outcomes produce distinct typed client results.
- [x] Invalid-credential and non-admin messages are accessible, clear, and do not expose sensitive details.
- [x] Password values are never persisted to browser storage.
- [x] Guard redirects unknown sessions to `/admin/login` and preserves the requested protected destination.
- [x] Successful sign-in redirects to the requested destination or `/admin`.
- [x] Sign-out clears client state and returns to `/admin/login`.
- [x] Browser refresh/session-probe limitation is documented if no API session endpoint is added.
- [x] UI API integration convention is documented in `UI/AI_PROJECT_GUIDE.md`; `UI/README.md` is updated if local setup changes.
- [x] New visible template strings are marked for Angular i18n.
- [x] `UI/src/locale/messages.xlf` is refreshed.
- [x] Focused service, guard, component, and accessibility tests cover the in-scope behaviours.
- [x] `npm run format` has been run from `UI/`.
- [x] `npm run lint` has been run from `UI/`.
- [x] `npm run build` has been run from `UI/`.
- [ ] `npm run validate:i18n` has been run from `UI/`.
- [x] `npm run test:run` has been run from `UI/`.
- [x] `npm run test:a11y` has been run from `UI/`.
