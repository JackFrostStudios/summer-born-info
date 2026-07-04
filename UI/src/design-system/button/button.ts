import { booleanAttribute, Component, computed, effect, input, isDevMode, output } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary';

@Component({
  selector: 'sbi-button',
  templateUrl: './button.html',
  styleUrl: './button.scss',
})
export class Button {
  readonly buttonType = input<ButtonVariant>('primary');
  readonly disabled = input(false, { transform: booleanAttribute });
  readonly ariaPressed = input<'true' | 'false' | null>(null);
  readonly ariaLabel = input<string | null>(null);
  readonly ariaLabelledBy = input<string | null>(null);
  readonly ariaDescribedBy = input<string | null>(null);
  readonly testId = input<string | null>(null);
  readonly pressed = output<MouseEvent>();
  protected readonly $forwardedAriaLabel = computed(() => this.normaliseAriaReference(this.ariaLabel()));
  protected readonly $forwardedAriaLabelledBy = computed(() => {
    const ariaLabelledBy = this.normaliseAriaReference(this.ariaLabelledBy());

    return this.$forwardedAriaLabel() === null ? ariaLabelledBy : null;
  });

  constructor() {
    effect(() => {
      if (!isDevMode()) {
        return;
      }

      if (this.$forwardedAriaLabel() !== null && this.normaliseAriaReference(this.ariaLabelledBy()) !== null) {
        console.warn(
          'sbi-button received both ariaLabel and ariaLabelledBy. Provide only one explicit accessible-name input; ariaLabel takes precedence and ariaLabelledBy will not be forwarded.',
        );
      }
    });
  }

  protected handleClick(event: MouseEvent): void {
    this.pressed.emit(event);
  }

  private normaliseAriaReference(value: string | null): string | null {
    const normalisedValue = value?.trim();

    if (!normalisedValue) {
      return null;
    }

    return normalisedValue;
  }
}
