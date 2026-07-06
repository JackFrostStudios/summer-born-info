# UI Performance Review Remediation Plan

## 1. Overview

Resolve the actionable findings captured in `UI/review-findings/performance-expert.md`, using the current `UI/` codebase as the source of truth for issue status. This plan keeps every reviewed issue visible for tracking, including the portion of the routing finding that has already been addressed since the review was written.

## 2. Roadmap Source or Existing Plan Context

- Request source: top-level user request on 2026-07-06 to create a remediation plan for `UI/review-findings/performance-expert.md`.
- Review source: `UI/review-findings/performance-expert.md`.
- Verification source: a dedicated codebase-check sub-agent plus local inspection verified each issue against the current repository state on 2026-07-06 before this plan was drafted.
- Relevant repository guidance:
  - `Plans/AGENTS.md`
  - `UI/AGENTS.md`
  - `AI_PROJECT_GUIDE.md`
  - `UI/AI_PROJECT_GUIDE.md`
  - `UI/src/styles/README.md`

### Verified Review Status Snapshot

1. Issue 1: Large unoptimized above-the-fold image payload - `Resolved`
2. Issue 2: Font payload is large and served as TTF - `Open`
3. Issue 3: Routes are eagerly imported, so future pages will inflate initial JS - `Partially Resolved`
4. Issue 4: Asset budgets do not guard the real payload hotspots - `Open`
5. Issue 5: Localized build output uses root-relative asset URLs - `Open`
6. Issue 6: Theme icons use CSS masks and multiple external icon requests - `Open`
7. Issue 7: Inline theme script is small but parser-blocking - `Open`

### Current-State Notes That Shape The Plan

- The homepage hero now uses the prepared AVIF asset at `UI/public/images/hero-child-playing.avif` as the canonical priority LCP image source, replacing the previously oversized PNG on the critical path.
- Delivery-prepared replacement assets now exist in the repo: WOFF2 font files have been added under `UI/public/fonts/`, and an AVIF hero image has been added under `UI/public/images/`.
- Additional implementation input from 2026-07-06: treat the already-added delivery assets as the intended remediation inputs, specifically `UI/public/images/hero-child-playing.avif` for the homepage LCP image and the prepared WOFF2 files in `UI/public/fonts/`, rather than creating a new asset-generation workflow inside this plan.
- Font delivery is still backed by `Hanken Grotesk` variable TTF assets referenced from `UI/src/styles/_fonts.scss`; implementation should switch to the already-added WOFF2 assets rather than introducing a new conversion workflow in this slice.
- The route-loading issue is no longer fully open because `under-construction` already uses `loadComponent` in `UI/src/app/app.routes.ts`, but the broader route-growth strategy still needs to be tightened and documented.
- Existing production budgets cover only `initial` and `anyComponentStyle`, and the current UI CI workflow does not add image/font payload checks.
- The localized output currently combines `<base href="/en-GB/">` with root-relative asset URLs like `/fonts/...`, `/images/...`, and `/icons/...`, so the deployment-path risk remains active until it is proven safe in a production-like host.
- Theme icons should now be remediated by creating a reusable `UI/src/design-system/icons/` folder with inline SVG assets/components so icon colour can be controlled directly in CSS, eliminating the need for CSS background-mask delivery in the final implementation.
- The theme boot script is intentionally tiny and acceptable today, so this plan treats it as a guardrail item rather than a refactor mandate.

## 3. Scope

- Reduce critical-path asset weight for the homepage hero image.
- Move font delivery from TTF to the already-prepared WOFF2 assets in `UI/public/fonts/`.
- Lock in the current lazy-route improvement and extend route-loading guidance so future feature growth does not bloat the initial bundle.
- Add CI-visible bundle and static-asset guardrails.
- Resolve or explicitly validate the localized asset-path strategy.
- Reassess the low-priority theme icon and inline theme-script findings without over-optimizing them, with icons delivered through reusable inline SVGs rather than external masked assets.

## 4. Non-Goals

- Reworking homepage copy, layout, or CTA behavior beyond what is required for asset-delivery changes.
- Adding a broad image-processing pipeline for the whole app if a targeted homepage-asset fix is sufficient.
- Lazy-loading the root shell or homepage route, which remain appropriate as eager entry points.
- Replacing the theme boot script with an Angular-only solution if that would reintroduce a theme flash.
- Introducing new runtime dependencies for font or image delivery unless implementation proves they are clearly justified and the user agrees.

## 5. Behaviour Scenarios

### Scenario: A first-time visitor lands on the homepage

Given a visitor opens the homepage on a production build, when the browser requests above-the-fold assets, then the LCP image should still load with high priority but use a materially smaller payload than the current PNG-only version.

### Scenario: The homepage renders with the site font stack

Given the homepage renders on a localized production build, when the browser requests the Hanken Grotesk assets, then it should fetch WOFF2 files instead of TTF and only preload the face or faces truly needed for first paint.

