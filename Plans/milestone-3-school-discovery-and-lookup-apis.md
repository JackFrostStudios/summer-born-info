# Milestone 3 School Discovery and Lookup APIs

## 1. Overview

This plan turns Milestone 3 from the roadmap into delivery-ready work for the public school discovery surface.

The implementation goal is to preserve `GET /api/schools` as the public paged collection route while using a separate discovery route for the Milestone 1 school discovery contract by delivering:

- search behaviour on `GET /api/schools/search` for school discovery across school name and address or postcode fields;
- `GET /api/schools/search?urn=...` for exact URN lookup as a distinct query mode on the discovery route;
- the expected `400 Bad Request`, `404 Not Found`, and empty-result behaviours;
- generated OpenAPI output that is stable enough for downstream UI development.

Current implementation context on 2026-05-25:

- `API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs` already exposes `GET /api/schools` with `cursor` and `pageSize` query parameters and returns the baseline collection wrapper shape of `{ schools, nextCursor }`.
- `API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/GetAllSchoolsQueryHandler.cs` currently pages by raw school `Id` over all schools ordered by `Id`; it does not yet implement free-text filtering, ranked discovery, or URN lookup.
- `API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/Response/SchoolResponse.cs` already defines the full school result shape that the corrected Milestone 1 baseline now treats as the contract for public school responses.
- `API/SummerBornInfo.Domain/Entities/School.cs` stores `URN` as an `int`, which aligns with the current `GET /api/schools` result contract and can be reused for exact URN lookup.
- The current school entity does not yet store latitude or longitude, so Milestone 3 should preserve the existing full school response contract rather than introducing new spatial fields before Milestone 4.
- The local Aspire AppHost and the integration-test Testcontainer setup will use a shared PostgreSQL bootstrap path in application code to create and validate required extensions before `EnsureCreatedAsync` runs.

## 2. Roadmap Source or Existing Plan Context

Roadmap source:

- [Roadmap/initial-api-roadmap.md](C:\Projects\summer-born-info\Roadmap\initial-api-roadmap.md), Milestone 3: `School Discovery and Lookup APIs`

Milestone 1 baseline source:

- [Documentation/milestone-1-contract-baseline-and-delivery-decisions.md](C:\Projects\summer-born-info\Documentation\milestone-1-contract-baseline-and-delivery-decisions.md)

Milestone 1 defines the downstream expectations that this milestone must implement:

- `GET /api/schools` is the baseline public schools collection route and keeps the current full `{ schools, nextCursor }` result shape for plain collection traversal;
- `GET /api/schools/search` is the public school discovery route and uses the same full `{ schools, nextCursor }` result shape for ranked search results;
- `GET /api/schools/search?urn=...` is the public exact URN lookup route shape and remains distinct from the collection route;
- discovery matching on `GET /api/schools/search` targets school `name` and address or postcode fields only;
- blank or invalid search input returns `400 Bad Request` on the search route;
- valid discovery requests with no matches return `200 OK` with an empty `schools` collection;
- unknown URNs return `404 Not Found`;
- school response objects use the full current school result shape and include canonical `id` even when the caller arrived via URN lookup.

Related existing implementation plan:

- [Plans/milestone-2-admin-security-for-protected-operations.md](C:\Projects\summer-born-info\Plans\milestone-2-admin-security-for-protected-operations.md)

Milestone 3 builds on the same solution structure and OpenAPI expectations established in Milestone 2, but does not depend on further authentication work because these discovery endpoints are public.

## 3. Scope

This milestone includes:

- implementing the chosen PostgreSQL full-text plus `pg_trgm` hybrid search approach for useful school discovery quality;
- adding the public school discovery route at `GET /api/schools/search`;
- implementing exact URN lookup on `GET /api/schools/search?urn=...`;
- adding request validation for search term, URN input, cursor, page size, and mutually exclusive discovery modes;
- preserving the current full school DTO and collection wrapper contract for both collection discovery and URN lookup;
- defining a deterministic ranked search strategy that works against imported school data without breaking the existing collection contract;
- preserving the existing `cursor` parameter name and `pageSize` query contract on the new search route while making ranked discovery pagination deterministic;
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
When `GET /api/schools/search?q=amber` is called  
Then the API returns `200 OK`  
And matching schools are ordered by the selected ranking strategy  
And the response keeps the existing `{ schools, nextCursor }` wrapper  
And each school uses the full Milestone 1 school response contract  
And `nextCursor`, when present for free-text search results, is a server-generated opaque token rather than a raw school identifier.

