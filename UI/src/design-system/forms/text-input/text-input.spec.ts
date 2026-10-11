import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { disabled, form, readonly, required, validate, type ValidationError } from '@angular/forms/signals';
import { TextInput } from './text-input';

@Component({
  selector: 'sbi-text-input-test-host',
  imports: [TextInput],
  // eslint-disable-next-line @angular-eslint/component-max-inline-declarations -- Integration fixture keeps two real controls and their projected labels together.
  template: `
    <p id="text-help" i18n="Text input test help@@textInputTestHelp">Caller description</p>
    <sbi-text-input
      [$field]="$form.first"
      [$inputId]="firstId"
      [$type]="$type()"
      [$autocomplete]="$autocomplete()"
      [$ariaDescribedBy]="$description()"
    >
      <span i18n="Text input test label@@textInputTestLabel">First label</span>
    </sbi-text-input>
    <sbi-text-input [$field]="$form.second" [$inputId]="secondId">
      <span i18n="Text input test second label@@textInputTestSecondLabel">Second label</span>
    </sbi-text-input>
  `,
})
class TextInputTestHost {
  readonly firstId = 'first-text';
  readonly secondId = 'second-text';
  readonly $model = signal({ first: '', second: '' });
  readonly $required = signal(true);
  readonly $disabled = signal(false);
  readonly $readonly = signal(false);
  readonly $errors = signal<ValidationError[]>([]);
  readonly $type = signal<'text' | 'email'>('text');
  readonly $autocomplete = signal<string | null>(null);
  readonly $description = signal<string | null>(null);
  readonly $form = form(this.$model, (path) => {
    required(path.first, { when: () => this.$required(), message: 'Caller required message.' });
    disabled(path.first, { when: () => this.$disabled() });
    readonly(path.first, { when: () => this.$readonly() });
    validate(path.first, () => this.$errors());
    required(path.second, { message: 'Second required message.' });
  });
}

