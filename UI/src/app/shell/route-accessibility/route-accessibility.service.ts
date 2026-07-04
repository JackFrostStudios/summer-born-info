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
import { NavigationEnd, NavigationSkipped, NavigationStart, Router } from '@angular/router';
import { filter } from 'rxjs';
import { getRouteAccessibilityMetadata, type RouteSkipLink } from '../../app-route-accessibility';

const defaultDocumentTitle = 'Summer-born Info';

@Injectable({ providedIn: 'root' })
export class RouteAccessibilityService {
  private readonly injector = inject(Injector);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly $navigationAnnouncementState = signal('');
  private readonly $skipLinksState = signal<readonly RouteSkipLink[]>([]);
  private shouldPreserveHistoryScrollPosition = false;

  readonly $navigationAnnouncement = this.$navigationAnnouncementState.asReadonly();
  readonly $skipLinks = this.$skipLinksState.asReadonly();

  constructor() {
    this.router.events
      .pipe(
        filter(
          (event): event is NavigationStart | NavigationEnd | NavigationSkipped =>
            event instanceof NavigationStart || event instanceof NavigationEnd || event instanceof NavigationSkipped,
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((event) => {
        if (event instanceof NavigationStart) {
          this.shouldPreserveHistoryScrollPosition =
            event.navigationTrigger === 'popstate' && event.restoredState !== null;
          return;
        }

        const shouldPreserveHistoryScrollPosition = this.shouldPreserveHistoryScrollPosition;

        runInInjectionContext(this.injector, () => {
          // Wait until the route view is rendered before reading or focusing its targets.
          afterNextRender(() => {
            this.applyRouteAccessibility(shouldPreserveHistoryScrollPosition);
          });
        });
      });
  }

  private applyRouteAccessibility(shouldPreserveHistoryScrollPosition: boolean): void {
    if (typeof document === 'undefined') {
      return;
    }

    this.refreshAccessibilityMetadata();
    this.setFocusToPageTarget(shouldPreserveHistoryScrollPosition);
  }

  private refreshAccessibilityMetadata(): void {
    const metadata = getRouteAccessibilityMetadata(this.router.routerState.snapshot);

    if (metadata === undefined) {
      if (document.title !== defaultDocumentTitle) {
        document.title = defaultDocumentTitle;
      }

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

  private setFocusToPageTarget(shouldPreserveHistoryScrollPosition: boolean): void {
    if (shouldPreserveHistoryScrollPosition) {
      this.focusBodyForSkipLinkEntry();
      return;
    }

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
