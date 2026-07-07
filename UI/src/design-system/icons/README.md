# Icons

This folder owns the shared Angular `sbi-icon` component and the reusable inline SVG assets it exposes.

## Purpose

Use `sbi-icon` when more than one shell or feature consumer needs the same icon artwork and should be able to
style the icon colour directly with CSS. The component keeps the SVG markup inline so consumers do not depend on
external mask files or background-image delivery.

## Supported Public Contract

### Selector And Import

- selector: `sbi-icon`
- import: `import { Icon, type IconName } from '@design-system/icons'`

### Inputs

- `name`: `'builder' | 'moon-stars' | 'sun'`
  Selects which shared inline SVG to render.
- `label`: `string | null`
  Optional accessible name. Leave this unset for decorative icons so the component stays hidden from assistive
  technology.

## Accessibility And Behavior Expectations

- Decorative icons should omit `label`, which causes `sbi-icon` to render with `aria-hidden="true"`.
- Informative icons should provide `label`, which causes `sbi-icon` to expose `role="img"` with that accessible
  name.
- Consumers should keep interactive semantics on the owning control or feature component rather than on the icon.

## Styling Contract

- Supported: size the icon from consumer CSS by styling the `sbi-icon` host element.
- Supported: control the icon colour with the standard CSS `color` property; the inline SVG fills use
  `currentColor`.
- Supported: reuse the documented icon names through `name`.
- Not supported: depending on the component's internal `<svg>` markup shape as a consumer API.
- Not supported: importing from implementation paths such as `@design-system/icons/icon`.

## Current Limitations

- The current shared set is intentionally small and only includes the icons already reused by the theme control
  and placeholder/status routes.
- New shared artwork should be added here only when at least two consumers justify making it part of the
  design-system surface.
