import { Component, computed, inject } from '@angular/core';
import { ThemeControlService } from './theme-control.service';

@Component({
  selector: 'sbi-theme-control',
  templateUrl: './theme-control.html',
  styleUrl: './theme-control.scss',
})
export class ThemeControl {
  protected readonly colourMode = inject(ThemeControlService);
  protected readonly effectiveMode = this.colourMode.effectiveMode;
  protected readonly isDarkMode = computed(() => this.effectiveMode() === 'dark');

  protected get ariaPressed(): 'true' | 'false' {
    return this.isDarkMode() ? 'true' : 'false';
  }

  protected get darkModeActive(): boolean {
    return this.isDarkMode();
  }

  protected toggleColourMode(): void {
    this.colourMode.toggleMode();
  }
}
