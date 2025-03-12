import { ApplicationConfig } from '@angular/core';
import { provideRouter, withPreloading, PreloadAllModules } from '@angular/router';
import { APP_ROUTES } from '@app.routes';

export const appConfig: ApplicationConfig = {
  providers: [provideRouter(APP_ROUTES, withPreloading(PreloadAllModules))],
};
