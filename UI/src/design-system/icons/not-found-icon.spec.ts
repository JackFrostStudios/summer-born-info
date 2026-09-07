import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { NotFoundIcon } from './not-found-icon';

@Component({
  selector: 'sbi-not-found-icon-test-host',
  imports: [NotFoundIcon],
  template: `<sbi-not-found-icon class="labelled-host" [$label]="label" />
    <sbi-not-found-icon class="decorative-host" />`,
})
class NotFoundIconTestHost {
  protected readonly label = 'Page not found';
}

describe('NotFoundIcon', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotFoundIconTestHost],
    }).compileComponents();
  });

  it('renders not-found artwork through the base icon', () => {
    const fixture = TestBed.createComponent(NotFoundIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector<HTMLElement>('sbi-not-found-icon.labelled-host');

    if (icon === null) {
      throw new Error('Expected the not-found icon host to render.');
    }

    const baseIcon = icon.querySelector<HTMLElement>('sbi-icon');
    const svg = icon.querySelector<SVGElement>('svg');
    const path = svg?.querySelector('path') ?? null;

    if (baseIcon === null || svg === null || path === null) {
      throw new Error('Expected the not-found icon to project inline SVG artwork into the base icon.');
    }

    expect(svg.getAttribute('viewBox')).toBe('0 0 24 24');
    expect(svg.getAttribute('focusable')).toBe('false');
    expect(path.getAttribute('d')?.startsWith('m19.5,1H4.5')).toBe(true);
  });

  it('forwards accessible labels to the base icon and keeps unlabelled artwork decorative', () => {
    const fixture = TestBed.createComponent(NotFoundIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labelledBaseIcon = compiled.querySelector<HTMLElement>('sbi-not-found-icon.labelled-host sbi-icon');
    const decorativeBaseIcon = compiled.querySelector<HTMLElement>('sbi-not-found-icon.decorative-host sbi-icon');

    if (labelledBaseIcon === null || decorativeBaseIcon === null) {
      throw new Error('Expected both not-found base icons to render.');
    }

    expect(labelledBaseIcon.getAttribute('role')).toBe('img');
    expect(labelledBaseIcon.getAttribute('aria-label')).toBe('Page not found');
    expect(labelledBaseIcon.getAttribute('aria-hidden')).toBeNull();

    expect(decorativeBaseIcon.getAttribute('role')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-label')).toBeNull();
    expect(decorativeBaseIcon.getAttribute('aria-hidden')).toBe('true');
  });
});
