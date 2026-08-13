# PR 8 Review Comments

## Open Ended / Questions

- `UI/scripts/check-localized-ssr-assets.mjs`
  - Line 1
    - Do we really need this, we are testing the angular build process and our configuration more than anything.
    - response: Keep it as a narrow smoke test. The Angular build only proves the localized bundle compiles, while this script catches runtime/configuration regressions such as a wrong base href, broken locale routing, or missing localized assets.
    - decision: Keep the test. Comment is resolved.

- `UI/scripts/run-a11y-tests.mjs`
  - Line 1
    - Can we avoid the need for checking chrome installation and just run the tests using usual commands.
    - response: Keep the wrapper for now. `npm run test:a11y` still needs a real Chrome/Chromium executable, and this script adds cross-platform `CHROME_BIN` discovery plus a clearer failure mode. It could only be removed if we switch to a browser guaranteed by the toolchain.
    - decision: Keep the wrapper. Comment is resolved.

- `UI/src/app/features/not-found/not-found.html`
  - Line 3
    - This is not using icons in the same format as others, this page needs to be looked at in terms of design and will need a human sign off.
    - response: Will be dealt with in a follow up. Comment not resolved.
    - decision: Will be dealt with in a follow up. Comment not resolved.

- `UI/src/app/shell/public-footer/public-footer.html`
  - Line 17
    - We probably need a link to an "Open Source Software" page here.
    - response: Will be dealt with in a follow up. Comment not resolved.
    - decision: Will be dealt with in a follow up. Comment not resolved.

- `UI/src/app/shell/public-footer/public-footer.ts`
  - Line 8
    - Why has this been added?
    - response: It adds a stable `data-shell-footer="public-footer"` hook on the component host. That mirrors the shell/header convention, but it is not currently consumed anywhere else, so it can be removed safely unless we want to keep it as an explicit test hook.
    - decision: Remove the attribute. Comment not resolved.

- `UI/src/app/shell/public-header/public-header.scss`
  - Line 24
    - What is overflow-wrap: anywhere;
    - response: It allows the brand text to break almost anywhere if needed so it does not overflow the header on narrow screens or with longer translated labels. It is a defensive layout safeguard, not something that affects the current short English text.
    - decision: Explanation expected. Comment is resolved.

- `UI/src/app/shell/public-header/public-header.ts`
  - Line 10
    - Why has this been added?
    - response: It adds a stable `data-shell-header="public-header"` host attribute for tests or automation. It does not affect behavior, and if we do not need that hook we can remove it along with the extra property.
    - decision: Remove the attribute. Comment not resolved.

- `UI/src/design-system/button/button.html`
  - Line 12
    - Do we need to bind touch and keyboard events or does click fired for those?
    - response: The native `<button>` already funnels mouse, touch, and keyboard activation through `click`, so the explicit click binding is enough. Extra touch or key handlers would be redundant unless we want custom pre-click behavior.
    - decision: Click handles all interactions. Comment is resolved.

- `UI/src/design-system/button/button.spec.ts`
  - Line 98
    - Why are we forwarding a test Id? What it it's purpose?
    - response: The forwarded `testId` is an intentional testing hook, not runtime behavior. It gives consumers and host tests a stable `data-testid`, which is especially useful for icon-only or projected-content buttons, and it is documented as part of the public contract.
    - decision: ID is not used. It should be removed. Comment not resolved.

- `UI/src/design-system/panel/panel.html`
  - Line 2
    - Why do we need panel_media - what is displayed here?
    - response: The `panelMedia` slot is the dedicated place for media content such as an image, illustration, or other visual accent that accompanies the panel body. It keeps media separated from the main content so the component can support media-plus-content layouts cleanly.
    - decision: Used to display the hero sections etc. it should remain. Comment is resolved.

## Firm Statements / Needs Addressing

## `README.md`

- Line 1
  - Typo in this header.
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/app.spec.ts`

- Line 17
  - We can simplify these tests,they should just check the right components are loaded as they give us a high level view components and routes are registered correctly, but the components themselves test their own behaviour.
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/features/home/component.a11y-spec.ts`

- Line 8
  - We should add a alias for the a11y test helpers
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/features/home/home.spec.ts`

- Line 57
  - We should split out these tests by intent and only test content we DO display (e.g. the hero should be displayed, the article should be labelled by the header in the hero etc.)
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/features/home/home-hero/home-hero.spec.ts`

- Line 5
  - The assertions here are all over the place, we mainly want to check that the content is displayed.
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/features/not-found/not-found.spec.ts`

- Line 56
  - We don't need to test that classes have been added to the DOM, we just need to know that the rendered output matches the component contract.
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/features/under-construction/under-construction.spec.ts`

- Line 91
  - Same issue with testing classes here.
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/shell/public-header/public-header.spec.ts`

- Line 38
  - Refers to the prototype here, we just want to know the component is configured correctly.
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/shell/theme-control/theme-control.spec.ts`

- Line 74
  - checking class names here again
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/shell/theme-control/theme-control.ts`

- Line 21
  - There is no point storing these as variables, just use the values directly.
  - Ready for implementation: true
  - Open Questions:
    - None.

## `UI/src/app/app.config.spec.ts`

- Line 1
  - I'm not sure I like these tests, essentially they are testing framework wiring rather than application behaviour.
  - Ready for implementation: false
  - Open Questions:
    - Which framework-wiring checks should remain, if any?
    - Should these tests be replaced with higher-level app behaviour tests, or removed entirely?

## `UI/src/app/app.routes.ts`

- Line 50
  - Not Lazy Loaded
  - Ready for implementation: false
  - Open Questions:
    - Which route should be lazy loaded?
    - Should the change preserve the current route path and module/component boundary, or is a route restructure acceptable?

## `UI/src/design-system/button/button.html`

## `UI/src/design-system/icons/icon.html`

- Line 1
  - To avoid this growing too large, we should have a base "icon" component that handles styling and renders ng-content inside. Then the specific icons (e.g. sun-icon) are separate components that pass the SVG as the content to be rendered.
  - Ready for implementation: true
  - Open Questions:
    - None.
