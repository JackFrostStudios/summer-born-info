import { TestBed } from '@angular/core/testing';
import { ThemeControl } from './theme-control';

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
  let listener: ((event: MediaQueryListEvent) => void) | { handleEvent(event: MediaQueryListEvent): void } | null =
    null;

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

function requireToggleHost(compiled: HTMLElement): HTMLElement {
  const toggleHost = compiled.querySelector<HTMLElement>('sbi-button.theme-control__toggle');

  if (toggleHost === null) {
    throw new Error('Expected the theme toggle shared button host to render.');
  }

  return toggleHost;
}

function requireToggleButton(toggleHost: ParentNode): HTMLButtonElement {
  const toggle = toggleHost.querySelector('button');

  if (!(toggle instanceof HTMLButtonElement)) {
    throw new Error('Expected the theme toggle to render a native button inside the shared button host.');
  }

  return toggle;
}

function expectToggleSemantics(toggle: HTMLButtonElement, pressed: 'true' | 'false'): void {
  expect(toggle.getAttribute('aria-label')).toBe('Dark mode');
  expect(toggle.getAttribute('aria-pressed')).toBe(pressed);
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

  it('renders an accessible toggle without a reset action', () => {
    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggleHost = requireToggleHost(compiled);
    const toggle = requireToggleButton(toggleHost);

    expect(compiled.querySelector('.theme-control__reset')).toBeNull();

    const viewport = toggle.querySelector('.theme-control__viewport');

    if (viewport === null) {
      throw new Error('Expected the toggle icon viewport to render.');
    }

    const icons = toggle.querySelectorAll('sbi-icon.theme-control__icon');
    const inlineSvgs = toggle.querySelectorAll('sbi-icon.theme-control__icon svg');

    expect(toggle.tagName).toBe('BUTTON');
    expect(toggle.type).toBe('button');
    expect(toggle.classList.contains('sbi-button')).toBe(true);
    expect(toggle.classList.contains('sbi-button--secondary')).toBe(true);
    expect(toggle.classList.contains('sbi-button--icon-only')).toBe(true);
    expectToggleSemantics(toggle, 'false');
    expect(toggle.textContent.trim()).toBe('');
    expect(toggleHost.classList.contains('theme-control__toggle')).toBe(true);
    expect(viewport.getAttribute('aria-hidden')).toBe('true');
    expect(icons).toHaveLength(2);
    expect(inlineSvgs).toHaveLength(2);
  });

  it('toggles to an explicit dark mode and persists the override when activated from light', () => {
    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggleHost = requireToggleHost(compiled);
    const toggle = requireToggleButton(toggleHost);

    toggle.click();
    fixture.detectChanges();

    expectToggleSemantics(toggle, 'true');
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
    const toggle = requireToggleButton(requireToggleHost(compiled));

    expectToggleSemantics(toggle, 'true');

    toggle.click();
    fixture.detectChanges();
    expectToggleSemantics(toggle, 'false');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('light');
    expect(localStorage.getItem(storageKey)).toBe('light');
  });

  it('renders the dark toggle state immediately from a persisted override', () => {
    localStorage.setItem(storageKey, 'dark');
    document.documentElement.setAttribute(rootAttribute, 'dark');

    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggleHost = requireToggleHost(compiled);
    const toggle = requireToggleButton(toggleHost);

    expectToggleSemantics(toggle, 'true');
  });

  it('updates the rendered toggle state when the system preference changes without an explicit override', () => {
    const mediaQuery = installMatchMediaStub(false);

    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggle = requireToggleButton(requireToggleHost(compiled));

    expectToggleSemantics(toggle, 'false');

    mediaQuery.setMatches(true);
    fixture.detectChanges();

    expectToggleSemantics(toggle, 'true');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorage.getItem(storageKey)).toBeNull();
  });

  it('keeps the stable toggle semantics while the control has keyboard focus', () => {
    TestBed.configureTestingModule({
      imports: [ThemeControl],
    });

    const fixture = TestBed.createComponent(ThemeControl);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const toggle = requireToggleButton(requireToggleHost(compiled));

    toggle.focus();

    expect(document.activeElement).toBe(toggle);
    expectToggleSemantics(toggle, 'false');
  });
});
