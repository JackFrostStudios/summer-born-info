import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '@design-system/button/button';
import { Panel, type PanelMediaWidth } from '@design-system/panel/panel';

@Component({
  selector: 'sbi-not-found',
  imports: [Button, Panel],
  templateUrl: './not-found.html',
  styleUrl: './not-found.scss',
})
export class NotFound {
  private readonly router = inject(Router);

  protected readonly headingId = 'not-found-heading';
  protected readonly compactMediaWidth: PanelMediaWidth = 'compact';

  protected goHome(): void {
    void this.router.navigateByUrl('/');
  }
}
