import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PublicFooter } from '../public-footer/public-footer';
import { PublicHeader } from '../public-header/public-header';

@Component({
  selector: 'sbi-root-shell',
  imports: [RouterOutlet, PublicHeader, PublicFooter],
  templateUrl: './root-shell.html',
  styleUrl: './root-shell.scss',
  host: {
    '[attr.data-shell]': 'shellId',
  },
})
export class RootShell {
  protected readonly shellId = 'root-shell';
}
