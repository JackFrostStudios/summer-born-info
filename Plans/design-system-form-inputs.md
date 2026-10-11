# Design-system form inputs

## 1. Overview

Status: Complete. All five implementation steps, completion checks, and independent peer review finished.

Extract the labels, native inputs, field feedback, and field styling from admin login into `UI/src/design-system/forms/`, with `text-input` and `password-input` components. Each accepts an Angular Signal Forms string field and automatically presents its configured validation messages. Each consuming form retains ownership of its model, schema, validators, localized messages, submission, and form-level errors.

Preserve login appearance and interaction. Extend the shared feedback rendering to show all supplied messages when several errors exist; login currently supplies one message per field.

## 2. Source and existing plan context

- Source: the user's request to refactor [admin-login.html](../UI/src/app/features/admin/login/admin-login.html) into a design-system forms section.
- Follow-up to [Admin login Signal Forms migration](admin-login-signal-forms.md), including the completed presentation-state simplification; this is a separate slice.
- Guidance: [UI instructions](../UI/AGENTS.md), [UI project guide](../UI/AI_PROJECT_GUIDE.md), [design-system contract](../UI/src/design-system/README.md), and [shared styling guidance](../UI/src/styles/README.md).
- Confirmed baseline: login already owns `$model`, `$form`, localized schema errors, Angular `submit()`, root authentication errors, unchanged-value retry clearing, and invalid-field/summary focus. Native controls use `FormField`; feedback uses `touched() && invalid()`.
- Installed `@angular/forms` is 22.0.2. Public declarations expose `FieldTree<string>`, `FormField`, and `focusBoundControl()`. No dependency change is required.
- Existing `@design-system/*` alias supports a forms barrel; lint disallows deeper implementation imports.
- Planning inspected uncommitted login, lint, localization, and migration-plan changes. Preserve that baseline and reconcile intervening user edits before implementation.

## 3. Scope

- Add standalone `TextInput` and `PasswordInput` in a new forms section.
- Accept a typed writable string field tree through a required signal input.
- Render accessible labels, native controls, and automatic field feedback.
- Move field styling to the components using existing tokens.
- Adopt both in login, preserving native types, autocomplete, IDs, copy, and submission behaviour.
- Document public contracts and ownership; add focused tests and preserve login regression/axe coverage.

## 4. Non-goals

- API, authentication, routes, session, return-URL, or submission-policy changes.
- Design-system-owned models, validators, error dictionaries, localization decisions, or submit handlers.
- New email-format/password validation, async validators, or copy.
- Password visibility toggles, strength meters, clear buttons, hints, placeholders, or additional input types.
- Other form migrations, a general form renderer, reactive-forms/CVA compatibility, or a visual redesign.
- Replacing the login panel/submit button or extending global native-input defaults.

## 5. Behaviour scenarios

| Starting state / action | Expected outcome |
| --- | --- |
| Render untouched invalid field | No feedback; `aria-invalid="false"`, preserving login timing. |
| Type into native input | Existing caller model and Angular dirty state update; password whitespace is retained. |
| Caller updates model | Native value updates without mirrored value state or manual input handlers. |
| Blur invalid field | Angular marks it touched; supplied nonempty messages appear; `aria-invalid="true"`; native description references the local error container. |
| Correct touched field | Feedback disappears and ARIA state updates. |
| Several errors exist | Render every supplied nonempty message in Angular order within one stable description container; never map error kinds to wording. |
| Error lacks a message | Keep Angular invalidity, omit blank feedback and dangling description IDs. Caller schema must provide meaningful messages; no fallback copy is invented. |
| Caller touches field or submits invalid form | Feedback appears without a separate show-errors input. Login blocks authentication and focuses its first invalid native input through `focusBoundControl()`. |
| Schema changes required/disabled/readonly | Internal `FormField` continues applying native attributes/state; no competing component state inputs. |
| Login shows authentication summary | Without local messages, native inputs describe the existing summary. Local message container takes precedence when visible, preserving current semantics. |
| Multiple controls/forms render | Unique caller-supplied IDs connect the correct label, input, and errors; IDs remain stable for SSR/hydration. |
| Text component renders login email | Native `type="email"`, autocomplete `username`; required-only Angular validation and form `novalidate` remain. No new email validator. |
| Password component renders login password | Native type always `password`, autocomplete `current-password`; masking and password-manager semantics remain. |
| Login submits, fails, edits, retries, succeeds | Pending/duplicate protection, credential retention/clearing, root error lifecycle, focus, and safe navigation remain unchanged. |

