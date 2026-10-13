import { Component, input } from '@angular/core';
import { FormField, type FieldTree } from '@angular/forms/signals';
import { createInputFeedback } from '../input-feedback';

@Component({
  selector: 'sbi-password-input',
  imports: [FormField],
  templateUrl: './password-input.html',
  styleUrl: './password-input.scss',
})
export class PasswordInput {
  readonly $field = input.required<FieldTree<string>>();
  readonly $inputId = input.required<string>();
  readonly $autocomplete = input<string | null>(null);
  readonly $ariaDescribedBy = input<string | null>(null);

  protected readonly $view = createInputFeedback(this.$field, this.$inputId, this.$ariaDescribedBy);
}
