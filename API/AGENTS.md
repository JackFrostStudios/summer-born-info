# Agents

Use sub-agents extensively to delegate specific actions to specialists, this reduces bloat on the main context window.
Provide the smallest amount of context that still leads to a good decision.

## Delegation

- Use the `architect` sub-agent for code layout, feature placement, and project pattern questions.
- Use the `automated-test-developer` sub-agent for test design and implementation.

## Delivery Rules

- Assume implementation work already has a corresponding plan in the root `Plans` folder.
- Read the relevant plan before making changes and treat it as the delivery contract unless the user redirects the work.
- If the plan is missing, incomplete, or no longer matches the requested change, pause and align with the user instead of inventing a new implementation plan in the API workflow.
- Update the existing plan to reflect delivered tasks when the work changes its completion state.
- After each step is complete commit the changes to git.
- Prefer TDD for behaviour changes:
  - define the contract first,
  - add failing tests,
  - implement until the tests pass.
- After tests pass, review the result and refactor only where it improves clarity or maintainability.
- Prefer the simplest maintainable solution that fits the existing project patterns.

## Canonical Project Guide

- Use [AI_PROJECT_GUIDE.md](AI_PROJECT_GUIDE.md) as the source of truth for solution layout and common .NET conventions.
