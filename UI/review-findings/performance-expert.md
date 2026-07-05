# UI Performance Expert Review

Scope: `C:\Projects\summer-born-info\UI` only. I reviewed repository guidance, Angular/build configuration, routes, templates, component code, global styles, public assets, and the existing `dist` output. I did not modify source code or run destructive commands.

## Strengths

- The app is already on Angular 22 with SSR/prerender enabled via `provideClientHydration()` and `RenderMode.Prerender` for all routes (`src/app/app.config.ts:8`, `src/app/app.routes.server.ts:5-6`).
- Production builds have initial and component-style budgets configured (`angular.json:49-58`), and the current component styles are small.
- The dependency set is lean: no large UI framework, state library, icon package, date library, or analytics dependency is currently inflating the bundle (`package.json`).
- Templates are mostly static and cheap to render. I did not find `ngClass`, `ngStyle`, legacy structural directives, expensive template expressions, polling, or repeated heavy computations.
- Local state is signal-based, and the theme service guards browser-only APIs behind `isPlatformBrowser()` (`src/app/shell/theme-control/theme-control.service.ts:20-25`, `94-146`).
- The hero image uses `NgOptimizedImage` and `priority`, so Angular emits eager loading, high fetch priority, and a preload for the LCP image (`src/app/features/home/home-hero/home-hero.ts:1-8`, `src/app/features/home/home-hero/home-hero.html:30-36`).
- Static files are served with a long cache lifetime in the SSR server (`src/server.ts:31-32`).

## High-Impact Performance Issues

### 1. Large unoptimized above-the-fold image payload

The homepage LCP image is a 512x512 PNG at about 306.8 KB in the existing built output:

- Source: `public/images/hero-child-playing.png`
- Use site: `src/app/features/home/home-hero/home-hero.html:30-36`
- Built file observed: `dist/summer-born-info/browser/en-GB/images/hero-child-playing.png`

Because the image is marked `priority`, this cost is deliberately pulled into the critical path. That is the right loading priority for an LCP image, but the format/size should be more aggressive.

Recommended remediation:

- Convert the asset to AVIF and/or WebP, keeping a PNG fallback only if needed.
- Target a visual-equivalent asset closer to 40-100 KB for the displayed 512px square.
- If the image will be displayed at different sizes later, add responsive image variants through Angular image loader support or a generated `srcset`.
- Keep `priority` for the final LCP candidate.

### 2. Font payload is large and served as TTF

The app loads two variable TrueType font files:

- `src/styles/_fonts.scss:1-6` normal face, built size about 126.5 KB.
- `src/styles/_fonts.scss:9-14` italic face, built size about 135.4 KB.

That is about 261.9 KB of font payload before compression, and TTF is generally larger than WOFF2 for web delivery. The italic face is used above the fold by the hero highlight, so it can be requested during first render.

Recommended remediation:

- Convert the font assets to WOFF2 and update `format('woff2')`.
- Consider subsetting to the actual Latin glyph range needed for `en-GB`.
- Reassess whether the full variable range `100 900` is needed. If the site uses only 400, 700, and 800, a subsetted static or narrowed variable font may be smaller.
- If keeping the hero italic, preload only the normal/italic WOFF2 files that are truly needed above the fold.

### 3. Routes are eagerly imported, so future pages will inflate initial JS

`app.routes.ts` statically imports every routed component and binds them with `component`:

- Imports: `src/app/app.routes.ts:2-4`
- Route component references: `src/app/app.routes.ts:9`, `14`, `18`

The current built `main-*.js` is already about 274.6 KB uncompressed in the existing output. The current app is small enough that this is not yet alarming, but the routing pattern means new feature routes will keep landing in the initial bundle.

Recommended remediation:

- Convert non-shell feature routes to `loadComponent`, starting with `under-construction`.
- Keep the root shell eager only if it remains part of every route.
- As feature areas grow, route-level lazy loading should become the default.
- Add a stricter JavaScript-specific budget, not just an overall initial budget.

## Medium And Low Issues

### 4. Asset budgets do not guard the real payload hotspots

The production budget warns at 500 KB initial and errors at 1 MB (`angular.json:49-53`), but the biggest current costs are static assets: the hero PNG and fonts. Those are not meaningfully controlled by the current initial JS/CSS budget.

Recommended remediation:

- Add budgets that track broader payload categories such as `all`, `allScript`, `anyScript`, or named bundles as the app grows.
- Add a lightweight asset-size check in CI for `public/images` and `public/fonts`.
- Track compressed sizes as well as raw sizes, because image/font choices affect both.

### 5. Localized build output uses root-relative asset URLs

Source CSS/templates reference assets with root-relative paths such as `/fonts/...`, `/images/...`, and `/icons/...`:

- Fonts: `src/styles/_fonts.scss:3`, `11`
- Hero image: `src/app/features/home/home-hero/home-hero.html:32`
- Theme icons: `src/app/shell/theme-control/theme-control.scss:60-66`
- Builder icon: `src/app/features/under-construction/under-construction.scss:32-36`

The localized output uses `<base href="/en-GB/">`, while assets are emitted under `browser/en-GB/...`. Depending on deployment routing, root-relative URLs can bypass the locale base path and cause extra failed requests or cache fragmentation.

Recommended remediation:

- Confirm the production static hosting contract for localized assets.
- Prefer paths that resolve correctly under the configured base href, or configure deploy URL/asset serving explicitly.
- Verify with a production-like hosted localized build, not only `ng serve`.

### 6. Theme icons use CSS masks and multiple external icon requests

The theme toggle references two SVG mask assets (`src/app/shell/theme-control/theme-control.scss:60-66`) and promotes the icon with `translateZ(0)` (`src/app/shell/theme-control/theme-control.scss:56`).

The files are small, so this is a low-priority issue. Still, these are render-block-adjacent UI assets in the sticky header.

Recommended remediation:

- Inline these tiny SVGs as component markup or CSS data URLs if request count becomes a concern.
- Remove `translateZ(0)` unless profiling shows it helps; unnecessary layer promotion can increase memory and compositing overhead.

### 7. Inline theme script is small but parser-blocking

`src/index.html:8-18` runs a synchronous localStorage read before rendering to avoid a theme flash. The script is intentionally tiny and guarded, so this is acceptable today.

Recommended remediation:

- Keep it small and dependency-free.
- If it grows, move non-critical logic back into Angular and leave only the minimum attribute-setting snippet in `index.html`.

## Concrete Next Steps

1. Convert `hero-child-playing.png` to AVIF/WebP and keep it as the priority LCP image.
2. Convert Hanken Grotesk assets from TTF to subsetted WOFF2; preload only the face(s) required for first paint.
3. Change feature routes in `src/app/app.routes.ts` to `loadComponent` before adding more pages.
4. Add CI-visible budgets/checks for JS bundles plus static asset sizes.
5. Validate localized production hosting for root-relative `/fonts`, `/images`, and `/icons` paths.

## Validation Notes

- I did not run a new production build because the request asked for lightweight read-only review commands. Findings that cite build sizes use the existing `dist/summer-born-info/browser/en-GB` output.
- No source files were changed as part of this review.
