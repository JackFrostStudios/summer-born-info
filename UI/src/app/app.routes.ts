import { Routes } from '@angular/router';
import { HomePlaceholder } from './features/home/home-placeholder';
import { RootShell } from './shell/root-shell';

export const routes: Routes = [
  {
    path: '',
    component: RootShell,
    children: [
      {
        path: '',
        pathMatch: 'full',
        component: HomePlaceholder,
      },
    ],
  },
];
