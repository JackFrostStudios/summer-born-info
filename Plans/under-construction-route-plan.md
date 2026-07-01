# Under Construction Route Plan

## 1. Overview

Add a reusable under-construction route to the Angular UI so unfinished navigation targets can point somewhere intentional for now. The route must render inside the existing public shell with the header and footer still visible, and the main construction message should occupy a substantial portion of the viewport with centred content.

## 2. Source Context

- User request in the top-level thread on 2026-07-01.
- UX review completed by a dedicated sub-agent before implementation.
- Relevant UI areas:
  - `UI/src/app/app.routes.ts` owns route registration.
  - `UI/src/app/shell/` owns reusable shell framing such as the header and any shared footer.
  - `UI/src/app/features/` owns routed feature pages.
  - `UI/public/` owns production-served static assets.
- Supplied character asset:
  - `UI/prototypes/builder.svg`

## 3. Scope

- Add a dedicated routed UI page for temporary under-construction destinations.
- Keep the public header and footer visible on the new route.
- Use the supplied builder icon as decorative character on the page.
- Provide a friendly back button that navigates to the previous browser location when possible and falls back safely when not.
- Add or update tests to cover the shared shell framing and the new route content.
- Refresh localization output for the new user-facing copy and run the standard UI validation commands.

## 4. Non-Goals

- Building the eventual destination content for any unfinished linked pages.
- Expanding the navigation model beyond making this temporary route available.
- Reworking homepage content beyond any footer extraction needed to satisfy the shared-shell requirement.
- Adding new dependencies or animation libraries.

## 5. Behaviour Scenarios

### Scenario: A visitor lands on an unfinished route

Given a visitor opens a route that is not ready yet, when the under-construction page renders, then the public header remains visible above the routed content, the public footer remains visible below it, and the main message is centred inside a tall section that takes up a good portion of the viewport.

### Scenario: The page communicates temporary unavailability clearly

Given the page is rendered, when the visitor reads the content, then they should see a short `Coming soon` label, a clear heading, a brief reassuring explanation, and a single obvious back action without error-like or broken-product language.

### Scenario: A visitor uses the back action

Given the visitor activates the back button, when the browser has a previous history entry, then the app should navigate back to the previous location instead of forcing a fixed destination.

### Scenario: A direct visitor still has a clear way forward

Given a visitor opens the under-construction route without a meaningful browser history entry, when they activate the back button, then the app should send them to the homepage instead of leaving them stranded.

### Scenario: Assistive technology users encounter the page

Given a keyboard or screen-reader user lands on the route, when they move through the content, then the page should expose one clear `h1`, keep the builder illustration decorative, and present the back action as a focusable button with an accessible label.

## 6. UX-Approved Copy

- Eyebrow label: `Coming soon`
- Heading: `This page is still under construction`
- Body copy: `We're still building this part of Summer-born Info. Please go back and we'll help you continue from where you were.`
- Back button label: `Click here to go back`

## 7. Deliverables

1. Extract the footer into a shared shell-level footer component or equivalent shell structure so both the homepage and the new under-construction route show the same footer framing while the existing header remains unchanged.
2. Add the builder icon to the production-served UI asset path and create a routed under-construction feature component under `UI/src/app/features/` that uses the approved copy and a large centred layout.
3. Register the new route in `UI/src/app/app.routes.ts` so other pages can link to it later.
4. Implement previous-location navigation for the back button using Angular-supported browser navigation APIs, with a homepage fallback when no useful history entry exists.
5. Add or update unit tests for the shell composition and the new route rendering/interaction behaviour where practical.
6. Refresh i18n extraction output and run the standard UI completion commands before handoff.

## 8. Technology Requirements And Decisions

- Keep the route as a normal standalone Angular feature component under `UI/src/app/features/`.
- Keep shared framing in `UI/src/app/shell/` rather than duplicating footer markup across routed features.
- Use `NgOptimizedImage` for the static builder asset if it is rendered as an image.
- Treat the builder icon as decorative unless implementation reveals a compelling accessibility reason to expose it.
- Use component-scoped SCSS and existing shared tokens/primitives instead of introducing one-off global styles.
- Mark new visible template strings with Angular `i18n` metadata so `UI/src/locale/messages.xlf` remains authoritative.

## 9. Dependencies And Sequencing

1. Move footer ownership to the shell so the hard header-and-footer requirement applies consistently across public routes.
2. Add the new static asset and under-construction feature component.
3. Register the route and wire the back action.
4. Update tests for shell and route behaviour.
5. Refresh localization output and run format, lint, i18n validation, and unit tests.

## 10. Risks And Mitigations

- Risk: Moving the footer out of the homepage could unintentionally change homepage rendering.
  - Mitigation: keep the visual structure stable and cover footer presence through updated shell and route tests.
- Risk: Browser back navigation may feel broken when there is no prior meaningful history entry.
  - Mitigation: use the standard previous-location navigation mechanism when available and fall back to the homepage when it is not.
- Risk: The centred section could crowd the footer on smaller screens.
  - Mitigation: use a minimum-height approach that still allows the footer to remain visible and reachable without clipping content.

## 11. Unknowns And Clarifications

- No blocking product decisions remain after the UX copy review.
- If later work needs multiple temporary placeholder pages, they can all reuse this route pattern without changing the core shell structure.

## 12. Implementation Steps

- [x] Step 1: Implement the shared shell footer, the under-construction feature route, builder asset wiring, back-button behaviour, tests, and required UI validation.

## 13. Completion Checklist

- [x] The UI exposes a dedicated under-construction route that can be linked to by unfinished pages.
- [x] The public header remains visible on the under-construction route.
- [x] The public footer remains visible on the under-construction route.
- [x] The under-construction section fills a substantial portion of the viewport and centres its content.
- [x] The page uses the supplied builder icon as decorative character.
- [x] The route renders the UX-approved label, heading, body copy, and back button label.
- [x] The back button navigates to the previous location when available and falls back to the homepage when it is not.
- [x] Automated tests cover the changed shell and new route behaviour where practical.
- [x] `npm run format` has been run in `UI/`.
- [x] `npm run lint` has been run in `UI/`.
- [x] `npm run extract:i18n` has been run in `UI/`.
- [x] `npm run validate:i18n` has been run in `UI/`.
- [x] `npm run test:run` has been run in `UI/`.
