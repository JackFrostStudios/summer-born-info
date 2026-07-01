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
    const footer = compiled.querySelector('footer.public-footer');
    const brand = compiled.querySelector<HTMLElement>('.public-footer__brand');
    const attribution = compiled.querySelector<HTMLElement>('.public-footer__attribution');

    expect(footer).not.toBeNull();

    if (brand === null || attribution === null) {
      throw new Error('Expected the shared footer brand and attribution to render.');
    }

    const attributionLink = attribution.querySelector<HTMLAnchorElement>('a');

    expect(brand.textContent.trim()).toBe('Summer-born Info');
    expect(compiled.textContent).toContain('A developing guide for parents and carers of summer-born children.');
    expect(attribution.textContent.replace(/\s+/g, ' ').trim()).toBe('Uicons by Flaticon');

    if (attributionLink === null) {
      throw new Error('Expected the footer attribution link to render.');
    }

    expect(attributionLink.getAttribute('href')).toBe('https://www.flaticon.com/uicons');
  });
});