### Scenario 1a: Chosen search technology supports realistic school discovery

Given the current schools dataset includes realistic school names and address fragments  
When `GET /api/schools/search` is implemented using PostgreSQL full-text search combined with `pg_trgm` similarity support  
Then search quality supports tokenized school-name discovery plus partial, fragment, and typo-tolerant matching for realistic user input  
And the implementation uses PostgreSQL's `simple` text search configuration with `plainto_tsquery` for full-text matching  
And trigram fragment matching uses `word_similarity`-style rules  
And search terms shorter than 4 characters return `400 Bad Request` rather than attempting fuzzy discovery  
And the chosen technology is recorded with rationale in the milestone documentation.

### Scenario 2: Search matches address and postcode fields

Given a school name does not match the query text  
And its street, town, or postcode contains the query text  
When `GET /api/schools/search` is called with that text in `q`  
Then the school is still returned as a valid match  
And search behaviour does not require a name hit when address or postcode fields match.

### Scenario 3: Valid search with no results returns an empty page

Given no schools match the supplied search term  
When `GET /api/schools/search?q=nomatchvalue` is called  
Then the API returns `200 OK`  
And the response contains `"schools": []`  
And `nextCursor` is `null`.

### Scenario 4: Invalid discovery input returns `400`

Given the caller supplies only whitespace for `q`, supplies a search term shorter than 4 characters, supplies both `q` and `urn`, supplies neither `q` nor `urn`, supplies an invalid cursor, or supplies an unsupported `pageSize`  
When `GET /api/schools/search` is called  
Then the API returns `400 Bad Request`  
And the error response follows the shared baseline error shape  
And the API does not silently coerce invalid discovery inputs into another behaviour.

### Scenario 5: Search pagination remains stable across pages

Given more matches exist than fit in one response page  
When the caller follows `nextCursor` from the first page  
Then the second page continues from the prior ranked result set without duplicates or skipped items  
And pagination stability uses a deterministic tie-breaker when multiple schools share the same ranking bucket  
And the route continues to use the existing `cursor` query parameter name on `GET /api/schools/search`  
And the cursor value is treated as an opaque server-generated continuation token for ranked search traversal.

### Scenario 6: Exact URN lookup returns one school

Given a school exists with URN `123456`  
When `GET /api/schools/search?urn=123456` is called  
Then the API returns `200 OK`  
And the response uses the same full school result shape as `GET /api/schools`  
And the response still includes canonical `id` in addition to the URN.

### Scenario 7: Unknown or invalid URN returns the correct failure mode

Given the caller supplies a syntactically invalid URN value  
When `GET /api/schools/search?urn=invalid` is called  
Then the API returns `400 Bad Request`.

Given the caller supplies a valid URN format that does not exist  
When `GET /api/schools/search?urn=999999` is called  
Then the API returns `404 Not Found`.

### Scenario 8: Route design keeps lookup distinct from other discovery routes

Given the API exposes both the main schools collection route and the URN lookup route  
When the router evaluates `/api/schools` and `/api/schools/search`  
Then the collection route remains the canonical public schools GET surface  
And URN lookup does not alter the collection route contract
And the discovery route uses explicit query validation so free-text search and URN lookup do not overlap ambiguously.

### Scenario 9: OpenAPI output is ready for UI consumption

Given the API runs with OpenAPI enabled  
When the generated document is inspected  
Then it exposes `GET /api/schools` with the expected query parameters  
And it exposes `GET /api/schools/search` with the expected query parameters  
And the school collection and school lookup schemas match the agreed field names and requiredness  
And `400` and `404` responses are visible where the contract requires them.

## 6. Deliverables

1. Discovery contract evolution on the existing collection route

