import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ColourModeService, isColourMode } from './colour-mode.service';

@Component({
  selector: 'sbi-root-shell',
  imports: [RouterOutlet],
  templateUrl: './root-shell.html',
  styleUrl: './root-shell.scss',
  host: {
    '[attr.data-shell]': 'shellId',
  },
})
export class RootShell {
  protected readonly shellId = 'root-shell';
  protected readonly colourMode = inject(ColourModeService);
  protected selectedColourMode = this.colourMode.mode();

  protected selectColourMode(event: Event): void {
    const mode = (event.target as HTMLSelectElement).value;

    if (isColourMode(mode)) {
      this.colourMode.setMode(mode);
      this.selectedColourMode = mode;
    }
  }
}
