import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root-shell',
  imports: [RouterOutlet],
  templateUrl: './root-shell.html',
  styleUrl: './root-shell.scss',
  host: {
    '[attr.data-shell]': 'shellId',
  },
})
export class RootShell {
  protected readonly shellId = 'root-shell';
}
