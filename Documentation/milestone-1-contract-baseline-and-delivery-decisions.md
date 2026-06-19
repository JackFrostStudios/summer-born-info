# Milestone 1 Contract Baseline and Delivery Decisions

## 1. Purpose and Status

This document is the Milestone 1 markdown contract baseline for the initial Summer Born Information API surface.

It is the pre-implementation contract artifact that downstream milestones must implement and validate. The final contract handoff for the UI team remains generated OpenAPI output from the ASP.NET Core API project once Milestones 2 through 6 are complete.

This baseline is intentionally concrete at the HTTP contract layer and intentionally light on implementation detail. It defines the initial endpoint inventory, request and response shapes, validation expectations, and protected-operation rules without locking in deferred architecture choices.

## 2. Confirmed Delivery Decisions

- The published API contract will ultimately be generated from the ASP.NET Core API project rather than maintained as a separate hand-authored contract artifact.
- Milestone 1 delivers this markdown baseline so later milestones can implement toward a stable surface before generated OpenAPI exists.
- Admin-protected operations must be implemented using ASP.NET Core Identity as the agreed authentication direction.
- The public schools collection contract is rooted at `GET /api/schools`, uses the current paged collection response shape, and remains the baseline response model for downstream school discovery work.
- School discovery is exposed separately at `GET /api/schools/search` so its ranked-search, exact-URN lookup, and cursor behaviour can differ cleanly from the plain collection route.
- Radius-based school discovery remains a separate capability from the main schools collection contract.
- School discovery text search will use a PostgreSQL hybrid approach that combines full-text search with `pg_trgm` similarity support.
- The initial public CSA Application Review contract must include `name`, `applicationSuccessful`, and a free-text `comment`.
- `Id` is the canonical resource identifier carried by API response objects, while school-specific POST operations continue to use `schoolId` route parameters.

## 3. In-Scope Contract Surface

This baseline covers:

- admin-protected operations for school imports and CSA Application Review moderation;
- public school collection and discovery through the main schools GET route;
- public school retrieval by exact URN through the search route;
- public school discovery by radius-from-point query;
- public CSA Application Review submission for a specific school;
- public retrieval of comments for a specific school;
- public reporting of a specific comment for a specific school;
- shared validation and error response expectations used by those operations.

This baseline does not define full auth flows, persistence design, ranking internals, abuse-protection internals, or operational rollout details.

## 4. Shared Contract Conventions

### 4.1 Route and Naming Conventions

- Public school routes are rooted under `/api/schools`.
- Admin-only routes are rooted under `/api/admin`.
- `Id` is the canonical school resource identifier returned by school response objects, while school-specific POST routes continue to use `schoolId`.
- School discovery is a distinct public GET route at `/api/schools/search` rather than a mode of the main schools collection route.
- Exact URN lookup is handled through the public search route for callers that start from a URN rather than from free-text input.
- School association for review submission and reporting is carried by route parameter rather than an optional body field.
- Public comments are the moderated CSA Application Reviews exposed for school-specific display.

### 4.2 Shared School Model

`SchoolAddress`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `street` | string | No | First address line when present in imported data. |
| `locality` | string | No | Optional locality value. |
| `addressThree` | string | No | Optional third address line. |
| `town` | string | Yes | Town or city value. |
| `county` | string | No | Optional county value. |
| `postCode` | string | Yes | Postcode value used in school display and search. |

`LookupValue`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | Yes | Canonical identifier for the lookup row. |
| `code` | string | Yes | External or imported code for the lookup row. |
| `name` | string | Yes | Display name for the lookup row. |

`SchoolResponse`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | Yes | Canonical school resource identifier used in response payloads. |
| `urn` | integer | Yes | Exact school identifier used for exact-URN lookup and external reference. |
| `ukprn` | integer | No | Optional UK Provider Reference Number when present in source data. |
| `establishmentNumber` | integer | Yes | Establishment number from imported source data. |
| `name` | string | Yes | School display name. |
| `address` | `SchoolAddress` | Yes | Structured school address. |
| `openDate` | string (`date`) | No | School open date when known. |
| `closeDate` | string (`date`) | No | School close date when known. |
| `phaseOfEducation` | `LookupValue` | Yes | Phase of education lookup. |
| `localAuthority` | `LookupValue` | Yes | Local authority lookup. |
| `establishmentType` | `LookupValue` | Yes | Establishment type lookup. |
| `establishmentGroup` | `LookupValue` | Yes | Establishment group lookup. |
| `establishmentStatus` | `LookupValue` | Yes | Establishment status lookup. |

