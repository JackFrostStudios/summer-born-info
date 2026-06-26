import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  host: {
    '[attr.data-app-root]': 'componentId',
  },
})
export class App {
  protected readonly componentId = 'app-root';
}
