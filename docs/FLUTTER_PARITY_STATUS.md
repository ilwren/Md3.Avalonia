# Flutter parity status

Last reviewed: 2026-10-02  
Comparison target: Flutter stable public API documentation (see `spec-snapshot/manifest.json`).

Md3.Avalonia is Avalonia-native. A similar type name does **not** imply complete Flutter API,
rendering, route, platform-adaptation, accessibility, or state-machine parity. This status table is
normative for documentation and release notes.

| API family | Status | Supported behavior | Known boundary |
|---|---|---|---|
| `MdHero` | Implemented subset | Snapshot overlay, source/destination interpolation, cancellation, Reduced Motion fallback | Not certified for multi-window, every clip/elevation case, or Flutter pixel parity |
| `MdReorderableList` | Implemented subset | Pointer reorder, auto-scroll, keyboard alternative, Reduced Motion | No claim of every Flutter delegate/drag decorator API |
| `MdDismissible` | Implemented subset | Gesture threshold, confirmation, velocity and collapse lifecycle | Keyboard action and announcement coverage remains partial |
| `MdFocusTraversalGroup` | Implemented subset | Visual-order, TabIndex and geometric reading-order traversal; cycle and skip modes | It maps to Avalonia focus primitives rather than accepting Flutter `FocusTraversalPolicy` objects |
| `MdKeyboardAvoidingHost` | Implemented subset | Subscribes to Avalonia `TopLevel.InputPane`, applies occluded height and scrolls focused content | Floating keyboards that report no occlusion intentionally produce zero avoidance |
| `MdSimpleDialog` | Partial | Scrim, Escape, focus containment/restoration, selectable list and cancel action | It must be placed in a full-size overlay host; it is not a Navigator route or a clone of `showDialog<T>` |
| `MdPaginatedDataTable` | API shell | Basic paging over list rows | No Flutter columns/cells/header-sort data model; do not describe it as DataTable parity |
| `MdAdaptiveSwitch` | API shell | Material switch behavior with platform-size selection | Cupertino rendering and animation are not implemented |
| `MdAdaptiveProgressIndicator` | API shell | Material progress semantics with platform-tuned dimensions | Cupertino activity-indicator rendering is not implemented |
| `MdAboutDialog` | API shell | About metadata surface and license callback | No built-in dialog route or complete Flutter license flow |
| `MdContainerTransform`, `MdAnimatedVisibility` (Extra) | Implemented subset | Bounds and shape morph on Material's 30/70 cross-fade; clip-based enter/exit that never reflows content | Elevation morph and interruption velocity are not modelled; a reversal restarts rather than retargets |
| `MdSharedAxis`, `MdFadeThrough` (Extra) | Partial | Phased entrance with slide/scale, retained exit for templated and view-model content | A `Control` assigned to `Content` cannot be shown by two presenters at once, so its exit phase is skipped and only the entrance animates |

## Release policy

- `Implemented subset` may be documented only with its explicit supported behavior and boundary.
- `Partial` and `API shell` must not be counted as completed Flutter parity.
- `Experimental` APIs must not be included in compliance totals.
- Behavioral parity claims require tests against a named Flutter stable version and source commit.
