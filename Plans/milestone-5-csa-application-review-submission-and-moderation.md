# Milestone 5 CSA Application Review Submission and Moderation Plan

## 1. Overview

This plan covers delivery of Roadmap Milestone 5: public CSA Application Review submission, public retrieval of moderated reviews, reporting and flagging, admin moderation, and the abuse controls needed for an initial public launch.

Current status:

- Delivery-ready with the key moderation and abuse-control decisions now confirmed.

Confirmed decisions:

- use Cloudflare Turnstile as the initial CAPTCHA provider;
- support local development and end-to-end testing without a live Turnstile dependency by allowing verification to be disabled or swapped for a local mock verifier in non-production environments;
- newly submitted reviews are visible by default;
- the first report against a visible review hides it immediately and places it in the admin queue as `PendingApproval`;
- once an admin approves a hidden reported review, it becomes visible again and remains visible until 10 further users report it;
- when that post-approval report threshold is reached, the review is hidden again and moves into the admin queue as `PendingReapproval`.

## 2. Roadmap Source or Existing Plan Context

- Roadmap source: [Roadmap/initial-api-roadmap.md](../Roadmap/initial-api-roadmap.md), Milestone 5 "CSA Application Review Submission and Moderation".
- Contract baseline source: [Documentation/milestone-1-contract-baseline-and-delivery-decisions.md](../Documentation/milestone-1-contract-baseline-and-delivery-decisions.md), sections 5.5 through 5.8.
- Existing implementation state as of 2026-06-19:
  - Milestones 2 through 4 are already reflected in the codebase and API reference.
  - `POST /api/admin/csa-application-reviews/{reviewId}/moderation` exists only as a protected contract shell and is not backed by persisted review state.
  - No persisted CSA Application Review aggregate or reporting model exists yet.
  - The Milestone 1 baseline assumed new reviews would start in `pendingModeration`, but this plan now deliberately revises that contract direction so new reviews are visible immediately and reporting drives moderation queue entry.
  - The current baseline defines the moderation action endpoint but does not define an admin queue or listing route, so Milestone 5 includes that contract extension as part of the delivered workflow.

## 3. Scope

- Persist CSA Application Reviews against existing schools.
- Implement public review submission at `POST /api/schools/{schoolId}/csa-application-reviews`.
- Implement public retrieval of publicly visible reviews at `GET /api/schools/{schoolId}/csa-application-reviews`.
- Implement public report submission at `POST /api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports`.
- Replace the moderation placeholder behind `POST /api/admin/csa-application-reviews/{reviewId}/moderation` with a persistence-backed workflow.
- Add an admin moderation queue endpoint so admins can discover pending reviews and reported reviews without needing review ids out of band.
- Add abuse controls for anonymous public submission and reporting:
  - rate limiting;
  - Cloudflare Turnstile verification with a configurable local disable or mock path for development and automated end-to-end testing.
- Update OpenAPI, `API_REFERENCE.md`, and milestone-contract documentation to reflect the Milestone 5 contract revisions for review visibility, reporting thresholds, and the admin queue route.

## 4. Non-Goals

- Public user accounts or sign-in.
- Review editing or deletion by the original submitter.
- Reply threads or comment discussions.
- Automatic toxicity classification or AI moderation.
- A full moderation audit trail beyond current review status and report resolution state.
- Email notifications or background messaging for moderators.
- Cross-school moderation dashboards or analytics beyond the queue needed to process pending reviews.

## 5. Behaviour Scenarios

### Public review submission

Given an existing school, a valid review payload, and a valid bot-protection proof, when a public caller submits a review, then the API creates a persisted review that is immediately publicly visible, returns `201 Created`, and includes the new review id, school id, moderation status, and submission timestamp.

Given an unknown `schoolId`, when a public caller submits a review, then the API returns `404 Not Found` and does not persist any review.

Given a blank `name`, missing `applicationSuccessful`, blank `comment`, or content that exceeds defined length limits, when a public caller submits a review, then the API returns `400 Bad Request` with field-level validation details.

Given a non-production environment with CAPTCHA verification disabled or mocked, when a valid review is submitted through local or automated test flows, then the API accepts the request without requiring live Turnstile credentials.

