import { TestBed } from '@angular/core/testing';
import { HomeHero } from './home-hero';

describe('HomeHero', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeHero],
    }).compileComponents();
  });

  it('renders the approved homepage hero copy, heading treatment, and cta', () => {
    const fixture = TestBed.createComponent(HomeHero);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const highlight = compiled.querySelector<HTMLElement>('.home__hero-highlight');
    const button = compiled.querySelector<HTMLButtonElement>('.home__cta-button');

    if (highlight === null || button === null) {
      throw new Error('Expected the homepage hero highlight and CTA button to render.');
    }

    const highlightText = highlight.textContent;
    const buttonText = button.textContent;

    expect(compiled.querySelector('header.home__hero')).not.toBeNull();
    expect(heading?.id).toBe('home-heading');
    expect(heading?.textContent).toContain('Help your summer-born child start school at the right time for them.');
    expect(highlightText.trim()).toBe('right time for them');
    expect(compiled.textContent).toContain(
      `If your child was born in the summer, you may be able to delay their start to Reception until the September after their fifth birthday. It's not about holding them back \u2014 it's about giving them the best possible start.`,
    );
    expect(compiled.textContent).toContain(`We'll help you understand your rights and make the case with confidence.`);
    expect(button.type).toBe('button');
    expect(buttonText.trim()).toBe('Take the first step');
    expect(button.classList.contains('sbi-button')).toBe(true);
  });

  it('renders the approved hero image semantics without extra overlay content', () => {
    const fixture = TestBed.createComponent(HomeHero);
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
});
