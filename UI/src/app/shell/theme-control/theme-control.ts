import { Component, computed, inject } from '@angular/core';
import { Button } from '@design-system/button';
import { Icon } from '@design-system/icons';
import { ThemeControlService } from './theme-control.service';

@Component({
  selector: 'sbi-theme-control',
  imports: [Button, Icon],
  templateUrl: './theme-control.html',
  styleUrl: './theme-control.scss',
})
export class ThemeControl {
  private readonly colourMode = inject(ThemeControlService);
  private readonly $effectiveMode = this.colourMode.$effectiveMode;
  private readonly $isDarkMode = computed(() => this.$effectiveMode() === 'dark');

  protected readonly variant = 'secondary' as const;
  protected readonly layout = 'icon-only' as const;
  protected readonly ariaLabel = $localize`:Theme toggle button label|Names the toggle that enables or disables dark mode@@themeToggleLabel:Dark mode`;

  protected readonly $ariaPressed = computed(() => (this.$isDarkMode() ? 'true' : 'false'));

  protected toggleColourMode(): void {
    this.colourMode.toggleMode();
  }
}
