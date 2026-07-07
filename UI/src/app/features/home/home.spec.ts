import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import {
  routeAccessibilityDataKey,
  type RouteAccessibilityData,
  type RouteAccessibilityMetadata,
} from '../../app-route-accessibility';
import { routes } from '../../app.routes';
import { Home } from './home';

function requireHomeRouteAccessibility(): RouteAccessibilityMetadata {
  const shellRoute = routes.find((route) => route.path === '');
  const homeRoute = shellRoute?.children?.find((route) => route.path === '');
  const metadata = (homeRoute?.data as RouteAccessibilityData | undefined)?.[routeAccessibilityDataKey];

  if (metadata === undefined) {
    throw new Error('Expected the homepage route to declare accessibility metadata.');
  }

  return metadata;
}

function requireArticle(host: ParentNode): HTMLElement {
  const article = host.querySelector('article');

  if (!(article instanceof HTMLElement)) {
    throw new Error('Expected the homepage to render a single article landmark.');
  }

  return article;
}

function requireHeading(host: ParentNode): HTMLHeadingElement {
  const heading = host.querySelector('h1');

  if (!(heading instanceof HTMLHeadingElement)) {
    throw new Error('Expected the homepage hero to provide a single level-one heading.');
  }

  return heading;
}

describe('Home', () => {
  const router = {
    navigateByUrl: vi.fn().mockResolvedValue(true),
  };

  beforeEach(async () => {
    router.navigateByUrl.mockClear();

    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [{ provide: Router, useValue: router }],
    }).compileComponents();
  });

  it('composes the homepage as a single labelled article around the hero', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const article = requireArticle(compiled);
    const hero = article.querySelector('sbi-home-hero');
    const heading = requireHeading(article);

    expect(hero).not.toBeNull();
    expect(article.childElementCount).toBe(1);
    expect(article.firstElementChild).toBe(hero);
    expect(article.getAttribute('aria-labelledby')).toBe(heading.id);
    expect(article.hasAttribute('i18n-aria-labelledby')).toBe(false);
    expect(heading.id).toBe('home-heading');
    expect(heading.tabIndex).toBe(-1);
    expect(compiled.querySelectorAll('h1')).toHaveLength(1);
  });

  it('matches the homepage route accessibility contract for the focus target and skip-link destination', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const article = requireArticle(compiled);
    const heading = requireHeading(compiled);
    const metadata = requireHomeRouteAccessibility();
    const [skipLink] = metadata.skipLinks;

    if (skipLink === undefined) {
      throw new Error('Expected the homepage route to declare a skip-link target.');
    }

    expect(article.getAttribute('aria-labelledby')).toBe(metadata.focusTargetId);
    expect(heading.id).toBe(metadata.focusTargetId);
    expect(skipLink.targetId).toBe(metadata.focusTargetId);
    expect(skipLink.label).toBe('Skip to main content');
  });

  it('keeps the homepage page structure intentionally limited to the hero article', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const article = requireArticle(compiled);

    expect(compiled.children).toHaveLength(1);
    expect(compiled.firstElementChild).toBe(article);
    expect(article.querySelectorAll('header')).toHaveLength(1);
    expect(article.querySelectorAll('button')).toHaveLength(1);
    expect(article.querySelectorAll('img')).toHaveLength(1);
  });
});
