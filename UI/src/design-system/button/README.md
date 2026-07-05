# Button

This folder owns the shared Angular `sbi-button` component and its canonical styling.

## Purpose

Use `sbi-button` when a feature or shell consumer needs the shared application button treatment.
The component currently supports primary and secondary visual variants while preserving native button
semantics.

## Supported Public Contract

### Selector And Import

- selector: `sbi-button`
- import: import `Button` from `UI/src/design-system/button/button`

### Inputs

- `$buttonType`: `'primary' | 'secondary'`
  This controls the visual variant. Despite the current input name, this is a styling choice rather than the
  native HTML button `type`.
- `$disabled`: `boolean`
  Forwards to the native `disabled` attribute.
- `$ariaPressed`: `'true' | 'false' | null`
  Use for toggle-button state when the button represents a pressed/unpressed control.
- `$ariaLabel`: `string | null`
  Explicit accessible name. Takes precedence over `$ariaLabelledBy` when both are supplied.
- `$ariaLabelledBy`: `string | null`
  Accessible-name reference when the visible label lives outside the button.
- `$ariaDescribedBy`: `string | null`
  Accessible-description reference.
- `$testId`: `string | null`
  Testing hook forwarded to `data-testid`.

### Outputs

- `pressed`
  Re-emits the native click event when the button is enabled.

### Content

- Project visible button content with `<ng-content />`.
- Use an explicit accessible name input when projected content is not sufficient for screen readers, such as
  for an icon-only control.

## Accessibility And Behavior Expectations

- The component renders a native `<button>` element and currently fixes `type="button"`.
- Keyboard focus remains on the native button element.
- Disabled buttons do not emit the `pressed` output because native button behavior suppresses click
  interaction.
- Provide only one explicit accessible-name input at a time. If both `$ariaLabel` and `$ariaLabelledBy` are
  supplied, `aria-label` wins and development mode logs a warning.
- Use `$ariaPressed` only when the control represents a toggle state.

## Styling Contract

The supported styling contract is intentionally narrow:

- Supported: choose the documented visual variant through `$buttonType`.
- Supported: provide appropriate projected content and accessibility inputs.
- Not supported: overriding internal `--sbi-button-*` custom properties from feature or shell styles.
- Not supported: depending on `.sbi-button`, `.sbi-button--secondary`, or the internal DOM structure as a
  consumer API.

The following implementation hooks are internal-only and may change in a future slice without a consumer API
guarantee:

- `--sbi-button-*` custom properties used inside `button.scss` and `_button-styles.scss`
- `.sbi-button` and `.sbi-button--secondary`
- SCSS mixins in [`_button-styles.scss`](./_button-styles.scss)

## Current Limitations

- The public visual-variant input is currently named `$buttonType`; a later remediation slice is expected to
  rename this API to better separate visual variant from native button semantics.
- The component does not yet expose a public size, icon-only, or native submit/reset API.
- If a consumer needs a new sanctioned composition pattern, add it here as a documented contract instead of
  reaching for feature-level CSS overrides.