- Preserve `GET /api/schools` as the canonical public schools GET route.
- Keep public school discovery grouped under `/api/schools`.
- Keep the collection route focused on plain collection traversal and use `GET /api/schools/search` as the dedicated discovery endpoint for both free-text and exact-URN lookup.

2. Search technology foundation

- Use PostgreSQL full-text search as the primary tokenized discovery mechanism across school name and address text.
- Add `pg_trgm` support for similarity-based matching, typo tolerance, and fragment matching where full-text search alone would be too brittle.
- Optimize the storage model for search-read performance by materializing search artifacts as stored generated columns on `school` rather than relying only on expression indexes.
- Update the Aspire AppHost PostgreSQL setup so local development environments have the required extension support enabled.
- Update the Testcontainer-based integration environment so test databases initialize with the required extension support enabled.
- Make a shared PostgreSQL bootstrap component the owner of extension creation and validation in both environments, running before `EnsureCreatedAsync`.
- Document the chosen hybrid approach and why it was selected over simpler SQL matching and heavier dedicated-search infrastructure.

3. Full school response contract preservation

- Reuse the current `SchoolResponse` shape for discovery and URN lookup rather than introducing a compact summary DTO.
- Preserve the existing collection wrapper of `{ schools, nextCursor }`.
- Change free-text search pagination on `GET /api/schools/search` to emit and accept opaque encoded cursor values while preserving the existing field and parameter names.
- Keep `URN` integer-backed in both persistence and HTTP response contracts unless a later milestone deliberately revises that decision.

4. Free-text search query slice

- Add a dedicated `Features/Schools/Queries/SearchSchools` slice with request model, handler, and response mapping.
- Expose the public search route at `GET /api/schools/search`, with free-text and exact-URN input carried through query parameters on that route.
- Implement matching over school `Name`, `Address.Street`, `Address.Locality`, `Address.AddressThree`, `Address.Town`, `Address.County`, and `Address.PostCode`.
- Store the search inputs on the shared `school` table as generated search columns:
  - a weighted `search_vector` generated `tsvector` column covering `Name`, `Street`, `Locality`, `AddressThree`, `Town`, `County`, and `PostCode`;
  - a `search_name_normalized` generated text column for trigram matching over normalized school names;
  - a `search_postcode_normalized` generated text column for trigram matching over normalized postcodes;
  - a `search_address_normalized` generated text column for trigram matching over normalized address fragments if representative tests show address-fragment quality needs it.
- Normalize generated search columns in SQL so the indexing rules are deterministic and shared by all callers:
  - use `coalesce(..., '')` for nullable source fields;
  - use lowercase normalization for trigram-targeted text;
  - normalize postcodes by lowercasing and removing whitespace before indexing and comparison.
- Implement full-text query semantics explicitly:
  - use PostgreSQL's `simple` text search configuration for the generated `search_vector`;
  - build the full-text query with `plainto_tsquery`;
  - treat surviving full-text terms as an implicit `AND` query.
- Implement trigram fragment matching explicitly:
  - use `word_similarity`-style matching for fragment-oriented school-name, address, and postcode discovery;
  - keep the trigram operator family consistent across implementation and tests so fragment behaviour is not left to interpretation.
- Handle short search input explicitly:
  - reject free-text search terms shorter than 4 characters with `400 Bad Request`;
  - do not fall back to degraded fuzzy matching for short inputs.
- Build ranking from the chosen PostgreSQL hybrid approach so it favors stronger matches such as:
  - high-confidence full-text matches on school name ahead of weaker address-only matches;
  - exact or prefix-like school-name matches boosted ahead of looser matches where appropriate;
  - postcode and address fragment matches supported through `word_similarity`-style trigram matching where relevant;
  - a stable tie-breaker such as `Id` after ranking and normalized display fields.
- Keep the ranking approach maintainable so weighting can be tuned later without changing the public route or response contract.

5. Exact URN lookup query slice

- Add a dedicated `Features/Schools/Queries/GetSchoolByUrn` slice.
- Validate the `urn` query value before querying the database.
- Return the same full school response shape used by `GET /api/schools`.
- Return `404` when no matching school exists and do not fall back to collection discovery behaviour.

