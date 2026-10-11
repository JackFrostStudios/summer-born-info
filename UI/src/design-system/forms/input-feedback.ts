import { computed, type Signal } from '@angular/core';
import { type FieldTree } from '@angular/forms/signals';

export function createInputFeedback(
  $field: Signal<FieldTree<string>>,
  $inputId: Signal<string>,
  $ariaDescribedBy: Signal<string | null>,
) {
  return computed(() => {
    const state = $field()();
    const visible = state.touched() && state.invalid();
    const messages = state.errors().flatMap(({ message }) => (message?.trim() ? [message] : []));
    const externalDescription = $ariaDescribedBy()?.trim();
    // Blank strings intentionally omit the attribute along with null/undefined.
    // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
    const aria = visible && messages.length ? `${$inputId()}-error` : externalDescription || null;
    return { visible, messages, aria };
  });
}
