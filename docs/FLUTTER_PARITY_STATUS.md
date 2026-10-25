# Flutter behavior boundaries

[English](FLUTTER_PARITY_STATUS.md) | [简体中文](FLUTTER_PARITY_STATUS.zh-CN.md)

Md3.Avalonia uses Avalonia controls, properties, and lifecycles. Similar type names do not imply identical Flutter APIs, rendering, routing, platform adaptation, or accessibility. `Md3.Avalonia.Extra` is an optional extension package, not part of the official Material component catalog.

The table describes the limited scope currently exposed by the public API. Verify behavior against the control source and tests.

| API | Status | Implemented behavior and limits |
|---|---|---|
| `MdHero` | Implemented subset | Provides snapshot transitions between source and destination and supports cancellation; does not cover every Flutter clipping, shadow, or multi-window case. |
| `MdReorderableList` | Implemented subset | Supports drag reordering, auto-scroll, and keyboard alternatives; it does not reproduce Flutter's complete delegate API. |
| `MdDismissible` | Implemented subset | Supports gesture thresholds, confirmation, velocity, and dismissal lifecycle; keyboard operation and notification coverage remain limited. |
| `MdFocusTraversalGroup` | Implemented subset | Provides visual order, `TabIndex`, and geometric reading-order strategies using Avalonia focus primitives; it does not accept Flutter strategy objects. |
| `MdKeyboardAvoidingHost` | Implemented subset | Uses the height reported by Avalonia `TopLevel.InputPane` to add bottom padding and can scroll the focused control into view. It does not compute rectangle overlap; a floating keyboard that reports no occlusion causes no inset. |
| `MdSimpleDialog` | Partial implementation | Provides a Material-style option list and dismissal interaction; it is not a Flutter Navigator route or a replacement for `showDialog<T>`. |
| `MdPaginatedDataTable` | API subset | Provides basic paging; it does not include Flutter `DataTable`'s complete column, cell, and sorting model. |
| `MdAdaptiveSwitch` | API subset | The `:cupertino` state adjusts minimum size while the other states reuse `MdSwitch`; it is not a complete visual or behavioral match for Flutter Cupertino switch. |
| `MdAdaptiveProgressIndicator` | API subset | Reuses the Material circular progress control; the Cupertino state adjusts stroke and track only and does not implement the Cupertino activity indicator. |
| `MdAboutDialog` | API subset | Displays application metadata and raises an event to request license content; it does not include Flutter routing or the full license flow. |
| `MdContainerTransform`, `MdAnimatedVisibility` (Extra) | Implemented subset | Provide container or visibility transitions; they do not match Flutter's complete shared-element or interruptible-velocity models. |
| `MdSharedAxis`, `MdFadeThrough` (Extra) | Partial implementation | Switch content using a retained presenter. When the old content is a `Control`, both contents cannot be presented simultaneously, so its exit/overlap phase is skipped while the new content still enters. |

Types marked `[MdExperimental]` are not evidence of completed Flutter parity. The project does not publish an overall parity percentage; every compatibility claim must be limited to the APIs, tested cases, and boundaries listed here.
