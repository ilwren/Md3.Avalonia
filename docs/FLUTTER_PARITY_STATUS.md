# Flutter behavior boundaries

[English](FLUTTER_PARITY_STATUS.md) | [简体中文](FLUTTER_PARITY_STATUS.zh-CN.md)

Last reviewed: 2026-10-22  
Comparison target: Flutter stable public API documentation (see `spec-snapshot/manifest.json`).

Md3.Avalonia uses Avalonia controls, properties, and lifecycles. Similar type names do not imply identical Flutter APIs, rendering, routing, platform adaptation, or accessibility. `Md3.Avalonia.Extra` is an optional extension package, not part of the official Material component catalog.

| API family | Status | Supported behavior | Known boundary |
|---|---|---|---|
| `MdHero` | Implemented subset | Snapshot overlay, source/destination interpolation, cancellation, Reduced Motion fallback | Not certified for multi-window, every clip/elevation case, or Flutter pixel parity |
| `MdReorderableList` | Implemented subset | Pointer reorder, auto-scroll, keyboard alternative, Reduced Motion | No claim of every Flutter delegate/drag decorator API |
| `MdDismissible` | Implemented subset | Axis-aware pointer drag, velocity-preserving spring settle, logical-direction backgrounds, confirmation and collapse lifecycle | Keyboard action and announcement coverage remains partial |
| `MdFocusTraversalGroup` | Implemented subset | Visual-order, TabIndex and geometric reading-order traversal; cycle and skip modes | It maps to Avalonia focus primitives rather than accepting Flutter `FocusTraversalPolicy` objects |
| `MdKeyboardAvoidingHost` | Implemented subset | Subscribes to Avalonia `TopLevel.InputPane`, applies occluded height and scrolls focused content | Floating keyboards that report no occlusion intentionally produce zero avoidance |
| `MdSimpleDialog` | Partial | Scrim, Escape, focus containment/restoration, selectable list and cancel action | It must be placed in a full-size overlay host; it is not a Navigator route or a clone of `showDialog<T>` |
| `MdPaginatedDataTable` | API shell | Basic paging over list rows | No Flutter columns/cells/header-sort data model; do not describe it as DataTable parity |
| `MdAdaptiveSwitch` | API shell | Material switch behavior with platform-size selection | Cupertino rendering and animation are not implemented |
| `MdAdaptiveProgressIndicator` | API shell | Material progress semantics with platform-tuned dimensions | Cupertino activity-indicator rendering is not implemented |
| `MdAboutDialog` | API shell | About metadata surface and license callback | No built-in dialog route or complete Flutter license flow |
| `MdContainerTransform`, `MdAnimatedVisibility` (Extra) | Implemented subset | Bounds and shape morph on Material's 30/70 cross-fade; clip-based enter/exit that never reflows content | Elevation morph and interruption velocity are not modelled; a reversal restarts rather than retargets |
| `MdSharedAxis`, `MdFadeThrough` (Extra) | Partial | Phased entrance with slide/scale, retained exit for templated and view-model content | A `Control` assigned to `Content` cannot be shown by two presenters at once, so its exit phase is skipped and only the entrance animates |
| `MdStepper` | Implemented subset | Vertical and horizontal (`Orientation`) step flows; per-step `State` styling, index navigation and `ActiveStepChanged` | No Flutter `controlsStep`/`currentStep` animation-timing parity; horizontal step content placement is simplified versus Flutter's |
| `MdPagination` | No Flutter counterpart | Numbered and dot display modes, previous/next paging over `NumberOfPages`, two-way `SelectedPageIndex` | Flutter has no first-party pagination widget; the control is based on Avalonia `PipsPager`, not a Flutter port |
| `MdMaskedTextBox` | No Flutter counterpart | Input constrained to a mask with auto-filled literals, `PromptChar`, `HidePromptOnLeave`, and the `MdTextBox` field-state surface | Flutter masks input with `TextInputFormatter` (no first-party widget); the control extends Avalonia `MaskedTextBox` |
| `MdCommitGraph` | No Flutter counterpart | Lane-based commit history with branch/merge connectors, head marker, per-lane Material colors, `HeadId` and read-only `LaneCount` | No first-party Flutter commit graph; layout is a custom GitHub-style lane algorithm, items are plain `MdCommitNode` models supplied by the host |
| `MdCarousel` | Mapped to `PageView` | Weighted, hero, center-aligned and uncontained carousel variants with controller and autoplay | Not certified for Flutter `PageView` physics/scroll-position parity |


Types marked `[MdExperimental]` are not evidence of completed Flutter parity. The project does not publish an overall parity percentage; every compatibility claim must be limited to the APIs, tested cases, and boundaries listed here.
