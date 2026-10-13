import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from '../../../app.routes';
import { AdminAuthService, type AdminSignInResult, type AdminSignOutResult } from '../auth/admin-auth.service';
import { AdminHome } from './admin-home';

class Deferred<T> {
  readonly promise: Promise<T>;
  resolve!: (value: T | PromiseLike<T>) => void;

  constructor() {
    this.promise = new Promise<T>((resolve) => {
      this.resolve = resolve;
    });
  }
}

describe('AdminHome', () => {
  let fixture: ComponentFixture<AdminHome>;
  let compiled: HTMLElement;
  let $isAuthenticated: ReturnType<typeof signal<boolean>>;
  let $isSigningOut: ReturnType<typeof signal<boolean>>;
  let signOut: ReturnType<typeof vi.fn<() => Promise<AdminSignOutResult>>>;

  beforeEach(async () => {
    $isAuthenticated = signal(true);
    $isSigningOut = signal(false);
    signOut = vi.fn().mockResolvedValue({ kind: 'signed-out' });

    await TestBed.configureTestingModule({
      imports: [AdminHome],
      providers: [
        provideRouter([{ path: 'admin/login', component: AdminHome }]),
        {
          provide: AdminAuthService,
          useValue: {
            $isAuthenticated: () => $isAuthenticated(),
            $isSigningOut: () => $isSigningOut(),
            signOut: () => signOut(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminHome);
    compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders minimal protected landing content without future admin workflow controls', () => {
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const signOutButton = getSignOutButton();

    expect(compiled.querySelector('section')?.getAttribute('aria-labelledby')).toBe('admin-home-heading');
    expect(heading?.id).toBe('admin-home-heading');
    expect(heading?.textContent.trim()).toBe('Admin tools');
    expect(compiled.textContent).toContain(
      "You're signed in. Admin workflows will appear here as they become available.",
    );
    expect(compiled.textContent).toContain(
      'Protected admin access is ready. The next tools will be added here when they are available.',
    );
    expect(signOutButton.textContent.trim()).toBe('Sign out');
    expect(compiled.querySelector('input')).toBeNull();
    expect(compiled.querySelector('form')).toBeNull();
  });

  it('signs out and redirects to admin login after a successful server response', async () => {
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await clickSignOut();

    expect(signOut).toHaveBeenCalledTimes(1);
    expect(navigateByUrl).toHaveBeenCalledWith('/admin/login');
  });

  it.each([
    ['network-failure', 'We could not reach the sign-out service. Stay signed in here and try again.'],
    ['unexpected-failure', 'Something went wrong while signing you out. Stay signed in here and try again.'],
  ] as const)(
    'keeps the admin on the protected page after a recoverable %s response',
    async (kind, expectedMessage) => {
      const router = TestBed.inject(Router);
      const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
      signOut.mockResolvedValue({ kind });

      await clickSignOut();

      const error = getErrorSummary();
      expect($isAuthenticated()).toBe(true);
      expect(navigateByUrl).not.toHaveBeenCalled();
      expect(error.textContent.trim()).toBe(expectedMessage);
      expect(document.activeElement).toBe(error);
      expect(getSignOutButton().disabled).toBe(false);
    },
  );

  it('communicates pending state and prevents duplicate sign-out attempts', async () => {
    const pendingSignOut = new Deferred<AdminSignOutResult>();
    signOut.mockReturnValue(pendingSignOut.promise);
    const signOutButton = getSignOutButton();

    signOutButton.click();
    $isSigningOut.set(true);
    fixture.detectChanges();

    expect(signOutButton.textContent.trim()).toBe('Signing out...');
    expect(signOutButton.disabled).toBe(true);
    expect(signOutButton.getAttribute('aria-busy')).toBe('true');

    signOutButton.click();
    expect(signOut).toHaveBeenCalledTimes(1);

    pendingSignOut.resolve({ kind: 'signed-out' });
    $isSigningOut.set(false);
    await settle();
  });

  async function clickSignOut(): Promise<void> {
    getSignOutButton().click();
    await settle();
  }

  async function settle(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise<void>((resolve) => setTimeout(resolve));
    fixture.detectChanges();
  }

  function getSignOutButton(): HTMLButtonElement {
    return requireElement('button[type="button"]') as HTMLButtonElement;
  }

  function getErrorSummary(): HTMLElement {
    return requireElement('#admin-home-sign-out-error') as HTMLElement;
  }

  function requireElement(selector: string): Element {
    const element = compiled.querySelector(selector);

    if (element === null) {
      throw new Error(`Expected to find ${selector}.`);
    }

    return element;
  }
});

describe('AdminHome routing', () => {
  let $isAuthenticated: ReturnType<typeof signal<boolean>>;

  beforeEach(() => {
    $isAuthenticated = signal(false);

    TestBed.configureTestingModule({
      providers: [
        provideRouter(routes),
        {
          provide: AdminAuthService,
          useValue: {
            $isAuthenticated: () => $isAuthenticated(),
            $isSigningIn: () => false,
            $isSigningOut: () => false,
            signIn: vi.fn<() => Promise<AdminSignInResult>>().mockResolvedValue({ kind: 'signed-in' }),
            signOut: vi.fn<() => Promise<AdminSignOutResult>>().mockResolvedValue({ kind: 'signed-out' }),
          },
        },
      ],
    });
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('redirects unauthenticated /admin navigation to login with the protected return URL', async () => {
    const harness = await RouterTestingHarness.create();

    await harness.navigateByUrl('/admin');

    expect(TestBed.inject(Router).url).toBe('/admin/login?returnUrl=%2Fadmin');
    expect(harness.routeNativeElement?.textContent).toContain('Sign in to admin tools');
    expect(harness.routeNativeElement?.textContent).not.toContain('Admin tools');
  });

  it('renders the protected landing page when the runtime session is authenticated', async () => {
    $isAuthenticated.set(true);

    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/admin');

    expect(TestBed.inject(Router).url).toBe('/admin');
    expect(harness.routeNativeElement?.textContent).toContain('Admin tools');
  });
});
