# Dual API and UI Guidance Alignment Plan

## 1. Overview

Prepare the repository for a dual-service shape where contributors and AI agents can clearly distinguish between API-specific and UI-specific setup, workflows, and conventions.

The outcome is a lean root-level entry point that routes people to focused child documentation, plus updated agent guidance and project-local skills that no longer assume the repository is API-only.

## 2. Roadmap Source or Existing Plan Context

- Source context: direct user request to prepare the solution for both API and UI service work.
- Current repository state:
  - `README.md` points only to `API/README.md`.
  - `API/README.md` is project-specific and already usable.
  - `UI/README.md` is still the default Angular CLI scaffold.
  - Root `AGENTS.md` routes only to API, Plans, and Roadmap guidance.
  - `UI/AGENTS.md` exists, but it is framework guidance rather than repository workflow guidance.
  - Project-local skills such as `architect` and `automated-test-developer` still assume API-only structure and test patterns.
- Relevant existing constraints:
  - Plans should stay in `Plans/` and follow the implementation-planner structure.
  - API structural guidance currently lives in `API/AI_PROJECT_GUIDE.md`.

## 3. Scope

- Update repository documentation so the root README acts as a concise index for API and UI contributors.
- Replace generic or API-only agent guidance with repo-aware routing for API and UI work.
- Align project-local skills with the new dual-service repository shape.
- Introduce any missing child-level guidance files needed to keep root guidance lean and focused.
- Document current setup and run/test processes for the generated Angular UI without inventing workflows that do not yet exist.

## 4. Non-Goals

- Implementing UI features beyond documentation and guidance.
- Changing API or UI runtime behavior.
- Adding combined root-level build, run, or orchestration scripts unless they already exist and only need documenting.
- Defining the full long-term frontend architecture beyond what is needed for contributor guidance.
- Unifying API and UI into a single startup flow before the underlying implementation exists.

## 5. Behaviour Scenarios

### Scenario: New contributor lands in the repository root

Given a contributor opens the repository for the first time  
When they read the root `README.md`  
Then they should immediately see:
- that the repository contains separate API and UI projects,
- which folder to enter for each type of work,
- where to find setup, run, test, and architecture guidance for each surface,
- and any current limitations about running them together.

### Scenario: AI agent receives a task in the UI folder

Given an AI agent is asked to make UI changes  
When it reads the repository guidance  
Then it should be routed to `UI/AGENTS.md` and any UI project guide instead of defaulting to API-specific rules or .NET assumptions.

### Scenario: AI agent receives a task in the API folder

Given an AI agent is asked to make API changes  
When it reads the repository guidance  
Then it should still follow the existing API delivery rules, `API/AGENTS.md`, and `API/AI_PROJECT_GUIDE.md` without regression.

### Scenario: Contributor needs the right local setup steps

Given a contributor wants to run or test one service  
When they open the child README for that service  
Then they should find only the commands, prerequisites, and notes relevant to that service, rather than a mixed set of instructions for the whole repository.

### Scenario: Repo-specific skill guidance is used

Given a project-local skill is invoked for architecture or test work  
When the skill describes repository structure or test strategy  
Then it should distinguish API and UI paths, conventions, and verification approaches instead of assuming everything belongs to the API solution.

## 6. Deliverables

1. Root documentation refresh
   - Update `README.md` to describe the repository as a dual API/UI solution.
   - Keep the root README intentionally short.
   - Add clear links to `API/README.md` and `UI/README.md`.
   - Add a compact repository map and a "where to go next" section for common tasks.

2. Root agent routing update
   - Update `AGENTS.md` to include UI guidance alongside API, Plans, and Roadmap guidance.
   - Make the routing explicit so agents choose the correct child instructions before acting.

3. UI contributor documentation
   - Replace the generated Angular boilerplate in `UI/README.md` with project-specific guidance.
   - Cover prerequisites, install, run, build, and test commands that match the current Angular app.
   - Explain the current relationship to the API, including any assumptions or gaps in local integration.
   - Link to deeper UI guidance files rather than overloading the README.

4. UI agent workflow guidance
   - Rewrite `UI/AGENTS.md` so it includes repository workflow expectations, not just framework best practices.
   - Preserve useful Angular coding guidance, but add delivery rules similar in spirit to `API/AGENTS.md`:
     - follow plan-driven work,
     - update plans when completion state changes,
     - test appropriately for UI changes,
     - keep project-structure guidance in a canonical child document.

