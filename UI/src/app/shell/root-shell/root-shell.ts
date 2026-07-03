import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { afterNextRender, Component, DestroyRef, inject, Injector, runInInjectionContext, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { getRouteAccessibilityMetadata, type RouteSkipLink } from '../../app-route-accessibility';
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
  private injector = inject(Injector);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly shellId = 'root-shell';
  protected readonly $navigationAnnouncement = signal('');
  protected readonly $skipLinks = signal<readonly RouteSkipLink[]>([]);

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        runInInjectionContext(this.injector, () => {
          // Refresh accessibility after the initial render to ensure that elements are available to gain focus and page is ready before being anounced.
          afterNextRender(() => {
            this.applyRouteAccessibility();
          });
        });
      });
  }

  private applyRouteAccessibility(): void {
    if (typeof document === 'undefined') {
      return;
    }

    this.refreshAccessibilityMetadata();
    this.setFocusToPageTarget();
  }

  private refreshAccessibilityMetadata() {
    const metadata = getRouteAccessibilityMetadata(this.router.routerState.snapshot);

    if (metadata === undefined) {
      this.$navigationAnnouncement.set('');
      this.$skipLinks.set([]);
      return;
    }

    if (document.title !== metadata.title) {
      document.title = metadata.title;
    }

    this.$navigationAnnouncement.set(metadata.title);
    this.$skipLinks.set(metadata.skipLinks);
  }

  private setFocusToPageTarget() {
    const fragmentTarget = this.resolveFragmentFocusTarget();

    if (fragmentTarget !== null) {
      this.focusFragmentTarget(fragmentTarget);
      return;
    }

    this.focusBodyForSkipLinkEntry();
  }

  private focusBodyForSkipLinkEntry(): void {
    document.body.focus();
  }

  private resolveFragmentFocusTarget(): HTMLElement | null {
    const fragment = this.router.parseUrl(this.router.url).fragment;

    if (fragment === null) {
      return null;
    }

    const fragmentTarget = document.getElementById(fragment);

    return fragmentTarget instanceof HTMLElement ? fragmentTarget : null;
  }

  private focusFragmentTarget(target: HTMLElement): void {
    target.focus();
  }
}
