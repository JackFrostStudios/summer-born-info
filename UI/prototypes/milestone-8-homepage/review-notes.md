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

## Stakeholder feedback driving the signal refinement pass

Direction C remained compelling for its public-facing energy, but the next round should make that
family feel more clearly designed and easier to review as a homepage rather than a loud concept
sketch. The new options therefore focus on:

- More clearly composed site headers with visible brand, navigation, and action framing
- Different typography systems and colour palettes while staying bold and campaign-like
- Strong public-facing hierarchy without drifting into generic app-shell styling
- Meaningful variation within the same assertive neighbourhood-signal family

## Stakeholder feedback driving the C-family colour pass

The later C-family refinements landed well on layout, hierarchy, and header composition, but the
colour systems still did not feel right. The next pass therefore focuses on:

- Keeping the strongest C-family layout and site-header baseline intact
- Testing dramatically different palette directions without drifting toward Direction B
- Making each option's primary colour obviously different at a glance
- Preserving the assertive, campaign-like, public-facing tone of Direction C

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

## Refined Direction C1: Transit Beacon

**Intent**

Keep Direction C's visible neighbourhood energy, but translate it into a stronger civic-broadcast
identity with a masthead-style header and a cleaner public-information cadence.

**Strengths**

- The header feels most like a finished public site frame rather than a prototype title bar.
- Cobalt, cream, and signal yellow stay energetic while feeling grounded in service communication.
- Condensed display typography makes the page highly scannable for announcements or updates.

**Accessibility and implementation considerations**

- Very large condensed headings will need careful line-breaking and spacing on narrow screens.
- Bright accent yellow should remain paired with dark text to preserve contrast.
- The stronger header architecture gives a clean translation path into a reusable homepage shell.

## Refined Direction C2: Street Notice

**Intent**

Push Direction C toward a street-poster and local-campaign language, using warmer rust tones and a
layered masthead to make the homepage feel visible and active.

**Strengths**

- The layered header has the clearest "designed header" feeling of the three C refinements.
- Poster-inspired serif display type makes the direction feel more local and human than tech-like.
- Rust, sand, and berry create urgency without relying on fluorescent contrast.

**Accessibility and implementation considerations**

- The warmer palette still needs deliberate contrast checks for secondary text and tinted panels.
- Poster-style hierarchy can feel crowded if later implementation adds too many competing messages.
- This is the most overtly campaign-like option, so it should be chosen only if that tone is wanted.

## Refined Direction C3: Assembly Signal

**Intent**

Keep the campaign energy of Direction C, but shift it toward a community-organising tone with bold
banners, stronger contrast blocks, and a fuller brand-and-navigation header.

**Strengths**

- The green and apricot palette feels distinct while remaining active and public-facing.
- The banner-led header creates a memorable front door with clear separation between brand and nav.
- The content blocks feel assertive without depending entirely on one loud accent colour.

**Accessibility and implementation considerations**

- The darker green sections are safer for contrast, but apricot accents should not carry important
  text alone.
- This direction has the strongest brand voice, which may be excellent or excessive depending on
  how neutral the homepage should feel.
- The visual energy is scalable, but only if later Angular implementation stays disciplined about
  how many banner treatments appear on one page.

## Colour Variant C4: Strike Poster

**Intent**

Keep the strongest Direction C masthead logic, but drive the page with a scarlet-led public-notice
palette that feels urgent, visible, and unmistakably campaign-like.

**Strengths**

- The red-led system is the clearest urgency option in the whole prototype family.
- Brass and cream supporting tones stop the page from feeling flat or purely warning-like.
- The header still reads as a designed site frame rather than a title and utility links.

**Accessibility and implementation considerations**

- The scarlet should stay paired with light surfaces and dark text rather than carrying body copy.
- This is the loudest palette, so the Angular implementation would need restraint around repeated
  red blocks.
- If the homepage should feel more supportive than urgent, this may overshoot the desired tone.

## Colour Variant C5: Harbour Flare

**Intent**

Stay inside Direction C's public-signal family while testing a vivid teal-led palette that feels
cleaner, more civic-broadcast, and less expected than the earlier greens and blues.

**Strengths**

- Teal gives the page a strong identity without reading as institutional navy or editorial neutral.
- Acid-lime support colours keep the page active and visible without collapsing into warning red.
- The broadcast-style header feels polished and directional while remaining highly scannable.

**Accessibility and implementation considerations**

- Lime accents should remain secondary and should not carry small text on their own.
- Teal can feel fresher and more contemporary, but the team should confirm it still matches the
  desired trust posture.
- This is likely the easiest of the new palettes to scale across multiple public pages.

## Colour Variant C6: Public Rhythm

**Intent**

Push Direction C into a more contemporary event-poster direction using electric violet as the
primary colour while preserving the same campaign-style hierarchy and designed header framing.

**Strengths**

- Violet is the most unexpected of the C-family primaries and makes the comparison round feel
  clearly different at a glance.
- Peach and cream soften the palette enough to keep the page human rather than synthetic.
- The stacked header and banner language still feel public-facing and deliberate.

**Accessibility and implementation considerations**

- Violet can become harsh if overused, so the final implementation should preserve generous neutral
  space around it.
- Peach accents need deliberate contrast checks whenever they sit behind text.
- This option has strong personality, but it risks feeling more branded or event-like than the
  service may ultimately want.
