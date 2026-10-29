# Timeline and slidable research — 2026-10-29

This note records the required clean-room research pass completed before the October 2026 `MdTimeline` and `MdSlidableItem` refinement. It documents behavior and public concepts only; no third-party source was copied.

## 1. Similar Avalonia controls

### Timeline

Reference: [`irihitech/Ursa.Avalonia`](https://github.com/irihitech/Ursa.Avalonia/tree/main/src/Ursa/Controls/Timeline), including its `Timeline`, item container, panel, and Semi theme.

Useful findings:

- keep item state and first/last position on the item container so themes can terminate connectors reliably;
- preserve an item template/custom marker path;
- use one ordered item model and derive connector state from collection position.

Ursa's layouts are primarily vertical, so its panel was not used as the horizontal solution.

### Swipe actions

Reference: [`AvaloniaUI/Avalonia.Labs` SwipeView](https://github.com/AvaloniaUI/Avalonia.Labs/tree/main/src/Avalonia.Labs.Controls/SwipeView) and its test suite.

Useful findings:

- distinguish gesture direction before capture;
- expose programmatic state and command paths in addition to pointer input;
- combine thresholds with release direction/velocity;
- test disabled directions, capture loss, and settle behavior.

SwipeView does not provide Flutter-style multi-action pane motions, so it served as an Avalonia input/lifecycle reference rather than the visual design.

## 2. Original Md3.Avalonia design review

### `MdTimeline`

The control already exposed `Orientation`, but the horizontal template was not demonstrated and its per-item margins left visible gaps between connector segments. The active state had only a different dot color. The item `Icon` was also present in the API without a rendered marker path.

Refinement decisions:

- reserve the same 32 px marker slot on either axis;
- make before/after connector halves reach each item boundary, while spacing only the content;
- keep first/last termination in presenter pseudo-classes;
- render active `Auto` indicators with an outer `MdCircularProgressIndicator`;
- allow `None` or forced `ProgressRing` through `MdTimelineIndicatorEffect`;
- rely on `MdCircularProgressIndicator` motion-scheme behavior: Standard/Expressive spins, while Reduced/None retains a static, visible arc.

### `MdSlidableItem`

The original implementation translated the foreground over fixed layers, but its threshold was calculated from the full row width rather than the action-pane extent. Start/end layers stayed physically left/right in RTL, vertical movement could begin a horizontal drag, absent panes could reveal blank color, and action feedback happened only after settle.

Refinement decisions:

- keep `Offset` logical (positive start, negative end) and expose `VisualOffset` for physical RTL translation;
- mirror pane alignment and keyboard direction in RTL;
- lock horizontally only after movement exceeds vertical movement;
- calculate `OpenThreshold` against `ActionExtent`;
- use release velocity to distinguish opening from closing flings;
- make exposed action surfaces hover/press targets and close after command by default;
- close the previous open item in an explicit `GroupName`;
- keep release transitions motion-scheme-aware and disable them during direct manipulation.

## 3. Flutter equivalents

### Timeline Tile

Reference: [`JHBitencourt/timeline_tile`](https://github.com/JHBitencourt/timeline_tile).

Adopted concepts:

- one axis model for horizontal and vertical orientation;
- independent before/after line segments;
- first/last line termination;
- a replaceable marker area and optional gap around the indicator.

The Avalonia API keeps its existing ordered `ItemsControl` model instead of reproducing Flutter widget composition.

### flutter_slidable

Reference: [`letsar/flutter_slidable`](https://github.com/letsar/flutter_slidable), especially `ActionPane`, `SlidableController`, `BehindMotion`, `StretchMotion`, `ScrollMotion`, and `DrawerMotion`.

Adopted concepts:

- semantic start/end panes;
- pane-relative threshold and velocity-aware settle;
- fixed-behind, stretch, scroll, and drawer motion choices;
- direct drag progress with animated release;
- command auto-close and group coordination.

Avalonia implementation notes:

- `MdSlidableActionPaneMotion` provides `Behind`, `Stretch`, `Scroll`, and `Drawer` without importing Flutter code;
- the foreground remains a normal Avalonia content surface, and action content remains host-supplied;
- dismiss-by-overdrag remains command-driven through the existing `DismissThreshold` contract;
- nested-scroll and touch-velocity quality still require Android/iOS platform validation beyond Headless tests.
