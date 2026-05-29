# Milestone 4 Spatial School Search Support

## 1. Overview

This plan turns Milestone 4 from the roadmap into delivery-ready work for map-based and proximity-based school discovery.

The implementation goal is to add a durable spatial model to schools, expose the Milestone 1 radius-search contract at `GET /api/schools/nearby`, and produce deterministic, paged nearby-school results that are suitable for downstream map-oriented UI work.

Current implementation context on 2026-05-28:

- `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` currently exposes the public collection and discovery routes delivered for Milestone 3, but does not yet expose `GET /api/schools/nearby`.
- `API/SummerBornInfo.Domain/Entities/School.cs` and the current school response contract do not yet include persisted spatial coordinates, a geometry/geography location value, or map-usable coordinate fields in school API responses.
- `API/SummerBornInfo.Infrastructure/Persistence/ApplicationDbContext.cs` currently enables PostgreSQL support for `pg_trgm`, but not yet PostGIS.
- `API/SummerBornInfo.Infrastructure/Persistence/PostgreSqlDatabaseBootstrapper.cs` currently bootstraps `pg_trgm` only, so local and integration-test databases do not yet create or validate the PostGIS extension.
- Milestone 3 already established the public school DTO, paging wrapper, OpenAPI expectations, and PostgreSQL-specific query approach, which Milestone 4 should extend rather than replace.

## 2. Roadmap Source or Existing Plan Context

Roadmap source:

- [Roadmap/initial-api-roadmap.md](C:\Projects\summer-born-info\Roadmap\initial-api-roadmap.md), Milestone 4: `Spatial School Search Support`

Milestone 1 baseline source:

- [Plans/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Plans\milestone-1-contract-baseline-and-delivery-decisions.md)

Milestone 3 implementation plan source:

- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md)

Milestone 1 defines the downstream expectations that this milestone must implement:

- `GET /api/schools/nearby` is the public radius-based search route.
- The request contract requires `latitude`, `longitude`, and `radiusMiles`.
- `latitude` must validate within `-90` to `90`.
- `longitude` must validate within `-180` to `180`.
- `radiusMiles` must validate as a positive number no greater than `100`.
- Nearby search responses use the same `{ schools, nextCursor }` wrapper shape as the public school collection contract.
- Valid nearby searches with no matches return `200 OK` with an empty `schools` collection and `nextCursor: null`.
- The returned contract must be suitable for map-oriented UI work while preserving the agreed school response schema.

Confirmed user clarification:

- Milestone 4 should use PostgreSQL `PostGIS` for spatial storage and querying.
- EF Core spatial mapping should use `NetTopologySuite`.

## 3. Scope

This milestone includes:

- introducing the spatial persistence model required to store a usable school point location;
- enabling `PostGIS` in the application, local development, and integration-test database bootstrap paths;
- mapping school spatial data through EF Core using `NetTopologySuite`;
- defining how school location is sourced during import when valid coordinates are available;
- exposing `GET /api/schools/nearby` as the public radius-search route;
- validating coordinate, radius, cursor, and page-size inputs for nearby search;
- implementing radius-from-point querying and distance-ordered paging against PostgreSQL spatial indexes;
- deciding how schools with missing location data are handled so the contract remains predictable;
- extending the shared full school response DTO with map-usable coordinate fields across all public school GET routes while preserving the `{ schools, nextCursor }` wrapper contract where it already applies;
- documenting the route and its failure modes in generated OpenAPI output;
- adding HTTP-level integration coverage for successful nearby search, empty results, invalid input, and multi-page traversal;
- updating API-facing documentation for the new public nearby-search capability.

## 4. Non-Goals

- changing the Milestone 3 `GET /api/schools` or `GET /api/schools/search` contract;
- introducing polygon, bounding-box, route-based, or travel-time search modes;
- exposing raw geometry objects in the public API contract;
- adding client-facing distance formatting, map tiles, or reverse geocoding;
- implementing a separate geocoding pipeline for schools that currently have no trustworthy coordinates unless a minimal import hook is required to persist already-available source coordinates;
- adding advanced spatial ranking beyond radius filtering and deterministic nearby ordering;
- changing public review, moderation, or admin-authentication workflows.

## 5. Behaviour Scenarios

