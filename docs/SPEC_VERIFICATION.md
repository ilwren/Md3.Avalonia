# Specification conformance harness

This document describes the automated layers that check Md3.Avalonia against Material Design 3,
what each one can prove, and — more importantly — what it cannot.

It exists because of a specific failure. The 2026-10-02 audit reported "100% compliance" on the
strength of tests that compared the library's resource dictionaries against *the library's own
resource dictionaries*. Such a test cannot fail, so it is not evidence. Everything below is
designed so that a passing run means something an outside reader can check.

---

## 1. The oracle

All expected values live in [`spec-snapshot/`](../spec-snapshot/), outside the source tree:

| File | Role |
| --- | --- |
| `manifest.json` | Provenance. Pinned m3.material.io URLs with a retrieval date, the frozen `androidx/androidx` commit, Flutter version, font commit, Avalonia version, and the `claim_policy` list of things the project refuses to claim. |
| `tokens.json` | The values themselves: resource key, type, expected value, **confidence**, and a pointer back to the source that justifies it. |
| `conformance-policy.json` | The ratchet: `mode`, per-layer switches, visual-diff thresholds, and explicit waivers. |

Two rules keep the oracle honest:

1. **It is never generated from Md3.Avalonia.** `scripts/sync-spec-tokens.py` regenerates and
   cross-checks it from the pinned androidx token sources, never from `src/`.
2. **Confidence is explicit.** `high` means the number is traceable to a published source and may
   fail a build. `medium` means it is plausible but was not re-verified this cycle; it is reported
   and never blocks. Guessing a value and marking it `high` is the one way to make this whole
   harness lie, so a promotion from `medium` to `high` should be reviewed like a code change.

---

## 2. The layers

| Layer | Where | Needs | What it proves |
| --- | --- | --- | --- |
| **L0** static lint | `scripts/lint-design-tokens.py` | Python only (~0.2 s) | No literal colours in shipped themes; every `{DynamicResource Md.*}` resolves; no broken `ResourceInclude`; token text matches the oracle. |
| **L1** token conformance | `Spec/MdTokenConformanceTests.cs`, `Spec/MdBaselineColorTests.cs` | headless session | Values the *resource system actually hands a control* match the oracle, through the real 58-dictionary merge, in both theme variants. The colour scheme is additionally compared role by role against the published M3 baseline, in the static theme and in the generator's output. |
| **L2** geometry & targets | `Spec/MdGeometryConformanceTests.cs` | headless session | Arranged bounds and corner radii of template parts match the oracle, and every interactive control offers a 48×48dp hit-testable target. |
| **L3** visual goldens | `Spec/MdVisualGoldenTests.cs` | headless session | Pixels did not change unintentionally — the only layer that catches regressions nobody predicted. |
| **L5** motion | `Spec/MdMotionConformanceTests.cs` | partly none, partly headless | Spring physics match an independent integration; published durations really are the settling points they claim to be; the runtime integrator and the closed form agree. |
| **L4** inventory | `Spec/MdInventoryTests.cs`, `Spec/MdTemplateSourceTests.cs`, `Spec/MdLocalizationSourceTests.cs` | headless + source scan | How much of the library is measured at all: declared tokens against the merged dictionary, themed types against the geometry matrix, template parts against `[TemplatePart]`, colour roles against WCAG in every generator configuration. Reports the backlog; two of its checks are invariants and gate the build. |

### Why L0 and L1 both exist

They answer different questions and fail in different ways.

L0 parses XAML text, so it runs in a fraction of a second with no SDK and can gate a pull request
before anything is restored. It cannot see merge order, theme dictionaries, or anything resolved at
runtime.

L1 resolves through the live resource system. It is the only thing that catches a token shadowed by
a later `ResourceInclude`, or one stranded in the wrong `ThemeDictionaries` entry — a class of bug
that is completely invisible to text parsing and completely invisible to a passing light-theme
screenshot.

### Why L1 and L2 both exist

`MdChip` is the worked example. Its theme hard-codes 32dp and 8dp instead of exposing
`Md.Comp.Chip.*` tokens, so it is a **gap at L1** (the token is missing) and a **pass at L2** (the
rendered geometry is right). A token that exists but is never bound in a template is the mirror
image. Neither layer alone tells you whether a user sees the correct chip.

### What L4 is for

L1–L3 and L5 all answer "is this value right". None of them answers "how much of this library is
answered at all", and that is the question a preview release needs: **157 types carry a
`ControlTheme` while L2 lays out eight of them**, and **144 oracle entries point at 585 declared
tokens**. Both of those numbers were invisible before L4 existed; neither is a defect.

L4 therefore measures the library against itself and reports, it does not adjudicate:

- declared tokens that the merged theme does not resolve (a key stranded in the wrong
  `ThemeDictionaries` entry renders with a fallback and fails nothing);
