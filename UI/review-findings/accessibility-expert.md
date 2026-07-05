# Accessibility Expert Findings

Review scope: `C:\Projects\summer-born-info\UI` only. No source code was modified.

Validation performed:
- Read `AI_PROJECT_GUIDE.md`, `UI/AGENTS.md`, and `UI/AI_PROJECT_GUIDE.md`.
- Inspected Angular templates, shell/feature components, design-system button, global styles, tokens, and related tests.
- Ran `npm run lint` from `UI`; it passed.
- Manually checked representative color-token contrast pairs; tested pairs were above WCAG AA thresholds in light and dark modes.

## Strengths

- The shell uses semantic landmarks with a real `<main>` around routed content in `UI/src/app/shell/root-shell/root-shell.html:4`.
- Primary actions are rendered as native `<button>` elements through the shared design-system component in `UI/src/design-system/button/button.html:1`, preserving keyboard and assistive technology behavior.
- The theme icon animation is hidden from assistive tech with `aria-hidden="true"` in `UI/src/app/shell/theme-control/theme-control.html:9`.
- Decorative imagery/icons are handled carefully: the under-construction illustration is hidden from AT in `UI/src/app/features/under-construction/under-construction.html:3`, while the homepage hero image has useful alt text in `UI/src/app/features/home/home-hero/home-hero.html:37`.
- Focus visibility is intentionally tokenized and applied globally in `UI/src/styles/_base.scss:28`, with a matching button-specific focus-visible rule in `UI/src/design-system/button/button.scss:6`.
- Reduced-motion support exists globally in `UI/src/styles/_reset.scss:48` and specifically for the theme control in `UI/src/app/shell/theme-control/theme-control.scss:69`.
- Existing tests assert several accessibility-adjacent invariants, including native button focusability in `UI/src/design-system/button/button.spec.ts:126`, labelled page regions in `UI/src/app/features/home/home.spec.ts:103`, and theme-control ARIA in `UI/src/app/shell/theme-control/theme-control.spec.ts:151`.

## High-Impact Issues

### 1. SPA route changes do not manage focus or announce page context

The CTA routes from the homepage to `/under-construction` in `UI/src/app/features/home/home-hero/home-hero.ts:16`, and the back button may navigate to `/` in `UI/src/app/features/under-construction/under-construction.ts:25`. The shell renders routed content inside `<main>` at `UI/src/app/shell/root-shell/root-shell.html:4`, but there is no route-change focus management, title update, or live announcement.

Impact: after keyboard activation, focus can remain on the removed/previous control or land unpredictably. Screen reader users may not be told that the main content changed, especially because this is an SPA route transition rather than a full document load. This affects WCAG 2.4.3 Focus Order, 2.4.2 Page Titled, and the practical discoverability of new routed content.

Remediation:
- Add a route-change accessibility service or shell-level listener that updates `document.title` and moves focus to the new page `<h1>` or to a focusable `<main tabindex="-1">`.
- Prefer route metadata for title and focus target ownership so each feature route declares its page title.
- Add tests around navigation behavior: after route activation, assert `document.title` and `document.activeElement` point to the expected page heading/main region.

## Medium Issues

### 2. Theme toggle mixes `aria-pressed` with a changing action label

The theme control sets `aria-pressed` in `UI/src/app/shell/theme-control/theme-control.html:6`, while the accessible label changes between "Switch to dark mode" and "Switch to light mode" in `UI/src/app/shell/theme-control/theme-control.ts:18`.

Impact: ARIA toggle buttons are clearest when the accessible name is stable and `aria-pressed` communicates the state. A changing label plus `aria-pressed` can produce confusing announcements such as an action label paired with a pressed state.

Remediation:
- Use one of these patterns consistently:
  - Stable toggle: accessible name `Dark mode`, with `aria-pressed="true|false"`.
  - Action button: accessible name `Switch to dark mode/light mode`, without `aria-pressed`.
- Keep the visible/icon behavior unchanged, but update tests in `UI/src/app/shell/theme-control/theme-control.spec.ts` to assert the selected pattern.

### 3. Document title is static and not route-specific

