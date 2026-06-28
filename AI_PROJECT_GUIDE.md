# AI Project Guide

This file is the canonical top-level map for the repository. Use it to decide which service owns a change before switching to service-specific guidance.

## Repository shape

- `API/` contains the .NET backend solution, supporting infrastructure projects, and API-focused tests.
- `UI/` contains the Angular frontend application and UI-specific tooling.
- `Plans/` contains delivery-ready implementation plans.
- `Roadmap/` contains higher-level roadmap and milestone documentation.
- Root docs (`README.md`, `AGENTS.md`, this file) stay short and route work to the correct child surface.

## Ownership boundaries

- Put backend HTTP, domain, persistence, queues, storage, and Aspire/local infrastructure work in `API/`.
- Put frontend views, routing, client-side state, styling, and browser-facing behaviour in `UI/`.
- Put delivery planning updates in `Plans/`.
- Put initiative, milestone, and discovery planning in `Roadmap/`.

## Service-specific guides

- API workflow and delivery rules: [API/AGENTS.md](./API/AGENTS.md)
- API structure and conventions: [API/AI_PROJECT_GUIDE.md](./API/AI_PROJECT_GUIDE.md)
- API setup and run/test guidance: [API/README.md](./API/README.md)
- UI workflow and coding guidance: [UI/AGENTS.md](./UI/AGENTS.md)
- UI structure and conventions: [UI/AI_PROJECT_GUIDE.md](./UI/AI_PROJECT_GUIDE.md)
- UI setup and run/test guidance: [UI/README.md](./UI/README.md)

## Routing rule

Start at this file for repository boundaries, then move into the child guide for the service you are changing. Do not treat root documentation as the place for API-internal or UI-internal implementation detail.
