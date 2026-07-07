import { TestBed } from '@angular/core/testing';
import { PublicHeader } from './public-header';

describe('PublicHeader', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PublicHeader],
    }).compileComponents();
  });

  it('renders only the requested brand and shell theme control structure', () => {
    const fixture = TestBed.createComponent(PublicHeader);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const header = compiled.querySelector('header.public-header');
    const brand = compiled.querySelector('.public-header__brand');
    const themeControl = compiled.querySelector('sbi-theme-control');

    expect(compiled.getAttribute('data-shell-header')).toBe('public-header');
    expect(header).not.toBeNull();
    expect(brand).not.toBeNull();
    expect(themeControl).not.toBeNull();

    if (header === null) {
      throw new Error('Expected the public header container to render.');
    }

    if (brand === null) {
      throw new Error('Expected the public header brand to render.');
    }

    expect(header.children.length).toBe(2);
    expect(brand.textContent.trim()).toBe('Summer-born Info');
  });

  it('keeps the brand in the stronger prototype-inspired wordmark style', () => {
    const fixture = TestBed.createComponent(PublicHeader);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const brand = compiled.querySelector('.public-header__brand');

    if (brand === null) {
      throw new Error('Expected the public header brand to render.');
    }

    expect(brand.textContent).not.toContain('SummerBornTrust');
    expect(brand.classList.contains('public-header__brand')).toBe(true);
  });

  it('keeps the theme control beside the brand inside the shell header landmark', () => {
    const fixture = TestBed.createComponent(PublicHeader);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const header = compiled.querySelector('header.public-header');
    const themeControl = compiled.querySelector('sbi-theme-control');

    if (header === null || themeControl === null) {
      throw new Error('Expected the shell header and theme control to render together.');
    }

    expect(header.contains(themeControl)).toBe(true);
    expect(themeControl.parentElement).toBe(header);
  });
});
