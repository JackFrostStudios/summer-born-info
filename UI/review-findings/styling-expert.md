# Styling Expert Findings

Scope: reviewed `UI/` only. Read `AI_PROJECT_GUIDE.md`, `UI/AGENTS.md`, `UI/AI_PROJECT_GUIDE.md`, and `UI/src/styles/README.md`; inspected all `.scss`, `.sass`, and `.css` files currently present under `UI/`.

## Strengths

- The global styling foundation is deliberately small and well organized: `src/styles.scss` composes fonts, reset, tokens, base rules, and primitives in a predictable order.
- Design tokens in `src/styles/_tokens.scss` cover the main reusable concerns: color, type, spacing, layout widths, radius, borders, shadows, and focus.
- Component styles are mostly component-owned and low-specificity. Most selectors are single-class BEM-style selectors, which keeps overrides understandable.
- Global primitives in `src/styles/_primitives.scss` are prefixed with `sbi-` and solve reusable layout/surface concerns without becoming a broad utility framework.
- The button design-system styles expose customization through CSS custom properties, which lets host components size or tune buttons without piercing the component internals.
- Reduced-motion and forced-colors cases are considered in core areas: reset motion handling, surface forced-colors fallback, button forced-colors fallback, and theme-control motion fallback.

## Redundant Styles

- `src/styles/_base.scss:60` globally constrains every `p` and `li` to `--sbi-size-readable`, while `.sbi-readable` in `src/styles/_primitives.scss:63` provides the same reusable reading-width concept explicitly. This duplication can create surprising behavior when paragraphs live in grids, cards, headers, footers, or future data-dense UI where text should fill its container. Prefer removing the global `p, li` width cap and applying `.sbi-readable` or a component-local max width only where long-form reading is intended.
- `src/app/features/home/home-hero/home-hero.scss:36` duplicates much of the `.sbi-surface` visual recipe already applied in `home-hero.html` via `class="home__hero-art sbi-surface"`: surface background, shadow, and rounded/bordered framing are split between global primitive and component CSS. Keep the primitive as the base surface and reserve the component class for feature-specific additions such as `overflow`, `margin`, image sizing, or a modifier class if this gradient/shadow combination becomes reusable.
- `src/app/features/under-construction/under-construction.scss:12` hand-builds a card surface with background, border, radius, and shadow instead of using `.sbi-surface` plus a feature modifier. If this is intentionally a one-off hero panel, it is acceptable; otherwise, consider adding `sbi-surface` in the template and only keeping the gradient, layout, and padding locally.
- Link underline recipes appear in both global anchors (`src/styles/_base.scss:65`) and the hero highlight (`src/app/features/home/home-hero/home-hero.scss:17`). If highlight-underlines become a repeated motif, a token or primitive for emphasis underline thickness/offset would keep the visual language consistent.

## Maintainability Issues

- The project uses raw viewport-driven `clamp()` expressions in component files for major spacing and type-related sizing, for example `public-header.scss:9`, `public-header.scss:17`, `under-construction.scss:9`, `under-construction.scss:23`, and `home-hero.scss:10`. The values are reasonable today, but repeated raw formulas make future tuning harder. Consider adding semantic tokens for shell padding, section block padding, and compact display heading size if these patterns repeat in new pages.
- `src/app/shell/public-header/public-header.scss:19` uses negative letter spacing even though the styling guidance and token set establish `--sbi-letter-spacing: 0`. This is a small visual exception, but it bypasses the token language. Either normalize to the token or add a clearly named brand-heading letter-spacing token if this is an intentional brand treatment.
- `src/app/shell/theme-control/theme-control.scss:20`, `:22`, `:35`, `:47`, and `:49` repeat the `1.5rem` icon size and use it inside transform math. Extracting a local custom property such as `--theme-control-icon-size` would make the reel geometry less fragile.
- `src/design-system/button/_button-styles.scss:15` hard-codes button padding as `0.75rem 1.25rem` rather than using spacing tokens. Using `var(--sbi-space-3) var(--sbi-space-5)` would align the default with the documented spacing scale while preserving the override hook.

## Specificity And Cascade Concerns

