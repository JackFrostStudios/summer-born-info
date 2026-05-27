# Milestone 3 School Discovery and Lookup APIs

## 1. Overview

This plan turns Milestone 3 from the roadmap into delivery-ready work for the public school discovery surface.

The implementation goal is to extend the current `GET /api/schools` contract into the Milestone 1 school discovery contract while preserving the existing full school result shape by delivering:

- search behaviour on `GET /api/schools` for school discovery across school name and address or postcode fields;
- `GET /api/schools/{urn}` for exact URN lookup as a distinct capability;
- the expected `400 Bad Request`, `404 Not Found`, and empty-result behaviours;
- generated OpenAPI output that is stable enough for downstream UI development.

Current implementation context on 2026-05-25:

- `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` already exposes `GET /api/schools` with `cursor` and `pageSize` query parameters and returns the baseline collection wrapper shape of `{ schools, nextCursor }`.
- `API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/GetAllSchoolsQueryHandler.cs` currently pages by `Guid` cursor over all schools ordered by `Id`; it does not yet implement free-text filtering, ranked discovery, or URN lookup.
- `API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/Response/SchoolResponse.cs` already defines the full school result shape that the corrected Milestone 1 baseline now treats as the contract for public school responses.
- `API/SummerBornInfo.Domain/Entities/School.cs` stores `URN` as an `int`, which aligns with the current `GET /api/schools` result contract and can be reused for exact URN lookup.
- The current school entity does not yet store latitude or longitude, so Milestone 3 should preserve the existing full school response contract rather than introducing new spatial fields before Milestone 4.
- The local Aspire AppHost and the integration-test Testcontainer setup will need to ensure PostgreSQL instances support the extensions and initialization required for full-text and `pg_trgm`-backed search.

## 2. Roadmap Source or Existing Plan Context

Roadmap source:

- [Roadmap/initial-api-roadmap.md](C:\Projects\summer-born-info\Roadmap\initial-api-roadmap.md), Milestone 3: `School Discovery and Lookup APIs`

Milestone 1 baseline source:

- [Documentation/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Documentation\milestone-1-contract-baseline-and-delivery-decisions.md)

Milestone 1 defines the downstream expectations that this milestone must implement:

- `GET /api/schools` is the baseline public schools collection route and keeps the current full `{ schools, nextCursor }` result shape;
- `GET /api/schools/{urn}` is the public exact URN lookup route and remains distinct from the collection route;
- discovery matching on `GET /api/schools` targets school `name` and address or postcode fields only;
- blank or invalid search input returns `400 Bad Request` when search behaviour is invoked on the collection route;
- valid discovery requests with no matches return `200 OK` with an empty `schools` collection;
- unknown URNs return `404 Not Found`;
- school response objects use the full current school result shape and include canonical `id` even when the caller arrived via URN lookup.

Related existing implementation plan:

- [Plans/milestone-2-admin-security-for-protected-operations.md](C:\Projects\summer-born-info\Plans\milestone-2-admin-security-for-protected-operations.md)

Milestone 3 builds on the same solution structure and OpenAPI expectations established in Milestone 2, but does not depend on further authentication work because these discovery endpoints are public.

## 3. Scope

This milestone includes:

- implementing the chosen PostgreSQL full-text plus `pg_trgm` hybrid search approach for useful school discovery quality;
- extending the public schools collection route at `GET /api/schools` with discovery behaviour;
- implementing the public exact URN lookup route at `GET /api/schools/{urn}`;
- adding request validation for search term, cursor, page size, and URN format;
- preserving the current full school DTO and collection wrapper contract for both collection discovery and URN lookup;
- defining a deterministic ranked search strategy that works against imported school data without breaking the current collection contract;
- preserving the existing `cursor` and `pageSize` query contract while making ranked discovery pagination deterministic;
- updating endpoint metadata and OpenAPI output to reflect the discovery contract and failure modes;
- adding HTTP-level integration coverage for successful search, no-match search, invalid input, successful URN lookup, and unknown URN behaviour;
- keeping the current `GET /api/schools` list endpoint as the supported public contract and evolving it to satisfy the roadmap baseline.

## 4. Non-Goals

- radius-based or map-oriented school discovery, which belongs to Milestone 4;
- adding latitude or longitude persistence to the school model;
- changing school import behaviour or source-data shape beyond what search needs to read;
- introducing advanced filters, sort modes, or faceted search;
- adding a separate dedicated search service for Milestone 3;
- expanding the full school response contract beyond the corrected Milestone 1 baseline shape;
- replacing the existing full school response DTO with a compact summary shape.

