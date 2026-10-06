# Admin login Signal Forms migration

## 1. Overview

Status: Implementation in progress. Checklist items track delivered work.

Replace the custom form state in `UI/src/app/features/admin/login/admin-login.ts` with a typed Angular Signal Forms model. Angular must own field binding, validation results and messages, touched/dirty state, submission gating, pending state, and sign-in errors. Preserve the current authentication and navigation behaviour.

## 2. Source and existing context

- Source: the user's request to migrate the admin login to Angular's signal-based forms package.
- Existing delivery context: [Milestone 9 admin login](milestone-9-admin-log-in.md). This is a follow-up implementation slice, not a replacement for that plan.
- UI conventions: [UI guidance](../UI/AGENTS.md) and [UI project guide](../UI/AI_PROJECT_GUIDE.md).
- `UI/package.json` already declares `@angular/forms` at `^22.0.0`; the installed declarations and runtime inspected for this plan are version 22.0.2. No new dependency or Angular upgrade is needed.
- The current component implements individual value and touched signals, input/blur handlers, required checks, service-derived pending state, and a separate sign-in error signal. Existing tests cover field feedback, authentication outcomes, navigation, and duplicate submission.

## 3. Scope

- Migrate the login component and template to `@angular/forms/signals`.
- Keep one typed writable `$model` containing `{ email: string; password: string }`, and a `$form` field tree created with `form()`.
- Bind native inputs with `FormField`; derive visible errors and ARIA state from the field tree.
- Use Angular `submit()` for validation gating, marking fields touched, concurrent-submission protection, and `submitting()` state.
- Represent all four sign-in failure kinds as root submission errors with localized messages.
- Preserve same-credential retries and clearing the sign-in summary when either credential changes.
- Update existing component and accessibility coverage and localization artifacts as necessary.
- Adjust only the total-script production error budget to accommodate the measured Signal Forms runtime; retain existing warning and initial/per-script thresholds.

## 4. Non-goals

- API, auth service, route guard, session persistence, sign-out, or return-URL policy changes.
- Adding email-format validation, password policy, async validators, or new login copy. The current form only requires a nonblank trimmed email and a nonempty password; retain that contract.
- Visual redesign, shared form abstractions, or migration of other forms.
- Replacing the custom implementation with `FormGroup`, `FormControl`, or template-driven forms.

## 5. Behaviour scenarios

| Starting state / action | Expected outcome |
| --- | --- |
| Initial empty form | No visible errors; submit disabled; fields untouched. |
| Blur an empty field | Angular marks it touched; its required message appears with matching `aria-invalid` and `aria-describedby`. |
| Enter only whitespace for email | Email remains invalid with the existing required message. |
| Enter a nonblank email and nonempty password | Submit becomes available. Preserve acceptance of malformed-but-nonblank email; no new email-format rule. Whitespace in a password is preserved. |
| Dispatch submit with incomplete values, including keyboard/programmatic submission | Angular marks interactive fields touched, shows required errors, and prevents any authentication request. Focus the first invalid bound control. |
| Submit valid credentials | Pass trimmed email and unchanged password to `AdminAuthService.signIn`; Angular reports submitting for the full asynchronous action. |
| Submit again while pending | Exactly one authentication request; button disabled, `aria-busy=true`, and existing pending label displayed. |
| Receive `signed-in` | Clear password through the model and navigate to the existing safe return URL or `/admin`. |
| Receive `invalid-credentials` | Retain email, clear password before returning the root submission error, render the existing generic summary, and focus it after rendering. Required password feedback may also show because Angular touched the form on submit. |
| Receive `not-admin`, `network-failure`, or `unexpected-failure` | Retain both values, display the corresponding existing localized root error, focus the summary, and make retry available once submission completes. |
| Retry an unchanged recoverable failure | Clear the previous root submission error before Angular validates the retry; make a new request without requiring edits. A new failure replaces the previous summary. |
| Edit either credential after failure | Angular clears the root submission error; field validation remains reactive. |
| Already authenticated or unsafe return URL | Preserve current redirect behaviour and all existing return-URL regression cases. |

## 6. Deliverables

### A. Typed model, validators, and messages

Target: `UI/src/app/features/admin/login/admin-login.ts`.

- Create `$model` and `$form`; import `form`, `FormField`, `required`, `validate`, and `submit` from `@angular/forms/signals`.
- Use `required` for password. Use a schema `validate` rule for email that returns a `required` error when `value().trim().length === 0`; include `required` metadata for the native required attribute and avoid duplicate empty-email errors. One concrete arrangement is built-in `required` for empty email plus custom `required` error only for nonempty whitespace-only email.
- Put the existing localized required messages on validator errors using their current message IDs. Display `errors()` messages rather than a second template-owned validation/message system. Preserve existing localization meanings and IDs when moving text to `$localize`.
- Remove `$email`, `$password`, both touched signals, input-value extraction, input/blur handlers, and handwritten required checks. Remove `$error` and its separately stored error message state.
- Presentation-only computed signals are allowed, with the repository `$` prefix, but must read Angular field state rather than duplicate it.

