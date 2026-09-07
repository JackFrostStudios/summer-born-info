import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MoonStarsIcon } from './moon-stars-icon';

@Component({
  selector: 'sbi-moon-stars-icon-test-host',
  imports: [MoonStarsIcon],
  template: `<sbi-moon-stars-icon class="labelled-host" [$label]="label" />
    <sbi-moon-stars-icon class="decorative-host" />`,
})
class MoonStarsIconTestHost {
  protected readonly label = 'Dark mode';
}

describe('MoonStarsIcon', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MoonStarsIconTestHost],
    }).compileComponents();
  });

  it('renders moon and stars artwork through the base icon', () => {
    const fixture = TestBed.createComponent(MoonStarsIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector<HTMLElement>('sbi-moon-stars-icon.labelled-host');

    if (icon === null) {
      throw new Error('Expected the moon and stars icon host to render.');
    }

    const baseIcon = icon.querySelector<HTMLElement>('sbi-icon');
    const svg = icon.querySelector<SVGElement>('svg');
    const path = svg?.querySelector('path') ?? null;

    if (baseIcon === null || svg === null || path === null) {
      throw new Error('Expected the moon and stars icon to project inline SVG artwork into the base icon.');
    }

    expect(svg.getAttribute('viewBox')).toBe('0 0 24 24');
    expect(svg.getAttribute('focusable')).toBe('false');
    expect(path.getAttribute('d')?.startsWith('M12.009 24A12.067')).toBe(true);
  });

  it('forwards accessible labels to the base icon and keeps unlabelled artwork decorative', () => {
    const fixture = TestBed.createComponent(MoonStarsIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labelledBaseIcon = compiled.querySelector<HTMLElement>('sbi-moon-stars-icon.labelled-host sbi-icon');
    const decorativeBaseIcon = compiled.querySelector<HTMLElement>('sbi-moon-stars-icon.decorative-host sbi-icon');

    if (labelledBaseIcon === null || decorativeBaseIcon === null) {
      throw new Error('Expected both moon and stars base icons to render.');
    }

    expect(labelledBaseIcon.getAttribute('role')).toBe('img');
    expect(labelledBaseIcon.getAttribute('aria-label')).toBe('Dark mode');
    expect(labelledBaseIcon.getAttribute('aria-hidden')).toBeNull();

    expect(decorativeBaseIcon.getAttribute('role')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-label')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-hidden')).toBe('true');
  });
});
