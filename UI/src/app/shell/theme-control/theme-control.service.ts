import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { computed, DestroyRef, inject, PLATFORM_ID, Service, signal } from '@angular/core';

export type ColourMode = 'system' | 'light' | 'dark';
export type ExplicitColourMode = Exclude<ColourMode, 'system'>;

const storedColourModes = ['light', 'dark'] as const satisfies readonly ExplicitColourMode[];
const colourModes = ['system', ...storedColourModes] as const;
const storageKey = 'sbi:colour-mode';
const rootAttribute = 'data-sbi-colour-mode';

export function isColourMode(value: string): value is ColourMode {
  return colourModes.includes(value as ColourMode);
}

@Service()
export class ThemeControlService {
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly $selectedMode = signal<ColourMode>(this.readStoredMode());
  private readonly $systemMode = signal<ExplicitColourMode>(this.readSystemMode());

  readonly $mode = this.$selectedMode.asReadonly();
  readonly $effectiveMode = computed(() => this.currentExplicitMode());

  constructor() {
    this.syncSystemModePreference();
    this.applyMode(this.$selectedMode());
  }

  setMode(mode: ColourMode): void {
    this.$selectedMode.set(mode);
    this.persistMode(mode);
    this.applyMode(mode);
  }

  toggleMode(): void {
    this.setMode(this.nextExplicitMode());
  }

  nextExplicitMode(): ExplicitColourMode {
    return this.currentExplicitMode() === 'dark' ? 'light' : 'dark';
  }

  currentExplicitMode(): ExplicitColourMode {
    const selectedMode = this.$selectedMode();

    if (selectedMode === 'system') {
      return this.$systemMode();
    }

    return selectedMode;
  }

  private readStoredMode(): ColourMode {
    const storedMode = this.readStoredValue();

    if (storedColourModes.includes(storedMode as (typeof storedColourModes)[number])) {
      return storedMode as ColourMode;
    }

    if (storedMode !== null) {
      this.removeStoredValue();
    }

    return 'system';
  }

  private applyMode(mode: ColourMode): void {
    if (mode === 'system') {
      this.document.documentElement.removeAttribute(rootAttribute);
      return;
    }

    this.document.documentElement.setAttribute(rootAttribute, mode);
  }

  private persistMode(mode: ColourMode): void {
    if (mode === 'system') {
      this.removeStoredValue();
      return;
    }

    this.writeStoredValue(mode);
  }

  private readStoredValue(): string | null {
    if (!this.isBrowser) {
      return null;
    }

    try {
      return globalThis.localStorage.getItem(storageKey);
    } catch {
      return null;
    }
  }

  private writeStoredValue(mode: Exclude<ColourMode, 'system'>): void {
    if (!this.isBrowser) {
      return;
    }

    try {
      globalThis.localStorage.setItem(storageKey, mode);
    } catch {
      // Storage can be unavailable in restricted browser contexts; the in-memory mode still applies.
    }
  }

  private removeStoredValue(): void {
    if (!this.isBrowser) {
      return;
    }

    try {
      globalThis.localStorage.removeItem(storageKey);
    } catch {
      // Storage can be unavailable in restricted browser contexts; clearing is best-effort.
    }
  }

  private readSystemMode(): ExplicitColourMode {
    if (!this.isBrowser || typeof globalThis.matchMedia !== 'function') {
      return 'light';
    }

    return globalThis.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  private syncSystemModePreference(): void {
    if (!this.isBrowser || typeof globalThis.matchMedia !== 'function') {
      return;
    }

    const mediaQuery = globalThis.matchMedia('(prefers-color-scheme: dark)');
    const updateSystemMode = (event?: MediaQueryListEvent): void => {
      this.$systemMode.set((event?.matches ?? mediaQuery.matches) ? 'dark' : 'light');
    };

    updateSystemMode();

    mediaQuery.addEventListener('change', updateSystemMode);
    this.destroyRef.onDestroy(() => {
      mediaQuery.removeEventListener('change', updateSystemMode);
    });
  }
}
