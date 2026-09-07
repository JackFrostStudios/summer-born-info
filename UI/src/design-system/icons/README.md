# Icons

This folder owns the shared Angular `sbi-icon` base component and the reusable concrete inline SVG icon
components it supports.

## Purpose

Use the selector-specific icon components when more than one shell or feature consumer needs the same icon artwork
and should be able to style the icon colour directly with CSS. The components keep the SVG markup inline so
consumers do not depend on external mask files or background-image delivery.

Use `sbi-icon` directly only as the base accessibility and styling wrapper for projected icon artwork.

## Supported Public Contract

### Selector And Import

- base selector: `sbi-icon`
- concrete selectors: `sbi-builder-icon`, `sbi-moon-stars-icon`, `sbi-not-found-icon`, `sbi-sun-icon`
- import: `import { BuilderIcon, Icon, MoonStarsIcon, NotFoundIcon, SunIcon } from '@design-system/icons'`

### Inputs

- `$label`: `string | null`
  Optional accessible name on the base and concrete icon components. Leave this unset for decorative icons so the
  base icon stays hidden from assistive technology.

### Content Projection

- `sbi-icon` renders projected content with `ng-content`.
- Concrete icon components import `Icon` and project their own inline SVG artwork into it.

## Accessibility And Behavior Expectations

- Decorative icons should omit `$label`, which causes the base `sbi-icon` to render with `aria-hidden="true"`.
- Informative icons should provide `$label`, which causes the base `sbi-icon` to expose `role="img"` with that
  accessible name.
- Consumers should keep interactive semantics on the owning control or feature component rather than on the icon.

## Styling Contract

- Supported: size concrete icons from consumer CSS by styling the selector-specific icon host element.
- Supported: size projected base-icon artwork by styling the `sbi-icon` host element.
- Supported: control the icon colour with the standard CSS `color` property; the inline SVG fills use
  `currentColor`.
- Supported: use documented concrete selectors rather than passing icon-name strings.
- Not supported: depending on the component's internal `<svg>` markup shape as a consumer API.
- Not supported: importing from implementation paths such as `@design-system/icons/icon`.

## Current Limitations

- The current shared set is intentionally small and only includes the icons already reused by the theme control
  and placeholder/status routes.
- New shared artwork should be added here only when at least two consumers justify making it part of the
  design-system surface.
