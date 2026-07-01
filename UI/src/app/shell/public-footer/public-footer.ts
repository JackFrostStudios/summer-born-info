import { Component } from '@angular/core';

@Component({
  selector: 'sbi-public-footer',
  templateUrl: './public-footer.html',
  styleUrl: './public-footer.scss',
  host: {
    '[attr.data-shell-footer]': 'componentId',
  },
})
export class PublicFooter {
  protected readonly componentId = 'public-footer';
}