describe('TextInput', () => {
  let fixture: ComponentFixture<TextInputTestHost>;
  let host: TextInputTestHost;
  let compiled: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [TextInputTestHost] }).compileComponents();
    fixture = TestBed.createComponent(TextInputTestHost);
    host = fixture.componentInstance;
    compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  });

  afterEach(() => TestBed.resetTestingModule());

  it('associates projected labels with distinct native IDs and defaults to text', () => {
    const first = input('first-text');
    const second = input('second-text');
    const firstLabel = compiled.querySelector<HTMLLabelElement>('label[for="first-text"]');
    const secondLabel = compiled.querySelector<HTMLLabelElement>('label[for="second-text"]');

    expect(firstLabel?.textContent.trim()).toBe('First label');
    expect(secondLabel?.textContent.trim()).toBe('Second label');
    expect(firstLabel?.control).toBe(first);
    expect(secondLabel?.control).toBe(second);
    expect(compiled.querySelectorAll('#first-text')).toHaveLength(1);
    expect(compiled.querySelectorAll('#second-text')).toHaveLength(1);
    expect(compiled.querySelector('sbi-text-input')?.hasAttribute('id')).toBe(false);
    expect(second.type).toBe('text');
    expect(first.getAttribute('autocomplete')).toBeNull();
  });

  it('synchronizes native edits and caller model updates through the existing field', async () => {
    const first = input('first-text');
    expect(host.$form.first().dirty()).toBe(false);
    expect(host.$form.first().touched()).toBe(false);

    edit(first, 'typed value');
    await settle();
    expect(host.$model().first).toBe('typed value');
    expect(host.$form.first().dirty()).toBe(true);
    expect(host.$form.first().touched()).toBe(false);
    expect(host.$model().second).toBe('');

    host.$model.update((model) => ({ ...model, first: 'caller value' }));
    await settle();
    expect(first.value).toBe('caller value');
    first.dispatchEvent(new Event('blur'));
    await settle();
    expect(host.$form.first().touched()).toBe(true);
  });

  it('keeps invalid feedback quiet until blur and removes it after correction', async () => {
    const first = input('first-text');
    expect(host.$form.first().invalid()).toBe(true);
    expect(compiled.querySelector('#first-text-error')).toBeNull();
    expect(first.getAttribute('aria-invalid')).toBe('false');
    expect(first.getAttribute('aria-describedby')).toBeNull();

    first.dispatchEvent(new Event('blur'));
    await settle();
    expect(host.$form.first().touched()).toBe(true);
    expect(compiled.querySelector('#first-text-error')?.textContent.trim()).toBe('Caller required message.');
    expect(first.getAttribute('aria-invalid')).toBe('true');
    expect(first.getAttribute('aria-describedby')).toBe('first-text-error');
    expect(compiled.querySelector('#second-text-error')).toBeNull();

    edit(first, 'corrected');
    await settle();
    expect(compiled.querySelector('#first-text-error')).toBeNull();
    expect(first.getAttribute('aria-invalid')).toBe('false');
    expect(first.getAttribute('aria-describedby')).toBeNull();
  });

  it('renders every caller message in Angular order as text in one stable container', async () => {
    host.$required.set(false);
    host.$errors.set([
      { kind: 'first', message: '<strong>First message</strong>' },
      { kind: 'missing' },
      { kind: 'blank', message: '   ' },
      { kind: 'last', message: 'Last message.' },
    ]);
    host.$form.first().markAsTouched();
    host.$form.second().markAsTouched();
    await settle();

    const feedback = compiled.querySelector('#first-text-error');
    expect(feedback?.textContent).toContain('<strong>First message</strong>');
    expect(feedback?.textContent).toContain('Last message.');
    expect(feedback?.textContent.indexOf('First message')).toBeLessThan(
      feedback?.textContent.indexOf('Last message') ?? -1,
    );
    expect(feedback?.querySelector('strong')).toBeNull();
    expect(compiled.querySelectorAll('#first-text-error')).toHaveLength(1);
    expect(compiled.querySelectorAll('#second-text-error')).toHaveLength(1);
    expect(input('second-text').getAttribute('aria-describedby')).toBe('second-text-error');
    expect(input('first-text').getAttribute('aria-describedby')).toBe('first-text-error');

    host.$errors.set([{ kind: 'replacement', message: 'Replacement message.' }]);
    await settle();
    expect(compiled.querySelector('#first-text-error')?.textContent.trim()).toBe('Replacement message.');
    expect(input('first-text').getAttribute('aria-describedby')).toBe('first-text-error');
  });

  it.each([undefined, '', '   '])('does not invent feedback for a message of %s', async (message) => {
    host.$required.set(false);
    host.$errors.set([message === undefined ? { kind: 'caller' } : { kind: 'caller', message }]);
    host.$form.first().markAsTouched();
    await settle();

    expect(host.$form.first().invalid()).toBe(true);
    expect(input('first-text').getAttribute('aria-invalid')).toBe('true');
    expect(compiled.querySelector('#first-text-error')).toBeNull();
    expect(input('first-text').getAttribute('aria-describedby')).toBeNull();
  });

  it('uses caller descriptions when local messages are absent and restores them after correction', async () => {
    host.$description.set('text-help');
    await settle();
    expect(input('first-text').getAttribute('aria-describedby')).toBe('text-help');

    host.$form.first().markAsTouched();
    await settle();
    expect(input('first-text').getAttribute('aria-describedby')).toBe('first-text-error');
    edit(input('first-text'), 'valid');
    await settle();
    expect(input('first-text').getAttribute('aria-describedby')).toBe('text-help');

    host.$errors.set([{ kind: 'no-message' }]);
    await settle();
    expect(input('first-text').getAttribute('aria-invalid')).toBe('true');
    expect(input('first-text').getAttribute('aria-describedby')).toBe('text-help');
    host.$description.set('');
    await settle();
    expect(input('first-text').getAttribute('aria-describedby')).toBeNull();
  });

  it('follows reactive required, disabled and readonly schema metadata on the native input', async () => {
    const first = input('first-text');
    expect(first.required).toBe(true);
    expect(first.disabled).toBe(false);
    expect(first.readOnly).toBe(false);

    host.$required.set(false);
    host.$disabled.set(true);
    await settle();
    expect(first.required).toBe(false);
    expect(first.disabled).toBe(true);
    expect(host.$form.first().disabled()).toBe(true);
    expect(input('second-text').disabled).toBe(false);

    host.$disabled.set(false);
    host.$readonly.set(true);
    await settle();
    expect(first.disabled).toBe(false);
    expect(first.readOnly).toBe(true);
    host.$readonly.set(false);
    host.$required.set(true);
    await settle();
    expect(first.readOnly).toBe(false);
    expect(first.required).toBe(true);
  });

  it('registers its internal native input for field focus', () => {
    host.$form.first().focusBoundControl();
    expect(document.activeElement).toBe(input('first-text'));
    host.$form.second().focusBoundControl();
    expect(document.activeElement).toBe(input('second-text'));
  });

  it('forwards email and autocomplete semantics without adding an email validator', async () => {
    host.$type.set('email');
    host.$autocomplete.set('username');
    await settle();
    const first = input('first-text');
    expect(first.type).toBe('email');
    expect(first.autocomplete).toBe('username');
    edit(first, 'not-an-email');
    await settle();
    expect(host.$model().first).toBe('not-an-email');
    expect(host.$form.first().valid()).toBe(true);
    expect(host.$form.first().errors()).toEqual([]);
    host.$autocomplete.set(null);
    await settle();
    expect(first.getAttribute('autocomplete')).toBeNull();
  });

  function input(id: string): HTMLInputElement {
    const element = compiled.querySelector<HTMLInputElement>(`#${id}`);
    if (element === null) throw new Error(`Expected native input ${id}.`);
    return element;
  }

  function edit(element: HTMLInputElement, value: string): void {
    element.value = value;
    element.dispatchEvent(new Event('input'));
  }

  async function settle(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }
});
