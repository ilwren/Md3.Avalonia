# Material component repeat-defect checklist

Run this checklist after **every new component or component-template change**. A component is not complete until the applicable items are recorded in its pull request/release note and automated regressions are added for fixed defects.

## 1. Package and API boundaries

- [ ] The type is an independent `Md*` control; it does not globally restyle an Avalonia control.
- [ ] `Md3.Avalonia` does not reference `Md3.Avalonia.Icons` or any concrete icon font.
- [ ] Icon slots accept host objects; built-in glyph defaults use `Md.Icon.*` dynamic resources and the `Md.Sys.Typeface.Symbols.Rounded` token.
- [ ] Missing icon/font packages leave icon slots visually empty without startup or XAML-load failure.
- [ ] Ecosystem controls reference `Md3.Avalonia`, never the reverse, and do not require the Icons package.
- [ ] Bindable properties use correct one-way/two-way defaults; commands, events, and direct methods all work.
- [ ] Public APIs avoid desktop-only/native types and construct safely on Android.
- [ ] Async/provider APIs cancel superseded and detached work, expose loading/empty/error/completed states, and never retain a concrete network, chart, editor, AI, media, or database provider.
- [ ] Density is inherited and opt-in; compact density never silently shrinks required touch targets in Touch mode.

## 2. Surface, shape, and state layer

- [ ] Light, Dark, high-contrast, and a non-purple seed color use the correct surface/on-surface pair.
- [ ] Every corner is rendered at rest, on hover, while pressed, while selected, and after popup/open-state changes.
- [ ] State/ripple layers cover the **entire interactive container**, including padding, rows, connected segments, and rounded icon areas.
- [ ] Layers are clipped to the same corner radius as their container; no square overflow or one-pixel gaps appear.
- [ ] Outlined variants stay transparent in Dark mode; floating labels are vertically centered on the outline.
- [ ] Primary/tonal action foregrounds meet contrast requirements in both themes.
- [ ] Disabled, error, selected, focused, hovered, and pressed states remain distinct.

## 3. Layout and responsive behavior

- [ ] Content remains usable at 360 dp compact width, medium width, expanded desktop width, and 200% scaling.
- [ ] Touch targets are at least 48×48 dp unless the compact desktop variant is explicitly documented.
- [ ] Icons and text are optically and vertically centered; trailing `+/-`, arrows, and indicators do not drift.
- [ ] Long/localized text wraps or elides without clipping actions.
- [ ] RTL mirrors directional layout and directional icons where applicable.
- [ ] Gallery content has a finite viewport and real vertical scrolling; no unbounded nested `ScrollViewer` is introduced.
- [ ] Compact/mobile navigation actually changes the current page and preserves safe margins.
- [ ] Virtualized collection controls retain a bounded realized-container count; masonry/quilted layouts receive a finite width and never create a nested infinite-height scroller.
- [ ] Data grids keep header/cell columns aligned after resize/reorder and preserve selection through sort/filter refresh.

## 4. Popup, menu, and picker safety

- [ ] The custom surface exists before `Popup.IsOpen` becomes true; there is no first-frame empty shell.
- [ ] `PopupRoot` is transparent with a themed fallback; there are no white native corners in Light or Dark mode.
- [ ] The popup uses platform fallback (`ShouldUseOverlayLayer=False`) and never assumes an overlay host.
- [ ] Placement touches its anchor as specified; top/bottom radii and maximum height match Material guidance.
- [ ] Opening another Material popup closes the old one; light dismiss and Escape restore focus.
- [ ] Menus/pickers do not hang or crash without an `IPopupImpl` (headless/Android-safe state tests).
- [ ] Calendar/time content is non-empty, localized, culture-first, and constrained to a realistic height.

## 5. Input, keyboard, and accessibility

- [ ] Pointer, touch/drag, wheel, keyboard, and programmatic paths produce the same state changes.
- [ ] Every visible affordance is real: drag handles drag, shortcut hints invoke, pull-to-refresh pulls, and sliders adjust values.
- [ ] Tab order is predictable; Enter/Space activate; Escape dismisses; focus returns to the trigger.
- [ ] Editable controls have auto-closing Cut/Copy/Paste/Undo/Redo/Select All menus with shortcuts.
- [ ] Automation name, role, value/range, selected/expanded/checked state, and live status are exposed.
- [ ] Reduced-motion mode removes nonessential motion without removing state feedback.

## 6. Motion and lifecycle

- [ ] State changes animate with Material motion tokens; selection/page transitions do not jump.
- [ ] Dragged surfaces bypass transform transitions while tracking the pointer, then restore transitions after release.
- [ ] Ripples originate from the interaction point and remain clipped.
- [ ] Timers, subscriptions, pointer capture, and async cancellation are released on detach/dispose.
- [ ] Animations are deterministic in tests and do not continuously allocate while idle.

### Borderless-window addendum

- [ ] Title-bar interactive descendants never begin a window drag; empty title space does.
- [ ] Double-click maximize/restore, right-click system-menu fallback, caption commands and every enabled resize edge work.
- [ ] Caption buttons expose automation names and remain legible in Light/Dark and active/inactive states.
- [ ] 100%, 150% and 200% scaling do not create dead hit-test strips or clip the client area.
- [ ] Windows, macOS, X11 and Wayland receive manual compositor validation; Android adapter remains a safe no-op.

