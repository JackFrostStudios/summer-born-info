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
      'Information for parents and carers of summer-born children.',
    );
    expect(compiled.textContent).toContain(
      'Summer Born Info is being prepared as a clear guide to school-start and admission information',
    );
    expect(compiled.textContent).toContain('Discovery tools');
  });

  it('keeps prototype-only claims and actions out of the rendered copy', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const pageText = (fixture.nativeElement as HTMLElement).textContent;

    expect(pageText).not.toMatch(/success rate/i);
    expect(pageText).not.toMatch(/parents helped/i);
    expect(pageText).not.toMatch(/donate/i);
    expect(pageText).not.toMatch(/book a call/i);
    expect(pageText).not.toMatch(/join \d/i);
    expect(pageText).not.toMatch(/expert advocacy/i);
  });

  it('states scope limits without implying official status or legal advice', () => {
    const fixture = TestBed.createComponent(Home);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const scopeNote = compiled.querySelector('.home__note');

    expect(scopeNote?.textContent).toContain('not an official admissions service');
    expect(scopeNote?.textContent).toContain('does not provide legal advice');
  });
});
