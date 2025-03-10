import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { PreloadAllModules, provideRouter, withPreloading } from '@angular/router';
import { APP_ROUTES } from './app/app.routes';

bootstrapApplication(AppComponent, {
  providers: [provideRouter(APP_ROUTES, withPreloading(PreloadAllModules))],
}).catch(err => console.error(err));
