import { Component, inject } from '@angular/core';
import { ColourModeService, isColourMode } from './colour-mode.service';

@Component({
  selector: 'sbi-theme-control',
  templateUrl: './theme-control.html',
  styleUrl: './theme-control.scss',
})
export class ThemeControl {
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