6. Discovery validation and pagination support

- Add validation rules for mutually exclusive `q` and `urn`, non-blank `q`, minimum search-term length of 4 characters for free-text discovery, valid `urn`, non-blank and decodable opaque search cursors, and supported positive `pageSize` values on `GET /api/schools/search`.
- Standardize Milestone 3 pagination on the existing defaults unless implementation evidence requires a change:
  - default page size `100`;
  - maximum page size `200`.
- Preserve the existing `cursor` query parameter name and `{ schools, nextCursor }` wrapper on `GET /api/schools/search` while changing search cursor values to server-generated opaque tokens that encode the ranked resume boundary.

7. OpenAPI and error metadata

- Add endpoint metadata so OpenAPI reflects the corrected Milestone 1 surface for the collection route, search route, and URN lookup route.
- Ensure `400` and `404` outcomes are documented on the relevant operations.
- Keep the full existing school schema aligned with the baseline to reduce downstream UI ambiguity.

8. Automated verification

- Add integration tests for:
  - plain collection traversal on `GET /api/schools`;
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
- Update existing `GET /api/schools` integration tests so they continue to validate plain collection traversal, and add dedicated `GET /api/schools/search` tests for discovery behaviour.

9. Documentation updates

- Update API-facing documentation to describe plain collection behaviour on `GET /api/schools`, free-text discovery on `GET /api/schools/search`, and exact URN lookup.
- Document the chosen PostgreSQL full-text plus `pg_trgm` hybrid approach and why it was selected over simpler SQL matching or a dedicated search service.
- Record any intentional relevance limitations or cursor rules that callers need to understand.

## 7. Technology Requirements and Decisions

Confirmed decisions:

- Keep school discovery public under `/api/schools`.
- Keep the main schools collection route separate from the discovery route, with exact URN lookup exposed as a distinct query mode on that discovery route.
- Keep the response contract aligned with the full current school result shape.
- Leave location persistence and nearby search for Milestone 4.
- Use PostgreSQL full-text search combined with `pg_trgm` similarity support for Milestone 3 text discovery.

Implementation decisions:

- Preserve `School.URN` as an integer in the current domain, persistence, and HTTP response contracts.
- Implement collection discovery and URN lookup as separate vertical slices under `API/SummerBornInfo.Features/Schools/Queries`.
- Expose free-text discovery and exact URN lookup through `GET /api/schools/search` rather than overloading `GET /api/schools`.
- Use PostgreSQL full-text search as the primary ranking basis over normalized school name and address text.
- Use `pg_trgm` similarity to supplement full-text search for fragment and typo-tolerant matching.
- Use PostgreSQL's `simple` text search configuration for generated search vectors so school and place-name tokens remain close to their normalized source values.
- Use `plainto_tsquery` to turn free-text input into the primary full-text query so discovery semantics remain simple and predictable.
- Use `word_similarity`-style trigram matching for fragment-oriented postcode, address, and partial-name discovery instead of relying on whole-string similarity alone.
- Reject free-text search terms shorter than 4 characters with `400 Bad Request` instead of attempting degraded fuzzy matching on low-signal inputs.
- Materialize search storage on the shared `school` table using stored generated columns rather than expression indexes alone.
- Create a generated `search_vector` `tsvector` column that combines:
  - `Name` with the highest full-text weight;
  - `Town` and `PostCode` with a medium weight;
  - `Street`, `Locality`, `AddressThree`, and `County` with a lower weight.
- Create generated normalized text columns for school-name and postcode trigram search, and add a generated normalized combined-address column if representative search cases need it for acceptable address-fragment quality.
- Keep normalization in SQL generation expressions instead of query-time-only normalization or EF-only conventions so indexed values are deterministic and reusable across queries and tests.
- Create these indexes as the Milestone 3 baseline:
  - `GIN` on `search_vector`;
  - `GIN` with `gin_trgm_ops` on `search_name_normalized`;
  - `GIN` with `gin_trgm_ops` on `search_postcode_normalized`;
  - `GIN` with `gin_trgm_ops` on `search_address_normalized` if that column is included.
