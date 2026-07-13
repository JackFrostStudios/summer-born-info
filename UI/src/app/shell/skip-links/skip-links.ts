import { Component, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { type RouteSkipLink } from '../../app-route-accessibility';

@Component({
  selector: 'sbi-skip-links',
  imports: [RouterLink],
  templateUrl: './skip-links.html',
  styleUrl: './skip-links.scss',
  host: {
    class: 'skip-links',
    '[class.skip-links--visible]': '$isVisible()',
    '(focusin)': 'handleFocusIn()',
    '(focusout)': 'handleFocusOut($event)',
  },
})
export class SkipLinks {
  readonly $links = input<readonly RouteSkipLink[]>([]);

  protected readonly $isVisible = signal(false);

  protected handleFocusIn(): void {
    this.$isVisible.set(true);
  }

  protected handleFocusOut(event: FocusEvent): void {
    const currentTarget = event.currentTarget;
    const nextFocusTarget = event.relatedTarget;

    if (
      !(currentTarget instanceof HTMLElement) ||
      !(nextFocusTarget instanceof Node) ||
      !currentTarget.contains(nextFocusTarget)
    ) {
      this.$isVisible.set(false);
    }
  }
}
