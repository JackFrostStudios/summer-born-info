import { Component, input } from '@angular/core';
import { Icon } from './icon';

@Component({
  selector: 'sbi-sun-icon',
  imports: [Icon],
  templateUrl: './sun-icon.html',
  styleUrl: './icon-artwork.scss',
})
export class SunIcon {
  readonly $label = input<string | null>(null);
}
