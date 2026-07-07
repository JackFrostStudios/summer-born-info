import { TestBed } from '@angular/core/testing';
import { DefaultUrlSerializer, Router, type Navigation, type UrlTree } from '@angular/router';
import {
  routeAccessibilityDataKey,
  type RouteAccessibilityData,
  type RouteAccessibilityMetadata,
} from '../../app-route-accessibility';
import { routes } from '../../app.routes';
import { UnderConstruction } from './under-construction';

function requireUnderConstructionRouteAccessibility(): RouteAccessibilityMetadata {
  const shellRoute = routes.find((route) => route.path === '');
  const underConstructionRoute = shellRoute?.children?.find((route) => route.path === 'under-construction');
  const metadata = (underConstructionRoute?.data as RouteAccessibilityData | undefined)?.[routeAccessibilityDataKey];

  if (metadata === undefined) {
    throw new Error('Expected the under-construction route to declare accessibility metadata.');
  }

  return metadata;
}

describe('UnderConstruction', () => {
  const urlSerializer = new DefaultUrlSerializer();
  const router = {
    lastSuccessfulNavigation: vi.fn<() => Navigation | null>(),
    navigateByUrl: vi.fn().mockResolvedValue(true),
    serializeUrl: vi.fn<(urlTree: UrlTree | string) => string>(),
  };

  function createNavigation(url: string, previousNavigation: Navigation | null = null): Navigation {
    const urlTree = urlSerializer.parse(url);

    return {
      id: 1,
      initialUrl: urlTree,
      extractedUrl: urlTree,
      finalUrl: urlTree,
      trigger: 'imperative',
      extras: {},
      previousNavigation,
      abort: vi.fn(),
    };
  }

  beforeEach(async () => {
    router.lastSuccessfulNavigation.mockReset();
    router.lastSuccessfulNavigation.mockReturnValue(null);
    router.navigateByUrl.mockClear();
    router.serializeUrl.mockImplementation((urlTree) =>
      typeof urlTree === 'string' ? urlTree : urlSerializer.serialize(urlTree),
    );

    await TestBed.configureTestingModule({
      imports: [UnderConstruction],
      providers: [{ provide: Router, useValue: router }],
    }).compileComponents();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders the approved copy with one clear main heading and a decorative builder image', () => {
    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const section = compiled.querySelector<HTMLElement>('section.under-construction');
    const panel = compiled.querySelector<HTMLElement>('sbi-panel.under-construction__panel');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const icon = compiled.querySelector<HTMLElement>('sbi-icon.under-construction__icon');
    const iconSvg = icon?.querySelector('svg') ?? null;
    const buttonHost = compiled.querySelector<HTMLElement>('sbi-button.under-construction__back-button');
    const button = buttonHost?.querySelector<HTMLButtonElement>('button') ?? null;

    if (
      panel === null ||
      heading === null ||
      icon === null ||
      iconSvg === null ||
      buttonHost === null ||
      button === null
    ) {
      throw new Error('Expected the under-construction panel, content, icon, and back button to render.');
    }

    expect(section?.getAttribute('aria-labelledby')).toBe('under-construction-heading');
    expect(panel).not.toBeNull();
    expect(compiled.querySelectorAll('h1')).toHaveLength(1);
    expect(heading.id).toBe('under-construction-heading');
    expect(heading.textContent.trim()).toBe(`We're still working on this page`);
    expect(compiled.textContent).toContain('Coming soon');
    expect(compiled.textContent).toContain(
      `This part of the site isn't ready yet, but the rest of our guidance on delayed starts is.`,
    );
    expect(button.textContent.trim()).toBe('Back to where you were');
    expect(button.type).toBe('button');
    expect(button.classList.contains('sbi-button')).toBe(true);
    expect(button.classList.contains('sbi-button--secondary')).toBe(false);
    expect(icon.getAttribute('aria-hidden')).toBe('true');
    expect(iconSvg.tagName).toBe('svg');
  });

  it('matches the route accessibility contract for the focus target and skip-link destination', () => {
    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const section = compiled.querySelector<HTMLElement>('section.under-construction');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const metadata = requireUnderConstructionRouteAccessibility();
    const [skipLink] = metadata.skipLinks;

    if (skipLink === undefined) {
      throw new Error('Expected the under-construction route to declare a skip-link target.');
    }

    expect(section?.getAttribute('aria-labelledby')).toBe(metadata.focusTargetId);
    expect(heading?.id).toBe(metadata.focusTargetId);
    expect(skipLink.targetId).toBe(metadata.focusTargetId);
    expect(skipLink.label).toBe('Skip to main content');
  });

  it('navigates to the previous non-under-construction route when the visitor uses the button', () => {
    router.lastSuccessfulNavigation.mockReturnValue(
      createNavigation('/under-construction', createNavigation('/guidance/term-dates')),
    );

    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('sbi-button.under-construction__back-button button');

    if (button === null) {
      throw new Error('Expected the back button to render.');
    }

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/guidance/term-dates');
  });

  it('skips under-construction entries and preserves the last real route including its fragment', () => {
    router.lastSuccessfulNavigation.mockReturnValue(
      createNavigation(
        '/under-construction',
        createNavigation('/under-construction#under-construction-heading', createNavigation('/admissions#summer-born')),
      ),
    );

    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('sbi-button.under-construction__back-button button');

    if (button === null) {
      throw new Error('Expected the back button to render.');
    }

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/admissions#summer-born');
  });

  it('does not skip a legitimate route whose path contains under-construction', () => {
    router.lastSuccessfulNavigation.mockReturnValue(
      createNavigation(
        '/under-construction',
        createNavigation('/guidance/under-construction-evidence#summary', createNavigation('/admissions')),
      ),
    );

    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('sbi-button.under-construction__back-button button');

    if (button === null) {
      throw new Error('Expected the back button to render.');
    }

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/guidance/under-construction-evidence#summary');
  });

  it('does not skip a legitimate route whose query contains under-construction', () => {
    router.lastSuccessfulNavigation.mockReturnValue(
      createNavigation(
        '/under-construction',
        createNavigation('/guidance?topic=under-construction#details', createNavigation('/admissions')),
      ),
    );

    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('sbi-button.under-construction__back-button button');

    if (button === null) {
      throw new Error('Expected the back button to render.');
    }

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/guidance?topic=under-construction#details');
  });

  it('falls back to the homepage when there is no prior non-under-construction route', () => {
    router.lastSuccessfulNavigation.mockReturnValue(
      createNavigation('/under-construction', createNavigation('/under-construction#under-construction-heading')),
    );

    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('sbi-button.under-construction__back-button button');

    if (button === null) {
      throw new Error('Expected the back button to render.');
    }

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });
});
