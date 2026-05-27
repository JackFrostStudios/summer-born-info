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

Related references:

- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L167)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L168)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L183)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L247)
- [API/SummerBornInfo.Infrastructure/Persistence/Configuration/SchoolConfiguration.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Infrastructure\Persistence\Configuration\SchoolConfiguration.cs#L7)
- [API/SummerBornInfo.Infrastructure/Persistence/Configuration/AddressConfiguration.cs](C:\Projects\summer-born-info\API\SummerBornInfo.Infrastructure\Persistence\Configuration\AddressConfiguration.cs#L7)

## 3. Full Text Query Semantics

The current plan does not yet define the precise query semantics for the PostgreSQL full text search implementation.

The unresolved questions include:

- Which text search configuration should be used, such as `simple` or an English configuration?
- Which tsquery constructor should be used, such as `plainto_tsquery` or `websearch_to_tsquery`?
- How should postcodes be normalized before search and comparison?
- What trigram threshold or similarity rules should apply?
- How should very short inputs be handled when trigram matching is noisy or ineffective?

These choices are important because they directly affect relevance, edge-case behaviour, and the stability of integration tests built around realistic school discovery cases.

The plan should lock down enough of these semantics that two engineers would not produce meaningfully different search behaviour while both claiming to follow the plan.

Related references:

- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L88)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L184)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L224)
- [Plans/milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md#L247)

## 4. Extension and Environment Bootstrap Ownership

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
- the database storage and index design for full text and trigram support;
- the exact full text and trigram query semantics;
- the environment bootstrap path that creates and verifies required PostgreSQL extensions.

Until those decisions are recorded, the search-related tasks in the milestone plan still require implementation-time design choices rather than straightforward execution.