- themed controls with no rendered-geometry case;
- `Md*` classes with no theme at all;
- `PART_*` names a template uses that the owning type never declares;
- dual-declared tokens — 27 keys exist both as a static XAML value and as an assignment in
  `MdThemeManager.Apply`, so L1 only ever sees the first and a dynamic-theme app only the second;
- WCAG AA failures across 6 scheme variants × 2 modes × 3 contrast levels, where the existing suite
  checks one configuration;
- localization keys that one culture cannot render, or whose `{0}` placeholders disagree.

Five of these do gate a run, because they are invariants rather than smells:

1. **a template must realise the parts it names** (`Control_Templates_Expose_The_Parts_They_Name`);
2. **a declared token must resolve through the real merge**, in both variants
   (`Every_Declared_Token_Resolves_In_Both_Theme_Variants`);
3. **a `[TemplatePart]` attribute must have a matching `x:Name` in its template**
   (`Declared_Template_Parts_Exist_In_Their_Template`);
4. **a themed dictionary must declare the same keys in Light and Dark**
   (`Both_Theme_Variants_Declare_The_Same_Keys`);
5. **no culture table may define a key twice, and every culture must carry the English key set with
   the same placeholders** (`MdLocalizationSourceTests`).

The last one exists because a duplicate key in a dictionary literal is invisible at run time: the
later entry wins, so the object that comes back matches the source exactly. Only the text can show it.

Which keys does the resolution gate actually cover? `MaterialTheme.axaml` merges 66 dictionaries and
`DeclaredTokenKeys()` resolves its list from that include list rather than by globbing `src`, so a
dictionary that exists but is nobody's `ResourceInclude` is not counted as verified. 52 `Md.*` keys
in sibling packages (PropertyGrid, AvaloniaEdit, DataGrid, Dock) ship in their own `Styles`
collection, which an application opts into with `Styles.Add`; they are reported as
`tokens_outside_the_core_theme` instead of being asserted against the wrong application.

The rest become numbers in `artifacts/spec/L4-*.json`, and the JSON twin of each report carries the
member list behind every inventory count, not just the count.

---

### The budget: what "still missing" means, numerically

`spec-snapshot/conformance-policy.json` answers *may this build pass*. It cannot answer *is the
library getting closer to the specification*, because it only knows how to fail on a high-confidence
divergence. `spec-snapshot/conformance-budget.json` is the second question, and
`scripts/check-conformance-ratchet.py` enforces it in two passes:

- **static-checks** runs it with `--allow-not-measured`: the L0 metrics only, no SDK needed;
- **build-and-test** runs it with `--strict` after the suite: a pinned metric this run could not
  measure is a failure, because an unverified budget entry is an unverified claim.

Each entry carries `direction` (`lower` = a gap with a ceiling, `higher` = coverage with a floor)
and `budget`. Moving a budget is therefore a visible diff, and only one direction is ever expected:

| metric | meaning | today |
| --- | --- | ---: |
| `hardcoded_geometry` | themable literals inside every shipped `Themes/*.axaml` | 268 |
| `hardcoded_ui_text` | English authored in a shipped template, bypassing `MdLocalization` | 0 |
| `orphan_tokens` | declared tokens nothing references | 73 |
| `dual_declaration_keys` | keys declared in both XAML and `MdThemeManager` | 27 |
| `oracle_coverage_ratio` | oracle entries ÷ declared keys | 0.3423 |
| `oracle_families` | component families with at least one oracle value | 19 |
| `measured_controls` / `measured_cases` | controls and cases the L2 matrix lays out | 14 / 43 |
| `armed_goldens` | committed L3 baselines | 0 |
| `undeclared_template_parts` | `PART_*` names with no `[TemplatePart]` | 64 |
| `templates_without_part_contract` | themed controls whose template has no contract at all | 19 |
| `templates_with_a_part_contract` | themes whose target type declares its parts | 103 |
| `variant_asymmetric_tokens` | tokens declared in one theme variant only | 0 |
| `tokens_outside_the_core_theme` | `Md.*` keys in sibling theme packages | 52 |
| `unmeasured_themed_controls` | themed controls with no L2 geometry case | 144 |
| `unstyled_control_classes` | `Md*` classes with no control theme (mostly parts) | 85 |
| `unresolved_declared_tokens` | declared keys the merged theme cannot resolve | 0 |
| `contrast_failures` | WCAG AA failures across every generator configuration | 0 |
| `constructible_themed_controls` | themed controls with an implicit theme | 115 |
| `constructed_themed_controls` | of those, the ones that built and laid out | 114 |
| `rendered_themed_controls` | of those, the ones that produced a visual tree | 110 |

`contrast_failures` is the one entry above with no ceiling even though it measures zero: contrast
remediation is out of scope for this round, so the number is recorded and nothing gates on it.

### The colour scheme is the published one, not an approximation of it

