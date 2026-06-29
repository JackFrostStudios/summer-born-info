import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { RootShell } from './shell/root-shell';

export const routes: Routes = [
  {
    path: '',
    component: RootShell,
    children: [
      {
        path: '',
        pathMatch: 'full',
        component: Home,
      },
    ],
  },
];
