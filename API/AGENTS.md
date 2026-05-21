# Agents

Use sub-agents extensively to delegate specific actions to specialists, this reduces bloat on the main context window.
Provide the smallest amount of context that still leads to a good decision.

## Delegation

- Use the `architect` sub-agent for code layout, feature placement, and project pattern questions.
- Use the `automated-test-developer` sub-agent for test design and implementation.

## Delivery Rules

- For non-trivial or cross-cutting work, draft a short bullet-point plan before implementing.
- Always place the plan in a "Plans" folder at the root of the repo.
- Confirm the plan with the user when the change is ambiguous, risky, or spans multiple files or layers.
- For small, safe edits, proceed without a separate confirmation step.
- Write the plan to a markdown file, when each task has been delivered update the file to indicate the step is complete.
- After each step is complete commit the changes to git.
- Prefer TDD for behaviour changes:
  - define the contract first,
  - add failing tests,
  - implement until the tests pass.
- After tests pass, review the result and refactor only where it improves clarity or maintainability.
- Prefer the simplest maintainable solution that fits the existing project patterns.

## Canonical Project Guide

- Use [AI_PROJECT_GUIDE.md](AI_PROJECT_GUIDE.md) as the source of truth for solution layout and common .NET conventions.
