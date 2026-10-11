import { Component, computed, input } from '@angular/core';
import { FormField, type FieldTree } from '@angular/forms/signals';

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

  protected readonly $messages = computed(() =>
    this.$field()()
      .errors()
      .flatMap(({ message }) => (message?.trim() ? [message] : [])),
  );
  protected readonly $externalDescription = computed(() => {
    const description = this.$ariaDescribedBy()?.trim();
    if (!description) {
      return null;
    }
    return description;
  });
}
