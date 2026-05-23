---
name: implement-plan
description: Implement a delivery-ready plan from the Plans folder step by step using isolated sub-agents, sequential execution, plan progress updates, validation, and commit checkpoints. Use when Codex is asked to carry out an existing implementation plan in `Plans/`, keep the main thread context clean while executing each plan step, or drive a plan through final checklist and review completion.
---

# Implement Plan

## Overview

Use this skill to execute an existing implementation plan in `Plans/` without letting the main thread accumulate the detailed context of every code change. Treat the plan as the source of truth, execute one step at a time, and keep progress visible in the plan itself.

## Workflow

1. Open the target plan in `Plans/` and confirm it is implementation-ready.
2. Execute each implementation step sequentially with a dedicated sub-agent that also verify build and tests.
3. After each completed step, update the plan and commit the change before moving on.
4. After all steps are complete, run the high-level completion checklist.
5. Use another sub-agent for peer review and implement any feedback before final handoff.

## Validate The Plan First

Before implementation:

- Confirm the plan is in `Plans/` and clearly identifies the work to build.
- Confirm the plan has actionable implementation steps and a completion checklist.
- If the plan is ambiguous, incomplete, or blocked by unresolved decisions, stop implementation and refine it first, using the `implementation-planner` skill if needed.
- Gather only the code and project context needed for the current step.

## Execute Each Step

For every plan step:

1. Start a fresh sub-agent for that step so the main context stays clean.
2. Give the sub-agent:
   - The exact plan step to implement
   - The minimum relevant files and constraints
   - The delivery instructions needed for that step
   - These additional instructions:
     - Use the `architect` skill to design the implementation.
     - Use the `automated-test-developer` skill to write tests.
     - Before handoff, ensure the build and tests are passing.
3. Require the sub-agent to report:
   - What it changed
   - What tests it added or updated
   - What validation it ran
   - Any residual risk or follow-up it could not complete
4. Review the sub-agent output, inspect the resulting changes, and resolve any remaining issues in the main thread before marking the step complete.

Run steps sequentially. Do not start the next step until the current step is implemented, verified, and committed. This avoids overlapping edits, hidden coupling, and confusing commit history.

## Update The Plan And Commit Progress

After a step is confirmed complete:

- Mark that task as completed in the plan document.
- Keep the wording of unfinished tasks intact unless the implementation changed the intended scope and the plan must be corrected.
- Commit the code and plan update together before moving to the next step.
- Use commit messages that clearly map to the completed plan step.

## Finish The Plan

Once every implementation step is complete:

1. Walk through the plan's high-level completion checklist item by item.
2. If anything is missing, start a new sub-agent to implement the missing functionality before continuing.
3. Do not treat the plan as done until the checklist reflects the delivered state.

## Run Peer Review

After the completion checklist is satisfied:

1. Start a fresh sub-agent to peer-review the implemented changes.
2. Ask it to focus on correctness, regressions, missing tests, plan drift, and maintainability risks.
3. Implement the review feedback, using additional sub-agents if needed for larger fixes.
4. Re-run the relevant build and test validation after review changes.

## Sub-Agent Prompt Shape

Use a prompt structure close to this:

```text
Implement step <step id/title> from <plan path>.

Step details:
<paste the exact step text>

Relevant context:
<files, constraints, behaviour notes>

Requirements:
- Use the architect skill to design the implementation.
- Use the automated-test-developer skill to write tests.
- Before handoff, ensure build and tests are passing.
- Return a concise summary of changes, tests, validation, and any residual risks.
```

## Quality Bar

- Keep the main thread focused on orchestration, verification, and plan state.
- Keep each sub-agent focused on one plan step or one targeted follow-up.
- Prefer small, reviewable commits aligned to plan milestones.
- Ensure the plan document remains an accurate record of what is complete.
- Do not hand off with failing builds or failing tests unless the user explicitly accepts that risk.
