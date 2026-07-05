import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HydrationFeatureKind } from '@angular/platform-browser';

import { App } from './app';
import { appConfig, appHydrationFeatures, appInMemoryScrollingOptions } from './app.config';
import { getRouteAccessibilityMetadata } from './app-route-accessibility';

describe('app config', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [...appConfig.providers],
    }).compileComponents();
  });

  afterEach(() => {
    document.title = '';
    TestBed.resetTestingModule();
  });

  it('declares enabled in-memory scrolling for route fragments and history navigation', () => {
    expect(appInMemoryScrollingOptions).toEqual({
      anchorScrolling: 'enabled',
      scrollPositionRestoration: 'enabled',
    });
  });

  it('enables hydration support for i18n blocks and pre-hydration event replay', () => {
    expect(appHydrationFeatures.map((feature) => Reflect.get(feature, 'ɵkind'))).toEqual([
      HydrationFeatureKind.I18nSupport,
      HydrationFeatureKind.EventReplay,
    ]);
  });

  it('updates the document title from the active route and keeps accessibility metadata available', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);

    router.initialNavigation();
    await router.navigateByUrl('/under-construction');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(document.title).toBe('Summer-born Info - Page coming soon');
    expect(getRouteAccessibilityMetadata(router.routerState.snapshot)).toEqual({
      title: 'Summer-born Info - Page coming soon',
      focusTargetId: 'under-construction-heading',
      skipLinks: [{ label: 'Skip to main content', targetId: 'under-construction-heading' }],
    });
  });
});
