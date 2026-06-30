import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RootShell } from './root-shell';

@Component({
  selector: 'sbi-test-route-content',
  template: '<span aria-hidden="true"></span>',
})
class TestRouteContent {
  protected readonly componentId = 'test-route-content';
}

describe('RootShell', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders the shell as a header plus routed main content composition', () => {
    TestBed.configureTestingModule({
      imports: [RootShell],
      providers: [provideRouter([{ path: '', component: TestRouteContent }])],
    });

    const fixture = TestBed.createComponent(RootShell);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    const shell = compiled.querySelector('.app-shell');
    const header = compiled.querySelector('sbi-public-header');
    const main = compiled.querySelector('main.app-shell__main');

    expect(shell).not.toBeNull();
    expect(header).not.toBeNull();
    expect(main).not.toBeNull();

    if (shell === null || header === null || main === null) {
      throw new Error('Expected the shell, header, and main regions to render.');
    }

    expect(shell.firstElementChild).toBe(header);
    expect(shell.lastElementChild).toBe(main);
    expect(main.querySelector('router-outlet')).not.toBeNull();
  });
});
