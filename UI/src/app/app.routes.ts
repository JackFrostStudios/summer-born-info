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
        loadComponent: async () => (await import('./features/under-construction/under-construction')).UnderConstruction,
        title: underConstructionRouteAccessibility.title,
        data: {
          [routeAccessibilityDataKey]: underConstructionRouteAccessibility,
        },
      },
    ],
  },
];
