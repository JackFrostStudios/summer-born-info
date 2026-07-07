import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HomeHero } from './home-hero';

function requireSharedButton(host: ParentNode): HTMLButtonElement {
  const button = host.querySelector('sbi-button.home__cta-button button');

  if (!(button instanceof HTMLButtonElement)) {
    throw new Error('Expected the homepage hero shared CTA to render a native button.');
  }

  return button;
}

describe('HomeHero', () => {
  const router = {
    navigateByUrl: vi.fn().mockResolvedValue(true),
  };

  beforeEach(async () => {
    router.navigateByUrl.mockClear();

    await TestBed.configureTestingModule({
      imports: [HomeHero],
      providers: [{ provide: Router, useValue: router }],
    }).compileComponents();
  });

  it('renders the approved homepage hero copy, heading treatment, and cta', () => {
    const fixture = TestBed.createComponent(HomeHero);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const copy = compiled.querySelector<HTMLElement>('.home__hero-copy');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const highlight = compiled.querySelector<HTMLElement>('.home__hero-highlight');
    const buttonHost = compiled.querySelector<HTMLElement>('sbi-button.home__cta-button');
    const button = requireSharedButton(compiled);

    if (copy === null || highlight === null || buttonHost === null) {
      throw new Error('Expected the homepage hero copy, highlight, and shared CTA button to render.');
    }

    const highlightText = highlight.textContent;
    const buttonText = button.textContent;

    expect(compiled.querySelector('header.home__hero')).not.toBeNull();
    expect(copy.classList.contains('sbi-readable')).toBe(true);
    expect(heading?.id).toBe('home-heading');
    expect(heading?.textContent).toContain('Help your summer-born child start school at the right time for them.');
    expect(highlightText.trim()).toBe('right time for them');
    expect(compiled.textContent).toContain(
      `If your child was born in the summer, you may be able to delay their start to Reception until the September after their fifth birthday. It's not about holding them back \u2014 it's about giving them the best possible start.`,
    );
    expect(compiled.textContent).toContain(`We'll help you understand your rights and make the case with confidence.`);
    expect(buttonHost.classList.contains('home__cta-button')).toBe(true);
    expect(button.type).toBe('button');
    expect(buttonText.trim()).toBe('Take the first step');
    expect(button.classList.contains('sbi-button')).toBe(true);
    expect(button.classList.contains('sbi-button--secondary')).toBe(false);
  });

  it('renders the approved hero image semantics without extra overlay content', () => {
    const fixture = TestBed.createComponent(HomeHero);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const heroArt = compiled.querySelector<HTMLElement>('.home__hero-art');
    const heroImage = compiled.querySelector<HTMLImageElement>('.home__hero-art-image');

    expect(heroArt).not.toBeNull();
    expect(heroArt?.classList.contains('sbi-surface')).toBe(true);
    expect(heroImage?.getAttribute('src')).toContain('images/hero-child-playing.avif');
    expect(heroImage?.getAttribute('fetchpriority')).toBe('high');
    expect(heroImage?.getAttribute('loading')).toBe('eager');
    expect(heroImage?.getAttribute('alt')).toBe('Young child playing with wooden blocks in a bright room.');
    expect(compiled.querySelector('.home__hero-art-badge')).toBeNull();
    expect(heroArt?.querySelector('figcaption')).toBeNull();
  });

  it('routes the call to action to the under-construction page', () => {
    const fixture = TestBed.createComponent(HomeHero);
    fixture.detectChanges();

    const button = requireSharedButton(fixture.nativeElement as HTMLElement);

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/under-construction');
  });
});
