import { Component } from '@angular/core';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import {
  NavigationEnd,
  provideRouter,
  Router,
  RouterOutlet,
  type Routes,
  withInMemoryScrolling,
} from '@angular/router';
import { filter, firstValueFrom } from 'rxjs';
import { describe, it } from 'vitest';
import { defineRouteAccessibility, routeAccessibilityDataKey } from '../../app-route-accessibility';
import {
  A11yStylesHost,
  a11yColourModes,
  applyA11yColourMode,
  expectNoA11yViolations,
} from '../../../testing/a11y/a11y-test-helpers';
import { RootShell } from './root-shell';

const shellA11yMetadata = defineRouteAccessibility({
  title: 'Summer-born Info - Accessibility shell',
  focusTargetId: 'root-shell-a11y-heading',
  skipLinks: [{ label: 'Skip to main content', targetId: 'root-shell-a11y-heading' }],
});

@Component({
  selector: 'sbi-root-shell-a11y-route',
  template:
    '<article aria-labelledby="root-shell-a11y-heading" i18n-aria-labelledby="Accessibility shell article label reference@@rootShellA11yArticleLabelledBy"><h1 id="root-shell-a11y-heading" tabindex="-1" i18n="Accessibility shell heading@@rootShellA11yHeading">Accessibility shell route</h1><p i18n="Accessibility shell body@@rootShellA11yBody">Representative routed content for shell accessibility smoke coverage.</p></article>',
})
class RootShellA11yRoute {}

@Component({
  selector: 'sbi-root-shell-a11y-host',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
class RootShellA11yHost {}

const testRoutes: Routes = [
  {
    path: '',
    component: RootShell,
    children: [
      {
        path: '',
        pathMatch: 'full',
        component: RootShellA11yRoute,
        title: shellA11yMetadata.title,
        data: {
          [routeAccessibilityDataKey]: shellA11yMetadata,
        },
      },
    ],
  },
];

describe('RootShell accessibility smoke', () => {
  for (const colourMode of a11yColourModes) {
    it(`has no axe violations in ${colourMode} mode`, async () => {
      applyA11yColourMode(colourMode);

      const { fixture } = await renderShellRoute();

      await expectNoA11yViolations(fixture.nativeElement as HTMLElement);
    });
  }
});

async function renderShellRoute(): Promise<{ fixture: ComponentFixture<RootShellA11yHost>; router: Router }> {
  await TestBed.configureTestingModule({
    imports: [A11yStylesHost, RootShellA11yHost],
    providers: [
      provideRouter(
        testRoutes,
        withInMemoryScrolling({
          anchorScrolling: 'enabled',
          scrollPositionRestoration: 'enabled',
        }),
      ),
    ],
  }).compileComponents();

  const stylesFixture = TestBed.createComponent(A11yStylesHost);
  stylesFixture.detectChanges();

  const fixture = TestBed.createComponent(RootShellA11yHost);
  const router = TestBed.inject(Router);

  router.initialNavigation();
  await waitForNavigation(router);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();

  return { fixture, router };
}

async function waitForNavigation(router: Router): Promise<void> {
  await firstValueFrom(router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd)));
}