## 5. Behaviour Scenarios

### Scenario 1: Free-text search returns ranked matches

Given imported schools exist with searchable values in `name`, address fields, or postcode  
When `GET /api/schools?q=amber` is called  
Then the API returns `200 OK`  
And matching schools are ordered by the selected ranking strategy  
And the response keeps the existing `{ schools, nextCursor }` wrapper  
And each school uses the full Milestone 1 school response contract.

### Scenario 1a: Chosen search technology supports realistic school discovery

Given the current schools dataset includes realistic school names and address fragments  
When `GET /api/schools` search is implemented using PostgreSQL full-text search combined with `pg_trgm` similarity support  
Then search quality supports tokenized school-name discovery plus partial, fragment, and typo-tolerant matching for realistic user input  
And the chosen technology is recorded with rationale in the milestone documentation.

### Scenario 2: Search matches address and postcode fields

Given a school name does not match the query text  
And its street, town, or postcode contains the query text  
When `GET /api/schools` is called with that text in `q`  
Then the school is still returned as a valid match  
And search behaviour does not require a name hit when address or postcode fields match.

### Scenario 3: Valid search with no results returns an empty page

Given no schools match the supplied search term  
When `GET /api/schools?q=nomatchvalue` is called  
Then the API returns `200 OK`  
And the response contains `"schools": []`  
And `nextCursor` is `null`.

### Scenario 4: Invalid search input returns `400`

Given the caller supplies only whitespace for `q`, supplies an invalid cursor, or supplies an unsupported `pageSize`  
When `GET /api/schools` is called  
Then the API returns `400 Bad Request`  
And the error response follows the shared baseline error shape  
And the API does not silently coerce invalid discovery inputs into another behaviour.

### Scenario 5: Search pagination remains stable across pages

Given more matches exist than fit in one response page  
When the caller follows `nextCursor` from the first page  
Then the second page continues from the prior ranked result set without duplicates or skipped items  
And pagination stability uses a deterministic tie-breaker when multiple schools share the same ranking bucket  
And the route continues to use the existing `cursor` query parameter contract.

### Scenario 6: Exact URN lookup returns one school

Given a school exists with URN `123456`  
When `GET /api/schools/123456` is called  
Then the API returns `200 OK`  
And the response uses the same full school result shape as `GET /api/schools`  
And the response still includes canonical `id` in addition to the URN.

### Scenario 7: Unknown or invalid URN returns the correct failure mode

Given the caller supplies a syntactically invalid URN value  
When `GET /api/schools/{urn}` is called  
Then the API returns `400 Bad Request`.

Given the caller supplies a valid URN format that does not exist  
When `GET /api/schools/{urn}` is called  
Then the API returns `404 Not Found`.

### Scenario 8: Route design keeps lookup distinct from other discovery routes

Given the API exposes both the main schools collection route and the URN lookup route  
When the router evaluates `/api/schools` and `/api/schools/{urn}`  
Then the collection route remains the canonical public schools GET surface  
And URN lookup does not alter the collection route contract.

### Scenario 9: OpenAPI output is ready for UI consumption

Given the API runs with OpenAPI enabled  
When the generated document is inspected  
Then it exposes `GET /api/schools` and `GET /api/schools/{urn}` with the expected query or route parameters  
And the school collection and school lookup schemas match the agreed field names and requiredness  
And `400` and `404` responses are visible where the contract requires them.

## 6. Deliverables

1. Discovery contract evolution on the existing collection route

- Preserve `GET /api/schools` as the canonical public schools GET route.
- Keep public school discovery grouped under `/api/schools`.
- Extend the existing collection route contract with discovery behaviour instead of introducing a replacement collection endpoint.

2. Search technology foundation

- Use PostgreSQL full-text search as the primary tokenized discovery mechanism across school name and address text.
- Add `pg_trgm` support for similarity-based matching, typo tolerance, and fragment matching where full-text search alone would be too brittle.
- Update the Aspire AppHost PostgreSQL setup so local development environments have the required extension support enabled.
- Update the Testcontainer-based integration environment so test databases initialize with the required extension support enabled.
- Document the chosen hybrid approach and why it was selected over simpler SQL matching and heavier dedicated-search infrastructure.

3. Full school response contract preservation

