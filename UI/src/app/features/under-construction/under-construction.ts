import { Component, inject } from '@angular/core';
import { Navigation, PRIMARY_OUTLET, Router, UrlTree } from '@angular/router';
import { Button } from '@design-system/button';
import { Icon, type IconName } from '@design-system/icons';
import { Panel } from '@design-system/panel';

@Component({
  selector: 'sbi-under-construction',
  imports: [Button, Icon, Panel],
  templateUrl: './under-construction.html',
  styleUrl: './under-construction.scss',
})
export class UnderConstruction {
  private readonly router = inject(Router);

  protected readonly builderIconName: IconName = 'builder';
  protected readonly headingId = 'under-construction-heading';

  protected goBack(): void {
    const previousUrl = this.findLastNonUnderConstructionUrl(this.router.lastSuccessfulNavigation());

    void this.router.navigateByUrl(previousUrl ?? '/');
  }

  private findLastNonUnderConstructionUrl(navigation: Navigation | null | undefined): string | null {
    let currentNavigation = navigation?.previousNavigation ?? null;

    while (currentNavigation !== null) {
      const currentUrlTree = currentNavigation.finalUrl ?? currentNavigation.extractedUrl;

      if (!this.isUnderConstructionRoute(currentUrlTree)) {
        return this.router.serializeUrl(currentUrlTree);
      }

      currentNavigation = currentNavigation.previousNavigation;
    }

    return null;
  }

  private isUnderConstructionRoute(urlTree: UrlTree): boolean {
    const primarySegments = urlTree.root.children[PRIMARY_OUTLET]?.segments;

    return primarySegments?.length === 1 && primarySegments[0]?.path === 'under-construction';
  }
}
