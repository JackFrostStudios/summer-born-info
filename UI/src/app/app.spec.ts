import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the cleaned app shell', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);

    router.initialNavigation();
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('main')).not.toBeNull();
    expect(compiled.querySelector('h1')?.textContent).toContain('Summer Born Info');
    expect(compiled.querySelector('p')?.textContent).toContain(
      'Application shell ready for upcoming UI work.'
    );
  });
});
