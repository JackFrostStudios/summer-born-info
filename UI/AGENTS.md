# Agents

Use this file when the requested change belongs to the Angular UI in `UI/`.

## Delegation

- Use the `ui-architect` sub-agent for UI code layout, feature placement, and project pattern questions inside `UI/`.
- Use the `ui-automated-test-developer` sub-agent for UI test design and implementation inside `UI/`.

## Delivery Rules

- Assume implementation work already has a corresponding plan in the root `Plans` folder.
- Read the relevant plan before making changes and treat it as the delivery contract unless the user redirects the work.
- If the plan is missing, incomplete, or no longer matches the requested change, pause and align with the user instead of inventing a new UI workflow.
- Update the existing plan to reflect delivered tasks when the work changes its completion state.
- Test in proportion to the UI change:
  - documentation-only work should be validated for command accuracy, links, and internal consistency,
  - component or routing behaviour changes should include or update automated tests where practical,
  - accessibility-sensitive work should be reviewed against WCAG AA expectations.
- Prefer the simplest maintainable solution that fits the existing Angular application shape.
- If you change UI structure, conventions, or canonical file ownership, update [AI_PROJECT_GUIDE.md](./AI_PROJECT_GUIDE.md) in the same work.

## Canonical Project Guide

- Use [AI_PROJECT_GUIDE.md](./AI_PROJECT_GUIDE.md) as the source of truth for UI structure, conventions, and testing/layout expectations.
- Use [../AI_PROJECT_GUIDE.md](../AI_PROJECT_GUIDE.md) first if you need to confirm whether a change belongs in `UI/` or somewhere else in the repository.

## Angular And TypeScript Guidance

### TypeScript Best Practices

- Use strict type checking.
- Prefer type inference when the type is obvious.
- Avoid the `any` type; use `unknown` when the type is uncertain.

### Angular Best Practices

- Always use standalone components over NgModules.
- Must NOT set `standalone: true` inside Angular decorators. It is the default in Angular v20+.
- Do NOT set `changeDetection: ChangeDetectionStrategy.OnPush` explicitly. `OnPush` is the default in Angular v22+.
- Use signals for state management.
- Implement lazy loading for feature routes when the application grows beyond the shell.
- Do NOT use the `@HostBinding` and `@HostListener` decorators. Put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead.
- Use `NgOptimizedImage` for static images.
  - `NgOptimizedImage` does not work for inline base64 images.

### Accessibility Requirements

- It must pass relevant AXE checks for the changed UI.
- It must follow WCAG AA minimums, including focus management, color contrast, semantics, and required ARIA attributes.

### Components

- Keep components small and focused on a single responsibility.
- Use `input()` and `output()` functions instead of decorators.
- Use `computed()` for derived state.
- Prefer inline templates for small components.
- Prefer Signal Forms (`@angular/forms/signals`) for new forms.
- When not using Signal Forms, prefer reactive forms over template-driven forms.
- Do NOT use `ngClass`; use `class` bindings instead.
- Do NOT use `ngStyle`; use `style` bindings instead.
- When using external templates or styles, use paths relative to the component TypeScript file.

### State Management

- Use signals for local component state.
- Use `computed()` for derived state.
- Keep state transformations pure and predictable.
- Do NOT use `mutate` on signals; use `update` or `set` instead.

### Templates

- Keep templates simple and avoid complex logic.
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, and `*ngSwitch`.
- Use the async pipe to handle observables.
- Do not assume globals such as `new Date()` are available in templates.

### Services

- Design services around a single responsibility.
- Use `providedIn: 'root'` for singleton services.
- Prefer the `@Service` decorator over `@Injectable({ providedIn: 'root' })` for new singleton services when supported by the project Angular version.
- Use the `inject()` function instead of constructor injection where it improves clarity and matches existing patterns.
