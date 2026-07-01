import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RootShell } from './root-shell';

@Component({
  selector: 'sbi-test-route-content',
  template: '<section class="test-route-content"><span aria-hidden="true"></span></section>',
})
class TestRouteContent {
  protected readonly componentId = 'test-route-content';
}

describe('RootShell', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders the shell as a header, routed main content, and shared footer composition', async () => {
    await TestBed.configureTestingModule({
      imports: [RootShell],
      providers: [provideRouter([{ path: '', component: TestRouteContent }])],
    }).compileComponents();

    const fixture = TestBed.createComponent(RootShell);
    const router = TestBed.inject(Router);

    router.initialNavigation();
    await router.navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    const shell = compiled.querySelector('.app-shell');
    const header = compiled.querySelector('sbi-public-header');
    const main = compiled.querySelector('main.app-shell__main');
    const footer = compiled.querySelector('sbi-public-footer');
    const routedContent = compiled.querySelector<HTMLElement>('.test-route-content');

    expect(shell).not.toBeNull();
    expect(header).not.toBeNull();
    expect(main).not.toBeNull();
    expect(footer).not.toBeNull();
    expect(routedContent).not.toBeNull();

    if (shell === null || header === null || main === null || footer === null || routedContent === null) {
      throw new Error('Expected the shell, header, main region, footer, and routed content to render.');
    }

    expect(shell.firstElementChild).toBe(header);
    expect(shell.lastElementChild).toBe(footer);
    expect(main.contains(routedContent)).toBe(true);
    expect(routedContent.closest('main.app-shell__main')).toBe(main);
    expect(header.contains(routedContent)).toBe(false);
    expect(footer.contains(routedContent)).toBe(false);
  });
});
