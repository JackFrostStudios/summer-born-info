# Design System Expert Review

Review scope: `UI/` only. I reviewed the required project guidance, global style foundation, shared Angular design-system component, theming service/control, feature styles, shell styles, usage sites, and available UI docs. No source code was modified.

## Strengths

- The UI already has a clear design-system spine: `UI/src/styles.scss` composes fonts, reset, tokens, base rules, and primitives in a predictable order.
- Token coverage is broader than the current app needs, which is healthy for early growth. `UI/src/styles/_tokens.scss:4` through `UI/src/styles/_tokens.scss:116` covers typography, spacing, layout, radii, borders, focus, semantic color roles, surface elevation, and composite recipes.
- The token documentation is unusually practical. `UI/src/styles/README.md:16` through `UI/src/styles/README.md:136` explains when to use each token and primitive, not just what values exist.
- Reusable primitives are intentionally small and prefixed. `UI/src/styles/_primitives.scss:2` through `UI/src/styles/_primitives.scss:64` defines layout, surface, label, and readable-width helpers without becoming a utility framework.
- Feature and shell styles mostly consume tokens instead of raw color values. The main raw hex usage found in `UI/src` is confined to token definitions in `UI/src/styles/_tokens.scss:50` through `UI/src/styles/_tokens.scss:109`.
- The shared `sbi-button` is backed by a real native button, projects content, forwards ARIA hooks, supports disabled state, and has focused tests. See `UI/src/design-system/button/button.html:1` and `UI/src/design-system/button/button.spec.ts`.
- Theming has a system/light/dark model rather than one-off dark overrides. `UI/src/app/shell/theme-control/theme-control.service.ts:4` through `UI/src/app/shell/theme-control/theme-control.service.ts:149` centralizes persistence, system preference sync, and root attribute application.
- High-contrast support has started in the right places, with forced-colors handling for shared surfaces and the button. See `UI/src/styles/_primitives.scss:67` and `UI/src/design-system/button/button.scss:16`.

## Highest-Impact Gaps

### 1. Component-level design tokens are undocumented and currently informal

`UI/src/design-system/button/_button-styles.scss:3` through `UI/src/design-system/button/_button-styles.scss:27` exposes many `--sbi-button-*` custom properties, but `UI/src/styles/README.md` only states that button visuals live in `UI/src/design-system/button/` at `UI/src/styles/README.md:136`. Consumers can override button size, padding, colors, focus outline, hover translation, and transition duration without a documented contract.

Impact: this creates a hidden API. As more components arrive, teams may copy private custom properties or override internals inconsistently, making visual governance harder than the current small codebase suggests.

Remediation:

- Add component-level documentation next to the component, such as `UI/src/design-system/button/README.md`, documenting public inputs, outputs, variants, accessibility expectations, and supported CSS custom properties.
- Classify component custom properties as public, internal, or deprecated before additional consumers depend on them.
- Add a short index from `UI/src/styles/README.md` to component docs so token docs and component docs form one discoverable system.

### 2. The `sbi-button` API uses `buttonType`, which undercuts design-system semantics

The public input is `buttonType` in `UI/src/design-system/button/button.ts:11`, while the type is `ButtonVariant` at `UI/src/design-system/button/button.ts:3`. In design-system terms, "type" often means native button `type` values (`button`, `submit`, `reset`), but here it means visual variant. The template hard-codes the native type to `button` at `UI/src/design-system/button/button.html:4`.

Impact: this naming will become more confusing once forms, links, submit actions, destructive actions, icon buttons, and loading states are introduced.

Remediation:

- Rename the conceptual API to `variant` in a planned breaking change, keeping `primary | secondary` as the initial values.
- Consider separately adding a native `type` input with a safe default of `button` when forms enter the product.
- Document when to use `primary` versus `secondary` in component docs, not just token docs.

### 3. Reusable panel/surface patterns are already reimplemented in feature styles

`UI/src/styles/_primitives.scss:36` through `UI/src/styles/_primitives.scss:60` defines `.sbi-surface` and surface modifiers. `UI/src/app/features/home/home-hero/home-hero.html:29` uses `.sbi-surface`, then `UI/src/app/features/home/home-hero/home-hero.scss:36` through `UI/src/app/features/home/home-hero/home-hero.scss:38` adds a gradient and stronger shadow. Separately, `UI/src/app/features/under-construction/under-construction.scss:12` through `UI/src/app/features/under-construction/under-construction.scss:24` recreates a raised panel with background gradient, strong border, radius, shadow, spacing, and padding instead of using or extending a system primitive.

Impact: these may be legitimate bespoke hero/placeholder treatments today, but they are also the first signs of "card drift". Similar page panels will likely diverge in shadow, gradient, padding, and responsive behavior.

Remediation:

- Decide whether gradient raised panels are a reusable primitive. If yes, add a documented surface modifier such as `.sbi-surface--hero` or a component-level panel primitive.
- If not reusable, document why these are feature-specific exceptions in comments or component docs.
- Prefer composing `.sbi-surface` plus modifiers in templates for repeated panel shells instead of restating border/radius/shadow recipes in feature SCSS.

### 4. Token naming mixes semantic roles with elevation-style surface names but lacks a decision model

The color scale includes semantic roles (`--sbi-color-primary`, `--sbi-color-error`), paired foreground roles (`--sbi-color-on-primary-container`), and Material-like elevation names (`--sbi-color-surface-lowest`, `--sbi-color-surface-highest`) in `UI/src/styles/_tokens.scss:49` through `UI/src/styles/_tokens.scss:109`. The README explains individual tokens at `UI/src/styles/README.md:88` through `UI/src/styles/README.md:119`, but it does not describe the hierarchy for choosing neutral surface levels.

