# Borderless window W0–W4 implementation plan

Status (2026-09-26): **W0 complete; W1 managed template complete and headless-tested; W2 Avalonia decoration roles complete; native platform enhancement and W4 real-platform sign-off remain open.**

The custom-chrome feature is in the core `Md3.Avalonia` package. It introduces no dependency on the optional Icons or Ecosystem packages and uses Avalonia 12 window APIs.

## W0 — contracts and platform boundary (complete)

- `MdBorderlessWindowOptions` carries resize, extension and title-height options.
- `IMdWindowPlatformAdapter` isolates apply, move, resize and system-menu operations.
- `MdWindowCapabilities` exposes feature support without platform checks in view models.
- `MdWindowPlatformAdapterResolver` selects Windows, macOS, Linux, Android or unknown adapters.
- Host applications may inject a specialized adapter through `MdBorderlessWindow.PlatformAdapter`.

## W1 — self-contained Material custom chrome (managed implementation complete)

- `MdBorderlessWindow` now selects its own `ControlTheme`; applications no longer have to assemble a title bar, frame outline, corner clipping and resize grip manually.
- The default template consumes `TemplateSettings.TitleBarHeight` and `TemplateSettings.ContentMargin` and binds the default title bar to `Title` and the window caption visibility properties.
- The template owns all four rounded corners and switches its margin, outline and corner radius for maximized state.
- Eight edge/corner resize regions are included by default and disappear when maximized, when resizing is disabled, or when custom chrome is disabled.
- `MdWindowTitleBar` tracks later title-height changes rather than reading the height only once at attachment.
- Caption actions expose minimize, maximize/restore and close; maximize changes to a restore glyph and automation name while maximized.
- `MinimizeCommand`, `ToggleMaximizeCommand`, `CloseCommand`, matching request events and direct methods continue to support MVVM and imperative operation.
- Gallery no longer supplies a private frame/titlebar workaround; the secondary-window sample uses the default `MdBorderlessWindow` template.

This phase is verified structurally by headless tests. It does not by itself prove compositor shadows, OS corner rendering or native window behavior.

## W2 — Avalonia 12 decoration roles and generic fallback (complete)

- The default title bar and explicit `MdWindowDragRegion` carry `WindowDecorationsElementRole.TitleBar`.
- Caption buttons carry native MinimizeButton, MaximizeButton and CloseButton roles.
- All eight resize regions carry the corresponding ResizeN/S/E/W/NE/NW/SE/SW roles.
- `MdAvaloniaWindowPlatformAdapter` retains `BeginMoveDrag` and `BeginResizeDrag` as the generic fallback.
- `MdAndroidWindowPlatformAdapter` reports no desktop window capabilities and performs no desktop chrome calls.

## W3 — native platform enhancement (not complete)

The named Windows/macOS/Linux adapters are still extension points over the generic Avalonia fallback. The following work remains:

- Windows native system menu, Alt+Space and title-bar right-click menu state;
- verified Windows 11 Snap Layout behavior for the maximize caption role;
- DWM dark mode, border color, corner preference and optional backdrop integration;
- physical/logical DPI conversion, monitor work area and negative-coordinate handling;
- macOS traffic-light/safe-area/full-screen behavior;
- Linux X11/Wayland/window-manager-specific fallback behavior.

Until those implementations and tests exist, the named adapters must not be described as FluentAvalonia-equivalent native integrations.

## W4 — Gallery, automated verification and real-platform sign-off

Completed automated work:

- `BorderlessWindowGalleryPage` contains a themed preview, capability diagnostics, usage samples and a real secondary-window launcher.
- `MdBorderlessWindowTemplateTests` verifies the self-contained template, content margin, title-height updates, eight decoration roles, caption roles and restore state.
- Adapter/API regression tests remain in the full headless suite.
- Release build passes with zero warnings/errors.
- Full headless suite passes: **178/178**.
- Core, Icons and Ecosystem packages pack successfully.

Still required before a stable release:

| Platform | Drag | Resize edges | Min/max/close | Snap/system menu | 100/150/200% scale | Light/Dark | Native shadow/corners |
|---|---:|---:|---:|---:|---:|---:|---:|
| Windows 10/11 | required | required | required | required | required | required | required |
| macOS | required | required | required | platform equivalent | Retina + scaled | required | required |
| Linux X11/Wayland | required | required | required | WM-dependent | required | required | WM-dependent |
| Android | no-op safety | n/a | n/a | n/a | required | required | n/a |

Headless verification covers managed template structure, bindings, roles, commands and state. It cannot validate a native system menu, Snap Layout, compositor shadow, DPI behavior or actual OS hit testing.
