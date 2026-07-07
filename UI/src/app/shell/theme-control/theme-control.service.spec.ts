import { EnvironmentInjector, PLATFORM_ID, createEnvironmentInjector, runInInjectionContext } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ThemeControlService } from './theme-control.service';
import {
  installLocalStorageStub,
  installMatchMediaStub,
  rootAttribute,
  storageKey,
} from './theme-control.test-helpers';

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

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('light');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorage.getItem(storageKey)).toBeNull();
  });

  it('applies and persists an explicit light mode selection', () => {
    const service = TestBed.inject(ThemeControlService);

    service.setMode('light');

    expect(service.$mode()).toBe('light');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('light');
    expect(localStorage.getItem(storageKey)).toBe('light');
  });

  it('applies and persists an explicit dark mode selection', () => {
    const service = TestBed.inject(ThemeControlService);

    service.setMode('dark');

    expect(service.$mode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorage.getItem(storageKey)).toBe('dark');
  });

  it('restores a persisted dark mode override on creation', () => {
    localStorage.setItem(storageKey, 'dark');

    const service = TestBed.inject(ThemeControlService);

    expect(service.$mode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
  });

  it('discards an invalid persisted mode and falls back to the system preference', () => {
    const localStorageStub = installLocalStorageStub({
      initialValues: { [storageKey]: 'sepia' },
    });

    const service = TestBed.inject(ThemeControlService);

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('light');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorageStub.removeItem).toHaveBeenCalledWith(storageKey);
    expect(localStorageStub.read(storageKey)).toBeNull();
  });

  it('falls back to system mode when reading persisted storage throws', () => {
    const localStorageStub = installLocalStorageStub({
      getItemError: new Error('Storage unavailable'),
    });

    const service = TestBed.inject(ThemeControlService);

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('light');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorageStub.getItem).toHaveBeenCalledWith(storageKey);
  });

  it('toggles from the current effective system preference when no explicit mode is set', () => {
    installMatchMediaStub(true);
    const service = TestBed.inject(ThemeControlService);

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('dark');

    service.toggleMode();

    expect(service.$mode()).toBe('light');
    expect(service.$effectiveMode()).toBe('light');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('light');
    expect(localStorage.getItem(storageKey)).toBe('light');
  });

  it('tracks system preference changes while following system mode', () => {
    const matchMediaStub = installMatchMediaStub(false);
    const service = TestBed.inject(ThemeControlService);

    expect(service.$effectiveMode()).toBe('light');

    matchMediaStub.setMatches(true);

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('dark');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
  });

  it('keeps applying explicit modes when writing persisted storage throws', () => {
    const localStorageStub = installLocalStorageStub({
      setItemError: new Error('Storage unavailable'),
    });
    const service = TestBed.inject(ThemeControlService);

    service.setMode('dark');

    expect(service.$mode()).toBe('dark');
    expect(service.$effectiveMode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorageStub.setItem).toHaveBeenCalledWith(storageKey, 'dark');
    expect(localStorageStub.read(storageKey)).toBeNull();
  });

  it('clears the root attribute and persisted override when returning to system mode', () => {
    const localStorageStub = installLocalStorageStub();
    const service = TestBed.inject(ThemeControlService);

    service.setMode('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorageStub.read(storageKey)).toBe('dark');

    service.setMode('system');

    expect(service.$mode()).toBe('system');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorageStub.removeItem).toHaveBeenLastCalledWith(storageKey);
    expect(localStorageStub.read(storageKey)).toBeNull();
  });

  it('keeps applying system mode when clearing persisted storage throws', () => {
    const localStorageStub = installLocalStorageStub({
      initialValues: { [storageKey]: 'dark' },
      removeItemError: new Error('Storage unavailable'),
    });
    const service = TestBed.inject(ThemeControlService);

    service.setMode('system');

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('light');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorageStub.removeItem).toHaveBeenCalledWith(storageKey);
  });

  it('uses browser guards on the server platform and falls back predictably', () => {
    const localStorageStub = installLocalStorageStub();
    const matchMediaStub = vi.fn();

    Object.defineProperty(globalThis, 'matchMedia', {
      configurable: true,
      value: matchMediaStub,
    });

    TestBed.configureTestingModule({
      providers: [{ provide: PLATFORM_ID, useValue: 'server' }],
    });

    const service = TestBed.inject(ThemeControlService);

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('light');
    expect(localStorageStub.getItem).not.toHaveBeenCalled();
    expect(matchMediaStub).not.toHaveBeenCalled();
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);

    service.setMode('dark');
    expect(service.$mode()).toBe('dark');
    expect(service.$effectiveMode()).toBe('dark');
    expect(document.documentElement.getAttribute(rootAttribute)).toBe('dark');
    expect(localStorageStub.setItem).not.toHaveBeenCalled();

    service.setMode('system');
    expect(document.documentElement.hasAttribute(rootAttribute)).toBe(false);
    expect(localStorageStub.removeItem).not.toHaveBeenCalled();
  });

  it('removes the system-preference listener on injector destruction', () => {
    const matchMediaStub = installMatchMediaStub(false);
    const parentInjector = TestBed.inject(EnvironmentInjector);
    const injector = createEnvironmentInjector([], parentInjector);
    const service = runInInjectionContext(injector, () => new ThemeControlService());

    expect(service.$effectiveMode()).toBe('light');

    injector.destroy();

    expect(matchMediaStub.removeEventListener).toHaveBeenCalledWith('change', expect.any(Function));

    matchMediaStub.setMatches(true);

    expect(service.$mode()).toBe('system');
    expect(service.$effectiveMode()).toBe('light');
  });
});
