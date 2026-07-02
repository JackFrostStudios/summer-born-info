import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Button } from './button';

@Component({
  selector: 'sbi-button-test-host',
  imports: [Button],
  template:
    '<sbi-button [buttonType]="buttonType" [disabled]="disabled" [ariaPressed]="ariaPressed" [ariaLabel]="ariaLabel" [ariaLabelledBy]="ariaLabelledBy" [ariaDescribedBy]="ariaDescribedBy" [testId]="testId" (pressed)="handlePressed($event)"><span class="projected-content">Test action</span></sbi-button>',
})
class TestHostComponent {
  buttonType: 'primary' | 'secondary' = 'primary';
  disabled = false;
  ariaPressed: 'true' | 'false' | null = null;
  ariaLabel: string | null = null;
  ariaLabelledBy: string | null = null;
  ariaDescribedBy: string | null = null;
  testId: string | null = null;
  lastPressedEvent: MouseEvent | null = null;

  handlePressed(event: MouseEvent): void {
    this.lastPressedEvent = event;
  }
}

function requireButton(compiled: HTMLElement): HTMLButtonElement {
  const button = compiled.querySelector('button');

  if (!(button instanceof HTMLButtonElement)) {
    throw new Error('Expected the shared button to render a native button element.');
  }

  return button;
}

describe('Button', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();
  });

  it('renders a native button shell with projected content', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.detectChanges();

    const host = fixture.componentInstance;
    const compiled = fixture.nativeElement as HTMLElement;
    const button = requireButton(compiled);
    const projectedContent = button.querySelector('.projected-content');

    if (projectedContent === null) {
      throw new Error('Expected the shared button to render projected content.');
    }

    expect(button.getAttribute('type')).toBe('button');
    expect(button.classList.contains('sbi-button')).toBe(true);
    expect(button.classList.contains('sbi-button--secondary')).toBe(false);
    expect(button.textContent.trim()).toBe('Test action');
    expect(projectedContent.textContent.trim()).toBe('Test action');
    expect(host.lastPressedEvent).toBeNull();
  });

  it('maps the secondary variant to the secondary button class', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.componentInstance.buttonType = 'secondary';
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = requireButton(compiled);

    expect(button.classList.contains('sbi-button--secondary')).toBe(true);
  });

  it('forwards disabled, aria, and test-hook inputs to the native button', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.componentInstance.disabled = true;
    fixture.componentInstance.ariaPressed = 'true';
    fixture.componentInstance.ariaLabel = 'Toggle theme';
    fixture.componentInstance.ariaLabelledBy = 'theme-label';
    fixture.componentInstance.ariaDescribedBy = 'theme-description';
    fixture.componentInstance.testId = 'theme-toggle';
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = requireButton(compiled);

    expect(button.disabled).toBe(true);
    expect(button.getAttribute('aria-pressed')).toBe('true');
    expect(button.getAttribute('aria-label')).toBe('Toggle theme');
    expect(button.getAttribute('aria-labelledby')).toBe('theme-label');
    expect(button.getAttribute('aria-describedby')).toBe('theme-description');
    expect(button.getAttribute('data-testid')).toBe('theme-toggle');
  });

  it('re-emits the native click event through the pressed output', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.detectChanges();

    const host = fixture.componentInstance;
    const compiled = fixture.nativeElement as HTMLElement;
    const button = requireButton(compiled);

    button.click();
    fixture.detectChanges();

    expect(host.lastPressedEvent).toBeInstanceOf(MouseEvent);
    expect(host.lastPressedEvent?.type).toBe('click');
  });

  it('does not emit pressed when the native button is disabled', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.componentInstance.disabled = true;
    fixture.detectChanges();

    const host = fixture.componentInstance;
    const compiled = fixture.nativeElement as HTMLElement;
    const button = requireButton(compiled);

    button.click();
    fixture.detectChanges();

    expect(host.lastPressedEvent).toBeNull();
  });

  it('keeps the native button shell focusable for keyboard users', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = requireButton(compiled);

    button.focus();

    expect(document.activeElement).toBe(button);
  });
});
