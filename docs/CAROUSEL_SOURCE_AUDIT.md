# Material carousel source audit

Audit date: 2026-10-03

## Frozen references

- Material 3 carousel specs: <https://m3.material.io/components/carousel/specs>
- Material Components Android commit `60ff09436d5d477a4b9d02940f31eb01e1250620`
- Flutter framework commit `53d381d9067f16b1d4132f5b8f059a9cc5ddb9ec`

The review covered Android's `CarouselStrategy`, `MultiBrowseCarouselStrategy`, `HeroCarouselStrategy`, `Arrangement`, `CarouselStrategyHelper`, `KeylineState` and `CarouselLayoutManager`, plus Flutter's complete `packages/flutter/lib/src/material/carousel.dart` implementation and API documentation.

## Source behavior used

- Carousel geometry is a viewport/keyline concern, not selected-state styling. Flutter exposes `onTap` independently of its weighted sliver geometry; tapping a child does not replace the weight list.
- Flutter weighted carousels derive each visible extent from viewport proportions. The next child gradually adopts the preceding keyline's size as scrolling progresses. Hero and center-aligned hero use `consumeMaxWeight` so every item can reach the focal extent.
- Android multi-browse searches arrangements containing large, medium and small items and adjusts sizes to fit the available container while minimizing the change to the target large size.
- Android defines the medium target as `(large + small) / 2`.
- Android hero reserves room for one small preview; center alignment doubles the surrounding small/medium counts when enough items exist.
- Android's pinned resources define the supported small range as 40–56 dp. Very small containers may drop a small keyline rather than violate the large-item ordering constraint.
- Uncontained layout retains uniform item extents.

## Avalonia decisions

`MdCarousel` remains a native `ListBox` in a native horizontal `ScrollViewer`, preserving `ItemsSource`, data templates, selection, keyboard, automation and direct manipulation. The correction separates `_layoutAnchorIndex` from `SelectedIndex`:

- pointer tap may select/invoke content but does not move the keyline or resize cards;
- controller navigation, `ScrollTo`, keyboard, autoplay and post-gesture snapping move the keyline;
- weighted variants fit their leading arrangement to the current viewport;
- multi-browse uses Android's medium midpoint and a 40–56 DIP small width;
- hero reserves a small preview, center-aligned reserves previews on both sides when enough items exist, and uncontained stays uniform;
- width changes recompute the arrangement, while Material motion tokens animate snapped keyline size changes and reduced/none schemes remove those transitions.

This is a clean-room Avalonia adaptation, not a line-for-line port. Flutter's render sliver continuously interpolates every weighted extent during each scroll frame; this control retains Avalonia's native virtualizing panel and commits its keyline arrangement at native snap boundaries. The acceptance invariant addressed here is that selection alone never changes geometry, while viewport and scroll navigation do.
