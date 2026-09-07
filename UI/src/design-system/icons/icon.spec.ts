import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Icon } from './icon';

@Component({
  selector: 'sbi-base-icon-test-host',
  imports: [Icon],
  template: `<sbi-icon class="icon-host" [$label]="label"><span data-projected-artwork></span></sbi-icon>
    <sbi-icon class="icon-decorative"><span data-projected-artwork></span></sbi-icon>`,
})
class BaseIconTestHost {
  protected readonly label = 'Theme icon';
}

describe('Icon', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BaseIconTestHost],
    }).compileComponents();
  });

  it('projects icon artwork and exposes an accessible name when provided', () => {
    const fixture = TestBed.createComponent(BaseIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labelledIcon = compiled.querySelector<HTMLElement>('sbi-icon.icon-host');
    const decorativeIcon = compiled.querySelector<HTMLElement>('sbi-icon.icon-decorative');

    if (labelledIcon === null || decorativeIcon === null) {
      throw new Error('Expected both labelled and decorative icons to render.');
    }

    expect(labelledIcon.getAttribute('role')).toBe('img');
    expect(labelledIcon.getAttribute('aria-label')).toBe('Theme icon');
    expect(labelledIcon.getAttribute('aria-hidden')).toBeNull();
    expect(labelledIcon.querySelector('[data-projected-artwork]')).not.toBeNull();

    expect(decorativeIcon.getAttribute('role')).toBeNull();
    expect(decorativeIcon.getAttribute('aria-label')).toBeNull();
    expect(decorativeIcon.getAttribute('aria-hidden')).toBe('true');
    expect(decorativeIcon.querySelector('[data-projected-artwork]')).not.toBeNull();
  });
});
