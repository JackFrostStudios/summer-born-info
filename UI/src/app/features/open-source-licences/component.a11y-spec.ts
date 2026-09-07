import { a11yColourModes, applyA11yColourMode, expectNoA11yViolations, renderFixtureForA11y } from '@a11y-test-helpers';
import { describe, it } from 'vitest';
import { OpenSourceLicences } from './open-source-licences';

describe('OpenSourceLicences accessibility smoke', () => {
  for (const colourMode of a11yColourModes) {
    it(`has no axe violations in ${colourMode} mode`, async () => {
      applyA11yColourMode(colourMode);

      const fixture = await renderFixtureForA11y(OpenSourceLicences);

      await expectNoA11yViolations(fixture.nativeElement as HTMLElement);
    });
  }
});