- Change free-text school-discovery pagination on `GET /api/schools/search` from raw school identifiers to opaque encoded cursor tokens carried in the existing `cursor` and `nextCursor` fields.
- Ensure both the Aspire AppHost PostgreSQL instance and the Testcontainer PostgreSQL instance support the required full-text and `pg_trgm` extension setup.
- Create a shared PostgreSQL bootstrap component that runs idempotent extension-creation SQL and validation before `EnsureCreatedAsync` in both development startup and the integration-test fixture.
- Preserve the current `GET /api/schools` route and response shape rather than introducing a second collection shape for the same resource.

Rationale:

- keeping `URN` integer-backed avoids unnecessary persistence churn and aligns with the corrected baseline contract;
- the PostgreSQL hybrid approach gives materially better discovery quality than a simplistic SQL `LIKE` query while staying inside the current infrastructure stack;
- adding `pg_trgm` avoids the brittleness of full-text-only search for short school names, postcode fragments, and misspelled queries;
- choosing PostgreSQL's `simple` configuration avoids surprising stemming for school names, abbreviations, and place-name tokens that behave more like identifiers than prose;
- choosing `plainto_tsquery` keeps the public discovery contract narrow and predictable by treating search input as plain text rather than a richer operator grammar;
- choosing `word_similarity`-style trigram matching better fits address fragments and partial school-name discovery than whole-string similarity;
- rejecting search strings shorter than 4 characters avoids low-signal fuzzy matching and gives the route a clear, testable boundary for noisy inputs;
- stored generated search columns favor read performance and predictable index usage over a leaner schema, which is the right trade for a public search endpoint;
- keeping normalization in generated SQL columns avoids duplicated query-time transformations and makes test and production ranking behavior easier to keep aligned;
- separating `GET /api/schools/search` from `GET /api/schools` makes the discovery behaviour, validation, and cursor contract explicit instead of mode-switching the collection route;
- opaque cursor tokens fit ranked keyset pagination better than raw entity identifiers because continuation depends on a query-specific ordering tuple rather than on `Id` alone;
- preserving the existing collection route and schema avoids contract churn for downstream consumers while still allowing Milestone 3 to add discovery behaviour.
- using a shared application/test bootstrap component for `pg_trgm` creation fits the current `EnsureCreated`-based setup better than container-only initialization and gives local development and integration tests one parity-preserving ownership path.

## 8. Dependencies and Sequencing

1. Confirm the corrected Milestone 1 route and response contract for `GET /api/schools` and `GET /api/schools/search`, including exact URN lookup on the search route.
2. Add the PostgreSQL full-text plus `pg_trgm` search foundation.
3. Update the Aspire AppHost and Testcontainer environments to support the required PostgreSQL extensions.
   This task includes adding the shared bootstrap component and ensuring it runs before `EnsureCreatedAsync` in both paths.
4. Extend the current school endpoint registration with collection discovery behaviour and URN lookup.
5. Add the exact URN lookup slice and validation path.
6. Add the free-text search slice, ranking rules, and cursor support behind the dedicated search route.
7. Add endpoint metadata for error responses and OpenAPI descriptions.
8. Update existing HTTP tests with milestone-aligned search and lookup integration tests.
9. Update API documentation and any milestone checklist state once the implementation lands.

Dependency notes:

- Milestone 4 depends on the route surface and full school response contract from this milestone remaining stable when spatial data is added.
- Search pagination should be designed so a later Milestone 4 nearby-search implementation can coexist without forcing a Milestone 3 contract rewrite.

### Sequential Task Breakdown

Deliver the milestone as the following one-task-at-a-time sequence, with one git commit after each completed task:

1. Task 1: Discovery contract alignment on the existing route

- Align the endpoint plan to the corrected baseline so `GET /api/schools` remains the public collection route and `GET /api/schools/search` is the dedicated discovery route for both free-text and URN lookup.
- Outcome: the public route surface matches the corrected roadmap contract before search internals are finalized.
- Commit boundary: endpoint registration and route contract wiring only.

2. Task 2: PostgreSQL search foundation

- Add the PostgreSQL full-text and `pg_trgm` foundation needed for the chosen hybrid search approach.
- Outcome: the milestone has the selected search technology in place before higher-level discovery behaviour is implemented.
- Commit boundary: database/search foundation only.

