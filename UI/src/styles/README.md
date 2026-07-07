# Shared Styling Foundation

This folder owns reusable global CSS for the Angular UI. Keep `UI/src/styles.scss` as the only
global stylesheet entry point and add shared concerns here when they are useful across features.

## Files

- `_fonts.scss` registers the local Hanken Grotesk variable font files served from `UI/public/fonts/`.
  The retained full-resolution source fonts live under `UI/full-resolution-assets/fonts/` and are not part of the runtime build.
- `_reset.scss` contains the modern reset and browser-normalisation rules.
- `_tokens.scss` defines design-system custom properties for colour, typography, spacing, borders,
  focus states, surfaces, shadows, and page width.
- `_base.scss` applies safe document-level defaults for typography, links, reset-level form
  controls, focus, and selection.
- `_primitives.scss` provides small reusable layout and surface classes prefixed with `sbi-`.

## Token Usage

Use custom properties instead of copying raw prototype values into feature styles. For example,
prefer `var(--sbi-color-primary)`, `var(--sbi-space-5)`, `var(--sbi-border-strong)`, and
`var(--sbi-shadow-hard)` over one-off colours, spacing, borders, or shadows.

Feature-specific layout and presentation should stay in component-scoped styles under
`UI/src/app/`. Add a new global token only when the value is part of the shared visual language and
is likely to be reused by more than one feature.

## Shared Governance

### Surface Stack

Choose neutral surface tokens by the job the layer is doing in the page, not by trying to climb the
whole scale one step at a time.

| Typical Usage | Preferred Token | Why |
| --- | --- | --- |
| Page canvas behind the whole app | `--sbi-color-background` | This is the document-level baseline and should stay the calmest neutral layer. |
| Shell chrome such as the sticky header or other app framing | `--sbi-color-surface` | Use the first neutral lift above the canvas for persistent app structure. |
| Section band or grouped page region | `--sbi-color-surface-low` or `--sbi-color-surface-container` | Start with `surface-low` for gentle grouping; step up to `surface-container` when the band needs clearer separation from the surrounding page. |
| Card or panel shell | `--sbi-color-surface-lowest` | Use this for the main neutral card layer when the content needs to read as its own object. |
| Inset container inside another neutral surface | `--sbi-color-surface-container` or `--sbi-color-surface-high` | Use these for nested neutral regions that need to read as intentional sub-sections rather than separate cards. |
| Strongest neutral separation in a stack | `--sbi-color-surface-highest` | Reserve this for the most pronounced neutral contrast, such as a highlighted inset or a dense layered stack that still should not switch to an accent colour. |

If a layer needs semantic emphasis rather than just neutral separation, switch to the accent or
status tokens instead of continuing up the neutral surface scale.

### Typography Exception Policy

The shared typography tokens are the default contract. Reach for feature-local typography only when a
component has a clear product reason that the current token set does not express cleanly.

- Use a local `clamp()` size when the treatment is intentionally responsive, belongs to one feature
  or one shell element, and would be misleading as a shared named token today. The public-header
  wordmark is the current example of this kind of exception.
- Use local non-zero letter spacing only for short-form brand, display, or label treatments where
  the spacing change is part of the visual identity rather than a general reading default.
- Introduce or extend a named token when the same type treatment appears in more than one place, is
  likely to become a reusable design-system pattern, or would otherwise force multiple features to
  copy the same `clamp()` or spacing recipe.
- Do not use feature-local typography to solve ordinary hierarchy or spacing problems that the
  existing size, weight, and line-height tokens already cover.

### Minimum Form Baseline

The application does not yet provide shared field primitives or form components. Global form-control
rules in `_base.scss` are intentionally limited to reset-level defaults so plain `input`, `select`,
and `textarea` elements remain readable without implying that labels, hint text, validation,
disabled messaging, spacing, or error presentation are already part of a shared form system.

When the first substantial form feature lands, it should define the real shared contract for field
structure and validation states rather than extending these base rules ad hoc.

### Interaction-State Model

The current design system documents the shared state model first and only promotes additional tokens
when a second real consumer needs the same values.

