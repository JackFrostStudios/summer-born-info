import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HomeHero } from './home-hero';

function requireCallToActionButton(host: ParentNode): HTMLButtonElement {
  const button = host.querySelector('sbi-button button');

  if (!(button instanceof HTMLButtonElement)) {
    throw new Error('Expected the homepage hero shared CTA to render a native button.');
  }

  return button;
}

function requireHeroHeader(host: ParentNode): HTMLElement {
  const header = host.querySelector('header');

  if (!(header instanceof HTMLElement)) {
    throw new Error('Expected the homepage hero to render a header landmark.');
  }

  return header;
}

function requireHeading(host: ParentNode): HTMLHeadingElement {
  const heading = host.querySelector('h1');

  if (!(heading instanceof HTMLHeadingElement)) {
    throw new Error('Expected the homepage hero to render a single level-one heading.');
  }

  return heading;
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

  it('renders the approved homepage hero copy and cta', () => {
    const fixture = TestBed.createComponent(HomeHero);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const header = requireHeroHeader(compiled);
    const heading = requireHeading(compiled);
    const button = requireCallToActionButton(compiled);

    const buttonText = button.textContent;

    expect(header.contains(heading)).toBe(true);
    expect(heading.id).toBe('home-heading');
    expect(heading.textContent).toContain('Help your summer-born child start school at the right time for them.');
    expect(compiled.textContent).toContain(
      `If your child was born in the summer, you may be able to delay their start to Reception until the September after their fifth birthday. It's not about holding them back \u2014 it's about giving them the best possible start.`,
    );
    expect(compiled.textContent).toContain(`We'll help you understand your rights and make the case with confidence.`);
    expect(button.type).toBe('button');
    expect(buttonText.trim()).toBe('Take the first step');
    expect(compiled.querySelectorAll('h1')).toHaveLength(1);
    expect(compiled.querySelectorAll('button')).toHaveLength(1);
  });

  it('renders the approved hero image semantics without extra overlay content', () => {
    const fixture = TestBed.createComponent(HomeHero);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const heroImage = compiled.querySelector<HTMLImageElement>(
      'img[alt="Young child playing with wooden blocks in a bright room."]',
    );

    expect(heroImage?.getAttribute('src')).toContain('images/hero-child-playing.avif');
    expect(heroImage?.getAttribute('fetchpriority')).toBe('high');
    expect(heroImage?.getAttribute('loading')).toBe('eager');
    expect(heroImage?.getAttribute('alt')).toBe('Young child playing with wooden blocks in a bright room.');
    expect(heroImage?.closest('figure')).not.toBeNull();
  });

  it('routes the call to action to the under-construction page', () => {
    const fixture = TestBed.createComponent(HomeHero);
    fixture.detectChanges();

    const button = requireCallToActionButton(fixture.nativeElement as HTMLElement);

    button.click();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/under-construction');
  });
});
