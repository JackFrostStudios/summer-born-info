import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { BuilderIcon } from './builder-icon';
import { Icon } from './icon';
import { MoonStarsIcon } from './moon-stars-icon';
import { SunIcon } from './sun-icon';

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

@Component({
  selector: 'sbi-concrete-icon-test-host',
  imports: [BuilderIcon, MoonStarsIcon, SunIcon],
  template: `<sbi-sun-icon class="sun-host" [$label]="label" />
    <sbi-moon-stars-icon class="moon-stars-host" />
    <sbi-builder-icon class="builder-host" />`,
})
class ConcreteIconTestHost {
  protected readonly label = 'Light mode';
}

describe('Concrete icons', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConcreteIconTestHost],
    }).compileComponents();
  });

  it('renders the supported inline artwork through selector-specific components', () => {
    const fixture = TestBed.createComponent(ConcreteIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const sunIcon = compiled.querySelector<HTMLElement>('sbi-sun-icon.sun-host');
    const moonStarsIcon = compiled.querySelector<HTMLElement>('sbi-moon-stars-icon.moon-stars-host');
    const builderIcon = compiled.querySelector<HTMLElement>('sbi-builder-icon.builder-host');

    if (sunIcon === null || moonStarsIcon === null || builderIcon === null) {
      throw new Error('Expected every concrete icon host to render.');
    }

    const renderedArtwork = [
      { host: sunIcon, expectedPathStart: 'M23 11H18.92' },
      { host: moonStarsIcon, expectedPathStart: 'M12.009 24A12.067' },
      { host: builderIcon, expectedPathStart: 'm1.971 7h1.011' },
    ];

    for (const { host, expectedPathStart } of renderedArtwork) {
      const baseIcon = host.querySelector<HTMLElement>('sbi-icon');
      const svg = host.querySelector<SVGElement>('svg');
      const path = svg?.querySelector('path') ?? null;

      if (baseIcon === null || svg === null || path === null) {
        throw new Error('Expected the concrete icon to project inline SVG artwork into the base icon.');
      }

      expect(svg.getAttribute('viewBox')).toBe('0 0 24 24');
      expect(svg.getAttribute('focusable')).toBe('false');
      expect(path.getAttribute('d')?.startsWith(expectedPathStart)).toBe(true);
    }
  });

  it('forwards accessible labels to the base icon and keeps unlabelled artwork decorative', () => {
    const fixture = TestBed.createComponent(ConcreteIconTestHost);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const labelledBaseIcon = compiled.querySelector<HTMLElement>('sbi-sun-icon sbi-icon');
    const decorativeBaseIcons = compiled.querySelectorAll<HTMLElement>(
      'sbi-moon-stars-icon sbi-icon, sbi-builder-icon sbi-icon',
    );

    if (labelledBaseIcon === null) {
      throw new Error('Expected the labelled concrete icon to render its base icon.');
    }

    expect(labelledBaseIcon.getAttribute('role')).toBe('img');
    expect(labelledBaseIcon.getAttribute('aria-label')).toBe('Light mode');
    expect(labelledBaseIcon.getAttribute('aria-hidden')).toBeNull();
    expect(decorativeBaseIcons).toHaveLength(2);

    for (const decorativeBaseIcon of decorativeBaseIcons) {
      expect(decorativeBaseIcon.getAttribute('role')).toBeNull();
      expect(decorativeBaseIcon.getAttribute('aria-label')).toBeNull();
      expect(decorativeBaseIcon.getAttribute('aria-hidden')).toBe('true');
    }
  });
});
