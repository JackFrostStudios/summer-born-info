import type { Data } from '@angular/router';

export const routeAccessibilityDataKey = 'accessibility';

export type RouteSkipLink = Readonly<{
  label: string;
  targetId: string;
}>;

export type RouteAccessibilityMetadata = Readonly<{
  title: string;
  focusTargetId: string;
  skipLinks: readonly RouteSkipLink[];
}>;

export type RouteAccessibilityData = Data & {
  [routeAccessibilityDataKey]?: RouteAccessibilityMetadata;
};

export function defineRouteAccessibility(metadata: RouteAccessibilityMetadata): RouteAccessibilityMetadata {
  return metadata;
}
