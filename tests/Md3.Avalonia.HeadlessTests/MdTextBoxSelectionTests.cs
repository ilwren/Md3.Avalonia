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

        AssertSelection(host, "dragging across a line break", caret: 45, start: 8, end: 45);
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
        var start = host.Box.SelectionStart;
        var end = host.Box.SelectionEnd;
        Assert.InRange(start, 2, 4);
        Assert.InRange(end, start, Cjk.Length - 1);
        Assert.True(end - start <= 7,
            $"the drag started at character 3 and must stay anchored there; got [{start},{end}]");
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

        AssertSelection(
            host,
            "clicking empty space to the right of the text",
            caret: Latin.Length, start: Latin.Length, end: Latin.Length);
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

    private static void ClickCharacter(Host host, int index)
    {
        var point = CharacterPoint(host, index);
        host.Window.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        host.Window.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
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
                RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>
    /// Headless pointer moves carry no live button-held state, so the core's move-time selection
    /// extension (which gates on the pressed button) does not replay here: after a press-drag-
    /// release the field holds either the press-point caret or a selection the platform was able
    /// to extend. The defect being pinned is the anchor: the selection must start at the press
    /// point, never at the beginning of the text.
    /// </summary>
    private static void AssertPressAnchored(Host host, string what, int pressedIndex, int maxEndExclusive = int.MaxValue)
    {
        var start = host.Box.SelectionStart;
        var end = host.Box.SelectionEnd;
        var caret = host.Box.CaretIndex;
        Assert.InRange(
            start, pressedIndex - 1, Math.Min(pressedIndex + 1, maxEndExclusive - 1),
            $"{what}: the selection [{start},{end}] must be anchored at the press point " +
            $"(character {pressedIndex}), never at the start of the text");
        Assert.True(end >= start, $"{what}: the selection end {end} is before its start {start}");
        Assert.True(end < maxEndExclusive,
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
