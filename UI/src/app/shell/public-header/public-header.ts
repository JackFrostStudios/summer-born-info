import { Component } from '@angular/core';
import { ThemeControl } from '../theme-control/theme-control';

@Component({
  selector: 'sbi-public-header',
  imports: [ThemeControl],
  templateUrl: './public-header.html',
  styleUrl: './public-header.scss',
  host: {
    '[attr.data-shell-header]': 'componentId',
  },
})
export class PublicHeader {
  protected readonly componentId = 'public-header';
}
