# Milestone 1 Contract Baseline and Delivery Decisions

## 1. Purpose and Status

This document is the Milestone 1 markdown contract baseline for the initial Summer Born Information API surface.

It is the pre-implementation contract artifact that downstream milestones must implement and validate. The final contract handoff for the UI team remains generated OpenAPI output from the ASP.NET Core API project once Milestones 2 through 6 are complete.

This baseline is intentionally concrete at the HTTP contract layer and intentionally light on implementation detail. It defines the initial endpoint inventory, request and response shapes, validation expectations, and protected-operation rules without locking in deferred architecture choices.

## 2. Confirmed Delivery Decisions

- The published API contract will ultimately be generated from the ASP.NET Core API project rather than maintained as a separate hand-authored contract artifact.
- Milestone 1 delivers this markdown baseline so later milestones can implement toward a stable surface before generated OpenAPI exists.
- Admin-protected operations must be implemented using ASP.NET Core Identity as the agreed authentication direction.
- Free-text search, exact URN lookup, and radius-based school discovery are separate capabilities and must remain separate in the contract.
- The initial public CSA Application Review contract must include `name`, `applicationSuccessful`, and a free-text `comment`.
- `Id` is the canonical resource identifier carried by API response objects, while school-specific POST operations continue to use `schoolId` route parameters.

## 3. In-Scope Contract Surface

This baseline covers:

- admin-protected operations for school imports and CSA Application Review moderation;
- public school discovery by free-text search;
- public school retrieval by exact URN;
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
- Exact URN lookup remains a separate public GET capability for callers that start from a URN rather than a `schoolId`.
- School association for review submission and reporting is carried by route parameter rather than an optional body field.
- Public comments are the moderated CSA Application Reviews exposed for school-specific display.

### 4.2 Shared School Model

`SchoolSummary`

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | string | Yes | Canonical school resource identifier used in response payloads. |
| `urn` | string | Yes | Exact school identifier exposed for dedicated URN lookup and external reference. |
| `name` | string | Yes | School display name. |
| `addressLine1` | string | Yes | Primary address line. |
| `addressLine2` | string | No | Optional secondary address line. |
| `townOrCity` | string | Yes | Town or city used in result display. |
| `postcode` | string | Yes | Postcode shown in search and lookup results. |
| `latitude` | number | No | Present when location data is available for map-oriented use. |
| `longitude` | number | No | Present when location data is available for map-oriented use. |

`SchoolDetail`

- The Milestone 1 baseline keeps `SchoolDetail` aligned to `SchoolSummary`.
- Later milestones may expand the detail shape only when the generated OpenAPI contract and consumers can absorb the change deliberately.

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
- Valid search requests that return no matches still return `200 OK` with an empty `items` array.
- `429 Too Many Requests` is reserved for later abuse-protection implementation and is not required to be implemented in Milestone 1.

## 5. Endpoint Inventory

### 5.1 Public Free-Text School Search

`GET /api/schools/search`

Purpose:
Return ranked school matches using free-text matching against school name and address or postcode fields.

