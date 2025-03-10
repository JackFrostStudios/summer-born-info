import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.routes').then(m => m.ADMIN_ROUTES),
  },
];
