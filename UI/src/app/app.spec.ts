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

  it('should register the homepage route inside the shared app shell', async () => {
    const compiled = await renderRoute('/');

    expectSharedShell(compiled);
    expect(compiled.querySelector('sbi-home')).not.toBeNull();
  });

  it('should register the under-construction route inside the shared app shell', async () => {
    const compiled = await renderRoute('/under-construction');

    expectSharedShell(compiled);
    expect(compiled.querySelector('sbi-under-construction')).not.toBeNull();
  });

  it('should register the not-found route inside the shared app shell when the URL is unmatched', async () => {
    const compiled = await renderRoute('/missing-page');

    expectSharedShell(compiled);
    expect(compiled.querySelector('sbi-not-found')).not.toBeNull();
  });
});

async function renderRoute(url: string): Promise<HTMLElement> {
  const fixture = TestBed.createComponent(App);
  const router = TestBed.inject(Router);

  router.initialNavigation();
  await router.navigateByUrl(url);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();

  return fixture.nativeElement as HTMLElement;
}

function expectSharedShell(compiled: HTMLElement): void {
  expect(compiled.querySelector('sbi-public-header')).not.toBeNull();
  expect(compiled.querySelector('main')).not.toBeNull();
  expect(compiled.querySelector('footer')?.textContent).toContain(
    'A developing guide for parents and carers of summer-born children.',
  );
}
