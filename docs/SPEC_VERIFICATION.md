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
| **L1** token conformance | `Spec/MdTokenConformanceTests.cs` | headless session | Values the *resource system actually hands a control* match the oracle, through the real 58-dictionary merge, in both theme variants. |
| **L2** geometry & targets | `Spec/MdGeometryConformanceTests.cs` | headless session | Arranged bounds and corner radii of template parts match the oracle, and every interactive control offers a 48×48dp hit-testable target. |
| **L3** visual goldens | `Spec/MdVisualGoldenTests.cs` | headless session | Pixels did not change unintentionally — the only layer that catches regressions nobody predicted. |
| **L5** motion | `Spec/MdMotionConformanceTests.cs` | partly none, partly headless | Spring physics match an independent integration; published durations really are the settling points they claim to be; the runtime integrator and the closed form agree. |

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
- **267 hard-coded geometry literals** inside `Themes/Controls/*.axaml`.
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
| `Md.Comp.Fab.Medium.Shape` 24dp vs 20dp, `Md.Comp.Fab.Large.IconSize` 32dp vs 36dp, `Md.Comp.Button.XSmall.Content.Padding` 16dp vs 12dp | **Medium confidence: the oracle is the weaker party.** These three entries were transcribed by review rather than mapped to a pinned upstream file, so the published value needs re-verifying before any rendering changes. They never fail a build. |
| `Md.Sys.Motion.*` keys are declarative | `MdMotion` resolves only `Md.Sys.Motion.Scheme` at runtime and takes every spring from the compiled constants, so overriding a spring key changes nothing. Recorded as a note by the L5 layer so the catalogue cannot be mistaken for a theming hook. Making motion genuinely themeable is a behavioural change, not a token change. |
| `Md.Comp.Switch.Handle.Unselected.Size` and the two `*Increased` shape steps are declared but unreferenced | The switch animates one 24dp handle under `scale(0.667)` rather than swapping two sizes, and nothing uses the new shape steps yet. The lint warnings are accurate and accepted. |
| 3 L3 goldens have no committed baseline | By design. Candidates are written to `artifacts/spec/baselines-new/`; a human reviews one and copies it into `Spec/baselines/` to arm it. The layer never adopts its own output. |

### 4.4 Second round: conformed to the specification

These were open in 4.3 and have since been resolved rather than waived.

- **Navigation bar 64dp -> 80dp, navigation rail 96dp -> 80dp.** Both now M3's published values. The
  rail is the more interesting one: its own item draws a 56dp indicator inside 12dp of padding per
  side, which comes to exactly 80, so the control had been carrying 16dp of slack that nothing in
  its own template asked for. Verified that no `ComboBox` sits inside a rail or bar, so the
  containment problem that motivated the original sizes is not affected.
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
  full rebuild of all four packaged projects to get an identical answer.
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
