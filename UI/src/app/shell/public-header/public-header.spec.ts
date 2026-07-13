import { TestBed } from '@angular/core/testing';
import { PublicHeader } from './public-header';

describe('PublicHeader', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PublicHeader],
    }).compileComponents();
  });

  it('renders the public header landmark with the site brand and theme control', () => {
    const fixture = TestBed.createComponent(PublicHeader);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const header = compiled.querySelector('header');
    const brand = findParagraphByText(compiled, 'Summer-born Info');
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

    expect(brand.textContent.trim()).toBe('Summer-born Info');
    expect(header.contains(brand)).toBe(true);
    expect(header.contains(themeControl)).toBe(true);
  });

  it('renders the site brand as visible text instead of the retired prototype name', () => {
    const fixture = TestBed.createComponent(PublicHeader);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const brand = findParagraphByText(compiled, 'Summer-born Info');

    if (brand === null) {
      throw new Error('Expected the public header brand to render.');
    }

    expect(brand.textContent).not.toContain('SummerBornTrust');
  });

  it('keeps the theme control inside the header landmark beside the brand text', () => {
    const fixture = TestBed.createComponent(PublicHeader);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const header = compiled.querySelector('header');
    const brand = findParagraphByText(compiled, 'Summer-born Info');
    const themeControl = compiled.querySelector('sbi-theme-control');

    if (header === null || brand === null || themeControl === null) {
      throw new Error('Expected the shell header, brand, and theme control to render together.');
    }

    expect(header.contains(brand)).toBe(true);
    expect(themeControl.parentElement).toBe(header);
    expect(brand.nextElementSibling).toBe(themeControl);
  });
});

function findParagraphByText(root: ParentNode, text: string): HTMLParagraphElement | null {
  return (
    Array.from(root.querySelectorAll('p')).find(
      (paragraph): paragraph is HTMLParagraphElement => paragraph.textContent.trim() === text,
    ) ?? null
  );
}
