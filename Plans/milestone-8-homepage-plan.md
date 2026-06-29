# Milestone 8 Homepage Plan

## 1. Overview

Deliver Milestone 8 by first creating a small set of static HTML-only homepage prototypes that explore distinct visual directions, then implement the approved direction in the Angular UI as the first intentional public homepage. The finished milestone should leave the application with a responsive, accessible homepage and a reusable visual foundation that later public pages can extend without restyling the shell from scratch.

## 2. Roadmap Source or Existing Plan Context

- Roadmap source: [Roadmap/initial-ui-roadmap.md](../Roadmap/initial-ui-roadmap.md)
- Milestone: `Milestone 8: Homepage`
- Relevant repository guidance:
  - [Plans/AGENTS.md](./AGENTS.md)
  - [UI/AGENTS.md](../UI/AGENTS.md)
  - [AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md)
  - [UI/AI_PROJECT_GUIDE.md](../UI/AI_PROJECT_GUIDE.md)
- Current UI baseline:
  - Angular shell and root route already exist under `UI/src/app/`
  - The current home route renders `UI/src/app/features/home/home-placeholder.*`
  - Global styling is still minimal in `UI/src/styles.scss`

## 3. Scope

- Create several simple static homepage prototypes that focus on typography, color palette, spacing, and overall tone rather than Angular implementation detail.
- Present clearly differentiated design directions so the user can approve a preferred style before the main UI implementation begins.
- Convert the approved direction into the Angular homepage route with a header, welcome content, and footer.
- Establish the first reusable visual foundation for homepage-adjacent public pages, including theme tokens and shared layout conventions where justified by at least two consumers.
- Ensure the delivered homepage is responsive, accessible, and consistent across browser and SSR entry paths already present in the UI project.

## 4. Non-Goals

- Building public school discovery, search, or review submission flows.
- Creating a full design system, component library, or exhaustive brand guidelines.
- Introducing admin login, protected routes, or API integration as part of this milestone.
- Polishing multiple public routes beyond the homepage and any small shared shell elements needed to support it.
- Adding complex animation, CMS-driven content, or dynamic personalization.

## 5. Behaviour Scenarios

### Scenario: Stakeholders review visual directions before Angular implementation

Given Milestone 7 has established a runnable UI baseline, when the team opens the milestone 8 prototype files, then they should be able to compare multiple homepage directions that use different typography, color choices, and stylistic tone without needing the full Angular feature to be implemented first.

### Scenario: A design direction is approved

Given the prototype review has finished, when the user selects one direction or requests a small combination of elements from the explored options, then the plan should treat that decision as the styling baseline for the Angular homepage implementation and avoid further broad visual churn during the same milestone.

### Scenario: A public visitor lands on the homepage

Given an unauthenticated visitor opens the root route, when the homepage loads, then they should see a clear header, a welcoming hero or introduction area, and a footer that together communicate purpose, trust, and that more discovery functionality is coming later.

### Scenario: The homepage is viewed on mobile and desktop

Given a visitor uses either a narrow mobile viewport or a wider desktop viewport, when the homepage renders, then layout, spacing, typography, and navigation framing should remain readable and intentional without horizontal scrolling or broken section order.

### Scenario: Keyboard and assistive-technology users navigate the page

Given a user relies on keyboard navigation or screen-reader semantics, when they move through the homepage, then landmarks, heading structure, link/button focus states, contrast, and reading order should remain clear enough to meet the project's WCAG AA baseline expectations.

### Scenario: Future public pages reuse the chosen style

Given later roadmap work adds more public routes, when contributors extend the UI, then they should be able to reuse the homepage's agreed theme tokens, shell framing, and content section patterns instead of restyling the application from scratch.

## 6. Deliverables

1. Create a milestone-specific prototype area, outside the Angular route implementation, that contains a set of simple static HTML/CSS homepage mockups for review. The initial target should be three clearly different directions so approval is meaningful rather than cosmetic.
2. Give each prototype a distinct visual thesis, such as different font pairings, color systems, spacing density, and overall tone, while keeping the content structure broadly comparable so stakeholders can evaluate style rather than copy differences.
3. Include lightweight review notes with the prototypes that identify the intent of each direction, its strengths, and any accessibility or implementation considerations the team should keep in mind during approval.
4. Capture the selected direction in the plan or linked implementation notes before Angular implementation begins, including any agreed blend of elements if the chosen result is not a single prototype unchanged.
5. Replace the current `home-placeholder` implementation with a real homepage feature under `UI/src/app/features/home/`, preserving the route ownership pattern already defined in `UI/src/app/app.routes.ts`.
6. Implement the in-scope homepage sections in Angular: header, welcome or hero content, supporting introductory content as needed, and footer.
7. Introduce only the minimum shared styling foundation required by the approved design, such as CSS custom properties for color, typography, spacing, and page width, keeping global styles intentional and pushing feature-specific presentation into the homepage component styles where possible.
8. Update shell-level layout only where the approved homepage requires app-level framing, while keeping feature-specific rendering out of `UI/src/app/shell/`.
9. Mark visible homepage copy for Angular i18n extraction where required by the existing UI localization workflow.
10. Add or update automated UI tests for homepage rendering and any shell-route composition affected by the change.
11. Refresh generated localization output if user-facing template text changes it, and run the expected UI validation commands before handoff.

