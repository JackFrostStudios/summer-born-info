import { Component, computed, inject } from '@angular/core';
import { Button } from '@design-system/button/button';
import { ThemeControlService } from './theme-control.service';

@Component({
  selector: 'sbi-theme-control',
  imports: [Button],
  templateUrl: './theme-control.html',
  styleUrl: './theme-control.scss',
})
export class ThemeControl {
  private readonly colourMode = inject(ThemeControlService);
  private readonly $effectiveMode = this.colourMode.$effectiveMode;
  private readonly $isDarkMode = computed(() => this.$effectiveMode() === 'dark');

  protected readonly buttonType = 'secondary' as const;

  protected readonly $ariaLabel = computed(() =>
    this.$isDarkMode()
      ? $localize`:Theme toggle button label|Switches the app from dark mode to light mode@@themeToggleToLightLabel:Switch to light mode`
      : $localize`:Theme toggle button label|Switches the app from light mode to dark mode@@themeToggleToDarkLabel:Switch to dark mode`,
  );
  protected readonly $ariaPressed = computed(() => (this.$isDarkMode() ? 'true' : 'false'));

  protected toggleColourMode(): void {
    this.colourMode.toggleMode();
  }
}