Given a production caller that fails bot verification or any caller that exceeds rate limits, when a public caller submits a review, then the API rejects the request without persisting a review and returns the configured abuse-control response (`400` or `403` for invalid bot proof, `429` for throttling).

### Public review retrieval

Given a school that has publicly visible reviews, when a public caller requests `GET /api/schools/{schoolId}/csa-application-reviews`, then the API returns only publicly visible reviews in a stable newest-first order with cursor pagination.

Given a school that has only pending or rejected reviews, when a public caller requests the public review list, then the API returns an empty page rather than exposing moderation-internal state.

Given an unknown `schoolId`, when the public review list is requested, then the API returns `404 Not Found`.

### Public reporting and flagging

Given a publicly visible review that has never been admin-approved, when the first valid report is submitted, then the API stores the report, hides the review from public retrieval, transitions it to `PendingApproval`, and places it in the admin moderation queue.

Given a publicly visible review that has already been approved by an admin, when fewer than 10 further valid reports from distinct users have been submitted since that approval, then the API stores each report and keeps the review publicly visible.

Given a publicly visible review that has already been approved by an admin, when the 10th further valid report from a distinct user is submitted since that approval, then the API stores the report, hides the review from public retrieval, transitions it to `PendingReapproval`, and places it in the admin moderation queue.

Given `reason = other` with blank `details`, an unsupported report reason, an unknown `reviewId`, or a review that does not belong to the supplied school, when a report is submitted, then the API returns `400 Bad Request` or `404 Not Found` and does not persist a report.

Given a review that is not publicly visible, when a caller attempts to report it, then the API returns `404 Not Found` so hidden moderation state is not leaked.

### Admin moderation workflow

Given an authenticated admin caller, when the moderation queue is requested, then the API returns pending reviews and reported reviews with enough detail to triage them, including school summary, review content, current moderation status, report count, latest report timestamp, and paging metadata.

Given an authenticated admin caller, when a `PendingApproval` review is approved, then the review becomes publicly visible, its pre-approval reports are marked resolved, and it is removed from the pending queue.

Given an authenticated admin caller, when a `PendingApproval` review is rejected, then the review remains hidden, its open reports are marked resolved, and it is removed from the pending queue.

Given an authenticated admin caller, when a `PendingReapproval` review is approved, then the review becomes publicly visible again, the post-approval report counter is reset, its open reports are marked resolved, and it is removed from the pending queue.

Given an authenticated admin caller, when a `PendingReapproval` review is rejected, then the review remains hidden, its open reports are marked resolved, and it is removed from the pending queue.

Given an unauthenticated caller or an authenticated non-admin caller, when an admin queue or moderation endpoint is called, then the API returns `401 Unauthorized` or `403 Forbidden` respectively.

## 6. Deliverables

### Deliverable A: Review domain and persistence model

- Add a `CsaApplicationReview` aggregate in `SummerBornInfo.Domain` with domain methods for:
  - initial submission;
  - first-report hiding;
  - approval;
  - rejection;
  - report attachment;
  - post-approval report counting;
  - report-threshold hiding;
  - report resolution after moderation.
- Add a persisted report model for anonymous flagging, likely `CsaApplicationReviewReport`, that captures:
  - review id;
  - report reason;
  - optional details;
  - submission timestamp;
  - resolution timestamp;
  - a best-effort anonymous reporter fingerprint used to prevent the same caller from counting multiple times toward the 10-user reapproval threshold.
- Model review moderation state explicitly with at least `Visible`, `PendingApproval`, `Approved`, `PendingReapproval`, and `Rejected`, or an equivalent shape that preserves the same behavior semantics.
- Add EF Core configurations and `DbSet`s in `SummerBornInfo.Infrastructure`.
- Enforce foreign-key linkage to `School` and cascade rules that match the existing import-first data model.

### Deliverable B: Public submission slice

- Add a vertical feature slice for review submission in `SummerBornInfo.Features`.
- Define validation rules for:
  - required `name`, `applicationSuccessful`, and `comment`;
  - maximum lengths for `name`, `comment`, and report `details`;
  - school existence;
  - bot-protection token presence when Turnstile verification is enabled.
