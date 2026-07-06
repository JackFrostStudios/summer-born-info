import { Route } from '@angular/router';
import {
  routeAccessibilityDataKey,
  type RouteAccessibilityData,
  type RouteAccessibilityMetadata,
} from './app-route-accessibility';
import { routes } from './app.routes';
import { Home } from './features/home/home';
import { NotFound } from './features/not-found/not-found';
import { UnderConstruction } from './features/under-construction/under-construction';
import { RootShell } from './shell/root-shell/root-shell';

function requireRoute(routesToSearch: readonly Route[], path: string): Route {
  const route = routesToSearch.find((candidate) => candidate.path === path);

  if (route === undefined) {
    throw new Error(`Expected to find the "${path}" route.`);
  }

  return route;
}

function requireAccessibilityMetadata(route: Route): RouteAccessibilityMetadata {
  const metadata = (route.data as RouteAccessibilityData | undefined)?.[routeAccessibilityDataKey];

  if (metadata === undefined) {
    throw new Error(`Expected the "${route.path ?? '<root>'}" route to declare accessibility metadata.`);
  }

  return metadata;
}

describe('app routes', () => {
  it('renders the root shell as the shared host for child routes', () => {
    expect(routes).toHaveLength(1);

    const [shellRoute] = routes;

    expect(shellRoute?.path).toBe('');
    expect(shellRoute?.component).toBe(RootShell);
    expect(shellRoute?.children).toBeDefined();
  });

  it('keeps the homepage title, focus target, and skip links together in route metadata', () => {
    const shellRoute = requireRoute(routes, '');
    const homeRoute = requireRoute(shellRoute.children ?? [], '');
    const metadata = requireAccessibilityMetadata(homeRoute);

    expect(homeRoute.component).toBe(Home);
    expect(homeRoute.pathMatch).toBe('full');
    expect(homeRoute.title).toBe('Summer-born Info - Home');
    expect(metadata).toEqual({
      title: 'Summer-born Info - Home',
      focusTargetId: 'home-heading',
      skipLinks: [{ label: 'Skip to main content', targetId: 'home-heading' }],
    });
  });

  it('keeps the secondary under-construction route lazy-loaded while preserving its title and accessibility metadata', async () => {
    const shellRoute = requireRoute(routes, '');
    const underConstructionRoute = requireRoute(shellRoute.children ?? [], 'under-construction');
    const metadata = requireAccessibilityMetadata(underConstructionRoute);

    expect(underConstructionRoute.component).toBeUndefined();
    expect(underConstructionRoute.loadComponent).toBeDefined();
    await expect(underConstructionRoute.loadComponent?.()).resolves.toBe(UnderConstruction);
    expect(underConstructionRoute.title).toBe('Summer-born Info - Page coming soon');
    expect(metadata).toEqual({
      title: 'Summer-born Info - Page coming soon',
      focusTargetId: 'under-construction-heading',
      skipLinks: [{ label: 'Skip to main content', targetId: 'under-construction-heading' }],
    });
  });

  it('registers the final wildcard route inside the shared shell with a title and accessibility metadata', () => {
    const shellRoute = requireRoute(routes, '');
    const notFoundRoute = requireRoute(shellRoute.children ?? [], '**');
    const metadata = requireAccessibilityMetadata(notFoundRoute);

    expect(notFoundRoute.component).toBe(NotFound);
    expect(notFoundRoute.title).toBe('Summer-born Info - Page not found');
    expect(metadata).toEqual({
      title: 'Summer-born Info - Page not found',
      focusTargetId: 'not-found-heading',
      skipLinks: [{ label: 'Skip to main content', targetId: 'not-found-heading' }],
    });
  });
});
