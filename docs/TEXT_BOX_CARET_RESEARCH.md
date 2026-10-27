# TextBox caret synchronization research

Research date: 2026-10-27

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

The release did not override `OnKeyDown` or `OnTextInput`. Padding, label animation and icon columns
do not choose the insertion index. The incorrect insertion occurs only when the visual presenter and
the logical `TextBox` state stop satisfying the synchronization invariant.

A later CJK selection correction on this branch added another relevant difference from stock
Avalonia: it re-anchors `SelectionStart` and `SelectionEnd` after Avalonia's pointer handler. A
non-empty selection does not automatically copy the presenter's caret back to `TextBox.CaretIndex`,
so that correction also needs to synchronize the logical caret explicitly.

## Resolution

The Material text controls now enforce synchronization at their custom-template boundary:

1. after a pointer gesture leaves a collapsed selection, the presenter's visible caret is copied to
   `TextBox.CaretIndex`;
2. after committed text input, text and logical caret are explicitly copied back to the presenter,
   mirroring Avalonia's upstream fix;
3. the CJK word-selection correction sets the logical caret before restoring its corrected range.

The same boundary is used by `MdTextBox` and `MdSearchBar`. Regression coverage drives real headless
keyboard input for reverse selection, replacement, a following character, and selection followed by
a pointer click at the text end. Every step checks text, selection, logical caret, and presenter
caret rather than checking only the final string.
