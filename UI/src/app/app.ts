import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'sbi-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  host: {
    '[attr.data-sbi-root]': 'componentId',
  },
})
export class App {
  protected readonly componentId = 'sbi-root';
}
