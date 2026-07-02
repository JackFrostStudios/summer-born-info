import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Button } from './button';

@Component({
  selector: 'sbi-button-test-host',
  imports: [Button],
  template: '<sbi-button>Test action</sbi-button>',
})
class TestHostComponent {}

describe('Button', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();
  });

  it('renders a native button shell with projected content', () => {
    const fixture = TestBed.createComponent(TestHostComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const button = compiled.querySelector('button');

    if (button === null) {
      throw new Error('Expected the shared button to render a native button element.');
    }

    expect(button.getAttribute('type')).toBe('button');
    expect(button.textContent.trim()).toBe('Test action');
  });
});
