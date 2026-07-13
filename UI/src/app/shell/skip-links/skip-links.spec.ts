import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SkipLinks } from './skip-links';

describe('SkipLinks', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders route-provided skip-link entries as fragment links', async () => {
    const fixture = await renderComponent();

    fixture.componentRef.setInput('$links', [
      { label: 'Skip to main content', targetId: 'main-content' },
      { label: 'Skip to filters', targetId: 'filter-panel' },
    ]);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const linkElements = Array.from(host.querySelectorAll<HTMLAnchorElement>('a.skip-links__link'));

    expect(linkElements).toHaveLength(2);
    expect(linkElements.map((link) => link.textContent.trim())).toEqual(['Skip to main content', 'Skip to filters']);
    expect(linkElements.map((link) => link.getAttribute('href'))).toEqual(['/#main-content', '/#filter-panel']);
  });

  it('stays hidden until a skip link receives focus and hides again when focus leaves the component', async () => {
    const fixture = await renderComponent();

    fixture.componentRef.setInput('$links', [{ label: 'Skip to main content', targetId: 'main-content' }]);
    fixture.detectChanges();

    const host = fixture.nativeElement as HTMLElement;
    const link = host.querySelector<HTMLAnchorElement>('a.skip-links__link');

    expect(link).not.toBeNull();
    expect(host.classList.contains('skip-links--visible')).toBe(false);

    if (link === null) {
      throw new Error('Expected the skip-link anchor to render.');
    }

    link.dispatchEvent(new FocusEvent('focusin', { bubbles: true, relatedTarget: null }));
    fixture.detectChanges();

    expect(host.classList.contains('skip-links--visible')).toBe(true);

    link.dispatchEvent(new FocusEvent('focusout', { bubbles: true, relatedTarget: document.body }));
    fixture.detectChanges();

    expect(host.classList.contains('skip-links--visible')).toBe(false);
  });
});

async function renderComponent(): Promise<ComponentFixture<SkipLinks>> {
  await TestBed.configureTestingModule({
    imports: [SkipLinks],
    providers: [provideRouter([])],
  }).compileComponents();

  const fixture = TestBed.createComponent(SkipLinks);
  fixture.detectChanges();

  return fixture;
}