## 7. Technology Requirements and Decisions

- Prototype format:
  Use plain static HTML and CSS for the initial design exploration so feedback can focus on visual direction without Angular structure slowing iteration.
- Prototype location:
  Keep prototypes in a clearly named non-production workspace area under `UI/` such as `UI/prototypes/milestone-8-homepage/`, so they are easy to review without becoming part of the routed application surface.
- Implementation target:
  The approved design must be implemented in the existing Angular homepage feature route, not left as a static artifact.
- Styling approach:
  Prefer CSS custom properties and component-scoped styles over introducing a third-party design framework or utility CSS dependency for this milestone.
- Asset strategy:
  Use simple local static assets only if they materially support the chosen design. Do not introduce a large illustration pack, font-hosting pipeline, or external asset dependency without explicit follow-up approval.
- Accessibility baseline:
  Treat semantic HTML, focus visibility, color contrast, and responsive behaviour as required implementation constraints, not polish.
- Localization:
  Any homepage text rendered in Angular templates should participate in the existing `UI/src/locale/messages.xlf` workflow.

## 8. Dependencies and Sequencing

1. Confirm Milestone 7 is in place and the current homepage route still points to the placeholder feature.
2. Create the static prototype set first, because that review checkpoint is meant to reduce theme churn before Angular code is expanded.
3. Review prototypes with the user and capture the approved direction, including any requested mix-and-match adjustments.
4. Translate the approved direction into implementation-ready styling decisions: theme tokens, layout rules, content hierarchy, and any required asset choices.
   Current conclusion:
   Style and layout are approved for both light and dark mode via [milestone-8-homepage-style-decision.md](./milestone-8-homepage-style-decision.md). Final homepage content is intentionally still open and must be resolved during implementation.
5. Implement the Angular homepage feature and any minimal shell/global style updates needed to support it.
6. Add or update tests and refresh localization artifacts affected by the new homepage copy.
7. Run UI validation commands and perform responsive and accessibility checks before considering the milestone complete.

## 9. Risks and Mitigations

- Risk: Prototype review drifts because the variants are too similar.
  - Mitigation: require each prototype to represent a clearly different design direction rather than minor palette swaps.
- Risk: Theme churn continues after approval because the chosen direction is not documented precisely enough.
  - Mitigation: record the selected direction and any combined elements immediately after review, then treat that record as the implementation baseline.
- Risk: Static prototypes introduce styling ideas that are difficult to translate into the Angular app cleanly.
  - Mitigation: keep prototypes intentionally simple, HTML-first, and within the technical constraints of the existing Angular and SSR setup.
- Risk: The homepage becomes over-engineered for a milestone intended as a foundation.
  - Mitigation: keep scope focused on the homepage route, minimal shared tokens, and the sections explicitly called for in the roadmap.
- Risk: Visual ambition introduces accessibility regressions.
  - Mitigation: review contrast, heading structure, landmarks, and keyboard focus during both prototype selection and Angular implementation.
- Risk: User-facing copy changes create localization drift.
  - Mitigation: treat i18n extraction and validation as part of the definition of done for the implementation slice.

## 10. Unknowns and Required Clarifications

- Brand messaging remains lightly defined in the roadmap. The selected prototypes settle style and layout, but final production homepage content still needs to be decided during implementation.
- The exact final number of homepage content sections beyond header, welcome content, and footer can remain implementation-level flexible as long as the page stays simple and roadmap-aligned.
- If the user wants external web fonts, illustration assets, or a more editorial content direction after prototype review, that should be treated as a follow-up decision and documented before implementation expands.
- No blocking clarification is required to create the plan, because the proposed prototype-first workflow is specifically intended to resolve the biggest open visual decision safely.

## 11. Completion Checklist

- [x] A milestone 8 prototype folder exists under `UI/` with multiple static HTML/CSS homepage directions.
- [x] The prototype set includes at least three visually distinct options.
- [x] Each prototype includes enough context for the user to choose a preferred direction confidently.
- [x] The approved direction is captured before Angular homepage implementation begins.
- [ ] The placeholder home feature is replaced by a real homepage implementation in `UI/src/app/features/home/`.
- [ ] The Angular homepage includes a header, welcome content, and footer.
- [ ] Shared theme tokens or layout primitives are introduced only where they support the approved design and future reuse.
- [ ] Homepage text is marked for i18n extraction where appropriate.
- [ ] Responsive behaviour is verified for mobile and desktop layouts.
- [ ] Accessibility checks are completed for semantics, focus states, and contrast at a minimum.
- [ ] Homepage tests are added or updated for the changed route output.
- [ ] `npm run format` has been run in `UI/`.
- [ ] `npm run lint` has been run in `UI/`.
- [ ] `npm run extract:i18n` has been run in `UI/` if template text changed tracked messages.
- [ ] `npm run validate:i18n` has been run in `UI/` if localization output was affected.
- [ ] `npm run test:run` has been run in `UI/`.
