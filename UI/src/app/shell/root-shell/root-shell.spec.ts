import { Component } from '@angular/core';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { provideRouter, Router, type Routes } from '@angular/router';
import { defineRouteAccessibility, routeAccessibilityDataKey } from '../../app-route-accessibility';
import { RootShell } from './root-shell';

const testRouteAccessibility = defineRouteAccessibility({
  title: 'Summer-born Info - Test route',
  focusTargetId: 'test-heading',
  skipLinks: [{ label: 'Skip to main content', targetId: 'test-heading' }],
});

@Component({
  selector: 'sbi-test-route-content',
  template:
    '<section class="test-route-content" aria-labelledby="test-heading" i18n-aria-labelledby="Test route section label reference@@testRouteContentAriaLabelledBy"><h1 id="test-heading" i18n="Test route heading@@testRouteContentHeading">Test route heading</h1><div id="test-fragment-target" i18n="Test fragment target@@testRouteContentFragmentTarget">Fragment target</div></section>',
})
class TestRouteContent {}

describe('RootShell', () => {
  afterEach(() => {
    document.body.replaceChildren();
    document.title = '';
    TestBed.resetTestingModule();
  });

  it('renders the shell as a header, routed main content, and shared footer composition', async () => {
    const fixture = await renderShellWithRoutes([
      {
        path: '',
        component: TestRouteContent,
        title: testRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: testRouteAccessibility,
        },
      },
    ]);

    const compiled = fixture.nativeElement as HTMLElement;

    const shell = compiled.querySelector('.app-shell');
    const skipLinks = compiled.querySelector('sbi-skip-links');
    const header = compiled.querySelector('sbi-public-header');
    const main = compiled.querySelector('main.app-shell__main');
    const footer = compiled.querySelector('sbi-public-footer');
    const routedContent = compiled.querySelector<HTMLElement>('.test-route-content');

    expect(shell).not.toBeNull();
    expect(skipLinks).not.toBeNull();
    expect(header).not.toBeNull();
    expect(main).not.toBeNull();
    expect(footer).not.toBeNull();
    expect(routedContent).not.toBeNull();

    if (
      shell === null ||
      skipLinks === null ||
      header === null ||
      main === null ||
      footer === null ||
      routedContent === null
    ) {
      throw new Error('Expected the shell, skip links, header, main region, footer, and routed content to render.');
    }

    expect(shell.firstElementChild).toBe(skipLinks);
    expect(skipLinks.nextElementSibling?.classList.contains('app-shell__route-announcement')).toBe(true);
    expect(header.nextElementSibling).toBe(main);
    expect(shell.lastElementChild).toBe(footer);
    expect(main.contains(routedContent)).toBe(true);
    expect(routedContent.closest('main.app-shell__main')).toBe(main);
    expect(header.contains(routedContent)).toBe(false);
    expect(footer.contains(routedContent)).toBe(false);
  });

  it('focuses the routed page heading by default and publishes a lightweight route announcement', async () => {
    const fixture = await renderShellWithRoutes([
      {
        path: '',
        component: TestRouteContent,
        title: testRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: testRouteAccessibility,
        },
      },
    ]);

    applyRouteAccessibility(fixture);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const heading = compiled.querySelector<HTMLElement>('#test-heading');
    const announcement = compiled.querySelector<HTMLElement>('.app-shell__route-announcement');
    const skipLink = compiled.querySelector<HTMLAnchorElement>('sbi-skip-links a.skip-links__link');

    expect(document.title).toBe('Summer-born Info - Test route');
    expect(heading).not.toBeNull();
    expect(announcement).not.toBeNull();
    expect(skipLink).not.toBeNull();

    if (heading === null || announcement === null || skipLink === null) {
      throw new Error('Expected the default route heading, route announcement, and skip link to render.');
    }

    expect(skipLink.textContent.trim()).toBe('Skip to main content');
    expect(skipLink.getAttribute('href')).toBe('/#test-heading');

    const focusSpy = vi.spyOn(heading, 'focus');

    applyRouteAccessibility(fixture);
    fixture.detectChanges();

    expect(announcement.textContent.trim()).toBe('Summer-born Info - Test route');
    expect(focusSpy).toHaveBeenCalled();
    expect(heading.getAttribute('tabindex')).toBe('-1');
  });

  it('focuses the requested fragment target instead of the default heading when navigation includes a fragment', async () => {
    const fixture = await renderShellWithRoutes(
      [
        {
          path: '',
          component: TestRouteContent,
          title: testRouteAccessibility.title,
          data: {
            [routeAccessibilityDataKey]: testRouteAccessibility,
          },
        },
      ],
      '/#test-fragment-target',
    );

    const compiled = fixture.nativeElement as HTMLElement;
    const heading = compiled.querySelector<HTMLElement>('#test-heading');
    const fragmentTarget = compiled.querySelector<HTMLElement>('#test-fragment-target');

    expect(fragmentTarget).not.toBeNull();

    if (heading === null || fragmentTarget === null) {
      throw new Error('Expected the route heading and fragment target to render.');
    }

    const fragmentFocusSpy = vi.spyOn(fragmentTarget, 'focus');
    const headingFocusSpy = vi.spyOn(heading, 'focus');

    applyRouteAccessibility(fixture);
    fixture.detectChanges();

    expect(fragmentFocusSpy).toHaveBeenCalled();
    expect(fragmentTarget.getAttribute('tabindex')).toBe('-1');
    expect(headingFocusSpy).not.toHaveBeenCalled();
  });
});

async function renderShellWithRoutes(routes: Routes, initialUrl = '/'): Promise<ComponentFixture<RootShell>> {
  await TestBed.configureTestingModule({
    imports: [RootShell],
    providers: [provideRouter(routes)],
  }).compileComponents();

  const fixture = TestBed.createComponent(RootShell);
  document.body.appendChild(fixture.nativeElement);

  const router = TestBed.inject(Router);

  router.initialNavigation();
  await router.navigateByUrl(initialUrl);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();

  return fixture;
}

function applyRouteAccessibility(fixture: ComponentFixture<RootShell>): void {
  const rootShell = fixture.componentInstance as unknown as {
    applyRouteAccessibility(): void;
  };

  rootShell.applyRouteAccessibility();
}
