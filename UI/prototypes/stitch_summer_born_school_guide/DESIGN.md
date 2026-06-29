---
name: Academic Honesty
colors:
  surface: '#f7fafa'
  surface-dim: '#d7dadb'
  surface-bright: '#f7fafa'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f1f4f4'
  surface-container: '#ebeeef'
  surface-container-high: '#e6e9e9'
  surface-container-highest: '#e0e3e3'
  on-surface: '#181c1d'
  on-surface-variant: '#3e494a'
  inverse-surface: '#2d3132'
  inverse-on-surface: '#eef1f2'
  outline: '#6f797a'
  outline-variant: '#bec8ca'
  surface-tint: '#006972'
  primary: '#00535b'
  on-primary: '#ffffff'
  primary-container: '#006d77'
  on-primary-container: '#9becf7'
  inverse-primary: '#82d3de'
  secondary: '#236863'
  on-secondary: '#ffffff'
  secondary-container: '#a9ece5'
  on-secondary-container: '#286d67'
  tertiary: '#5d453e'
  on-tertiary: '#ffffff'
  tertiary-container: '#765d54'
  on-tertiary-container: '#fad8cd'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#9ff0fb'
  primary-fixed-dim: '#82d3de'
  on-primary-fixed: '#001f23'
  on-primary-fixed-variant: '#004f56'
  secondary-fixed: '#acefe7'
  secondary-fixed-dim: '#90d3cb'
  on-secondary-fixed: '#00201e'
  on-secondary-fixed-variant: '#00504b'
  tertiary-fixed: '#fddbd0'
  tertiary-fixed-dim: '#e0c0b5'
  on-tertiary-fixed: '#291711'
  on-tertiary-fixed-variant: '#58423a'
  background: '#f7fafa'
  on-background: '#181c1d'
  surface-variant: '#e0e3e3'
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
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  base: 8px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 48px
  container-max: 1280px
---

## Brand & Style

The design system adopts a **Soft Brutalist** aesthetic, balancing the structural integrity of educational institutions with the approachability required for student and educator engagement. It prioritizes clarity, hierarchy, and "structural honesty" by utilizing visible borders and grid-based layouts.

The brand personality is authoritative yet optimistic. By replacing the harsh blacks and whites of traditional Brutalism with a refined pastel palette and subtle corner radii, the UI evokes a sense of organized creativity. The goal is to feel dependable and institutional without being cold or intimidating.

## Colors

The palette centers on **Teal (#006D77)** as the primary anchor for headers, primary actions, and key brand moments. This is supported by a system of functional pastels used for section backgrounds and surface layering.

- **Primary:** Teal (#006D77) – Used for high-emphasis elements and navigation.
- **Secondary:** Soft Mint (#83C5BE) – Used for accents and secondary buttons.
- **Tertiary:** Pale Peach (#FFDDD2) – Used for highlights and warning-state backgrounds.
- **Surface Palette:** Light Azure and variant pastels are used to differentiate content blocks.
- **Typography & Borders:** Always use Neutral Black (#1A1A1A) to ensure WCAG AA/AAA compliance and maintain the "Brutalist" structural definition.

## Typography

**Hanken Grotesk** is the sole typeface for this design system. Its geometric purity aligns with the Brutalist structure while its contemporary finishing ensures legibility for educational content.

- **Headlines:** Use heavy weights (700-800) with tighter letter spacing to create high-impact "blocks" of text.
- **Body:** Use a generous line height (1.6) to ensure long-form educational reading remains comfortable.
- **Labels:** Use uppercase and bold weights for metadata and small UI triggers to ensure they hold their own against strong borders.

## Layout & Spacing

The design system utilizes a **Fixed Grid** philosophy. Content is housed within defined containers that use a strict 8px-based spacing rhythm.

- **Desktop:** 12-column grid with 24px gutters. Content should be boxed in defined containers with 2px borders.
- **Mobile:** 4-column grid with 16px margins.
- **Structure:** Spacing should feel "industrial." Instead of using whitespace to separate sections, use 2px solid borders. Padding within containers should be generous (typically 24px or 32px) to prevent the "Soft Brutalist" look from feeling cluttered.

## Elevation & Depth

In line with Neo-Brutalism, this design system rejects naturalistic shadows and blurs.

- **Hard Shadows:** Instead of blurs, use "Block Shadows." These are solid offsets of Neutral Black (#1A1A1A) at a 4px or 8px distance, usually placed at a 45-degree angle (bottom-right).
- **Outlines:** Every interactive element or distinct container must have a **2px solid border** (#1A1A1A).
- **Layering:** Depth is achieved by "stacking" cards. A secondary card may peek out from behind a primary card using a different pastel background color to indicate hierarchy.

## Shapes

To soften the inherent rigidity of the Brutalist grid, a **Soft (0.25rem / 4px)** corner radius is applied to all components.

- **Standard Radius:** 4px for buttons, input fields, and small cards.
- **Large Radius:** 8px for major container blocks or sections.
- **Interactive States:** When a component is hovered or pressed, the "Block Shadow" should decrease in offset, simulating the physical act of pushing the element into the page.

## Components

### Buttons

Primary buttons use the Teal background with White or high-contrast text and a 2px black border. They must feature a 4px solid black block shadow. On hover, the shadow disappears and the button "moves" 2px down and right.

### Cards

Cards are the primary vehicle for educational content. Use pastel backgrounds (Mint, Peach, Azure) to categorize different subjects or modules. Each card must have a 2px border. Use 8px block shadows for featured content.

### Input Fields

Inputs are stark white with 2px black borders. Labels should be `label-bold` and positioned outside the field, aligned to the top-left. Focused states use a thick 3px Teal border.

### Chips & Tags

Use secondary pastel colors with 1px black borders. These do not require shadows unless they are interactive (closable or filterable).

### Progress Indicators

Educational progress is shown via "Chunked Bars"—solid blocks of Teal separated by 2px black lines, rather than a smooth fluid gradient or fill. This reinforces the modular, structured nature of the design system.
