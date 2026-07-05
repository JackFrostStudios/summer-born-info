# Angular Expert Review - UI

Review scope: `C:\Projects\summer-born-info\UI` only. No source code was modified.

## What Is Good

- The project is already on the Angular 22 family (`@angular/*` v22, `@angular/build`, TypeScript 6.0) and uses the modern `@angular/build:application` and `@angular/build:unit-test` builders (`UI/package.json`, `UI/angular.json`).
- TypeScript and Angular compiler strictness are strong: `strict`, `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`, `strictTemplates`, strict DI, and strict input access are all enabled (`UI/tsconfig.json:6-27`).
- Components are standalone by default without explicitly writing `standalone: true`, and the code avoids explicit `ChangeDetectionStrategy.OnPush`, matching the repository's Angular v22 guidance.
- The app keeps a thin root host and a separate shell/feature split (`UI/src/app/app.ts`, `UI/src/app/shell/root-shell/root-shell.ts`, `UI/src/app/features/home/home.ts`), which matches the UI project guide.
- New Angular authoring APIs are used well: `input()` and `output()` in the shared button (`UI/src/design-system/button/button.ts:11-18`), `inject()` in components/services, and signals/computed state for theme control (`UI/src/app/shell/theme-control/theme-control.ts:12-23`).
- The project has a useful lint convention for signal naming and template calls (`UI/eslint.config.js:51-87`), which is exactly the kind of local rule that keeps signal templates readable.
- SSR and hydration are already wired (`UI/angular.json:38-45`, `UI/src/app/app.config.ts:7-8`), and browser-only APIs in the theme service are guarded with `isPlatformBrowser` (`UI/src/app/shell/theme-control/theme-control.service.ts:20`, `UI/src/app/shell/theme-control/theme-control.service.ts:88-147`).
- i18n extraction is treated as first-class project state, with source locale, extraction config, missing translation errors for localized builds, and visible template strings marked in current features (`UI/angular.json:20-21`, `UI/angular.json:63-65`, `UI/src/app/features/home/home-hero/home-hero.html:3-38`).
- `NgOptimizedImage` is used for the static hero image with explicit dimensions and priority (`UI/src/app/features/home/home-hero/home-hero.ts:1-11`, `UI/src/app/features/home/home-hero/home-hero.html:30-39`).
- Tests are close to the components they cover and assert visible behavior rather than implementation internals in many places (`UI/src/app/app.spec.ts`, `UI/src/design-system/button/button.spec.ts`, `UI/src/app/shell/theme-control/theme-control.service.spec.ts`).

## High-Impact Issues

### 1. SSR hydration is enabled, but i18n blocks and early interactions are not fully covered

`provideClientHydration()` is configured without `withI18nSupport()` or `withEventReplay()` (`UI/src/app/app.config.ts:5-8`). This app has i18n blocks throughout the shell and homepage (`UI/src/app/features/home/home-hero/home-hero.html:3-38`, `UI/src/app/shell/public-header/public-header.html`, `UI/src/app/shell/public-footer/public-footer.html`). Angular v22 docs state that i18n blocks are skipped by default during hydration unless `withI18nSupport()` is added, and event replay prevents interactions before hydration from being ignored.

Impact: SSR is present, but much of the user-facing DOM may miss full hydration reuse and users can lose a click on the CTA or theme toggle if they interact before hydration completes.

Remediation:

- Import `withI18nSupport` and `withEventReplay` from `@angular/platform-browser`.
- Configure hydration as `provideClientHydration(withI18nSupport(), withEventReplay())`.
- Add a focused smoke check for SSR/hydration in the browser or document a manual verification step with Angular DevTools hydration overlay.

### 2. Route components are eager-loaded beyond the primary landing route

`app.routes.ts` directly imports and binds both routed feature components (`UI/src/app/app.routes.ts:2-18`). Angular v22 route loading guidance recommends eager loading for primary landing pages and lazy loading other pages when bundle size matters.

