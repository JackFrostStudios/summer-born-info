import { TestBed } from '@angular/core/testing';
import { applyA11yDocumentBaseline, resetA11yTestPage } from '@a11y-test-helpers';
import { afterEach, beforeEach } from 'vitest';

beforeEach(() => {
  applyA11yDocumentBaseline();
});

afterEach(() => {
  resetA11yTestPage();
  TestBed.resetTestingModule();
});