### Scenario 1: Nearby search returns schools within a radius

Given schools exist with valid stored point locations  
When `GET /api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5` is called  
Then the API returns `200 OK`  
And only schools within the requested radius are returned  
And the response uses the existing `{ schools, nextCursor }` wrapper  
And each school uses the agreed full school response shape extended with map-usable latitude and longitude fields.

### Scenario 2: Nearby results are ordered deterministically for map and list use

Given multiple schools match the requested radius  
When `GET /api/schools/nearby` is called  
Then results are ordered by ascending distance from the supplied point  
And a deterministic tie-breaker such as `Id` is applied when distances are effectively equal  
And pagination does not produce duplicates or skipped results across pages.

### Scenario 3: Valid nearby search with no matches returns an empty page

Given no schools with valid stored locations fall within the requested radius  
When `GET /api/schools/nearby` is called with valid coordinates and radius  
Then the API returns `200 OK`  
And the response contains `"schools": []`  
And `nextCursor` is `null`.

### Scenario 4: Invalid nearby-search inputs return `400`

Given the caller omits `latitude`, `longitude`, or `radiusMiles`, supplies out-of-range coordinates, supplies a zero, negative, or greater-than-100 `radiusMiles`, supplies an invalid cursor, or supplies an unsupported `pageSize`  
When `GET /api/schools/nearby` is called  
Then the API returns `400 Bad Request`  
And the error response follows the shared baseline error shape  
And the API does not coerce invalid values into a broader or narrower search.

### Scenario 5: Schools without location data do not produce ambiguous results

Given some imported schools do not have a valid stored point location  
When `GET /api/schools/nearby` is called  
Then schools without a persisted point location are excluded from nearby results  
And the API does not guess or synthesize a location at query time  
And the behaviour is documented so consumers understand why a school may appear in text discovery but not in nearby results.

### Scenario 6: Nearby pagination uses an opaque continuation token

Given more nearby schools match than fit in one page  
When the caller follows `nextCursor` from a nearby-search response  
Then the next page continues from the prior ordered result set without duplicates or skipped records  
And the `cursor` query parameter remains the public continuation input name  
And the cursor value is treated as an opaque server-generated token rather than raw location or row identifiers alone  
And the cursor is only valid when replayed with the original nearby-search inputs and compatible paging parameters.

### Scenario 7: Spatial storage and querying use PostGIS with NetTopologySuite

Given the project has chosen PostgreSQL `PostGIS` and EF Core `NetTopologySuite`  
When spatial persistence and querying are implemented  
Then school location is stored in a PostGIS-backed spatial column  
And EF Core maps that column using `NetTopologySuite` types  
And nearby search executes through database-side spatial filtering and ordering rather than application-side in-memory distance calculations.

### Scenario 8: OpenAPI output is ready for UI consumption

Given the API runs with OpenAPI enabled  
When the generated document is inspected  
Then it exposes `GET /api/schools/nearby` with `latitude`, `longitude`, `radiusMiles`, `cursor`, and `pageSize` query parameters  
And it documents `200 OK` and `400 Bad Request` outcomes  
And the response schema matches the agreed school collection wrapper and full school DTO contract.

## 6. Deliverables

1. Spatial persistence foundation

- Add the PostgreSQL spatial capability required for the schools table by enabling `PostGIS`.
- Introduce a persisted point location field on `school` suitable for radius querying.
- Decide and document whether the persisted type should be PostGIS `geography(Point, 4326)` or `geometry(Point, 4326)`, with the default plan using `geography(Point, 4326)` for meter-based radius distance calculations on latitude/longitude coordinates.
- Add the spatial index needed for efficient radius filtering and distance ordering.

2. EF Core and infrastructure spatial support

- Add the required EF Core spatial package support for Npgsql plus `NetTopologySuite`.
- Update `ApplicationDbContext` and `SchoolConfiguration` so the school entity maps the persisted point location correctly.
- Ensure the generated database script reflects PostGIS extension usage and spatial column/index creation.

3. Shared PostgreSQL bootstrap evolution