`GetSchoolsResponse`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `schools` | array of `SchoolResponse` | Yes | Current page of schools. |
| `nextCursor` | string | No | Continuation token for the next page. `null` when there is no next page. Its format is route- and mode-specific. |

### 4.3 Shared Error Model

`ErrorResponse`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `status` | integer | Yes | HTTP status code. |
| `title` | string | Yes | Short error summary. |
| `errors` | object | No | Field-level validation details keyed by input name when relevant. |

Baseline rules:

- `400 Bad Request` is used for malformed input and semantic validation failures.
- `401 Unauthorized` is used when a protected endpoint is called without a valid authenticated admin identity.
- `403 Forbidden` is used when a caller is authenticated but is not authorized for the admin operation.
- `404 Not Found` is used when a requested school or review does not exist.
- Valid school discovery requests that return no matches still return `200 OK` with an empty `schools` array.
- `429 Too Many Requests` is implemented for the anonymous public review submission and report routes once Milestone 5 abuse controls are enabled.

## 5. Endpoint Inventory

### 5.1 Public Schools Collection

`GET /api/schools`

Purpose:
Return a paged collection of schools using the current schools response contract.

Authentication:
Public.

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `cursor` | string | No | Continuation value from a previous response. Must not be blank when supplied. |
| `pageSize` | integer | No | Optional positive result limit. If supplied, must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "schools": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "urn": 100001,
      "ukprn": 200001,
      "establishmentNumber": 3001,
      "name": "Northbridge Primary",
      "address": {
        "street": "1 Market Street",
        "locality": "Old Town",
        "addressThree": "Suite A",
        "town": "Leeds",
        "county": "West Yorkshire",
        "postCode": "LS1 1AA"
      },
      "openDate": "2010-09-01",
      "closeDate": null,
      "phaseOfEducation": {
        "id": "11111111-1111-1111-1111-111111111111",
        "code": "PRIMARY-100001",
        "name": "Primary 100001"
      },
      "localAuthority": {
        "id": "22222222-2222-2222-2222-222222222222",
        "code": "LA-100001",
        "name": "Local Authority 100001"
      },
      "establishmentType": {
        "id": "33333333-3333-3333-3333-333333333333",
        "code": "COMM-100001",
        "name": "Community school 100001"
      },
      "establishmentGroup": {
        "id": "44444444-4444-4444-4444-444444444444",
        "code": "GROUP-100001",
        "name": "Local authority maintained schools 100001"
      },
      "establishmentStatus": {
        "id": "55555555-5555-5555-5555-555555555555",
        "code": "OPEN-100001",
        "name": "Open 100001"
      }
    }
  ],
  "nextCursor": "00000000-0000-0000-0000-000000000001"
}
```

Validation and failure expectations:

- Blank or invalid `cursor` values return `400 Bad Request`.
- Invalid `pageSize` values return `400 Bad Request`.

Baseline notes:

- The response shape for the schools collection is the current API contract and should remain the baseline shape for downstream discovery work.
- Milestone 3 preserves this route for plain collection traversal and introduces a separate free-text discovery route at `GET /api/schools/search`.
- The working page-size defaults for this route remain `100` by default and `200` maximum unless a later milestone deliberately revises them.

### 5.2 Public School Discovery Search

`GET /api/schools/search`

Purpose:
Return school discovery results using either free-text matching across school name and address or postcode fields, or exact lookup by URN.

Authentication:
Public.

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `q` | string | No | Free-text search term. Must not be blank when supplied. Mutually exclusive with `urn`. |
| `urn` | integer | No | Exact school URN. Mutually exclusive with `q`. |
| `cursor` | string | No | Opaque continuation value from a previous search response. Must not be blank when supplied. Clients must treat the value as server-generated and immutable. |
| `pageSize` | integer | No | Optional positive result limit. If supplied, must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "schools": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "urn": 100001,
      "ukprn": 200001,
      "establishmentNumber": 3001,
      "name": "Northbridge Primary",
      "address": {
        "street": "1 Market Street",
        "locality": "Old Town",
        "addressThree": "Suite A",
        "town": "Leeds",
        "county": "West Yorkshire",
        "postCode": "LS1 1AA"
      },
      "openDate": "2010-09-01",
      "closeDate": null,
      "phaseOfEducation": {
        "id": "11111111-1111-1111-1111-111111111111",
        "code": "PRIMARY-100001",
        "name": "Primary 100001"
      },
      "localAuthority": {
        "id": "22222222-2222-2222-2222-222222222222",
        "code": "LA-100001",
        "name": "Local Authority 100001"
      },
      "establishmentType": {
        "id": "33333333-3333-3333-3333-333333333333",
        "code": "COMM-100001",
        "name": "Community school 100001"
      },
      "establishmentGroup": {
        "id": "44444444-4444-4444-4444-444444444444",
        "code": "GROUP-100001",
        "name": "Local authority maintained schools 100001"
      },
      "establishmentStatus": {
        "id": "55555555-5555-5555-5555-555555555555",
        "code": "OPEN-100001",
        "name": "Open 100001"
      }
    }
  ],
  "nextCursor": "eyJ2IjoxLCJtb2RlIjoic2Nob29scy1zZWFyY2giLCJsYXN0SWQiOiIwMDAwMDAwMC0wMDAwLTAwMDAtMDAwMC0wMDAwMDAwMDAwMDEifQ"
}
```

