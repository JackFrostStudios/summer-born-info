import { Routes } from '@angular/router';
import { defineRouteAccessibility, routeAccessibilityDataKey } from './app-route-accessibility';
import { Home } from './features/home/home';
import { NotFound } from './features/not-found/not-found';
import { RootShell } from './shell/root-shell/root-shell';

const homeRouteAccessibility = defineRouteAccessibility({
  title: 'Summer-born Info - Home',
  focusTargetId: 'home-heading',
  skipLinks: [{ label: 'Skip to main content', targetId: 'home-heading' }],
});

const underConstructionRouteAccessibility = defineRouteAccessibility({
  title: 'Summer-born Info - Page coming soon',
  focusTargetId: 'under-construction-heading',
  skipLinks: [{ label: 'Skip to main content', targetId: 'under-construction-heading' }],
});

const notFoundRouteAccessibility = defineRouteAccessibility({
  title: 'Summer-born Info - Page not found',
  focusTargetId: 'not-found-heading',
  skipLinks: [{ label: 'Skip to main content', targetId: 'not-found-heading' }],
});

export const routes: Routes = [
  {
    path: '',
    component: RootShell,
    children: [
      {
        path: '',
        pathMatch: 'full',
        component: Home,
        title: homeRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: homeRouteAccessibility,
        },
      },
      {
        path: 'under-construction',
        // Keep secondary routes lazy-loaded by default so future route growth does not bloat the homepage entry bundle.
        loadComponent: async () => (await import('./features/under-construction/under-construction')).UnderConstruction,
        title: underConstructionRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: underConstructionRouteAccessibility,
        },
      },
      {
        path: '**',
        component: NotFound,
        title: notFoundRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: notFoundRouteAccessibility,
        },
      },
    ],
  },
];