- Extend `PostgreSqlDatabaseBootstrapper` so it creates and validates both `pg_trgm` and `postgis`.
- Update the local Aspire-backed PostgreSQL startup path so development databases can create the spatial extension before schema creation.
- Update the Testcontainer integration environment so test databases also create and validate the spatial extension before schema creation.
- Preserve one shared bootstrap ownership path so local and test environments stay aligned.

4. School location data model and import alignment

- Extend the domain and persistence model to carry a canonical stored school point location.
- Decide how source coordinates are mapped into that location during school import.
- Parse source `Easting` and `Northing` values from the import feed and convert British National Grid coordinates to WGS84 longitude and latitude before persisting the canonical point location.
- Reject or skip invalid coordinate source values rather than persisting malformed points.
- Expose public latitude and longitude fields in the shared school response DTO used by all public school GET routes so downstream map UI work can place markers without reverse-engineering internal spatial storage.

5. Nearby-search query slice

- Add a dedicated `Features/Schools/Queries/GetNearbySchools` slice with request model, handler, validation, and response mapping.
- Expose the public route at `GET /api/schools/nearby`.
- Filter schools by radius from the supplied latitude and longitude using database-side spatial predicates.
- Order results by ascending distance from the supplied point, then apply a deterministic tie-breaker.
- Preserve the existing collection wrapper shape.
- Extend the shared school response DTO with map-usable latitude and longitude fields for `GET /api/schools`, `GET /api/schools/search`, `GET /api/schools/search?urn=...`, and `GET /api/schools/nearby`.

6. Nearby-search validation and pagination

- Validate required `latitude`, `longitude`, and `radiusMiles` inputs.
- Enforce latitude range `-90` to `90`.
- Enforce longitude range `-180` to `180`.
- Enforce `radiusMiles > 0`.
- Enforce `radiusMiles <= 100`.
- Validate non-blank and decodable opaque nearby-search cursors.
- Treat nearby cursors as bound to the original `latitude`, `longitude`, `radiusMiles`, and compatible `pageSize` inputs, returning `400 Bad Request` when callers replay a cursor against incompatible search parameters.
- Preserve the existing paging defaults unless implementation evidence requires change:
  - default page size `100`;
  - maximum page size `200`.
- Implement an opaque continuation token that captures the ordered nearby-search resume boundary, including enough ordering context to continue distance-ordered traversal safely.

7. OpenAPI and error metadata

- Add endpoint metadata so OpenAPI reflects the Milestone 1 nearby-search contract.
- Ensure `400 Bad Request` outcomes are documented explicitly.
- Ensure the documented nearby-search parameter rules include the maximum supported radius of `100` miles.
- Keep the response schema aligned with the existing school collection and school DTO contract.

8. Automated verification

- Add integration tests for:
  - successful nearby search with one or more matches;
  - no-match nearby search;
  - invalid or missing `latitude`, `longitude`, and `radiusMiles`;
  - radius values above `100` miles;
  - invalid `cursor` and `pageSize`;
  - invalid cursor replay against different nearby-search inputs;
  - multi-page nearby traversal with stable ordering;
  - exclusion of schools with missing location data;
  - OpenAPI contract exposure for `GET /api/schools/nearby`.
- Add infrastructure tests that validate the generated database script includes `postgis`, the spatial column, and the spatial index.

9. Documentation updates

- Update API-facing documentation to describe `GET /api/schools/nearby`.
- Document the chosen `PostGIS` plus `NetTopologySuite` approach and why it was selected.
- Record how missing school coordinates affect nearby-search results so downstream UI work can account for it.

## 7. Technology Requirements and Decisions

Confirmed decisions:

- Use PostgreSQL `PostGIS` for spatial storage and querying.
- Use EF Core `NetTopologySuite` for spatial type mapping.
- Preserve the Milestone 1 route contract at `GET /api/schools/nearby`.
- Preserve the current full school DTO and `{ schools, nextCursor }` response wrapper.

Implementation decisions:

- Store each school's canonical search location as a single PostGIS point rather than as separate query-only latitude and longitude columns.
- Default the persisted spatial column to `geography(Point, 4326)` so radius queries and distance ordering over WGS84 latitude/longitude coordinates can use meter-based calculations without bespoke conversion logic.
- Use `NetTopologySuite.Geometries.Point` in the domain/persistence mapping for the canonical location field.
- Convert imported British National Grid `Easting` and `Northing` values to WGS84 longitude and latitude before constructing the canonical persisted point.
- Keep the public nearby-search request contract in latitude/longitude plus `radiusMiles`, converting miles to meters in the query layer before executing spatial predicates.
- Limit public nearby-search radius values to a maximum of `100` miles.
- Expose latitude and longitude in the shared school response DTO returned by all public school GET routes so the API surface stays consistent and genuinely usable for map-marker placement.
- Exclude schools with null or missing canonical locations from nearby-search results instead of returning partial or guessed positions.
- Implement nearby searching as a dedicated vertical slice under `API/SummerBornInfo.Features/Schools/Queries/GetNearbySchools`.
- Use database-side spatial filtering and ordering through PostGIS functions/operators rather than loading candidate schools into application memory.
- Use an opaque encoded cursor for nearby traversal because continuation depends on ordered distance results rather than on `Id` alone.
- Treat nearby cursors as tied to the originating search parameters so callers cannot safely replay them against a different point or radius.

Rationale:

- `PostGIS` keeps the spatial workload inside PostgreSQL and aligns with the project's existing decision to use PostgreSQL-native capabilities for search concerns;
- `NetTopologySuite` is the standard EF Core path for strongly typed spatial mapping and avoids ad hoc coordinate handling;
- a single canonical point field is simpler and less drift-prone than managing separate public and private coordinate representations;
- `geography(Point, 4326)` is a good fit for mile-based radius queries over earth coordinates because it reduces the risk of inaccurate manual spherical-distance calculations;
- the current import feed already carries `Easting` and `Northing`, so converting those to WGS84 at import time is a cleaner fit than storing British National Grid coordinates directly in the public search model;
- keeping the HTTP request in latitude/longitude plus miles preserves the Milestone 1 contract and avoids exposing storage-specific details;
- capping nearby searches at `100` miles keeps the first-release contract bounded and reduces the risk of expensive, low-signal wide-area scans;
- returning latitude and longitude in the shared school response DTO is necessary for map-oriented UI work and keeps the public school schema consistent even if the database persists a richer internal spatial type;
- excluding schools with missing locations keeps nearby-search behaviour honest and predictable for consumers;
- opaque cursors are necessary because stable continuation depends on a distance-based ordering tuple and the originating search parameters, not on one identifier alone.

## 8. Dependencies and Sequencing

1. Confirm the Milestone 1 nearby-search route and validation contract.
2. Add `PostGIS` and `NetTopologySuite` support to the persistence and infrastructure layer.
3. Extend the shared PostgreSQL bootstrap path so both local and integration-test databases provision `postgis` before schema creation.
4. Add the school location persistence model and spatial index.
5. Align school import or seed paths so valid source coordinates populate the canonical location field, including conversion from imported `Easting` and `Northing` values to WGS84 longitude and latitude.
6. Add the `GetNearbySchools` query slice, route, validation, and mapping, including the 100-mile maximum radius rule.
7. Add nearby-search cursor handling for stable multi-page traversal and incompatible-cursor replay rejection.
8. Add OpenAPI metadata and error response declarations.
9. Add integration and infrastructure tests.
10. Update API-facing documentation.

Dependency notes:

- Milestone 4 depends on the Milestone 3 school DTO and paging wrapper remaining stable.
- Milestone 5 review features can remain independent of the spatial model, but downstream UI work may expect Milestone 4 and Milestone 5 to coexist on the same school resource surface.
- The current import example already includes `Easting` and `Northing`, so this milestone should treat coordinate parsing and WGS84 conversion as part of the primary implementation path rather than as an optional later enrichment.

### Sequential Task Breakdown

Deliver the milestone as the following one-task-at-a-time sequence, with one git commit after each completed task:

1. Task 1: Spatial package and model foundation

- Add the required Npgsql/EF Core spatial packages and entity mapping support for `NetTopologySuite`.
- Outcome: the solution can represent a canonical school point location in the persistence model.
- Commit boundary: package references and entity mapping only.

2. Task 2: PostGIS bootstrap support

- Extend the shared PostgreSQL bootstrap component, AppHost startup path, and Testcontainer setup so `postgis` is created and validated before schema creation.
- Outcome: local and integration-test databases can create the spatial schema reliably.
- Commit boundary: bootstrap and environment support only.

