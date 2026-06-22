# API Reference

This document holds the deeper API behavior and runtime notes that do not belong in the onboarding README.

## Public School Discovery

The public school surface stays grouped under `/api/schools`, with separate routes for collection traversal, text discovery, exact URN lookup, and nearby search.

### `GET /api/schools`

Use `GET /api/schools` for plain collection traversal only. It returns the shared collection wrapper:

```json
{
  "schools": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "urn": 100001,
      "name": "Northbridge Primary",
      "latitude": 53.8008,
      "longitude": -1.5491
    }
  ],
  "nextCursor": "00000000-0000-0000-0000-000000000001"
}
```

- `pageSize` is optional and defaults to `100`.
- Large `pageSize` values are capped to `200` on this collection route.
- `cursor` continues traversal by school `id`, so the collection cursor is not opaque.

### `GET /api/schools/search?q=...`

Use `GET /api/schools/search` with `q` for free-text school discovery across school name plus address and postcode fields. Successful responses use the same `{ schools, nextCursor }` wrapper as the collection route.

- `q` is required for free-text search, trimmed server-side, and must contain at least 4 non-whitespace characters.
- `pageSize` is optional and defaults to `100`; values outside `1` to `200` return `400 Bad Request`.
- `cursor`, when supplied, must be the opaque `nextCursor` returned by an earlier search for the same normalized query text.
- Search cursors are versioned, query-bound continuation tokens. Callers should treat them as opaque values and not infer ordering from their contents.
- A valid search with no matches returns `200 OK` with `"schools": []` and `"nextCursor": null`.
- Supplying blank `q`, a short query, an invalid cursor, or both `q` and `urn` returns `400 Bad Request`.

### `GET /api/schools/nearby?latitude=...&longitude=...&radiusMiles=...`

Use `GET /api/schools/nearby` for radius-based school discovery around a caller-supplied point. Successful responses use the same `{ schools, nextCursor }` wrapper as the collection and free-text search routes.

- `latitude`, `longitude`, and `radiusMiles` are required.
- `latitude` must be between `-90` and `90`.
- `longitude` must be between `-180` and `180`.
- `radiusMiles` must be greater than `0` and no more than `100`.
- `pageSize` is optional and must be between `1` and `200` when supplied.
- `cursor`, when supplied, must be the opaque `nextCursor` returned by an earlier nearby search for the same `latitude`, `longitude`, `radiusMiles`, and compatible paging inputs.
- Nearby cursors are bound to the original search inputs. Replaying a cursor against a different point, radius, or incompatible page size returns `400 Bad Request`.
- A valid nearby search with no matches returns `200 OK` with `"schools": []` and `"nextCursor": null`.
- Nearby results exclude schools that do not have a persisted canonical location, so a school can appear in collection or text-search results without appearing in nearby results.

Nearby responses reuse the shared `SchoolResponse` shape returned by the other public school GET routes. That DTO now includes `latitude` and `longitude` when the school has a persisted canonical location, and `null` values when it does not.

### `GET /api/schools/search?urn=...`

Use `GET /api/schools/search?urn=...` for exact URN lookup. This is a distinct query mode on the discovery route and returns a single full `SchoolResponse` object rather than the collection wrapper.

- `urn` must be a positive integer or the API returns `400 Bad Request`.
- Unknown URNs return `404 Not Found`.
- Do not combine `urn` with `q`; the route requires exactly one discovery mode.

### Search implementation notes

The current search implementation uses PostgreSQL full-text search together with `pg_trgm`:

- full-text ranking uses PostgreSQL's `simple` configuration with `plainto_tsquery`, which keeps school and place-name tokens closer to their source values than stemmed language dictionaries would;
- trigram matching uses `word_similarity` to support partial-name, address-fragment, postcode-fragment, and mild typo-tolerant discovery that plain full-text search would miss.

This hybrid approach was chosen over plain SQL `LIKE` matching because it gives materially better ranking and fragment matching, and over a separate search service because the current discovery scope can be supported within the existing PostgreSQL stack.

### Nearby search implementation notes

Nearby search uses PostgreSQL `PostGIS` together with EF Core `NetTopologySuite`.

- Each school's canonical search location is stored as a PostGIS-backed `geography(Point, 4326)` value.
- Imported `Easting` and `Northing` values are converted to WGS84 longitude and latitude before that canonical point is persisted.
- If a later import row has blank or invalid `Easting` or `Northing` values, the stored canonical location is cleared rather than leaving stale coordinates in place.
- The API exposes those persisted coordinates back through the shared `SchoolResponse` contract as `latitude` and `longitude`; it does not expose raw spatial database types.
- Radius filtering and distance ordering run in PostgreSQL rather than in application memory, which keeps the nearby route aligned with the repository's PostgreSQL-first search approach.
- Schools without a persisted canonical location are intentionally excluded from nearby results instead of being assigned guessed coordinates at query time.