Material fixes the baseline scheme as 49 named roles with literal values, and `material-web` ships them
as tokens (`tokens/versions/v0_192/_md-sys-color.scss` over `_md-ref-palette.scss`, pinned in
`spec-snapshot/manifest.json`). `MdBaselineScheme` is that table transcribed, and `MdBaselineColorTests`
compares three things against it on every run: the static theme in both variants, the generator's output
for the default options, and the structure (every role ships a `.Brush`; the 12 fixed roles are
identical across variants).

The third comparison exists because the generator and the theme used to disagree. `MdThemeOptions`
defaults to `#6750A4` with tonal spot and standard contrast, which *is* the baseline, but running that
seed through HCT returns `#65558F`: tone mapping snaps each role to the tone grid, and it does not
round-trip the colour it started from. So a default application repainted every accent away from the
specification it claimed to implement, and the disagreement showed up only as eight budgeted
`dual_declaration_mismatches`. It is now zero, and the ceiling is zero. Any other seed, variant or
contrast level still goes through HCT; only the exact baseline request is served from the table.

One structural limit worth stating: `spec-snapshot/tokens.json` keys a value by token name, so it cannot
hold a light value and a dark value for the same role, and 36 of the 49 roles legitimately have both.
The baseline is therefore asserted in C# rather than added to the oracle file — adding the light values
alone would have made the oracle a half-truth and inflated `oracle_coverage_ratio` without measuring
anything new.

### The oracle cannot catch a value that is wrong in both places

Every token in `spec-snapshot/tokens.json` was transcribed by hand from Material's pages, and every one of
them is compared against the theme on each run. That is a real check, and it has one blind spot that no
amount of coverage fixes: if a number is transcribed wrong into the oracle *and* into the theme, the two
agree with each other, the layer passes, and the library ships the wrong number with a green report saying
it matches the specification.

`spec-snapshot/component-tokens.json` closes that, by taking the oracle out of the loop. It is a
machine-produced dump of Material Web's own token files at the pinned commit - 55 `_md-comp-*.scss` files,
307 values - and it holds only what the specification states as a number: a px literal, or an `md-sys-shape`
step resolved against `_md-sys-shape.scss`. Colours and type-scale references are deliberately absent,
because a second copy of the colour scheme would drift from `MdBaselineScheme`, and the type scale is
already covered.

`MdComponentTokenTests` then compares 49 of this library's `Md.Comp.*` keys against it at runtime, through
the same theme merge the application uses. The mapping is the interesting part and it lives in the snapshot
rather than in the test: Material ships one file per variant (`fab-primary-small`) where this library ships
one token per size (`Fab.Small.*`), and its names are more specific (`container-height` for what this library
calls `Size`). Every key with no counterpart is listed as a measured queue of 220, not silently skipped, and
the three that are out of reach on purpose say why in `excluded_by_design`.

The first run found one real divergence - a large FAB icon at 32dp where `fab-primary-large.icon-size` is
36px - and then produced eleven findings that were the test's own fault, in three waves:

- shape values were compared as scalars, because the snapshot stores a shape as its number *and* the step
  it came from, and the theme resolves it as a `CornerRadius`;
- border widths read as "not declared at all", because this library authors them as a `Thickness` (one
  value per edge) where Material states one number. A *uniform* quad is that number; a non-uniform one is a
  real difference, and the check now says which;
- and the shape check itself was wrong twice. It first demanded that anything Material calls `corner-full`
  hold the 999 step, which flagged three tokens that were fine - a 6dp dot badge rounds at 3, a 16dp badge
  at 8, a 40dp state layer at 20: exactly half the box, which *is* full. Then it fell back to a "at least
  28" threshold for the tokens it had no box for, which is the scale's largest fixed step and not a
  property of any control: a 40dp round button rounds at 20 and is still a circle. The invariant that
  survives is the real one - a full-shape token must be at least as round as the box it rounds - and every
  such token is now paired with its box.

That sequence is worth recording because it is the shape of the whole exercise: a check that measures the
wrong invariant produces findings that look like defects, and "fixing" them would have made the library
worse (a dot badge with a 999dp radius is the same circle; a badge whose radius changed from 3 to 999 is a
cosmetic change nobody asked for). The rule applied throughout - never widen a gate, never round a value to
make a comparison pass, and check the meaning rather than the number - is what turned those eleven findings
into one.

#### The snapshot was only half the specification

The component-token layer started as "307 literal values across 55 files", and that phrasing hid a
coverage bug of exactly the kind the layer exists to catch elsewhere: Material states a great many
values as a *reference* to the published type scale rather than as a number -

```scss
'input-text-size': map.get($deps, 'md-sys-typescale', 'body-large-size'),
```

- and the collector only kept `if($exclude-hardcoded-values, null, 16px)`. So 56 font-size tokens were
invisible: not compared, not counted as missing, simply absent from both the snapshot and the report.
A Checkbox that had forgotten to set its label size would have passed every layer in this repository.