## 7. Gallery and documentation

- [ ] Gallery includes a working interactive demo, not a decorative mock.
- [ ] Usage switches with a single-select Material button group.
- [ ] Both selectable/copyable AXAML and C# examples are complete.
- [ ] AvaloniaEdit syntax highlighting remains readable in both Light and Dark themes.
- [ ] Theme, localization, RTL, compact/mobile, keyboard, and direct-API behavior can be demonstrated.
- [ ] Public XML docs and the Flutter parity/status document are updated.

### Wave F structured-input addendum

- [ ] PIN/OTP paste is sanitized and distributed without replacing native editing/IME; focus, complete, error, mask and disabled states remain distinct.
- [ ] Tree expand/collapse never loses selection; Left/Right reverse in RTL, indentation mirrors, and cycles cannot recurse indefinitely.
- [ ] Tag input wraps real removable input chips, validates direct/keyboard/suggestion paths consistently, and keeps mutable MVVM collections synchronized.

## 8. Required regression gates

- [ ] Add a headless API/default/binding test.
- [ ] Add a template geometry/state-layer/popup-owner test for any visual defect fixed.
- [ ] Add an interaction test for click/keyboard/drag/wheel/command behavior.
- [ ] Render together with Light and Dark theme resources.
- [ ] Build and test Release with zero warnings and errors.
- [ ] Pack all three libraries independently and inspect dependency metadata.
- [ ] If Android workload is available, build the Android Gallery; otherwise record it as an explicit unverified platform gate.
- [ ] Immediately after every build, test, or pack command, delete generated `bin`, `obj`, `TestResults`, DLL/PDB, packages, downloaded fonts, and temporary assets before continuing; repeat the gate before delivery.

## Rounded corners (recurring — seen on the data table, then again on the borderless window)

A corner radius that looks right in the designer and square at runtime has two distinct causes.
They need different fixes, and checking for one does not cover the other.

**1. A child paints over the corner.** The rounded `Border` is correct, but its content has its
own `Background` and no clip, so it fills the square bounds and the radius is hidden underneath.
`Grid.ClipToBounds` does not round anything — only a `Border` with the matching `CornerRadius`
clips to a rounded shape. Fix: `ClipToBounds="True"` on the rounding `Border`.

**2. The surface behind the control is square.** The control rounds itself correctly, but it sits
on an opaque square window or popup surface that shows through at the corners — a rounded card
floating inside a square frame. No amount of clipping inside the control helps, because the thing
showing through is not the control. Fix: make the host surface transparent
(`Background="Transparent"` plus `TransparencyLevelHint="Transparent"`, with
`TransparencyBackgroundFallback` set to the surface brush so platforms that refuse transparency
degrade to the old opaque look rather than to white).

Check both before calling a corner defect fixed:

- [ ] Does the rounding `Border` have `ClipToBounds="True"` if any descendant sets `Background`?
- [ ] Is the host window or `PopupRoot` transparent, with a non-white fallback?
- [ ] Verified on a desktop run, not only headless — neither failure mode is visible headless,
      because headless composites everything into one surface.

## Pointer reachability

`RaiseEvent(new RoutedEventArgs(Button.ClickEvent))` skips hit testing. It proves a handler is
wired and nothing else: it cannot see an occluding layer, a transparent ancestor, a control
clipped out of its container, or a render transform that moves the pixels away from the
coordinates a pointer is given. Every "it does not respond to clicks" report lives in that
blind spot, so a green suite built on synthetic clicks is not evidence that a control is
clickable.

Use `PointerInput.Click` when the claim under test is that a user can operate the control -
anything layered over other content (a clear button over a text box's caret and selection
layers), small targets (an expander arrow), overlay content (snackbar actions), and popup
content. Keep a synthetic click only when the assertion is about wiring rather than
reachability, and say so in a comment at the call site.

`PointerInput` settles animation before clicking. The animation clock runs on wall time, so
forcing render-timer ticks does not fast-forward it: a popup surface measured straight after
opening sat at Opacity 0.101, and was still at 0.364 after 120 forced ticks. A click fired in
that window misses and lands on whatever is underneath, which looks exactly like a product
defect and is not one.

Two tests were found asserting things no user could do, both hidden by synthetic clicks:
the context-toolbar test never showed its page, so nothing was laid out; the Android gallery
test clicked a destination inside a closed navigation drawer, and then one scrolled far below
the viewport.

## Rendered pixels are testable, and were not being tested

`UseHeadlessDrawing=false` means these tests run the real Skia renderer, so
`window.CaptureRenderedFrame()` returns actual pixels. Thirty-odd tests here call it and then
assert `NotNull`, or save a PNG nobody diffs. Until `MdRenderedCornerTests` **no test in this
repository had ever read a single pixel**, which is why corner rounding, transparency and
overdraw were written off as "desktop only, not observable headless". They are observable.

To sample: `CopyPixels` into a pinned `uint[]`, index `y * width + x`. Give the window a
background colour the theme never uses, so "nothing painted here" is unambiguous.

Two cautions, both already paid for:

- Pin the page with `VerticalAlignment.Top`. Gallery pages declare a tall `MinHeight`; in a
  shorter window they get centred, the content lands at a negative offset, and every sample
  reads somewhere meaningless.
- Sample a point that must be painted as well as one that must not. Otherwise a control that
  failed to render at all passes the test.
