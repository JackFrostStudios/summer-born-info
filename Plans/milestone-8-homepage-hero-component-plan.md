# Milestone 8 Homepage Hero Component Plan

## 1. Overview

Refactor the homepage feature so the current hero block becomes its own focused `home-hero` Angular component. This first slice should establish a maintainable pattern for splitting the homepage into section-level components without changing the approved visual design, copy, accessibility semantics, or current rendered behaviour.

## 2. Roadmap Source or Existing Plan Context

- Existing homepage delivery baseline: [milestone-8-homepage-plan.md](./milestone-8-homepage-plan.md)
- Existing homepage follow-up work: [milestone-8-homepage-follow-up-plan.md](./milestone-8-homepage-follow-up-plan.md)
- Relevant repository guidance:
  - [Plans/AGENTS.md](./AGENTS.md)
  - [UI/AGENTS.md](../UI/AGENTS.md)
  - [AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md)
  - [UI/AI_PROJECT_GUIDE.md](../UI/AI_PROJECT_GUIDE.md)
- Current implementation context:
  - `UI/src/app/features/home/home.ts` owns the full homepage route component.
  - `UI/src/app/features/home/home.html` currently contains all homepage sections inline, including the hero, topics, coming-soon panel, and scope note.
  - `UI/src/app/features/home/home.scss` contains both route-level layout styles and section-specific styles for the hero and later sections.
  - `UI/src/app/features/home/home.spec.ts` currently verifies the homepage as a single rendered surface.

## 3. Scope

- Extract the current homepage hero markup, image, and hero-specific styling into a dedicated `home-hero` component under the `home` feature area.
- Update the `Home` route component so it becomes the page composer for the hero and the remaining inline sections.
- Preserve the current hero text, CTA label, image usage, styling appearance, accessibility semantics, and localization metadata during the refactor.
- Add or update tests so the new component boundary is covered and the homepage still renders the approved hero content correctly.
- Leave the remaining homepage sections in place for now, while making the next section extractions easier.

## 4. Non-Goals

- Rewriting homepage copy, changing the hero CTA behaviour, or introducing new navigation.
- Redesigning the hero layout, image treatment, spacing, or visual hierarchy.
- Extracting the topics, coming-soon, or scope-note sections in the same slice.
- Introducing shared cross-feature UI abstractions or a generic section-component framework.
- Changing shell-level layout, colour-mode behaviour, or global styling tokens.

## 5. Behaviour Scenarios

### Scenario: A visitor lands on the homepage after the refactor

Given the homepage route renders in the root shell, when the page loads after the refactor, then the hero should appear with the same heading, supporting copy, CTA text, and image as before, with no visible regression in layout or spacing.

### Scenario: The homepage keeps a single page-level heading

Given the homepage article is announced to assistive technology, when the hero component renders the main title, then the page should still expose exactly one `h1`, the article should remain labelled by that heading, and section extraction should not break the current heading hierarchy.

### Scenario: The hero component owns its local presentation

Given a contributor needs to understand or edit the hero section, when they open the `home-hero` files, then they should be able to find the hero template, styles, and tests without having to scan unrelated homepage sections.

### Scenario: The homepage route remains easy to compose

Given future refactor work will extract more homepage sections, when a contributor opens the `Home` route component, then the template should read as a high-level composition of page sections rather than one long block of mixed content and styling.

### Scenario: Existing hero copy remains in the i18n workflow

Given the hero text already participates in Angular i18n extraction, when the hero moves into its own component, then the `i18n` metadata should remain attached to the rendered strings so extraction and message identity stay intentionally managed.

## 6. Deliverables

1. Create a dedicated `home-hero` component within `UI/src/app/features/home/`, preferably in its own subfolder such as `UI/src/app/features/home/home-hero/`, with colocated TypeScript, template, stylesheet, and test files.
2. Move the hero-specific markup from `home.html` into the new component and render that component from the `Home` template.
3. Move hero-specific styles out of `home.scss` into the new component stylesheet, keeping only route-level layout and non-hero section styles in `home.scss`.
4. Decide and document the ownership of the page-level heading ID so the `article[aria-labelledby]` contract remains intact.
5. Update the `Home` component imports so it composes the new `HomeHero` component cleanly.
6. Add or update automated tests to cover both:
   - homepage composition still including the hero content, and
   - focused hero component rendering for heading, CTA, highlight treatment, and image semantics.
