# Outstanding Full Text Search Questions

This document captures the remaining PostgreSQL full text search questions identified while reviewing [milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md).

These points should be resolved before Milestone 3 is treated as fully delivery-ready for the search foundation, search implementation, and ranked pagination work.

## 1. Ranked Search Cursor Contract

The current plan says Milestone 3 should preserve the existing `cursor` field names on the dedicated search route while also describing cursor validation in terms of a non-blank and decodable cursor.

That leaves an unresolved implementation question:

- Is the Milestone 3 `GET /api/schools/search` cursor still a plain school `Guid`, or does it become an opaque encoded cursor payload?

This matters because ranked full text search plus trigram ordering cannot reliably resume from `lastSchoolId` alone unless the plan also defines the exact resume algorithm and ordering tuple used to continue pagination.

The plan should explicitly decide one of the following:

- keep `cursor` on `GET /api/schools/search` as a plain `Guid` and document the exact server-side recomputation and boundary logic used to resume ranked search; or
- change `cursor` on `GET /api/schools/search` to an opaque encoded value and document its compatibility expectations and validation behaviour.

### Research Findings and Potential Solutions

Relevant external references used for this issue:

- PostgreSQL requires a unique `ORDER BY` for predictable paging results, especially with `LIMIT`: [PostgreSQL 18 docs, `LIMIT` and `OFFSET`](https://www.postgresql.org/docs/current/queries-limit.html)
- PostgreSQL full text search ranking is query-dependent rather than a stable stored value: [PostgreSQL 18 docs, text search ranking](https://www.postgresql.org/docs/18/textsearch-controls.html)
- `pg_trgm` similarity and word-similarity scores are also query-dependent and threshold-sensitive: [PostgreSQL 17 docs, `pg_trgm`](https://www.postgresql.org/docs/17/pgtrgm.html)
- Cursor/keyset pagination guidance generally treats continuation tokens as opaque server-owned values: [JSON:API cursor pagination profile](https://jsonapi.org/profiles/ethanresnick/cursor-pagination/), [Microsoft Data API Builder `$after`](https://learn.microsoft.com/en-us/azure/data-api-builder/keywords/after-rest), [GraphQL Cursor Connections spec](https://relay.dev/graphql/connections.htm)

#### Option A. Keep `cursor` as a plain school `Guid`

Implementation shape:

- Keep the public `cursor` query parameter and `nextCursor` response field on `GET /api/schools/search` as a raw school `Guid`.
- Define the ranked search sort order explicitly, for example:
  - full text rank descending;
  - trigram or word-similarity score descending;
  - normalized school name ascending;
  - `Id` ascending as the final unique tie-breaker.
- When resuming, treat the incoming `Guid` as the anchor row from the prior page, reload that school, recompute its ranking values for the active query text, and apply a strict boundary predicate using the full ordering tuple so the next page starts after that exact ranked row.

Benefits:

- Preserves the current externally visible cursor value format for existing clients.
- Minimizes immediate contract churn because callers can continue treating `cursor` as a `Guid`.
- Works if the plan fully defines the ranking tuple and the server always recomputes the same values from the same search input.

Limitations:

- The cursor is no longer self-sufficient. A `Guid` alone does not describe where the caller is in a ranked result set; the server must reload the anchor row and recompute ranking state on every continuation request.
- The approach becomes brittle when ranking inputs change between requests. If the indexed text, trigram thresholds, normalization rules, or ranking weights change, the anchor row may move within the ordering and pagination can skip or duplicate rows.
- If the anchor school stops matching the query between page requests, resume behaviour becomes awkward: the server must either fail the cursor, fall back to a best-effort resume, or accept possible instability.
- Future search variations such as alternate sort modes, boosted postcode ranking, or query-configuration changes remain hidden server assumptions because the cursor itself carries no version or sort metadata.
- Documentation and tests must lock down the exact resume algorithm, not just the fact that the cursor contains a `Guid`.

Assessment:

- This is viable, but only if backward compatibility with raw `Guid` cursors is a stronger requirement than simplicity or long-term search flexibility.

#### Option B. Change `cursor` to an opaque encoded payload

Implementation shape:

- Keep the public parameter name `cursor` on `GET /api/schools/search`, but change the value to a server-generated opaque token such as base64url-encoded JSON.
- Encode the minimum resume tuple needed for deterministic keyset pagination, for example:
  - cursor schema version;
  - anchor school `Id`;
  - the last row's full text rank;
  - the last row's trigram or word-similarity score;
  - any additional stable tie-break values used in ordering;
  - optionally a hash of the normalized query text or ranking mode so mismatched cursors can be rejected cleanly.
- Validate that the token is decodable, version-supported, and compatible with the active search request before applying the continuation boundary.

Benefits:

- Matches common cursor-pagination practice where the token is server-owned and opaque.
- Makes ranked keyset pagination explicit instead of relying on hidden server recomputation from a bare `Guid`.
- Supports deterministic resume behaviour even when ordering depends on multiple values rather than a single primary key.
- Gives the API a place to version cursor semantics, reject incompatible old tokens, and evolve search ordering without exposing internal details directly.
- Reduces ambiguity in the milestone plan because cursor validation can be defined in terms of decoding and schema checks rather than pretending the token is still just a `Guid`.

Limitations:

- It is a breaking contract change for any client that currently assumes `nextCursor` is always a raw `Guid` and perhaps stores or validates it as such.
- The server must choose a stable serialization format and be deliberate about floating-point precision if rank and similarity values are embedded directly.
- Opaque tokens are harder to inspect manually during debugging unless helper logging or decode tooling is added.
- If the project wants to preserve old raw-`Guid` cursors temporarily, the handler and validation rules become more complex during the transition.

Assessment:

- This is the cleanest fit for ranked full text search and is the lowest-risk option if Milestone 3 is allowed to evolve the cursor value format while keeping the `cursor` parameter name.

#### Option C. Use a dual-mode contract: raw `Guid` for plain listing, opaque cursor for ranked search

Implementation shape:

- Preserve the current raw-`Guid` cursor behaviour on `GET /api/schools`.
- Emit and accept opaque encoded cursors on `GET /api/schools/search`.
- Document cursor validation as route-specific: existing listing semantics on `GET /api/schools`, opaque token on `GET /api/schools/search`.

Benefits:

- Preserves existing behaviour for the current unfiltered list path.
- Avoids forcing a breaking cursor change onto consumers that only page through the non-search collection.
- Lets the ranked-search path adopt the more appropriate cursor design without rewriting the plain `Id`-ordered path.

Limitations:

- This option is now effectively replaced by route separation, so the main remaining drawback is migration churn if earlier notes assumed search lived on `GET /api/schools`.

Assessment:

- This is a reasonable compatibility bridge, but it adds behavioural complexity that the current plan is trying to remove rather than add.

#### Recommended Direction for Milestone 3

Recommended choice:

- Adopt Option B for `GET /api/schools/search` and update the plan to say that Milestone 3 preserves the `cursor` parameter name and collection wrapper shape on the search route, but not the current raw-`Guid` cursor value format there.

Why this is the best fit:

- Ranked full text search and trigram search produce a query-dependent ordering, so the continuation token should carry server-owned resume state rather than rely on a bare entity key.
- The existing plan already talks about decodable cursors, which aligns more naturally with an opaque encoded payload than with a plain `Guid`.
- This avoids hiding a complex recomputation algorithm behind a cursor format that no longer expresses the real continuation boundary.

Suggested plan wording change:

- Replace "preserve the existing `cursor` contract" with "preserve the existing `cursor` query parameter and `{ schools, nextCursor }` wrapper on `GET /api/schools/search` while changing the cursor value to a server-generated opaque token for ranked search pagination."
- Record that `GET /api/schools` keeps its existing collection pagination semantics while `GET /api/schools/search` adopts opaque ranked-search cursors.
- Add an explicit validation rule that invalid, undecodable, version-unsupported, or query-incompatible cursor tokens return `400 Bad Request`.

Related references:

- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L59)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L122)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L200)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L204)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L318)
- [API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Web\API\Schools\SchoolEndpoints.cs#L14)
- [API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/GetAllSchoolsQueryHandler.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Features\Schools\Queries\GetAllSchools\GetAllSchoolsQueryHandler.cs#L15)

## 2. Search Storage and Index Design

Status: resolved on 2026-05-27.

The plan selects PostgreSQL full text search plus `pg_trgm`, but it does not yet define the actual database artifacts needed to support that choice.

The unresolved question is:

- What concrete storage and indexing model should Milestone 3 create?

Examples of the missing decisions include:

- whether search uses a generated `tsvector` column or an expression-based search vector;
- whether normalization happens in SQL, in EF configuration, or at query time;
- which fields get trigram support;
- which indexes are required and what type they are.

This is especially important because `School` and `SchoolAddress` are both mapped to the same `school` table, so the implementation needs a clear decision about where the search vector and any supporting columns live.

The plan should explicitly name:

- the fields that feed the full text vector;
- the normalization rules applied before indexing;
- the fields that get trigram indexes;
- the specific index types to create.

### Research Findings and Potential Solutions

Relevant external references used for this issue:

- PostgreSQL supports either expression indexes or a separate stored generated `tsvector` column, and notes that the separate-column approach avoids repeating the text-search configuration in queries and avoids redoing `to_tsvector` to verify matches: [PostgreSQL 16 docs, text search tables and indexes](https://www.postgresql.org/docs/16/textsearch-tables.html)
- PostgreSQL generated columns are stored values computed on write and updated automatically when the row changes: [PostgreSQL 18 docs, generated columns](https://www.postgresql.org/docs/current/ddl-generated-columns.html)
- PostgreSQL `GIN` is the preferred full-text index type for `tsvector`: [PostgreSQL 18 docs, preferred index types for text search](https://www.postgresql.org/docs/current/textsearch-indexes.html)
- `pg_trgm` supports both `GIN` and `GiST` operator classes for similarity and fragment matching, with `gin_trgm_ops` being a straightforward fit for fast indexed similarity filtering: [PostgreSQL 17 docs, `pg_trgm`](https://www.postgresql.org/docs/17/pgtrgm.html)
- Npgsql supports both generated `tsvector` columns and PostgreSQL-specific index methods and operator classes in EF configuration: [Npgsql full text search docs](https://www.npgsql.org/efcore/mapping/full-text-search.html), [Npgsql index modeling docs](https://www.npgsql.org/efcore/modeling/indexes.html)

#### Option 1. Stored generated search columns on `school`

Implementation shape:

- Add all Milestone 3 search artifacts to the existing shared `school` table because both `School` and `SchoolAddress` already map there.
- Create a stored generated `search_vector` `tsvector` column that combines:
  - `Name` with the highest full-text weight;
  - `Town` and `PostCode` with a medium weight;
  - `Street`, `Locality`, `AddressThree`, and `County` with a lower weight.
- Create stored generated normalized text columns for trigram search:
  - `search_name_normalized`;
  - `search_postcode_normalized`;
  - `search_address_normalized` if representative search tests show combined-address fragment search needs dedicated support.
- Apply normalization in SQL generation expressions so indexed values are deterministic:
  - `coalesce(..., '')` for nullable text;
  - lowercase normalization for trigram-targeted text;
  - postcode normalization by lowercasing and removing whitespace before indexing and comparison.
- Create the baseline indexes:
  - `GIN` on `search_vector`;
  - `GIN` with `gin_trgm_ops` on `search_name_normalized`;
  - `GIN` with `gin_trgm_ops` on `search_postcode_normalized`;
  - `GIN` with `gin_trgm_ops` on `search_address_normalized` if that column is included.

Benefits:

- Best fit for the stated Milestone 3 priority of search-read performance.
- Keeps query logic simpler and more predictable because search expressions are materialized once on write rather than reconstructed on every query.
- Matches PostgreSQL guidance that the separate-column approach avoids repeating text-search configuration in queries and avoids redoing `to_tsvector` for match verification.
- Works cleanly with the existing shared-table mapping because the search artifacts live exactly where the searchable fields already live.
- Gives the plan a precise, testable definition of normalization, weighting, and required indexes.

Limitations:

- Adds extra stored columns and indexes to the `school` table, increasing schema surface area.
- Increases insert and update cost because generated values and indexes must be maintained whenever a searchable field changes.
- Requires deliberate EF mapping decisions for generated columns that are persistence details rather than domain concepts.

Assessment:

- This is the recommended option and should be treated as the Milestone 3 requirement because the user has chosen search performance over minimal schema churn.

#### Option 2. Expression indexes only

Implementation shape:

- Keep the schema lean and create indexes directly on `to_tsvector(...)` and normalized text expressions instead of storing generated helper columns.

Benefits:

- Fewer new columns to add and map.
- Lower visible schema churn in the `school` table.

Limitations:

- Weaker fit for a performance-first search requirement.
- More fragile because the query shape and configuration must stay aligned with the expression index definition.
- PostgreSQL documentation notes practical advantages for the separate-column approach in both query simplicity and search execution.

Assessment:

- This remains viable, but it is no longer the right choice for Milestone 3 given the explicit performance requirement.

#### Recommended Direction for Milestone 3

Recommended choice:

- Adopt Option 1 and update the milestone plan to require stored generated search columns on `school`, led by a weighted `search_vector` and normalized trigram helper columns.

Suggested plan wording change:

- Replace the open storage-design wording with an explicit requirement that Milestone 3 uses stored generated search columns on `school` for best search-read performance.
- Record the exact full-text fields, normalization rules, trigram-targeted fields, and required `GIN` / `gin_trgm_ops` indexes in the milestone plan.

Resolution status:

- Resolved on 2026-05-27: Milestone 3 should optimize for best search-read performance and therefore use Option 1, the stored generated search-column model.

Related references:

- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L167)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L168)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L183)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L247)
- [API/SummerBornInfo.Infrastructure/Persistence/Configuration/SchoolConfiguration.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Infrastructure\Persistence\Configuration\SchoolConfiguration.cs#L7)
- [API/SummerBornInfo.Infrastructure/Persistence/Configuration/AddressConfiguration.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Infrastructure\Persistence\Configuration\AddressConfiguration.cs#L7)

## 3. Full Text Query Semantics

Status: resolved on 2026-05-27.

The current plan does not yet define the precise query semantics for the PostgreSQL full text search implementation.

The unresolved questions include:

- Which text search configuration should be used, such as `simple` or an English configuration?
- Which tsquery constructor should be used, such as `plainto_tsquery` or `websearch_to_tsquery`?
- How should postcodes be normalized before search and comparison?
- What trigram threshold or similarity rules should apply?
- How should very short inputs be handled when trigram matching is noisy or ineffective?

These choices are important because they directly affect relevance, edge-case behaviour, and the stability of integration tests built around realistic school discovery cases.

The plan should lock down enough of these semantics that two engineers would not produce meaningfully different search behaviour while both claiming to follow the plan.

### Research Findings and Potential Solutions

Relevant external references used for this issue:

- PostgreSQL text-search dictionaries and configurations, including `simple`, stemming dictionaries, and stop-word behaviour: [PostgreSQL 18 docs, text search dictionaries](https://www.postgresql.org/docs/current/textsearch-dictionaries.html)
- PostgreSQL tsquery constructors, including `plainto_tsquery` and `websearch_to_tsquery`: [PostgreSQL 14 docs, text search controls](https://www.postgresql.org/docs/14/textsearch-controls.html)
- PostgreSQL trigram similarity operators, thresholds, and short-pattern caveats: [PostgreSQL 17 docs, `pg_trgm`](https://www.postgresql.org/docs/17/pgtrgm.html)
- Npgsql support for PostgreSQL full-text search configuration and tsquery functions in EF Core: [Npgsql full text search docs](https://www.npgsql.org/efcore/mapping/full-text-search.html)

#### Question A. Which text search configuration should Milestone 3 use?

##### Option A1. Use `pg_catalog.simple`

Implementation shape:

- Build the generated `search_vector` with PostgreSQL's `simple` text search configuration.
- Keep search lexemes close to their lowercased input tokens rather than applying English stemming rules.

Benefits:

- Preserves original tokens more faithfully, which is attractive for school names, abbreviations, and place names that should not be stemmed aggressively.
- Reduces the risk of surprising linguistic transformations when users search for proper nouns or domain-specific terms.
- Keeps integration-test expectations simpler because token matching stays closer to literal normalized words.

Limitations:

- Does not provide stemming, so singular or plural and other inflected variants do not automatically collapse together.
- Can be less forgiving than an English configuration for natural-language queries.
- Still depends on explicit stop-word decisions if the implementation introduces them later.

Assessment:

- This is the safer choice if Milestone 3 should optimize for recognisable school and place-name matching over broader natural-language semantics.

##### Option A2. Use `pg_catalog.english`

Implementation shape:

- Build the generated `search_vector` with PostgreSQL's English text search configuration.
- Accept stemming and stop-word handling as part of the search contract.

Benefits:

- Supports broader natural-language matching because related English word forms can reduce to the same stem.
- Better fits user-entered descriptive search phrases when meaningful English stemming improves recall.
- Maps cleanly onto Npgsql's generated-column and tsquery support.

Limitations:

- Can produce less intuitive behaviour for school names, locality names, and abbreviations that are not ordinary prose.
- Stop-word handling and stemming can make ranking and test expectations harder to reason about.
- May introduce "helpful but surprising" matches or misses when proper nouns are transformed.

Assessment:

- This is viable if Milestone 3 wants broader English-language recall, but it carries more relevance risk for name-heavy datasets.

##### Option A3. Use a custom configuration

Implementation shape:

- Define a PostgreSQL text search configuration that places a domain-specific dictionary or synonym layer ahead of more general stemming behaviour.
- Use that custom configuration in the generated `search_vector`.

Benefits:

- Offers the best long-term fit if the project needs domain-specific handling for abbreviations, school terms, or place-name quirks.
- Allows future synonym handling or protection of known tokens before broader stemming runs.
- Can preserve important search vocabulary while still benefiting from more general dictionaries later in the pipeline.

Limitations:

- Adds operational and bootstrap complexity that Milestone 3 may not want.
- Requires more environment setup and parity work across AppHost and Testcontainers.
- Is harder to justify before representative search failures show the need for it.

Assessment:

- This is probably beyond Milestone 3 unless research cases prove that both `simple` and `english` are materially inadequate.

#### Question B. Which tsquery constructor should Milestone 3 use?

##### Option B1. Use `plainto_tsquery`

Implementation shape:

- Parse the incoming search text with `plainto_tsquery`.
- Treat surviving terms as an implicit `AND` query for full-text matching.

Benefits:

- Gives the simplest and most predictable contract for a public search endpoint.
- Avoids exposing special search syntax that the current plan does not mention.
- Is straightforward to document, test, and implement through Npgsql.

Limitations:

- Does not support quoted phrases, explicit `OR`, or exclusion operators.
- Can feel stricter than users expect from a general search box.
- Relies more heavily on trigram support to rescue imperfect or partial input.

Assessment:

- This is the cleanest baseline if Milestone 3 wants a narrow, easy-to-test discovery contract.

##### Option B2. Use `websearch_to_tsquery`

Implementation shape:

- Parse incoming search text with `websearch_to_tsquery`.
- Support familiar web-style operators such as quoted phrases, `OR`, and `-` exclusion.

Benefits:

- Better matches user expectations for a free-text search box.
- Accepts raw input safely; PostgreSQL documents that it does not raise syntax errors.
- Enables richer search behaviour without requiring callers to learn native tsquery syntax.

Limitations:

- Broadens the public contract because operators such as phrases, `OR`, and exclusion become meaningful behaviours to support.
- Requires additional OpenAPI notes and more extensive integration-test coverage.
- Makes it easier for two implementations to diverge subtly unless the plan documents examples clearly.

Assessment:

- This is the better fit only if Milestone 3 intentionally wants search-box semantics rather than a simpler token-match contract.

##### Option B3. Hybrid contract: `plainto_tsquery` for full-text matching plus trigram rescue

Implementation shape:

- Use `plainto_tsquery` as the primary full-text query constructor.
- Rely on `pg_trgm` similarity rules to recover partial, misspelled, or fragment-based queries that the strict full-text clause would miss.

Benefits:

- Keeps the full-text contract simple while still supporting forgiving discovery quality.
- Fits the current milestone direction of combining PostgreSQL full text search with `pg_trgm`.
- Is easier to explain than a richer operator grammar while still handling realistic user mistakes.

Limitations:

- Still requires the plan to define when trigram rescue applies and how it influences inclusion and ranking.
- Does not support phrase or explicit `OR` search unless the contract grows later.

Assessment:

- This is a strong fit if Milestone 3 wants a delivery-ready contract without committing to advanced search syntax.

#### Question C. What trigram threshold or similarity rules should Milestone 3 use?

##### Option C1. Use the default `%` similarity operator and default threshold

Implementation shape:

- Use PostgreSQL trigram similarity with the `%` operator.
- Rely on the default `pg_trgm.similarity_threshold` unless implementation evidence justifies changing it.

Benefits:

- Simplest to implement and explain.
- Avoids introducing another tuning parameter into the milestone plan.
- Uses standard PostgreSQL behaviour with minimal ceremony.

Limitations:

- Leaves a material relevance decision implicit unless the plan records the default threshold value explicitly.
- Whole-string similarity is not always the best fit for long school names or address fragments.
- May produce weaker fragment behaviour than the milestone expects.

Assessment:

- This is acceptable as a baseline only if the milestone is comfortable with standard PostgreSQL similarity semantics and records the default threshold explicitly.

##### Option C2. Use `word_similarity`-style matching for fragments

Implementation shape:

- Base trigram matching on `word_similarity` or related operators such as `<%` for fragment-oriented discovery.
- Tune inclusion and ranking around the best matching continuous extent inside the stored text.

Benefits:

- Better aligned with address fragments and partial school-name searches than whole-string similarity.
- Fits realistic discovery cases where the query is only one part of a longer school or address value.
- Gives the plan a more defensible story for fragment matching than a generic `%` rule.

Limitations:

- Still requires the plan to choose an explicit threshold.
- Adds more complexity to ranking and test expectations than a single default `%` rule.
- Needs clear route-level documentation so implementers use the same operator family consistently.

Assessment:

- This is likely the strongest candidate if Milestone 3 cares materially about fragment matching quality.

##### Option C3. Use `strict_word_similarity` for tighter word-boundary matching

Implementation shape:

- Base trigram support on `strict_word_similarity` or related operators such as `<<%`.
- Favor whole-word boundary matches over looser mid-word fragments.

Benefits:

- Reduces noisy partial hits compared with looser trigram similarity.
- May suit school-name token matching when typo tolerance is needed but arbitrary substrings are too permissive.
- Can improve precision when fragment overmatching is a concern.

Limitations:

- Is probably too strict for postcode or address-fragment search.
- Still requires threshold selection and explicit ranking guidance.
- Risks missing valid partial-input behaviours that the milestone currently wants to support.

Assessment:

- This is better as a precision-oriented fallback than as the default cross-field trigram rule for Milestone 3.

#### Question D. How should very short inputs be handled?

##### Option D1. Reject very short free-text queries with `400 Bad Request`

Implementation shape:

- Define a minimum `q` length for free-text search.
- Reject shorter values before attempting full-text or trigram matching.

Benefits:

- Gives the strongest protection against noisy fuzzy matching and weak trigram behaviour.
- Makes the API contract explicit and easy to test.
- Reduces the risk of broad or expensive low-signal search execution.

Limitations:

- Can be unfriendly for legitimate short school names or postcode fragments.
- Adds a new validation rule that is not currently part of the Milestone 1 baseline.
- Requires careful communication so clients understand why some apparently valid inputs are refused.

Assessment:

- This is the safest operational choice, but it is also the most user-visible contract change.

##### Option D2. Allow short inputs but disable trigram rescue below a threshold

Implementation shape:

- Accept short `q` values.
- Apply stricter matching for short inputs by skipping fuzzy trigram rescue and relying on exact, prefix, postcode-normalized, or full-text logic only.

Benefits:

- Preserves permissive input acceptance for legitimate short queries.
- Avoids the noisiest fuzzy-matching cases without requiring the route to reject input outright.
- Gives the milestone a clearer answer to the short-input problem while staying close to current route expectations.

Limitations:

- Requires explicit rules for which matching paths remain enabled for short inputs.
- Can produce edge cases where longer misspelled input works but short misspelled input does not.
- Needs careful tests to avoid accidental behaviour drift.

Assessment:

- This is a balanced option if Milestone 3 wants to remain input-friendly while still containing trigram noise.

##### Option D3. Allow short inputs with stricter trigram thresholds

Implementation shape:

- Accept short `q` values.
- Keep trigram matching enabled, but use stricter thresholds or a stricter trigram operator family for very short inputs.

Benefits:

- Most permissive user experience because short inputs still participate in fuzzy matching.
- Could preserve useful postcode-fragment or short-name behaviour if tuned carefully.

Limitations:

- Hardest option to make delivery-ready because thresholds and fallback behaviour become highly implementation-sensitive.
- Highest risk of unintuitive relevance and test instability.
- Requires representative evidence to justify the chosen thresholds.

Assessment:

- This is feasible, but it pushes too much tuning complexity into Milestone 3 unless the team is prepared to spend time calibrating it.

#### Recommended Direction for Milestone 3

Recommended choice:

- Adopt `pg_catalog.simple` for the generated full-text search vector.
- Adopt `plainto_tsquery` for the primary full-text query constructor.
- Adopt `word_similarity`-style trigram matching for fragment-oriented discovery support.
- Reject free-text search terms shorter than 4 characters with `400 Bad Request`.

Plan impact:

- Point 2's storage and normalization findings close the postcode-normalization part of this issue.
- The milestone plan should now record these semantics explicitly in the search deliverables, validation rules, behaviour scenarios, and completion checklist so implementation and test behaviour stay aligned.

Resolution status:

- Resolved on 2026-05-27: Milestone 3 should use PostgreSQL's `simple` configuration, `plainto_tsquery`, `word_similarity`-style trigram matching, and a minimum free-text search length of 4 characters.

Related references:

- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L88)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L184)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L224)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L247)

## 4. Extension and Environment Bootstrap Ownership

Status: resolved on 2026-05-27.

The plan says the Aspire AppHost and Testcontainer environments must support the required PostgreSQL extensions, but it does not yet assign clear ownership for creating and validating them.

The unresolved question is:

- Which startup or provisioning path is responsible for creating `pg_trgm` and any related search prerequisites in each environment?

This is a delivery risk because the application and tests currently rely on `EnsureCreated`, and the plan does not yet state:

- whether extension creation happens before `EnsureCreated`;
- whether it happens through raw SQL, EF model configuration, container initialization, or another bootstrap step;
- how parity between local development and integration tests is verified.

The plan should specify the exact bootstrap path for:

- local Aspire-backed development;
- integration tests using the PostgreSQL Testcontainer;
- any validation step that proves required extensions exist before search tests run.

### Research Findings and Potential Solutions

Current implementation context in this repository:

- [API/SummerBornInfo.Web/Program.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Web\Program.cs#L45) currently calls `EnsureCreatedAsync` during development startup, so any required PostgreSQL extensions must exist before the application relies on generated columns, trigram operators, or related search artifacts.
- [API/SummerBornInfo.IntegrationTests/IntegrationTestDatabaseServerFixture.cs](C:\Projects\summer-born-info\API\SummerBornInfo.IntegrationTests\IntegrationTestDatabaseServerFixture.cs#L21) currently creates the template database through application code and also relies on `EnsureCreatedAsync`, which means the test path has the same ordering requirement.
- [API/SummerBornInfo.AppHost/AppHost.cs](C:\Projects\summer-born-info\API\SummerBornInfo.AppHost\AppHost.cs#L5) provisions the PostgreSQL container, but it does not currently own schema or extension initialization logic beyond standing up the server.

#### Option 1. Shared application/test bootstrap step

Implementation shape:

- Add a shared PostgreSQL bootstrap component in the API infrastructure layer that is responsible for ensuring required extensions exist before schema creation depends on them.
- Have that component connect to the target database and run idempotent SQL such as `CREATE EXTENSION IF NOT EXISTS pg_trgm`.
- Invoke the component from both:
  - local development startup in `Program.cs` before `EnsureCreatedAsync`;
  - the integration-test database fixture before `EnsureCreatedAsync` creates the template database schema.
- Add an explicit validation query after bootstrap, such as checking `pg_extension`, so startup and tests fail early if the extension was not created successfully.

Benefits:

- Fits the current repository shape because both local development and integration tests already perform database bootstrap in application code.
- Creates a single, reusable ownership point for extension initialization and validation instead of splitting the logic across container setup scripts.
- Keeps local and test environment behaviour aligned because both paths call the same bootstrap code in the same order.
- Makes the milestone's ordering requirement explicit: extension bootstrap first, then `EnsureCreated`, then any queue or seed initialization.

Limitations:

- Application and test startup now own part of database infrastructure provisioning, which some teams prefer to keep outside runtime code.
- The bootstrap connection still requires sufficient privileges to create extensions in the target database.
- The shared bootstrap code must be careful to run against the intended database in the test template-database flow.

Assessment:

- This is the best fit for Milestone 3 because it solves the immediate delivery risk without forcing a broader change away from the current `EnsureCreated`-based setup.

#### Option 2. Container-owned initialization

Implementation shape:

- Configure the Aspire PostgreSQL container and the PostgreSQL Testcontainer to run initialization SQL that creates `pg_trgm` before the application connects.
- Keep application startup focused on `EnsureCreatedAsync` and higher-level schema or queue setup.

Benefits:

- Keeps infrastructure provisioning separated from application startup logic.
- Makes extension creation independent of application bootstrap ordering once the container is running.

Limitations:

- Requires parallel container initialization behaviour in Aspire and Testcontainers, which is more work to keep aligned.
- Pushes an important Milestone 3 dependency into environment-specific setup rather than the shared application/test bootstrap path.
- Makes failures less obvious to contributors because the extension bootstrap is no longer visible in the code path that creates the database schema.

Assessment:

- Viable, but weaker than Option 1 for this repository because the current bootstrap responsibility already lives in startup code rather than container init assets.

#### Option 3. Migrations-driven extension provisioning

Implementation shape:

- Move Milestone 3 search prerequisites into migrations or another ordered database deployment path.
- Create required extensions and search artifacts as part of that migration flow instead of the current `EnsureCreated` path.

Benefits:

- Strong long-term database ownership model.
- Aligns extension creation with other schema changes such as generated columns and indexes.

Limitations:

- Requires a broader provisioning-model change than Milestone 3 currently needs.
- Does not fit the current local development and integration-test bootstrap model without additional refactoring.

Assessment:

- Better as a future platform change than as the immediate Milestone 3 answer.

#### Recommended Direction for Milestone 3

Recommended choice:

- Adopt Option 1 and make a shared application/test bootstrap step the owner of PostgreSQL extension creation and validation.

Decision details to record in the milestone plan:

- Local Aspire-backed development:
  - development startup must invoke the shared PostgreSQL bootstrapper before `EnsureCreatedAsync`;
  - the bootstrapper must create `pg_trgm` with idempotent SQL and validate that it exists before search-dependent schema creation continues.
- Integration tests using the PostgreSQL Testcontainer:
  - the integration-test fixture must invoke the same shared PostgreSQL bootstrapper before `EnsureCreatedAsync` creates the template database schema;
  - extension validation must happen in the fixture so search tests fail fast if the environment is missing prerequisites.
- Parity verification:
  - both startup paths must call the same bootstrap code rather than duplicate raw SQL in separate files;
  - automated integration coverage should exercise at least one search path that depends on `pg_trgm`, proving the required extension exists before search tests run.

Suggested plan wording change:

- Replace general statements about AppHost and Testcontainer support with an explicit requirement that a shared PostgreSQL bootstrap component owns `pg_trgm` creation and validation in both local development startup and the integration-test fixture, and that this bootstrap runs before `EnsureCreatedAsync`.

Resolution status:

- Resolved on 2026-05-27: Milestone 3 will use a shared application/test PostgreSQL bootstrap step to create and validate `pg_trgm` before `EnsureCreatedAsync` in both local Aspire-backed development and the Testcontainer integration-test flow.

Related references:

- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L169)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L170)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L249)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L294)
- [API/SummerBornInfo.Web/Program.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Web\Program.cs#L45)
- [API/SummerBornInfo.AppHost/AppHost.cs](C:\Projects\summer-born-info\API\SummerBornInfo.AppHost\AppHost.cs#L5)
- [API/SummerBornInfo.IntegrationTests/IntegrationTestDatabaseServerFixture.cs](C:\Projects\summer-born-info\API\SummerBornInfo.IntegrationTests\IntegrationTestDatabaseServerFixture.cs#L21)

## Summary

Milestone 3 still needs explicit answers for:

- the search cursor format and ranked resume strategy;

Issue 2 is now resolved: Milestone 3 will optimize for best search-read performance by using stored generated search columns on `school`, including a weighted `search_vector` and normalized trigram helper columns with explicit `GIN` and `gin_trgm_ops` indexes.
Issue 3 is now resolved: Milestone 3 will use PostgreSQL's `simple` text search configuration, `plainto_tsquery`, `word_similarity`-style trigram matching, and reject free-text search terms shorter than 4 characters.
Issue 4 is now resolved: Milestone 3 will use a shared application/test PostgreSQL bootstrap step to create and validate `pg_trgm` before `EnsureCreatedAsync` in both local Aspire-backed development and the Testcontainer integration-test flow.

Until those remaining decisions are recorded, the search-related tasks in the milestone plan still require some implementation-time design choices rather than straightforward execution.
