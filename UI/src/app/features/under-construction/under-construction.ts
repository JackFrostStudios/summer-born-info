import { Component, inject } from '@angular/core';
import { Navigation, Router } from '@angular/router';
import { Button } from '@design-system/button/button';
import { Panel } from '@design-system/panel/panel';

@Component({
  selector: 'sbi-under-construction',
  imports: [Button, Panel],
  templateUrl: './under-construction.html',
  styleUrl: './under-construction.scss',
})
export class UnderConstruction {
  private readonly router = inject(Router);

  protected readonly headingId = 'under-construction-heading';

  protected goBack(): void {
    const previousUrl = this.findLastNonUnderConstructionUrl(this.router.lastSuccessfulNavigation());

    void this.router.navigateByUrl(previousUrl ?? '/');
  }

  private findLastNonUnderConstructionUrl(navigation: Navigation | null | undefined): string | null {
    let currentNavigation = navigation?.previousNavigation ?? null;

    while (currentNavigation !== null) {
      const currentUrlTree = currentNavigation.finalUrl ?? currentNavigation.extractedUrl;
      const currentUrl = this.router.serializeUrl(currentUrlTree);

      if (!currentUrl.includes('under-construction')) {
        return currentUrl;
      }

      currentNavigation = currentNavigation.previousNavigation;
    }

    return null;
  }
}