| State | Current Expectation |
| --- | --- |
| Default | Start from the component's resting visual treatment with clear text contrast and hit area. |
| Hover | Use hover for pointer affordance only; it may add emphasis or motion, but it should not be required to understand the control. |
| Active | Keep active feedback brief and tactile, usually as a pressed or reduced-offset variation of the hover/default state. |
| Focus-visible | Always provide a keyboard-visible indicator. The current shared contract is the global `:focus-visible` ring backed by `--sbi-focus-ring-width`, `--sbi-focus-ring-offset`, and `--sbi-color-primary`. |
| Disabled | Disabled controls should remove interactive affordance, keep readable contrast where possible, and avoid looking like selected or current state. Prefer native semantics first. |
| Selected/current | Use a persistent visual distinction that survives hover loss so users can tell what is active, chosen, or represents the current location. |

Today only focus-ring and shadow-offset values are shared as tokens. Hover motion, active motion,
disabled opacity, or selected-state recipes should stay component-owned until at least one more
interactive component needs the same contract.

### Naming Conventions

CSS custom properties and token names use `color` because they follow CSS terminology and align with
the property names contributors will already search for in stylesheets. App-facing theme language
uses `colour`, such as `data-sbi-colour-mode`, because the product copy and source locale follow
British English. The mixed spelling is intentional unless the project later chooses to standardize
on one vocabulary for both layers.

### Visual Quality Posture

The current UI quality baseline already includes automated accessibility and browser-level checks:
`npm run test:run` covers the Angular unit suite, and `npm run test:a11y` exercises the shared UI in
a real Chromium-based browser with the accessibility smoke coverage described in `UI/README.md`.

Dedicated visual regression tooling is intentionally deferred for now. Revisit that decision when
the UI has multiple stable shared components, light/dark or forced-colors regressions become a
meaningful recurring risk, or manual review of shared visual changes is no longer keeping pace with
the component surface area.

### Typography Tokens

| Token | Meaning | When To Use |
| --- | --- | --- |
| `--sbi-font-family-sans` | Default UI typeface stack. | Use for standard application copy and headings rather than introducing feature-specific font stacks. |
| `--sbi-font-size-0` | Small supporting text size. | Use for captions, labels, attribution, and other secondary text that still needs to remain readable. |
| `--sbi-font-size-1` | Base body text size. | Use for standard paragraph copy and default control text. |
| `--sbi-font-size-2` | Large body or intro text size. | Use to draw attention to key supporting copy, such as a lead paragraph or important explanatory text. |
| `--sbi-font-size-3` | Section heading size. | Use for prominent headings inside a page section or card. |
| `--sbi-font-size-4` | Large display heading size. | Use for major page headings that should anchor a section. |
| `--sbi-font-size-5` | Largest display heading size. | Use sparingly for hero-scale messaging or the single most prominent headline on a screen. |
| `--sbi-font-size-5-compact` | Reduced display heading size for constrained layouts. | Use when display text needs the same emphasis as `--sbi-font-size-5` but a smaller footprint on mobile or narrow containers. |
| `--sbi-font-weight-regular` | Default reading weight. | Use for body copy and long-form text where readability matters more than emphasis. |
| `--sbi-font-weight-bold` | Strong emphasis weight. | Use for labels, buttons, short emphasis, and text that needs to stand out without becoming the page focal point. |
| `--sbi-font-weight-extra-bold` | Highest emphasis weight. | Use for brand moments, large headings, or numbers that should immediately catch the user's eye. |
| `--sbi-line-height-tight` | Tight vertical rhythm. | Use on short display text where a compact silhouette is intentional. |
| `--sbi-line-height-heading` | Balanced heading line height. | Use for most headings to keep them readable while still visually compact. |
| `--sbi-line-height-body` | Comfortable paragraph line height. | Use for body copy, notes, and any multi-line content meant to be read carefully. |
| `--sbi-letter-spacing` | Default letter spacing. | Use as the standard spacing unless a specific component has a justified typographic reason to deviate. |

### Spacing Tokens

| Token | Meaning | When To Use |
| --- | --- | --- |
| `--sbi-space-1` | Tightest shared spacing step. | Use for very small gaps, icon-to-text separation, or compact badge padding. |
| `--sbi-space-2` | Small spacing step. | Use for closely related items, such as label-to-value or compact chip padding. |
| `--sbi-space-3` | Small-to-medium spacing step. | Use for grouped content that needs a little more breathing room without feeling loose. |
| `--sbi-space-4` | Baseline layout spacing. | Use for common gaps between controls, stacked copy, or card internals. |
| `--sbi-space-5` | Comfortable section-internal spacing. | Use when content blocks should feel clearly separated but still belong to the same surface. |
| `--sbi-space-6` | Large spacing step. | Use for card padding, larger component spacing, or stronger separation between content groups. |
| `--sbi-space-7` | Extra-large spacing step. | Use for major page sections or hero layouts that need deliberate openness. |
| `--sbi-space-8` | Largest shared spacing step. | Use sparingly for top-level page rhythm between major sections. |