- Add the public endpoint in `SummerBornInfo.Web` using the existing minimal-API route registration pattern.
- Return the revised Milestone 5 response shape with persisted ids, timestamps, and the visible-by-default review status rather than generated placeholder values.

### Deliverable C: Public review retrieval slice

- Add a query handler that returns only publicly visible reviews for a school.
- Use stable newest-first pagination with an opaque cursor based on `submittedAtUtc` and `id`.
- Keep pending, rejected, and moderation-only metadata out of the public response shape.

### Deliverable D: Public reporting slice

- Add a feature slice for report submission against a publicly visible review.
- Enforce the first-report hide rule for visible but not yet admin-approved reviews.
- Enforce the 10-distinct-report threshold for reviews that were previously approved by an admin.
- Enforce school and review ownership checks so callers cannot report a review through the wrong school route.
- Return the baseline acceptance contract while persisting the report so it becomes actionable for admins.

### Deliverable E: Admin moderation workflow

- Replace the current placeholder moderation handler with a persistence-backed implementation.
- Keep `POST /api/admin/csa-application-reviews/{reviewId}/moderation` as the write endpoint for approve or reject decisions.
- Add a new admin queue endpoint:
  - route: `GET /api/admin/csa-application-reviews`;
  - default behavior: return items in `PendingApproval` or `PendingReapproval`;
  - recommended filters: queue state, cursor, and page size.
- Return enough review and report summary data from the queue endpoint that a basic moderator UI can be built without another discovery endpoint.
- Mark open reports resolved when an admin finishes moderation on a queued review.
- Reset the post-approval report counter only when a `PendingReapproval` review is approved again.

### Deliverable F: Abuse controls

- Add ASP.NET Core rate-limiting policies for anonymous review submission and report submission.
- Partition throttling by route and caller signal, with IP-based partitioning as the minimum baseline.
- Add provider-agnostic bot-verification plumbing so the feature code depends on an interface and the provider-specific verification lives in infrastructure.
- Implement Cloudflare Turnstile verification behind that interface.
- Support non-production disablement or replacement with a local mock verifier so local development and end-to-end test environments do not require live provider credentials.
- Wire provider configuration through app settings or secrets without hard-coding provider credentials.

### Deliverable G: Tests and documentation

- Add domain tests for moderation-state transitions and report resolution behavior.
- Add integration tests for public submit, public list, public report, admin queue, and admin moderation.
- Add threshold tests covering:
  - first report hides a newly visible review and queues it for `PendingApproval`;
  - admin approval restores visibility;
  - 10 further distinct reports after approval hide the review again and queue it for `PendingReapproval`.
- Add failure-path tests for invalid payloads, unknown school or review ids, school and review mismatch, duplicate-report counting, throttling, auth behavior, and Turnstile-disabled local execution.
- Update `API_REFERENCE.md` to replace the moderation-placeholder note with actual workflow behavior.
- Update the milestone contract artifact or add an explicit Milestone 5 contract addendum for the admin queue route and the revised review-visibility behavior.

## 7. Technology Requirements and Decisions

- Follow the existing vertical-slice structure described in [API/AI_PROJECT_GUIDE.md](../API/AI_PROJECT_GUIDE.md):
  - business rules in `Domain`;
  - use-case orchestration in `Features`;
  - EF Core persistence in `Infrastructure`;
  - HTTP wiring in `Web`.
- Persist review and report data in PostgreSQL through EF Core; do not introduce a separate document store or queueing system for Milestone 5.
- Use domain methods for moderation state changes instead of setting entity status directly from handlers.
- Keep the existing admin auth approach from Milestone 2 and reuse the current admin authorization policy.
- Use ASP.NET Core rate limiting for throttle enforcement so this part of the abuse-control story does not require a new package.
- Use Cloudflare Turnstile for production-facing verification, because it provides a standard server-side verification flow and can be isolated behind a provider interface.
- Add a non-production configuration mode that disables Turnstile enforcement or swaps in a fake verifier so local development and end-to-end test environments remain practical even if live provider setup is unavailable.
- Required contract extension: add `GET /api/admin/csa-application-reviews`. The current baseline moderation write route alone is not sufficient to deliver a workable moderation workflow.
- Required contract revision: Milestone 5 no longer follows the earlier assumption that submissions start hidden in `pendingModeration`; visibility and moderation are now driven by reporting thresholds.
- Treat "10 further users report the comment" as 10 distinct anonymous reporters according to a best-effort deduplication key defined by the implementation, because public user accounts are out of scope.