The snapshot now resolves those references through the same published scale the theme uses (the 15 steps
in `Md.Sys.TypeScale.*.Size`) and keeps the step name beside the value, so the provenance survives the
resolution and a later change to the scale is visibly a re-baseline rather than a silent drift. That is
what let the text-field and combo-box font sizes be checked at all, and all seven agreed on the first run.

Two findings from the same round are worth recording because both are the check being wrong rather than
the library:

- a filled text field's underline is stated as `active-indicator-height: 1px` and authored here as
  `Thickness(0, 0, 0, 1)`. The comparison was demanding a uniform quad and would have "fixed" a correct
  underline into a boxed field. A single non-zero edge equal to the stated value is now accepted as the
  same property; a *non*-uniform quad with more than one distinct non-zero edge still fails;
- the combo box's dropdown is `0,0,4,4` - round at the bottom only, because it attaches to the field -
  while Material's `menu-container-shape` is 4 all round. Same step, different surface, so the pair was
  removed and the reason recorded in `excluded_by_design` rather than left as a passing coincidence.

### The geometry metric measured the wrong tree, and then it halved

`hardcoded_geometry` is the largest number in the table and for a long time it was also the least
honest one. `rule_hardcoded_geometry` globbed `src/Md3.Avalonia/Themes/Controls/*.axaml`, one folder
of one package. The sibling packages author control themes exactly the same way - `Md3.Avalonia.Extra`
alone had 198 literals - and every one of them was outside the measurement. The metric said 279
because it was not looking at the other 195.

The rule now walks every `src/Md3.Avalonia*/Themes/*.axaml`, which is the artefact the requirement is
actually about. On the old tree that correction reads 474, so the first thing the fix does is make the
number worse, and that is the correct direction: a metric that flatters the tree cannot be burnt down,
it can only be believed. Against the corrected measurement, 218 literals became tokens:

- every `FontSize` whose value is a step of the published type scale resolves through
  `Md.Sys.TypeScale.*.Size`, including `MdText`'s 15 type-scale classes, where the matching
  `.LineHeight` replaced the literal line height too;
- every `CornerRadius` whose value is on the M3 shape scale (4, 8, 12, 16, 20, 28) resolves through
  `Md.Sys.Shape.Corner.*`;
- both the attribute form and the `Setter ... Value=` form, in all seven packages.

`hardcoded_geometry` is now 269 and the budget is 269. What is still literal is deliberate and each
case is a different bug:

- **values with no token**: `MinHeight="56"`, `MinWidth="160"`, the width of a dialog. No `Md.Comp.*`
  measurement exists for most of these, so there is nothing to point at until one does;
- **derived values**: an inset ring is one step inside its parent (18 outside, 16 inside), and a
  per-corner quad (`0,16,16,0`) composes two steps. Rounding either to the nearest token changes
  pixels, so they stay numbers;
- **values off the scale**: 13, 10, 9, 17, 18, 22. Several of those *are* design decisions worth
  revisiting (a 13dp label is not a Material step), but replacing them with the nearest step is a
  visual change, and a commit that claims "no visual change" must not contain one.

That last rule was enforced mechanically rather than by eye: the substitution was replayed against
`SystemTokens.axaml` and every literal/token pair whose numbers differed was reverted. Fifteen were.

### L4: the template contract

`undeclared_template_parts` started at 470 and is now 64. Every `PART_*` name a `ControlTheme`
contains on one of this repository's own controls is a `[TemplatePart]` on the class that theme
targets, with the element type read from the template. That is what makes a part name part of the
public surface: before, a rename on either side was caught by nothing, and no consumer could
discover `PART_SupportingText` existed short of reading the AXAML.

The 64 that remain name parts of framework types this repository re-themes (`Button`,
`ListBoxItem`, `TextBox`, `Window`, `DataGridCell`, `Thumb`). A framework type cannot carry our
attribute, so this is the boundary of the technique rather than a backlog, and the gate keeps
matching templates against declarations in the other direction so an attribute that promises a part
its template does not deliver still fails.

Reading those numbers needed the scanner fixed first. It ran a text window from one `<ControlTheme>`
to the next, so after a self-closing theme it swallowed the following templates and attributed their
part names to the wrong control: 39 phantom gaps, and it hid one real omission (`MdFadeThrough`).
The scan now walks the parsed document.

### L2c: the chrome a theme draws follows the culture

`hardcoded_ui_text` reached 0 by moving eleven English strings out of the shipped templates and into
localized properties. The lint can only see a literal, so the other half of the hole is closed by
`MdThemedTextTests`: it switches the inherited `MdLocalization.Culture` between `en` and `zh-Hans` and
asserts every one of those labels is non-empty and actually differs. A string moved into C# but never
bound would pass the lint and still be frozen for every consumer.

That test found a real defect on its first run — `MdPaginatedDataTable` resolved its new label in the
constructor and never re-raised it on a culture change. The fix was a class handler, and the lesson is
recorded in the test's own comments: a made-up sentinel, or a check that measures the panel instead of
the control, is how a coverage number stops meaning anything.

