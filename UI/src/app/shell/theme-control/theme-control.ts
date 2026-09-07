import { Component, computed, inject } from '@angular/core';
import { Button } from '@design-system/button';
import { MoonStarsIcon, SunIcon } from '@design-system/icons';
import { ThemeControlService } from './theme-control.service';

@Component({
  selector: 'sbi-theme-control',
  imports: [Button, MoonStarsIcon, SunIcon],
  templateUrl: './theme-control.html',
  styleUrl: './theme-control.scss',
  host: {
    '[attr.data-sbi-theme-control-mode]': '$selectedMode()',
  },
})
export class ThemeControl {
  private readonly colourMode = inject(ThemeControlService);
  protected readonly $selectedMode = this.colourMode.$mode;
  protected readonly $effectiveMode = this.colourMode.$effectiveMode;
  private readonly $isDarkMode = computed(() => this.$effectiveMode() === 'dark');

  protected readonly ariaLabel = $localize`:Theme toggle button label|Names the toggle that enables or disables dark mode@@themeToggleLabel:Dark mode`;

  protected readonly $ariaPressed = computed(() => (this.$isDarkMode() ? 'true' : 'false'));

  protected readonly iconVariant = 'secondary';
  protected readonly iconLayout = 'icon-only';

  protected toggleColourMode(): void {
    this.colourMode.toggleMode();
  }
}