Validation and failure expectations:

- Supplying neither `q` nor `urn` returns `400 Bad Request`.
- Supplying both `q` and `urn` returns `400 Bad Request`.
- Blank `q` returns `400 Bad Request`.
- Invalid URN format returns `400 Bad Request`.
- Blank or invalid `cursor` values return `400 Bad Request`.
- Invalid `pageSize` values return `400 Bad Request`.
- No matches return `200 OK` with `"schools": []` and `"nextCursor": null`.
- Unknown URN returns `404 Not Found`.

Baseline notes:

- This route is the dedicated school discovery surface.
- The chosen text-search approach for school discovery is PostgreSQL full-text search combined with `pg_trgm` similarity support so the implementation can balance relevance, partial matching, and typo tolerance without introducing a separate search service.
- Search cursors on this route are intentionally opaque and may encode the ranked resume boundary needed by the server for free-text traversal.

### 5.3 Public Exact URN Lookup

`GET /api/schools/search?urn={urn}`

Purpose:
Return one school by exact URN through the school search route as a distinct capability from free-text discovery.

Authentication:
Public.

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `urn` | integer | Yes | Exact school URN. |

Response:

`200 OK`

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "urn": 100001,
  "ukprn": 200001,
  "establishmentNumber": 3001,
  "name": "Northbridge Primary",
  "address": {
    "street": "1 Market Street",
    "locality": "Old Town",
    "addressThree": "Suite A",
    "town": "Leeds",
    "county": "West Yorkshire",
    "postCode": "LS1 1AA"
  },
  "openDate": "2010-09-01",
  "closeDate": null,
  "phaseOfEducation": {
    "id": "11111111-1111-1111-1111-111111111111",
    "code": "PRIMARY-100001",
    "name": "Primary 100001"
  },
  "localAuthority": {
    "id": "22222222-2222-2222-2222-222222222222",
    "code": "LA-100001",
    "name": "Local Authority 100001"
  },
  "establishmentType": {
    "id": "33333333-3333-3333-3333-333333333333",
    "code": "COMM-100001",
    "name": "Community school 100001"
  },
  "establishmentGroup": {
    "id": "44444444-4444-4444-4444-444444444444",
    "code": "GROUP-100001",
    "name": "Local authority maintained schools 100001"
  },
  "establishmentStatus": {
    "id": "55555555-5555-5555-5555-555555555555",
    "code": "OPEN-100001",
    "name": "Open 100001"
  }
}
```

Validation and failure expectations:

- Invalid URN format returns `400 Bad Request`.
- Unknown URN returns `404 Not Found`.

Baseline notes:

- URN is the lookup input for this specific operation, but the returned school resource still uses the same full `SchoolResponse` shape as the main schools GET contract.
- This capability shares the `/api/schools/search` route with free-text discovery but uses explicit query validation so callers must choose either free-text search or exact URN lookup.

### 5.4 Public Radius-Based School Search

`GET /api/schools/nearby`

Purpose:
Return schools near a supplied point for map and proximity-based experiences.

Authentication:
Public.

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `latitude` | number | Yes | Must be within `-90` to `90`. |
| `longitude` | number | Yes | Must be within `-180` to `180`. |
| `radiusMiles` | number | Yes | Positive search radius in miles. |
| `cursor` | string | No | Continuation value from a previous response. Must not be blank when supplied. |
| `pageSize` | integer | No | Optional positive result limit. If supplied, must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "schools": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "urn": 100001,
      "ukprn": 200001,
      "establishmentNumber": 3001,
      "name": "Northbridge Primary",
      "address": {
        "street": "1 Market Street",
        "locality": "Old Town",
        "addressThree": "Suite A",
        "town": "Leeds",
        "county": "West Yorkshire",
        "postCode": "LS1 1AA"
      },
      "openDate": "2010-09-01",
      "closeDate": null,
      "phaseOfEducation": {
        "id": "11111111-1111-1111-1111-111111111111",
        "code": "PRIMARY-100001",
        "name": "Primary 100001"
      },
      "localAuthority": {
        "id": "22222222-2222-2222-2222-222222222222",
        "code": "LA-100001",
        "name": "Local Authority 100001"
      },
      "establishmentType": {
        "id": "33333333-3333-3333-3333-333333333333",
        "code": "COMM-100001",
        "name": "Community school 100001"
      },
      "establishmentGroup": {
        "id": "44444444-4444-4444-4444-444444444444",
        "code": "GROUP-100001",
        "name": "Local authority maintained schools 100001"
      },
      "establishmentStatus": {
        "id": "55555555-5555-5555-5555-555555555555",
        "code": "OPEN-100001",
        "name": "Open 100001"
      }
    }
  ],
  "nextCursor": "00000000-0000-0000-0000-000000000001"
}
```