## 8. Dependencies and Sequencing

1. [x] Update the Milestone 5 contract notes to reflect the confirmed Turnstile choice, visible-by-default submission behavior, first-report hide rule, 10-report reapproval threshold, and admin queue route.
2. [x] Add the review and report domain model and infrastructure mapping.
3. [x] Implement public review submission and public review retrieval.
4. [x] Implement public report submission, including first-report hide logic, distinct-reporter counting, and the reapproval threshold transition.
5. [x] Replace the moderation placeholder with persistence-backed moderation and add the admin queue endpoint.
6. Add rate limiting and Turnstile-backed bot verification to the anonymous public routes, together with the non-production disable or mock path.
7. Update OpenAPI-facing endpoint metadata, API reference documentation, and contract notes.
8. Run the relevant automated tests and close any query-ordering, duplicate-report, or moderation-state issues found during validation.

## 9. Risks and Mitigations

- Moderation workflow risk:
  The current contract baseline does not include a way for admins to discover pending or reported reviews.
  Mitigation: explicitly add and document an admin queue endpoint before implementation starts.

- Abuse-control gap risk:
  Anonymous public submission can attract spam immediately.
  Mitigation: treat rate limiting and bot verification as first-class deliverables rather than post-launch hardening.

- State-model complexity risk:
  Mixing immediate publication, first-report hiding, admin reapproval, and threshold-based rehiding can become hard to reason about if modeled ad hoc.
  Mitigation: keep review moderation state, report resolution state, and post-approval report counting explicit in the domain model, with domain methods owning transitions.

- Pagination regression risk:
  Public review lists and admin moderation queues can become unstable under concurrent inserts if ordering is underspecified.
  Mitigation: use deterministic sort keys and cursor tests that exercise multi-page traversal.

- Content-handling risk:
  Free-text names, comments, and report details can introduce oversize payloads or unsafe rendering assumptions.
  Mitigation: enforce explicit length limits, treat values as plain text, and document that UI layers remain responsible for output encoding.

- Provider integration risk:
  Bot-verification providers introduce environment configuration and failure modes outside the local database stack.
  Mitigation: isolate verification behind an interface, add configuration validation, and provide a non-production disable or mock path for local and automated test environments.

- Distinct-reporter counting risk:
  Public reporting is anonymous, so "10 further users" cannot rely on authenticated user ids.
  Mitigation: define a best-effort reporter fingerprint strategy, reject duplicate open reports from the same fingerprint for the same review, and cover the threshold behavior with integration tests.

## 10. Unknowns and Required Clarifications

- The main product decisions are now confirmed.
- The remaining implementation choice is how to define the anonymous reporter fingerprint used for "distinct user" counting.
  Recommended answer: treat one open report per review per client fingerprint as countable, where the fingerprint is derived from the same anti-abuse signals used by the reporting throttle and bot-verification flow.

This remaining item should be documented during implementation, but it does not block delivery planning.

## 11. Completion Checklist

- The Milestone 5 moderation and abuse-control decisions are recorded in this plan and approved.
- A persisted review aggregate and report model exist with infrastructure configuration.
- `POST /api/schools/{schoolId}/csa-application-reviews` is implemented against persisted data and returns the documented visible-by-default response.
- `GET /api/schools/{schoolId}/csa-application-reviews` returns only publicly visible reviews with stable cursor pagination.
- `POST /api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports` persists reports, enforces route ownership validation, and applies the first-report and reapproval-threshold hiding rules.
- `GET /api/admin/csa-application-reviews` exists as the moderation queue contract extension and is documented.
- `POST /api/admin/csa-application-reviews/{reviewId}/moderation` performs real state transitions against persisted reviews.
- Public submit and report routes enforce rate limits and Turnstile verification, with a documented non-production disable or mock path.
- Integration tests cover success paths, auth failures, validation failures, mismatch cases, throttling, duplicate-report prevention, threshold transitions, and pagination behavior.
- API reference and contract documentation reflect the final Milestone 5 surface instead of the current moderation placeholder note.
