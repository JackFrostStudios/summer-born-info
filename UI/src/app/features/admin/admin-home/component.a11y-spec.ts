import { provideRouter } from '@angular/router';
import { a11yColourModes, applyA11yColourMode, expectNoA11yViolations, renderFixtureForA11y } from '@a11y-test-helpers';
import { describe, it, vi } from 'vitest';
import { AdminAuthService } from '../auth/admin-auth.service';
import { AdminHome } from './admin-home';

describe('AdminHome accessibility smoke', () => {
  for (const colourMode of a11yColourModes) {
    it(`has no axe violations in ${colourMode} mode`, async () => {
      applyA11yColourMode(colourMode);

      const fixture = await renderFixtureForA11y(AdminHome, {
        providers: [
          provideRouter([]),
          {
            provide: AdminAuthService,
            useValue: {
              $isAuthenticated: () => true,
              $isSigningOut: () => false,
              signOut: vi.fn().mockResolvedValue({ kind: 'signed-out' }),
            },
          },
        ],
      });

      await expectNoA11yViolations(fixture.nativeElement as HTMLElement);
    });
  }
});