3. Task 3: Spatial schema and indexing

- Add the school spatial column and spatial index to the schema configuration.
- Outcome: the database can store and query school locations efficiently.
- Commit boundary: schema and indexing only.

4. Task 4: Import and location persistence alignment

- Populate the canonical location field from valid source coordinates during school import or equivalent data-ingestion flow.
- Parse imported `Easting` and `Northing` values and convert them to WGS84 longitude and latitude before building the canonical point.
- Outcome: imported schools with usable coordinates become discoverable through nearby search.
- Commit boundary: import/data-mapping behaviour only.

5. Task 5: Nearby-search route contract

- Add `GET /api/schools/nearby` route wiring, request model, and validation path.
- Extend the shared school response contract with map-usable latitude and longitude fields across all public school GET routes.
- Enforce the maximum supported nearby-search radius of `100` miles.
- Outcome: the public nearby-search endpoint exists with the Milestone 1 query contract and validation behaviour.
- Commit boundary: route and validation only.

6. Task 6: Nearby-search query implementation

- Implement radius filtering, distance ordering, and first-page nearby retrieval through the `GetNearbySchools` slice.
- Outcome: callers can retrieve the first page of nearby schools for a valid point and radius.
- Commit boundary: nearby query behaviour without continuation-token traversal.

7. Task 7: Nearby pagination and cursor continuation

- Add opaque cursor generation and continuation handling for distance-ordered nearby traversal, including rejection of incompatible cursor replay with different search inputs.
- Outcome: multi-page nearby-search traversal is stable and deterministic.
- Commit boundary: cursor and pagination behaviour only.

8. Task 8: OpenAPI and error metadata

- Add explicit endpoint metadata for the nearby-search route and failure modes.
- Outcome: generated OpenAPI accurately reflects the nearby-search contract.
- Commit boundary: OpenAPI and metadata only.

9. Task 9: Integration and infrastructure verification

- Add HTTP-level nearby-search tests plus infrastructure tests for PostGIS schema/bootstrap expectations.
- Outcome: the milestone is regression-protected across API behaviour and database provisioning.
- Commit boundary: automated verification only, unless a minimal production fix is required to make tests pass.

10. Task 10: Documentation alignment

- Update API documentation for the nearby-search capability, spatial technology choice, and missing-location behaviour.
- Outcome: contributors and downstream consumers can understand the supported nearby-search surface and its limitations.
- Commit boundary: documentation only.

### Task State Checklist

- [x] Task 1 complete: Spatial package and model foundation committed.
- [x] Task 2 complete: PostGIS bootstrap support committed.
- [x] Task 3 complete: Spatial schema and indexing committed.
- [x] Task 4 complete: Import and location persistence alignment committed.
- [x] Task 5 complete: Nearby-search route contract committed.
- [x] Task 6 complete: Nearby-search query implementation committed.
- [x] Task 7 complete: Nearby pagination and cursor continuation committed.
- [x] Task 8 complete: OpenAPI and error metadata committed.
- [x] Task 9 complete: Integration and infrastructure verification committed.
- [ ] Task 10 complete: Documentation alignment committed.

## 9. Risks and Mitigations

- Spatial source-data risk:
  Nearby-search quality depends on imported schools having valid and sufficiently complete coordinates.
  Mitigation: make missing-location behaviour explicit, reject malformed coordinates during import, and make British National Grid to WGS84 conversion an explicit tested part of the import path rather than an implied follow-up.

- Extension parity risk:
  Spatial work can pass in one PostgreSQL environment and fail in another if `postgis` is not provisioned consistently.
  Mitigation: route development and integration tests through the same shared bootstrap component that creates and validates `postgis` before schema creation.

- Query-correctness risk:
  Radius filtering and ordering can become inaccurate if the wrong spatial type, SRID, or distance semantics are used.
  Mitigation: standardize on one canonical point type and SRID, keep miles-to-meters conversion explicit, and cover representative radius-search cases in tests.

- Wide-query risk:
  Very large radius requests can increase query cost while producing low-value broad-area results for the initial UI experience.
  Mitigation: enforce a maximum supported radius of `100` miles and document the boundary in the contract and OpenAPI output.