Validation and failure expectations:

- Missing `latitude`, `longitude`, or `radiusMiles` returns `400 Bad Request`.
- Out-of-range coordinates return `400 Bad Request`.
- Zero or negative `radiusMiles` returns `400 Bad Request`.
- Blank or invalid `cursor` values return `400 Bad Request`.
- Invalid `pageSize` values return `400 Bad Request`.
- No matches return `200 OK` with `"schools": []` and `"nextCursor": null`.

Baseline notes:

- Radius-from-point is the only required geospatial mode in the initial release.
- The fixed contract unit is miles to avoid a baseline-level unit negotiation decision.
- Pagination uses the same collection response shape as `GET /api/schools`.

### 5.5 Public CSA Application Review Submission

`POST /api/schools/{schoolId}/csa-application-reviews`

Purpose:
Create a CSA Application Review associated with a specific school.

Authentication:
Public.

Route parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `schoolId` | string | Yes | Canonical school identifier for the reviewed school. |

Request body:

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `name` | string | Yes | Submitter-provided name. Must not be blank. |
| `applicationSuccessful` | boolean | Yes | Indicates whether the CSA application was successful. |
| `comment` | string | Yes | Free-text review comment. Must not be blank. |
| `botVerificationToken` | string | No | Bot-verification token for abuse-control enforcement when enabled. |

Example request:

```json
{
  "name": "Parent A",
  "applicationSuccessful": true,
  "comment": "Our application was accepted after appeal and the school was responsive.",
  "botVerificationToken": "test-token"
}
```

Response:

`201 Created`

```json
{
  "id": "rev_123",
  "schoolId": "sch_123",
  "name": "Parent A",
  "applicationSuccessful": true,
  "comment": "Our application was accepted after appeal and the school was responsive.",
  "status": "visible",
  "submittedAtUtc": "2026-05-21T10:30:00Z"
}
```

Validation and failure expectations:

- Malformed request bodies return `400 Bad Request`.
- Blank `name` returns `400 Bad Request`.
- `name` longer than 200 characters returns `400 Bad Request`.
- Missing `applicationSuccessful` returns `400 Bad Request`.
- Blank `comment` returns `400 Bad Request`.
- `comment` longer than 4000 characters returns `400 Bad Request`.
- Failed bot verification returns `400 Bad Request`.
- Unknown `schoolId` returns `404 Not Found`.
- Anonymous callers that exceed the submission rate limit return `429 Too Many Requests`.

Baseline notes:

- The route carries the school association by `schoolId` so the body cannot drift from the target school.
- Public submission is synchronous at contract level with `201 Created`.
- Newly submitted reviews are publicly visible by default; they do not enter the moderation queue until reporting triggers a moderation transition.
- Cloudflare Turnstile is the initial CAPTCHA provider for public review submission. Non-production environments may disable verification or substitute a local mock verifier so local development and automated end-to-end tests do not require the live service.