## 6. Deliverables

### A. Forms section and public contract

Create this structure:

```text
UI/src/design-system/forms/
  index.ts
  README.md
  _input-styles.scss
  text-input/
    text-input.ts
    text-input.html
    text-input.scss
    text-input.spec.ts
    README.md
  password-input/
    password-input.ts
    password-input.html
    password-input.scss
    password-input.spec.ts
    README.md
```

Export only `TextInput`, `PasswordInput`, and intentionally public types from `forms/index.ts`. Import through `@design-system/forms`; no nested public barrels or lint-boundary relaxation. Selectors: `sbi-text-input` and `sbi-password-input`. Use Angular standalone defaults, `input()` APIs, strict typing, and `$` signal prefixes; do not explicitly set `standalone: true` or `OnPush`.

| Input/content | Contract |
| --- | --- |
| `$field` | Required `input.required<FieldTree<string>>()`. Caller passes `$form.email`, not a raw `Signal<string>`, string value, or `$form.email()` state. |
| `$inputId` | Required string, nonempty and page-unique. Applied to native input, label `for`, and derived `${inputId}-error` container. Do not duplicate the ID on the host. |
| Projected content | Required visible label content only; place default `<ng-content />` inside native `<label>`. Caller retains localized markup; no nested labels or interactive label content. |
| `$autocomplete` | `input<string \| null>(null)`; forward caller token to native control. |
| `$ariaDescribedBy` | `input<string \| null>(null)`; external description IDs forwarded to native control when no local message container is visible. Otherwise use local container. Empty values omit attribute. |
| `$type`, text only | `input<'text' \| 'email'>('text')`; email preserves login semantics without adding validation. |

Password fixes native `type="password"`. No value outputs, model creation, validators/messages, submit handlers, or separate required/disabled/readonly inputs.

### B. Native binding, feedback, styling, and lint

- Bind `[formField]="$field()"` exactly once on each internal native input. Import `FormField` there; do not bind it to component host, implement `FormValueControl`, or create another form tree.
- Derive state from the passed node and visibility from `touched() && invalid()`. Prefer template-local state; use `$`-prefixed computed signals for useful derivations such as filtering message-bearing errors.
- Render all nonempty `error.message` values as text, never HTML, in Angular order. Use one stable `${inputId}-error` container around messages, avoiding repeated IDs and blank feedback.
- Native `aria-invalid` uses the same touched/invalid predicate. Native `aria-describedby` references actual rendered feedback, otherwise caller description IDs. Read this field's `errors()`, not aggregate root `errorSummary()`.
- Let Angular own value synchronization, blur/touch, dirty state, and schema metadata. Preserve native keyboard/focus/password behaviour.
- Share the field styling through a private `_input-styles.scss` mixin included by both components. Do not introduce inheritance or another Angular wrapper merely to share short templates; keep private DOM/classes/Sass private.
- Move existing field grid/gap, label weight, input surface/border/radius/size/padding, error typography, and input focus ring out of `admin-login.scss`. Host layout must fit the existing form grid. Explicitly reset label/error margins because feature descendant styles no longer reach the internal template.
- Use existing tokens and review disabled/readonly, light/dark, narrow-layout, and forced-colors focus states.
- Move the narrow Signal Forms template accessor allowlist to the new external templates. Keep only still-needed login allowances, such as `submitting`; do not globally disable `no-call-expression` or widen import exceptions.