### B. Angular submission and root error lifecycle

Target: `admin-login.ts` and `admin-login.html`.

- Configure the form's `submission.action` to await sign-in, apply existing password-clearing rules, perform successful navigation, and return `{ kind: result.kind, message: localizedMessage }` for failures. Errors belong to the root, not an individual credential, because the response does not identify a faulty field.
- Derive pending label, button busy state, and submission blocking from `$form().submitting()`, not the auth service's `$isSigningIn` signal. Keep service state intact for its other consumers.
- Use a thin native `(submit)` adapter that prevents the default event, exits if Angular reports submitting, clears any previous root submission error as described below, then delegates to Angular `submit($form)`. Configure `onInvalid` to focus the first invalid bound field. Keep `novalidate` on this manual integration.
- Retry clearing: if the root has a prior submission error, replace `$model` with a fresh object containing the same credential values before calling `submit`. The installed Angular runtime stores submission errors in a `linkedSignal` sourced from that node's value; changing the root object identity clears its submission errors without changing credential strings or resetting touched/dirty state. Add a regression test proving this behaviour. Do not use private Angular internals or reset the entire form to clear errors.
- Do not attach `FormRoot` alongside this adapter: it would create a second submission path. `FormRoot` is suitable for ordinary submission, but this slice needs error clearing before validation on unchanged retries. Angular `submit()` still owns the form lifecycle.
- Disable the submit button for `submitting()` or invalid credential fields. Do not disable it merely because a root server error makes the root invalid: the retry adapter clears that error before Angular validates. All local invalidity remains authoritative in Angular field state.
- Derive the summary from root `errors()` filtered to the four auth failure kinds. Focus the existing summary element once the returned error has entered Angular state and the view has rendered; use a view/render-safe mechanism, without storing a second error signal.
- Clear password before returning `invalid-credentials` so the programmatic model update cannot immediately erase the newly returned error. Ensure retries and later edits clear stale summaries as specified.

### C. Template, accessibility, and localization

Target: `UI/src/app/features/admin/login/admin-login.html`; `UI/src/locale/messages.xlf` if extraction changes.

- Add `FormField` to component imports and bind `[formField]="$form.email"` and `[formField]="$form.password"`. Remove manual `[value]`, `(input)`, and `(blur)` bindings.
- Gate field feedback on each field's `touched()` and `invalid()`; render messages from its `errors()` signal. Keep IDs stable and avoid duplicate IDs if rendering multiple messages.
- Preserve labels, input types, autocomplete, required metadata, summary alert semantics, keyboard operation, and focus visibility.
- Derive explicit ARIA bindings from the same Angular state used for visible messages. Ensure directive-provided attributes and template attributes agree.
- Preserve existing styles; edit `admin-login.scss` only if necessary for equivalent error rendering.

### D. Regression coverage and validation

Targets: existing `admin-login.spec.ts` and `component.a11y-spec.ts` beside the component.

- Retain current rendered interaction and routing tests; adjust only expectations intentionally changed by Angular's submit-time touching.
- Add whitespace-only email, untouched initial state, correction of touched errors, invalid native submit, unchanged-value retry for all recoverable errors, and summary clearing on edits to either field.
- Test pending state using an unresolved sign-in promise without manually toggling service pending state. Dispatch two submit events directly while pending to prove Angular's concurrency protection as well as disabled-button behaviour.
- Cover repeated failures, successful retry, password clearing order, absence of accidental navigation on failure, and retention of credential values.
- Extend existing axe coverage to visible required errors and submission errors in supported colour modes. Check focused error summary and first invalid field in behaviour tests.
- Use existing Angular/Vitest setup and browser accessibility helpers; no new test framework.

Run from `UI/`, in order: `npm run format`, `npm run lint`, `npm run extract:i18n`, `npm run test:run`, `npm run test:a11y`, `npm run validate:i18n`. Review and retain legitimate extraction changes before the drift check. `validate:i18n` checks the localized build path; report any unavailable browser prerequisite for axe rather than claiming it passed.

## 7. Technology requirements and decisions

