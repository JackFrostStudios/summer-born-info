import { Location } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import {
  NavigationEnd,
  NavigationSkipped,
  provideRouter,
  Router,
  RouterOutlet,
  type Routes,
  withInMemoryScrolling,
} from '@angular/router';
import { filter, firstValueFrom } from 'rxjs';
import { defineRouteAccessibility, routeAccessibilityDataKey } from '../../app-route-accessibility';
import { RouteAccessibilityService } from './route-accessibility.service';

const firstRouteAccessibility = defineRouteAccessibility({
  title: 'Summer-born Info - First route',
  focusTargetId: 'first-route-heading',
  skipLinks: [{ label: 'Skip to main content', targetId: 'first-route-heading' }],
});

const secondRouteAccessibility = defineRouteAccessibility({
  title: 'Summer-born Info - Second route',
  focusTargetId: 'second-route-heading',
  skipLinks: [{ label: 'Skip to next main content', targetId: 'second-route-heading' }],
});

@Component({
  selector: 'sbi-route-accessibility-test-host',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
class RouteAccessibilityTestHost {
  readonly routeAccessibility = inject(RouteAccessibilityService);
}

@Component({
  selector: 'sbi-first-route-content',
  template:
    '<section class="first-route-content"><h1 id="first-route-heading" tabindex="-1" i18n="First route heading@@routeAccessibilityServiceFirstRouteHeading">First route heading</h1><button id="first-route-content-focus-target" type="button" i18n="First route focus target@@routeAccessibilityServiceFirstRouteFocusTarget">Focusable content</button></section>',
})
class FirstRouteContent {}

@Component({
  selector: 'sbi-second-route-content',
  template:
    '<section class="second-route-content"><h1 id="second-route-heading" tabindex="-1" i18n="Second route heading@@routeAccessibilityServiceSecondRouteHeading">Second route heading</h1><button id="second-route-content-focus-target" type="button" i18n="Second route focus target@@routeAccessibilityServiceSecondRouteFocusTarget">Second focusable content</button><div id="second-route-fragment-target" tabindex="-1" i18n="Second route fragment target@@routeAccessibilityServiceSecondRouteFragmentTarget">Second fragment target</div></section>',
})
class SecondRouteContent {}

@Component({
  selector: 'sbi-route-without-accessibility',
  template:
    '<section class="no-accessibility-route"><h1 i18n="Route without accessibility heading@@routeAccessibilityServiceNoAccessibilityHeading">No accessibility metadata</h1></section>',
})
class RouteWithoutAccessibility {}

const testRoutes: Routes = [
  {
    path: '',
    component: FirstRouteContent,
    title: firstRouteAccessibility.title,
    data: {
      [routeAccessibilityDataKey]: firstRouteAccessibility,
    },
  },
  {
    path: 'second',
    component: SecondRouteContent,
    title: secondRouteAccessibility.title,
    data: {
      [routeAccessibilityDataKey]: secondRouteAccessibility,
    },
  },
  {
    path: 'no-accessibility',
    component: RouteWithoutAccessibility,
  },
];

describe('RouteAccessibilityService', () => {
  afterEach(() => {
    document.body.replaceChildren();
    document.title = '';
    TestBed.resetTestingModule();
  });

  it('publishes the active route announcement and skip links after navigation', async () => {
    const { host } = await renderHost('/');

    expect(document.title).toBe(firstRouteAccessibility.title);
    expect(host.routeAccessibility.$navigationAnnouncement()).toBe(firstRouteAccessibility.title);
    expect(host.routeAccessibility.$skipLinks()).toEqual(firstRouteAccessibility.skipLinks);
  });

  it('clears the announcement and skip links when the active route has no accessibility metadata', async () => {
    const { host, fixture, router } = await renderHost('/');

    await navigate(router, fixture, '/no-accessibility');

    expect(document.title).toBe('Summer-born Info');
    expect(host.routeAccessibility.$navigationAnnouncement()).toBe('');
    expect(host.routeAccessibility.$skipLinks()).toEqual([]);
  });

  it('returns focus to the body after a navigation without a fragment target', async () => {
    const { fixture, router } = await renderHost('/');
    const compiled = fixture.nativeElement as HTMLElement;
    const firstRouteFocusTarget = compiled.querySelector<HTMLElement>('#first-route-content-focus-target');

    expect(firstRouteFocusTarget).not.toBeNull();

    if (firstRouteFocusTarget === null) {
      throw new Error('Expected the first route focus target to render.');
    }

    firstRouteFocusTarget.focus();
    expect(document.activeElement).toBe(firstRouteFocusTarget);

    await navigate(router, fixture, '/second');

    expect(document.activeElement).toBe(document.body);
  });

  it('moves focus to the fragment target after a navigation with an anchor', async () => {
    const { fixture, router } = await renderHost('/');

    await navigate(router, fixture, '/second#second-route-fragment-target');

    const compiled = fixture.nativeElement as HTMLElement;
    const fragmentTarget = compiled.querySelector<HTMLElement>('#second-route-fragment-target');

    expect(fragmentTarget).not.toBeNull();

    if (fragmentTarget === null) {
      throw new Error('Expected the second route fragment target to render.');
    }

    expect(document.activeElement).toBe(fragmentTarget);
  });

  it('keeps history restoration in control when browser back returns to a fragment entry', async () => {
    const { fixture, router } = await renderHost('/');

    await navigate(router, fixture, '/second#second-route-fragment-target');
    await navigate(router, fixture, '/');

    const compiled = fixture.nativeElement as HTMLElement;
    const firstRouteFocusTarget = compiled.querySelector<HTMLElement>('#first-route-content-focus-target');

    expect(firstRouteFocusTarget).not.toBeNull();

    if (firstRouteFocusTarget === null) {
      throw new Error('Expected the first route focus target to render after returning to the first route.');
    }

    firstRouteFocusTarget.focus();
    expect(document.activeElement).toBe(firstRouteFocusTarget);

    const location = TestBed.inject(Location);

    await triggerHistoryNavigation(router, fixture, () => {
      location.back();
    });

    const updatedCompiled = fixture.nativeElement as HTMLElement;
    const fragmentTarget = updatedCompiled.querySelector<HTMLElement>('#second-route-fragment-target');

    expect(fragmentTarget).not.toBeNull();
    expect(router.url).toBe('/second#second-route-fragment-target');
    expect(document.activeElement).toBe(document.body);
  });
});

async function renderHost(initialUrl: string): Promise<{
  fixture: ComponentFixture<RouteAccessibilityTestHost>;
  host: RouteAccessibilityTestHost;
  router: Router;
}> {
  await TestBed.configureTestingModule({
    imports: [RouteAccessibilityTestHost],
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

  const fixture = TestBed.createComponent(RouteAccessibilityTestHost);
  document.body.appendChild(fixture.nativeElement);

  const host = fixture.componentInstance;
  const router = TestBed.inject(Router);
  router.initialNavigation();

  await navigate(router, fixture, initialUrl);

  return { fixture, host, router };
}

async function navigate(
  router: Router,
  fixture: ComponentFixture<RouteAccessibilityTestHost>,
  url: string,
): Promise<void> {
  await router.navigateByUrl(url);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

async function triggerHistoryNavigation(
  router: Router,
  fixture: ComponentFixture<RouteAccessibilityTestHost>,
  action: () => void,
): Promise<void> {
  const navigationCompleted = firstValueFrom(
    router.events.pipe(
      filter(
        (event): event is NavigationEnd | NavigationSkipped =>
          event instanceof NavigationEnd || event instanceof NavigationSkipped,
      ),
    ),
  );

  action();
  await navigationCompleted;
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}
