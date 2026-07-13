# Initial API Roadmap for Summer-born Info

## Overview

This roadmap defines the remaining API capability needed to support the initial Summer-born Info experience and establish a stable contract for a follow-on UI project.

The primary delivery outcome for this roadmap is an API contract that is complete enough for UI development to begin with confidence. The roadmap focuses on the API surface, the supporting domain and persistence behaviour behind that contract, and the minimum operational safeguards needed for public submissions and volunteer administration.

## Problem Statement

The project needs a usable first-generation API that supports:

- secure administration by a small set of volunteer site managers,
- school discovery through search and location-based queries, and
- collection and moderation of parent-submitted CSA Application Reviews tied to specific schools.

Without these capabilities, the UI cannot be developed against a dependable contract and the platform cannot deliver its initial public value.

## Goals

- Deliver an API contract that is sufficient for UI development.
- Protect administrative actions so only authenticated admin users can perform them.
- Enable users to find schools through free-text search, exact URN lookup, and radius-based location queries.
- Support public submission of CSA Application Reviews for a specific school.
- Provide moderation and abuse controls for CSA Application Reviews that are appropriate for an initial public launch.

## Non-Goals

- UI implementation.
- Comment or review reply threads.
- School editing workflows beyond school data import.
- Non-admin user accounts or public sign-in.
- Advanced geospatial analysis beyond the initial radius-from-point search requirement.

## Stakeholders or Users

- Public users:
  Parents and guardians using the platform to research schools and record their experience with a CSA Application at a specific school.
- Admin users:
  A small group of volunteers responsible for school data import and review moderation.
- UI project team:
  The downstream consumer of the API contract once the contract is stable enough for interface implementation.

## High-Level Requirements

- Admin authentication and authorization:
  The API must support authenticated admin access and restrict privileged actions, including school import and CSA Application Review moderation, to authorized users only.
- Secure-by-default admin implementation:
  The final implementation approach is undecided, but the delivered solution must follow current security best practices and avoid introducing avoidable vulnerabilities in authentication, authorization, and input handling.
- School search:
  The API must support ranked free-text search across school name and address/postcode fields.
- School retrieval by key:
  The API must support exact-match retrieval by URN as a distinct lookup path.
- School location data:
  Schools must store location/spatial data needed to support map experiences and geographic querying.
- School location search:
  The API must support radius-from-point queries for nearby schools.
- CSA Application Reviews:
  The API must allow public users to create reviews associated with a specific school and a CSA Application experience.
- Review moderation and abuse controls:
  The API must support moderation, rate limiting, CAPTCHA or equivalent bot protection, and reporting/flagging workflows. An audit trail is not required in this initial roadmap.
- Contract readiness:
  The API should expose its contract clearly enough for the UI project to consume with minimal ambiguity, likely including stable endpoint shapes, request/response models, and validation behaviour.

## Milestones

### Milestone 1: Contract Baseline and Delivery Decisions

- Type:
  Discovery and decision
- Status:
  Complete on 2026-05-22
- Objective:
  Align the initial API contract scope, confirm resource shapes and endpoint boundaries, and resolve the main delivery choices that affect later work.
- Target window:
  Early roadmap phase
- Dependencies:
  Confirmed roadmap scope and agreement on API-first delivery
- Output:
  [Milestone 1 Contract Baseline and Delivery Decisions](C:\Projects\summer-born-info\Documentation\milestone-1-contract-baseline-and-delivery-decisions.md)
- Exit criteria:
  A documented contract baseline exists for admin access, school search, school lookup, location queries, and CSA Application Reviews; unresolved implementation choices are reduced to a manageable set of explicit decisions.

### Milestone 2: Admin Security for Protected Operations

- Type:
  Delivery
- Objective:
  Deliver authenticated admin access and enforce authorization for privileged operations used by volunteer administrators.
- Target window:
  Early to mid roadmap phase
- Dependencies:
  Milestone 1 decisions on contract and security direction
- Exit criteria:
  Protected endpoints require admin authentication, school import is restricted, CSA Application Review moderation is restricted, and the contract for auth-protected operations is stable for UI integration planning.

### Milestone 3: School Discovery and Lookup APIs