### Scenario: The app grows with more routed pages

Given a developer adds a future secondary route, when they extend `UI/src/app/app.routes.ts`, then the default expectation should be route-level lazy loading for non-shell, non-primary routes so initial JavaScript growth is constrained.

### Scenario: CI validates performance guardrails

Given a pull request changes UI assets or bundle composition, when UI CI runs, then it should surface failures or warnings for oversized scripts and tracked static assets rather than allowing regressions to hide behind a broad initial budget.

### Scenario: A localized build is deployed under `/en-GB/`

Given the localized production output sets `<base href="/en-GB/">`, when the browser resolves image, icon, and font URLs, then those requests should still resolve correctly in the intended hosting model without locale-path bypasses, failed requests, or duplicate caches.

### Scenario: Theme controls continue to avoid a theme flash

Given a visitor has previously chosen a colour mode, when the document starts parsing, then the initial theme attribute should still be applied early enough to avoid a visible flash, while the implementation remains intentionally tiny and easy to reason about.

## 6. Deliverables

1. Replace the current hero-image delivery strategy with optimized assets for the homepage LCP image:
   - adopt the already-added modern-format asset in `UI/public/images/` as the primary homepage image source
   - keep a compatible fallback only if the chosen Angular image setup still needs one
   - preserve `priority` on the final LCP candidate
   - add responsive variants only if the implemented layout genuinely benefits from them
2. Update homepage hero image usage and any related tests so the optimized asset path is the canonical source in `UI/src/app/features/home/home-hero/`.
3. Convert the Hanken Grotesk font assets from TTF delivery to the already-added WOFF2 files and update `UI/src/styles/_fonts.scss` accordingly.
4. Narrow the font payload where practical during the same slice:
   - confirm which weights are actually used in current UI
   - decide whether the current full `100 900` variable range is still required
   - preload only the face or faces needed for first paint if preload is added
5. Preserve the already-addressed route improvement by keeping `under-construction` lazy-loaded and documenting the route-loading expectation for future non-primary routes.
6. Tighten bundle guardrails in `UI/angular.json` and/or UI CI:
   - add at least one JS-specific budget such as `allScript` or `anyScript`
   - add a lightweight asset-size check for tracked files under `UI/public/images/` and `UI/public/fonts/`
   - keep the check understandable enough that contributors can maintain the thresholds
7. Validate and then fix the localized asset-path strategy:
   - confirm the intended production hosting contract for localized assets
   - if the current root-relative asset strategy is unsafe, update asset references or build/deploy configuration so localized assets resolve correctly
   - verify the chosen approach against a production-like localized build, not only `ng serve`
8. Reassess the low-priority theme-icon implementation:
   - remove `translateZ(0)` if profiling or review confirms it is unnecessary
   - replace external SVG masks with reusable inline SVG assets/components under `UI/src/design-system/icons/`
   - update icon consumers so colour is controlled directly through CSS rather than via background-mask styling
9. Preserve the inline theme boot script as a minimal early-execution snippet unless another change proves it can stay flash-free with less parser-blocking work.
10. Run the normal UI validation commands required by the touched implementation slice, including `npm run format`, `npm run lint`, `npm run test:run`, and `npm run validate:i18n` when localization-sensitive asset or build-path changes are made.

## 7. Technology Requirements and Decisions

- Hero image strategy:
  Prefer modern compressed image formats for the LCP image, with a targeted optimization pass rather than a broad media-pipeline rollout.
- Font conversion strategy:
  Treat WOFF2 as the target delivery format and use the already-added files in `UI/public/fonts/`. Do not introduce a new conversion dependency or repo-owned conversion workflow in this slice unless a blocker is discovered.
- Icon strategy:
  Replace reusable theme icons with inline SVG assets/components under `UI/src/design-system/icons/` so consumers can set icon colour directly in CSS without mask-based rendering.
- Route-loading strategy:
  Keep the homepage and root shell eager, but treat secondary feature routes as lazy by default going forward.
- Guardrail strategy:
  Reuse Angular budgets plus a lightweight repository-owned asset-size check rather than introducing a heavyweight third-party performance tool unless later growth justifies it.
- Localized asset-path strategy:
  Do not quietly assume the current root-relative URLs are safe; the implemented fix must match the actual deploy/base-href contract.
- Theme boot strategy:
  Preserve the current early theme-application behavior unless measurements show a clearly better alternative that still avoids flash-of-incorrect-theme regressions.

## 8. Dependencies and Sequencing

1. Confirm the current payload baseline and the real hosting/path contract so the image, font, and localized-asset work all target the right constraints.
2. Optimize the homepage hero image first because it is the clearest critical-path win and has minimal coupling to the other findings.
3. Convert fonts to the prepared WOFF2 assets next, because the font files are also currently on the first-render path and may influence preload decisions.
4. Lock in route-loading guidance and any route-level cleanup after the asset changes, because that slice is already partially complete.
5. Add budgets and CI asset-size checks once the optimized assets exist, so thresholds can be calibrated against the improved baseline rather than the current oversized files.
6. Validate and fix the localized asset-path behavior before closing the plan, because it affects fonts, images, and icons together.
7. Finish by deciding whether the low-priority theme-icon and inline-theme-script items need code changes or should remain intentionally minimal with explicit validation notes.
8. Implement the icon remediation after the localized asset-path work so any remaining path concerns are resolved before consumers are switched to inline design-system assets.

