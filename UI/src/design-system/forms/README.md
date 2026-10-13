# Forms

This section owns the shared `sbi-text-input` and `sbi-password-input` string controls.
Import the components through `@design-system/forms`; their implementation paths, DOM,
classes, and private Sass mixin are not public APIs.

The requested extraction from admin login is an explicit exception to the usual
two-consumer promotion threshold. Each feature continues to own its model, schema,
validators, localized messages and labels, submission, and form-level errors.

## Public contract

| Input/content | Contract |
| --- | --- |
| `$field` | Required `FieldTree<string>` from Angular Signal Forms, for example `$form.email`. Pass the writable field node, not its state, a raw signal, or a string. |
| `$inputId` | Required nonempty, page-unique string. Keep it stable for SSR/hydration. The native input uses this ID, the label targets it, and local feedback uses `${inputId}-error`. Do not set the same ID on the host. |
| Projected content | Required visible label content. The component places it inside a native label. Keep localized text/markup in the caller; do not project a nested label or interactive content. |
| `$autocomplete` | `string \| null`, default `null`. Forwarded to the native input. Caller chooses the appropriate token. |
| `$ariaDescribedBy` | `string \| null`, default `null`. Caller-owned description IDs; blank values omit the attribute. Visible local feedback takes precedence. |
| `$type` | Text input only: `'text' \| 'email'`, default `'text'`. Password input always uses `password`. |

Neither component exposes outputs. Native `FormField` owns value synchronization,
touch/dirty state, required/disabled/readonly metadata, and bound-control focus.
The components do not create models, mirror values, validate, or submit.

## Usage

```ts
import { TextInput, PasswordInput } from '@design-system/forms';
```

```html
<sbi-text-input
  [$field]="$form.email"
  [$inputId]="'account-email'"
  [$type]="'email'"
  [$autocomplete]="'username'"
>
  <span>Email address</span>
</sbi-text-input>

<sbi-password-input
  [$field]="$form.password"
  [$inputId]="'account-password'"
  [$autocomplete]="'current-password'"
>
  <span>Password</span>
</sbi-password-input>
```

Mark labels for localization in the consuming template and supply localized validator
messages in its schema. The example does not add an email validator.

## Accessibility and feedback

Feedback becomes visible when this field is touched and invalid. Every nonempty
`error.message` appears as text in Angular error order inside one description container.
Whitespace-only or missing messages are omitted without inventing fallback copy.
Callers must supply meaningful schema messages. The input remains
`aria-invalid="true"` when touched and invalid even if no message is available.

`aria-describedby` points to the local container only while it is rendered and contains
messages; otherwise it forwards the external description IDs. External referenced
elements remain the caller's responsibility. No automatic live region is added.

The native label, keyboard interaction, password masking, and password-manager
attributes are preserved. Calling `$form.email().focusBoundControl()` focuses the
internal native control. Disabled state and readonly state come from the schema;
readonly controls remain focusable.

## Styling and limits

A private computed presentation helper derives feedback from the caller field without storing
another model or control state. Both controls retain their own native templates.

Both controls own the field grid, label, input, feedback, and focus styling using existing
tokens. Their host fits a form grid, and input width can shrink with narrow containers.
Disabled styling retains readable text; readonly uses the normal readable treatment.
Tokens provide light/dark colours, and forced-colors focus uses system colours. Native font
inheritance and default text colour use the existing global reset/base rules.

Consumers may lay out the component host and style their own projected label content.
Internal markup, CSS classes and `_input-styles.scss` are private and must not be overridden
or imported as styling hooks.

These components support writable Signal Forms string fields only. They do not provide
reactive-forms/CVA adapters, a form renderer, additional input types, hints, placeholders,
password visibility toggles, strength meters, clear buttons, or form-level summaries.

See [Text input](./text-input/README.md) and [Password input](./password-input/README.md).
