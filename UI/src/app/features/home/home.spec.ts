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

    expect(compiled.querySelector('sbi-home-hero')).not.toBeNull();
    expect(compiled.querySelector('header.home__hero')).not.toBeNull();
    expect(compiled.querySelector('h1')?.textContent).toContain(
      'Help your summer-born child start school at the right time for them.',
    );
    expect(compiled.textContent).toContain(
      `If your child was born in the summer, you may be able to delay their start to Reception until the September after their fifth birthday. It's not about holding them back \u2014 it's about giving them the best possible start.`,
    );
    expect(compiled.textContent).toContain(`We'll help you understand your rights and make the case with confidence.`);
    expect(compiled.textContent).toContain('Take the first step');
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

  it('labels the page from the single hero heading for assistive technology', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const article = compiled.querySelector<HTMLElement>('article.home');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    expect(article?.getAttribute('aria-labelledby')).toBe('home-heading');
    expect(heading?.id).toBe('home-heading');
    expect(compiled.querySelectorAll('h1').length).toBe(1);
    expect(compiled.querySelectorAll('section[aria-labelledby], aside[aria-labelledby]')).toHaveLength(0);
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

  it('keeps the page-level labelling relationship explicit through the composed hero heading', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const article = compiled.querySelector<HTMLElement>('article.home');
    const hero = compiled.querySelector<HTMLElement>('sbi-home-hero');
    const heading = hero?.querySelector<HTMLHeadingElement>('h1');

    expect(article?.getAttribute('aria-labelledby')).toBe('home-heading');
    expect(hero).not.toBeNull();
    expect(heading?.id).toBe('home-heading');
    expect(compiled.querySelectorAll('h1')).toHaveLength(1);
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

  it('renders only the hero section while other homepage sections are intentionally removed', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('.home__topics')).toBeNull();
    expect(compiled.querySelector('.home__coming-soon')).toBeNull();
    expect(compiled.querySelector('.home__note')).toBeNull();
    expect(compiled.textContent).not.toContain('What the guide will cover');
    expect(compiled.textContent).not.toContain('Coming later');
    expect(compiled.textContent).not.toContain('Scope note');
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