- Reuse the current `SchoolResponse` shape for discovery and URN lookup rather than introducing a compact summary DTO.
- Preserve the existing collection wrapper of `{ schools, nextCursor }`.
- Keep `URN` integer-backed in both persistence and HTTP response contracts unless a later milestone deliberately revises that decision.

4. Free-text search query slice

- Add a dedicated `Features/Schools/Queries/SearchSchools` slice with request model, handler, and response mapping.
- Keep the public route at `GET /api/schools`, with search input carried through query parameters on that route.
- Implement matching over school `Name`, `Address.Street`, `Address.Locality`, `Address.AddressThree`, `Address.Town`, `Address.County`, and `Address.PostCode`.
- Build ranking from the chosen PostgreSQL hybrid approach so it favors stronger matches such as:
  - high-confidence full-text matches on school name ahead of weaker address-only matches;
  - exact or prefix-like school-name matches boosted ahead of looser matches where appropriate;
  - postcode and address fragment matches supported through trigram similarity where relevant;
  - a stable tie-breaker such as `Id` after ranking and normalized display fields.
- Keep the ranking approach maintainable so weighting can be tuned later without changing the public route or response contract.

5. Exact URN lookup query slice

- Add a dedicated `Features/Schools/Queries/GetSchoolByUrn` slice.
- Validate the route value before querying the database.
- Return the same full school response shape used by `GET /api/schools`.
- Return `404` when no matching school exists and do not fall back to collection discovery behaviour.

6. Discovery validation and pagination support

- Add validation rules for non-blank `q` when supplied, non-blank and decodable cursors, and supported positive `pageSize` values.
- Standardize Milestone 3 pagination on the existing defaults unless implementation evidence requires a change:
  - default page size `100`;
  - maximum page size `200`.
- Preserve the externally visible `cursor` contract and implement the minimum server-side resume logic needed to keep ranked traversal stable.

7. OpenAPI and error metadata

- Add endpoint metadata so OpenAPI reflects the corrected Milestone 1 discovery surface on the existing collection route.
- Ensure `400` and `404` outcomes are documented on the relevant operations.
- Keep the full existing school schema aligned with the baseline to reduce downstream UI ambiguity.

8. Automated verification

- Add integration tests for:
  - search by school name;
  - search by address or postcode;
  - no-result search;
  - invalid `q`, `cursor`, and `pageSize` values;
  - multi-page ranked search traversal;
  - successful URN lookup;
  - invalid URN input;
  - unknown URN lookup;
  - OpenAPI contract exposure for the new discovery endpoints.
- Add representative-case coverage that exercises the chosen search technology against realistic school-name and address searches.
- Update existing `GET /api/schools` integration tests so they validate the discovery-enhanced behaviour without changing the response shape.

9. Documentation updates

- Update API-facing documentation to describe discovery behaviour on `GET /api/schools` plus exact URN lookup.
- Document the chosen PostgreSQL full-text plus `pg_trgm` hybrid approach and why it was selected over simpler SQL matching or a dedicated search service.
- Record any intentional relevance limitations or cursor rules that callers need to understand.

## 7. Technology Requirements and Decisions

Confirmed decisions:

- Keep school discovery public under `/api/schools`.
- Keep the main schools collection route and exact URN lookup as separate capabilities.
- Keep the response contract aligned with the full current school result shape.
- Leave location persistence and nearby search for Milestone 4.
- Use PostgreSQL full-text search combined with `pg_trgm` similarity support for Milestone 3 text discovery.

Implementation decisions:

- Preserve `School.URN` as an integer in the current domain, persistence, and HTTP response contracts.
- Implement collection discovery and URN lookup as separate vertical slices under `API/SummerBornInfo.Features/Schools/Queries`.
- Use PostgreSQL full-text search as the primary ranking basis over normalized school name and address text.
- Use `pg_trgm` similarity to supplement full-text search for fragment and typo-tolerant matching.
- Ensure both the Aspire AppHost PostgreSQL instance and the Testcontainer PostgreSQL instance support the required full-text and `pg_trgm` extension setup.
- Preserve the current `GET /api/schools` route and response shape rather than introducing a second collection shape for the same resource.

Rationale:

- keeping `URN` integer-backed avoids unnecessary persistence churn and aligns with the corrected baseline contract;
- the PostgreSQL hybrid approach gives materially better discovery quality than a simplistic SQL `LIKE` query while staying inside the current infrastructure stack;
- adding `pg_trgm` avoids the brittleness of full-text-only search for short school names, postcode fragments, and misspelled queries;
- preserving the existing collection route and schema avoids contract churn for downstream consumers while still allowing Milestone 3 to add discovery behaviour.

