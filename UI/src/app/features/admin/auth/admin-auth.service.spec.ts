import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AdminAuthService, type AdminSignInResult, type AdminSignOutResult } from './admin-auth.service';

describe('AdminAuthService', () => {
  let service: AdminAuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AdminAuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
  });

  it('signs in with browser credentials and records an authenticated runtime session after a 204 response', async () => {
    const resultPromise = service.signIn({ email: 'admin@example.com', password: 'correct horse battery staple' });

    expect(service.$isSigningIn()).toBe(true);

    const request = http.expectOne('/api/admin/auth/sign-in');
    expect(request.request.method).toBe('POST');
    expect(request.request.withCredentials).toBe(true);
    expect(request.request.body).toEqual({
      email: 'admin@example.com',
      password: 'correct horse battery staple',
    });

    request.flush(null, { status: 204, statusText: 'No Content' });

    await expect(resultPromise).resolves.toEqual({ kind: 'signed-in' } satisfies AdminSignInResult);
    expect(service.$isAuthenticated()).toBe(true);
    expect(service.$isSigningIn()).toBe(false);
  });

  it('maps a 401 sign-in response to invalid credentials and clears pending/auth state', async () => {
    const resultPromise = service.signIn({ email: 'admin@example.com', password: 'wrong-password' });
    const request = http.expectOne('/api/admin/auth/sign-in');

    request.flush(null, { status: 401, statusText: 'Unauthorized' });

    await expect(resultPromise).resolves.toEqual({ kind: 'invalid-credentials' } satisfies AdminSignInResult);
    expect(service.$isAuthenticated()).toBe(false);
    expect(service.$isSigningIn()).toBe(false);
  });

  it('maps a 403 sign-in response to a non-admin account and leaves the session unauthenticated', async () => {
    const resultPromise = service.signIn({ email: 'parent@example.com', password: 'valid-but-not-admin' });
    const request = http.expectOne('/api/admin/auth/sign-in');

    request.flush(null, { status: 403, statusText: 'Forbidden' });

    await expect(resultPromise).resolves.toEqual({ kind: 'not-admin' } satisfies AdminSignInResult);
    expect(service.$isAuthenticated()).toBe(false);
    expect(service.$isSigningIn()).toBe(false);
  });

  it('maps a network sign-in failure to a retryable network outcome', async () => {
    const resultPromise = service.signIn({ email: 'admin@example.com', password: 'password' });
    const request = http.expectOne('/api/admin/auth/sign-in');

    request.error(new ProgressEvent('error'));

    await expect(resultPromise).resolves.toEqual({ kind: 'network-failure' } satisfies AdminSignInResult);
    expect(service.$isAuthenticated()).toBe(false);
    expect(service.$isSigningIn()).toBe(false);
  });

  it('maps an unexpected sign-in status to a generic failure and clears a previous authenticated state', async () => {
    await signInSuccessfully();

    const resultPromise = service.signIn({ email: 'admin@example.com', password: 'password' });
    const request = http.expectOne('/api/admin/auth/sign-in');

    request.flush(null, { status: 418, statusText: "I'm a teapot" });

    await expect(resultPromise).resolves.toEqual({ kind: 'unexpected-failure' } satisfies AdminSignInResult);
    expect(service.$isAuthenticated()).toBe(false);
    expect(service.$isSigningIn()).toBe(false);
  });

  it('signs out with browser credentials and clears the authenticated runtime session after a 204 response', async () => {
    await signInSuccessfully();

    const resultPromise = service.signOut();

    expect(service.$isSigningOut()).toBe(true);

    const request = http.expectOne('/api/admin/auth/sign-out');
    expect(request.request.method).toBe('POST');
    expect(request.request.withCredentials).toBe(true);
    expect(request.request.body).toEqual({});

    request.flush(null, { status: 204, statusText: 'No Content' });

    await expect(resultPromise).resolves.toEqual({ kind: 'signed-out' } satisfies AdminSignOutResult);
    expect(service.$isAuthenticated()).toBe(false);
    expect(service.$isSigningOut()).toBe(false);
  });

  it('surfaces sign-out failures without clearing the authenticated runtime session', async () => {
    await signInSuccessfully();

    const resultPromise = service.signOut();
    const request = http.expectOne('/api/admin/auth/sign-out');

    request.flush(null, { status: 500, statusText: 'Internal Server Error' });

    await expect(resultPromise).resolves.toEqual({ kind: 'unexpected-failure' } satisfies AdminSignOutResult);
    expect(service.$isAuthenticated()).toBe(true);
    expect(service.$isSigningOut()).toBe(false);
  });

  it('maps a sign-out network failure without clearing the authenticated runtime session', async () => {
    await signInSuccessfully();

    const resultPromise = service.signOut();
    const request = http.expectOne('/api/admin/auth/sign-out');

    request.error(new ProgressEvent('error'));

    await expect(resultPromise).resolves.toEqual({ kind: 'network-failure' } satisfies AdminSignOutResult);
    expect(service.$isAuthenticated()).toBe(true);
    expect(service.$isSigningOut()).toBe(false);
  });

  it('exposes a reusable path for future API workflows to clear auth state after a 401', async () => {
    await signInSuccessfully();

    service.markUnauthenticated();

    expect(service.$isAuthenticated()).toBe(false);
  });

  async function signInSuccessfully(): Promise<void> {
    const resultPromise = service.signIn({ email: 'admin@example.com', password: 'password' });
    const request = http.expectOne('/api/admin/auth/sign-in');

    request.flush(null, { status: 204, statusText: 'No Content' });

    await expect(resultPromise).resolves.toEqual({ kind: 'signed-in' } satisfies AdminSignInResult);
    expect(service.$isAuthenticated()).toBe(true);
  }
});
