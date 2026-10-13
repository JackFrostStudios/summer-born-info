import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AdminAuthService } from './admin-auth.service';

export const adminAuthGuard: CanActivateFn = (_route, state) => {
  const adminAuthService = inject(AdminAuthService);

  if (adminAuthService.$isAuthenticated()) {
    return true;
  }

  return inject(Router).createUrlTree(['/admin/login'], {
    queryParams: {
      returnUrl: state.url,
    },
  });
};