- Pagination risk:
  Distance-ordered result sets are more complex to continue safely than simple identifier ordering.
  Mitigation: use opaque continuation tokens that encode the ordered resume boundary and test multi-page traversal thoroughly.

- Contract drift risk:
  Spatial work could accidentally introduce an inconsistent school DTO variant or route shape.
  Mitigation: treat the existing `{ schools, nextCursor }` wrapper as fixed, add latitude and longitude to one shared public school DTO across all school GET routes, and verify the resulting schema in OpenAPI and HTTP tests.

- Performance risk:
  Nearby queries may degrade if the schema stores locations without the right spatial index or if ordering forces inefficient scans.
  Mitigation: make spatial indexing a first-class deliverable and validate the schema artifacts alongside behaviour tests.

## 10. Resolved Clarifications

- Milestone 4 will use PostgreSQL `PostGIS` for spatial storage and querying.
- Milestone 4 will use EF Core `NetTopologySuite` for spatial mapping.
- The nearby-search contract remains `GET /api/schools/nearby` with `latitude`, `longitude`, and `radiusMiles`.
- Nearby search will reject `radiusMiles` values greater than `100`.
- The shared public school DTO used by collection, search, URN lookup, and nearby responses will be extended with map-usable latitude and longitude fields.
- Schools without a valid stored location will be excluded from nearby-search results rather than receiving guessed coordinates.
- The default persistence direction for school locations is a canonical PostGIS point stored as `geography(Point, 4326)`.
- Imported `Easting` and `Northing` values will be converted to WGS84 longitude and latitude before persistence.
- Nearby cursors will be valid only for the originating nearby-search inputs and compatible paging parameters.

This plan is delivery-ready for Milestone 4 with the spatial technology decision now fixed to `PostGIS` plus `NetTopologySuite`.

## 11. Completion Checklist

- [x] `GET /api/schools/nearby` exists and matches the Milestone 1 request and response contract.
- [x] Nearby search validates required `latitude`, `longitude`, and `radiusMiles` inputs.
- [x] Nearby search rejects out-of-range latitude and longitude values with `400 Bad Request`.
- [x] Nearby search rejects zero or negative `radiusMiles` with `400 Bad Request`.
- [x] Nearby search rejects `radiusMiles` values greater than `100` with `400 Bad Request`.
- [x] Nearby search rejects invalid `cursor` and `pageSize` values with `400 Bad Request`.
- [x] Nearby search returns `200 OK` with an empty `schools` collection and `nextCursor: null` when no schools match.
- [x] Nearby search returns only schools within the requested radius.
- [x] Nearby search orders results deterministically by ascending distance with a stable tie-breaker.
- [x] Nearby-search pagination uses opaque cursor values in the existing `cursor` and `nextCursor` contract fields.
- [x] Nearby-search cursors are rejected when replayed against incompatible `latitude`, `longitude`, `radiusMiles`, or paging inputs.
- [x] Schools persist a canonical PostGIS-backed point location suitable for radius queries.
- [x] EF Core maps the school location through `NetTopologySuite`.
- [x] The application database model declares the `postgis` extension requirement.
- [x] The shared PostgreSQL bootstrap component creates and validates both `pg_trgm` and `postgis`.
- [x] The local development PostgreSQL environment supports the spatial extension before schema creation.
- [x] The integration-test PostgreSQL environment supports the spatial extension before schema creation.
- [x] The school spatial column and spatial index exist in the generated schema.
- [ ] Import or ingestion paths parse `Easting` and `Northing`, convert them to WGS84 longitude and latitude, and persist valid source coordinates into the canonical school location field.
- [ ] Schools without valid stored coordinates are excluded from nearby results in a documented, predictable way.
- [x] The shared public school DTO used by collection, search, URN lookup, and nearby responses includes map-usable latitude and longitude fields.
- [x] Generated OpenAPI output documents `GET /api/schools/nearby` and its validation behaviour.
- [x] Integration tests cover successful, empty, invalid-input, and multi-page nearby-search behaviour.
- [x] Infrastructure tests cover PostGIS bootstrap and schema artifacts.
- [ ] API documentation reflects the supported nearby-search surface and spatial technology choice.