- Use the existing Angular 22 dependency and public Signal Forms APIs. No package installation or version change.
- Preserve the required-only validation contract; input `type="email"` does not add Angular email validation when the native form uses `novalidate`.
- Choose Angular `submit()` with a small retry adapter over `FormRoot` for this form. All authoritative validation, errors, touched state, and submitting state stay in Angular.
- Keep authentication and HTTP status mapping in `AdminAuthService`; only map its existing typed outcomes into form errors in the login feature.
- Keep feature code local; do not add a shared form service or architectural convention for one consumer.
- Reference APIs: [Angular submission guide](https://angular.dev/guide/forms/signals/form-submission), [submit API](https://angular.dev/api/forms/signals/submit), and [validation guide](https://angular.dev/guide/forms/signals/validation). Installed version behaviour governs implementation where documentation differs.

## 8. Dependencies and sequencing

1. Implement the typed model, schema, localized errors, and submission action together; accommodate its measured lazy runtime in the total-script error budget.
2. Bind the template and implement retry clearing and focus behaviour; remove old state and handlers in the same slice.
3. Update regression and accessibility coverage, including the unchanged-value retry test before accepting the error integration.
4. Refresh localization, run validation, review the diff, and update this checklist.

No backend or roadmap work blocks this slice. Existing auth service and route contracts remain dependencies.

### Execution record

- Step 1 completed by `model_step`: added the typed model, trim-aware schema, localized messages, and Angular submission action. Legacy template/state remain temporarily for a compiling checkpoint and will be removed in step 2. Formatting, lint, extraction (48 messages), production build, and all 125 existing unit tests passed. The intermediate build measured 405.72 kB total scripts; only the total-script error limit changed to 420 kB. Extraction's duplicate-ID whitespace warnings are intermediate and must disappear when step 2 removes template-owned required messages.
- Step 2 completed by `binding_step`: native inputs use `FormField`, all custom value/touched/error state and input/blur handlers are removed, Angular submission handles validation and pending state, and retry clearing plus render-safe summary focus are wired. Formatting, lint, extraction (46 messages, no duplicate-ID warnings), production build, and all 125 unit tests passed. The complete binding measured 436.79 kB total scripts; the error limit is now 440 kB. Native required attributes are supplied by the schema through `FormField`.

## 9. Risks and mitigations

- **Root server errors block retries:** default `submit()` validates existing submission errors too. Clear root errors before retry using a new model object, prove same-value retry in tests, and never globally bypass validators with `ignoreValidators: 'all'`.
- **Version-sensitive error clearing:** the root object identity behaviour was verified in installed 22.0.2 runtime. Recheck the installed runtime if dependency resolution changes; preserve the public-API-only constraint and regression coverage.
- **Messages disappear after password clearing:** update values before returning the new submission error.
- **Conflicting submit handlers:** use exactly one native submit adapter and Angular `submit()`, without `FormRoot` on the same form.
- **Localization drift:** retain message IDs, extract moved `$localize` messages, and validate localized output.
- **Accessibility regressions:** prove touched/error/ARIA agreement, summary focus, invalid-field focus, and axe checks on error states.
- **Total script budget:** the first production build with the Signal Forms model measured 405.72 kB against the existing 368 kB error limit; binding the inputs through `FormField` measured 436.79 kB (358.78 kB initial; 68.83 kB lazy login chunk). Set the total-script error limit to 440 kB while retaining the 350 kB warning, verify the final localized size, and leave initial/per-script budgets intact. The existing admin login plan records the same measured-budget approach for the lazy login route.

## 10. Unknowns and required clarifications

No blocking product decision remains. This plan assumes a behaviour-preserving migration, including required-only validation and disabled-submit behaviour. Email-format validation or changing when the button is enabled would be a separate product change.

The implementation must verify retry clearing against the installed Angular version; the concrete approach above has been checked against its declarations and runtime, but no component implementation or execution test is part of this planning task.

## 11. Completion checklist

- [x] Typed `$model` and `$form` replace separate credential and touched signals.
- [x] Angular schema owns required validation and localized error messages, including whitespace-only email.
- [x] Native controls use `FormField`; manual value/input/blur handling is removed.
- [x] Angular `submit()` owns touching, validation gating, pending state, and duplicate protection.
- [x] Root form errors replace the independent sign-in error signal.
- [ ] Same-credential retries work without bypassing credential validation or resetting interaction state.
- [ ] Password clearing, credential retention, summary clearing, and focus behaviour match the scenarios.
- [x] Existing successful navigation and safe-return-URL cases remain covered.
- [ ] Component and axe tests cover the changed form and error states.
- [ ] Formatting, lint, localization extraction, unit tests, axe tests, and localized validation pass or specific limitations are recorded.
- [ ] Final diff contains no unrelated API, routing, styling, or dependency changes; the scoped total-script budget adjustment is supported by measured build output.
