import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '@design-system/button';
import { NotFoundIcon } from '@design-system/icons';
import { Panel } from '@design-system/panel';

@Component({
  selector: 'sbi-not-found',
  imports: [Button, NotFoundIcon, Panel],
  templateUrl: './not-found.html',
  styleUrl: './not-found.scss',
})
export class NotFound {
  private readonly router = inject(Router);

  protected readonly headingId = 'not-found-heading';

  protected goHome(): void {
    void this.router.navigateByUrl('/');
  }
}
