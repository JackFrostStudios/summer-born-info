import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Icon, type IconName } from './icon';

@Component({
  selector: 'sbi-icon-test-host',
  imports: [Icon],
  template: `<sbi-icon class="icon-host" [$name]="sunIconName" [$label]="label" />
    <sbi-icon class="icon-decorative" [$name]="builderIconName" />`,
})
class TestHost {
  protected readonly sunIconName: IconName = 'sun';
  protected readonly builderIconName: IconName = 'builder';
  protected readonly label = 'Theme icon';
}

describe('Icon', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHost],
    }).compileComponents();
  });

  it('renders the requested inline svg and exposes an accessible name when provided', () => {
    const fixture = TestBed.createComponent(TestHost);
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
    expect(labelledIcon.querySelector('svg')).not.toBeNull();

    expect(decorativeIcon.getAttribute('role')).toBeNull();
    expect(decorativeIcon.getAttribute('aria-label')).toBeNull();
    expect(decorativeIcon.getAttribute('aria-hidden')).toBe('true');
    expect(decorativeIcon.querySelector('svg')).not.toBeNull();
  });
});
