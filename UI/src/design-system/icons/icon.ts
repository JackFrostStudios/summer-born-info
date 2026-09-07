import { Component, input } from '@angular/core';

@Component({
  selector: 'sbi-icon',
  templateUrl: './icon.html',
  styleUrl: './icon.scss',
  host: {
    class: 'sbi-icon',
    '[attr.aria-hidden]': '$label() === null ? "true" : null',
    '[attr.aria-label]': '$label()',
    '[attr.role]': '$label() === null ? null : "img"',
  },
})
export class Icon {
  readonly $label = input<string | null>(null);
}
