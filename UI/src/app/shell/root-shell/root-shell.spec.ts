import { Component } from '@angular/core';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { provideRouter, Router, type Routes } from '@angular/router';
import { defineRouteAccessibility, routeAccessibilityDataKey } from '../../app-route-accessibility';
import { RootShell } from './root-shell';

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
  selector: 'sbi-first-route-content',
  template:
    '<section class="first-route-content" aria-labelledby="first-route-heading" i18n-aria-labelledby="First route section label reference@@firstRouteContentAriaLabelledBy"><h1 id="first-route-heading" tabindex="-1" i18n="First route heading@@firstRouteHeading">First route heading</h1><button id="first-route-content-focus-target" type="button" i18n="First route focusable content@@firstRouteFocusableContent">Focusable content</button></section>',
})
class FirstRouteContent {}

@Component({
  selector: 'sbi-second-route-content',
  template:
    '<section class="second-route-content" aria-labelledby="second-route-heading" i18n-aria-labelledby="Second route section label reference@@secondRouteContentAriaLabelledBy"><h1 id="second-route-heading" tabindex="-1" i18n="Second route heading@@secondRouteHeading">Second route heading</h1><button id="second-route-content-focus-target" type="button" i18n="Second route focusable content@@secondRouteFocusableContent">Second focusable content</button><div id="second-route-fragment-target" tabindex="-1" i18n="Second route fragment target@@secondRouteFragmentTarget">Second fragment target</div></section>',
})
class SecondRouteContent {}

const testRoutes: Routes = [
  {
    path: '',
    component: RootShell,
    children: [
      {
        path: '',
        pathMatch: 'full',
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
    ],
  },
];

describe('RootShell', () => {
  afterEach(() => {
    document.body.replaceChildren();
    document.title = '';
    TestBed.resetTestingModule();
  });

  it('displays the header, routed content, and footer as expected', async () => {
    const { fixture } = await renderShell('/');
    const compiled = fixture.nativeElement as HTMLElement;

    const header = compiled.querySelector('sbi-public-header');
    const main = compiled.querySelector('main.app-shell__main');
    const footer = compiled.querySelector('sbi-public-footer');
    const firstRouteContent = compiled.querySelector('.first-route-content');

    expect(header).not.toBeNull();
    expect(main).not.toBeNull();
    expect(footer).not.toBeNull();
    expect(firstRouteContent).not.toBeNull();

    if (header === null || main === null || footer === null || firstRouteContent === null) {
      throw new Error('Expected the root shell header, main content, footer, and first route content to render.');
    }

    expect(main.contains(firstRouteContent)).toBe(true);
    expect(header.contains(firstRouteContent)).toBe(false);
    expect(footer.contains(firstRouteContent)).toBe(false);
  });

  it('sets the page title and aria announcement from the active route metadata after navigation', async () => {
    const { fixture } = await renderShell('/');
    const announcement = getAnnouncementElement(fixture);

    expect(document.title).toBe(firstRouteAccessibility.title);
    expect(announcement.textContent.trim()).toBe(firstRouteAccessibility.title);
  });

  it('updates the page title and aria announcement when a second navigation activates a new route', async () => {
    const { fixture, router } = await renderShell('/');

    await navigate(router, fixture, '/second');

    const announcement = getAnnouncementElement(fixture);
    const compiled = fixture.nativeElement as HTMLElement;
    const secondRouteHeading = compiled.querySelector('#second-route-heading');

    expect(secondRouteHeading).not.toBeNull();
    expect(document.title).toBe(secondRouteAccessibility.title);
    expect(announcement.textContent.trim()).toBe(secondRouteAccessibility.title);
  });

  it('resets focus to the body after navigation when the new route URL has no anchor', async () => {
    const { fixture, router } = await renderShell('/');
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

  it('moves focus to the anchor element after navigation when the new route URL includes an anchor', async () => {
    const { fixture, router } = await renderShell('/');
    const compiled = fixture.nativeElement as HTMLElement;
    const firstRouteFocusTarget = compiled.querySelector<HTMLElement>('#first-route-content-focus-target');

    expect(firstRouteFocusTarget).not.toBeNull();

    if (firstRouteFocusTarget === null) {
      throw new Error('Expected the first route focus target to render.');
    }

    firstRouteFocusTarget.focus();
    expect(document.activeElement).toBe(firstRouteFocusTarget);

    await navigate(router, fixture, '/second#second-route-fragment-target');

    const updatedCompiled = fixture.nativeElement as HTMLElement;
    const fragmentTarget = updatedCompiled.querySelector<HTMLElement>('#second-route-fragment-target');

    expect(fragmentTarget).not.toBeNull();

    if (fragmentTarget === null) {
      throw new Error('Expected the second route fragment target to render.');
    }

    expect(document.activeElement).toBe(fragmentTarget);
  });
});

async function renderShell(initialUrl: string): Promise<{ fixture: ComponentFixture<RootShell>; router: Router }> {
  await TestBed.configureTestingModule({
    imports: [RootShell],
    providers: [provideRouter(testRoutes)],
  }).compileComponents();

  const fixture = TestBed.createComponent(RootShell);
  document.body.appendChild(fixture.nativeElement);

  const router = TestBed.inject(Router);
  router.initialNavigation();

  await navigate(router, fixture, initialUrl);

  return { fixture, router };
}

async function navigate(router: Router, fixture: ComponentFixture<RootShell>, url: string): Promise<void> {
  await router.navigateByUrl(url);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function getAnnouncementElement(fixture: ComponentFixture<RootShell>): HTMLElement {
  const compiled = fixture.nativeElement as HTMLElement;
  const announcement = compiled.querySelector<HTMLElement>('.app-shell__route-announcement');

  if (announcement === null) {
    throw new Error('Expected the root shell route announcement to render.');
  }

  return announcement;
}
