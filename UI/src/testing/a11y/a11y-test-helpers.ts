import { Component, Type, ViewEncapsulation } from '@angular/core';
import { TestBed, type ComponentFixture, type TestModuleMetadata } from '@angular/core/testing';
import * as axe from 'axe-core';

const documentLanguage = 'en-GB';
const documentTitle = 'Summer-born Info';
const colourModeAttribute = 'data-sbi-colour-mode';
const colourModeStorageKey = 'sbi:colour-mode';

export const a11yColourModes = ['light', 'dark'] as const;

export type A11yColourMode = (typeof a11yColourModes)[number];

@Component({
  selector: 'sbi-a11y-styles-host',
  template: '',
  styleUrl: '../../styles.scss',
  encapsulation: ViewEncapsulation.None,
})
export class A11yStylesHost {}

export function applyA11yDocumentBaseline(): void {
  document.documentElement.lang = documentLanguage;
  document.title = documentTitle;
}

export function applyA11yColourMode(mode: A11yColourMode): void {
  document.documentElement.setAttribute(colourModeAttribute, mode);

  try {
    globalThis.localStorage.setItem(colourModeStorageKey, mode);
  } catch {
    // Browser storage can be unavailable in restricted contexts; the root attribute still applies.
  }
}

export async function renderFixtureForA11y<T>(
  component: Type<T>,
  metadata: TestModuleMetadata = {},
): Promise<ComponentFixture<T>> {
  const additionalImports = (metadata.imports ?? []) as readonly unknown[];

  await TestBed.configureTestingModule({
    ...metadata,
    imports: [A11yStylesHost, component, ...additionalImports],
  }).compileComponents();

  const stylesFixture = TestBed.createComponent(A11yStylesHost);
  stylesFixture.detectChanges();

  const fixture = TestBed.createComponent(component);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();

  return fixture;
}

export async function expectNoA11yViolations(context: axe.ElementContext = document.body): Promise<void> {
  const results = await axe.run(context, {
    reporter: 'v2',
  });

  if (results.violations.length === 0) {
    return;
  }

  throw new Error(formatA11yViolations(results));
}

export function resetA11yTestPage(): void {
  document.body.replaceChildren();
  document.documentElement.removeAttribute(colourModeAttribute);
  document.documentElement.lang = '';
  document.title = '';

  try {
    globalThis.localStorage.removeItem(colourModeStorageKey);
  } catch {
    // Storage cleanup is best-effort in browser automation contexts.
  }
}

function formatA11yViolations(results: axe.AxeResults): string {
  return results.violations
    .map((violation) => {
      const nodes = violation.nodes
        .map((node) => `${node.target.join(' ')}: ${node.failureSummary ?? 'No failure summary provided.'}`)
        .join('\n');

      return `${violation.id} (${violation.impact ?? 'unknown impact'}): ${violation.help}\n${nodes}`;
    })
    .join('\n\n');
}
