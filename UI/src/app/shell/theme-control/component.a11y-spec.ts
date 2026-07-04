import { describe, it } from 'vitest';
import {
  a11yColourModes,
  applyA11yColourMode,
  expectNoA11yViolations,
  renderFixtureForA11y,
} from '../../../testing/a11y/a11y-test-helpers';
import { ThemeControl } from './theme-control';

describe('ThemeControl accessibility smoke', () => {
  for (const colourMode of a11yColourModes) {
    it(`has no axe violations in ${colourMode} mode`, async () => {
      applyA11yColourMode(colourMode);

      const fixture = await renderFixtureForA11y(ThemeControl);

      await expectNoA11yViolations(fixture.nativeElement as HTMLElement);
    });
  }
});
