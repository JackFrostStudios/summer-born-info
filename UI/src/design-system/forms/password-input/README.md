# Password input

`sbi-password-input` presents a native masked password input, its projected label, and
the caller's Signal Forms validation messages.

```ts
import { PasswordInput } from '@design-system/forms';
```

## Supported inputs and content

- `$field`: required `FieldTree<string>` from the caller's writable form model.
- `$inputId`: required nonempty, page-unique native input ID, stable across SSR/hydration.
- `$autocomplete`: `string | null`, default `null`; caller supplies a password-manager
  token such as `current-password`.
- `$ariaDescribedBy`: `string | null`, default `null`; external IDs used when no local
  message container is visible. Blank values omit the attribute.
- Project visible localized label content only. It goes inside a native label, so
  nested labels and interactive label content are unsupported.

Native type is always `password`. There are no outputs, competing metadata inputs,
or component-owned model/schema.

## Behaviour and accessibility

`FormField` binds the internal input once, preserving exact password whitespace,
touch/dirty state, schema metadata and bound-control focus. Touched invalid fields
show all supplied nonempty messages as text in Angular order, with one
`${inputId}-error` description container. Local feedback takes description precedence;
missing messages leave no dangling local description. Native `aria-invalid` follows
touched/invalid state even when messages are absent.

See the [forms contract](../README.md) for caller ownership and focus expectations.

## Styling and limits

The component owns the field grid, label/input/error presentation and focus ring,
using existing tokens and the private shared Sass mixin. Consumer host layout and
caller-projected content remain supported; internal DOM/classes/Sass are private.
Light/dark tokens, narrow input sizing, disabled/readonly semantics and forced-colors
focus are supported.

Only writable string fields and the native password type are supported. Validation,
submission, localization, summaries, password visibility toggles, strength meters,
placeholders, hints and custom-control adapters remain outside this component.
