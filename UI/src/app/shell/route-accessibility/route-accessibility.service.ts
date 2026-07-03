import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  afterNextRender,
  DestroyRef,
  inject,
  Injectable,
  Injector,
  runInInjectionContext,
  signal,
} from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { getRouteAccessibilityMetadata, type RouteSkipLink } from '../../app-route-accessibility';

@Injectable({ providedIn: 'root' })
export class RouteAccessibilityService {
  private readonly injector = inject(Injector);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly $navigationAnnouncementState = signal('');
  private readonly $skipLinksState = signal<readonly RouteSkipLink[]>([]);

  readonly $navigationAnnouncement = this.$navigationAnnouncementState.asReadonly();
  readonly $skipLinks = this.$skipLinksState.asReadonly();

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        runInInjectionContext(this.injector, () => {
          // Wait until the route view is rendered before reading or focusing its targets.
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

  private refreshAccessibilityMetadata(): void {
    const metadata = getRouteAccessibilityMetadata(this.router.routerState.snapshot);

    if (metadata === undefined) {
      this.$navigationAnnouncementState.set('');
      this.$skipLinksState.set([]);
      return;
    }

    if (document.title !== metadata.title) {
      document.title = metadata.title;
    }

    this.$navigationAnnouncementState.set(metadata.title);
    this.$skipLinksState.set(metadata.skipLinks);
  }

  private setFocusToPageTarget(): void {
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
