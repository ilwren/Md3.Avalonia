# Borderless window W0–W4 implementation plan

Status (2026-10-03): **W0–W2 complete; Windows W3 now preserves native DWM state-animation styles and exposes the native system menu; Snap Layout/DPI/backdrop and W4 real-platform sign-off remain open.**

The custom-chrome feature is in the core `Md3.Avalonia11` package. It introduces no dependency on the optional Icons or Extra packages and uses Avalonia 11 window APIs.

## W0 — contracts and platform boundary (complete)

- `MdBorderlessWindowOptions` carries resize, extension and title-height options.
- `IMdWindowPlatformAdapter` isolates apply, move, resize and system-menu operations.
- `MdWindowCapabilities` exposes feature support without platform checks in view models.
- `MdWindowPlatformAdapterResolver` selects Windows, macOS, Linux, Android or unknown adapters.
- Host applications may inject a specialized adapter through `MdBorderlessWindow.PlatformAdapter`.

## W1 — self-contained Material custom chrome (managed implementation complete)

- `MdBorderlessWindow` now selects its own `ControlTheme`; applications no longer have to assemble a title bar, frame outline, corner clipping and resize grip manually.
- The default template consumes `TemplateSettings.TitleBarHeight` and `TemplateSettings.ContentMargin` and binds the default title bar to `Title` and the window caption visibility properties.
- When no native frame is requested, the template owns all four rounded corners and switches its margin, outline and corner radius for maximized state. When a native frame is preserved, the Material outer outline/clipping is disabled so there is no double border.
- Eight edge/corner resize regions are included by default and disappear when maximized, when resizing is disabled, or when custom chrome is disabled.
- `MdWindowTitleBar` tracks later title-height changes rather than reading the height only once at attachment.
- Caption actions expose minimize, maximize/restore and close; maximize changes to a restore glyph and automation name while maximized.
- `MinimizeCommand`, `ToggleMaximizeCommand`, `CloseCommand`, matching request events and direct methods continue to support MVVM and imperative operation.
- Gallery no longer supplies a private frame/titlebar workaround; the secondary-window sample uses the default `MdBorderlessWindow` template.

This phase is verified structurally by headless tests. It does not by itself prove compositor shadows, OS corner rendering or native window behavior.

## W2 — Avalonia 11 extended client area and generic fallback (complete)

- The default title bar and explicit `MdWindowDragRegion` use the platform adapter's `BeginMoveDrag` fallback.
- Caption buttons expose managed commands and distinct automation names for minimize, maximize/restore and close.
- All eight resize regions carry their corresponding `WindowEdge` values and call `BeginResizeDrag`; Avalonia 12-only `WindowDecorationsElementRole` values are not used.
- `MdAvaloniaWindowPlatformAdapter` retains `BeginMoveDrag` and `BeginResizeDrag` as the generic fallback.
- `MdAndroidWindowPlatformAdapter` reports no desktop window capabilities and performs no desktop chrome calls.

## W3 — native platform enhancement (Windows state path complete; broader sign-off open)

Source comparison was repeated against FluentAvalonia commit `e6def96e6281b79cb859e7984c7807e911e90878`:

- `FAAppWindow` overrides `ExtendClientAreaToDecorationsHint` to `true` on Windows and retains the system frame; its Windows template explicitly avoids drawing a second frame.
- `Win32WindowManager` subclasses the HWND procedure for right-click/Alt+Space system-menu behavior and posts the selected command as `WM_SYSCOMMAND`; it does not fake maximize/restore with an Avalonia animation.
- FluentAvalonia leaves window-state changes on Avalonia's native backend. `SystemDecorations.Full` retains the caption style bits used by DWM, while `BorderOnly` is the portable fallback for other desktop platforms.

The current package baseline is Avalonia `11.3.22`. Native state transitions still require real Windows validation; headless template tests are not evidence of DWM behavior.

Accordingly, `MdWindowsWindowPlatformAdapter` now uses `SystemDecorations.Full` when `PreserveNativeBorder=true`, extends the client area, leaves maximize/restore on `Window.WindowState`, and uses `GetSystemMenu`/`TrackPopupMenu`/`PostMessage(WM_SYSCOMMAND)` for the title-bar context menu. No custom maximize animation is present. The Material title bar is placed with `ExtendClientAreaToDecorationsHint` and `ExtendClientAreaTitleBarHeightHint`; no Avalonia 12-only `WindowDrawnDecorations` theme or decoration-property API is used.

The following work still requires real Windows or platform-specific evidence:

- verified Windows 11 Snap Layout behavior for the maximize caption role and Alt+Space/right-click menu enable-state matrix;
- DWM dark mode, border color, corner preference and optional backdrop integration;
- physical/logical DPI conversion, monitor work area and negative-coordinate handling;
- macOS traffic-light/safe-area/full-screen behavior;
- Linux X11/Wayland/window-manager-specific fallback behavior.

Until those tests exist, the implementation is described as matching FluentAvalonia's native state/system-command path, not as complete cross-platform native parity.

## W4 — Gallery, automated verification and real-platform sign-off

Completed automated work:

- `BorderlessWindowGalleryPage` contains a themed preview, capability diagnostics, usage samples and a real secondary-window launcher.
- `MdBorderlessWindowTemplateTests` verifies the self-contained template, content margin, title-height updates, eight grip `WindowEdge` values, caption automation names and restore state.
- Adapter/API regression tests remain in the full headless suite.
- Release build and full headless suite remain required CI gates; exact totals are recorded by the workflow rather than copied into this plan.
- Core, Icons, Icons.Lite and Extra package verification is part of the NuGet gate.

Still required before a stable release:

| Platform | Drag | Resize edges | Min/max/close | Snap/system menu | 100/150/200% scale | Light/Dark | Native shadow/corners |
|---|---:|---:|---:|---:|---:|---:|---:|
| Windows 10/11 | required | required | required | required | required | required | required |
| macOS | required | required | required | platform equivalent | Retina + scaled | required | required |
| Linux X11/Wayland | required | required | required | WM-dependent | required | required | WM-dependent |
| Android | no-op safety | n/a | n/a | n/a | required | required | n/a |

Headless verification covers managed template structure, bindings, roles, commands and state. It cannot validate a native system menu, Snap Layout, compositor shadow, DPI behavior or actual OS hit testing.
