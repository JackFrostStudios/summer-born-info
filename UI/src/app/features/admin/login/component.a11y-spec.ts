import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { a11yColourModes, applyA11yColourMode, expectNoA11yViolations, renderFixtureForA11y } from '@a11y-test-helpers';
import { describe, expect, it, vi } from 'vitest';
import { AdminAuthService } from '../auth/admin-auth.service';
import { AdminLogin } from './admin-login';

describe('AdminLogin accessibility smoke', () => {
  for (const colourMode of a11yColourModes) {
    for (const state of [
      'initial',
      'required-errors',
      'invalid-credentials',
      'not-admin',
      'network-failure',
      'unexpected-failure',
    ] as const) {
      it(`has no axe violations with ${state} in ${colourMode} mode`, async () => {
        applyA11yColourMode(colourMode);

        const fixture = await renderFixtureForA11y(AdminLogin, {
          providers: [
            provideRouter([]),
            {
              provide: ActivatedRoute,
              useValue: {
                snapshot: {
                  queryParamMap: convertToParamMap({}),
                },
              },
            },
            {
              provide: AdminAuthService,
              useValue: {
                $isAuthenticated: () => false,
                $isSigningIn: () => false,
                signIn: vi.fn().mockResolvedValue({
                  kind: state === 'initial' || state === 'required-errors' ? 'signed-in' : state,
                }),
              },
            },
          ],
        });

        const compiled = fixture.nativeElement as HTMLElement;
        const email = compiled.querySelector<HTMLInputElement>('#admin-login-email');
        const password = compiled.querySelector<HTMLInputElement>('#admin-login-password');
        expect(compiled.querySelector('form')?.noValidate).toBe(true);
        expect(email?.type).toBe('email');
        expect(email?.autocomplete).toBe('username');
        expect(password?.type).toBe('password');
        expect(password?.autocomplete).toBe('current-password');
        if (state !== 'initial') {
          if (state !== 'required-errors') {
            for (const [selector, value] of [
              ['#admin-login-email', 'admin@example.test'],
              ['#admin-login-password', 'secret'],
            ] as const) {
              const input = compiled.querySelector<HTMLInputElement>(selector);
              if (!input) throw new Error(`Missing ${selector}`);
              input.value = value;
              input.dispatchEvent(new Event('input'));
            }
            fixture.detectChanges();
            await fixture.whenStable();
          }
          compiled.querySelector('form')?.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
          fixture.detectChanges();
          await fixture.whenStable();
          await new Promise<void>((resolve) => setTimeout(resolve));
          fixture.detectChanges();
          if (state === 'required-errors') {
            expect(compiled.querySelector('#admin-login-email-error')?.textContent.trim()).toBe(
              'Enter your email address.',
            );
            expect(compiled.querySelector('#admin-login-password-error')?.textContent.trim()).toBe(
              'Enter your password.',
            );
            expect(document.activeElement).toBe(email);
            expect(email?.getAttribute('aria-describedby')).toBe('admin-login-email-error');
            expect(password?.getAttribute('aria-describedby')).toBe('admin-login-password-error');
          } else {
            const summary = compiled.querySelector('#admin-login-error');
            expect(summary).not.toBeNull();
            expect(document.activeElement).toBe(summary);
            expect(email?.getAttribute('aria-describedby')).toBe('admin-login-error');
            expect(password?.getAttribute('aria-describedby')).toBe(
              state === 'invalid-credentials' ? 'admin-login-password-error' : 'admin-login-error',
            );
          }
        }
        await expectNoA11yViolations(compiled);
      });
    }
  }
});