### C. Login adoption and ownership docs

Targets: login `.ts`, `.html`, `.scss`, `UI/eslint.config.js`, and documentation below.

- Replace login's `FormField` import/registration with the two components from the forms barrel.
- Replace both label/input/error blocks and their template-local feedback state. Keep IDs `admin-login-email` and `admin-login-password`, native email type, and existing autocomplete tokens.
- Keep labels projected with original i18n meanings/IDs; leave localized validator messages in login schema.
- Pass `$errorMessage() ? 'admin-login-error' : null` as the external description for each component.
- Retain `$model`, `$form`, `$canSubmit`, `$errorMessage`, retry clearing, `onInvalid`, submission action, and root-summary focus in login.
- Remove migrated field styles only; retain page/panel/form layout, intro, summary, submit button, and summary/button focus styling.

Intended usage:

```html
<sbi-text-input
  [$field]="$form.email"
  [$inputId]="'admin-login-email'"
  [$type]="'email'"
  [$autocomplete]="'username'"
  [$ariaDescribedBy]="$errorMessage() ? 'admin-login-error' : null"
>
  <span i18n="Admin login email label|Label for the admin email address field@@adminLoginEmailLabel">
    Email address
  </span>
</sbi-text-input>

<sbi-password-input
  [$field]="$form.password"
  [$inputId]="'admin-login-password'"
  [$autocomplete]="'current-password'"
  [$ariaDescribedBy]="$errorMessage() ? 'admin-login-error' : null"
>
  <span i18n="Admin login password label|Label for the admin password field@@adminLoginPasswordLabel">
    Password
  </span>
</sbi-password-input>
```

Documentation changes:

- List Forms in `UI/src/design-system/README.md`; document API, caller-owned model/messages, accessibility, styling, and intentional limits in the new READMEs.
- Update `UI/AI_PROJECT_GUIDE.md` with forms ownership, public import, and native binding.
- Update the outdated minimum-form-baseline section in `UI/src/styles/README.md`: global rules remain reset-only; components own field presentation.
- Record the explicitly requested extraction as an exception to the usual two-consumer promotion threshold. No additional feature consumer is required.
- Refresh `UI/src/locale/messages.xlf` after template changes; retain message IDs and wording.

### D. Tests and validation

- Add colocated component tests using host forms with actual signals, `form()`, schema validators, and native events, rather than mocked field state.
- Cover input/model synchronization in both directions, touch/dirty state, label associations, quiet initial errors, blur/correction, several caller-defined messages, and absent messages.
- Cover required/disabled/readonly metadata, multiple instance IDs, external-description fallback/local-message precedence, and `focusBoundControl()` focusing internal native input.
- Prove default text/configured email semantics without added validators; password masking, exact whitespace preservation, and caller autocomplete.
- Preserve login tests for invalid submit/first-invalid focus, pending/duplicate submits, all auth outcomes, unchanged-value retry, edit-time error clearing, password clearing, and safe navigation. Add summary-description fallback assertions if absent.
- Replace removed `.admin-login__field-error` assertions in login unit/axe tests with native IDs, message content, and observable ARIA relationships; avoid depending on private shared classes.
- Retain login's browser axe state matrix in both colour modes. Add `forms/component.a11y-spec.ts` with existing helpers for both controls, multiple errors, and disabled/readonly states; avoid duplicating authentication scenarios.
- Review keyboard order, label-click focus, focus visibility, narrow layout, password-manager attributes, and forced-colors appearance. Inspect localized/SSR output for matching unique IDs/labels.

Run from `UI/` after implementation:

1. `npm run format`
2. `npm run lint`
3. `npm run extract:i18n`, then review generated changes
4. `npm run test:run`
5. `npm run test:a11y`
6. `npm run validate:i18n` after reviewed localization changes are in the appropriate checkpoint

