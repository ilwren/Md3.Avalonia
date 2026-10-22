using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Corrects double-click word selection for text that has no space-separated word boundaries,
/// such as CJK. Avalonia's core expands a double click to the whole run of letter/digit
/// characters and walks that run back to the start of the line, so a double click (or a second
/// click inside the double-click window, as happens when a user clicks a field and immediately
/// starts a selection drag) on Chinese text selects from the beginning of the field, and every
/// later drag extends the range from there. This guard re-anchors the range to the Unicode word
/// at the caret position - .NET word segmentation treats each ideograph as one word - and keeps
/// the drag extension consistent with that anchor.
/// </summary>
/// <remarks>
/// Attach it from <c>OnPointerPressed</c>/<c>OnPointerMoved</c>/<c>OnPointerReleased</c> of a
/// <see cref="TextBox"/> subclass, always after the base call so the core keeps full ownership of
/// caret movement, pointer capture and the touch selection handles.
/// </remarks>
internal static class MdWordSelectionGuard
{
    /// <summary>Per-gesture state. Null means the guard does not intervene for this gesture.</summary>
    private sealed class State
    {
        public int WordStart;
        public int WordEnd;
    }

    public static readonly AttachedProperty<State?> PreemptedStateProperty =
        AvaloniaProperty.RegisterAttached<MdWordSelectionGuard, TextBox, State?>(
            nameof(PreemptedState),
            defaultValue: null,
            inherits: false);

    public static State? GetPreemptedState(TextBox box) => box.GetValue(PreemptedStateProperty);
    public static void SetPreemptedState(TextBox box, State? value) => box.SetValue(PreemptedStateProperty, value);

    /// <summary>Call from <see cref="TextBox.OnPointerPressed"/> after <c>base</c>.</summary>
    public static void CorrectAfterPress(TextBox box, PointerPressedEventArgs e, TextPresenter? presenter)
    {
        // Every new gesture starts without a guard anchor; the core's own single-click path resets
        // the word anchor internally, so only double clicks need a replacement one.
        SetPreemptedState(box, null);

        if (presenter is null || e.ClickCount != 2) return;

        switch (e.Pointer.Type)
        {
            case PointerType.Mouse:
                if (!e.GetCurrentPoint(box).Properties.IsLeftButtonPressed) return;
                break;
            case PointerType.Pen:
            case PointerType.Touch:
                break;
            default:
                return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;

        var text = box.Text;
        if (string.IsNullOrEmpty(text)) return;

        // The core moved the presenter caret to the press point before the word expansion.
        var caret = presenter.CaretIndex;
        if (caret <= 0 || caret >= text.Length) return;

        // ASCII letters and digits keep the core's word behaviour; anything else (CJK, Cyrillic,
        // Greek, ...) is what makes PreviousWord/NextWord degenerate to the whole run.
        if (!IsNonAsciiLetterOrDigit(text[caret])) return;

        var (start, end) = WordRange(text, caret);

        var currentStart = box.SelectionStart;
        var currentEnd = box.SelectionEnd;
        if (Math.Min(currentStart, currentEnd) == start && Math.Max(currentStart, currentEnd) == end)
            return;

        box.SetCurrentValue(TextBox.SelectionStartProperty, start);
        box.SetCurrentValue(TextBox.SelectionEndProperty, end);
        SetPreemptedState(box, new State { WordStart = start, WordEnd = end });
    }

    /// <summary>Call from <see cref="TextBox.OnPointerMoved"/> after <c>base</c>.</summary>
    public static void ExtendDuringMove(TextBox box, PointerEventArgs e, TextPresenter? presenter)
    {
        var state = GetPreemptedState(box);
        if (state is null || presenter is null) return;

        // Touch drags after a double tap are caret moves by core design (selection on touch starts
        // from a long press), so only the mouse drag needs its range re-derived.
        if (e.Pointer.Type != PointerType.Mouse) return;
        if (e.Pointer.Captured != presenter) return;
        if (!e.GetCurrentPoint(box).Properties.IsLeftButtonPressed) return;

        if (string.IsNullOrEmpty(box.Text))
        {
            SetPreemptedState(box, null);
            return;
        }

        var caret = presenter.CaretIndex;
        var desiredStart = caret > state.WordStart ? state.WordStart : caret;
        var desiredEnd = caret > state.WordStart ? caret : state.WordEnd;

        if (box.SelectionStart != desiredStart || box.SelectionEnd != desiredEnd)
        {
            box.SetCurrentValue(TextBox.SelectionStartProperty, desiredStart);
            box.SetCurrentValue(TextBox.SelectionEndProperty, desiredEnd);
        }
    }

    /// <summary>Call from <see cref="TextBox.OnPointerReleased"/> after <c>base</c>.</summary>
    public static void ClearOnRelease(TextBox box, PointerReleasedEventArgs e)
    {
        if (GetPreemptedState(box) is null) return;
        if (!e.GetCurrentPoint(box).Properties.IsLeftButtonPressed)
            SetPreemptedState(box, null);
    }

    /// <summary>
    /// The Unicode word boundaries around the character at <paramref name="index"/>, using
    /// <see cref="StringInfo"/> word segmentation, which treats each CJK ideograph as one word and
    /// keeps Latin runs contiguous.
    /// </summary>
    internal static (int Start, int End) WordRange(string text, int index)
    {
        // Position at `index`; the word containing element index-1 starts at `s` and ends at `e`.
        var info = StringInfo.CreateInstance(text, index);
        info.MovePreviousWord();
        var s = info.ElementIndex;
        info.MoveNextWord();
        var e = info.ElementIndex;

        // Element index-1 and element index are one word iff that word extends past index.
        var start = e > index ? s : index;

        info = StringInfo.CreateInstance(text, start);
        info.MoveNextWord();
        return (start, info.ElementIndex);
    }

    private static bool IsNonAsciiLetterOrDigit(char ch)
    {
        if (!char.IsLetterOrDigit(ch)) return false;
        return !(ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9');
    }
}
