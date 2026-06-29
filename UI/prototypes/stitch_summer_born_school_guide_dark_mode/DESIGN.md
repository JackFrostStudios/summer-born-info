---
name: Academic Honesty
colors:
  surface: '#131313'
  surface-dim: '#131313'
  surface-bright: '#393939'
  surface-container-lowest: '#0e0e0e'
  surface-container-low: '#1c1b1b'
  surface-container: '#1e2121'
  surface-container-high: '#282a2a'
  surface-container-highest: '#333535'
  on-surface: '#e1e3e3'
  on-surface-variant: '#bec8ca'
  inverse-surface: '#e1e3e3'
  inverse-on-surface: '#181c1d'
  outline: '#899394'
  outline-variant: '#3e494a'
  surface-tint: '#82d3de'
  primary: '#82d3de'
  on-primary: '#00363c'
  primary-container: '#004f56'
  on-primary-container: '#9becf7'
  inverse-primary: '#006972'
  secondary: '#90d3cb'
  on-secondary: '#ffffff'
  secondary-container: '#00504b'
  on-secondary-container: '#a9ece5'
  tertiary: '#e0c0b5'
  on-tertiary: '#ffffff'
  tertiary-container: '#5d453e'
  on-tertiary-container: '#fddbd0'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#9ff0fb'
  primary-fixed-dim: '#82d3de'
  on-primary-fixed: '#001f23'
  on-primary-fixed-variant: '#9ff0fb'
  secondary-fixed: '#acefe7'
  secondary-fixed-dim: '#90d3cb'
  on-secondary-fixed: '#00201e'
  on-secondary-fixed-variant: '#00504b'
  tertiary-fixed: '#fddbd0'
  tertiary-fixed-dim: '#e0c0b5'
  on-tertiary-fixed: '#291711'
  on-tertiary-fixed-variant: '#58423a'
  background: '#131313'
  on-background: '#e1e3e3'
  surface-variant: '#3e494a'
typography:
  headline-xl:
    fontFamily: Hanken Grotesk
    fontSize: 48px
    fontWeight: '800'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-xl-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '800'
    lineHeight: '1.2'
  headline-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '700'
    lineHeight: '1.2'
  headline-md:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '700'
    lineHeight: '1.3'
  body-lg:
    fontFamily: Hanken Grotesk
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Hanken Grotesk
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  label-bold:
    fontFamily: Hanken Grotesk
    fontSize: 14px
    fontWeight: '700'
    lineHeight: '1.0'
rounded:
  DEFAULT: 0.5rem
  lg: 0.5rem
  xl: 0.5rem
  full: 0.75rem
spacing:
  base: 8px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 48px
  container-max: 1280px
---

## Brand & Style

The design system adopts a **Soft Brutalist** dark-mode aesthetic. It keeps the visible borders,
rigid grid, and hard-offset shadows of the light-mode direction, but rebalances the palette around
charcoal surfaces and cool teal accents so the interface feels high-contrast, deliberate, and easy
to scan in a low-light presentation.

The personality remains authoritative and optimistic. The dark treatment should feel structured and
institutional without becoming harsh or glossy. Contrast comes from layered dark surfaces, pale
text, and bright teal emphasis rather than from pure black-and-white extremes.

## Colors

The palette is anchored by a dark neutral surface stack and a bright aqua-teal primary.

- **Background and page surface:** `background` and `surface` use `#131313`.
- **Primary accent:** `primary` uses `#82d3de` for brand emphasis, active links, and highlighted
  text.
- **Primary action fill:** `primary-container` uses `#004f56` with `on-primary-container`
  `#9becf7`.
- **Secondary accent:** `secondary` uses `#90d3cb`, with `secondary-container` at `#00504b`.
- **Tertiary accent:** muted warm beige-brown values are used through `tertiary`,
  `tertiary-container`, and their paired on-colors.
- **Text:** `on-surface` and `on-background` use `#e1e3e3`; supporting text uses
  `on-surface-variant` `#bec8ca`.
- **Borders and hard-shadow neutrals:** `outline` is `#899394`, while `outline-variant` and the
  hard shadow tone use `#3e494a`.

## Typography

**Hanken Grotesk** is the sole typeface in the prototype. It is used across headlines, body copy,
labels, and navigation.

- **Headline XL:** `48px`, `800`, `1.1`, `-0.02em`
- **Headline XL mobile:** `32px`, `800`, `1.2`
- **Headline LG:** `32px`, `700`, `1.2`
- **Headline MD:** `24px`, `700`, `1.3`
- **Body LG:** `18px`, `400`, `1.6`
- **Body MD:** `16px`, `400`, `1.6`
- **Label Bold:** `14px`, `700`, `1.0`

## Layout & Spacing

The layout uses a fixed-width content shell with a strict spacing rhythm.

- **Container max width:** `1280px`
- **Desktop page margin:** `48px`
- **Mobile page margin:** `16px`
- **Gutter:** `24px`
- **Base spacing unit:** `8px`

The prototype uses boxed sections, strong card divisions, and large vertical spacing between major
bands rather than a minimal editorial flow.

## Elevation & Depth

Depth is created through visible borders and hard-cut offset shadows rather than blur.

- **Borders:** `2px solid #899394`
- **Standard hard shadow:** `4px 4px 0 0 #3e494a`
- **Large hard shadow:** `8px 8px 0 0 #3e494a`
- **Pressed/hovered interaction:** the shadow disappears and the element translates `4px` down and
  right

## Shapes

The prototype uses slightly softened corners, but less roundness than a soft editorial system.

- **Default radius:** `0.5rem`
- **Large radius:** `0.5rem`
- **Extra-large radius:** `0.5rem`
- **Full radius:** `0.75rem`

## Components

### Navigation

The top navigation uses `surface-container-lowest` on a sticky bar with a `2px` bottom border in
`outline` and a hard shadow in `#3e494a`. Brand text uses the `primary` accent. Supporting links
use `on-surface-variant` and brighten toward `primary` on hover.

### Buttons

The prototype uses two main button treatments:

- **Primary action:** `primary-container` background with `on-primary-container` text, `2px`
  border, and hard shadow
- **Secondary action:** dark surface fills such as `surface-container` or
  `surface-container-lowest` with `on-surface` text, the same border treatment, and the same hard
  shadow

Hover/press behavior removes the hard shadow and translates the button by `4px, 4px`.

### Cards

Cards are the main content containers. They use dark surface variants or accent containers, always
with `2px` borders and hard shadows. Featured cards use the larger `8px` shadow. Accent cards may
switch to `secondary-container` or `primary-container` while keeping high-contrast paired text.

### Inputs

Inputs use `surface-container-lowest` backgrounds, `2px` borders in `outline`, and `on-surface`
text. The prototype keeps the same corner radius as other interactive elements.

### Chips and Labels

Small labels and chips use high-contrast dark-accent fills such as `secondary-container`, with
uppercase or bold label styling from the shared `label-bold` token.

### Imagery and Highlight Panels

Image frames use the same border and shadow system as cards. Overlay badges and lower-third
callouts use darker surface tiers like `surface-container-highest` to remain legible over imagery.