### L2b: the coverage numbers, filled in

L2 measures fourteen controls against the oracle. L2b asks the question underneath it, for every themed
control at once, and the first run answers it: **114 of 115 themed controls with an implicit theme
construct, theme and lay out, and 110 of them produce a visual tree.** The gap between those two
numbers is named rather than averaged away — three are `TextBlock`/`ContentPresenter` subclasses that
draw through `Text`/`Content`, one (`MdSliderThumb`) is only ever themed by the slider's own template,
and one (`MdWindowResizeGrip`) has a property-only theme because it is a hit target, not a surface.

That layer gates two things, both statements of fact: construction must not throw, and an
`{x:Type}` theme that sets `Property="Template"` must actually deliver a template to a bare instance.
It deliberately does not gate "did it render something", because a control that legitimately draws
nothing and a control that lost its template are indistinguishable from the outside — so the second
would have needed an exception list, and an exception list is how a check stops meaning anything.

A `null` budget means **tracked, not gated yet**: the number is measured and printed, and it gets a
ceiling once a run has established it. That is why most of the table was `null` in the first commit
and pinned in the second: the numbers come from a headless run, and inventing a threshold before one
has happened is exactly the defect `manifest.json` forbids for token values. Inventing a threshold before the measurement would be exactly the
defect `manifest.json` forbids for token values.

### What L5 deliberately does not do

Asserting `MdSpring.Sample` returns what `MdSpring.Sample` returns is the self-referential trap
again. L5 only makes claims an independent computation can falsify:

- the closed-form solution against RK4 integration of `x'' = k(1-x) - 2ζ√k·x'`;
- each published duration against the spring's own ~0.1 % settling point (measured residual across
  all twelve scheme/kind/speed combinations: 0.00095–0.00104, against a 0.0015 tolerance);
- `MdSpatialSpringRunner`'s semi-implicit Euler integrator against that closed form — and, because
  a wrong-but-close integrator would still pass a fixed tolerance, against the **convergence order**
  as well: halving the step must halve the error;
- the C# `MdMotionTokens` against the XAML `Md.Sys.Motion.*` resources, two independent
  declarations that nothing in the build keeps in sync.

`MdSpatialSpringRunner.Advance(TimeSpan)` is stepped with an explicit clock, so none of this
depends on timers, wall-clock, or frame scheduling. L5 cannot flake.

---

## 3. The ratchet

`spec-snapshot/conformance-policy.json` has a `mode`:

```jsonc
"mode": "bootstrap"   // every divergence is reported; nothing fails
"mode": "enforce"     // non-waived, high-confidence divergences fail the build
```

The harness ships in `bootstrap`. That is intentional, and it is the only way a conformance suite
can be merged into a project that has never had one: the first run inventories the gap rather than
turning the tree red. Reports land in `artifacts/spec/` and in the job summary.

**To move the ratchet forward:**

1. Read `artifacts/spec/L1-tokens.md`, `L2-geometry.md`, `L3-visual.md`, `L5-motion-*.md`.
2. For each high-confidence gap, either fix the library or add a waiver with a real reason.
3. Flip `mode` to `enforce`.

Flipping back to `bootstrap` is a regression and should be challenged in review.

### Waivers

A waiver is a documented decision, not a mute button. `reason` is mandatory and is reproduced in
every generated report, so nobody has to diff the policy file to understand a divergence:

```jsonc
{
  "id": "combobox-menu-item-72",
  "layer": "L1_tokens",
  "keys": ["Md.Comp.MenuItem.Container.Height"],
  "reason": "Md.Comp.ComboBox.MenuItem.MinHeight is intentionally 72dp (two-line item with supporting text) …",
  "expires": null
}
```

---

## 4. What the first run found

The bootstrap inventory is real output, not a placeholder. From `scripts/lint-design-tokens.py`:

- **15 high-confidence token gaps.** Seven component families — `Chip`, `Card`, `Dialog`, `Divider`,
  `Menu`, `NavigationBar`/`Rail`/`Drawer`, `Switch` — have no `Md.Comp.*` tokens at all; their
  values are hard-coded inside the control themes. They render correctly today and cannot be
  retargeted by a consumer theme. *(Twelve of these are now closed — see 4.2.)*
- **3 medium-confidence divergences**, including `Md.Comp.Fab.Medium.Shape` at 24dp against an
  expected 20dp.
- **105 orphan tokens** that are declared and never referenced.
- **267 hard-coded geometry literals** inside `Themes/Controls/*.axaml` (recounted later across
  every package: the real first-run figure was 474 - see "The geometry metric measured the wrong tree").
- **0 errors** — the repository already has a clean baseline for literal colours, unresolved
  resource keys, and broken resource includes, so those three rules are armed as hard failures from
  day one and will catch the next regression.

The shape scale is also missing M3 Expressive's `LargeIncreased` (20dp) and `ExtraLargeIncreased`
(32dp) steps; that one is waived with an expiry because adding them is purely additive.