- Type:
  Delivery
- Objective:
  Deliver the core school search and lookup capabilities needed for user-facing discovery.
- Target window:
  Mid roadmap phase
- Dependencies:
  Milestone 1 contract baseline; school import data remains available as the source dataset
- Exit criteria:
  Ranked free-text search works across school name and address/postcode, exact URN lookup is available as a separate path, and the contract is ready for UI consumption.

### Milestone 4: Spatial School Search Support

- Type:
  Delivery
- Objective:
  Add the location model and query support required for map-based and proximity-based school discovery.
- Target window:
  Mid roadmap phase
- Dependencies:
  Milestone 3 school discovery foundation; storage approach for spatial data
- Exit criteria:
  Schools can store the required location data, radius-from-point queries are available through the API, and the returned contract is suitable for map-oriented UI work.

### Milestone 5: CSA Application Review Submission and Moderation

- Type:
  Delivery
- Objective:
  Deliver public review submission plus the abuse controls and moderation capabilities needed for safe initial operation.
- Target window:
  Late roadmap phase
- Dependencies:
  Milestone 2 admin security; Milestone 1 review contract decisions
- Exit criteria:
  Public users can submit CSA Application Reviews for specific schools, moderation endpoints and workflows exist for admins, and abuse controls cover rate limiting, bot protection, and reporting/flagging.

### Milestone 6: Contract Stabilization for UI Handoff

- Type:
  Validation and rollout
- Objective:
  Validate the overall API contract, close major gaps, and hand off a stable surface for UI delivery.
- Target window:
  Final roadmap phase
- Dependencies:
  Milestones 2 through 5
- Exit criteria:
  The initial API contract is complete for UI development, major validation and error-handling expectations are defined, and known gaps are either resolved or explicitly deferred.

## Dependencies

- Existing school import capability and source data quality, because school discovery and location support depend on reliable school records.
- Implementation of the chosen ASP.NET Core Identity approach for admin authentication and authorization, because protected workflows cannot be finalized without it.
- A storage and query approach that can support location data and radius-based search.
- A workable distribution approach for generated OpenAPI output so the UI project can consume the implemented contract.

## Risks and Mitigations

- Security implementation risk:
  The authentication direction is agreed, but the detailed ASP.NET Core Identity setup and admin bootstrap flow still need careful execution.
  Mitigation: validate the chosen implementation against established security practices before protected endpoints proliferate.
- Search relevance risk:
  Ranked free-text matching may require iteration to produce useful results for real school names and address data.
  Mitigation: define a minimal relevance target early and validate against representative imported data.
- Spatial data quality risk:
  Radius search quality depends on whether school location data is complete, accurate, and consistently stored.
  Mitigation: make location completeness and fallback handling explicit in the contract and verify import/data enrichment expectations early.
- Public submission abuse risk:
  Public review submission creates spam and misuse exposure from day one.
  Mitigation: treat moderation, rate limiting, bot protection, and reporting as core scope rather than hardening to be added later.
- Contract ambiguity risk:
  If the API contract is only partially defined, UI delivery may begin on assumptions that later change.
  Mitigation: treat contract stabilization as a discrete milestone with clear exit criteria before UI handoff.

## Open Questions

- What minimum school location source will be used if imported school data does not already provide a directly usable latitude/longitude representation?
- What user-visible behaviour should apply when reviews are flagged or moderated, for example immediate hiding versus queued review?

## Assumptions

- The initial release target is contract completeness for UI development rather than a broader production-complete platform roadmap.
- Public users will remain anonymous in the initial release and will not require sign-in.
- Only admin users need authenticated access in this roadmap.
- School search scope is intentionally limited to school name and address/postcode for ranked free-text matching, with URN handled separately as an exact lookup.
- Radius-from-point is the only required geospatial search mode for the first release.
- An audit trail for review moderation is not required in the initial release.

## Next Steps

- Use the Milestone 1 contract baseline as the reference input for Milestones 2 through 6 implementation planning and delivery.
- Identify the source and storage approach for school spatial data if the current import pipeline does not already provide the necessary shape.
- Break the roadmap milestones into implementation plans within the API solution once contract decisions are agreed.