### 5.6 Public School Comment Retrieval

`GET /api/schools/{schoolId}/csa-application-reviews`

Purpose:
Return publicly visible comments for one school.

Authentication:
Public.

Route parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `schoolId` | string | Yes | Canonical school identifier. |

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `cursor` | string | No | Continuation value from a previous response. Must not be blank when supplied. |
| `pageSize` | integer | No | Optional positive result limit. Defaults to `20` and must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "reviews": [
    {
      "id": "rev_123",
      "name": "Parent A",
      "applicationSuccessful": true,
      "comment": "Our application was accepted after appeal and the school was responsive.",
      "submittedAtUtc": "2026-05-21T10:30:00Z"
    }
  ],
  "nextCursor": "eyJ2IjoxLCJwYWdlU2l6ZSI6MjAsImxhc3RTdWJtaXR0ZWRBdFV0YyI6IjIwMjYtMDUtMjFUMTA6MzA6MDBaIiwibGFzdElkIjoicmV2XzEyMyJ9"
}
```

Validation and failure expectations:

- Unknown `schoolId` returns `404 Not Found`.
- Blank or invalid `cursor` values return `400 Bad Request`.
- Invalid `pageSize` values return `400 Bad Request`.
- No public comments return `200 OK` with `"reviews": []` and `"nextCursor": null`.

Baseline notes:

- This endpoint returns only comments that are publicly visible after moderation.
- Internal moderation status is not exposed in this public list response.
- Pagination order must be stable for cursor traversal.

### 5.7 Public Comment Report Submission

`POST /api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports`

Purpose:
Allow a public caller to report a specific visible comment for review.

Authentication:
Public.

Route parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `schoolId` | string | Yes | Canonical school identifier. |
| `reviewId` | string | Yes | Review identifier scoped to the school. |

Request body:

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `reason` | string | Yes | Initial baseline values are `spam`, `abusive`, `privacy`, or `other`. |
| `details` | string | No | Optional supporting detail. Required when `reason` is `other`. |
| `botVerificationToken` | string | No | Bot-verification token for abuse-control enforcement when enabled. |

Example request:

```json
{
  "reason": "spam",
  "details": "Repeated promotional content.",
  "botVerificationToken": "test-token"
}
```

Response:

`202 Accepted`

```json
{
  "id": "rev_123",
  "status": "reportAccepted",
  "reportedAtUtc": "2026-05-21T11:00:00Z"
}
```

Validation and failure expectations:

- Malformed request bodies return `400 Bad Request`.
- Missing or unsupported `reason` returns `400 Bad Request`.
- Blank `details` when `reason` is `other` returns `400 Bad Request`.
- `details` longer than 1000 characters returns `400 Bad Request`.
- Failed bot verification returns `400 Bad Request`.
- Unknown `schoolId` returns `404 Not Found`.
- Unknown `reviewId`, a review that does not belong to the supplied school, or a non-reportable review returns `404 Not Found`.
- Anonymous callers that exceed the reporting rate limit return `429 Too Many Requests`.

Baseline notes:

- Reporting is public and separate from admin moderation actions.
- The first valid report against a visible review that has not previously been approved hides it immediately, changes its moderation status to `pendingApproval`, and places it in the admin queue.
- After an admin approves a hidden reported review, it is visible again. It remains visible until 10 further distinct reporters have submitted valid reports since that approval; the 10th report hides it, changes its moderation status to `pendingReapproval`, and returns it to the admin queue.
- Distinct reporters are determined using a best-effort anonymous reporter fingerprint because public user accounts are outside this milestone's scope.
- Cloudflare Turnstile is the initial CAPTCHA provider for public reporting, with the same non-production disable or local-mock support as public review submission.

### 5.8 Admin Review Moderation Queue

`GET /api/admin/csa-application-reviews`

Purpose:
Allow an authenticated admin to discover reviews that require moderation.

Authentication:
Admin-only using ASP.NET Core Identity.

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `queueState` | string | No | Optional queue-state filter. Supported values are `PendingApproval` and `PendingReapproval`, repeated or comma-separated. Defaults to both states. |
| `cursor` | string | No | Opaque continuation token from a previous queue response. |
| `pageSize` | integer | No | Optional positive result limit. Defaults to `25` and must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "reviews": [
    {
      "id": "rev_123",
      "reviewerName": "Parent A",
      "applicationSuccessful": true,
      "comment": "Our application was accepted after appeal and the school was responsive.",
      "status": "pendingApproval",
      "submittedAtUtc": "2026-05-21T10:30:00Z",
      "openReportCount": 1,
      "postApprovalDistinctReportCount": 0,
      "latestReportAtUtc": "2026-05-21T11:00:00Z",
      "school": {
        "id": "sch_123",
        "urn": 100001,
        "name": "Northbridge Primary"
      },
      "reports": [
        {
          "id": "rep_123",
          "reason": "spam",
          "details": "Repeated promotional content.",
          "submittedAtUtc": "2026-05-21T11:00:00Z"
        }
      ]
    }
  ],
  "nextCursor": "eyJ2IjoxLCJxdWV1ZVN0YXRlcyI6IlBlbmRpbmdBcHByb3ZhbCxQZW5kaW5nUmVhcHByb3ZhbCIsImxhc3RTdWJtaXR0ZWRBdFV0YyI6IjIwMjYtMDUtMjFUMTA6MzA6MDBaIiwibGFzdElkIjoicmV2XzEyMyJ9"
}
```

