import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AdminAuthService } from './admin-auth.service';
import { adminAuthGuard } from './admin-auth.guard';

let protectedRouteCreationCount = 0;

@Component({
  selector: 'sbi-admin-guard-protected-test-route',
  template: '<p i18n="Admin guard test protected route text@@adminGuardTestProtectedRoute">{{ renderedText }}</p>',
})
class AdminGuardProtectedTestRoute {
  protected readonly renderedText = 'Protected admin route rendered.';

  constructor() {
    protectedRouteCreationCount += 1;
  }
}

@Component({
  selector: 'sbi-admin-guard-login-test-route',
  template: '<p i18n="Admin guard test login route text@@adminGuardTestLoginRoute">Admin login route rendered.</p>',
})
class AdminGuardLoginTestRoute {}

describe('adminAuthGuard', () => {
  let $isAuthenticated: () => boolean;

  beforeEach(() => {
    protectedRouteCreationCount = 0;
    $isAuthenticated = () => false;

    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          {
            path: 'admin/protected',
            canActivate: [adminAuthGuard],
            component: AdminGuardProtectedTestRoute,
          },
          {
            path: 'admin/reports',
            canActivate: [adminAuthGuard],
            component: AdminGuardProtectedTestRoute,
          },
          {
            path: 'admin/login',
            component: AdminGuardLoginTestRoute,
          },
        ]),
        {
          provide: AdminAuthService,
          useValue: {
            $isAuthenticated: () => $isAuthenticated(),
          },
        },
      ],
    });
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('allows protected admin route activation when the runtime session is authenticated', async () => {
    $isAuthenticated = () => true;

    const harness = await RouterTestingHarness.create();
    const routedComponent = await harness.navigateByUrl('/admin/protected', AdminGuardProtectedTestRoute);

    expect(routedComponent).toBeInstanceOf(AdminGuardProtectedTestRoute);
    expect(TestBed.inject(Router).url).toBe('/admin/protected');
    expect(harness.routeNativeElement?.textContent).toContain('Protected admin route rendered.');
    expect(protectedRouteCreationCount).toBe(1);
  });

  it('redirects an unauthenticated protected route request to admin login', async () => {
    const harness = await RouterTestingHarness.create();
    const routedComponent = await harness.navigateByUrl('/admin/protected');

    expect(routedComponent).toBeInstanceOf(AdminGuardLoginTestRoute);
    expect(TestBed.inject(Router).url).toBe('/admin/login?returnUrl=%2Fadmin%2Fprotected');
    expect(harness.routeNativeElement?.textContent).toContain('Admin login route rendered.');
    expect(protectedRouteCreationCount).toBe(0);
  });

  it('preserves the attempted protected URL query and fragment for post-login routing', async () => {
    const attemptedUrl = '/admin/reports?school=summer-born&tab=imports#status';
    const harness = await RouterTestingHarness.create();

    await harness.navigateByUrl(attemptedUrl, AdminGuardLoginTestRoute);

    expect(TestBed.inject(Router).url).toBe(
      '/admin/login?returnUrl=%2Fadmin%2Freports%3Fschool%3Dsummer-born%26tab%3Dimports%23status',
    );
    expect(protectedRouteCreationCount).toBe(0);
  });
});
