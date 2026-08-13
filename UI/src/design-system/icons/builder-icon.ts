import { Component, input } from '@angular/core';
import { Icon } from './icon';

@Component({
  selector: 'sbi-builder-icon',
  imports: [Icon],
  templateUrl: './builder-icon.html',
  styleUrl: './icon-artwork.scss',
})
export class BuilderIcon {
  readonly $label = input<string | null>(null);
}