Validation and failure expectations:

- Unauthenticated callers receive `401 Unauthorized`.
- Authenticated non-admin callers receive `403 Forbidden`.
- Invalid queue filters, cursors, or page sizes return `400 Bad Request`.

Baseline notes:

- This route is the discovery surface for the moderation workflow; review decisions continue to use the moderation action route below.
- The default queue contains both first-report `pendingApproval` reviews and threshold-triggered `pendingReapproval` reviews.

### 5.9 Admin Review Moderation

`POST /api/admin/csa-application-reviews/{reviewId}/moderation`

Purpose:
Allow an authenticated admin to moderate a submitted review.

Authentication:
Admin-only using ASP.NET Core Identity.

Route parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `reviewId` | string | Yes | Review identifier. |

Request body:

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `decision` | string | Yes | Initial baseline values are `approve` or `reject`. |
| `moderatorNote` | string | No | Optional admin note. |

Example request:

```json
{
  "decision": "approve",
  "moderatorNote": "Looks valid."
}
```

Response:

`200 OK`

```json
{
  "id": "rev_123",
  "status": "approved",
  "moderatedAtUtc": "2026-05-21T10:45:00Z",
  "moderatorNote": "Looks valid."
}
```

Validation and failure expectations:

- Unauthenticated callers receive `401 Unauthorized`.
- Authenticated non-admin callers receive `403 Forbidden`.
- Missing or unsupported `decision` returns `400 Bad Request`.
- Unknown `reviewId` returns `404 Not Found`.
- Reviews that are not currently in `pendingApproval` or `pendingReapproval` return `409 Conflict`.

Baseline notes:

- The baseline fixes approve or reject as the minimum moderation action set.
- Approving a `pendingApproval` or `pendingReapproval` review makes it publicly visible, resolves its open reports, and removes it from the moderation queue. Reapproving a `pendingReapproval` review also resets the post-approval report counter.
- Rejecting either queued status keeps the review hidden, resolves its open reports, and removes it from the moderation queue.

### 5.10 Admin School Import Trigger

`POST /api/admin/school-imports`

Purpose:
Allow an authenticated admin to trigger school import work.

Authentication:
Admin-only using ASP.NET Core Identity.

Request body:

- No request body is required in the baseline contract.
- Later milestones may add explicit import-source options if the implementation requires them.

Response:

`202 Accepted`

```json
{
  "id": "imp_123",
  "status": "queued"
}
```

Validation and failure expectations:

- Unauthenticated callers receive `401 Unauthorized`.
- Authenticated non-admin callers receive `403 Forbidden`.

Baseline notes:

- Milestone 1 only needs a protected import contract placeholder so protected school-management operations are not ambiguous.
- Import workflow detail is intentionally deferred.

## 6. Cross-Cutting Validation and Security Expectations

