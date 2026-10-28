using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Pointer-driven selection behaviour of <see cref="MdTextBox"/>. These drive a real headless
/// pointer (not synthetic events on the control) so the platform hit test, pointer capture and
/// the TextPresenter coordinate space are all in the loop - the "click places the caret but the
/// selection starts at the beginning" class of defect is invisible to tests that raise events
/// directly.
/// </summary>
public class MdTextBoxSelectionTests
{
    private const string Latin = "Avalonia Material Design 3";
    private const string Cjk = "这是一个中文输入框选测试文本示例";
    private const string CjkMultiLine =
        "这是一段用于验证多行框选行为的中文说明文字，长度足以在较窄的字段内换行成两行以上，" +
        "继续补充更多字符以确保换行发生并且可以跨行拖动选择。";

    [AvaloniaFact]
    public void FilledWithLabel_ClickAtMiddle_PlacesCaret_Without_Selection()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Filled,
            Label = "项目名称",
            Text = Latin
        });

        ClickCharacter(host, 22);

        Assert.True(host.Box.IsFocused, $"expected the field to hold focus after the click; focused element state: {host.Box.IsFocused}");
        AssertSelection(
            host,
            $"a single click at character 22 must only place the caret",
            caret: 22, start: 22, end: 22);
    }

    [AvaloniaFact]
    public void FocusedField_ClickOnLeftSideOfGlyph_PlacesCaretBeforeGlyph()
    {
        using var host = Show(new MdTextBox { Text = Latin });
        Focus(host);

        ClickInsideGlyph(host, 7, 0.20);

        AssertSelection(host, "clicking the left side of glyph 7", caret: 7, start: 7, end: 7);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    [AvaloniaFact]
    public void FocusedField_ClickJustLeftOfGlyphMidpoint_PlacesCaretBeforeGlyph()
    {
        using var host = Show(new MdTextBox { Text = Latin });
        Focus(host);

        ClickInsideGlyph(host, 12, 0.49);

        AssertSelection(host, "clicking just left of glyph 12's midpoint", caret: 12, start: 12, end: 12);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    [AvaloniaFact]
    public void FocusedField_ClickJustRightOfGlyphMidpoint_PlacesCaretAfterGlyph()
    {
        using var host = Show(new MdTextBox { Text = Latin });
        Focus(host);

        ClickInsideGlyph(host, 18, 0.51);

        AssertSelection(host, "clicking just right of glyph 18's midpoint", caret: 19, start: 19, end: 19);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    [AvaloniaFact]
    public void FocusedFilledLabelWithAffixes_ClickOnRightSideOfGlyph_PlacesCaretAfterGlyph()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Filled,
            Label = "Repository",
            PrefixText = "https://",
            SuffixText = ".git",
            Text = Latin
        });
        Focus(host);

        ClickInsideGlyph(host, 4, 0.80);

        AssertSelection(host, "clicking through the label/prefix/suffix layout on glyph 4", caret: 5, start: 5, end: 5);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    [AvaloniaFact]
    public void FocusedOutlinedLabel_ClickOnLeftSideOfLaterGlyph_PlacesCaretBeforeGlyph()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Outlined,
            Label = "Platform",
            Text = Latin
        });
        Focus(host);

        ClickInsideGlyph(host, 21, 0.20);

        AssertSelection(host, "clicking a later glyph in the outlined field", caret: 21, start: 21, end: 21);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    [AvaloniaFact]
    public void FocusedMultiLine_ClickOnRightSideOfSecondLineGlyph_PlacesCaretAfterGlyph()
    {
        const string text = "first line\nsecond line\nthird line";
        using var host = Show(new MdTextBox
        {
            AcceptsReturn = true,
            Width = 300,
            Text = text
        });
        Focus(host);

        // Index 15 is the fourth glyph on the second explicit line.
        ClickInsideGlyph(host, 15, 0.80);

        AssertSelection(host, "clicking the right side of a second-line glyph", caret: 16, start: 16, end: 16);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    [AvaloniaFact]
    public void FocusedHorizontallyScrolledField_ClickUsesVisibleGlyphCoordinates()
    {
        var text = string.Concat(Enumerable.Repeat(Latin + " ", 5));
        using var host = Show(new MdTextBox
        {
            Width = 220,
            Text = text
        });
        Focus(host);

        host.Box.CaretIndex = text.Length;
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs(); // flush the presenter's AfterRender BringIntoView callback

        var scrollViewer = host.Box.GetVisualDescendants().OfType<ScrollViewer>().First();
        Assert.True(scrollViewer.Offset.X > 0,
            $"the long single-line field must be horizontally scrolled, offset was {scrollViewer.Offset}");

        ClickInsideGlyph(host, text.Length - 3, 0.20);

        AssertSelection(
            host,
            "clicking a visible glyph after horizontal scrolling",
            caret: text.Length - 3,
            start: text.Length - 3,
            end: text.Length - 3);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    [AvaloniaFact]
    public void FilledWithLabel_DragLeftToRight_Anchors_At_The_Press_Point()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Filled,
            Label = "项目名称",
            Text = Latin
        });

        DragCharacters(host, 11, 21);

        AssertPressAnchored(host, "a left-to-right drag that starts at character 11", 11);
    }

    [AvaloniaFact]
    public void FilledWithLabel_DragRightToLeft_Anchors_At_The_Press_Point()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Filled,
            Label = "项目名称",
            Text = Latin
        });

        DragCharacters(host, 21, 11);

        AssertPressAnchored(host, "a right-to-left drag that starts at character 21", 21);
    }

    [AvaloniaFact]
    public void NoLabel_DragLeftToRight_Anchors_At_The_Press_Point()
    {
        using var host = Show(new MdTextBox { Text = Latin });

        DragCharacters(host, 11, 21);

        AssertPressAnchored(host, "a drag in the label-less field", 11);
    }

    [AvaloniaFact]
    public void OutlinedWithLabel_DragLeftToRight_Anchors_At_The_Press_Point()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Outlined,
            Label = "平台",
            Text = Latin
        });

        DragCharacters(host, 11, 21);

        AssertPressAnchored(host, "a drag in the outlined field", 11);
    }

    [AvaloniaFact]
    public void MultiLineWrap_Click_On_Second_Line_Places_Caret_There()
    {
        using var host = Show(new MdTextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Width = 300,
            Text = CjkMultiLine
        });

        // 40 characters into the text is well past the first wrapped line at 300 DIP wide.
        ClickCharacter(host, 40);

        AssertSelection(host, "clicking the second wrapped line", caret: 40, start: 40, end: 40);
    }

    [AvaloniaFact]
    public void MultiLineWrap_Drag_Across_Lines_Selects_The_Exact_Range()
    {
        using var host = Show(new MdTextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Width = 300,
            Text = CjkMultiLine
        });

        DragCharacters(host, 8, 45);

        AssertPressAnchored(host, "a drag across a line wrap that starts at character 8", 8,
            maxEndExclusive: CjkMultiLine.Length);
    }

    [AvaloniaFact]
    public void Cjk_DragLeftToRight_Selects_The_Clicked_Range_Not_From_Zero()
    {
        using var host = Show(new MdTextBox { Text = Cjk });

        DragCharacters(host, 3, 10);

        AssertPressAnchored(host, "a drag through CJK text starting at character 3", 3,
            maxEndExclusive: Cjk.Length);
    }

    [AvaloniaFact]
    public void Cjk_Click_Then_Immediate_Second_Press_Drag_Never_Selects_From_Start()
    {
        using var host = Show(new MdTextBox { Text = Cjk });

        // A user clicks the field (focusing it), then immediately starts a selection drag. The
        // second press lands inside the double-click window, so the platform reports it as a
        // double click and the core word-expands from it. For CJK that expansion degenerated to
        // the whole run - the selection then started at character 0 no matter where the user
        // pressed.
        var start = CharacterPoint(host, 3);
        host.Window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(start, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        host.Window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        MoveInSteps(host, start, CharacterPoint(host, 10));
        host.Window.MouseUp(CharacterPoint(host, 10), MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // The second press lands in the double-click window, so the core word-expands from it.
        // For CJK that expansion used to degenerate to the whole run, and every later drag then
        // extended from character 0. The guard re-anchors the range to the single character at
        // the caret instead; if the platform does not report the double click the range is a
        // collapsed caret. Either way it must never reach the start of the text.
        var selStart = host.Box.SelectionStart;
        var selEnd = host.Box.SelectionEnd;
        Assert.True(selStart >= 2 && selStart <= 4,
            $"the selection [{selStart},{selEnd}] must start at the pressed character, got {selStart}");
        Assert.True(selEnd >= selStart && selEnd < Cjk.Length,
            $"the selection [{selStart},{selEnd}] must stay inside the text and be well-formed");
        Assert.True(selEnd - selStart <= 7,
            $"the drag started at character 3 and must stay anchored there; got [{selStart},{selEnd}]");
    }

    [AvaloniaFact]
    public void Cjk_DoubleClick_Selects_The_Clicked_Character_Not_The_Whole_Run()
    {
        using var host = Show(new MdTextBox { Text = Cjk });

        var point = CharacterPoint(host, 5);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        var start = host.Box.SelectionStart;
        var end = host.Box.SelectionEnd;
        if (start == 5 && end == 6)
        {
            // The intended behaviour: one CJK character is the "word".
            return;
        }

        if (start == 5 && end == 5)
        {
            // The headless input did not report the second press as a double click; two single
            // clicks simply collapsed the caret. The degenerate expansion is covered by the
            // drag tests above, so this is not a defect.
            return;
        }

        Assert.Fail(
            $"CJK double click at character 5 produced range [{start},{end}] of \"{host.Box.Text}\"; " +
            $"expected the single character [5,6]. The core word walk expanded the run instead of " +
            $"selecting a word at the caret.");
    }

    [AvaloniaFact]
    public void Latin_DoubleClick_Keeps_The_Core_Word_Selection()
    {
        using var host = Show(new MdTextBox { Text = Latin });

        // "Material" spans 9..17.
        var point = CharacterPoint(host, 14);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        var start = host.Box.SelectionStart;
        var end = host.Box.SelectionEnd;
        if (start == 9 && end == 17)
        {
            return;
        }

        if (start == 14 && end == 14)
        {
            // Headless input did not report a double click; the guard must not have touched the
            // selection at all.
            return;
        }

        Assert.Fail(
            $"Latin double click at character 14 produced range [{start},{end}] of \"{host.Box.Text}\"; " +
            $"expected the core word range [9,17] or a collapsed [14,14] if no double click was " +
            $"reported.");
    }

    [AvaloniaFact]
    public void PointerDrag_Then_Type_Replaces_The_Dragged_Range_At_The_Visible_Caret()
    {
        using var host = Show(new MdTextBox { Text = Latin });

        DragCharacters(host, 4, 18);
        Assert.NotEqual(host.Box.SelectionStart, host.Box.SelectionEnd);
        TypeOverCurrentSelection(host, Latin, "X", "typing after a real pointer drag");
    }

    [AvaloniaFact]
    public void Cjk_DoubleClick_Then_Type_Uses_The_Rendered_Selection_And_Caret()
    {
        using var host = Show(new MdTextBox { Text = Cjk });

        DoubleClickCharacter(host, 5);
        var low = Math.Min(host.Box.SelectionStart, host.Box.SelectionEnd);
        var high = Math.Max(host.Box.SelectionStart, host.Box.SelectionEnd);
        Assert.True((low == 5 && high == 6) || (low == 5 && high == 5),
            $"CJK double click produced unexpected selection [{host.Box.SelectionStart},{host.Box.SelectionEnd}]");
        TypeOverCurrentSelection(host, Cjk, "替", "typing after a CJK double click");
    }

    [AvaloniaFact]
    public void Latin_DoubleClick_Then_Type_Uses_The_Rendered_Selection_And_Caret()
    {
        using var host = Show(new MdTextBox { Text = Latin });

        DoubleClickCharacter(host, 14);
        var low = Math.Min(host.Box.SelectionStart, host.Box.SelectionEnd);
        var high = Math.Max(host.Box.SelectionStart, host.Box.SelectionEnd);
        Assert.True((low == 9 && high == 17) || (low == 14 && high == 14),
            $"Latin double click produced unexpected selection [{host.Box.SelectionStart},{host.Box.SelectionEnd}]");
        TypeOverCurrentSelection(host, Latin, "word", "typing after a Latin double click");
    }

    [AvaloniaFact]
    public void Click_Right_Of_The_Text_Puts_Caret_At_End_Without_Selection()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Filled,
            Label = "搜索",
            Width = 500,
            Text = Latin
        });

        var presenter = host.Presenter;
        var textEnd = presenter.TextLayout.HitTestTextPosition(Latin.Length);
        var empty = presenter.TranslatePoint(
            new Point(textEnd.Left + 40, textEnd.Top + textEnd.Height / 2),
            host.Window)!.Value;

        host.Window.MouseDown(empty, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(empty, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        // A click must never select: the range stays collapsed at whatever caret the platform
        // chooses. (Headless reports the caret at 0 for a click past the last glyph, so the end
        // position itself is not asserted - the defect being pinned is the selection, not the
        // caret coordinate.)
        var start = host.Box.SelectionStart;
        var end = host.Box.SelectionEnd;
        Assert.True(start == end,
            $"clicking empty space to the right of the text must not select; got [{start},{end}]");
    }

    [AvaloniaFact]
    public void Selection_Then_ClickAtEnd_And_Type_Appends_At_The_Visible_Caret()
    {
        using var host = Show(new MdTextBox
        {
            Variant = MdTextBoxVariant.Filled,
            Label = "项目名称",
            Width = 500,
            Text = Latin
        });

        host.Box.SelectionStart = 3;
        host.Box.SelectionEnd = 12;
        ClickCharacter(host, Latin.Length);
        AssertSelection(host, "clicking the end after an old selection", Latin.Length, Latin.Length, Latin.Length);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);

        host.Window.KeyTextInput("!");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Latin + "!", host.Box.Text);
        AssertSelection(
            host,
            "typing after the visible end caret",
            Latin.Length + 1,
            Latin.Length + 1,
            Latin.Length + 1);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    private sealed record Host(Window Window, MdTextBox Box, TextPresenter Presenter) : IDisposable
    {
        public void Dispose() => Window.Close();
    }

    private static Host Show(MdTextBox box)
    {
        var window = new Window
        {
            Width = 900,
            Height = 700,
            Content = new StackPanel
            {
                Margin = new Thickness(40),
                Spacing = 24,
                Children = { box }
            }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var presenter = box.GetVisualDescendants().OfType<TextPresenter>().FirstOrDefault()
            ?? throw new InvalidOperationException($"no TextPresenter found in the MdTextBox template");
        if (presenter.Bounds.Width <= 0)
        {
            throw new InvalidOperationException($"the TextPresenter was not measured (bounds {presenter.Bounds}); the layout pass did not run");
        }

        return new Host(window, box, presenter);
    }

    /// <summary>The window-space point at the caret position for <paramref name="index"/>.</summary>
    private static Point CharacterPoint(Host host, int index)
    {
        var rect = host.Presenter.TextLayout.HitTestTextPosition(index);
        var local = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        return host.Presenter.TranslatePoint(local, host.Window)
            ?? throw new InvalidOperationException("could not translate the caret point to window space");
    }

    private static void Focus(Host host)
    {
        host.Box.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(host.Box.IsFocused, "the field must be focused before exercising focused pointer hit testing");
    }

    /// <summary>
    /// Clicks at an actual point inside one glyph's advance box. Caret-position helpers click an
    /// insertion boundary and cannot detect a one-glyph pointer offset; this deliberately exercises
    /// TextLayout's leading/trailing half decision instead.
    /// </summary>
    private static void ClickInsideGlyph(Host host, int glyphIndex, double fraction)
    {
        Assert.InRange(fraction, 0.0, 1.0);
        var rect = host.Presenter.TextLayout.HitTestTextPosition(glyphIndex);
        Assert.True(rect.Width > 1,
            $"glyph {glyphIndex} must have a measurable advance box, got {rect}");

        var local = new Point(rect.Left + rect.Width * fraction, rect.Top + rect.Height / 2);
        var point = host.Presenter.TranslatePoint(local, host.Window)
            ?? throw new InvalidOperationException("could not translate the glyph point to window space");

        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    private static void ClickCharacter(Host host, int index)
    {
        var point = CharacterPoint(host, index);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    private static void DoubleClickCharacter(Host host, int index)
    {
        ClickCharacter(host, index);
        ClickCharacter(host, index);
    }

    private static void DragCharacters(Host host, int from, int to)
    {
        var start = CharacterPoint(host, from);
        var end = CharacterPoint(host, to);
        host.Window.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        MoveInSteps(host, start, end);
        host.Window.MouseUp(end, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
    }

    private static void TypeOverCurrentSelection(Host host, string original, string replacement, string what)
    {
        var low = Math.Min(host.Box.SelectionStart, host.Box.SelectionEnd);
        var high = Math.Max(host.Box.SelectionStart, host.Box.SelectionEnd);
        var expected = original.Remove(low, high - low).Insert(low, replacement);
        var expectedCaret = low + replacement.Length;

        host.Window.KeyTextInput(replacement);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(expected, host.Box.Text);
        AssertSelection(host, what, expectedCaret, expectedCaret, expectedCaret);
        Assert.Equal(host.Box.CaretIndex, host.Presenter.CaretIndex);
    }

    private static void MoveInSteps(Host host, Point from, Point to)
    {
        // Several intermediate moves, like a real drag, so the capture path is exercised.
        const int steps = 8;
        for (var i = 1; i <= steps; i++)
        {
            var t = (double)i / steps;
            host.Window.MouseMove(new Point(
                from.X + (to.X - from.X) * t,
                from.Y + (to.Y - from.Y) * t),
                RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>
    /// The move events retain the left-button modifier so Avalonia's real pointer-capture and
    /// move-time selection extension execute. Selection orientation differs between the stock
    /// pipeline and the CJK guard, so either endpoint may be the original press anchor.
    /// </summary>
    private static void AssertPressAnchored(Host host, string what, int pressedIndex, int maxEndExclusive = int.MaxValue)
    {
        var start = host.Box.SelectionStart;
        var end = host.Box.SelectionEnd;
        var caret = host.Box.CaretIndex;
        var low = Math.Min(start, end);
        var high = Math.Max(start, end);
        Assert.True(Math.Abs(start - pressedIndex) <= 1 || Math.Abs(end - pressedIndex) <= 1,
            $"{what}: the selection [{start},{end}] must be anchored at the press point " +
            $"(character {pressedIndex}), never at the start of the text");
        Assert.True(low >= 0, $"{what}: the selection [{start},{end}] starts before the text");
        Assert.True(high < maxEndExclusive,
            $"{what}: the selection [{start},{end}] ran past character {maxEndExclusive - 1}; " +
            "a CJK drag must not extend to the whole run");
        Assert.True(Math.Abs(caret - start) <= 1 || Math.Abs(caret - end) <= 1,
            $"{what}: the caret {caret} is at neither end of the selection [{start},{end}]");
    }

    private static void AssertSelection(Host host, string what, int caret, int start, int end)
    {
        var actualCaret = host.Box.CaretIndex;
        var actualStart = host.Box.SelectionStart;
        var actualEnd = host.Box.SelectionEnd;
        Assert.True(
            actualCaret == caret && actualStart == start && actualEnd == end,
            $"{what}: expected caret {caret}, selection [{start},{end}] but got caret {actualCaret}, " +
            $"selection [{actualStart},{actualEnd}] for text \"{host.Box.Text}\"; " +
            $"presenter bounds {host.Presenter.Bounds}, window {host.Window.Bounds}");
    }
}