## 8. Dependencies and Sequencing

1. Confirm the corrected Milestone 1 route and response contract for `GET /api/schools`.
2. Add the PostgreSQL full-text plus `pg_trgm` search foundation.
3. Update the Aspire AppHost and Testcontainer environments to support the required PostgreSQL extensions.
4. Extend the current school endpoint registration with collection discovery behaviour and URN lookup.
5. Add the exact URN lookup slice and validation path.
6. Add the free-text search slice, ranking rules, and cursor support behind the existing collection contract.
7. Add endpoint metadata for error responses and OpenAPI descriptions.
8. Update existing HTTP tests with milestone-aligned search and lookup integration tests.
9. Update API documentation and any milestone checklist state once the implementation lands.

Dependency notes:

- Milestone 4 depends on the route surface and full school response contract from this milestone remaining stable when spatial data is added.
- Search pagination should be designed so a later Milestone 4 nearby-search implementation can coexist without forcing a Milestone 3 contract rewrite.

### Sequential Task Breakdown

Deliver the milestone as the following one-task-at-a-time sequence, with one git commit after each completed task:

1. Task 1: Discovery contract alignment on the existing route

- Align the endpoint plan to the corrected baseline so `GET /api/schools` remains the public collection route and `GET /api/schools/{urn}` is added as the separate lookup route.
- Outcome: the public route surface matches the corrected roadmap contract before search internals are finalized.
- Commit boundary: endpoint registration and route contract wiring only.

2. Task 2: PostgreSQL search foundation

- Add the PostgreSQL full-text and `pg_trgm` foundation needed for the chosen hybrid search approach.
- Outcome: the milestone has the selected search technology in place before higher-level discovery behaviour is implemented.
- Commit boundary: database/search foundation only.

3. Task 3: AppHost and Testcontainer PostgreSQL support

- Update the Aspire AppHost PostgreSQL provisioning and the Testcontainer-based test environment so both support the extensions or initialization required by the chosen search approach.
- Outcome: local development and automated tests run against PostgreSQL environments that can execute the full-text plus `pg_trgm` search implementation.
- Commit boundary: local/test environment support only.

4. Task 4: Full response contract preservation

- Reuse or refactor the existing full school response DTO and collection wrapper so both discovery and URN lookup return the agreed contract.
- Outcome: both collection discovery and URN lookup can return the agreed full school shape.
- Commit boundary: response contract and mapping only.

5. Task 5: Exact URN lookup implementation

- Add the `GetSchoolByUrn` query slice, validation, and endpoint handler.
- Outcome: callers can retrieve a school by exact URN with `400` versus `404` handled correctly.
- Commit boundary: URN lookup behaviour only.

6. Task 6: Free-text search implementation

- Add the `SearchSchools` query slice, matching logic, ranking, and first-page retrieval behaviour behind `GET /api/schools`.
- Outcome: callers can discover schools by name, address, or postcode through the existing collection route.
- Commit boundary: search behaviour without pagination edge cases beyond what the first page needs.

7. Task 7: Search pagination and cursor validation

- Add ranked continuation handling behind the existing `cursor` contract.
- Outcome: search can traverse multiple pages without unstable ordering, duplicates, or skipped results while preserving the current query contract.
- Commit boundary: pagination and cursor behaviour only.

8. Task 8: OpenAPI and error metadata

- Add explicit metadata for `400` and `404` responses and ensure schema output reflects the corrected full DTO.
- Outcome: generated OpenAPI is aligned with the implemented discovery contract.
- Commit boundary: documentation metadata only.

9. Task 9: Integration test coverage

- Replace or update web integration tests to cover milestone search and URN lookup behaviours while preserving the current collection response shape.
- Outcome: the public discovery contract is regression-protected at the HTTP level.
- Commit boundary: automated verification only, unless a minimal production fix is required to make tests pass.

10. Task 10: Documentation alignment

- Update `API/README.md` and any other implementation-facing notes to describe discovery behaviour on the existing schools GET route.
- Outcome: contributors and downstream consumers can discover the supported school discovery surface without relying on stale route assumptions.
- Commit boundary: documentation only.

### Task State Checklist

