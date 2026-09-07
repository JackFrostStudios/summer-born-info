import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { BuilderIcon } from './builder-icon';

@Component({
  selector: 'sbi-builder-icon-test-host',
  imports: [BuilderIcon],
  template: `<sbi-builder-icon class="labelled-host" [$label]="label" /> <sbi-builder-icon class="decorative-host" />`,
})
class BuilderIconTestHost {
  protected readonly label = 'Builder';
}

describe('BuilderIcon', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BuilderIconTestHost],
    }).compileComponents();
  });

  it('renders builder artwork through the base icon', () => {
    const fixture = TestBed.createComponent(BuilderIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector<HTMLElement>('sbi-builder-icon.labelled-host');

    if (icon === null) {
      throw new Error('Expected the builder icon host to render.');
    }

    const baseIcon = icon.querySelector<HTMLElement>('sbi-icon');
    const svg = icon.querySelector<SVGElement>('svg');
    const path = svg?.querySelector('path') ?? null;

    if (baseIcon === null || svg === null || path === null) {
      throw new Error('Expected the builder icon to project inline SVG artwork into the base icon.');
    }

    expect(svg.getAttribute('viewBox')).toBe('0 0 24 24');
    expect(svg.getAttribute('focusable')).toBe('false');
    expect(path.getAttribute('d')?.startsWith('m1.971 7h1.011')).toBe(true);
  });

  it('forwards accessible labels to the base icon and keeps unlabelled artwork decorative', () => {
    const fixture = TestBed.createComponent(BuilderIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labelledBaseIcon = compiled.querySelector<HTMLElement>('sbi-builder-icon.labelled-host sbi-icon');
    const decorativeBaseIcon = compiled.querySelector<HTMLElement>('sbi-builder-icon.decorative-host sbi-icon');

    if (labelledBaseIcon === null || decorativeBaseIcon === null) {
      throw new Error('Expected both builder base icons to render.');
    }

    expect(labelledBaseIcon.getAttribute('role')).toBe('img');
    expect(labelledBaseIcon.getAttribute('aria-label')).toBe('Builder');
    expect(labelledBaseIcon.getAttribute('aria-hidden')).toBeNull();

    expect(decorativeBaseIcon.getAttribute('role')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-label')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-hidden')).toBe('true');
  });
});
