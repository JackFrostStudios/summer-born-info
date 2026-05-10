# Agents

When performing certain actions, spawn a sub-agent to deliver this work efficiently. Provide specific context to that agent to deliver that specific task:

- When determining how to implement a code change use `architect` -> [`.agents/architect.md`](.agents/architect.md), which should load [`.codex/skills/architect/SKILL.md`](.codex/skills/architect/SKILL.md), for code structure, placement, and project patterns.
- When implementing automated tests use `automated-test-developer` -> [`.agents/automated-test-developer.md`](.agents/automated-test-developer.md), which should load [`.codex/skills/automated-test-developer/SKILL.md`](.codex/skills/automated-test-developer/SKILL.md), for automated test design and implementation.

## Feature Delivery Process

- Before starting implementation, identify a concise bullet-point plan for delivering the feature.
- Confirm the plan with the user and resolve any clarifications before proceeding.
- Do not begin implementation until the plan has been agreed.
- Deliver features using TDD:
  - create required interfaces or contracts first,
  - add failing tests that define the desired behavior,
  - implement until the tests pass.
- Once the tests pass, review the implementation and refactor where needed to improve code quality.
- Prefer the simplest solution that is maintainable and fits the existing project patterns.
