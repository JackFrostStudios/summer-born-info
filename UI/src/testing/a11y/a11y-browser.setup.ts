import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach } from 'vitest';
import { applyA11yDocumentBaseline, resetA11yTestPage } from './a11y-test-helpers';

beforeEach(() => {
  applyA11yDocumentBaseline();
});

afterEach(() => {
  resetA11yTestPage();
  TestBed.resetTestingModule();
});
