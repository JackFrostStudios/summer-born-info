import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { RouteAccessibilityService } from '../route-accessibility/route-accessibility.service';
import { PublicFooter } from '../public-footer/public-footer';
import { PublicHeader } from '../public-header/public-header';
import { SkipLinks } from '../skip-links/skip-links';

@Component({
  selector: 'sbi-root-shell',
  imports: [RouterOutlet, PublicHeader, PublicFooter, SkipLinks],
  templateUrl: './root-shell.html',
  styleUrl: './root-shell.scss',
  host: {
    '[attr.data-shell]': 'shellId',
  },
})
export class RootShell {
  private readonly routeAccessibility = inject(RouteAccessibilityService);

  protected readonly shellId = 'root-shell';
  protected readonly $navigationAnnouncement = this.routeAccessibility.$navigationAnnouncement;
  protected readonly $skipLinks = this.routeAccessibility.$skipLinks;
}
