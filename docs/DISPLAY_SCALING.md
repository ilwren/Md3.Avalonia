# Display scaling

Short answer: **yes.** The library is scale-independent by construction, and the handful of places
that leave device-independent space handle the scale factor explicitly. This page records the
evidence rather than the assurance, because "it should scale" is the kind of claim that is wrong
in exactly the places nobody checked.

## Why it works by construction

Avalonia lays out in **device-independent pixels (DIPs)**. A `Width="48"` in a template is 48 DIPs;
the compositor multiplies by the window's `RenderScaling` on the way to the framebuffer. At 100% a
DIP is one physical pixel, at 150% it is 1.5, at 200% it is 2. Nothing in a template has to know.

So every hard-coded number in this library — the 40dp comparison handle, the 360 DIP date picker,
the 48px cascader chevron column, the Material token sizes — scales with the OS setting without
any code. The same is true of `FontSize`: Avalonia font sizes are DIPs.

## Where a scale factor can actually be dropped

Four categories, and each one is either absent from the library or handled:

| Category | Status |
|---|---|
| **Raster assets** | None. `src/` ships no `.png`, `.jpg`, `.bmp`, `.gif` or `.ico` at all. Every glyph the library draws is a vector `Path`, a brush, or the Material Symbols **font** — outline data, so it is resolution-independent. Pinned by `The_Libraries_Ship_No_Raster_Assets`. |
| **`RenderTargetBitmap`** | Two uses — the reorderable-list drag ghost in `MdCascaderTransfer` and the snapshot helper in `MdFlutterPhase4`. Both size the bitmap as `Bounds * RenderScaling` and set the DPI to `96 * scale`. Sizing one from `Bounds` alone is the classic bug: a half-resolution blurry ghost at 200%. Pinned by `Every_Render_Target_Bitmap_Is_Sized_In_Physical_Pixels`. |
| **Screen-space coordinates** | One use: `MdBorderlessWindow` calls `window.PointToScreen(clientPoint)` before `TrackPopupMenu`. `PointToScreen` returns a `PixelPoint` in physical screen pixels and the Win32 API wants physical screen pixels, so the conversion is the right one — this is the correct way to cross the boundary, not a leak. |
| **Layout rounding** | On. `UseLayoutRounding` defaults to true and rounds to whole *physical* pixels using `RenderScaling`, which is what stops a 1 DIP outline from straddling two pixels at 125% and rendering as a grey smear. No template in `src/` switches it off. Pinned by `Layout_Rounding_Stays_On_So_Edges_Land_On_Whole_Pixels`. |

## What is *not* covered

Being honest about the boundary of the claim:

- **Non-integer scale factors are not pixel-perfect, and cannot be.** At 125% a 1 DIP hairline is
  1.25 physical pixels. Layout rounding snaps element *edges* to whole pixels, but a stroke whose
  width is not a whole number of pixels is still antialiased. This is true of every DIP-based UI
  framework including WPF and WinUI; it is not a defect in this library.
- **Runtime scale changes** — dragging a window between a 100% and a 200% monitor — are Avalonia's
  responsibility, and are **not tested here**. Headless windows report a fixed `RenderScaling`, so
  there is no way to exercise a scale change in this suite. If a control caches a pixel size across
  a monitor change it would not be caught; the two `RenderTargetBitmap` sites read `RenderScaling`
  at use rather than caching it, which is the behaviour that matters.
- **The OS text-size setting** (Windows "Make text bigger", Android font scale) is separate from
  display scaling and Avalonia does not apply it to `FontSize` automatically. An application that
  wants to honour it has to scale its own type ramp. The library does not block that — every size
  comes from a token in `Themes/Tokens/` and can be overridden in a merged dictionary — but it does
  not do it for you.
- **Physical verification** at 125% / 150% / 200% on a real Windows display is item 8 of the manual
  matrix in `RELEASE_VALIDATION.md` and remains unsigned. Everything above is a source-level and
  headless argument, not a screenshot.

## Guard tests

`tests/Md3.Avalonia.HeadlessTests/MdScalingTests.cs`:

- `Every_Render_Target_Bitmap_Is_Sized_In_Physical_Pixels` — source scan; fails if a new
  `RenderTargetBitmap` is constructed without `RenderScaling` nearby.
- `The_Libraries_Ship_No_Raster_Assets` — fails the moment raster artwork lands in `src/`.
- `Layout_Is_Expressed_In_Device_Independent_Pixels` — pins the DIP contract behind every template.
- `Layout_Rounding_Stays_On_So_Edges_Land_On_Whole_Pixels` — fails if a template disables it.
