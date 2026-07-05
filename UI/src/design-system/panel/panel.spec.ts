import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Panel } from './panel';

@Component({
  selector: 'sbi-panel-test-host',
  imports: [Panel],
  template:
    '<sbi-panel [$mediaWidth]="mediaWidth"><span panelMedia class="panel-test-media" aria-hidden="true">?</span><p class="panel-test-copy" i18n="Panel test host copy@@panelTestHostCopy">Shared panel content</p></sbi-panel>',
})
class TestHostComponent {
  mediaWidth: 'default' | 'compact' = 'default';
}

function requirePanel(compiled: HTMLElement): HTMLElement {
  const panel = compiled.querySelector('.sbi-panel');

  if (!(panel instanceof HTMLElement)) {
    throw new Error('Expected the shared panel to render its panel shell.');
  }

  return panel;
}

describe('Panel', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();
  });

  it('renders the shared panel shell with projected media and content', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const panel = requirePanel(compiled);
    const media = compiled.querySelector<HTMLElement>('.panel-test-media');
    const content = compiled.querySelector<HTMLElement>('.panel-test-copy');
    const contentWrapper = panel.querySelector<HTMLElement>('.sbi-panel__content');

    if (media === null || content === null || contentWrapper === null) {
      throw new Error('Expected the shared panel media, content, and wrapper to render.');
    }

    expect(panel.classList.contains('sbi-panel--media-compact')).toBe(false);
    expect(media.textContent.trim()).toBe('?');
    expect(content.textContent.trim()).toBe('Shared panel content');
    expect(contentWrapper.classList.contains('sbi-stack')).toBe(true);
  });

  it('maps the compact media-width option to the narrower wide-screen layout class', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.componentInstance.mediaWidth = 'compact';
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const panel = requirePanel(compiled);

    expect(panel.classList.contains('sbi-panel--media-compact')).toBe(true);
  });
});
