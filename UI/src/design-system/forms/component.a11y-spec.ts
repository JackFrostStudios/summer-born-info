import { Component, signal } from '@angular/core';
import { type ComponentFixture } from '@angular/core/testing';
import { disabled, form, readonly, required, validate, type ValidationError } from '@angular/forms/signals';
import { a11yColourModes, applyA11yColourMode, expectNoA11yViolations, renderFixtureForA11y } from '@a11y-test-helpers';
import { PasswordInput, TextInput } from '@design-system/forms';
import { cdp, page, userEvent } from 'vitest/browser';
import { type CDPSession } from '@vitest/browser-playwright';
import { describe, expect, it } from 'vitest';

@Component({
  selector: 'sbi-form-inputs-a11y-host',
  imports: [TextInput, PasswordInput],
  // eslint-disable-next-line @angular-eslint/component-max-inline-declarations -- The browser fixture keeps the real field bindings, labels and keyboard boundaries together.
  template: `
    <main>
      <h1 i18n="Form inputs browser test heading@@formInputsA11yHeading">Form inputs</h1>
      <form novalidate>
        <button type="button" i18n="Form inputs browser test start@@formInputsA11yStart">Before fields</button>
        <sbi-text-input
          [$field]="$form.email"
          [$inputId]="emailId"
          [$type]="emailType"
          [$autocomplete]="emailAutocomplete"
        >
          <span i18n="Form inputs browser test email label@@formInputsA11yEmail">Email address</span>
        </sbi-text-input>
        <sbi-password-input [$field]="$form.password" [$inputId]="passwordId" [$autocomplete]="passwordAutocomplete">
          <span i18n="Form inputs browser test password label@@formInputsA11yPassword">Password</span>
        </sbi-password-input>
        <button type="button" i18n="Form inputs browser test end@@formInputsA11yEnd">After fields</button>
      </form>
    </main>
  `,
  // eslint-disable-next-line @angular-eslint/component-max-inline-declarations -- Minimal host layout models a consuming form at normal and narrow widths.
  styles: `
    main {
      margin-inline: auto;
      max-inline-size: 28rem;
      padding: 1rem;
    }
    button {
      background: var(--sbi-color-surface);
      color: var(--sbi-color-text);
    }
    form {
      display: grid;
      gap: 1rem;
    }
  `,
})
class FormInputsA11yHost {
  readonly emailId = 'forms-a11y-email';
  readonly passwordId = 'forms-a11y-password';
  readonly emailType = 'email';
  readonly emailAutocomplete = 'username';
  readonly passwordAutocomplete = 'current-password';
  readonly $model = signal({ email: '', password: '' });
  readonly $disabled = signal(false);
  readonly $readonly = signal(false);
  readonly $emailErrors = signal<ValidationError[]>([]);
  readonly $passwordErrors = signal<ValidationError[]>([]);
  readonly $form = form(this.$model, (path) => {
    required(path.email, { message: 'Enter your email address.' });
    required(path.password, { message: 'Enter your password.' });
    disabled(path.email, { when: () => this.$disabled() });
    disabled(path.password, { when: () => this.$disabled() });
    readonly(path.email, { when: () => this.$readonly() });
    readonly(path.password, { when: () => this.$readonly() });
    validate(path.email, () => this.$emailErrors());
    validate(path.password, () => this.$passwordErrors());
  });
}