- Protected operations must explicitly document both `401 Unauthorized` and `403 Forbidden` outcomes.
- Public operations must clearly distinguish invalid input from valid-but-empty result sets.
- Search and lookup operations must not silently fall back from one lookup mode to another.
- School response objects must include `Id` even when the caller reached the resource by URN lookup.
- Anonymous public review submission and reporting routes now document both abuse-control validation failures (`400`) and throttle failures (`429`).
- Later generated OpenAPI output must preserve the same field names and required-field expectations defined here unless a later milestone deliberately revises the contract.
- Contract-level auth expectations reference ASP.NET Core Identity, but Milestone 1 does not define bootstrap, persistence, seeding, or login endpoint details.

## 7. Deferred Decisions

The following decisions are intentionally not settled by this baseline and must be handled by later milestones:

- exact ASP.NET Core Identity setup, persistence, admin bootstrap, and sign-in flow;
- exact search ranking implementation details, excluding the agreed decision that free-text school-discovery pagination on `GET /api/schools/search?q=...` uses opaque cursor tokens in the existing `cursor` and `nextCursor` contract fields;
- geospatial storage and query implementation;
- maximum supported radius and handling for schools with missing location data;
- exact URN format validation constraints beyond requiring an exact query identifier;
- final length and content rules for `name` and `comment`;
- report reason taxonomy expansion and the precise anonymous reporter-fingerprint implementation used for duplicate-report handling;
- exact rate-limit policy values and Cloudflare Turnstile configuration details;
- distribution workflow for generated OpenAPI outside the running API project once implementation is complete.

## 8. Downstream Milestone Inputs

### Milestone 2: Admin Security for Protected Operations

- Implement ASP.NET Core Identity-backed authentication and admin authorization for `/api/admin/*` endpoints.
- Enforce the documented `401` and `403` behaviours.
- Confirm protected moderation and import endpoints generate the expected OpenAPI security metadata.

### Milestone 3: School Discovery and Lookup APIs

- Implement school discovery using the chosen PostgreSQL hybrid search approach that combines full-text search with `pg_trgm` similarity support.
- Preserve `GET /api/schools` as the plain collection route while using `GET /api/schools/search` for both free-text school discovery and exact URN lookup.
- Implement opaque cursor-token pagination for free-text discovery on `GET /api/schools/search`, preserving the existing `cursor` query parameter name and `nextCursor` response field while changing the value format away from raw school identifiers.
- Implement exact URN lookup on `GET /api/schools/search?urn=...` using the same full school response shape used by the main schools GET contract.
- Validate search behaviour against the required searchable fields: school name and address or postcode.
- Keep URN lookup distinct from free-text discovery in request validation and generated OpenAPI while returning `Id` in school schemas.

### Milestone 4: Spatial School Search Support

- Implement `GET /api/schools/nearby` using the fixed latitude, longitude, and `radiusMiles` contract.
- Add the location storage and query support needed to return map-usable school results.
- Validate coordinate and radius failure paths in generated API behaviour and OpenAPI output.

### Milestone 5: CSA Application Review Submission and Moderation

- Implement public review submission at `POST /api/schools/{schoolId}/csa-application-reviews` using the baseline `name`, `applicationSuccessful`, and `comment` fields.
- Implement public comment retrieval at `GET /api/schools/{schoolId}/csa-application-reviews`, returning only publicly visible comments.
- Implement public comment reporting at `POST /api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports`.
- Make new reviews visible by default, hide them on the first valid report as `pendingApproval`, and hide an admin-approved review again as `pendingReapproval` after 10 further distinct reporters.
- Implement the admin moderation queue at `GET /api/admin/csa-application-reviews` and moderation decisions using the baseline moderation endpoint and minimum decision set.
- Use Cloudflare Turnstile for public submission and reporting, with verification disabled or locally mocked in non-production environments.
- Validate school and review mismatch cases plus invalid report payload failure paths.
- Add rate limiting without breaking the baseline request and response surface unless a deliberate contract revision is approved.

### Milestone 6: Contract Stabilization for UI Handoff

- Validate the combined implemented surface against this markdown baseline.
- Resolve any remaining contract ambiguities before UI handoff.
- Ensure the generated OpenAPI output from the API project matches the implemented routes, `Id`-bearing schemas, and major error behaviours expected by this baseline.

## 9. Stability Statement

This contract baseline is stable enough for downstream implementation planning and UI contract discussions because it fixes:

- the initial endpoint inventory;
- the separation between search, lookup, spatial query, submission, and admin-only operations;
- the initial required request and response fields for each in-scope operation;
- the baseline validation and error expectations that materially affect consumers;
- the implementation choices that are already agreed versus the decisions that remain deferred.
