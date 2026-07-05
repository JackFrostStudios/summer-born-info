# UI Review Summary

Review date: 2026-07-02

Scope: `C:\Projects\summer-born-info\UI`

Specialist reports:

- `angular-expert.md`
- `front-end-architect.md`
- `styling-expert.md`
- `design-system-expert.md`
- `performance-expert.md`
- `test-expert.md`
- `accessibility-expert.md`

## Overall Assessment

The UI is in a strong early state. The project already has a modern Angular 22 setup, SSR/prerender support, strict TypeScript and template checking, colocated tests, a clean app/shell/feature split, and a useful Sass token/primitives foundation.

The highest-impact concerns are mostly preventative: make SSR hydration complete for the i18n-heavy app, improve route accessibility and route metadata, reduce critical asset payload, formalize the design-system API before it grows, and strengthen tests around browser/platform edge cases.

## Priority 1 - Fix Route Accessibility And Page Context

Issue: SPA route changes do not manage focus or announce new page context. Routes also lack titles, and the static document title is not route-specific.

Evidence:

- `UI/src/app/shell/root-shell/root-shell.html:4`
- `UI/src/app/app.routes.ts`
- `UI/src/index.html:5`
- `UI/src/app/features/home/home-hero/home-hero.ts:16`
- `UI/src/app/features/under-construction/under-construction.ts:25`

Impact: Keyboard and screen reader users can activate a route change without focus moving to the new content or the page title changing. This is the clearest user-facing accessibility gap.

Recommended action:

- Add route `title` metadata for each route.
- Add shell-level route focus management to focus `<main tabindex="-1">` or the new page `<h1>`.
- Add a skip link to main content.
- Add tests for `document.title` and `document.activeElement` after navigation.

## Priority 2 - Complete Angular SSR Hydration For I18n

Issue: `provideClientHydration()` is enabled without i18n hydration support or event replay.

Evidence:

- `UI/src/app/app.config.ts`
- i18n-heavy templates under `UI/src/app/features/home/` and `UI/src/app/shell/`

Impact: Angular can skip hydrating i18n blocks by default, reducing the value of SSR. Early user interactions, such as theme toggling or CTA clicks before hydration completes, can also be missed without event replay.

Recommended action:

- Configure `provideClientHydration(withI18nSupport(), withEventReplay())`.
- Add a focused SSR/hydration smoke check or manual verification note.

## Priority 3 - Reduce Critical Asset Payload

Issue: The above-the-fold hero image and font assets are heavy for the current page.

Evidence:

- `UI/public/images/hero-child-playing.png`
- `UI/src/app/features/home/home-hero/home-hero.html:30`
- `UI/src/styles/_fonts.scss`

Impact: The LCP image is intentionally priority-loaded but is a roughly 306 KB PNG in the existing output. The two TTF variable font files add roughly 262 KB uncompressed before compression.

Recommended action:

- Convert the hero image to AVIF/WebP and target roughly 40-100 KB at the displayed size.
- Convert fonts to subsetted WOFF2.
- Preload only the font faces needed for first paint.
- Add CI-visible checks for static asset sizes.

## Priority 4 - Make Routing Scalable

Issue: Secondary routes are eagerly imported, and there is no wildcard/not-found route.

Evidence:

- `UI/src/app/app.routes.ts`

Impact: Eager imports are harmless today, but this pattern will inflate the initial bundle as more features land. Unknown URLs also have no documented product behavior.

Recommended action:

- Keep the primary homepage eager if desired.
- Convert secondary feature routes, starting with `under-construction`, to `loadComponent`.
- Add a not-found feature and final `**` route with tests.

## Priority 5 - Formalize The Design-System Boundary

Issue: The shared button is useful, but its public API and styling contract are still informal.

Evidence:

- `UI/src/design-system/button/button.ts`
- `UI/src/design-system/button/button.html`
- `UI/src/design-system/button/_button-styles.scss`
- imports such as `@design-system/button/button`

Impact: Deep imports and undocumented component-level CSS custom properties can become a hidden API. `buttonType` also risks confusion with the native button `type` once forms arrive.

Recommended action:

- Add `UI/src/design-system/README.md`.
- Add `UI/src/design-system/button/README.md`.
- Add a button public export such as `UI/src/design-system/button/index.ts` and import via `@design-system/button`.
- Plan a rename from `buttonType` to `variant`.
- Later add a separate native `type` input with default `button`.

## Priority 6 - Tighten Styling Governance

Issue: The token system is strong, but repeated surface recipes, global text width rules, and local typography exceptions create drift risk.

Evidence:

- `UI/src/styles/_base.scss:60`
- `UI/src/styles/_primitives.scss`
- `UI/src/app/features/home/home-hero/home-hero.scss`
- `UI/src/app/features/under-construction/under-construction.scss`
- `UI/src/app/shell/public-header/public-header.scss`

Impact: Future pages may create one-off cards, typography rules, and responsive formulas faster than the docs can steer them.

Recommended action:

- Replace the global `p, li` readable-width cap with explicit `.sbi-readable` usage where needed.
- Decide whether raised gradient panels should become a documented primitive or remain deliberate feature exceptions.
- Add a surface stack section to `UI/src/styles/README.md`.
- Normalize or document the header brand typography and negative letter spacing.
- Add responsive safeguards for the public header before localization expands text.

## Priority 7 - Strengthen Tests Around Edge Cases And Semantics

Issue: The current tests pass and cover useful behavior, but the most valuable missing tests are around browser/platform resilience, route behavior, and accessibility-oriented assertions.

Evidence:

- `UI/src/app/shell/theme-control/theme-control.service.ts`
- `UI/src/app/shell/theme-control/theme-control.service.spec.ts`
- `UI/src/app/app.spec.ts`
- `UI/src/app/features/home/home.spec.ts`
- `UI/src/app/features/home/home-hero/home-hero.spec.ts`

Impact: SSR/browser guard behavior, malformed persisted theme values, storage failures, media-query listener cleanup, and unknown route behavior are not yet proven. Some tests assert classes or child counts where role/name assertions would catch more user-visible regressions.

Recommended action:

- Add `ThemeControlService` tests for malformed storage values, storage exceptions, SSR fallback, system-mode clearing, and listener cleanup.
- Add one rendered `ThemeControl` system-preference update test.
- Add route policy tests for unknown URLs.
- Gradually move key shell/page/button assertions toward role/name/heading semantics.
- Add axe smoke tests for shell, homepage, under-construction page, and theme control.

## Lower-Risk Cleanup

- Align `UI/src/index.html` language with `en-GB`.
- Remove `i18n-aria-labelledby` from fixed technical ID references such as `home-heading`.
- Expand template linting from `src/app/**/*.html` to `src/**/*.html` so design-system templates get the same rules.
- Document `/under-construction` as temporary with exit criteria.
- Document app-level browser preference ownership for services like theme control.
- Clarify localized deployment behavior for root-relative `/fonts`, `/images`, and `/icons` asset paths.

## Validation Notes From Review

- `npm run test:run` passed in the test review: 10 files, 38 tests.
- `npm run lint` passed in the accessibility review.
- No source-code implementation changes were made as part of this review; only markdown review artifacts were created.