describe('Shared form inputs accessibility', () => {
  for (const colourMode of a11yColourModes) {
    for (const state of ['initial', 'multiple-errors', 'disabled', 'readonly'] as const) {
      it(`has no axe violations with ${state} in ${colourMode} mode`, async () => {
        applyA11yColourMode(colourMode);
        const fixture = await renderFixtureForA11y(FormInputsA11yHost);
        const host = fixture.componentInstance;
        const compiled = fixture.nativeElement as HTMLElement;
        const email = nativeInput(compiled, host.emailId);
        const password = nativeInput(compiled, host.passwordId);

        if (state === 'multiple-errors') {
          showMultipleErrors(host);
        } else if (state === 'disabled' || state === 'readonly') {
          host.$model.set({ email: 'parent@example.test', password: '  secret  ' });
          if (state === 'disabled') host.$disabled.set(true);
          else host.$readonly.set(true);
        }
        await settle(fixture);

        expect(email.type).toBe('email');
        expect(email.autocomplete).toBe('username');
        expect(password.type).toBe('password');
        expect(password.autocomplete).toBe('current-password');
        for (const input of [email, password]) {
          expect(input.required).toBe(true);
          expect(input.disabled).toBe(state === 'disabled');
          expect(input.readOnly).toBe(state === 'readonly');
          expect(input.getAttribute('aria-invalid')).toBe(state === 'multiple-errors' ? 'true' : 'false');
          expect(compiled.querySelectorAll(`[id="${input.id}"]`)).toHaveLength(1);
          expect(compiled.querySelector<HTMLLabelElement>(`label[for="${input.id}"]`)?.control).toBe(input);
          if (state === 'multiple-errors') {
            const feedbackId = `${input.id}-error`;
            expect(input.getAttribute('aria-describedby')).toBe(feedbackId);
            const feedback = compiled.querySelector(`#${feedbackId}`);
            expect(feedback?.textContent).toContain(
              input === email ? 'Enter your email address.' : 'Enter your password.',
            );
            expect(feedback?.textContent).toContain(
              input === email ? 'Caller email message.' : 'Caller password message.',
            );
            expect(compiled.querySelectorAll(`#${feedbackId}`)).toHaveLength(1);
          } else {
            expect(input.getAttribute('aria-describedby')).toBeNull();
          }
        }
        if (state === 'disabled' || state === 'readonly') {
          expect(email.value).toBe('parent@example.test');
          expect(password.value).toBe('  secret  ');
          await userEvent.click(page.getByRole('button', { name: 'Before fields' }));
          await userEvent.tab();
          expect(document.activeElement).toBe(
            state === 'disabled' ? compiled.querySelector('button:last-child') : email,
          );
          if (state === 'readonly') {
            await userEvent.keyboard('x');
            expect(email.value).toBe('parent@example.test');
            await userEvent.tab();
            expect(document.activeElement).toBe(password);
            await userEvent.keyboard('x');
            expect(password.value).toBe('  secret  ');
            expect(host.$model()).toEqual({ email: 'parent@example.test', password: '  secret  ' });
          }
        }
        await expectNoA11yViolations(compiled);
      });
    }

    it(`supports native label clicks, field focus and visible keyboard order in ${colourMode} mode`, async () => {
      applyA11yColourMode(colourMode);
      const fixture = await renderFixtureForA11y(FormInputsA11yHost);
      const host = fixture.componentInstance;
      const compiled = fixture.nativeElement as HTMLElement;
      const email = nativeInput(compiled, host.emailId);
      const password = nativeInput(compiled, host.passwordId);

      for (const [label, input] of [
        ['Email address', email],
        ['Password', password],
      ] as const) {
        await userEvent.click(page.getByText(label, { exact: true }));
        expect(document.activeElement).toBe(input);
      }
      host.$form.email().focusBoundControl();
      expect(document.activeElement).toBe(email);
      host.$form.password().focusBoundControl();
      expect(document.activeElement).toBe(password);

      await userEvent.click(page.getByRole('button', { name: 'Before fields' }));
      for (const input of [email, password]) {
        await userEvent.tab();
        expect(document.activeElement).toBe(input);
        expectVisibleFocus(input);
      }
      await userEvent.tab();
      expect(document.activeElement).toBe(compiled.querySelector('button:last-child'));
      await userEvent.tab({ shift: true });
      expect(document.activeElement).toBe(password);
      expectVisibleFocus(password);
    });

    it(`keeps both fields and multiple messages within a 320px viewport in ${colourMode} mode`, async () => {
      applyA11yColourMode(colourMode);
      await page.viewport(320, 700);
      try {
        const fixture = await renderFixtureForA11y(FormInputsA11yHost);
        const compiled = fixture.nativeElement as HTMLElement;
        showMultipleErrors(fixture.componentInstance);
        await settle(fixture);

        expect(document.documentElement.scrollWidth).toBeLessThanOrEqual(window.innerWidth);
        for (const input of compiled.querySelectorAll('input')) {
          const bounds = input.getBoundingClientRect();
          expect(bounds.width).toBeGreaterThan(0);
          expect(bounds.left).toBeGreaterThanOrEqual(0);
          expect(bounds.right).toBeLessThanOrEqual(window.innerWidth);
          const feedback = compiled.querySelector(`#${input.id}-error`);
          expect(feedback).not.toBeNull();
          const feedbackBounds = feedback?.getBoundingClientRect();
          expect(feedbackBounds?.right).toBeLessThanOrEqual(window.innerWidth);
        }
        await expectNoA11yViolations(compiled);
      } finally {
        await page.viewport(1440, 1080);
      }
    });
  }

  it('retains visible keyboard focus for both native controls in forced colours', async () => {
    const session: CDPSession = cdp();
    await session.send('Emulation.setEmulatedMedia', { features: [{ name: 'forced-colors', value: 'active' }] });
    try {
      expect(window.matchMedia('(forced-colors: active)').matches).toBe(true);
      const fixture = await renderFixtureForA11y(FormInputsA11yHost);
      const compiled = fixture.nativeElement as HTMLElement;
      await userEvent.click(page.getByRole('button', { name: 'Before fields' }));
      for (const input of compiled.querySelectorAll('input')) {
        await userEvent.tab();
        expect(document.activeElement).toBe(input);
        expectVisibleFocus(input);
      }
    } finally {
      await session.send('Emulation.setEmulatedMedia', { features: [] });
    }
  });
});

function nativeInput(compiled: HTMLElement, id: string): HTMLInputElement {
  const input = compiled.querySelector<HTMLInputElement>(`#${id}`);
  if (input === null) throw new Error(`Missing native input ${id}.`);
  return input;
}

function showMultipleErrors(host: FormInputsA11yHost): void {
  host.$emailErrors.set([{ kind: 'caller-email', message: 'Caller email message.' }]);
  host.$passwordErrors.set([{ kind: 'caller-password', message: 'Caller password message.' }]);
  host.$form.email().markAsTouched();
  host.$form.password().markAsTouched();
}

function expectVisibleFocus(input: HTMLInputElement): void {
  expect(input.matches(':focus-visible')).toBe(true);
  const style = getComputedStyle(input);
  expect(style.outlineStyle).toBe('solid');
  expect(parseFloat(style.outlineWidth)).toBeGreaterThanOrEqual(2);
  expect(parseFloat(style.outlineOffset)).toBeGreaterThan(0);
  expect(style.outlineColor).not.toBe(style.backgroundColor);
}

async function settle(fixture: ComponentFixture<FormInputsA11yHost>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}