The static document title is `SummerBornInfo` in `UI/src/index.html:5`. There is no route-specific title metadata in `UI/src/app/app.routes.ts:8`.

Impact: browser tabs, screen reader page summaries, history entries, and search results do not distinguish the homepage from the under-construction page. The title also omits word spacing, making it less readable when announced.

Remediation:
- Set a readable base title such as `Summer-born Info`.
- Add route titles, for example `Summer-born Info - Home` and `Summer-born Info - Page coming soon`, using Angular route `title` metadata or a small title strategy.

### 4. No skip link or equivalent bypass mechanism

The shell places the sticky header before `<main>` in `UI/src/app/shell/root-shell/root-shell.html:2`, but there is no skip link to the main content.

Impact: this is currently a small page, so the immediate burden is low. As navigation grows, keyboard and switch users will have to traverse repeated header controls before reaching page content on every route. This maps to WCAG 2.4.1 Bypass Blocks.

Remediation:
- Add a visually hidden "Skip to main content" link as the first focusable element in the shell.
- Give `<main>` an `id` and either make it focusable with `tabindex="-1"` or route focus to the page `<h1>`.

### 5. Accessibility automation is not integrated

The project has lint and component tests, but no axe or equivalent automated accessibility checks were found in `UI/package.json` or tests. Existing tests cover useful semantics, but they do not catch contrast regressions, duplicate IDs, invalid ARIA combinations, or landmark/name issues.

Impact: regressions can slip in as the UI grows, especially around route shell composition, ARIA, and color token changes.

Remediation:
- Add focused automated a11y tests for the root shell, homepage, under-construction page, and theme control using an axe-compatible test utility.
- Keep manual checks for keyboard flow, zoom/reflow at 200% and 400%, forced colors, dark mode, and screen reader smoke testing because automation will not cover everything.

## Low Issues

### 6. Page language is less specific than the configured source locale

Angular is configured with `sourceLocale: "en-GB"` in `UI/angular.json:21`, but the document uses `<html lang="en">` in `UI/src/index.html:2`.

Impact: `en` is valid and much better than no language, but `en-GB` would better match the source locale and pronunciation expectations for UK-focused guidance.

Remediation:
- Change the static document language to `en-GB`, and ensure localized builds emit the correct locale-specific `lang` value if more locales are added.

### 7. Header brand is text only and not a home link

The header brand is rendered as a paragraph in `UI/src/app/shell/public-header/public-header.html:2`.

Impact: users commonly expect a site brand in the header to navigate home. This is not currently a WCAG failure because there is no visible claim that it is a link, but it may become a navigation expectation issue as pages grow.

Remediation:
- Consider making the brand a real home link once more routes exist.
- If made interactive, use a native `<a routerLink="/">` and preserve a clear focus state.

### 8. Shared button API permits conflicting accessible-name inputs

The shared button forwards both `aria-label` and `aria-labelledby` in `UI/src/design-system/button/button.html:7`. The test host even sets both at once in `UI/src/design-system/button/button.spec.ts:78`.

Impact: when both are present, `aria-labelledby` takes precedence in accessible name computation. This can surprise consumers and mask the intended label.

Remediation:
- Document that consumers should provide either visible/projection text, `ariaLabel`, or `ariaLabelledBy`, not multiple accessible-name sources.
- Consider a development-time assertion or test case that models one naming strategy at a time.

## Contrast And Reflow Notes

- Representative token contrast checks passed AA, including primary text on light surfaces, muted footer text, primary button text, and dark-mode equivalents.
- The UI uses responsive grid and container primitives in `UI/src/styles/_primitives.scss:1`, homepage layout in `UI/src/app/features/home/home-hero/home-hero.scss:5`, and under-construction layout in `UI/src/app/features/under-construction/under-construction.scss:5`.
- Recommended manual follow-up: verify at 200% browser zoom and 400%/320 CSS px width that the sticky header, theme toggle, hero CTA, footer attribution, and under-construction panel do not overlap or trap focus.

## Suggested Priority Order

1. Add route title and focus management.
2. Normalize the theme toggle ARIA pattern.
3. Add a skip link/main focus target.
4. Add axe-based component/page smoke tests.
5. Clean up low-risk metadata/API clarity items as nearby work.
