# Grid splitter control research

Research date: 2026-10-26

`MdGridSplitter` follows the project's required sequence: inspect comparable Avalonia controls first,
redesign the public and visual seam around existing Material controls second, and review Flutter
split-view behavior last.

## 1. Avalonia controls studied first

- [Avalonia `GridSplitter` 12.1.2](https://github.com/AvaloniaUI/Avalonia/blob/12.1.2/src/Avalonia.Controls/GridSplitter.cs)
  is the behavioral base. It already has mature row/column direction inference, resize-pair selection,
  star/fixed definition handling, min/max constraints, drag quantization, preview adorners, keyboard
  arrows and Escape cancellation. Reusing that engine is safer than copying or replacing layout logic.
- Avalonia's Fluent and Simple themes were inspected. Both expose only a solid narrow bar and a solid
  preview rectangle. The Fluent theme reserves six device-independent pixels and is focusable, but
  it does not separate the hit target from the visible divider or provide a grip affordance.
- [Semi.Avalonia's `GridSplitter` theme](https://github.com/irihitech/Semi.Avalonia/blob/main/src/Semi.Avalonia/Controls/GridSplitter.axaml)
  improves the stock style by putting a narrow divider inside transparent padding. This validates a
  larger hit viewport, but the visuals remain a single state-dependent bar.
- [Royal Apps' opt-in GridSplitter behavior](https://github.com/royalapplications/royalapps-community-avalonia/blob/main/src/RoyalApps.Community.Avalonia.Common/Behaviors/GridSplitterBehavior.cs)
  demonstrates that double-tap equalization is useful in desktop pane layouts. Its handling of star
  definitions and min/max constraints informed the reset contract; no source is copied.

The selected implementation therefore subclasses Avalonia's built-in `GridSplitter`. Core resize,
preview, keyboard and cancellation semantics remain upstream behavior, while the Material package
adds tokens, a transparent interaction track, an orientation-aware visual divider and grip,
localized automation text, double-tap reset and a public `ResetToEqual()` command.

## 2. Independent Material redesign

The splitter has two deliberately different geometries:

- a 16 dp interaction lane that is easy to acquire with mouse, pen or touch without a ripple; and
- a 2 dp visible divider with an optional rounded 56 dp three-dot capsule.

The follow-up also inspected and packaged the proven
[`MaterialGridSplitter`](https://github.com/ilwren/MdExplorer/blob/arena/c665818d-mdexplorer/src/MdExplorer.UI/Controls/MaterialGridSplitter.cs)
prototype and its `AppStyles.axaml` theme from MdExplorer. Its 16 dp lane, long three-dot capsule,
hover/pressed growth, Home-key reset request, and host-owned default pane restoration replace the
first Gallery treatment. Md3.Avalonia retains its more general row/column orientation, motion scheme,
preview and `ResetToEqual()` APIs around those validated interaction and visual decisions.

The visual divider uses `OutlineVariant` at rest, `OnSurfaceVariant` on hover/focus, and `Primary`
while dragging. The grip follows the same state colors. Horizontal and vertical rendering is selected
from the same direction rules as Avalonia's resize engine. Preview mode uses a translucent primary
track. All geometry and state colors are tokenized under `Md.Comp.GridSplitter.*`.

`ResetOnDoubleTapped` defaults to true. Reset applies only when both controlled definitions are star
sized, preserves all other star tracks' current proportions, obeys both definitions' minimum and
maximum constraints, and raises `SplitReset` only after a successful change. Set it to false when an
application assigns another double-tap gesture. The inherited `ResizeDirection`, `ResizeBehavior`,
`ShowsPreview`, `DragIncrement`, `KeyboardIncrement` and `PreviewContent` remain available.

## 3. Flutter equivalent studied last

[`multi_split_view`](https://github.com/caduandrade/multi_split_view) was the primary Flutter
reference. Its `MultiSplitViewThemeData` separates divider thickness from an additional handle
buffer, its divider builder receives highlighted and dragging states, and its default painters support
rounded groove affordances. `DividerWidget` drives those painter values through an animation
controller; the stock painters tween color, groove thickness and groove length over 250 ms rather
than animating pane geometry. Its horizontal/vertical cursor switching, drag lifecycle callbacks,
configurable painter and controller-managed proportions reinforce the selected separation between
Avalonia's resize engine and the Material visual/interaction shell.

The Gallery follow-up adopts that distinction: pane definitions update directly on every native drag
delta when `ShowsPreview="False"`, while only the divider's state layer, color and grip dimensions
animate. The visual transitions use the inherited `MdMotion` scheme and disappear under the `None`
scheme; layout itself is never eased behind the pointer.

The first `MdGridSplitter` intentionally stays a single-grid divider rather than introducing a second
pane-layout container. Applications that need multi-pane persistence can bind Avalonia row/column
definitions or compose multiple splitters; the control remains compatible with the normal `Grid` API.
