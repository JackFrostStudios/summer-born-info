# Front End Architecture Review

Scope: `C:\Projects\summer-born-info\UI` only. Reviewed repository routing guidance, UI agent guidance, the UI project guide, tree structure, routing, shell, feature placement, design-system code, styling foundation, assets, prototypes, tests, and documentation. No source code was modified.

## Strengths

- The current UI structure closely follows the canonical guide. `src/app/app.ts` stays a thin root host, `src/app/shell/root-shell/root-shell.ts` owns shell composition, and routed feature code lives under `src/app/features/`.
- Route ownership is simple and easy to reason about. `src/app/app.routes.ts` keeps the public route tree centralized while the application is small, matching the current project guide.
- The shell/feature boundary is mostly clean. `RootShell` composes `PublicHeader`, `PublicFooter`, and `RouterOutlet`, while homepage rendering remains in `features/home`.
- Shared styling has a mature foundation for the project stage. `src/styles.scss` is only an entry point, shared tokens/primitives are separated under `src/styles/`, and `src/styles/README.md` documents token intent clearly.
- The design-system button is appropriately placed in `src/design-system/button/` now that it has multiple consumers across the homepage, theme control, and under-construction page.
- Strictness and maintainability are actively enforced by configuration: strict TypeScript flags, strict Angular templates, selector prefix rules, accessibility template linting, and custom signal naming restrictions are present in `eslint.config.js` and `tsconfig.json`.
- SSR and i18n are first-class architectural concerns. The app has SSR entries, prerender configuration, source-locale extraction, and documented validation commands.
- Tests are close to the components they cover, which matches the guide's expectation and keeps early architecture verifiable.

## Highest-Impact Architectural Risks

### 1. Homepage CTA Is Coupled To A Temporary Placeholder Route

Evidence:

- `src/app/features/home/home-hero/home-hero.ts:13` injects `Router` directly into the hero component.
- `src/app/features/home/home-hero/home-hero.ts:15` exposes `goToUnderConstruction()`.
- `src/app/features/home/home-hero/home-hero.ts:16` navigates directly to `/under-construction`.
- `src/app/features/home/home-hero/home-hero.spec.ts:76` locks this behavior in as expected CTA routing.
- `src/app/app.routes.ts:16` defines `under-construction` as a normal routed feature next to the real homepage.

Risk:

This makes a temporary product state look like the canonical pattern for future journeys. As the app grows, CTAs may start hard-coding placeholder routes from leaf components instead of expressing user intent through real feature routes, route data, or a small navigation abstraction. The risk is not severe today, but it is likely to create route churn and brittle tests once the real "first step" journey exists.

Remediation:

- Treat `under-construction` as explicitly temporary in routing docs or route metadata.
- When the real CTA destination is known, replace the placeholder route with the owning feature route rather than extending the placeholder pattern.
- Prefer declarative router links for simple navigation CTAs when possible; use imperative `Router` injection only when there is branching, validation, analytics, or other logic.
- If placeholder navigation is needed across multiple features, centralize the pattern in a small shell or routing helper and document its sunset condition.

### 2. Shared Design-System Component Has No Barrel Or Public API Boundary

Evidence:

- `tsconfig.json:17` defines `@design-system/*`.
- `src/app/features/home/home-hero/home-hero.ts:4`, `src/app/features/under-construction/under-construction.ts:4`, and `src/app/shell/theme-control/theme-control.ts:2` import `Button` from `@design-system/button/button`.
- `src/design-system/button/_button-styles.scss` is colocated with the component but is an internal implementation file.

Risk:

Deep imports into component implementation files are workable for a single component, but they do not create a stable public API for shared UI. Future consumers may import internals, styles, or test helpers directly as the design system grows. That weakens dependency direction and makes later refactors more expensive.

Remediation:

- Introduce a narrow public export per shared component folder, such as `src/design-system/button/index.ts`, and import via `@design-system/button`.
- Consider a top-level `src/design-system/index.ts` only when there are enough shared components to justify it.
- Keep Sass partials private to the component folder unless another design-system component genuinely needs the mixin.
- Document the design-system import convention in `UI/AI_PROJECT_GUIDE.md` when the boundary is introduced.

### 3. Styling Governance Depends On Guidance More Than Enforcement

Evidence:

- `src/styles/_tokens.scss:1` defines the shared token layer.
- `src/styles/_primitives.scss:2` defines global layout primitives.
- `src/app/features/home/home-hero/home-hero.scss:36` and `src/app/features/under-construction/under-construction.scss:12` compose local feature surfaces from tokens, which is good.
- `src/app/shell/public-header/public-header.scss:17` uses viewport-scaled font sizing and `public-header.scss:19` uses negative letter spacing, both contrary to the active frontend design guidance for new work.

Risk:

