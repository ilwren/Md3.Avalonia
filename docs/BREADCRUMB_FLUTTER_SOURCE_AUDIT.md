# Breadcrumb Flutter source audit

Audit date: 2026-10-03

## Frozen reference

The implementation was checked against the mature Flutter package [`flutter_breadcrumb`](https://pub.dev/packages/flutter_breadcrumb) 1.0.1 (verified publisher, 130 likes, 311k reported downloads at audit time), repository commit:

```text
payam-zahedi/flutter_breadcrumb
05fe711a306428c8487ef512df0e02a9d2813f04
```

All package source files were reviewed:

- `lib/src/breadcrumb.dart`
- `lib/src/breadcrumb_item.dart`
- `lib/src/breadcrumb_overflow.dart`
- `lib/src/breadcrumb_widget.dart`
- public barrel, example screens, README/API behavior and the (empty) test file

No Dart code or package assets are included in Md3.Avalonia.

## Behavior extracted

- Breadcrumb content and dividers are separate concepts; the trailing divider is disabled by default but configurable.
- Overflow is a strategy rather than an accidental side effect. The package supplies `WrapOverflow` and `ScrollableOverflow` and permits custom strategies.
- Wrap exposes direction, spacing, line spacing and alignment. Scroll keeps one axis, accepts reverse/padding/controller/physics, and uses native scrolling.
- Each item is independently interactive and styleable. An item without a callback is treated as non-interactive.
- The implementation inserts a divider between items and removes the final divider unless `keepLastDivider=true`.

The source also contains a defect in `ScrollableOverflow.widgetItems`: its no-divider fallback checks `items.isEmpty` rather than `widgetItems.isEmpty`. Md3.Avalonia does not reproduce that bug.

## Avalonia decisions

`MdBreadcrumb` retains its richer MVVM model, commands, URI launcher, automation text, current-location state, keyboard support and explicit ellipsis expansion. It now adds:

- `MdBreadcrumbOverflowBehavior.Wrap` — wrapped `WrapPanel` layout;
- `MdBreadcrumbOverflowBehavior.Scroll` — a one-line horizontal panel in a native `ScrollViewer`;
- `MdBreadcrumbOverflowBehavior.Collapse` — one line with configurable leading/trailing retention and an invokable ellipsis;
- `ShowTrailingSeparator`, default `false`, corresponding to the audited package's `keepLastDivider` behavior;
- transient invocation: a clicked ancestor is not retained as a list selection, so it cannot remain highlighted beside the current location.

Material has no first-party breadcrumb component specification. Visual colors, state layers, 48-DIP interaction height, type and shape therefore continue to use this repository's Material tokens, while overflow/interactivity follow the frozen Flutter ecosystem behavior above.
