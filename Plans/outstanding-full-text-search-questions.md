# Outstanding Full Text Search Questions

This document captures the remaining PostgreSQL full text search questions identified while reviewing [milestone-3-school-discovery-and-lookup-apis.md](C:\Projects\summer-born-info\Plans\milestone-3-school-discovery-and-lookup-apis.md).

These points should be resolved before Milestone 3 is treated as fully delivery-ready for the search foundation, search implementation, and ranked pagination work.

## 1. Ranked Search Cursor Contract

The current plan says Milestone 3 should preserve the existing `cursor` contract while also describing cursor validation in terms of a non-blank and decodable cursor.

That leaves an unresolved implementation question:

- Is the Milestone 3 search cursor still a plain school `Guid`, or does it become an opaque encoded cursor payload?

This matters because ranked full text search plus trigram ordering cannot reliably resume from `lastSchoolId` alone unless the plan also defines the exact resume algorithm and ordering tuple used to continue pagination.

The plan should explicitly decide one of the following:

- keep `cursor` as a plain `Guid` and document the exact server-side recomputation and boundary logic used to resume ranked search; or
- change `cursor` to an opaque encoded value and document its compatibility expectations and validation behaviour.

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