### Layout Tokens

| Token | Meaning | When To Use |
| --- | --- | --- |
| `--sbi-size-container-max` | Maximum width for full-page content containers. | Use to keep wide layouts readable on large screens. |
| `--sbi-size-readable` | Maximum width for long-form reading content. | Use for paragraphs, notes, and any text-heavy block where shorter line lengths improve comprehension. |
| `--sbi-page-margin` | Responsive page edge spacing. | Use for outer page gutters instead of hard-coding horizontal padding. |
| `--sbi-grid-gutter` | Standard grid gap. | Use for multi-column layouts so cards and panels align with the rest of the system. |

### Shape, Border, And Focus Tokens

| Token | Meaning | When To Use |
| --- | --- | --- |
| `--sbi-radius-sm` | Small corner radius. | Use for small controls or subtle rounding where the shape should stay crisp. |
| `--sbi-radius-md` | Medium corner radius. | Use for inputs or components that need a little more softness than `--sbi-radius-sm`. |
| `--sbi-radius-lg` | Large shared corner radius. | Use for cards, buttons, panels, and other key surfaces in the current design language. |
| `--sbi-radius-pill` | Fully rounded pill radius. | Use for badges, chips, and rounded toggles where the silhouette should feel intentionally soft. |
| `--sbi-border-width` | Default thick border width. | Use when the border is a visible part of the visual identity, especially on cards and buttons. |
| `--sbi-border-width-thin` | Thin border width. | Use for dividers or lower-emphasis outlines. |
| `--sbi-shadow-offset` | Standard hard-shadow offset. | Use for small elevated elements that should feel tactile. |
| `--sbi-shadow-offset-lg` | Large hard-shadow offset. | Use for more prominent surfaces that need extra lift or a more playful visual punch. |
| `--sbi-focus-ring-width` | Focus outline thickness. | Use for keyboard-visible focus states rather than ad hoc outline sizes. |
| `--sbi-focus-ring-offset` | Spacing between focus outline and element edge. | Use to keep focus treatment clear and uncluttered. |
| `--sbi-border` | Default border recipe. | Use for standard outlined surfaces and controls. |
| `--sbi-border-strong` | Highest-emphasis border recipe. | Use when the outline itself helps establish hierarchy or draw attention to an important interactive surface. |
| `--sbi-border-subtle` | Lowest-emphasis border recipe. | Use for quiet separators and low-contrast container boundaries. |
| `--sbi-shadow-hard` | Standard hard shadow recipe. | Use for cards and buttons that should feel slightly raised and interactive. |
| `--sbi-shadow-hard-lg` | Larger hard shadow recipe. | Use when a key panel or hero element should stand out more strongly than surrounding content. |

### Colour Tokens