### 4.1 What the first run found in the harness itself

Two of the fourteen L2 "gaps" were defects in the measurement, not in the controls, and are worth
recording because both are easy to repeat:

- **The host panel was changing the answer.** `WrapPanel` arranges every child in a row at that
  row's height, so a tall neighbour inflated short controls and `MdChip` measured 66dp against a
  32dp oracle. The chip was correct all along. Pinning each case to `Top`/`Left` makes `Bounds` the
  control's own desired size.
- **The touch-target rule was looking in one place.** It inspected only the template's root panel,
  so `MdRadioButton` was reported as not hit-testable while its `PART_InteractiveArea` painted a
  correct 48x48 transparent surface one level down. It now looks for a painted surface anywhere in
  the control's visual subtree that actually covers the target.

A conformance layer is code, and its first run tests the layer as much as the subject.

### 4.2 Resolved

After acting on the first run, the layer counts moved as follows:

| Layer | Before | After |
| --- | --- | --- |
| L1 token conformance | 157 conforming, 14 gaps | **169 conforming, 2 gaps** |
| L2 rendered geometry | 105 conforming, 14 gaps | **114 conforming, 5 gaps** |
| L5 spring physics | 51 conforming, 0 gaps | 51 conforming, 0 gaps |
| L5 runtime motion | 15 conforming, 4 advisory | **18 conforming, 2 advisory** |

The remaining gaps are the ones listed in 4.3: every one of them is a deliberate open question
rather than an oversight.

- **Twelve of the fourteen L1 gaps are closed** by `Themes/Tokens/ContainmentTokens.axaml`. Every
  value involved already existed as a literal inside the matching control theme and agreed with the
  oracle, so this is pure extraction: rendering is unchanged by construction, and the numbers became
  addressable by a consumer theme and checkable by the oracle.
- **The three `Md.Sys.Motion.Standard.*Spatial` springs are now declared.** `MdMotionTokens` had
  carried them since motion shipped while the published surface described only the expressive half.
- **`MdSwitch`'s 48dp target is hit-testable.** Its 52x48 slot painted no background, so a tap near
  the track's edge fell through. This was the one genuine touch-target defect in the matrix.

### 4.3 Open, and why they are still open

Everything in the first round of open questions has now been answered. What is left:

| Finding | Status |
| --- | --- |
| `Md.Comp.Fab.Medium.Shape` 24dp vs 20dp, `Md.Comp.Fab.Large.IconSize` 32dp vs 36dp, `Md.Comp.Button.XSmall.Content.Padding` 16dp vs 12dp | **Medium confidence: the oracle is the weaker party**, and after the navigation episode that should be the default assumption for any entry transcribed by review. All three need mapping to a pinned upstream file before any rendering changes. They never fail a build. |
| `Md.Sys.Motion.*` keys are declarative | `MdMotion` resolves only `Md.Sys.Motion.Scheme` at runtime and takes every spring from the compiled constants, so overriding a spring key changes nothing. Recorded as a note by the L5 layer so the catalogue cannot be mistaken for a theming hook. Making motion genuinely themeable is a behavioural change, not a token change. |
| `Md.Comp.Switch.Handle.Unselected.Size` and the two `*Increased` shape steps are declared but unreferenced | The switch animates one 24dp handle under `scale(0.667)` rather than swapping two sizes, and nothing uses the new shape steps yet. The lint warnings are accurate and accepted. |
| 3 L3 goldens have no committed baseline | By design. Candidates are written to `artifacts/spec/baselines-new/`; a human reviews one and copies it into `Spec/baselines/` to arm it. The layer never adopts its own output. |

### 4.4 The ratchet is closed

Every layer now reports **zero high-confidence gaps**, so the policy has been moved from
`bootstrap` to `enforce`. From here a high-confidence divergence fails the build rather than
appearing in a report nobody reads.

| Layer | Checks | Conforming | Gaps | Advisory |
| --- | ---: | ---: | ---: | ---: |
| L1 token conformance | 178 | 172 | **0** | 5 |
| L2 rendered geometry | 120 | 119 | **0** | 1 |
| L3 visual goldens | 3 | 0 | **0** | 3 |
| L5 spring physics | 51 | 51 | **0** | 0 |
| L5 runtime motion | 20 | 18 | **0** | 2 |

Flipping the switch is safe precisely because the number is zero: nothing fails today, and the
next regression does. The remaining advisories are listed in 4.3 and none of them can fail a
build by construction.

### 4.5 Second round: conformed to the specification

These were open in 4.3 and have since been resolved rather than waived.

