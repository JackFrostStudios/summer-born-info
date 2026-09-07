import { Component, input } from '@angular/core';
import { Icon } from './icon';

@Component({
  selector: 'sbi-not-found-icon',
  imports: [Icon],
  templateUrl: './not-found-icon.html',
  styleUrl: './icon-artwork.scss',
})
export class NotFoundIcon {
  readonly $label = input<string | null>(null);
}