| Token | Meaning | When To Use |
| --- | --- | --- |
| `--sbi-color-background` | App-level page background. | Use for the canvas behind the main content area. |
| `--sbi-color-surface` | Default neutral surface tone. | Use for shell elements or large shared sections that sit on the page background. |
| `--sbi-color-surface-lowest` | Highest-contrast neutral surface. | Use for cards and panels that need to stand apart clearly from surrounding backgrounds. |
| `--sbi-color-surface-low` | Slightly raised neutral surface. | Use for grouped regions that need light separation without becoming the primary focal point. |
| `--sbi-color-surface-container` | Stronger neutral container surface. | Use for supporting panels, bands, and inset sections that should be visually distinct but still neutral. |
| `--sbi-color-surface-high` | Elevated neutral surface step. | Use when a neutral layer needs more contrast than `surface-container` but should not switch to an accent colour. |
| `--sbi-color-surface-highest` | Highest neutral elevation step. | Use sparingly for the strongest neutral separation in a stack of layered surfaces. |
| `--sbi-color-text` | Default foreground text colour. | Use for primary reading content and standard UI text. |
| `--sbi-color-text-muted` | Secondary text colour. | Use for supportive copy, metadata, and content that should recede slightly behind primary messaging. |
| `--sbi-color-inverse-surface` | Dark-on-light or light-on-dark reversed surface. | Use for inverse treatments such as banners or overlays where the standard neutral surface would not provide enough contrast. |
| `--sbi-color-inverse-text` | Foreground colour for inverse surfaces. | Use only on top of inverse surfaces to maintain contrast. |
| `--sbi-color-border` | Default border colour. | Use for standard outlines that should be visible but not dominant. |
| `--sbi-color-border-strong` | Strong border colour. | Use when the outline should help frame a surface or call attention to a control. |
| `--sbi-color-border-subtle` | Gentle border colour. | Use for quiet dividers and understated boundaries. |
| `--sbi-color-primary` | Core brand emphasis colour. | Use for links, heading accents, and moments where you want to draw the user's eye to important information or action. |
| `--sbi-color-on-primary` | Foreground on top of `--sbi-color-primary`. | Use only for text or icons placed directly on a primary-coloured background. |
| `--sbi-color-primary-container` | Strong primary-tinted surface. | Use for high-attention callouts, featured panels, or key actions that should stand out clearly from neutral content. |
| `--sbi-color-on-primary-container` | Foreground for primary containers. | Use only inside a `primary-container` surface. |
| `--sbi-color-secondary` | Supporting accent colour. | Use for secondary emphasis, alternative highlights, or moments that should feel distinct from the primary action colour. |
| `--sbi-color-secondary-container` | Secondary-tinted surface. | Use for supportive highlights, category callouts, or accent cards that need attention without outranking primary call-to-action areas. |
| `--sbi-color-on-secondary-container` | Foreground for secondary containers. | Use only inside a `secondary-container` surface. |
| `--sbi-color-tertiary-container` | Tertiary accent surface. | Use sparingly for a third emphasis lane when primary and secondary are already doing separate jobs. |
| `--sbi-color-on-tertiary-container` | Foreground for tertiary containers. | Use only inside a `tertiary-container` surface. |
| `--sbi-color-accent-warm` | Warm accent surface colour. | Use when content should feel welcoming, human, or gently highlighted rather than urgent or action-oriented. |
| `--sbi-color-on-accent-warm` | Foreground for warm accent surfaces. | Use only inside a warm accent surface. |
| `--sbi-color-error` | Error and destructive state colour. | Use for validation errors, critical failures, and destructive affordances that require immediate recognition. |
| `--sbi-color-on-error` | Foreground for error surfaces. | Use only on error-coloured backgrounds. |
| `--sbi-color-shadow` | Shared shadow colour. | Use through the shadow recipes rather than applying it directly. |

### Primitive Usage

| Primitive | Meaning | When To Use |
| --- | --- | --- |
| `.sbi-container` | Centers content and constrains maximum page width. | Use as the outer wrapper for page sections and shell content that should align to the shared layout gutter. |
| `.sbi-stack` | Vertical layout primitive with a configurable gap. | Use when items should flow top-to-bottom with consistent spacing, especially for copy blocks, forms, and card content. |
| `.sbi-cluster` | Horizontal wrapping group for related items. | Use for button rows, tag groups, metadata, or any small set of items that should stay visually associated and wrap gracefully. |
| `.sbi-grid` | Responsive card-style grid. | Use for collections of peer items such as cards, tiles, or summary panels. |
| `.sbi-label` | Small, bold overline-style text treatment. | Use for short labels that orient the user, such as section markers, card eyebrows, or status-like headings. |
| `.sbi-surface` | Default raised content container. | Use for standalone cards and panels that need a consistent shared shell. |
| `.sbi-surface--muted` | Lower-energy surface variation. | Use when a panel should be visually grouped but not compete with the main focal content. |
| `.sbi-surface--accent` | Secondary-accent surface variation. | Use for highlighted supporting content that should attract attention without becoming the primary call to action. |
| `.sbi-surface--primary` | Primary-accent surface variation. | Use for the most important callouts, actions, or key information blocks that should immediately catch the user's eye. |
| `.sbi-readable` | Readability width constraint. | Use on text-heavy blocks so line lengths stay comfortable. |

Shared button visuals are now owned by the Angular `sbi-button` component in `UI/src/design-system/button/` rather than by global primitive classes. Prefer that component for reusable application buttons instead of adding new global button selectors here.
