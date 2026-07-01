import { Location } from '@angular/common';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { UnderConstruction } from './under-construction';

describe('UnderConstruction', () => {
  const location = {
    back: vi.fn(),
  };
  const router = {
    navigateByUrl: vi.fn().mockResolvedValue(true),
  };
  let historyLength = 2;
  const historyWindow = {
    history: {
      get length() {
        return historyLength;
      },
    },
    location: window.location,
  } as Window & typeof globalThis;

  beforeEach(async () => {
    location.back.mockReset();
    router.navigateByUrl.mockClear();
    historyLength = 2;
    vi.spyOn(document, 'defaultView', 'get').mockReturnValue(historyWindow);

    await TestBed.configureTestingModule({
      imports: [UnderConstruction],
      providers: [
        { provide: Location, useValue: location },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders the approved copy with one clear main heading and a decorative builder image', () => {
    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const section = compiled.querySelector<HTMLElement>('section.under-construction');
    const heading = compiled.querySelector<HTMLHeadingElement>('h1');
    const icon = compiled.querySelector<HTMLElement>('.under-construction__icon');
    const button = compiled.querySelector<HTMLButtonElement>('button.under-construction__back-button');

    if (heading === null || icon === null || button === null) {
      throw new Error('Expected the under-construction content, icon, and back button to render.');
    }

    expect(section?.getAttribute('aria-labelledby')).toBe('under-construction-heading');
    expect(compiled.querySelectorAll('h1')).toHaveLength(1);
    expect(heading.id).toBe('under-construction-heading');
    expect(heading.textContent.trim()).toBe(`We're still working on this page`);
    expect(compiled.textContent).toContain('Coming soon');
    expect(compiled.textContent).toContain(
      `This part of the site isn't ready yet, but the rest of our guidance on delayed starts is.`,
    );
    expect(button.textContent.trim()).toBe('Back to where you were');
    expect(button.type).toBe('button');
    expect(button.classList.contains('sbi-button')).toBe(true);
    expect(icon.getAttribute('aria-hidden')).toBe('true');
  });

  it('navigates back to the previous location when the visitor uses the button', () => {
    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('button.under-construction__back-button');

    if (button === null) {
      throw new Error('Expected the back button to render.');
    }

    button.click();

    expect(location.back).toHaveBeenCalledTimes(1);
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('falls back to the homepage when there is no browser history to return to', () => {
    historyLength = 1;

    const fixture = TestBed.createComponent(UnderConstruction);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector<HTMLButtonElement>('button.under-construction__back-button');

    if (button === null) {
      throw new Error('Expected the back button to render.');
    }

    button.click();

    expect(location.back).not.toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });
});
