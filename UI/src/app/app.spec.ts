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

  it('should render the homepage route inside the app shell', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);

    router.initialNavigation();
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('main')).not.toBeNull();
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Help your summer-born child start school at the right time for them.',
    );
    expect(compiled.querySelector('footer')?.textContent).toContain(
      'A developing guide for parents and carers of summer-born children.',
    );
  });

  it('should render the under-construction route inside the shared shell with header and footer visible', async () => {
    const fixture = TestBed.createComponent(App);
    const router = TestBed.inject(Router);

    router.initialNavigation();
    await router.navigateByUrl('/under-construction');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const heading = compiled.querySelector('h1');

    expect(compiled.querySelector('sbi-public-header')).not.toBeNull();
    expect(compiled.querySelector('main')).not.toBeNull();
    expect(compiled.querySelector('footer')?.textContent).toContain(
      'A developing guide for parents and carers of summer-born children.',
    );

    if (heading === null) {
      throw new Error('Expected the under-construction page heading to render.');
    }

    expect(heading.textContent.trim()).toBe('This page is still under construction');
    expect(compiled.textContent).toContain('Coming soon');
    expect(compiled.textContent).toContain('Click here to go back');
  });
});