5. Canonical project guide alignment
   - Create a root `AI_PROJECT_GUIDE.md` that explains the top-level repository shape and points to service-specific guides.
   - Keep `API/AI_PROJECT_GUIDE.md` focused on API internals.
   - Create `UI/AI_PROJECT_GUIDE.md` for UI structure, conventions, and testing/layout expectations if no equivalent file exists.
   - Ensure agent and skill references point to the correct guide level.

6. Project-local skill updates
   - Update `.codex/skills/architect/SKILL.md` so it no longer assumes every task belongs in the API vertical-slice architecture.
   - Update `.codex/skills/automated-test-developer/SKILL.md` so it distinguishes API testing patterns from UI testing patterns and corrects stale API path references where needed.
   - Review any other project-local skills that reference API-only structure and adjust only those that materially affect guidance for this repository shape.

7. Cross-link consistency pass
   - Verify that root docs, child docs, agent instructions, and skills all point to the same canonical files.
   - Remove outdated wording that implies the repository is API-only.

## 7. Technology Requirements and Decisions

- Decision: use a layered documentation model.
  - Root files stay short and route users to service-specific docs.
  - Child files hold setup and workflow detail.
  - Rationale: this matches the user goal of keeping instructions lean and easy to navigate.

- Decision: treat API and UI as separate contributor surfaces.
  - API guidance remains .NET/Aspire/Testcontainers oriented.
  - UI guidance becomes Angular/npm/Vitest oriented.
  - Rationale: setup, tooling, and delivery expectations differ materially between the two projects.

- Decision: introduce a root `AI_PROJECT_GUIDE.md`.
  - Rationale: project-local skills already need a canonical repository map, and current skill references imply one should exist.

- Decision: document only workflows that exist today.
  - Current UI commands should come from the generated Angular app configuration.
  - Do not document a combined API+UI startup story unless it is already implemented.

## 8. Dependencies and Sequencing

1. Establish canonical structure
   - Define root vs child ownership for README, AGENTS, and project guide files.

2. Update repository routing
   - Refresh `README.md` and `AGENTS.md` so contributors reach the right child docs first.

3. Create or update child guidance
   - Finalize `UI/README.md` and `UI/AGENTS.md`.
   - Add `UI/AI_PROJECT_GUIDE.md` and root `AI_PROJECT_GUIDE.md`.
   - Adjust `API/README.md` or `API/AI_PROJECT_GUIDE.md` only where cross-links or boundaries need clarification.

4. Align skills
   - Update project-local skills after the canonical guide files exist, so skills can reference stable targets.

5. Validate consistency
   - Check links, terminology, and command accuracy across all updated files.

## 9. Risks and Mitigations

- Risk: root guidance becomes too long again.
  - Mitigation: keep the root README and root AGENTS focused on routing and repository shape, not detailed setup.

- Risk: UI guidance drifts from the real Angular app state.
  - Mitigation: derive commands and assumptions from `UI/package.json` and `UI/angular.json`, and avoid documenting future integration behavior as if it already exists.

- Risk: project-local skills continue to misroute work.
  - Mitigation: explicitly update skills that make repository-structure or testing recommendations, and point them at canonical guide files.

- Risk: duplicated guidance diverges over time.
  - Mitigation: give each layer a clear purpose:
    - root = navigation,
    - child README = contributor setup,
    - child AGENTS = delivery workflow,
    - project guide = structure and conventions.

## 10. Unknowns and Required Clarifications

- No blocker identified for producing this plan.
- Assumption: the current `UI` app should be treated as the initial UI service and documented as such.
- Assumption: there is not yet a supported combined local orchestration flow for starting API and UI together from the repository root.
- Follow-up question for implementation time if needed: whether the UI should eventually have its own custom project-local skill(s), or whether updating shared repo skills is sufficient for now.

## 11. Completion Checklist

- [x] Root `README.md` describes the repository as API + UI and links to both child READMEs.
- [x] Root `AGENTS.md` routes UI work to `UI/AGENTS.md`.
- [ ] `UI/README.md` is rewritten from Angular boilerplate into project-specific contributor guidance.
- [ ] `UI/AGENTS.md` combines repo workflow expectations with Angular-specific coding guidance.
- [x] Root `AI_PROJECT_GUIDE.md` exists and describes top-level repository boundaries.
- [ ] `UI/AI_PROJECT_GUIDE.md` exists if needed for canonical UI structure guidance.
- [x] `API/AI_PROJECT_GUIDE.md` remains accurate and linked from the correct places.
- [ ] Project-local skills that currently assume API-only structure are updated.
- [ ] Cross-links between README, AGENTS, project guides, and skills are valid and internally consistent.
- [ ] Final review confirms the repository guidance is lean at the root and detailed in child folders.