- **Navigation: the oracle was wrong and the library was right — twice.** Both nav entries were
  "corrected" towards the oracle and both corrections were wrong.
  `MdPhaseTwoToFourCompletionTests` failed immediately on the bar, and
  `material-components-android`'s own component docs settled the rail. M3 Expressive moved the
  collapsed rail **from 80dp to 96dp**, and the bar is **two** configurations rather than one
  number: 80dp with stacked items (`Baseline`) and 64dp with horizontal items (`Flexible`, the
  default). Both controls were already correct. What changed is the oracle: the rail entry now
  reads 96, and `Md.Comp.NavigationBar.Flexible.Container.Height` (64) was added beside the
  baseline entry so the pair is explicit.

  This is the most useful thing the exercise produced. Two entries marked `high` confidence —
  entitled to fail a build — were transcribed by review rather than mapped to a pinned upstream
  file, and both carried pre-Expressive values. The lesson is in §1: confidence has to track
  *provenance*, not plausibility. `bootstrap` mode and the existing test suite are what stopped a
  bad oracle from rewriting correct controls.
- **Input chip 50dp -> 32dp.** `Variant = Input` implies `:removable`, revealing `PART_RemoveButton`
  - an `MdIconButton` whose 48dp standalone target set the floor for the whole chip. The root grid
  in `MdIconButton`'s template now template-binds its minimum instead of hardcoding 48, so the
  ControlTheme still gives a standalone icon button its 48dp target while a host that embeds one in
  something smaller can lower it. Inside a chip the button collapses to the 18dp trailing icon M3
  specifies, and the chip body is the target.
- **48dp pointer targets for chips and the small FAB.** Material states the 32dp chip and the 40dp
  small FAB, and separately states a 48dp minimum target. Both are satisfied the way the
  specification resolves them: the visual container keeps its size and is centred inside a 48dp
  transparent region. Neither control looks different; both stop dropping taps near their edges.
- **`Md.Sys.Shape.Corner.LargeIncreased` (20dp) and `ExtraLargeIncreased` (32dp) added**, and the
  waiver that covered their absence retired. The only waiver left is the deliberate 72dp ComboBox
  menu item.

---

## 5. Running it

```bash
# Static only — no SDK, no network, finishes in well under a second.
python3 scripts/lint-design-tokens.py

# Full harness.
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release

# Just the conformance layers.
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release \
  --filter "FullyQualifiedName~Md3.Avalonia.HeadlessTests.Spec"

# Re-check the oracle against pinned upstream sources (needs network).
python3 scripts/sync-spec-tokens.py
```

Reports are written to `artifacts/spec/` (git-ignored, published as a CI artifact).

### Adding a golden image

L3 never adopts its own output as truth. On the first run for a new scene the candidate is written
to `artifacts/spec/baselines-new/<scene>.png` and the run reports — it does not fail. Review the
image, then copy it into `tests/Md3.Avalonia.HeadlessTests/Spec/baselines/` to arm the golden.

When a golden moves, the candidate **and** a magenta diff mask are written to the same place, so a
reviewer can see where it moved without flicking between two files.

Text rasterisation differs between Skia's platform back ends, so scenes containing glyphs are only
gated on `visual.baselineOs` (Linux, which is what CI runs). Text-free scenes are gated everywhere.

---

## 6. CI topology

| Job | Cost | Contents |
| --- | --- | --- |
| `static-checks` | seconds, no SDK | font verification, design-token lint, advisory upstream drift check |
| `build-and-test` | the bulk | one restore, one build graph, one test run, conformance reports in the job summary |

Changes made to shorten the critical path, none of which drop coverage:

- **Packaging removed from `ci.yml`.** `build-nuget.yml` already runs `scripts/build-nuget.sh` from
  a clean tree on push to `main`/`arena/**` and on every PR to `main`. CI was paying for a second
  full rebuild of all seven packaged projects to get an identical answer.
- **Multi-OS gallery builds removed.** `build-gallery.yml` covers Linux, Windows and macOS on the
  same triggers. The Linux Desktop head is still built in CI because it is nearly free once the
  shared graph is compiled, and it is what produces the inline compiler annotations.
- **One restore instead of three.** Everything downstream runs `--no-restore` / `--no-build`.
- **NuGet package cache**, keyed on `Directory.Packages.props` and the project files.
- **`concurrency: cancel-in-progress`.** A superseded run's answer is already irrelevant.
- **Preview bitmaps are opt-in.** Eleven call sites across eight test files used to encode a
  full-size PNG and write it to disk on every run, and nothing consumed the files. Every assertion
  those tests make still runs unconditionally; only the encode and the file write are now gated
  behind `MD3_WRITE_PREVIEWS=1`, which CI sets on `main` so the artefact is still published. The
  heaviest case, `Gallery_Shell_And_All_Documentation_Pages_Render`, was writing 43 full-window
  PNGs per run.
- **`scripts/build-nuget.sh --keep-intermediate`** lets a packaging check reuse an existing build
  instead of forcing a from-scratch rebuild. The default still wipes `bin/`/`obj/` first, because a
  release artifact must never pick up a stale intermediate.

---

## 7. Limits

Be precise about what a green run does **not** mean.