- `src/app/shell/theme-control/theme-control.scss:33` and `:39` include global `html[...] .theme-control__reel` selectors inside a component stylesheet, paired with `:host-context(...)`. Angular's emulated encapsulation can make global ancestor selectors in component styles harder to reason about, and the duplicated selector forms raise specificity. Prefer one scoped approach, ideally `:host-context(...)`, or put the root color-mode selector in an intentional global layer if cross-component theming selectors become common.
- `src/styles/_base.scss:24` sets `:focus { outline-color: transparent; }`. Since `:focus-visible` restores an outline at `:28`, keyboard users should still get a focus ring in modern browsers. The risk is browser or control-specific gaps where focus is visible but not matched by `:focus-visible`. Consider replacing this with a more targeted reset, or verify the supported browser matrix before relying on transparent focus outlines globally.
- No cascade layers are currently used. That is fine for the current small app, but as the UI grows, global reset/base/primitives/design-system/component styles may become harder to order mentally. A future `@layer reset, tokens, base, primitives, components` strategy would make cascade intent explicit without increasing selector specificity.

## Responsive Risks

- `src/app/shell/public-header/public-header.scss:1` lays out the brand and theme control in a non-wrapping flex row. The brand uses `font-size: clamp(1.5rem, 2vw + 1rem, 2rem)` at `:17`, and the theme control reserves `min-inline-size: 4.5rem` in `theme-control.scss:7`. On very narrow screens or with translated/longer brand text, the header can crowd or overflow. Add `min-inline-size: 0` and wrapping/truncation behavior for the brand, or allow the header to wrap with an intentional second-row layout.
- `src/app/features/home/home-hero/home-hero.scss:49` switches to a two-column layout at `48rem` with a second column minimum of `18rem` at `:51`. Combined with `gap: var(--sbi-space-7)` at `:8`, the layout has a fairly abrupt breakpoint. It is probably safe at 768px, but future copy or larger gutters could squeeze the first column. Consider a container-query or a slightly later breakpoint if hero content grows.
- `src/app/features/under-construction/under-construction.scss:70` similarly switches to two columns at `48rem` with a `12rem` to `16rem` icon column. The layout is clear, but the large icon plus `gap: var(--sbi-space-6)` can pressure translated heading/body text near the breakpoint. Test around 768px with longer localized strings.
- `src/app/features/under-construction/under-construction.scss:8` uses `min-block-size: min(68dvh, 48rem)`. This is modern and mostly good, but dynamic viewport units can behave differently around browser chrome. If vertical centering feels jumpy on mobile, consider whether the shell should own page-height rhythm instead of feature sections using viewport units independently.

## Browser And Performance Notes

- `src/styles/_tokens.scss:51` and related color tokens use `light-dark()` with preceding fallback declarations. That is a good progressive-enhancement pattern. Keep the plain fallback before each `light-dark()` value as long as older browser support matters.
- Masked icons in `theme-control.scss:59` and `under-construction.scss:32` are efficient and colorable, but external masks can fail silently if the asset path changes. Keep these covered by visual or DOM-level tests where possible.
- Current CSS volume is small, and selector complexity is low. There are no obvious CSS performance issues.

## Concrete Remediation Suggestions

1. Replace the global `p, li` max width in `src/styles/_base.scss:60` with explicit `.sbi-readable` usage in content-heavy templates.
2. Normalize reusable surface recipes by using `.sbi-surface` for the under-construction panel and keeping only feature-specific layout/gradient rules locally.
3. Refactor `theme-control.scss` to define local geometry variables for icon size and reel gap, then use those variables in size and transform calculations.
4. Decide whether the public-header negative letter spacing is a brand token or an exception; either add a token or use `var(--sbi-letter-spacing)`.
5. Simplify theme-control color-mode selectors to a single scoped strategy, or move root-level theme selectors into a deliberate global theming section.
6. Add responsive safeguards for the public header before localization increases text length: brand `min-inline-size: 0`, wrapping, or an explicit narrow-header layout.
7. Consider cascade layers once a second feature page or more design-system components are added; the current app does not need them urgently, but the foundation is ready for them.

## Validation

No source code was modified for this review. I did not run build, lint, or browser validation because this was a stylesheet architecture review only.