### Suggested Implementation Slices

1. Homepage media optimization:
   - optimize hero asset delivery
   - update hero usage/tests
   - status: delivered on 2026-07-06 using `UI/public/images/hero-child-playing.avif` as the canonical priority hero source, with homepage tests updated accordingly
2. Font delivery modernization:
   - introduce WOFF2 assets
   - update font-face declarations
   - add any justified preload changes
3. Bundle and route guardrails:
   - preserve current lazy-route pattern
   - add JS budgets
   - add asset-size CI checks
4. Localized asset-path validation and cleanup:
   - confirm hosting contract
   - fix asset references or deploy configuration
   - verify localized build behavior
5. Low-priority theme follow-up:
   - remove unnecessary `translateZ(0)` if justified
   - move reusable theme icons into `UI/src/design-system/icons/` as inline SVGs
   - keep the inline theme script minimal and documented

## 9. Risks and Mitigations

- Risk: Image optimization changes visual quality or composition enough to undermine the hero artwork.
  - Mitigation: compare the optimized asset against the current PNG at the rendered size and accept only a visually equivalent replacement.
- Risk: Font conversion introduces missing glyphs, incorrect weight behavior, or italic regressions.
  - Mitigation: verify the current glyph and weight needs before narrowing the font payload, and prefer a conservative full-glyph WOFF2 conversion if subsetting confidence is low.
- Risk: Adding a new font-conversion or asset-checking dependency would change the repo's tooling surface unexpectedly.
  - Mitigation: use the already-added delivery assets and prefer lightweight repo-owned checks before adding new packages.
- Risk: Converting icon delivery from masked background assets to inline SVGs could accidentally change sizing, alignment, or theming behaviour in existing controls.
  - Mitigation: centralize reusable icon markup under `UI/src/design-system/icons/`, update consumers incrementally, and verify visual states for theme controls after the swap.
- Risk: JS or asset-size thresholds become noisy and block useful work.
  - Mitigation: calibrate thresholds against the optimized baseline and keep the checks limited to the most meaningful bundles and assets.
- Risk: Changing root-relative asset references could break current production hosting if the deploy contract already depends on them.
  - Mitigation: validate the hosting model first and implement the path fix together with localized build verification.
- Risk: Over-optimizing the theme toggle or early theme script yields negligible wins while adding complexity.
  - Mitigation: keep those items low priority and only change them when a clear simplification or measured benefit exists.

## 10. Unknowns and Required Clarifications

- No blocking clarification is required to draft this plan.
- The font-delivery input is now settled for this slice because the WOFF2 assets already exist in `UI/public/fonts/`; only the runtime wiring and any preload narrowing remain to be validated during implementation.
- If implementation shows the deploy environment already rewrites root-relative asset URLs safely for localized builds, document that evidence and downgrade the asset-path item rather than forcing an unnecessary code change.
- If the team wants aggressive font subsetting rather than straightforward TTF-to-WOFF2 conversion, confirm the supported locale/glyph range before implementation narrows coverage.

## 11. Completion Checklist

- [x] Issue 1 open finding is resolved by replacing the current priority PNG delivery with a materially smaller production-ready hero asset strategy.
- [ ] Issue 2 open finding is resolved by serving WOFF2 font assets instead of TTF and keeping first-paint font loading intentional.
- [x] Issue 3 remains explicitly tracked as `Partially Resolved`, with `under-construction` already lazy-loaded and future secondary routes expected to follow the same pattern.
- [ ] Route-loading guidance or tests are updated enough that the current lazy-loading improvement is preserved.
- [ ] Issue 4 open finding is resolved by adding meaningful JS bundle guardrails plus a lightweight static-asset size check.
- [ ] Issue 5 open finding is resolved by validating and, if needed, correcting localized asset URL behavior under the deployed base-href strategy.
- [ ] Issue 6 open finding is resolved by replacing masked external theme icon assets with reusable inline SVG design-system icons and direct CSS colour control.
- [ ] Issue 7 open finding is either left intentionally minimal with explicit validation notes or improved without reintroducing a theme flash.
- [ ] Any image, font, route, or build-configuration tests affected by the remediation are updated and passing.
- [ ] `npm run format` has been run in `UI/` if implementation edits UI files.
- [ ] `npm run lint` has been run in `UI/`.
- [ ] `npm run test:run` has been run in `UI/` when component, route, or service behavior changes.
- [ ] `npm run validate:i18n` has been run in `UI/` when localized asset or build-path behavior changes.
