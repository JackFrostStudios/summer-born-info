import { TestBed } from '@angular/core/testing';
import { PublicFooter } from './public-footer';

describe('PublicFooter', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PublicFooter],
    }).compileComponents();
  });

  it('renders the shared project summary and icon attribution', () => {
    const fixture = TestBed.createComponent(PublicFooter);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const footer = compiled.querySelector('footer');
    const brand = findParagraphByText(compiled, 'Summer-born Info');
    const summary = findParagraphByText(compiled, 'A developing guide for parents and carers of summer-born children.');
    const attribution = compiled.querySelector<HTMLElement>('.public-footer__attribution');

    expect(footer).not.toBeNull();

    if (brand === null || summary === null || attribution === null) {
      throw new Error('Expected the shared footer brand, summary, and attribution to render.');
    }

    const attributionLink = attribution.querySelector<HTMLAnchorElement>('a');

    expect(brand.textContent.trim()).toBe('Summer-born Info');
    expect(summary.textContent.trim()).toBe('A developing guide for parents and carers of summer-born children.');
    expect(attribution.textContent.replace(/\s+/g, ' ').trim()).toBe('Uicons by Flaticon');

    if (attributionLink === null) {
      throw new Error('Expected the footer attribution link to render.');
    }

    expect(attributionLink.textContent.trim()).toBe('Flaticon');
    expect(attributionLink.getAttribute('href')).toBe('https://www.flaticon.com/uicons');
    expect(attributionLink.getAttribute('target')).toBeNull();
    expect(attributionLink.getAttribute('rel')).toBeNull();
  });
});

function findParagraphByText(root: ParentNode, text: string): HTMLParagraphElement | null {
  return (
    Array.from(root.querySelectorAll('p')).find(
      (paragraph): paragraph is HTMLParagraphElement => paragraph.textContent.trim() === text,
    ) ?? null
  );
}
