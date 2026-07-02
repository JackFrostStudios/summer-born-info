import { DOCUMENT, Location } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '@design-system/button/button';

@Component({
  selector: 'sbi-under-construction',
  imports: [Button],
  templateUrl: './under-construction.html',
  styleUrl: './under-construction.scss',
})
export class UnderConstruction {
  private readonly document = inject(DOCUMENT);
  private readonly location = inject(Location);
  private readonly router = inject(Router);

  protected readonly headingId = 'under-construction-heading';

  protected goBack(): void {
    if ((this.document.defaultView?.history.length ?? 0) > 1) {
      this.location.back();
      return;
    }

    void this.router.navigateByUrl('/');
  }
}
