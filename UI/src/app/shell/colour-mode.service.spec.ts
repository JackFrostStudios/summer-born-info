import { TestBed } from '@angular/core/testing';
import { ColourModeService } from './colour-mode.service';

const storageKey = 'sbi:colour-mode';
const rootAttribute = 'data-sbi-colour-mode';

function installLocalStorageStub(): void {
  const storedValues = new Map<string, string>();
  const localStorageStub = {
    get length() {
      return storedValues.size;
    },
    clear: () => {
      storedValues.clear();
    },
    getItem: (key: string) => storedValues.get(key) ?? null,
    key: (index: number) => Array.from(storedValues.keys())[index] ?? null,
    removeItem: (key: string) => {
      storedValues.delete(key);
    },
    setItem: (key: string, value: string) => {
      storedValues.set(key, value);
    },
  } satisfies Storage;

  Object.defineProperty(globalThis, 'localStorage', {
    configurable: true,
    value: localStorageStub,
  });
}

describe('ColourModeService', () => {
  beforeEach(() => {
    installLocalStorageStub();
  });

  afterEach(() => {
    document.documentElement.removeAttribute(rootAttribute);
    TestBed.resetTestingModule();
  });

  it('starts in system mode without a persisted override', () => {
    const service = TestBed.inject(ColourModeService);

    expect(service.mode()).toBe('system');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorage.getItem(storageKey)).toBeNull();
  });

  it('applies and persists an explicit light mode selection', () => {
    const service = TestBed.inject(ColourModeService);

    service.setMode('light');

    expect(service.mode()).toBe('light');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('light');
    expect(localStorage.getItem(storageKey)).toBe('light');
  });

  it('applies and persists an explicit dark mode selection', () => {
    const service = TestBed.inject(ColourModeService);

    service.setMode('dark');

    expect(service.mode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorage.getItem(storageKey)).toBe('dark');
  });

  it('restores a persisted dark mode override on creation', () => {
    localStorage.setItem(storageKey, 'dark');

    const service = TestBed.inject(ColourModeService);

    expect(service.mode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
  });

  it('clears persistence and the root override when reset to system default', () => {
    const service = TestBed.inject(ColourModeService);
    service.setMode('dark');

    service.setMode('system');

    expect(service.mode()).toBe('system');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorage.getItem(storageKey)).toBeNull();
  });
});
