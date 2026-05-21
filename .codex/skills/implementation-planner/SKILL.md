---
name: implementation-planner
description: Turn a roadmap item or existing implementation plan into a delivery-ready plan with concrete deliverables, behaviour scenarios, technology decisions, risks, dependencies, and a completion checklist. Use when Codex needs to create or refine markdown plans in the Plans folder, especially when converting high-level roadmap work into implementation detail or resolving gaps that would otherwise block delivery.
---

# Implementation Planner

## Overview

Use this skill to convert high-level roadmap intent into an implementation plan that a delivery agent can execute without further research or hidden decision making.

Plans produced with this skill should be specific enough to build from, while still calling out any decisions that require the user's input before the work can safely proceed.

## Workflow

1. Identify whether the task is:
   - Creating a new plan from a roadmap item in `Roadmap/`
   - Refining an existing plan in `Plans/`
2. Gather the minimum source context:
   - The roadmap item, milestone, or requirement being implemented
   - Any existing plan content
   - Relevant project constraints, conventions, or architecture notes
3. Separate confirmed facts from assumptions.
4. Expand the work into concrete behaviour, deliverables, dependencies, and verification expectations.
5. Stop and discuss unresolved decisions with the user when they materially affect implementation.
6. Finalise the plan only when it is delivery ready or clearly marked as blocked by a specific decision.

## Required Output

Default to the structure in [references/plan-template.md](references/plan-template.md).

Every plan should normally include:

- Overview
- Roadmap source or existing plan context
- Scope
- Non-goals
- Behaviour scenarios
- Deliverables
- Technology requirements and decisions
- Dependencies and sequencing
- Risks and mitigations
- Unknowns and required clarifications
- Completion checklist

Adjust section names if needed, but do not omit critical content.

## Behaviour Scenarios

Document expected actions and outcomes, not just build tasks.

For each important user or system behaviour:

- Describe the trigger or starting state.
- Describe the action taken.
- Describe the expected outcome.
- Include edge cases, invalid inputs, failure paths, retries, and error handling where relevant.
- Make acceptance conditions specific enough that implementation and testing can be derived directly from the plan.

Use lightweight scenario language such as `Given / When / Then` when it improves clarity.

## Technology Requirements and Decisions

Make technical expectations explicit. If the feature needs any of the following, document the decision and rationale in the plan:

- New architecture or architectural pattern
- New package, library, or dependency
- New infrastructure, storage, integration, or background processing capability
- Changes to observability, security, deployment, or operations

If a material technology choice is still open, discuss it with the user before finalising the plan. Do not quietly pick a dependency or architecture that changes long-term project direction without user alignment.

## Unknowns and Clarifications

Actively surface unknowns that would affect scope, sequencing, behaviour, technology choices, or delivery risk.

When an unknown materially changes the plan:

- Ask the user directly.
- Capture the answer in the plan.
- If the user is unavailable, mark the plan as draft or blocked rather than pretending the decision is settled.

Minor uncertainty can remain as documented follow-up, but critical delivery blockers should be resolved before the plan is presented as implementation ready.

## Deliverable Rules

- Break work into concrete deliverables, not vague intentions.
- Phrase deliverables so an implementation agent can tell what needs to exist or change.
- Include verification expectations when they are necessary for completion.
- Call out sequencing when one deliverable unlocks another.
- Prefer grouping by capability or slice of behaviour instead of by technical layer alone.

## Quality Gate

Before finalising a plan, confirm all of the following:

- The roadmap item or source intent is clearly traceable.
- Scope and non-goals are explicit.
- Core behaviour scenarios are documented with outcomes.
- Edge cases and error handling are covered for meaningful failure paths.
- Deliverables are concrete enough to implement without additional discovery.
- Technology requirements and decisions are recorded.
- Any new architecture, package, or dependency choice has been discussed with the user.
- Dependencies, risks, and sequencing are visible.
- Unknowns that would block delivery have been resolved with the user or clearly marked as blockers.
- The completion checklist is specific enough to verify when the plan is done.
- The document does not rely on implied decisions or hidden research.
