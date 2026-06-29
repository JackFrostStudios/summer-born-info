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
- Establish the first reusable visual foundation for homepage-adjacent public pages, including a modern reset, modular shared CSS files, theme tokens, light/dark mode support, and documented usage guidance.
- Ensure the delivered homepage is responsive, accessible, and consistent across browser and SSR entry paths already present in the UI project.

## 4. Non-Goals

- Building public school discovery, search, or review submission flows.
- Creating a full component library or exhaustive brand guidelines beyond the first reusable design-system foundation needed for the homepage and later public pages.
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

### Scenario: A visitor controls colour mode

Given the application supports operating-system light and dark preferences, when a visitor switches between light mode, dark mode, and system default, then the application should persist the explicit choice, clear persistence when reset to system, and continue to expose `color-scheme: light dark` so native browser controls render appropriately.

### Scenario: Contributors use the shared design foundation

Given a contributor adds a future public UI area, when they need common colours, borders, spacing, typography, focus states, or surface styles, then they should be able to discover and use documented shared CSS tokens and primitives without copying prototype CSS or inventing one-off values.

## 6. Deliverables

1. Create a milestone-specific prototype area, outside the Angular route implementation, that contains a set of simple static HTML/CSS homepage mockups for review. The initial target should be three clearly different directions so approval is meaningful rather than cosmetic.
2. Give each prototype a distinct visual thesis, such as different font pairings, color systems, spacing density, and overall tone, while keeping the content structure broadly comparable so stakeholders can evaluate style rather than copy differences.
3. Include lightweight review notes with the prototypes that identify the intent of each direction, its strengths, and any accessibility or implementation considerations the team should keep in mind during approval.
4. Capture the selected direction in the plan or linked implementation notes before Angular implementation begins, including any agreed blend of elements if the chosen result is not a single prototype unchanged.
5. Replace the current `home-placeholder` implementation with a real homepage feature under `UI/src/app/features/home/`, preserving the route ownership pattern already defined in `UI/src/app/app.routes.ts`.
6. Implement the in-scope homepage sections in Angular: header, welcome or hero content, supporting introductory content as needed, and footer.
7. Introduce the shared styling foundation required by the approved design, including a modern CSS reset, local Hanken Grotesk font registration, CSS custom properties for colour, typography, spacing, borders, focus states, surfaces, shadows, and page width, with global styles split into modular files grouped by concern.
8. Implement colour mode support with `color-scheme: light dark`, the modern `light-dark()` CSS function for theme-sensitive tokens, and a user-facing control that can select light, dark, or reset to the system default.
9. Document shared design-system styling usage, including where the modular CSS files live, how tokens should be used, how theme mode works, and which prototype content must not be copied into production.
10. Update shell-level layout only where the approved homepage requires app-level framing, while keeping feature-specific rendering out of `UI/src/app/shell/`.
11. Mark visible homepage copy for Angular i18n extraction where required by the existing UI localization workflow.
12. Add or update automated UI tests for homepage rendering, theme-mode behaviour, and any shell-route composition affected by the change.
13. Refresh generated localization output if user-facing template text changes it, and run the expected UI validation commands before handoff.

## 7. Technology Requirements and Decisions

- Prototype format:
  Use plain static HTML and CSS for the initial design exploration so feedback can focus on visual direction without Angular structure slowing iteration.
- Prototype location:
  Keep prototypes in a clearly named non-production workspace area under `UI/` such as `UI/prototypes/milestone-8-homepage/`, so they are easy to review without becoming part of the routed application surface.
- Implementation target:
  The approved design must be implemented in the existing Angular homepage feature route, not left as a static artifact.
- Styling approach:
  Prefer CSS custom properties and component-scoped styles over introducing a third-party design framework or utility CSS dependency for this milestone.
- Modular shared CSS:
  Keep `UI/src/styles.scss` as the shared entry point and move reusable global concerns into imported files grouped by purpose, such as reset, font faces, tokens, base elements, utilities, and theme-mode behaviour.
- Colour mode:
  Set `color-scheme: light dark` at the document level. Use `light-dark()` for theme-sensitive design tokens, and drive explicit user overrides through a root attribute or equivalent class that changes the active `color-scheme`. Provide a reset path that removes the override and returns to the system preference.
- Fonts:
  Use the locally supplied Hanken Grotesk files from `UI/prototypes/Hanken_Grotesk/` by copying the required production font files into the Angular static asset area rather than relying on a hosted font service.
