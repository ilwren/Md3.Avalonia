# TextBox caret synchronization research

Research dates: 2026-10-27–2026-10-28

## Reported scenario

The reproducible category reported against `0.5.0-preview.2` is Windows with the Avalonia 12
`Md3.Avalonia` package: after selection (including reverse keyboard selection), the rendered caret
can appear at the intended location while the next text input is applied at another index.

## Avalonia baseline

`MdTextBox` derives from Avalonia `TextBox`; it does not implement a separate text store. Avalonia
12.1.2 keeps two related states:

- `TextBox.CaretIndex`, `SelectionStart`, and `SelectionEnd` are the logical editing state used by
  `TextBox.HandleTextInput` to remove a selection and insert committed text.
- `PART_TextPresenter.CaretIndex` plus its private glyph hit are the rendered caret state.

Avalonia's Fluent template binds `Text`, `CaretIndex`, `SelectionStart`, and `SelectionEnd` to a
`TextPresenter`. Its core `TextBox` also performs explicit synchronization after editing. This was
added by upstream [PR 14960](https://github.com/AvaloniaUI/Avalonia/pull/14960), which fixed
[issue 13648](https://github.com/AvaloniaUI/Avalonia/issues/13648) (replace a reverse selection,
then see the caret at a different insertion point),
[issue 13886](https://github.com/AvaloniaUI/Avalonia/issues/13886) (replace bound text, then type at
an apparently incorrect caret), and issue 14563. The upstream fix first writes the new text directly
to `TextPresenter`, calls `MoveCaretToTextPosition`, and then publishes the logical caret.

## What MdTextBox changed

The `0.5.0-preview.2` control retained Avalonia's `TextBox` editing implementation and the same four
`TextPresenter` bindings. Its changes around that editing surface were:

- Material filled/outlined containers and a floating label;
- prefix, suffix, leading/trailing actions, clear and password buttons;
- supporting/error text and a character counter;
- a localized context menu;
- label/input-row motion and Material colors.

The release did not override `OnKeyDown` or `OnTextInput`. Padding and icon columns do not choose the
insertion index. The incorrect insertion occurs when the visual presenter and the logical `TextBox`
state stop satisfying the synchronization invariant.

A later CJK selection correction on this branch added another relevant difference from stock
Avalonia: it re-anchors `SelectionStart` and `SelectionEnd` after Avalonia's pointer handler. A
non-empty selection does not automatically copy the presenter's caret back to `TextBox.CaretIndex`,
so that correction also needs to synchronize the logical caret explicitly.

## Pointer hit-testing parity (2026-10-28)

A second report concerned the click itself: in an already focused field the caret should feel like it
lands exactly at the pointed insertion boundary, not merely somewhere near the selected character.
The 12.1.2 source and Fluent template were checked again end to end:

1. `TextBox.OnPointerPressed` asks the event for `GetPosition(PART_TextPresenter)`;
2. `TextPresenter.MoveCaretToPoint` passes that local point to `TextLayout.HitTestPoint`;
3. the text formatter chooses the leading or trailing `CharacterHit` for the glyph under the point;
4. `SelectionStart`/`SelectionEnd` collapse at the presenter's resulting `CaretIndex`;
5. drag moves use the same presenter coordinate space after clamping to its bounds.

The Fluent template therefore keeps its interactive `TextPresenter` in ordinary measured/arranged
layout. The Material template had one additional difference: the grid containing the presenter,
scroll viewer, prefix and suffix was render-translated by 8 DIP when a filled label floated, and the
transform itself had a transition. Although the index calculation still belongs to Avalonia, this
made the surface seen by the pointer a moving render coordinate space rather than the stable layout
space used by the reference template. It also made exact pointer regressions easy to miss: the old
tests generated points from caret boundaries and asserted only a target index or a loose ±1 range.

The filled-label offset is now represented by a stable asymmetric layout margin. Only the
non-interactive label is transformed/animated; the input row and `TextPresenter` are never
render-transformed. The resulting presenter-local point is the same point the user sees throughout
focus and label motion, including with affixes.

The horizontal-scroll regression exposed a second template-only difference. `MaterialTheme` does not
install a global theme for Avalonia's native `ScrollViewer` (doing so would violate the library's
scoping contract), so a native `ScrollViewer` inside the text-field template had no measured
scroll-content viewport in a Material-only host. `PART_ScrollViewer` now uses `MdScrollViewer`, which
is a `ScrollViewer` subclass with the library's scoped template. Avalonia's base `TextBox` still finds
and owns the part, while long lines now have a real extent, viewport and offset. Pointer-to-presenter
translation therefore remains exact after horizontal caret scrolling too.

New headless regressions click real positions inside glyph advance boxes rather than clicking a caret
boundary. They pin the leading side, both sides immediately around the midpoint, the trailing side,
several character indices, filled/outlined labels, prefix/suffix columns, a second line, and a
horizontally scrolled single-line field. Every case checks the logical caret, collapsed selection and
visual presenter caret exactly (no ±1 tolerance).

## Resolution

The Material text controls now enforce synchronization at their custom-template boundary:

1. after a pointer gesture leaves a collapsed selection, the presenter's visible caret is copied to
   `TextBox.CaretIndex`;
2. after committed text input, text and logical caret are explicitly copied back to the presenter,
   mirroring Avalonia's upstream fix;
3. the CJK word-selection correction sets the logical caret before restoring its corrected range;
4. the `MdTextBox` input row stays in measured layout coordinates while only its floating label uses
   a render transform, matching the official template's hit-test geometry;
5. `PART_ScrollViewer` uses the scoped `MdScrollViewer` theme so horizontal scrolling has real
   viewport geometry without requiring Fluent/Simple global styles.

The same synchronization boundary is used by `MdTextBox` and `MdSearchBar`. Regression coverage
also drives real headless keyboard input for reverse selection, replacement, a following character,
and selection followed by
a pointer click at the text end. Every step checks text, selection, logical caret, and presenter
caret rather than checking only the final string.
