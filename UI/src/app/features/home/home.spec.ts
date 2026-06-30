import { TestBed } from '@angular/core/testing';
import { Home } from './home';

describe('Home', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
    }).compileComponents();
  });

  it('renders the production homepage structure and factual purpose copy', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('header')).not.toBeNull();
    expect(compiled.querySelector('footer')).not.toBeNull();
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Help your summer-born child start school at the right time for them.',
    );
    expect(compiled.textContent).toContain(
      `If your child was born in the summer, you may be able to delay their start to Reception until the September after their fifth birthday. It's not about holding them back — it's about giving them the best possible start.`,
    );
    expect(compiled.textContent).toContain(
      `We'll help you understand your rights and make the case with confidence.`,
    );
    expect(compiled.textContent).toContain('Discovery tools');
    expect(compiled.textContent).toContain('Uicons by Flaticon');
  });

  it('labels the page and supporting sections for assistive technology', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const article = compiled.querySelector<HTMLElement>('article.home');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const labelledSections = Array.from(
      compiled.querySelectorAll<HTMLElement>('section[aria-labelledby], aside[aria-labelledby]'),
    );

    expect(article?.getAttribute('aria-labelledby')).toBe('home-heading');
    expect(heading?.id).toBe('home-heading');
    expect(compiled.querySelectorAll('h1').length).toBe(1);
    expect(labelledSections.length).toBe(3);

    for (const section of labelledSections) {
      const labelledBy = section.getAttribute('aria-labelledby');

      if (labelledBy === null) {
        throw new Error('Expected labelled homepage section to reference a heading.');
      }

      expect(compiled.querySelector(`#${labelledBy}`)).not.toBeNull();
    }
  });

  it('renders the approved hero image with a visible guide label', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const heroArt = compiled.querySelector<HTMLElement>('.home__hero-art');
    const heroImage = compiled.querySelector<HTMLImageElement>('.home__hero-art-image');
    const badge = compiled.querySelector<HTMLElement>('.home__hero-art-badge');

    expect(heroArt?.querySelector('figcaption')?.id).toBe('home-hero-art-caption');
    expect(heroImage?.getAttribute('src')).toContain('/images/hero-child-playing.png');
    expect(heroImage?.getAttribute('alt')).toBe(
      'Young child playing with wooden blocks in a bright room.',
    );
    expect(badge?.textContent.trim()).toBe('Public guide');
  });

  it('keeps prototype-only claims and actions out of the rendered copy', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const pageText = compiled.textContent;
    const links = Array.from(compiled.querySelectorAll<HTMLAnchorElement>('a'));

    expect(pageText).not.toMatch(/success rate/i);
    expect(pageText).not.toMatch(/parents helped/i);
    expect(pageText).not.toMatch(/donate/i);
    expect(pageText).not.toMatch(/book a call/i);
    expect(pageText).not.toMatch(/join \d/i);
    expect(pageText).not.toMatch(/expert advocacy/i);
    expect(compiled.querySelectorAll('button').length).toBe(0);
    expect(links).toHaveLength(1);
    const [attributionLink] = links;

    if (attributionLink === undefined) {
      throw new Error('Expected the footer attribution link to render.');
    }

    expect(attributionLink.textContent.trim()).toBe('Flaticon');
  });

  it('states scope limits without implying official status or legal advice', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const scopeNote = compiled.querySelector('.home__note');

    expect(scopeNote?.textContent).toContain('not an official admissions service');
    expect(scopeNote?.textContent).toContain('does not provide legal advice');
  });

  it('renders the icon attribution as secondary footer metadata', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const attribution = compiled.querySelector<HTMLElement>('.home__footer-attribution');
    const attributionLink = attribution?.querySelector<HTMLAnchorElement>('a');

    expect(attribution?.textContent.replace(/\s+/g, ' ').trim() ?? '').toBe('Uicons by Flaticon');

    if (attributionLink === undefined || attributionLink === null) {
      throw new Error('Expected the icon attribution link to render.');
    }

    expect(attributionLink.getAttribute('href')).toBe('https://www.flaticon.com/uicons');
  });
});
