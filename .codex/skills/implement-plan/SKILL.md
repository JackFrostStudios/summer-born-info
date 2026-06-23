---
name: implement-plan
description: Implement a delivery-ready plan from the Plans folder step by step using isolated sub-agents, sequential execution, plan progress updates, validation, and commit checkpoints. Use when Codex is asked to carry out an existing implementation plan in `Plans/`, keep the main thread context clean while executing each plan step, or drive a plan through final checklist and review completion.
---

# Implement Plan

## Overview

Use this skill to execute an existing implementation plan in `Plans/` without letting the main thread accumulate the detailed context of every code change. Treat the plan as the source of truth, execute one step at a time, and keep progress visible in the plan itself.

This skill has strict execution rules:

- You MUST execute every implementation step with a dedicated sub-agent.
- You MUST execute implementation steps strictly one at a time.
- You MUST NOT implement plan steps directly in the main thread except for narrow orchestration tasks called out below.
- You MUST NOT begin the next implementation step until the current one is implemented, reviewed, validated, reflected in the plan, and committed.

If the plan is too broad to support strict step-by-step execution, refine the plan first instead of improvising a looser implementation flow.

## Workflow

1. Open the target plan in `Plans/` and confirm it is implementation-ready.
2. Execute each implementation step sequentially with a dedicated sub-agent that also verifies build and tests.
3. After each completed step, update the plan and commit the change before moving on.
4. After all steps are complete, run the high-level completion checklist.
5. Use another sub-agent for peer review and implement any feedback before final handoff.

## Hard Rules

- MUST use exactly one implementation sub-agent for the active step.
- MUST keep the main thread focused on orchestration, review, validation, plan updates, and commits.
- MUST NOT perform implementation for an active plan step in the main thread.
- MUST NOT run multiple implementation sub-agents in parallel for different plan steps.
- MUST NOT implement one step locally while a sub-agent is implementing another step.
- MUST NOT split or merge plan steps ad hoc during implementation just to make parallel execution easier.
- MAY perform limited local integration work in the main thread after a sub-agent returns, but only to:
  - resolve small merge or compile issues,
  - rerun validation,
  - update the plan,
  - create the commit for the completed step.
- MAY do read-only exploration before starting a step, but once implementation begins, sequencing must remain strict.

## Define The Step Boundary

Before starting implementation, identify the next incomplete step from the plan and treat it as the only active step.

- If the plan already contains numbered or ordered implementation steps, use those steps as the execution boundary.
- If one plan step is too large or ambiguous for a single worker handoff, stop and refine the plan first.
- Do not silently decompose a step into parallel workstreams during implementation.

## Validate The Plan First

Before implementation:

- Confirm the plan is in `Plans/` and clearly identifies the work to build.
- Confirm the plan has actionable implementation steps and a completion checklist.
- If the plan is ambiguous, incomplete, or blocked by unresolved decisions, stop implementation and refine it first, using the `implementation-planner` skill if needed.
- Gather only the code and project context needed for the current step.

Before the first code change, write down or mentally confirm this execution loop:

1. Identify the next incomplete step.
2. Spawn one implementation sub-agent for that step.
3. Wait for that step to finish before starting another implementation step.
4. Review and integrate the returned work.
5. Run validation.
6. Update the plan.
7. Commit the completed step.
8. Only then move to the next step.

## Execute Each Step

For every plan step:

1. Start a fresh sub-agent for that step so the main context stays clean.
2. Give the sub-agent:
   - The exact plan step to implement
   - The minimum relevant files and constraints
   - The delivery instructions needed for that step
   - These additional instructions:
     - Use the surface-specific architect skill for the step (`api-architect` for `API/` work or `ui-architect` for `UI/` work).
     - Use the surface-specific automated test skill for the step (`api-automated-test-developer` for `API/` work or `ui-automated-test-developer` for `UI/` work).
     - Before handoff, ensure the build and tests are passing.
3. Require the sub-agent to report:
   - What it changed
   - What tests it added or updated
   - What validation it ran
   - Any residual risk or follow-up it could not complete
4. Review the sub-agent output, inspect the resulting changes, and resolve any remaining issues in the main thread before marking the step complete.

Run steps sequentially. Do not start the next step until the current step is implemented, verified, and committed. This avoids overlapping edits, hidden coupling, and confusing commit history.

While a step sub-agent is running:

- Do not start implementation for another step.
- Do not spawn another implementation worker for a different step.
- Do not make step-scoped code changes locally in parallel.
- You may review returned context, prepare validation commands, or gather read-only information that does not overlap with active implementation.

## Update The Plan And Commit Progress

After a step is confirmed complete:

- Mark that task as completed in the plan document.
- Keep the wording of unfinished tasks intact unless the implementation changed the intended scope and the plan must be corrected.
- Commit the code and plan update together before moving to the next step.
- Use commit messages that clearly map to the completed plan step.
- Record which sub-agent completed the step, what validation passed, and which commit corresponds to the step in your working notes or status update.

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

Review work may be delegated separately, but the same sequencing rule applies: finish review feedback, validate it, and commit it before any further implementation work begins.

## Recovery Rule

If you realize you have already drifted from this skill:

- Stop starting new implementation work immediately.
- Finish or safely unwind the current in-flight action.
- Return to strict one-step-at-a-time sub-agent execution.
- Note the deviation in your status update so the execution record stays honest.

## Sub-Agent Prompt Shape

Use a prompt structure close to this:

```text
Implement step <step id/title> from <plan path>.

Step details:
<paste the exact step text>

Relevant context:
<files, constraints, behaviour notes>

Requirements:
- Use the surface-specific architect skill to design the implementation.
- Use the surface-specific automated test skill to write tests.
- Before handoff, ensure build and tests are passing.
- Return a concise summary of changes, tests, validation, and any residual risks.
```

## Quality Bar

- Keep the main thread focused on orchestration, verification, and plan state.
- Keep each sub-agent focused on one plan step or one targeted follow-up.
- Prefer small, reviewable commits aligned to plan milestones.
- Ensure the plan document remains an accurate record of what is complete.
- Do not hand off with failing builds or failing tests unless the user explicitly accepts that risk.
- If you cannot follow the strict delegation and sequencing model, pause and fix the plan or approach before continuing.
