# Shared Styling Foundation

This folder owns reusable global CSS for the Angular UI. Keep `UI/src/styles.scss` as the only
global stylesheet entry point and add shared concerns here when they are useful across features.

## Files

- `_fonts.scss` registers the local Hanken Grotesk variable font files served from `UI/public/fonts/`.
- `_reset.scss` contains the modern reset and browser-normalisation rules.
- `_tokens.scss` defines design-system custom properties for colour, typography, spacing, borders,
  focus states, surfaces, shadows, and page width.
- `_base.scss` applies safe document-level defaults for typography, links, form controls, focus,
  and selection.
- `_primitives.scss` provides small reusable layout and surface classes prefixed with `sbi-`.

## Token Usage

Use custom properties instead of copying raw prototype values into feature styles. For example,
prefer `var(--sbi-color-primary)`, `var(--sbi-space-5)`, `var(--sbi-border-strong)`, and
`var(--sbi-shadow-hard)` over one-off colours, spacing, borders, or shadows.

Feature-specific layout and presentation should stay in component-scoped styles under
`UI/src/app/`. Add a new global token only when the value is part of the shared visual language and
is likely to be reused by more than one feature.

## Theme Readiness

The colour tokens use `light-dark()` so they can follow the operating-system colour preference.
The explicit light, dark, and system-default user control is a later milestone-8 step; do not add
mode persistence or UI controls in this foundation layer.

## Prototype Content

The approved prototype files are visual references only. Do not copy unsupported claims, invented
metrics, advocacy wording, sample calls to action, external font links, CDN scripts, or hosted
imagery from the prototypes into production Angular code.
