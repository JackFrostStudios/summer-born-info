import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { getRouteAccessibilityMetadata } from '../../app-route-accessibility';
import { PublicFooter } from '../public-footer/public-footer';
import { PublicHeader } from '../public-header/public-header';

@Component({
  selector: 'sbi-root-shell',
  imports: [RouterOutlet, PublicHeader, PublicFooter],
  templateUrl: './root-shell.html',
  styleUrl: './root-shell.scss',
  host: {
    '[attr.data-shell]': 'shellId',
  },
})
export class RootShell {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly shellId = 'root-shell';
  protected readonly $navigationAnnouncement = signal('');

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.scheduleRouteAccessibilityUpdate();
      });

    this.scheduleRouteAccessibilityUpdate();
  }

  protected handleRouteActivation(): void {
    this.scheduleRouteAccessibilityUpdate();
  }

  private applyRouteAccessibility(): void {
    if (typeof document === 'undefined') {
      return;
    }

    const metadata = getRouteAccessibilityMetadata(this.router.routerState.snapshot);

    if (metadata === undefined) {
      this.$navigationAnnouncement.set('');
      return;
    }

    if (document.title !== metadata.title) {
      document.title = metadata.title;
    }

    this.$navigationAnnouncement.set(metadata.title);

    const focusTarget = this.resolveFocusTarget(metadata.focusTargetId);

    if (focusTarget === null) {
      return;
    }

    if (focusTarget.tabIndex < 0) {
      focusTarget.setAttribute('tabindex', '-1');
    }

    focusTarget.focus();
  }

  private resolveFocusTarget(defaultTargetId: string): HTMLElement | null {
    const fragment = this.router.parseUrl(this.router.url).fragment;

    if (fragment !== null) {
      const fragmentTarget = document.getElementById(fragment);

      if (fragmentTarget instanceof HTMLElement) {
        return fragmentTarget;
      }
    }

    const defaultTarget = document.getElementById(defaultTargetId);

    return defaultTarget instanceof HTMLElement ? defaultTarget : null;
  }

  private scheduleRouteAccessibilityUpdate(): void {
    setTimeout(() => {
      this.applyRouteAccessibility();
    });
  }
}