3. Task 3: AppHost and Testcontainer PostgreSQL support

- Update the Aspire AppHost PostgreSQL provisioning and the Testcontainer-based test environment so both support the extensions or initialization required by the chosen search approach.
- Outcome: local development and automated tests run against PostgreSQL environments that can execute the full-text plus `pg_trgm` search implementation.
- Outcome: both environments use the same bootstrap code to create and validate `pg_trgm` before schema creation depends on it.
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

- Add the `SearchSchools` query slice, matching logic, ranking, and first-page retrieval behaviour behind `GET /api/schools/search`.
- Outcome: callers can discover schools by name, address, or postcode through the existing collection route.
- Commit boundary: search behaviour without pagination edge cases beyond what the first page needs.

7. Task 7: Search pagination and cursor validation

- Add ranked continuation handling behind the existing `cursor` parameter name on `GET /api/schools/search` using opaque encoded cursor values.
- Outcome: search can traverse multiple pages without unstable ordering, duplicates, or skipped results while preserving the current query parameter names and response wrapper.
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

- [x] Task 1 complete: Discovery contract alignment on the existing route.
- [x] Task 2 complete: PostgreSQL search foundation.
- [x] Task 3 complete: AppHost and Testcontainer PostgreSQL support.
- [x] Task 4 complete: Full response contract preservation.
- [x] Task 5 complete: Exact URN lookup implementation.
- [x] Task 6 complete: Free-text search implementation.
- [x] Task 7 complete: Search pagination and cursor validation.
- [x] Task 8 complete: OpenAPI and error metadata.
- [x] Task 9 complete: Integration test coverage.
- [ ] Task 10 pending: Documentation alignment.

## 9. Risks and Mitigations

- Search relevance risk:
  A simplistic query may technically work but still produce poor school ordering for realistic names and addresses.
  Mitigation: use the chosen PostgreSQL hybrid approach, cover representative search cases in tests, and keep weighting explicit so relevance can be tuned safely.

- Search technology risk:
  PostgreSQL full-text plus `pg_trgm` adds indexing and ranking complexity that can still be misconfigured or under-tuned.
  Mitigation: document the generated-column storage model, add representative search-case coverage, and keep normalization, weighting, and index rules explicit in implementation.

- Environment parity risk:
  The search implementation may work against one PostgreSQL environment but fail in local development or integration tests if the required extensions are not provisioned consistently.
  Mitigation: make AppHost and Testcontainer support first-class milestone tasks, route both through the same bootstrap component before `EnsureCreatedAsync`, and verify search behaviour against both environments.

- Pagination compatibility risk:
  Ranked results can conflict with raw-identifier cursor semantics because continuation logic relies on more ordering context than a bare school `Id` can express.
  Mitigation: keep ranked search on `GET /api/schools/search` and preserve the `cursor` parameter name and collection wrapper there while switching search cursor values to an opaque token that carries the ranked resume boundary and can be versioned.

- Contract drift risk:
  The discovery implementation could accidentally blur the contract boundary between plain collection traversal and ranked search.
  Mitigation: treat `GET /api/schools`, `GET /api/schools/search`, and the shared full DTO as first-class deliverables with explicit tests and OpenAPI coverage.

- Discovery mode risk:
  Sharing `GET /api/schools/search` between free-text discovery and exact URN lookup could create ambiguous or weakly validated request combinations.
  Mitigation: require exactly one of `q` or `urn`, document the rule in OpenAPI, and add tests covering invalid mixed-mode requests.

- Data-shape risk:
  Some imported school records may have missing street or optional address fields.
  Mitigation: implement search over nullable address members safely and keep required output fields aligned with data that is already mandatory in the entity model.

## 10. Resolved Clarifications