`validate:i18n` compares extraction against Git and runs localized production build/SSR asset checks. Existing uncommitted XLF changes can fail the drift check: do not discard them. If intentional changes remain uncommitted, verify repeated extraction is identical and run `npm run build:localize` and `npm run check:localized-ssr-assets`, reporting that precise limitation. Check existing build budgets; no budget increase is authorized here. Report unavailable browser prerequisites rather than claiming axe passed.

## 7. Technology requirements and decisions

- Existing Angular 22 public Signal Forms APIs and native inputs; no package, infrastructure, service, or version change.
- Pass `FieldTree<string>` through a signal input: a bare value signal lacks validation/interaction state. Native `FormField` composition retains binding/focus without a redundant custom-control adapter.
- Caller owns projected localized labels and schema-localized errors. Components know no login fields or error kinds.
- Preserve touched-only feedback and local-description precedence. Rendering multiple messages is the deliberate generalization; current login output stays equivalent.
- One public forms barrel, private component styles/DOM, and no global form primitive changes.
- References: [Angular form models](https://angular.dev/guide/forms/signals/models), [FieldTree API](https://angular.dev/api/forms/signals/FieldTree), [FormField API](https://angular.dev/api/forms/signals/FormField), and [field state](https://angular.dev/guide/forms/signals/field-state-management). Installed declarations/runtime govern if later docs differ.

## 8. Dependencies and sequencing

1. [x] Reconcile current login baseline and intervening user edits; retain migration contracts.
2. [x] Build both components, private styles, public barrel, READMEs, and focused tests.
3. [x] Adopt in login; remove migrated markup/styles/imports; update lint and ownership docs in the same compiling slice.
4. [x] Adapt login selectors, verify native focus/summary descriptions, and add shared axe coverage.
5. [x] Refresh localization, complete validation, review diff/build sizes, and update checklist.

No backend, route, or roadmap dependency blocks this slice. Existing schema messages, tokens, and test helpers suffice.

## 9. Risks and mitigations

- Lost binding/focus: use exactly one internal native `FormField`; test model updates, blur, and invalid-submit focus.
- Duplicate state: no component model/value mirror, validation, or submit path.
- Encapsulation styling gaps: move complete field styling including margin resets/focus; review both colour modes.
- Host-only labels/ARIA: connect IDs/descriptions explicitly to native input.
- Blank/duplicate feedback: filter missing messages, retain validity, use one stable container and caller-unique IDs.
- Localization drift: preserve IDs/schema copy; validate extraction/localized output without discarding existing work.
- Feature-class-dependent tests: assert IDs, ARIA, and messages instead.
- Scope growth: keep two string inputs and current login needs; defer adapters/additional features.

## 10. Unknowns and required clarifications

No blocking decision remains. Interpret "signal forms string value" as the existing writable string field-tree node so both value and errors are available. Retain login's touched-field feedback timing and require caller schemas to supply meaningful localized messages. The explicit design-system request authorizes this extraction despite the default two-consumer guideline.

Native focus and metadata behaviour require implementation verification against installed Angular; these are acceptance checks rather than open architecture decisions.

## 11. Completion checklist

- [x] Forms section contains both components with narrow public exports and documented APIs.
- [x] Typed string field inputs bind native controls through `FormField` without duplicate model/state.
- [x] Native labels, type/autocomplete, unique IDs, descriptions, focus, and schema metadata behave correctly.
- [x] Automatic touched-field feedback displays supplied messages, including multiple errors, with correct ARIA updates.
- [x] Login uses both components while retaining schema/copy/submission/retry/summary/navigation behaviour.
- [x] Shared styles own field presentation; login retains page/form/summary/button styling.
- [x] Ownership docs and narrow lint allowances reflect the forms section.
- [x] Component/login tests and browser axe checks pass; layout/focus review is recorded.
- [x] Localization and formatting/lint/unit/axe/localized-build/SSR checks pass or precise limitations are recorded.
- [x] Final diff respects existing work and contains no unrelated API, route, dependency, copy, or budget changes.

## 12. Execution record

- Step 1 — dedicated agent `baseline` audited and preserved the existing Signal Forms presentation simplification. No prerequisite fixes were needed. Formatting, lint, all 136 unit tests, stable i18n extraction, localized production build, and localized SSR asset checks passed. Existing total-script warning: 437.24 kB, below the 440 kB error budget. Full localization validation will run after its reviewed extraction checkpoint. Commit: `chore(forms): preserve verified login baseline (step 1)`.

- Step 2 — dedicated agent `forms_components` delivered both controls and real host-form tests, with architecture/test delegation. Review confirmed native binding, filtered text feedback, local-description precedence, and private styles. Formatting, lint, 22 focused tests, all 158 unit tests, production build, and full i18n validation passed. Localization remained unchanged; localized scripts retained the 437.24 kB warning. Browser axe/layout review remains step 4. Commit: `feat(forms): add native signal form inputs (step 2)`.

- Step 3 — dedicated agent `login_adoption` adopted the inputs, removed migrated field markup/styles, refreshed ownership docs, and minimally adapted old field-class assertions. Authentication/model/schema/submission logic and all 46 localized messages remain unchanged. To satisfy the unchanged 440 kB error budget, shared one private computed feedback helper (no model state) and simplified scoped CSS, relying on equivalent existing global font/text-colour reset declarations. Component templates now need no Signal Forms accessor exceptions; login permits only `submitting`. Bound literal inputs retain a narrow static-string lint exception because dollar-prefixed static attributes fail native SSR serialization. Formatting/check, lint, all 158 unit tests, all 26 existing browser axe tests, production build (439.57 kB scripts), localized build (439.99 kB), SSR asset checks, and stable repeated extraction passed. XLF changes are source locations only; full drift validation follows the checkpoint. Remaining script-budget headroom is extremely small; no budget was increased. Commit: `refactor(login): adopt shared form inputs (step 3)`.

- Step 4 — dedicated agent `accessibility_regressions` expanded login summary-description/local-feedback precedence assertions and added 13 shared browser cases. All 158 unit tests, all 39 browser accessibility tests, formatting/check, lint, production build, and full i18n/localized/SSR validation passed. Browser evidence: native label clicks and `focusBoundControl()` target internal controls; forward/reverse tab order is correct, disabled controls are skipped, readonly prevents edits, and password attributes/whitespace are retained. Both colour modes fit a 320 px viewport without overflow. Actual login browser review confirmed email → password → submit at desktop width; its narrow native inputs measured 244 px within 320 px document width. Focus rings were visible (3 px outline/offset) and uncut in light/dark and forced colours; screenshots showed readable label/error spacing without clipping. Ignored screenshots: `UI/test-results/forms-visual-review/`. No runtime code changed. Commit: `test(forms): verify accessibility and login regressions (step 4)`.

- Step 5 — dedicated agent `completion_audit` verified all checklist items and final scope with read-only architecture/test review. Full `validate:i18n` passed: all 46 messages retained, zero extraction drift, localized production build and SSR asset checks green. Localized prerender and two served login responses had five unique page IDs, no duplicate host ID, correct native label associations/projected labels, email/password/autocomplete/required/`novalidate` metadata, quiet initial ARIA/feedback, and stable IDs. Prior unchanged-code evidence remains 158 unit and 39 browser accessibility tests plus formatting/lint and production build. No API/routes/dependencies/copy/budget changes. Exact Angular localized script budget measurement before locale inlining is 439,993 bytes against 440,000 bytes (7 bytes headroom); delivered en-GB script files total 437,422 bytes. Existing warning remains; future runtime growth may hit the error budget. Commit: `docs(forms): record completed validation audit (step 5)`.

- Final peer review — fresh agent `peer_review` inspected `992db94..af87b27` for correctness, regressions, missing tests, plan drift, and maintainability. No actionable findings; native binding, feedback/ARIA, styling, login ownership, localization and coverage matched the plan. `git diff --check` passed. No review fixes or additional runtime validation were needed. All work is committed on `milestone-9-log-in`; script-budget headroom remains the recorded residual constraint.
