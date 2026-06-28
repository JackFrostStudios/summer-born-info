# Milestone 8 Homepage Prototype Review Notes

## Shared review frame

All three directions keep the same broad structure so stakeholders can compare style rather than
page architecture:

- Header with simple navigation framing
- Hero section with a clear homepage promise
- Supporting guidance cards or steps
- Trust or reassurance content
- Footer with practical follow-up links

The copy is intentionally draft-level and should be treated as easy to refine after a direction is
chosen.

## Stakeholder feedback driving the refinement pass

Direction B was the strongest baseline, but the next round should move away from soft rounded-card
framing and toward:

- Square or mostly square sections
- More separation through whitespace and rules instead of cards everywhere
- A more polished header composition than a title with native-looking links
- Meaningfully different typography and colour schemes while staying editorial and warm

## Direction A: Civic Clarity

**Intent**

Present the service as calm, dependable, and public-service adjacent. This is the most procedural
and guidance-led direction of the three.

**Strengths**

- Feels trustworthy and stable for a content area that may involve uncertainty or deadlines.
- Structured cards and a tighter grid would translate cleanly into Angular components later.
- High contrast, clear section boundaries, and conservative styling lower accessibility risk.

**Accessibility and implementation considerations**

- The dark teal on cream palette is safe, but any lighter secondary copy should be contrast-checked
  if the final implementation softens it.
- Dense information blocks could feel heavier on mobile unless spacing remains disciplined.
- This direction is the easiest to implement with minimal shared tokens and no external assets.

## Direction B: Editorial Warmth

**Intent**

Make the homepage feel more human and reassuring, using magazine-like typography and softer
surfaces to reduce institutional stiffness.

**Strengths**

- Most emotionally supportive and least bureaucratic in tone.
- Generous spacing gives content room to breathe and makes the page feel premium without extra
  graphics.
- The serif and sans pairing creates a distinctive public-facing identity that future informational
  pages could extend.

**Accessibility and implementation considerations**

- Large serif headings are expressive, but the final Angular version should keep line lengths short
  and avoid overly light font weights.
- Warm low-contrast neutrals need careful checking if any additional muted colors are introduced.
- The spacious rhythm is feasible, but the team should agree whether this softer tone matches the
  product's long-term trust posture.

## Direction C: Neighbourhood Signal

**Intent**

Frame the homepage as an energetic campaign or community resource, using strong shapes and bold
contrast to make the information feel current and proactive.

**Strengths**

- Most memorable and visually differentiated of the three directions.
- Strong hierarchy makes key calls to action and deadlines easy to scan.
- The card-and-band structure could scale well to future public landing pages if the team wants a
  more assertive brand.

**Accessibility and implementation considerations**

- The brighter palette and condensed heading style need the most contrast and readability care,
  especially on smaller screens.
- This direction would require the most restraint during implementation to avoid looking noisy once
  real content grows.
- It is still straightforward in HTML and CSS, but the bold visual language raises the risk of
  later churn if stakeholders want a more neutral tone after approval.

## Recommendation prompt for stakeholders

When choosing a direction, focus on:

- Which tone best fits how trustworthy and welcoming the service should feel
- Whether the spacing density feels appropriately informative versus overwhelming
- Whether the visual system seems reusable for later public pages without a full redesign
- Whether any prototype should be chosen as-is or used as the base for a small blend of elements

## Refined Direction B1: Morning Ledger

**Intent**

Keep the reassuring editorial mood of Direction B, but sharpen it with a masthead-style header,
crisper navy contrast, and quieter section framing.

**Strengths**

- The polished header immediately feels more deliberate and homepage-ready.
- Strong serif display type keeps warmth while the darker ink palette improves readability.
- Sections are defined mostly by whitespace, rules, and layout rhythm rather than stacked cards.

**Accessibility and implementation considerations**

- The thin divider lines work because the palette is darker; they should remain subtle but visible.
- Header navigation styling should preserve obvious focus states if implemented in Angular later.
- This is the cleanest option if the team wants warmth without drifting too soft or lifestyle-like.

## Refined Direction B2: Hearth Bulletin

**Intent**

Lean into warmth through clay and oat tones, but make the page feel more service-oriented by using
a composed utility bar, simple content rails, and restrained paneling.

**Strengths**

- Feels calm and human without relying on rounded surfaces.
- The warm palette is distinctive but still practical for informational content.
- The layout gives the content more breathing room and reduces the impression of stacked feature
  cards.

**Accessibility and implementation considerations**

- The lighter palette needs careful contrast checks if secondary text becomes any softer.
- The small utility details in the header should not reduce touch-target size on mobile.
- This direction is a strong middle ground if stakeholders want warmth plus a little more civic
  order.

## Refined Direction B3: Sunday Review

**Intent**

Push Direction B toward a more literary, weekend-journal style with deeper greens, elegant
hierarchy, and broad whitespace bands that feel composed rather than boxed in.

**Strengths**

- Most typographically distinctive of the refinements while remaining grounded.
- Whitespace-led sectioning makes the page feel confident and less componentized.
- The navigation and hero balance feels polished without looking corporate.

**Accessibility and implementation considerations**

- The moss palette should be checked carefully if muted olive accents are expanded later.
- Large editorial headings need disciplined line length and spacing on narrow screens.
- This is the most characterful refined option, but it may feel more brand-led than service-led
  compared with B1 or B2.
