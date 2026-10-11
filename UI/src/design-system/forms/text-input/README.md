# Text input

`sbi-text-input` presents a native text or email input, its projected label, and the
caller's Signal Forms validation messages.

```ts
import { TextInput } from '@design-system/forms';
```

## Supported inputs and content

- `$field`: required `FieldTree<string>` from the caller's writable form model.
- `$inputId`: required nonempty, page-unique native input ID, stable across SSR/hydration.
- `$type`: `'text' | 'email'`, default `'text'`; selecting email does not add a validator.
- `$autocomplete`: `string | null`, default `null`; forwarded to the native input.
- `$ariaDescribedBy`: `string | null`, default `null`; external IDs used when no local
  message container is visible. Blank values omit the attribute.
- Project visible localized label content only. It goes inside a native label, so
  nested labels and interactive label content are unsupported.

There are no outputs, competing metadata inputs, or component-owned model/schema.

## Behaviour and accessibility

`FormField` binds the internal input once and owns native value synchronization,
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

Only writable string fields and native text/email types are supported. Validation,
submission, localization, summaries, placeholders, hints and custom-control adapters
remain outside this component.