- Prototype content:
  The approved prototypes are visual references only. Production implementation must strip unsupported claims, invented metrics, advocacy language, and prototype-only CTAs while preserving the overall visual direction.
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
5. Implement the shared design-system foundation first:
   - Add the modern reset and modular shared CSS structure.
   - Define reusable colour, border, spacing, typography, focus, layout, and surface tokens.
   - Register the local Hanken Grotesk font for production use.
   - Document how contributors should use the shared styling.
6. Add light and dark mode support using `light-dark()` and a user control for light, dark, and system default.
7. Implement the Angular homepage feature and any minimal shell/global style updates needed to support it, using production-safe copy that strips unsupported prototype claims and invented metrics.
8. Add or update tests and refresh localization artifacts affected by the new homepage copy and theme controls.
9. Run UI validation commands and perform responsive and accessibility checks before considering the milestone complete.

## 8.1 Implementation Steps

1. Design-system foundation:
   Create the modular shared CSS structure under `UI/src/styles/`, wire it through `UI/src/styles.scss`, add a modern reset, register the local Hanken Grotesk font from `UI/prototypes/Hanken_Grotesk/` via static assets, define reusable tokens and base primitives, and document their intended use for future UI contributors. Do not implement homepage-specific content in this step.
2. Colour-mode support:
   Add application support for light, dark, and system-default modes using `color-scheme: light dark`, `light-dark()`-based tokens, and a small accessible user control. Persist explicit light/dark choices and clear persistence when the user resets to system default. Add tests for mode selection and reset behaviour.
3. Homepage implementation:
   Replace `home-placeholder` with the production homepage route using the approved prototype layout and visual system, but strip unsupported claims, invented metrics, advocacy language, and prototype-only CTAs. Keep final copy simple, factual, and roadmap-aligned. Mark user-facing template text for i18n extraction.
4. Homepage tests and validation:
   Add or update route, rendering, responsive, accessibility, and localization coverage as appropriate, refresh generated i18n output, and run the required UI validation commands before final review.

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
- The design-system foundation is intentionally limited to shared CSS assets, tokens, primitives, documentation, and colour mode support. It should not grow into a full Angular component library during this milestone.
- If the user wants external web fonts, illustration assets, or a more editorial content direction after prototype review, that should be treated as a follow-up decision and documented before implementation expands.
- No blocking clarification is required to create the plan, because the proposed prototype-first workflow is specifically intended to resolve the biggest open visual decision safely.

## 11. Completion Checklist

- [x] A milestone 8 prototype folder exists under `UI/` with multiple static HTML/CSS homepage directions.
- [x] The prototype set includes at least three visually distinct options.
- [x] Each prototype includes enough context for the user to choose a preferred direction confidently.
- [x] The approved direction is captured before Angular homepage implementation begins.
- [ ] The placeholder home feature is replaced by a real homepage implementation in `UI/src/app/features/home/`.
- [ ] The Angular homepage includes a header, welcome content, and footer.
- [x] A modern CSS reset is added and wired into the global stylesheet entry point.
- [x] Shared CSS is modularized into concern-specific files under `UI/src/styles/`.
- [x] Reusable colour, border, spacing, typography, focus, layout, and surface tokens are defined for the approved design direction.
- [x] The local Hanken Grotesk font is available to the Angular app without external font hosting.
- [x] Shared theme tokens or layout primitives are introduced where they support the approved design and future reuse.
- [x] Design-system styling usage is documented for future contributors.
- [x] Light and dark mode are implemented with `color-scheme: light dark` and `light-dark()`-based theme tokens.
- [x] Users can switch to light mode, switch to dark mode, and reset to the system default.
- [ ] Prototype-only unsupported claims, invented metrics, advocacy language, and unsupported CTAs are excluded from production homepage copy.
- [ ] Homepage text is marked for i18n extraction where appropriate.
- [ ] Responsive behaviour is verified for mobile and desktop layouts.
- [ ] Accessibility checks are completed for semantics, focus states, and contrast at a minimum.
- [ ] Homepage tests are added or updated for the changed route output.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run extract:i18n` has been run in `UI/` if template text changed tracked messages.
- [x] `npm run validate:i18n` has been run in `UI/` if localization output was affected.
- [x] `npm run test:run` has been run in `UI/`.
