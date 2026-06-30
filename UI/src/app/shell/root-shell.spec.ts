import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RootShell } from './root-shell';

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

@Component({
  selector: 'sbi-test-route-content',
  template: '<span aria-hidden="true"></span>',
})
class TestRouteContent {
  protected readonly componentId = 'test-route-content';
}

describe('RootShell', () => {
  beforeEach(() => {
    installLocalStorageStub();
  });

  afterEach(() => {
    document.documentElement.removeAttribute(rootAttribute);
    TestBed.resetTestingModule();
  });

  it('renders an accessible colour mode selector with system default selected', () => {
    TestBed.configureTestingModule({
      imports: [RootShell],
      providers: [provideRouter([{ path: '', component: TestRouteContent }])],
    });

    const fixture = TestBed.createComponent(RootShell);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    const label = compiled.querySelector('label[for="sbi-colour-mode"]');
    const select = compiled.querySelector<HTMLSelectElement>('#sbi-colour-mode');

    expect(label).not.toBeNull();
    expect(select).not.toBeNull();

    if (label === null || select === null) {
      throw new Error('Expected the colour mode label and selector to render.');
    }

    expect(label.textContent).toContain('Colour mode');
    expect(select.value).toBe('system');
    expect(Array.from(select.options, (option) => option.text)).toEqual([
      'System default',
      'Light',
      'Dark',
    ]);
  });

  it('renders the shell as a header plus routed main content composition', () => {
    TestBed.configureTestingModule({
      imports: [RootShell],
      providers: [provideRouter([{ path: '', component: TestRouteContent }])],
    });

    const fixture = TestBed.createComponent(RootShell);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    const shell = compiled.querySelector('.app-shell');
    const header = compiled.querySelector('.app-shell__header');
    const main = compiled.querySelector('main.app-shell__main');

    expect(shell).not.toBeNull();
    expect(header).not.toBeNull();
    expect(main).not.toBeNull();

    if (shell === null || header === null || main === null) {
      throw new Error('Expected the shell, header, and main regions to render.');
    }

    expect(shell.firstElementChild).toBe(header);
    expect(shell.lastElementChild).toBe(main);
    expect(main.querySelector('router-outlet')).not.toBeNull();
  });

  it('updates persisted mode and the document root when the user selects a mode', () => {
    TestBed.configureTestingModule({
      imports: [RootShell],
      providers: [provideRouter([{ path: '', component: TestRouteContent }])],
    });

    const fixture = TestBed.createComponent(RootShell);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const select = compiled.querySelector<HTMLSelectElement>('#sbi-colour-mode');

    if (select === null) {
      throw new Error('Expected the colour mode selector to render.');
    }

    select.value = 'dark';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(select.value).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorage.getItem(storageKey)).toBe('dark');
  });

  it('clears persisted mode and the document root override when reset to system default', () => {
    localStorage.setItem(storageKey, 'light');

    TestBed.configureTestingModule({
      imports: [RootShell],
      providers: [provideRouter([{ path: '', component: TestRouteContent }])],
    });

    const fixture = TestBed.createComponent(RootShell);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const select = compiled.querySelector<HTMLSelectElement>('#sbi-colour-mode');

    if (select === null) {
      throw new Error('Expected the colour mode selector to render.');
    }

    expect(select.value).toBe('light');

    select.value = 'system';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(select.value).toBe('system');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorage.getItem(storageKey)).toBeNull();
  });
});
