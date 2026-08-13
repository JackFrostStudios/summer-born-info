import { Component, input } from '@angular/core';
import { Icon } from './icon';

@Component({
  selector: 'sbi-moon-stars-icon',
  imports: [Icon],
  templateUrl: './moon-stars-icon.html',
  styleUrl: './icon-artwork.scss',
})
export class MoonStarsIcon {
  readonly $label = input<string | null>(null);
}
