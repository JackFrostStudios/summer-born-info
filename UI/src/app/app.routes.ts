import { Routes } from '@angular/router';
import { defineRouteAccessibility, routeAccessibilityDataKey } from './app-route-accessibility';
import { Home } from './features/home/home';
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

const openSourceLicencesRouteAccessibility = defineRouteAccessibility({
  title: 'Summer-born Info - Open source licences',
  focusTargetId: 'open-source-licences-heading',
  skipLinks: [{ label: 'Skip to main content', targetId: 'open-source-licences-heading' }],
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
        path: 'open-source-licences',
        loadComponent: async () =>
          (await import('./features/open-source-licences/open-source-licences')).OpenSourceLicences,
        title: openSourceLicencesRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: openSourceLicencesRouteAccessibility,
        },
      },
      {
        path: '**',
        loadComponent: async () => (await import('./features/not-found/not-found')).NotFound,
        title: notFoundRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: notFoundRouteAccessibility,
        },
      },
    ],
  },
];
