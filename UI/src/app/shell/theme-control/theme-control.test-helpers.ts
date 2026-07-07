import { vi } from 'vitest';

const storageKey = 'sbi:colour-mode';
const rootAttribute = 'data-sbi-colour-mode';

export { rootAttribute, storageKey };

export type MatchMediaStub = MediaQueryList & {
  addEventListener: ReturnType<typeof vi.fn>;
  removeEventListener: ReturnType<typeof vi.fn>;
  setMatches(matches: boolean): void;
};

interface LocalStorageStubOptions {
  getItemError?: Error;
  initialValues?: Readonly<Record<string, string>>;
  removeItemError?: Error;
  setItemError?: Error;
}

export type LocalStorageStub = Storage & {
  getItem: ReturnType<typeof vi.fn>;
  read(key: string): string | null;
  removeItem: ReturnType<typeof vi.fn>;
  setItem: ReturnType<typeof vi.fn>;
};

export function installLocalStorageStub(options: LocalStorageStubOptions = {}): LocalStorageStub {
  const storedValues = new Map(Object.entries(options.initialValues ?? {}));

  const localStorageStub = {
    get length() {
      return storedValues.size;
    },
    clear: vi.fn(() => {
      storedValues.clear();
    }),
    getItem: vi.fn((key: string) => {
      if (options.getItemError !== undefined) {
        throw options.getItemError;
      }

      return storedValues.get(key) ?? null;
    }),
    key: vi.fn((index: number) => Array.from(storedValues.keys())[index] ?? null),
    read: (key: string) => storedValues.get(key) ?? null,
    removeItem: vi.fn((key: string) => {
      if (options.removeItemError !== undefined) {
        throw options.removeItemError;
      }

      storedValues.delete(key);
    }),
    setItem: vi.fn((key: string, value: string) => {
      if (options.setItemError !== undefined) {
        throw options.setItemError;
      }

      storedValues.set(key, value);
    }),
  } satisfies LocalStorageStub;

  Object.defineProperty(globalThis, 'localStorage', {
    configurable: true,
    value: localStorageStub,
  });

  return localStorageStub;
}

export function installMatchMediaStub(initialMatches = false): MatchMediaStub {
  let matches = initialMatches;
  let listener: ((event: MediaQueryListEvent) => void) | { handleEvent(event: MediaQueryListEvent): void } | null =
    null;

  const mediaQuery = {
    media: '(prefers-color-scheme: dark)',
    onchange: null,
    addEventListener: vi.fn((_type: string, nextListener: EventListenerOrEventListenerObject | null) => {
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
    }),
    removeEventListener: vi.fn(() => {
      listener = null;
    }),
    addListener: vi.fn((nextListener: ((event: MediaQueryListEvent) => void) | null) => {
      listener = nextListener;
    }),
    removeListener: vi.fn(() => {
      listener = null;
    }),
    dispatchEvent: vi.fn(() => true),
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
    value: vi.fn(() => mediaQuery),
  });

  return mediaQuery;
}
