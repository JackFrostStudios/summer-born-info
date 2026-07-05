# Design System

This folder owns shared Angular UI components that are reused across multiple shell or feature consumers.
Keep shared component APIs, their canonical structure, and their canonical styling together here rather than
splitting ownership across feature styles or global primitives.

## Ownership

- Add a component to `UI/src/design-system/` only when at least two consumers justify a shared abstraction.
- Keep feature-owned presentation in `UI/src/app/` when it is specific to one route, screen, or shell concern.
- Prefer component-scoped styles here over new global primitives unless the abstraction is truly structure-free.

## Supported Contract Versus Internal Detail

Each design-system component should make a clear distinction between:

- supported contract: selector, documented Angular inputs and outputs, projected-content expectations,
  accessibility expectations, and any deliberately supported composition patterns;
- internal detail: private DOM structure, internal CSS classes, internal custom properties, SCSS mixins,
  and implementation-only styling hooks that consumers must not override or depend on.

If a styling hook is not documented in the component README as supported, treat it as internal even if it is
currently visible in DevTools or source.

## Documentation Expectations

- Add or update a README beside each shared component folder when its API or usage expectations change.
- Document the component purpose, supported inputs and outputs, accessibility expectations, and styling
  contract.
- Call out known intentional limitations so feature code does not invent unsupported workarounds.

## Import And Usage Conventions

- Import shared components directly from their owning folder until the UI explicitly adds a design-system
  barrel or component index.
- Prefer Angular composition over feature-level CSS overrides when adapting a shared component.
- Keep tests beside the shared component or behavior they validate.

## Accessibility Expectations

- Shared interactive components should preserve native semantics whenever possible.
- Document any required accessible-name, state, or focus behavior in the component README.
- Treat keyboard, focus-visible, and disabled-state behavior as part of the supported contract.

## Current Components

- [Button](./button/README.md): shared `sbi-button` component for primary and secondary button presentation.