The project has a solid token system, but there is no lint or review checklist that catches drift from token usage, typography guidance, or global primitive boundaries. As more contributors add screens, one-off style decisions can accumulate faster than the architecture docs can steer them.

Remediation:

- Add a lightweight style review checklist to `UI/AI_PROJECT_GUIDE.md` or `src/styles/README.md` covering raw colors, viewport-scaled type, negative letter spacing, global selector additions, and when to add tokens.
- Consider Stylelint later if CSS volume grows enough to justify the dependency.
- Normalize shell typography to shared font-size tokens when practical.
- Keep feature-specific layout in component styles, but promote only repeated, stable patterns into `src/styles/_primitives.scss`.

## Medium And Low Issues

### Prototype Assets Live Beside Production Assets Without A Retirement Signal

Evidence:

- `prototypes/README.md:5` identifies milestone 8 homepage prototypes.
- `prototypes/stitch_summer_born_school_guide/` and `prototypes/stitch_summer_born_school_guide_dark_mode/` remain in the UI tree.
- `public/images/hero-child-playing.png`, `public/images/builder.svg`, and prototype image/font assets coexist in the same UI project.

Risk:

Keeping prototypes in-tree can be useful, but without an owner or retirement rule they can blur source-of-truth decisions. Future contributors may copy prototype HTML/CSS directly instead of using Angular components, tokens, and feature styles.

Remediation:

- Add an explicit status note to `prototypes/README.md`: reference only, not production source.
- Record when a prototype has been fully harvested into Angular and when it can be archived or removed.
- Keep production assets in `public/` and avoid importing directly from prototype folders.

### The Under-Construction Feature Is Generic But Lives As A Full Feature Route

Evidence:

- `src/app/features/under-construction/under-construction.ts:12` defines a full feature component.
- `src/app/features/under-construction/under-construction.html:1` renders a generic placeholder page.
- `src/app/app.routes.ts:16` exposes it as a route peer to homepage.

Risk:

This is acceptable at the current size, but generic placeholder behavior can become a dumping ground for unfinished journeys. If many future routes point here, it may become unclear whether the feature owns content, routing policy, or temporary fallback behavior.

Remediation:

- Keep it only while it has one or two temporary uses.
- If placeholder behavior becomes common, move ownership to a clearly named shell/support area or use route data to render a shared placeholder from a single component.
- Avoid adding feature-specific copy or logic to this generic page.

### Theme State Is Well Encapsulated, But Its App-Level Ownership Should Be Documented

Evidence:

- `src/app/shell/theme-control/theme-control.service.ts:16` provides `ThemeControlService` at root.
- `src/app/shell/theme-control/theme-control.service.ts:21` owns the selected mode signal.
- `src/app/shell/theme-control/theme-control.service.ts:70` applies the app-level document attribute.
- `src/app/shell/theme-control/theme-control.ts:12` consumes the service from a shell control.

Risk:

The implementation is cohesive, SSR-aware, and small. The only architectural risk is discoverability: future app-level preferences may copy the theme service pattern without knowing whether shell-owned root services are the intended home for browser preference state.

Remediation:

- Add a short convention to `UI/AI_PROJECT_GUIDE.md` when the next app-level preference appears: shell-owned services can own cross-cutting browser preferences that affect the whole document; feature state should stay in the owning feature.
- Keep document mutation isolated to these app-level services and out of feature components.

### API Integration Gap Is Correctly Documented But Will Become A Scaling Blocker

Evidence:

- `UI/README.md` documents that no standard API client, proxy, backend URL convention, or end-to-end startup flow exists.
- `UI/AI_PROJECT_GUIDE.md` says not to invent endpoint conventions silently and to document the first concrete pattern.

Risk:

No issue for the current static UI, but the first live feature will set a precedent. If it is added under deadline pressure, endpoint configuration, error handling, request state, and test seams may scatter across components.

Remediation:

- Before the first API-consuming feature lands, add a small architecture decision covering API base URL ownership, local proxy strategy, generated vs handwritten clients, request state, error presentation, and test doubles.
- Place the first reusable API integration pattern where future features can import it without depending on a specific feature.

## Concrete Next Steps

1. Document the temporary nature and exit criteria for `/under-construction`.
2. Add a design-system public import boundary for the button before more shared components are introduced.
3. Tighten style governance with a short checklist or lint-backed rule set once CSS volume increases.
4. Add ownership guidance for app-level browser preference services if another preference beyond theme is introduced.
5. Create an API integration architecture note before the first feature consumes live backend endpoints.

## Overall Assessment

The UI architecture is in good shape for an early Angular application. The important boundaries are present: root host, routed shell, features, design-system component, global style foundation, assets, i18n, SSR, and tests. The main architectural work now is preventative: make temporary routes visibly temporary, give shared UI a public API boundary before it grows, and keep styling/API conventions from being invented feature by feature.
