import { Component, input } from '@angular/core';
import { FormField, type FieldTree } from '@angular/forms/signals';
import { createInputFeedback } from '../input-feedback';

@Component({
  selector: 'sbi-text-input',
  imports: [FormField],
  templateUrl: './text-input.html',
  styleUrl: './text-input.scss',
})
export class TextInput {
  readonly $field = input.required<FieldTree<string>>();
  readonly $inputId = input.required<string>();
  readonly $autocomplete = input<string | null>(null);
  readonly $ariaDescribedBy = input<string | null>(null);
  readonly $type = input<'text' | 'email'>('text');

  protected readonly $view = createInputFeedback(this.$field, this.$inputId, this.$ariaDescribedBy);
}
