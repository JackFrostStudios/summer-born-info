import { TestBed } from '@angular/core/testing';
import { OpenSourceLicences } from './open-source-licences';

describe('OpenSourceLicences', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OpenSourceLicences],
    }).compileComponents();
  });

  it('renders the open source licence notices and icon attribution', () => {
    const fixture = TestBed.createComponent(OpenSourceLicences);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const article = compiled.querySelector<HTMLElement>('article');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const noticesLink = findLinkByText(compiled, 'View the third-party licence notices');
    const attribution = compiled.querySelector<HTMLElement>('.open-source-licences__attribution');

    if (article === null || heading === null || noticesLink === null || attribution === null) {
      throw new Error('Expected the open source licences page content to render.');
    }

    const attributionLink = attribution.querySelector<HTMLAnchorElement>('a');

    expect(article.getAttribute('aria-labelledby')).toBe('open-source-licences-heading');
    expect(compiled.querySelectorAll('h1')).toHaveLength(1);
    expect(heading.id).toBe('open-source-licences-heading');
    expect(heading.textContent.trim()).toBe('Open source licences');
    expect(compiled.textContent).toContain(
      'This site is built using open source software. Below is a list of the third-party packages we use and their licence notices.',
    );
    expect(noticesLink.getAttribute('href')).toBe('/3rdpartylicenses.txt');
    expect(attribution.textContent.replace(/\s+/g, ' ').trim()).toBe('Uicons by Flaticon');

    if (attributionLink === null) {
      throw new Error('Expected the Flaticon attribution link to render.');
    }

    expect(attributionLink.textContent.trim()).toBe('Flaticon');
    expect(attributionLink.getAttribute('href')).toBe('https://www.flaticon.com/uicons');
  });
});

function findLinkByText(root: ParentNode, text: string): HTMLAnchorElement | null {
  return (
    Array.from(root.querySelectorAll('a')).find(
      (link): link is HTMLAnchorElement => link.textContent.trim() === text,
    ) ?? null
  );
}