- **Headless is not a platform test.** Everything here runs on Avalonia's headless platform with
  Skia on a Linux CI runner. It says nothing about Android, Windows, macOS, or any real device.
  `claim_policy.headless_test_is_platform_test` is `false` for this reason.
- **No accessibility or RTL coverage.** Both are deliberately out of scope for this harness. An
  accessibility-tree snapshot layer and a FlowDirection matrix would be separate work; nothing here
  should be read as evidence about either.
- **The oracle is a transcription.** m3.material.io is mutable and client-rendered; the values in
  `tokens.json` were read on the retrieval date recorded in `manifest.json`. `confidence` marks how
  much weight each one carries, and `sync-spec-tokens.py` re-checks the 27 entries that have an
  unambiguous upstream counterpart. The remaining entries are verified by review, and the script
  reports them as such rather than pretending otherwise.
- **`bootstrap` mode fails nothing.** Until the policy is flipped to `enforce`, a green CI run means
  "the harness ran and wrote a report", not "the library conforms". The report is the deliverable.

## 8. Families with no specification counterpart

The coverage queue cannot reach zero by design. Material Web v0_192 states almost no layout tokens — across
its 55 component files the only space keys are `list-item-leading-space`, `list-item-trailing-space`,
`divider-leading-space` and `divider-trailing-space` — so every library token whose value is a padding, a
margin, an offset or a gap has no counterpart to be checked against, by construction, not because the mapping
is unfinished. Those keys stay in the queue because the queue means "nothing asserts this", which remains true.

Beyond layout, whole component families have no v0_192 token table at all, and the Expressive size ladder this
library implements is not stated by v0_192 (which ships one 40dp button and one extended-fab size). Rather than
invent pairs that would report a design decision as a defect, these families are recorded as unmodelled in
`spec-snapshot/component-tokens.json` (`mapping.excluded_by_design`) and are deliberately not paired:

- **IconButton** — v0_192 states the filled-icon-button 40dp box (Small.DefaultWidth/Height/IconSize are paired)
  and the icon-button state layer; the NarrowWidth/PressedShape/SquareShape/WideWidth and the
  Medium/Large/XLarge/XSmall ladder are this library's Expressive scale, unstated by v0_192.
- **Button** — v0_192 states one filled button (40dp height, corner-full shape, with-icon 18dp); the
  32/40/56/96/136 Expressive ladder, content padding, icon label space, and pressed/round/square shapes are
  this library's own scale.
- **ExtendedFab** — v0_192 states one extended-fab size (56dp / shape 16 / icon 24); ExtendedFab.Small is paired.
  ExtendedFab.Medium (80/28/16/26) and ExtendedFab.Large (96/32/20/28) are Expressive steps with no v0_192
  counterpart; there is no medium or large extended-fab in the specification.
- **Fab** — v0_192 states fab-primary-small 40dp, fab-primary 56dp and fab-primary-large 96dp; there is no
  medium FAB, so Fab.Medium (80/24/28) has no counterpart. Fab.Target.Size 48 has no counterpart either: v0_192
  states no target-size (hit-target) token for any component.
- **CheckBox, RadioButton, Chip** — their Target.* keys (48dp) share the same finding: v0_192 states no
  target-size token. Hit targets are a platform concern handled by Avalonia, not a Material specification token.
- **NavigationRail** — Container.Width is 96dp here where Material states 80dp; the difference is deliberate
  (this library's own docs promise 96dp), not a defect.
- **BottomAppBar** — Height 80dp has no counterpart: v0_192's bottom-app-bar states no height token.
- **FileSystemPicker, GridSplitter, Toolbar, Dropdown, ConnectedButtonGroup, ButtonGroup, SplitButton, FabMenu**
  — v0_192 ships no token table for any of these, so none of their keys can be paired.

Two design decisions behind the exclusions are recorded explicitly:

1. **Fab.Medium = 80dp / shape 24 / icon 28.** Material Web's single FAB is 56dp / 16 / 24; v0_192 adds only a
   40dp small and a 96dp large. The library's 80dp Medium is an Expressive step between them, not a v0_192 value,
   so Fab.Medium.* is excluded rather than forced against the wrong spec row.
2. **Hit targets (Fab.Target.Size 48, CheckBox.Target.Size, RadioButton.Target.Size, Chip.Target.Height).**
   Material Web v0_192 states no target-size token for any component. The 48dp hit target is a platform
   concern (pointer/touch hit-box sizing) that Avalonia handles, not a token the specification defines. All four
   are excluded with a single, shared reason.

### What would shrink the queue

Only adding a genuine pair — a library key matched to a Material Web token with an equal measured value — lowers
the queue. The Expressive ladders, the hit targets, the padding/margin families and the table-less components above
have no such token in v0_192, so they are expected to remain. The budget for
`component_tokens_without_a_specification_counterpart` is set to the measured queue (201) and is a ratchet: it may
only move down when a new, value-verified pair is added.
