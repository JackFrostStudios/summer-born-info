import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { a11yColourModes, applyA11yColourMode, expectNoA11yViolations, renderFixtureForA11y } from '@a11y-test-helpers';
import { describe, it, vi } from 'vitest';
import { AdminAuthService } from '../auth/admin-auth.service';
import { AdminLogin } from './admin-login';

describe('AdminLogin accessibility smoke', () => {
  for (const colourMode of a11yColourModes) {
    it(`has no axe violations in ${colourMode} mode`, async () => {
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
              signIn: vi.fn().mockResolvedValue({ kind: 'signed-in' }),
            },
          },
        ],
      });

      await expectNoA11yViolations(fixture.nativeElement as HTMLElement);
    });
  }
});
