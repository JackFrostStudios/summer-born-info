import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { inject, Injectable, PLATFORM_ID, signal } from '@angular/core';

export type ColourMode = 'system' | 'light' | 'dark';

const storedColourModes = ['light', 'dark'] as const;
const colourModes = ['system', ...storedColourModes] as const;
const storageKey = 'sbi:colour-mode';
const rootAttribute = 'data-sbi-colour-mode';

export function isColourMode(value: string): value is ColourMode {
  return colourModes.includes(value as ColourMode);
}

@Injectable({ providedIn: 'root' })
export class ColourModeService {
  private readonly document = inject(DOCUMENT);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly selectedMode = signal<ColourMode>(this.readStoredMode());

  readonly mode = this.selectedMode.asReadonly();

  constructor() {
    this.applyMode(this.selectedMode());
  }

  setMode(mode: ColourMode): void {
    this.selectedMode.set(mode);
    this.persistMode(mode);
    this.applyMode(mode);
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
}
