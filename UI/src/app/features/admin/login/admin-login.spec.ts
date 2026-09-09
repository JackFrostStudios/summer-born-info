import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AdminAuthService, type AdminSignInResult } from '../auth/admin-auth.service';
import { AdminLogin } from './admin-login';

@Component({
  selector: 'sbi-admin-test-landing',
  template: '<p i18n="Admin login test landing@@adminLoginTestLanding">Admin landing</p>',
})
class AdminTestLanding {}

@Component({
  selector: 'sbi-admin-test-imports',
  template: '<p i18n="Admin login test imports@@adminLoginTestImports">Admin imports</p>',
})
class AdminTestImports {}

class Deferred<T> {
  readonly promise: Promise<T>;
  resolve!: (value: T | PromiseLike<T>) => void;

  constructor() {
    this.promise = new Promise<T>((resolve) => {
      this.resolve = resolve;
    });
  }
}

describe('AdminLogin', () => {
  let fixture: ComponentFixture<AdminLogin>;
  let compiled: HTMLElement;
  let $isAuthenticated: ReturnType<typeof signal<boolean>>;
  let $isSigningIn: ReturnType<typeof signal<boolean>>;
  let signIn: ReturnType<
    typeof vi.fn<(credentials: { email: string; password: string }) => Promise<AdminSignInResult>>
  >;

  beforeEach(async () => {
    $isAuthenticated = signal(false);
    $isSigningIn = signal(false);
    signIn = vi.fn().mockResolvedValue({ kind: 'signed-in' });

    await TestBed.configureTestingModule({
      imports: [AdminLogin],
      providers: [
        provideRouter([{ path: 'admin', component: AdminTestLanding }]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({}),
            },
          },
        },
        {
          provide: AdminAuthService,
          useValue: {
            $isAuthenticated: () => $isAuthenticated(),
            $isSigningIn: () => $isSigningIn(),
            signIn: (credentials: { email: string; password: string }) => signIn(credentials),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminLogin);
    compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders a semantic sign-in form with required email and password fields', () => {
    const form = compiled.querySelector<HTMLFormElement>('form');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const email = getEmailInput();
    const password = getPasswordInput();
    const submit = getSubmitButton();

    expect(form).not.toBeNull();
    expect(compiled.querySelector('section')?.getAttribute('aria-labelledby')).toBe('admin-login-heading');
    expect(heading?.id).toBe('admin-login-heading');
    expect(heading?.textContent.trim()).toBe('Sign in to admin tools');
    expect(getEmailLabel().textContent.trim()).toBe('Email address');
    expect(getPasswordLabel().textContent.trim()).toBe('Password');
    expect(email.type).toBe('email');
    expect(email.autocomplete).toBe('username');
    expect(email.required).toBe(true);
    expect(password.type).toBe('password');
    expect(password.autocomplete).toBe('current-password');
    expect(password.required).toBe(true);
    expect(submit.type).toBe('submit');
  });

  it('blocks submission until both required fields have non-empty values', async () => {
    const email = getEmailInput();
    const password = getPasswordInput();
    const submit = getSubmitButton();

    expect(submit.disabled).toBe(true);

    setInputValue(email, 'admin@example.test');
    await settle();
    expect(submit.disabled).toBe(true);

    setInputValue(password, 'secret');
    await settle();
    expect(submit.disabled).toBe(false);

    submitLoginForm();
    await settle();

    expect(signIn).toHaveBeenCalledWith({ email: 'admin@example.test', password: 'secret' });
  });

  it('redirects successful sign-in to /admin when no safe return URL is present', async () => {
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await submitForm('admin@example.test', 'secret');

    expect(navigateByUrl).toHaveBeenCalledWith('/admin');
  });

  it('redirects successful sign-in to a safe app-relative return URL', async () => {
    TestBed.resetTestingModule();
    await configureWithReturnUrl('/admin/imports?batch=1#review');
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await submitForm('admin@example.test', 'secret');

    expect(navigateByUrl).toHaveBeenCalledWith('/admin/imports?batch=1#review');
  });

  it.each([
    ['https://example.test/admin'],
    ['//example.test/admin'],
    ['admin/imports'],
    ['/admin/login'],
    ['/admin/login?returnUrl=%2Fadmin%2Fimports'],
    ['/admin/login#admin-login-heading'],
  ])('falls back to /admin when the return URL is unsafe or would keep the user on login: %s', async (returnUrl) => {
    TestBed.resetTestingModule();
    await configureWithReturnUrl(returnUrl);
    const router = TestBed.inject(Router);
    const navigateByUrl = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    await submitForm('admin@example.test', 'secret');

    expect(navigateByUrl).toHaveBeenCalledWith('/admin');
  });

  it('redirects away from the login page when the current runtime is already authenticated', async () => {
    TestBed.resetTestingModule();
    await configureWithReturnUrl('/admin/imports', true);

    expect(TestBed.inject(Router).url).toBe('/admin/imports');
  });

  it('keeps the email, clears only the password, and focuses a generic error after invalid credentials', async () => {
    signIn.mockResolvedValue({ kind: 'invalid-credentials' });

    await submitForm('admin@example.test', 'wrong-password');
    await settle();

    const error = getErrorSummary();
    expect(getEmailInput().value).toBe('admin@example.test');
    expect(getPasswordInput().value).toBe('');
    expect(error.textContent.trim()).toBe(
      'We could not sign you in with those details. Check your email and password, then try again.',
    );
    expect(document.activeElement).toBe(error);
  });

  it('keeps the form available and shows admin access denied copy for non-admin credentials', async () => {
    signIn.mockResolvedValue({ kind: 'not-admin' });

    await submitForm('volunteer@example.test', 'secret');

    expect(getEmailInput().value).toBe('volunteer@example.test');
    expect(getPasswordInput().value).toBe('secret');
    expect(getErrorSummary().textContent.trim()).toBe(
      'That account does not have access to the admin tools. Use an admin account, or ask the site owner to check your access.',
    );
    expect(getSubmitButton().disabled).toBe(false);
  });

  it.each([
    ['network-failure', 'We could not reach the sign-in service. Check your connection and try again.'],
    ['unexpected-failure', 'Something went wrong while signing you in. Please try again.'],
  ] as const)('keeps the form available for a recoverable %s response', async (kind, expectedMessage) => {
    signIn.mockResolvedValue({ kind });

    await submitForm('admin@example.test', 'secret');

    expect(getErrorSummary().textContent.trim()).toBe(expectedMessage);
    expect(getSubmitButton().disabled).toBe(false);
  });

  it('communicates pending state and prevents duplicate submissions', async () => {
    const pendingSignIn = new Deferred<AdminSignInResult>();
    signIn.mockReturnValue(pendingSignIn.promise);
    const submit = getSubmitButton();

    await submitForm('admin@example.test', 'secret', false);
    $isSigningIn.set(true);
    fixture.detectChanges();

    expect(submit.textContent.trim()).toBe('Signing in...');
    expect(submit.disabled).toBe(true);
    expect(submit.getAttribute('aria-busy')).toBe('true');

    submit.click();
    await settle();
    expect(signIn).toHaveBeenCalledTimes(1);

    pendingSignIn.resolve({ kind: 'signed-in' });
    $isSigningIn.set(false);
    await settle();
  });

  async function configureWithReturnUrl(returnUrl: string, isAuthenticated = false): Promise<void> {
    $isAuthenticated = signal(isAuthenticated);
    $isSigningIn = signal(false);
    signIn = vi.fn().mockResolvedValue({ kind: 'signed-in' });

    await TestBed.configureTestingModule({
      imports: [AdminLogin],
      providers: [
        provideRouter([
          { path: 'admin', component: AdminTestLanding },
          { path: 'admin/imports', component: AdminTestImports },
        ]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({ returnUrl }),
            },
          },
        },
        {
          provide: AdminAuthService,
          useValue: {
            $isAuthenticated: () => $isAuthenticated(),
            $isSigningIn: () => $isSigningIn(),
            signIn: (credentials: { email: string; password: string }) => signIn(credentials),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminLogin);
    compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
    await settle();
  }

  async function submitForm(email: string, password: string, waitForCompletion = true): Promise<void> {
    setInputValue(getEmailInput(), email);
    setInputValue(getPasswordInput(), password);
    await settle();
    submitLoginForm();

    if (waitForCompletion) {
      await settle();
    }
  }

  async function settle(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    await new Promise<void>((resolve) => setTimeout(resolve));
    fixture.detectChanges();
  }

  function getEmailInput(): HTMLInputElement {
    return requireElement('#admin-login-email') as HTMLInputElement;
  }

  function getPasswordInput(): HTMLInputElement {
    return requireElement('#admin-login-password') as HTMLInputElement;
  }

  function getSubmitButton(): HTMLButtonElement {
    return requireElement('button[type="submit"]') as HTMLButtonElement;
  }

  function getForm(): HTMLFormElement {
    return requireElement('form') as HTMLFormElement;
  }

  function getErrorSummary(): HTMLElement {
    return requireElement('#admin-login-error') as HTMLElement;
  }

  function getEmailLabel(): HTMLLabelElement {
    return requireElement('label[for="admin-login-email"]') as HTMLLabelElement;
  }

  function getPasswordLabel(): HTMLLabelElement {
    return requireElement('label[for="admin-login-password"]') as HTMLLabelElement;
  }

  function requireElement(selector: string): Element {
    const element = compiled.querySelector(selector);

    if (element === null) {
      throw new Error(`Expected to find ${selector}.`);
    }

    return element;
  }

  function setInputValue(input: HTMLInputElement, value: string): void {
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  function submitLoginForm(): void {
    getForm().dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
  }
});

describe('AdminLogin routing', () => {
  let $isAuthenticated: ReturnType<typeof signal<boolean>>;

  beforeEach(() => {
    $isAuthenticated = signal(false);
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          {
            path: 'admin/login',
            component: AdminLogin,
          },
          {
            path: 'admin',
            component: AdminTestLanding,
          },
        ]),
        {
          provide: AdminAuthService,
          useValue: {
            $isAuthenticated: () => $isAuthenticated(),
            $isSigningIn: () => false,
            signIn: vi.fn().mockResolvedValue({ kind: 'signed-in' }),
          },
        },
      ],
    });
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('redirects an already-authenticated runtime session away from /admin/login', async () => {
    $isAuthenticated.set(true);

    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/admin/login?returnUrl=%2Fadmin');
    await harness.fixture.whenStable();
    await new Promise<void>((resolve) => setTimeout(resolve));

    expect(TestBed.inject(Router).url).toBe('/admin');
  });
});
