import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { InMemoryScrollingOptions, provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideClientHydration, withEventReplay, withI18nSupport } from '@angular/platform-browser';

import { routes } from './app.routes';

export const appInMemoryScrollingOptions: InMemoryScrollingOptions = {
  anchorScrolling: 'enabled',
  scrollPositionRestoration: 'enabled',
};

export const appHydrationFeatures = [withI18nSupport(), withEventReplay()];

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withInMemoryScrolling(appInMemoryScrollingOptions)),
    provideClientHydration(...appHydrationFeatures),
  ],
};