7. Run the standard UI validation commands required for a component-structure change and refresh i18n artifacts only if extraction output changes.

## 7. Technology Requirements and Decisions

- Component placement:
  Keep the new component inside the existing `home` feature directory so the route and its section-level subcomponents stay together. A feature-local subfolder such as `home/home-hero/` is preferred over placing the component in a shared area, because this section is homepage-specific.
- Angular structure:
  Implement `home-hero` as a standalone Angular component following the current project defaults and import it directly into `Home`.
- Styling split:
  Keep page-composition styles in `home.scss` and move only hero-owned selectors into the new component stylesheet. Avoid duplicating shared selectors across both files.
- Accessibility contract:
  Preserve the single page-level `h1`, the hero image alt text, and the current article labelling. If the `h1` remains inside `HomeHero`, pass the heading ID from `Home` into the child component via `input()` so the page-level labelling relationship stays explicit.
- Localization:
  Preserve the existing `i18n` metadata on the hero content. Do not silently reword hero strings as part of the extraction.
- Testing approach:
  Prefer keeping one homepage composition spec and adding a focused `home-hero` spec rather than moving all assertions into one file. Tests should continue to assert visible behaviour rather than component internals.

## 8. Dependencies and Sequencing

1. Confirm the current hero boundary in `home.html` and identify all hero-owned selectors in `home.scss`.
2. Create the `home-hero` component files and move the hero template and hero styles into the new boundary.
3. Update `Home` to import and render `HomeHero`, passing any required heading ID or other minimal inputs.
4. Remove now-unused hero markup and CSS from the parent route files so the ownership split is clear.
5. Update or add tests for both composition and focused hero rendering.
6. Run `npm run format`, `npm run lint`, `npm run test:run`, and i18n commands as needed for the touched UI files.

## 9. Risks and Mitigations

- Risk: Moving the `h1` into a child component weakens the page-level labelling relationship.
  - Mitigation: keep the article label owned by `Home` and pass the heading ID into `HomeHero` explicitly so the relationship is still obvious and testable.
- Risk: Hero CSS extraction leaves behind dead selectors or accidentally changes layout due to specificity shifts.
  - Mitigation: inventory hero selectors before the move and verify that route-level spacing and section styles still live in the correct stylesheet after cleanup.
- Risk: Refactoring a translated template changes message extraction unexpectedly.
  - Mitigation: preserve existing `i18n` attributes and run extraction validation if the tool reports message drift.
- Risk: The new component boundary is too tailored to this exact hero and does not help later section extraction.
  - Mitigation: keep `Home` as a simple page composer and use this slice to establish a clear feature-local section pattern that later extractions can follow.

## 10. Unknowns and Required Clarifications

- No blocking clarification is required to plan this slice.
- During implementation, the team should decide whether `HomeHero` needs any inputs beyond the page heading ID. Based on the current static hero, no additional inputs are expected.
- If the follow-up refactor extracts multiple additional sections, the team may later choose to add a small feature-local README or update `UI/AI_PROJECT_GUIDE.md` if the section-component pattern becomes a broader UI convention. That is not required for this first extraction.

## 11. Completion Checklist

- [x] A dedicated `home-hero` component exists under `UI/src/app/features/home/`.
- [x] The homepage hero markup is removed from `home.html` and rendered through `HomeHero` instead.
- [x] Hero-specific styles are removed from `home.scss` and live with the new component.
- [x] The homepage still renders exactly one `h1` and preserves the article labelling relationship.
- [x] Hero copy, CTA text, image source, and image alt text remain unchanged unless the user requests content updates separately.
- [x] Hero strings remain covered by Angular template i18n metadata.
- [x] Homepage composition tests are updated for the new structure.
- [x] Focused tests exist for the `home-hero` component.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run extract:i18n` has been run in `UI/` because tracked message locations changed.
- [x] `npm run validate:i18n` has been run in `UI/` because localization output changed.
- [x] `npm run test:run` has been run in `UI/`.

## 12. Delivery Notes

- `npm run validate:i18n` was executed and failed on its expected `git diff --exit-code -- src/locale/messages.xlf` guard because the refactor moved existing translatable strings into `home-hero/home-hero.html`, which updates extracted source locations.
- `npm run build:localize` passed separately to confirm the localized production build path still succeeds with the updated extraction output.
