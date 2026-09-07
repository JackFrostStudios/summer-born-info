# Panel

This folder owns the shared Angular `sbi-panel` component and its canonical raised placeholder-panel styling.

## Purpose

Use `sbi-panel` when multiple features need the same elevated gradient panel shell with a decorative media area
and stacked supporting content, such as route-level placeholder or status pages.

## Supported Public Contract

### Selector And Import

- selector: `sbi-panel`
- import: `import { Panel } from '@design-system/panel'`

### Inputs

- `$mediaWidth`: `'default' | 'compact'`
  Controls the wide-screen media column width. Use `compact` when the decorative media should occupy a
  slightly narrower column, as in the current not-found page treatment.

### Content Projection

- Mark the decorative media element with the `panelMedia` attribute so it projects into the dedicated media
  slot.
- Project the rest of the heading, copy, and action content as normal child content. The component supplies
  the shared stacked content wrapper and the responsive mobile-to-two-column layout shell.

## Accessibility And Behavior Expectations

- `sbi-panel` is a non-interactive structural component. Keep page landmarks, heading levels, and
  `aria-labelledby` ownership in the feature route that uses it.
- Decorative artwork in the `panelMedia` slot should usually remain `aria-hidden="true"` unless a feature has a
  real informative reason to expose it.
- Keep interactive controls such as buttons inside the projected content area so native semantics stay with the
  owning component.

## Styling Contract

The supported styling contract is intentionally narrow:

- Supported: choose the documented media-width option through `$mediaWidth`.
- Supported: provide decorative media through `panelMedia` and feature-owned textual or interactive content
  through regular projection.
- Supported: add feature-local styles to the projected media or projected content elements when the content
  itself is route-specific.
- Not supported: depending on `.sbi-panel`, `.sbi-panel__media`, `.sbi-panel__content`, or the compact class as
  a consumer API beyond the documented component inputs and projection pattern.
- Not supported: overriding the shared gradient, border, radius, or shadow recipe from feature code to create
  one-off panel variants.
- Not supported: importing from the implementation file [`panel.ts`](./panel.ts) instead of the folder public API
  in [`index.ts`](./index.ts).

## Current Limitations

- The component is intentionally scoped to the repeated placeholder/status panel pattern rather than every
  elevated surface in the app.
- The homepage hero art remains feature-specific because it is a media-first `figure` treatment with its own
  accent-surface gradient and image semantics, not a status/placeholder panel with decorative media plus shared
  stacked copy.