## CSA Application Reviews

The public CSA Application Review surface also lives under `/api/schools`:

- `POST /api/schools/{schoolId}/csa-application-reviews`
- `GET /api/schools/{schoolId}/csa-application-reviews`
- `POST /api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports`

Public review submission is visible by default. Reviews enter moderation only after reporting:

- the first valid report against a visible review that has not been admin-approved hides it immediately and moves it to `pendingApproval`;
- if an admin approves that hidden review, it becomes visible again;
- after reapproval, the review stays visible until 10 further distinct reporters submit valid reports, at which point it is hidden again and moves to `pendingReapproval`.

Distinct reporters are counted with a best-effort anonymous fingerprint rather than an authenticated user id.

### `POST /api/schools/{schoolId}/csa-application-reviews`

Successful responses return `201 Created` with the persisted review payload:

```json
{
  "id": "00000000-0000-0000-0000-000000000101",
  "schoolId": "00000000-0000-0000-0000-000000000001",
  "name": "Parent A",
  "applicationSuccessful": true,
  "comment": "Our application was accepted after appeal and the school was responsive.",
  "status": "visible",
  "submittedAtUtc": "2026-05-21T10:30:00Z"
}
```

- `name` is required and limited to 200 characters after trimming.
- `applicationSuccessful` is required.
- `comment` is required and limited to 4000 characters after trimming.
- `botVerificationToken` is accepted on the request body and is used when anonymous bot verification is enabled.
- Unknown schools return `404 Not Found`.
- Invalid payloads or failed bot verification return `400 Bad Request` with validation details.
- Anonymous submission is rate-limited and can return `429 Too Many Requests`.

### `GET /api/schools/{schoolId}/csa-application-reviews`

Successful responses return only publicly visible reviews, newest first, with cursor pagination:

```json
{
  "reviews": [
    {
      "id": "00000000-0000-0000-0000-000000000101",
      "name": "Parent A",
      "applicationSuccessful": true,
      "comment": "Our application was accepted after appeal and the school was responsive.",
      "submittedAtUtc": "2026-05-21T10:30:00Z"
    }
  ],
  "nextCursor": "eyJ2IjoxLCJwYWdlU2l6ZSI6MjAsImxhc3RTdWJtaXR0ZWRBdFV0YyI6IjIwMjYtMDUtMjFUMTA6MzA6MDBaIiwibGFzdElkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMTAxIn0"
}
```

- `pageSize` is optional, defaults to `20`, and is capped at `200`.
- `cursor`, when present, must be the opaque `nextCursor` from an earlier response generated with the same effective page size.
- Hidden reviews in `pendingApproval`, `pendingReapproval`, or `rejected` state are excluded from the public list.
- Unknown schools return `404 Not Found`.
- Invalid cursors or page sizes return `400 Bad Request`.

### `POST /api/schools/{schoolId}/csa-application-reviews/{reviewId}/reports`

Successful responses return `202 Accepted`:

```json
{
  "id": "00000000-0000-0000-0000-000000000101",
  "status": "reportAccepted",
  "reportedAtUtc": "2026-05-21T11:00:00Z"
}
```

- `reason` is required and must be one of `spam`, `abusive`, `privacy`, or `other`.
- `details` is required for `reason = other` and is otherwise optional up to 1000 characters.
- `botVerificationToken` is accepted on the request body and is used when anonymous bot verification is enabled.
- Unknown schools, unknown reviews, school and review mismatches, and attempts to report hidden reviews return `404 Not Found`.
- Invalid payloads or failed bot verification return `400 Bad Request` with validation details.
- Anonymous reporting is rate-limited and can return `429 Too Many Requests`.

## Admin Authentication And Bootstrap

The supported admin auth contract is intentionally limited to project-specific endpoints under `/api/admin/auth/*`:

- `POST /api/admin/auth/sign-in`
- `POST /api/admin/auth/sign-out`

The sign-in endpoint accepts an email/password payload, validates that the user is in the `Admin` role, and issues the ASP.NET Core Identity application cookie on success. Invalid credentials return `401 Unauthorized`. Valid credentials for a non-admin user return `403 Forbidden`. Sign-out clears the same cookie and returns `204 No Content`.

Protected admin operations remain under `/api/admin`, including:

- `POST /api/admin/school-imports`
- `GET /api/admin/csa-application-reviews`
- `POST /api/admin/csa-application-reviews/{reviewId}/moderation`

The review moderation workflow is now fully implemented behind those admin routes:

- `GET /api/admin/csa-application-reviews` returns queued `pendingApproval` and `pendingReapproval` reviews by default, with optional `queueState`, `cursor`, and `pageSize` query parameters;
- queue items include the school summary, review content, current status, open report count, post-approval distinct report count, latest report timestamp, and the open report list needed for moderator triage;
- `POST /api/admin/csa-application-reviews/{reviewId}/moderation` accepts `approve` or `reject` decisions;
- approving a queued review restores public visibility, resolves open reports, and resets the post-approval distinct report counter only when the review was in `pendingReapproval`;
- rejecting a queued review keeps it hidden and resolves open reports;
- admin queue and moderation routes return `401 Unauthorized` for unauthenticated callers and `403 Forbidden` for authenticated non-admin callers.

### Local development bootstrap

Local development uses startup-time admin upsert in Development. Configure the initial admin account with `dotnet user-secrets` against the web project:

```bash
dotnet user-secrets --project SummerBornInfo.Web/SummerBornInfo.Web.csproj set AdminUserEmail admin@example.com
dotnet user-secrets --project SummerBornInfo.Web/SummerBornInfo.Web.csproj set AdminUserPassword "Choose-a-strong-dev-password"
```

When the app starts in Development, it upserts that user, ensures the password matches the configured value, and assigns the `Admin` role. Both `AdminUserEmail` and `AdminUserPassword` must be supplied together when either is configured.

After the app is running, sign in by posting JSON to `/api/admin/auth/sign-in`:

```json
{
  "email": "admin@example.com",
  "password": "Choose-a-strong-dev-password"
}
```

Use a client that preserves cookies if you want to call protected admin endpoints in the same session.

### Production bootstrap

For non-development environments, the checked-in bootstrap artifact is [ProductionScripts/bootstrap-initial-admin.sql](../ProductionScripts/bootstrap-initial-admin.sql). Operators should run it with `psql` after the identity schema exists.

The script requires:

- `admin_email`
- `admin_password_hash`

Optional:

- `admin_role_name`, which defaults to `Admin`

Example:

```bash
psql "$DATABASE_URL" -v admin_email='admin@example.com' -v admin_password_hash='AQAAAAIAAYagAAAA...' -f ProductionScripts/bootstrap-initial-admin.sql
```

Important operational note: the script expects a precomputed ASP.NET Core Identity password hash. Operators should generate that hash from trusted application code or another trusted .NET environment and supply the resulting hash value to the script. The SQL script does not generate passwords or hashes itself.

The script is designed to be safely re-run. It creates missing role, user, and membership rows, normalizes the relevant key fields, and does not overwrite an existing non-null password hash for a matched user.

## GDAL Runtime And OSTN15 Grid

British National Grid (`EPSG:27700`) import conversion lives in `SummerBornInfo.CoordinateConversion`, which owns the real GDAL-backed implementation, the minimal GDAL runtime packages, and the bundled OSTN15 grid.

- `SummerBornInfo.Features` and `SummerBornInfo.Web` consume the converter through `IBritishNationalGridLocationConverter`; they should not bootstrap GDAL directly.
- `SummerBornInfo.CoordinateConversion` references `MaxRev.Gdal.Core`, carries the Windows and Linux minimal runtime packages, and copies `Resources/Gdal/share/uk_os_OSTN15_NTv2_OSGBtoETRS.tif` into `GridShifts/` for build, test, and publish outputs.
- Runtime bootstrap is lazy and internal to the real converter. On first real conversion, `BritishNationalGridLocationConverter` calls `GdalRuntimeConfiguration.Configure()`, which runs `GdalBase.ConfigureAll()`, keeps `PROJ_NETWORK` disabled, preserves the package-provided PROJ data paths, and appends the local `GridShifts` folder so conversion stays offline-capable.
- Most feature and web-hosted tests should use fake converter implementations instead of the real GDAL runtime. `SummerBornInfo.CoordinateConversion.Tests` is the authoritative suite for real-runtime behavior, including offline PROJ/grid-shift configuration and representative conversion coverage.
- Build, test, and publish outputs should contain the bundled OSTN15 grid under `GridShifts/uk_os_OSTN15_NTv2_OSGBtoETRS.tif`.
- Package-provided GDAL and PROJ data should continue to come from the `gdal.netcore` runtime layout, including a local `proj.db` that `GdalBase.ConfigureAll()` can register.
- If you are validating output manually, confirm that `proj.db` is present in the shipped GDAL/PROJ runtime data and that `GridShifts/uk_os_OSTN15_NTv2_OSGBtoETRS.tif` is present for the custom GB grid shift.
- Do not rely on outbound network access for grid downloads when debugging conversion issues; the expected fix path is to restore the bundled runtime data or search-path configuration instead.
