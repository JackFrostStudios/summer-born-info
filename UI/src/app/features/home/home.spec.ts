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
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Help your summer-born child start school at the right time for them.',
    );
    expect(compiled.textContent).toContain(
      `If your child was born in the summer, you may be able to delay their start to Reception until the September after their fifth birthday. It's not about holding them back \u2014 it's about giving them the best possible start.`,
    );
    expect(compiled.textContent).toContain(`We'll help you understand your rights and make the case with confidence.`);
    expect(compiled.textContent).toContain('Take the first step');
    expect(compiled.textContent).toContain('Discovery tools');
  });

  it('highlights the key phrase in the main title using the approved hero treatment', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const highlight = compiled.querySelector<HTMLElement>('.home__hero-highlight');

    expect(highlight?.textContent.trim()).toBe('right time for them');
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Help your summer-born child start school at the right time for them.',
    );
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

  it('renders the approved hero image without extra overlay controls', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const heroArt = compiled.querySelector<HTMLElement>('.home__hero-art');
    const heroImage = compiled.querySelector<HTMLImageElement>('.home__hero-art-image');

    expect(heroArt).not.toBeNull();
    expect(heroImage?.getAttribute('src')).toContain('/images/hero-child-playing.png');
    expect(heroImage?.getAttribute('alt')).toBe('Young child playing with wooden blocks in a bright room.');
    expect(compiled.querySelector('.home__hero-art-badge')).toBeNull();
    expect(heroArt?.querySelector('figcaption')).toBeNull();
  });

  it('keeps prototype-only claims out of the rendered copy while allowing the homepage CTA', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const pageText = compiled.textContent;
    const buttons = Array.from(compiled.querySelectorAll<HTMLButtonElement>('button'));

    expect(pageText).not.toMatch(/success rate/i);
    expect(pageText).not.toMatch(/parents helped/i);
    expect(pageText).not.toMatch(/donate/i);
    expect(pageText).not.toMatch(/book a call/i);
    expect(pageText).not.toMatch(/join \d/i);
    expect(pageText).not.toMatch(/expert advocacy/i);
    expect(buttons).toHaveLength(1);
    const [callToActionButton] = buttons;

    if (callToActionButton === undefined) {
      throw new Error('Expected the homepage CTA button to render.');
    }

    expect(callToActionButton.textContent.trim()).toBe('Take the first step');
  });

  it('states scope limits without implying official status or legal advice', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const scopeNote = compiled.querySelector('.home__note');

    expect(scopeNote?.textContent).toContain('not an official admissions service');
    expect(scopeNote?.textContent).toContain('does not provide legal advice');
  });

  it('uses the shared button styling for the homepage call to action', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('.home__cta-button');

    if (button === null) {
      throw new Error('Expected the homepage CTA button to render.');
    }

    expect(button.type).toBe('button');
    expect(button.classList.contains('sbi-button')).toBe(true);
  });
});
