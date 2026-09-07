import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { SunIcon } from './sun-icon';

@Component({
  selector: 'sbi-sun-icon-test-host',
  imports: [SunIcon],
  template: `<sbi-sun-icon class="labelled-host" [$label]="label" /> <sbi-sun-icon class="decorative-host" />`,
})
class SunIconTestHost {
  protected readonly label = 'Light mode';
}

describe('SunIcon', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SunIconTestHost],
    }).compileComponents();
  });

  it('renders sun artwork through the base icon', () => {
    const fixture = TestBed.createComponent(SunIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector<HTMLElement>('sbi-sun-icon.labelled-host');

    if (icon === null) {
      throw new Error('Expected the sun icon host to render.');
    }

    const baseIcon = icon.querySelector<HTMLElement>('sbi-icon');
    const svg = icon.querySelector<SVGElement>('svg');
    const path = svg?.querySelector('path') ?? null;

    if (baseIcon === null || svg === null || path === null) {
      throw new Error('Expected the sun icon to project inline SVG artwork into the base icon.');
    }

    expect(svg.getAttribute('viewBox')).toBe('0 0 24 24');
    expect(svg.getAttribute('focusable')).toBe('false');
    expect(path.getAttribute('d')?.startsWith('M23 11H18.92')).toBe(true);
  });

  it('forwards accessible labels to the base icon and keeps unlabelled artwork decorative', () => {
    const fixture = TestBed.createComponent(SunIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labelledBaseIcon = compiled.querySelector<HTMLElement>('sbi-sun-icon.labelled-host sbi-icon');
    const decorativeBaseIcon = compiled.querySelector<HTMLElement>('sbi-sun-icon.decorative-host sbi-icon');

    if (labelledBaseIcon === null || decorativeBaseIcon === null) {
      throw new Error('Expected both sun base icons to render.');
    }

    expect(labelledBaseIcon.getAttribute('role')).toBe('img');
    expect(labelledBaseIcon.getAttribute('aria-label')).toBe('Light mode');
    expect(labelledBaseIcon.getAttribute('aria-hidden')).toBeNull();

    expect(decorativeBaseIcon.getAttribute('role')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-label')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-hidden')).toBe('true');
  });
});