- Milestone 3 should not add nearby or radius search; that remains Milestone 4 work.
- The current `GET /api/schools` list endpoint is part of the supported public contract and must be preserved.
- Milestone 3 will expose free-text discovery on `GET /api/schools/search` rather than overloading `GET /api/schools`.
- Milestone 3 will use PostgreSQL full-text search combined with `pg_trgm` similarity support rather than a simplistic SQL-based search approach.
- Milestone 3 will optimize search-read performance with stored generated search columns on `school`, led by a generated weighted `search_vector` plus generated normalized trigram helper columns.
- Milestone 3 will change free-text school-discovery cursor values on `GET /api/schools/search` from raw school identifiers to opaque encoded continuation tokens while preserving the existing `cursor` query parameter name and `{ schools, nextCursor }` response wrapper.
- Milestone 3 will use PostgreSQL's `simple` text search configuration and `plainto_tsquery` for the full-text portion of school discovery.
- Milestone 3 will use `word_similarity`-style trigram matching for fragment-oriented discovery support.
- Milestone 3 will reject free-text search terms shorter than 4 characters with `400 Bad Request`.
- The working page-size defaults for discovery should remain aligned with the existing values of `100` default and `200` maximum unless implementation evidence shows the baseline needs revision.
- Milestone 3 will use a shared application/test PostgreSQL bootstrap step to create and validate `pg_trgm` before `EnsureCreatedAsync` in both local Aspire-backed development and the Testcontainer integration-test flow.

This plan is delivery-ready for Milestone 3 with the search technology now fixed to a PostgreSQL full-text plus `pg_trgm` hybrid approach.

## 11. Completion Checklist

- [ ] `GET /api/schools` remains the public schools collection route and matches the corrected Milestone 1 request and response contract.
- [ ] `GET /api/schools/search` exists as the public school discovery route and matches the corrected Milestone 1 request and response contract.
- [ ] `GET /api/schools/search?urn=...` exists as the exact URN lookup shape and remains a distinct capability from plain collection discovery.
- [x] Search matches school name and address or postcode fields.
- [x] PostgreSQL full-text search plus `pg_trgm` support is implemented or configured as the foundation for text discovery.
- [ ] Search storage uses stored generated search columns on `school`, including a weighted `search_vector` and normalized trigram helper columns required by the chosen query design.
- [ ] Search normalization rules are explicitly implemented in SQL generation expressions, including postcode whitespace removal and lowercase trigram normalization.
- [ ] Required `GIN` and `gin_trgm_ops` indexes exist for the generated full-text and trigram search columns.
- [x] The Aspire AppHost PostgreSQL environment supports the required full-text and `pg_trgm` extension setup.
- [x] The Testcontainer PostgreSQL environment supports the required full-text and `pg_trgm` extension setup.
- [x] A shared PostgreSQL bootstrap component owns `pg_trgm` creation and validation in both environments before `EnsureCreatedAsync`.
- [ ] The chosen text-search technology is documented with rationale.
- [x] The generated `search_vector` uses PostgreSQL's `simple` text search configuration.
- [x] Free-text discovery uses `plainto_tsquery` for the primary full-text query contract.
- [x] Fragment-oriented trigram discovery uses `word_similarity`-style matching consistently across implementation and tests.
- [x] Search returns ranked results using deterministic ordering.
- [x] Search returns `200 OK` with an empty `schools` collection when no schools match.
- [x] Blank `q` returns `400 Bad Request` when search behaviour is invoked.
- [x] Free-text search terms shorter than 4 characters return `400 Bad Request`.
- [ ] Free-text search continuation uses opaque encoded cursor values in the existing `cursor` and `nextCursor` fields.
- [ ] Invalid, undecodable, or incompatible search cursor values return `400 Bad Request`.
- [ ] Invalid `pageSize` values return `400 Bad Request`.
- [ ] Invalid URN input returns `400 Bad Request`.
- [ ] Unknown URN values return `404 Not Found`.
- [ ] Discovery responses use the full Milestone 1 school response shape.
- [ ] Discovery responses include canonical `id`.
- [ ] Discovery responses expose `urn` as the agreed integer HTTP contract field.
- [ ] The existing `GET /api/schools` contract remains part of the supported public API surface.
- [x] Generated OpenAPI output documents the collection route and discovery route with their expected parameters and error responses.
- [x] Integration tests cover successful and failing search and URN lookup behaviours.
- [ ] API documentation reflects the supported school discovery surface.
