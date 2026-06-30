import { TestBed } from '@angular/core/testing';
import { ThemeControl } from './theme-control';

const storageKey = 'sbi:colour-mode';
const rootAttribute = 'data-sbi-colour-mode';

type MatchMediaStub = MediaQueryList & {
  setMatches(matches: boolean): void;
};

function requireElement<T extends Element>(element: T | null, message: string): T {
  if (element === null) {
    throw new Error(message);
  }

  return element;
}

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

describe('ThemeControl', () => {
  beforeEach(() => {
    installLocalStorageStub();
    installMatchMediaStub();
  });

  afterEach(() => {
    document.documentElement.removeAttribute(rootAttribute);
    TestBed.resetTestingModule();
  });

  it('renders an accessible toggle with a reset action', () => {
    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    const toggle = compiled.querySelector<HTMLButtonElement>('.theme-control__toggle');
    const reset = compiled.querySelector<HTMLButtonElement>('.theme-control__reset');

    expect(toggle).not.toBeNull();
    expect(reset).not.toBeNull();

    if (toggle === null || reset === null) {
      throw new Error('Expected the theme toggle and reset action to render.');
    }

    const screenReaderLabel = requireElement(
      toggle.querySelector('.theme-control__sr-only'),
      'Expected the toggle screen-reader label to render.',
    );
    const viewport = requireElement(
      toggle.querySelector('.theme-control__viewport'),
      'Expected the toggle icon viewport to render.',
    );
    const reel = requireElement(
      toggle.querySelector('.theme-control__reel'),
      'Expected the toggle icon reel to render.',
    );
    const icons = toggle.querySelectorAll('.theme-control__icon');

    expect(toggle.tagName).toBe('BUTTON');
    expect(toggle.type).toBe('button');
    expect(toggle.getAttribute('aria-pressed')).toBe('false');
    expect(toggle.textContent.trim()).toContain('Switch to dark mode');
    expect(screenReaderLabel.textContent.trim()).toBe('Switch to dark mode');
    expect(viewport.getAttribute('aria-hidden')).toBe('true');
    expect(reel.classList.contains('theme-control__reel--dark')).toBe(false);
    expect(icons).toHaveLength(2);
    expect(reset.tagName).toBe('BUTTON');
    expect(reset.type).toBe('button');
    expect(reset.textContent.trim()).toBe('Reset to system');
    expect(reset.disabled).toBe(true);
  });

  it('toggles to an explicit dark mode and enables reset when activated from light', () => {
    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggle = compiled.querySelector<HTMLButtonElement>('.theme-control__toggle');
    const reset = compiled.querySelector<HTMLButtonElement>('.theme-control__reset');

    if (toggle === null || reset === null) {
      throw new Error('Expected the theme toggle and reset action to render.');
    }

    toggle.click();
    fixture.detectChanges();
    const screenReaderLabel = requireElement(
      toggle.querySelector('.theme-control__sr-only'),
      'Expected the toggle screen-reader label to render after mode change.',
    );
    const reel = requireElement(
      toggle.querySelector('.theme-control__reel'),
      'Expected the toggle icon reel to render after mode change.',
    );

    expect(toggle.getAttribute('aria-pressed')).toBe('true');
    expect(toggle.classList.contains('theme-control__toggle--dark')).toBe(true);
    expect(reel.classList.contains('theme-control__reel--dark')).toBe(true);
    expect(toggle.textContent.trim()).toContain('Switch to light mode');
    expect(screenReaderLabel.textContent.trim()).toBe('Switch to light mode');
    expect(reset.disabled).toBe(false);
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorage.getItem(storageKey)).toBe('dark');
  });

  it('toggles from a dark system preference to an explicit light mode', () => {
    installMatchMediaStub(true);

    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggle = compiled.querySelector<HTMLButtonElement>('.theme-control__toggle');
    const reset = compiled.querySelector<HTMLButtonElement>('.theme-control__reset');

    if (toggle === null || reset === null) {
      throw new Error('Expected the theme toggle and reset action to render.');
    }

    expect(toggle.getAttribute('aria-pressed')).toBe('true');
    expect(reset.disabled).toBe(true);

    toggle.click();
    fixture.detectChanges();
    const reel = requireElement(
      toggle.querySelector('.theme-control__reel'),
      'Expected the toggle icon reel to render after switching from system mode.',
    );

    expect(toggle.getAttribute('aria-pressed')).toBe('false');
    expect(reel.classList.contains('theme-control__reel--dark')).toBe(false);
    expect(reset.disabled).toBe(false);
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('light');
    expect(localStorage.getItem(storageKey)).toBe('light');
  });

  it('clears persisted mode and the document root override when reset to system default', () => {
    localStorage.setItem(storageKey, 'light');

    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggle = compiled.querySelector<HTMLButtonElement>('.theme-control__toggle');
    const reset = compiled.querySelector<HTMLButtonElement>('.theme-control__reset');

    if (toggle === null || reset === null) {
      throw new Error('Expected the theme toggle and reset action to render.');
    }

    expect(toggle.getAttribute('aria-pressed')).toBe('false');
    expect(reset.disabled).toBe(false);

    reset.click();
    fixture.detectChanges();
    const screenReaderLabel = requireElement(
      toggle.querySelector('.theme-control__sr-only'),
      'Expected the toggle screen-reader label to render after reset.',
    );
    const reel = requireElement(
      toggle.querySelector('.theme-control__reel'),
      'Expected the toggle icon reel to render after reset.',
    );

    expect(toggle.getAttribute('aria-pressed')).toBe('false');
    expect(reel.classList.contains('theme-control__reel--dark')).toBe(false);
    expect(toggle.textContent.trim()).toContain('Switch to dark mode');
    expect(screenReaderLabel.textContent.trim()).toBe('Switch to dark mode');
    expect(reset.disabled).toBe(true);
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorage.getItem(storageKey)).toBeNull();
  });
});
