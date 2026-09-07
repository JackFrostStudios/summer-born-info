import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PublicFooter } from './public-footer';

describe('PublicFooter', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PublicFooter],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('renders the shared project summary and open source link', () => {
    const fixture = TestBed.createComponent(PublicFooter);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const footer = compiled.querySelector('footer');
    const brand = findParagraphByText(compiled, 'Summer-born Info');
    const summary = findParagraphByText(compiled, 'A developing guide for parents and carers of summer-born children.');
    const openSourceLink = findLinkByText(compiled, 'Open source licences');

    if (footer === null || brand === null || summary === null || openSourceLink === null) {
      throw new Error('Expected the shared footer brand, summary, and open source link to render.');
    }

    expect(footer).not.toBeNull();
    expect(brand.textContent.trim()).toBe('Summer-born Info');
    expect(summary.textContent.trim()).toBe('A developing guide for parents and carers of summer-born children.');
    expect(openSourceLink.getAttribute('href')).toBe('/open-source-licences');
    expect(compiled.textContent).not.toContain('Uicons by');
  });
});

function findLinkByText(root: ParentNode, text: string): HTMLAnchorElement | null {
  return (
    Array.from(root.querySelectorAll('a')).find(
      (link): link is HTMLAnchorElement => link.textContent.trim() === text,
    ) ?? null
  );
}

function findParagraphByText(root: ParentNode, text: string): HTMLParagraphElement | null {
  return (
    Array.from(root.querySelectorAll('p')).find(
      (paragraph): paragraph is HTMLParagraphElement => paragraph.textContent.trim() === text,
    ) ?? null
  );
}
