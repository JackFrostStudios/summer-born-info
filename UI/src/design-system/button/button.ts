import { booleanAttribute, Component, input, output } from '@angular/core';

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

  protected handleClick(event: MouseEvent): void {
    this.pressed.emit(event);
  }
}
