import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import {
  routeAccessibilityDataKey,
  type RouteAccessibilityData,
  type RouteAccessibilityMetadata,
} from '../../app-route-accessibility';
import { routes } from '../../app.routes';
import { NotFound } from './not-found';

function requireNotFoundRouteAccessibility(): RouteAccessibilityMetadata {
  const shellRoute = routes.find((route) => route.path === '');
  const notFoundRoute = shellRoute?.children?.find((route) => route.path === '**');
  const metadata = (notFoundRoute?.data as RouteAccessibilityData | undefined)?.[routeAccessibilityDataKey];

  if (metadata === undefined) {
    throw new Error('Expected the not-found route to declare accessibility metadata.');
  }

  return metadata;
}

describe('NotFound', () => {
  const router = {
    navigateByUrl: vi.fn().mockResolvedValue(true),
  };

  beforeEach(async () => {
    router.navigateByUrl.mockClear();

    await TestBed.configureTestingModule({
      imports: [NotFound],
      providers: [{ provide: Router, useValue: router }],
    }).compileComponents();
  });

  it('renders the missing-page copy with a single main heading and a homepage action', () => {
    const fixture = TestBed.createComponent(NotFound);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const section = compiled.querySelector<HTMLElement>('section.not-found');
    const panel = compiled.querySelector<HTMLElement>('sbi-panel.not-found__panel');
    const panelShell = compiled.querySelector<HTMLElement>('.sbi-panel');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const icon = compiled.querySelector<HTMLElement>('.not-found__icon');
    const buttonHost = compiled.querySelector<HTMLElement>('sbi-button.not-found__home-button');
    const button = buttonHost?.querySelector<HTMLButtonElement>('button') ?? null;

    if (panel === null || heading === null || icon === null || buttonHost === null || button === null) {
      throw new Error('Expected the not-found panel, content, icon, and homepage button to render.');
    }

    expect(section?.getAttribute('aria-labelledby')).toBe('not-found-heading');
    expect(panel).not.toBeNull();
    expect(panelShell?.classList.contains('sbi-panel--media-compact')).toBe(true);
    expect(compiled.querySelectorAll('h1')).toHaveLength(1);
    expect(heading.id).toBe('not-found-heading');
    expect(heading.textContent.trim()).toBe(`We can't find that page`);
    expect(compiled.textContent).toContain('Page not found');
    expect(compiled.textContent).toContain(
      'The page you were looking for may have moved, or the address may be wrong. You can return to the homepage and keep exploring from there.',
    );
    expect(button.textContent.trim()).toBe('Go to the homepage');
    expect(button.type).toBe('button');
    expect(button.classList.contains('sbi-button')).toBe(true);
    expect(button.classList.contains('sbi-button--secondary')).toBe(false);
    expect(icon.getAttribute('aria-hidden')).toBe('true');
  });

  it('matches the route accessibility contract for the focus target and skip-link destination', () => {
    const fixture = TestBed.createComponent(NotFound);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const section = compiled.querySelector<HTMLElement>('section.not-found');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const metadata = requireNotFoundRouteAccessibility();
    const [skipLink] = metadata.skipLinks;

    if (skipLink === undefined) {
      throw new Error('Expected the not-found route to declare a skip-link target.');
    }

    expect(section?.getAttribute('aria-labelledby')).toBe(metadata.focusTargetId);
    expect(heading?.id).toBe(metadata.focusTargetId);
    expect(skipLink.targetId).toBe(metadata.focusTargetId);
    expect(skipLink.label).toBe('Skip to main content');
  });

  it('returns the visitor to the homepage when the primary action is used', () => {
    const fixture = TestBed.createComponent(NotFound);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('sbi-button.not-found__home-button button');

    if (button === null) {
      throw new Error('Expected the homepage button to render.');
    }

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });
});