Authentication:
Public.

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `q` | string | Yes | Free-text search term. Must not be blank. |
| `cursor` | string | No | Continuation value from a previous response. Must not be blank when supplied. |
| `limit` | integer | No | Optional positive result limit. If supplied, must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "items": [
    {
      "id": "sch_123",
      "urn": "123456",
      "name": "Example Primary School",
      "addressLine1": "1 High Street",
      "addressLine2": null,
      "townOrCity": "Exampletown",
      "postcode": "AB1 2CD",
      "latitude": 51.501,
      "longitude": -0.141
    }
  ],
  "nextCursor": "eyJsYXN0U2Nob29sSWQiOiJzY2hfMTIzIn0",
  "hasMore": true
}
```

Validation and failure expectations:

- Blank or missing `q` returns `400 Bad Request`.
- Blank or invalid `cursor` values return `400 Bad Request`.
- Invalid `limit` values return `400 Bad Request`.
- No matches return `200 OK` with `"items": []`, `"nextCursor": null`, and `"hasMore": false`.

Baseline notes:

- Matching targets school `name` and address or postcode fields only.
- Results are ranked, but the ranking algorithm is deferred.
- Pagination uses cursor-based continuation over the returned ranked order.

### 5.2 Public Exact URN Lookup

`GET /api/schools/{urn}`

Purpose:
Return one school by exact URN as a distinct capability from free-text search.

Authentication:
Public.

Route parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `urn` | string | Yes | Exact school URN. |

Response:

`200 OK`

```json
{
  "id": "sch_123",
  "urn": "123456",
  "name": "Example Primary School",
  "addressLine1": "1 High Street",
  "addressLine2": null,
  "townOrCity": "Exampletown",
  "postcode": "AB1 2CD",
  "latitude": 51.501,
  "longitude": -0.141
}
```

Validation and failure expectations:

- Invalid URN format returns `400 Bad Request`.
- Unknown URN returns `404 Not Found`.

Baseline notes:

- URN is the lookup input for this specific operation, but the returned school resource still includes the canonical `Id`.

### 5.3 Public Radius-Based School Search

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
| `limit` | integer | No | Optional positive result limit. If supplied, must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "items": [
    {
      "id": "sch_123",
      "urn": "123456",
      "name": "Example Primary School",
      "addressLine1": "1 High Street",
      "addressLine2": null,
      "townOrCity": "Exampletown",
      "postcode": "AB1 2CD",
      "latitude": 51.501,
      "longitude": -0.141
    }
  ],
  "nextCursor": "eyJsYXN0U2Nob29sSWQiOiJzY2hfMTIzIn0",
  "hasMore": true
}
```

Validation and failure expectations:

- Missing `latitude`, `longitude`, or `radiusMiles` returns `400 Bad Request`.
- Out-of-range coordinates return `400 Bad Request`.
- Zero or negative `radiusMiles` returns `400 Bad Request`.
- Blank or invalid `cursor` values return `400 Bad Request`.
- Invalid `limit` values return `400 Bad Request`.
- No matches return `200 OK` with `"items": []`, `"nextCursor": null`, and `"hasMore": false`.

Baseline notes:

- Radius-from-point is the only required geospatial mode in the initial release.
- The fixed contract unit is miles to avoid a baseline-level unit negotiation decision.
- Pagination uses cursor-based continuation over the returned ordered result set.

### 5.4 Public CSA Application Review Submission

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

Example request:

```json
{
  "name": "Parent A",
  "applicationSuccessful": true,
  "comment": "Our application was accepted after appeal and the school was responsive."
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
  "status": "pendingModeration",
  "submittedAtUtc": "2026-05-21T10:30:00Z"
}
```

Validation and failure expectations:

- Malformed request bodies return `400 Bad Request`.
- Blank `name` returns `400 Bad Request`.
- Missing `applicationSuccessful` returns `400 Bad Request`.
- Blank `comment` returns `400 Bad Request`.
- Unknown `schoolId` returns `404 Not Found`.

Baseline notes:

- The route carries the school association by `schoolId` so the body cannot drift from the target school.
- Public submission is synchronous at contract level with `201 Created`.
- Abuse protection is required later, but its mechanism is deferred.

### 5.5 Public School Comment Retrieval

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
| `limit` | integer | No | Optional positive result limit. If supplied, must be within the supported maximum. |

Response:

`200 OK`

```json
{
  "items": [
    {
      "id": "rev_123",
      "schoolId": "sch_123",
      "name": "Parent A",
      "applicationSuccessful": true,
      "comment": "Our application was accepted after appeal and the school was responsive.",
      "submittedAtUtc": "2026-05-21T10:30:00Z"
    }
  ],
  "nextCursor": "eyJsYXN0UmV2aWV3SWQiOiJyZXZfMTIzIn0",
  "hasMore": true
}
```

Validation and failure expectations:

- Unknown `schoolId` returns `404 Not Found`.
- Blank or invalid `cursor` values return `400 Bad Request`.
- Invalid `limit` values return `400 Bad Request`.
- No public comments return `200 OK` with `"items": []`, `"nextCursor": null`, and `"hasMore": false`.

Baseline notes:

- This endpoint returns only comments that are publicly visible after moderation.
- Internal moderation status is not exposed in this public list response.
- Pagination order must be stable for cursor traversal.

### 5.6 Public Comment Report Submission

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

Example request:

```json
{
  "reason": "spam",
  "details": "Repeated promotional content."
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
- Unknown `schoolId` returns `404 Not Found`.
- Unknown `reviewId`, a review that does not belong to the supplied school, or a non-reportable review returns `404 Not Found`.

Baseline notes:

- Reporting is public and separate from admin moderation actions.
- The contract only fixes submission and acceptance behaviour; downstream moderation workflow remains flexible.

### 5.7 Admin Review Moderation

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
  "reviewId": "rev_123",
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

Baseline notes:

- The baseline fixes approve or reject as the minimum moderation action set.
- Richer moderation states remain a later decision.

### 5.8 Admin School Import Trigger

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
- Later generated OpenAPI output must preserve the same field names and required-field expectations defined here unless a later milestone deliberately revises the contract.
- Contract-level auth expectations reference ASP.NET Core Identity, but Milestone 1 does not define bootstrap, persistence, seeding, or login endpoint details.

## 7. Deferred Decisions

The following decisions are intentionally not settled by this baseline and must be handled by later milestones:

- exact ASP.NET Core Identity setup, persistence, admin bootstrap, and sign-in flow;
- search ranking implementation details;
- exact cursor format, traversal method, maximum cursor page size, cursor lifetime or expiry behaviour, filtering rules, and optional sort modes beyond the baseline query shape;
- geospatial storage and query implementation;
- maximum supported radius and handling for schools with missing location data;
- exact URN format validation constraints beyond requiring an exact route identifier;
- final length and content rules for `name` and `comment`;
- richer moderation state transitions beyond `approve` and `reject`;
- report reason taxonomy expansion, duplicate-report handling, reporter anonymity or storage design, and moderation workflow after report receipt;
- rate limiting, CAPTCHA or equivalent bot protection, and related abuse-control implementation details;
- distribution workflow for generated OpenAPI outside the running API project once implementation is complete.

## 8. Downstream Milestone Inputs

### Milestone 2: Admin Security for Protected Operations

- Implement ASP.NET Core Identity-backed authentication and admin authorization for `/api/admin/*` endpoints.
- Enforce the documented `401` and `403` behaviours.
- Confirm protected moderation and import endpoints generate the expected OpenAPI security metadata.

### Milestone 3: School Discovery and Lookup APIs

- Implement `GET /api/schools/search` and `GET /api/schools/{urn}` to the request, response, and error contracts defined here.
- Validate search behaviour against the required searchable fields: school name and address or postcode.
- Keep URN lookup distinct from free-text search in both route design and generated OpenAPI while returning `Id` in school schemas.

### Milestone 4: Spatial School Search Support

- Implement `GET /api/schools/nearby` using the fixed latitude, longitude, and `radiusMiles` contract.
- Add the location storage and query support needed to return map-usable school results.
- Validate coordinate and radius failure paths in generated API behaviour and OpenAPI output.

### Milestone 5: CSA Application Review Submission and Moderation

- Implement public review submission at `POST /api/schools/{schoolId}/csa-application-reviews` using the baseline `name`, `applicationSuccessful`, and `comment` fields.
- Implement public comment retrieval at `GET /api/schools/{schoolId}/csa-application-reviews`, returning only publicly visible comments.
- Implement public comment reporting at `POST /api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports`.
- Implement admin moderation using the baseline moderation endpoint and minimum decision set.
- Validate school and review mismatch cases plus invalid report payload failure paths.
- Add abuse-control measures without breaking the baseline request and response surface unless a deliberate contract revision is approved.

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
