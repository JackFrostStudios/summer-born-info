import { TestBed } from '@angular/core/testing';
import { ThemeControlService } from './theme-control.service';

const storageKey = 'sbi:colour-mode';
const rootAttribute = 'data-sbi-colour-mode';

type MatchMediaStub = MediaQueryList & {
  setMatches(matches: boolean): void;
};

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

function installMatchMediaStub(initialMatches = false): MatchMediaStub {
  let matches = initialMatches;
  let listener:
    | ((event: MediaQueryListEvent) => void)
    | { handleEvent(event: MediaQueryListEvent): void }
    | null = null;

  const mediaQuery = {
    media: '(prefers-color-scheme: dark)',
    onchange: null,
    addEventListener: (_type: string, nextListener: EventListenerOrEventListenerObject | null) => {
      if (typeof nextListener === 'function') {
        listener = (event: MediaQueryListEvent) => {
          nextListener(event);
        };
        return;
      }

      listener =
        nextListener === null
          ? null
          : {
              handleEvent: (event) => {
                nextListener.handleEvent(event);
              },
            };
    },
    removeEventListener: () => {
      listener = null;
    },
    addListener: (nextListener: ((event: MediaQueryListEvent) => void) | null) => {
      listener = nextListener;
    },
    removeListener: () => {
      listener = null;
    },
    dispatchEvent: () => true,
    setMatches: (nextMatches: boolean) => {
      matches = nextMatches;
      if (typeof listener === 'function') {
        listener({ matches: nextMatches } as MediaQueryListEvent);
        return;
      }

      listener?.handleEvent({ matches: nextMatches } as MediaQueryListEvent);
    },
  } as unknown as MatchMediaStub;

  Object.defineProperty(mediaQuery, 'matches', {
    configurable: true,
    get: () => matches,
  });

  Object.defineProperty(globalThis, 'matchMedia', {
    configurable: true,
    value: () => mediaQuery,
  });

  return mediaQuery;
}

describe('ThemeControlService', () => {
  beforeEach(() => {
    installLocalStorageStub();
    installMatchMediaStub();
  });

  afterEach(() => {
    document.documentElement.removeAttribute(rootAttribute);
    TestBed.resetTestingModule();
  });

  it('starts in system mode without a persisted override', () => {
    const service = TestBed.inject(ThemeControlService);

    expect(service.mode()).toBe('system');
    expect(service.effectiveMode()).toBe('light');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorage.getItem(storageKey)).toBeNull();
  });

  it('applies and persists an explicit light mode selection', () => {
    const service = TestBed.inject(ThemeControlService);

    service.setMode('light');

    expect(service.mode()).toBe('light');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('light');
    expect(localStorage.getItem(storageKey)).toBe('light');
  });

  it('applies and persists an explicit dark mode selection', () => {
    const service = TestBed.inject(ThemeControlService);

    service.setMode('dark');

    expect(service.mode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorage.getItem(storageKey)).toBe('dark');
  });

  it('restores a persisted dark mode override on creation', () => {
    localStorage.setItem(storageKey, 'dark');

    const service = TestBed.inject(ThemeControlService);

    expect(service.mode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
  });

  it('toggles from the current effective system preference when no explicit mode is set', () => {
    installMatchMediaStub(true);
    const service = TestBed.inject(ThemeControlService);

    expect(service.mode()).toBe('system');
    expect(service.effectiveMode()).toBe('dark');

    service.toggleMode();

    expect(service.mode()).toBe('light');
    expect(service.effectiveMode()).toBe('light');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('light');
    expect(localStorage.getItem(storageKey)).toBe('light');
  });

  it('tracks system preference changes while following system mode', () => {
    const matchMediaStub = installMatchMediaStub(false);
    const service = TestBed.inject(ThemeControlService);

    expect(service.effectiveMode()).toBe('light');

    matchMediaStub.setMatches(true);

    expect(service.mode()).toBe('system');
    expect(service.effectiveMode()).toBe('dark');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
  });
});