Impact: This is small today, but it sets the growth pattern. As soon as guidance pages, forms, or API-backed flows arrive, direct route imports will pull feature code into the initial route bundle.

Remediation:

- Keep the homepage eager if it remains the primary landing route.
- Convert secondary routes to `loadComponent`, for example the under-construction route.
- For larger feature areas, use `loadChildren` with colocated route files under `UI/src/app/features/<feature>/`.
- Prefer default exports for lazy route components if the team wants the shorter `loadComponent: () => import('./feature')` style.

## Medium / Low Issues

### 3. Routed pages do not define Angular route titles

The route definitions have no `title` values (`UI/src/app/app.routes.ts:6-22`), so the app falls back to the static document title (`UI/src/index.html:5`). Angular's router supports route titles, and the Angular v22 docs describe them as necessary for an accessible experience.

Remediation: Add `title` to each route, or introduce a small `TitleStrategy` when the project needs a shared suffix such as `Summer-born Info`.

### 4. Template linting misses design-system HTML

The Angular template lint config only applies accessibility and template rules to `src/app/**/*.html` (`UI/eslint.config.js:78-80`), while shared component templates live under `src/design-system/` (`UI/src/design-system/button/button.html`). `angular.json` asks ESLint to lint all `src/**/*.html` files (`UI/angular.json:126-130`), but the flat config does not apply the Angular template rules to all of them.

Impact: Shared UI components can drift from the same accessibility and no-call-expression standards enforced on app templates.

Remediation: Change the template config block to `files: ['src/**/*.html']`, unless there is a deliberate reason to exclude non-app templates.

### 5. Source locale and static HTML language differ

The Angular i18n source locale is `en-GB` (`UI/angular.json:20-21`), but the document shell uses `lang="en"` (`UI/src/index.html:2`). This is a small semantic mismatch for assistive technology, spellchecking, search, and future localized builds.

Remediation: Use `lang="en-GB"` for the source shell, and verify localized builds emit the expected locale language when additional locales are configured.

### 6. A fixed technical ARIA reference is marked for translation

`aria-labelledby="home-heading"` is paired with `i18n-aria-labelledby` (`UI/src/app/features/home/home.html:1`). The value is a technical ID reference, not user-facing text. Translating it can break the relationship if a translator changes the value.

Remediation: Remove translation metadata from fixed ID/ARIA-reference attributes. Keep translating visible labels and actual accessible names such as `alt`, `aria-label`, and headings.

### 7. New singleton service uses `@Injectable` instead of Angular v22 `@Service`

`ThemeControlService` uses `@Injectable({ providedIn: 'root' })` (`UI/src/app/shell/theme-control/theme-control.service.ts:16`). The local guidance says to prefer `@Service()` for new singleton services when supported, and Angular v22 exposes `Service` from `@angular/core`.

Remediation: For new services, use `@Service()` where appropriate. Existing `@Injectable` usage can be migrated opportunistically rather than churned immediately.

### 8. The app has no wildcard or not-found route

The browser routes only cover `/` and `/under-construction` (`UI/src/app/app.routes.ts:6-22`), while server rendering prerenders all paths (`UI/src/app/app.routes.server.ts`). Unknown client routes will have no matching page content.

Remediation: Add a small not-found feature route and a final `**` route before the UI grows, with a route title and tests for shell composition.

## Concrete Next Steps

1. Update hydration config to include i18n support and event replay.
2. Expand template lint coverage to `src/**/*.html`.
3. Add route titles and align `index.html` language/title metadata with the source locale and brand.
4. Convert non-primary feature routes to `loadComponent` as the next routed page is added.
5. Remove translation metadata from technical ID-reference attributes.
6. Add a wildcard not-found route before publishing broader navigation.

## References Used

- Angular v22 hydration guide: https://angular.dev/guide/hydration
- Angular v22 route loading strategies: https://angular.dev/guide/routing/loading-strategies
- Angular v22 route title guidance: https://angular.dev/guide/routing/define-routes
