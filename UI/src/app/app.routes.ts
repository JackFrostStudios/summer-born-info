import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { UnderConstruction } from './features/under-construction/under-construction';
import { RootShell } from './shell/root-shell/root-shell';

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
      {
        path: 'under-construction',
        component: UnderConstruction,
      },
    ],
  },
];