Impact: as screens grow, contributors will know what each token means in isolation but not which surface level to pick for shell, card, inset panel, modal, table row, or page band. That typically leads to inconsistent layering.

Remediation:

- Add a short "surface stack" section to `UI/src/styles/README.md`: app canvas, shell, section band, card/panel, inset, active/selected, modal.
- Include examples using existing files, such as header surface usage in `UI/src/app/shell/public-header/public-header.scss:3` and feature panel usage in `UI/src/app/features/under-construction/under-construction.scss:14`.
- Consider adding aliases for product intent if elevation labels prove too abstract, for example `--sbi-color-page`, `--sbi-color-card`, or `--sbi-color-callout`.

## Medium Issues

### 5. Typography governance allows local responsive type outside the documented scale

Global heading styles use tokenized sizes in `UI/src/styles/_base.scss:46` through `UI/src/styles/_base.scss:57`, but the public header brand uses a local responsive clamp and negative letter spacing in `UI/src/app/shell/public-header/public-header.scss:17` through `UI/src/app/shell/public-header/public-header.scss:19`. The README states `--sbi-letter-spacing` is the default at `UI/src/styles/README.md:44`, but there is no guidance for brand exceptions.

Impact: one brand exception is fine. Undocumented exceptions become a second typography system once navigation, page titles, stats, and cards arrive.

Remediation:

- Either convert the header brand to documented type tokens or introduce a named brand/display token if this treatment is intentional.
- Add guidance for when non-token `clamp()` type or non-zero letter spacing is allowed.

### 6. Global base form styling exists before form primitives or component APIs

`UI/src/styles/_base.scss:75` through `UI/src/styles/_base.scss:89` applies default button/input/select/textarea color and form-control styling. Buttons now have an Angular component, but inputs, labels, help text, validation, errors, and field groups do not.

Impact: future forms may inherit partial styling without a supported field API, leading to inconsistent labels, error placement, disabled states, and spacing.

Remediation:

- Treat the base form rules as reset-level defaults only and document that they are not a full field system.
- When the first real form lands, define field primitives or components for label, control, hint, error, required state, disabled state, and validation state.
- Add error-token usage examples before validation UX spreads across feature CSS.

### 7. Interaction state tokens are not first-class yet

The tokens include focus width/offset and shadow offsets, but hover motion, transition duration, active state, disabled opacity/color, and selected/current state are embedded in `UI/src/design-system/button/_button-styles.scss:18` through `UI/src/design-system/button/_button-styles.scss:29`.

Impact: future components may invent separate motion and state behavior, especially links, cards, tabs, toggles, and navigation items.

Remediation:

- Add interaction tokens for motion duration/easing and common state layers only when a second component needs them.
- Before adding more interactive components, define a shared state model: default, hover, active, focus-visible, disabled, selected/current, loading.
- Add disabled-state styling to `sbi-button` intentionally rather than relying mostly on native disabled behavior plus inherited styles.

### 8. There is no design-system component index or barrel

The only shared component lives at `UI/src/design-system/button/`, and consumers import it directly, for example `UI/src/app/shell/theme-control/theme-control.ts:2`. There is no visible `UI/src/design-system/README.md`, package-style index, or contribution checklist for new shared components.

Impact: direct deep imports are manageable with one component, but the design-system surface will become harder to discover and govern as components are added.

Remediation:

- Add `UI/src/design-system/README.md` with ownership rules, component maturity levels, accessibility expectations, and import conventions.
- Consider a small barrel once there are multiple stable components, while avoiding premature abstraction for experimental pieces.

## Low Issues

### 9. Locale and naming use British `colour`, while most CSS/design-system terms use `color`

The theme attribute is `data-sbi-colour-mode` in `UI/src/styles/_tokens.scss:119` and `UI/src/app/shell/theme-control/theme-control.service.ts:10`. This is consistent with the app locale, but CSS token names use `color`, matching CSS terminology.

Impact: low. The mixed spelling is understandable, but contributors may search for only one spelling.

Remediation:

- Keep as-is if product language intentionally prefers British English.
- Mention the convention in design-system docs: CSS property/token names use `color`; app-facing mode names and attributes use `colour`.

### 10. Visual regression coverage is not represented in the current tooling

`UI/package.json:4` through `UI/package.json:20` includes build, lint, i18n, format, and unit-test scripts, but no Storybook, screenshot, or visual regression workflow.

Impact: low today because the UI is small. It becomes higher risk once the token system supports multiple pages, themes, and component variants.

Remediation:

- Do not add heavy tooling immediately unless the roadmap needs it.
- When a second or third shared component lands, consider lightweight component examples plus screenshot checks for light/dark and forced-colors-sensitive states.

## Concrete Next Steps

1. Add `UI/src/design-system/README.md` covering component ownership, maturity, accessibility requirements, import conventions, and documentation expectations.
2. Add `UI/src/design-system/button/README.md` documenting `sbi-button` inputs/outputs, variant guidance, ARIA expectations, disabled behavior, and public CSS custom properties.
3. Plan a small API cleanup from `buttonType` to `variant`, with a separate native `type` input when form usage appears.
4. Decide whether raised gradient panels are reusable. If yes, promote the repeated pattern from `home-hero` and `under-construction` into a documented primitive or component.
5. Extend `UI/src/styles/README.md` with a surface-level decision model and documented typography exception policy.
6. Before adding forms, define the minimum field primitive/component API and validation state model.

## Overall Assessment

The design-system foundation is in good shape for an early Angular UI: tokenized, documented, accessible-minded, and already used by feature code. The main risk is not visual inconsistency today; it is the next layer of growth. Component APIs, component-level tokens, surface patterns, and contribution governance need to become explicit before additional routes and shared components multiply the current informal patterns.
