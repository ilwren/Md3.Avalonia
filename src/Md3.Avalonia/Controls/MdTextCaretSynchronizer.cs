using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace Md3.Avalonia.Controls;

/// <summary>
/// Keeps the logical editing caret owned by <see cref="TextBox"/> and the visual caret owned by
/// <see cref="TextPresenter"/> synchronized at the two points where a custom TextBox template can
/// expose stale state: after pointer placement and after selection replacement.
/// </summary>
internal static class MdTextCaretSynchronizer
{
    /// <summary>
    /// A pointer moves the presenter first. When it leaves no selection, that visible location is
    /// the insertion location and must also be copied to the TextBox before the next text input.
    /// </summary>
    public static void CopyCollapsedPresenterCaretToTextBox(TextBox box, TextPresenter? presenter)
    {
        if (presenter is null || box.SelectionStart != box.SelectionEnd) return;

        var visualCaret = presenter.CaretIndex;
        if (box.CaretIndex != visualCaret)
            box.SetCurrentValue(TextBox.CaretIndexProperty, visualCaret);
    }

    /// <summary>
    /// TextBox editing updates its logical text and caret first. Explicitly refresh the presenter
    /// afterwards so replacing a reverse selection cannot leave the old glyph hit as the visible
    /// caret while the next character is inserted at the new logical index.
    /// </summary>
    public static void CopyTextBoxStateToPresenter(TextBox box, TextPresenter? presenter)
    {
        if (presenter is null) return;

        presenter.SetCurrentValue(TextPresenter.TextProperty, box.Text);
        presenter.MoveCaretToTextPosition(box.CaretIndex);
    }
}