- [ ] Task 1 pending: Discovery contract alignment on the existing route.
- [ ] Task 2 pending: PostgreSQL search foundation.
- [ ] Task 3 pending: AppHost and Testcontainer PostgreSQL support.
- [ ] Task 4 pending: Full response contract preservation.
- [ ] Task 5 pending: Exact URN lookup implementation.
- [ ] Task 6 pending: Free-text search implementation.
- [ ] Task 7 pending: Search pagination and cursor validation.
- [ ] Task 8 pending: OpenAPI and error metadata.
- [ ] Task 9 pending: Integration test coverage.
- [ ] Task 10 pending: Documentation alignment.

## 9. Risks and Mitigations

- Search relevance risk:
  A simplistic query may technically work but still produce poor school ordering for realistic names and addresses.
  Mitigation: use the chosen PostgreSQL hybrid approach, cover representative search cases in tests, and keep weighting explicit so relevance can be tuned safely.

- Search technology risk:
  PostgreSQL full-text plus `pg_trgm` adds indexing and ranking complexity that can still be misconfigured or under-tuned.
  Mitigation: document the chosen approach, add representative search-case coverage, and keep normalization and ranking rules explicit in implementation.

- Environment parity risk:
  The search implementation may work against one PostgreSQL environment but fail in local development or integration tests if the required extensions are not provisioned consistently.
  Mitigation: make AppHost and Testcontainer support first-class milestone tasks and verify search behaviour against both environments.

- Pagination compatibility risk:
  Ranked results can conflict with the current `cursor` contract if continuation logic relies on more ordering context than the client currently sends.
  Mitigation: preserve the external contract and compute any extra resume state server-side from the last returned school and active search input.

- Contract drift risk:
  The discovery implementation could accidentally introduce a second collection shape or new route that competes with `GET /api/schools`.
  Mitigation: treat reuse of the existing route and full DTO as a first-class deliverable rather than a nice-to-have.

- URN route risk:
  `GET /api/schools/{urn}` still needs to coexist cleanly with the collection route and any future literal child routes.
  Mitigation: constrain or order routes deliberately and add tests proving lookup does not swallow future route expansions.

- Data-shape risk:
  Some imported school records may have missing street or optional address fields.
  Mitigation: implement search over nullable address members safely and keep required output fields aligned with data that is already mandatory in the entity model.

## 10. Resolved Clarifications

- Milestone 3 should not add nearby or radius search; that remains Milestone 4 work.
- The current `GET /api/schools` list endpoint is part of the supported public contract and must be preserved.
- Milestone 3 will use PostgreSQL full-text search combined with `pg_trgm` similarity support rather than a simplistic SQL-based search approach.
- The working page-size defaults for discovery should remain aligned with the existing values of `100` default and `200` maximum unless implementation evidence shows the baseline needs revision.

This plan is delivery-ready for Milestone 3 with the search technology now fixed to a PostgreSQL full-text plus `pg_trgm` hybrid approach.

## 11. Completion Checklist

- [ ] `GET /api/schools` remains the public schools collection route and matches the corrected Milestone 1 request and response contract.
- [ ] `GET /api/schools/{urn}` exists and remains a distinct capability from collection discovery.
- [ ] Search matches school name and address or postcode fields.
- [ ] PostgreSQL full-text search plus `pg_trgm` support is implemented or configured as the foundation for text discovery.
- [ ] The Aspire AppHost PostgreSQL environment supports the required full-text and `pg_trgm` extension setup.
- [ ] The Testcontainer PostgreSQL environment supports the required full-text and `pg_trgm` extension setup.
- [ ] The chosen text-search technology is documented with rationale.
- [ ] Search returns ranked results using deterministic ordering.
- [ ] Search returns `200 OK` with an empty `schools` collection when no schools match.
- [ ] Blank `q` returns `400 Bad Request` when search behaviour is invoked.
- [ ] Invalid search cursor values return `400 Bad Request`.
- [ ] Invalid `pageSize` values return `400 Bad Request`.
- [ ] Invalid URN input returns `400 Bad Request`.
- [ ] Unknown URN values return `404 Not Found`.
- [ ] Discovery responses use the full Milestone 1 school response shape.
- [ ] Discovery responses include canonical `id`.
- [ ] Discovery responses expose `urn` as the agreed integer HTTP contract field.
- [ ] The existing `GET /api/schools` contract remains part of the supported public API surface.
- [ ] Generated OpenAPI output documents the collection discovery and URN lookup routes with their expected parameters and error responses.
- [ ] Integration tests cover successful and failing search and URN lookup behaviours.
- [ ] API documentation reflects the supported school discovery surface.
