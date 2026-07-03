import { ActivatedRouteSnapshot, PRIMARY_OUTLET, RouterStateSnapshot, type Data } from '@angular/router';

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

export function getRouteAccessibilityMetadata(
  snapshot: ActivatedRouteSnapshot | RouterStateSnapshot,
): RouteAccessibilityMetadata | undefined {
  const route = snapshot instanceof RouterStateSnapshot ? snapshot.root : snapshot;
  const primaryRoute = findDeepestPrimaryRoute(route);

  return (primaryRoute.data as RouteAccessibilityData | undefined)?.[routeAccessibilityDataKey];
}

function findDeepestPrimaryRoute(snapshot: ActivatedRouteSnapshot): ActivatedRouteSnapshot {
  const primaryChild = snapshot.children.find((childSnapshot) => childSnapshot.outlet === PRIMARY_OUTLET);

  if (primaryChild === undefined) {
    return snapshot;
  }

  return findDeepestPrimaryRoute(primaryChild);
}
