using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Md3.Avalonia.Localization;

namespace Md3.Avalonia.Controls;

/// <summary>
/// A Material 3 splitter that reuses Avalonia's grid-resize engine while providing a larger
/// interaction track, orientation-aware visuals, localized automation text and pane equalization.
/// </summary>
/// <remarks>
/// Place the splitter in its own <c>Auto</c> row or column between two star-sized definitions and
/// leave <see cref="GridSplitter.ResizeBehavior"/> at
/// <see cref="GridResizeBehavior.PreviousAndNext"/> (or its alignment-derived equivalent) for the
/// common two-pane layout. Dragging, preview mode, keyboard arrows, min/max constraints and Escape
/// cancellation are inherited from Avalonia's <see cref="GridSplitter"/>.
/// </remarks>
[PseudoClasses(":columns", ":rows", ":dragging")]
public class MdGridSplitter : GridSplitter
{
    /// <summary>Defines the <see cref="ShowHandle"/> property.</summary>
    public static readonly StyledProperty<bool> ShowHandleProperty =
        AvaloniaProperty.Register<MdGridSplitter, bool>(nameof(ShowHandle), true);

    /// <summary>Defines the <see cref="ResetOnDoubleTapped"/> property.</summary>
    public static readonly StyledProperty<bool> ResetOnDoubleTappedProperty =
        AvaloniaProperty.Register<MdGridSplitter, bool>(nameof(ResetOnDoubleTapped), true);

    private string? _ownedAutomationName;

    static MdGridSplitter()
    {
        ResizeDirectionProperty.Changed.AddClassHandler<MdGridSplitter>((splitter, _) => splitter.UpdateDirectionState());
        HorizontalAlignmentProperty.Changed.AddClassHandler<MdGridSplitter>((splitter, _) => splitter.UpdateDirectionState());
        VerticalAlignmentProperty.Changed.AddClassHandler<MdGridSplitter>((splitter, _) => splitter.UpdateDirectionState());
        BoundsProperty.Changed.AddClassHandler<MdGridSplitter>((splitter, _) => splitter.UpdateDirectionState());
        MdLocalization.CultureProperty.Changed.AddClassHandler<MdGridSplitter>((splitter, _) => splitter.UpdateAutomationName());
    }

    /// <summary>Initializes a new Material grid splitter.</summary>
    public MdGridSplitter()
    {
        Focusable = true;
        DragStarted += (_, _) =>
        {
            PseudoClasses.Set(":dragging", true);
            UpdateDirectionState();
        };
        DragCompleted += (_, _) => PseudoClasses.Set(":dragging", false);
        DoubleTapped += OnDoubleTapped;
        UpdateDirectionState();
    }

    protected override Type StyleKeyOverride => typeof(MdGridSplitter);

    /// <summary>Whether the rounded grip is shown inside the divider track.</summary>
    public bool ShowHandle
    {
        get => GetValue(ShowHandleProperty);
        set => SetValue(ShowHandleProperty, value);
    }

    /// <summary>Whether a double-tap equalizes the two controlled star-sized definitions.</summary>
    public bool ResetOnDoubleTapped
    {
        get => GetValue(ResetOnDoubleTappedProperty);
        set => SetValue(ResetOnDoubleTappedProperty, value);
    }

    /// <summary>Raised after <see cref="ResetToEqual"/> successfully equalizes a compatible pair.</summary>
    public event EventHandler? SplitReset;

    /// <summary>
    /// Equalizes the two star-sized definitions controlled by this splitter while honoring both
    /// definitions' minimum and maximum sizes.
    /// </summary>
    /// <returns><see langword="true"/> when a compatible pair was reset; otherwise <see langword="false"/>.</returns>
    public bool ResetToEqual()
    {
        if (Parent is not Grid grid) return false;

        var direction = GetEffectiveResizeDirection();
        var span = direction == GridResizeDirection.Columns
            ? Grid.GetColumnSpan(this)
            : Grid.GetRowSpan(this);
        if (span != 1) return false;

        var splitterIndex = direction == GridResizeDirection.Columns
            ? Grid.GetColumn(this)
            : Grid.GetRow(this);
        var behavior = GetEffectiveResizeBehavior(direction);
        if (!TryGetDefinitionIndices(
                splitterIndex,
                behavior,
                direction == GridResizeDirection.Columns
                    ? grid.ColumnDefinitions.Count
                    : grid.RowDefinitions.Count,
                out var firstIndex,
                out var secondIndex))
        {
            return false;
        }

        var changed = direction == GridResizeDirection.Columns
            ? TryEqualizeColumns(grid, firstIndex, secondIndex)
            : TryEqualizeRows(grid, firstIndex, secondIndex);
        if (changed) SplitReset?.Invoke(this, EventArgs.Empty);
        return changed;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        UpdateDirectionState();
        return result;
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (!ResetOnDoubleTapped || !ResetToEqual()) return;
        e.Handled = true;
    }

    private GridResizeDirection GetEffectiveResizeDirection()
    {
        if (ResizeDirection != GridResizeDirection.Auto) return ResizeDirection;
        if (HorizontalAlignment != HorizontalAlignment.Stretch) return GridResizeDirection.Columns;
        if (VerticalAlignment != VerticalAlignment.Stretch) return GridResizeDirection.Rows;
        return Bounds.Width <= Bounds.Height ? GridResizeDirection.Columns : GridResizeDirection.Rows;
    }

    private GridResizeBehavior GetEffectiveResizeBehavior(GridResizeDirection direction)
    {
        if (ResizeBehavior != GridResizeBehavior.BasedOnAlignment) return ResizeBehavior;

        return direction switch
        {
            GridResizeDirection.Columns when HorizontalAlignment == HorizontalAlignment.Left =>
                GridResizeBehavior.PreviousAndCurrent,
            GridResizeDirection.Columns when HorizontalAlignment == HorizontalAlignment.Right =>
                GridResizeBehavior.CurrentAndNext,
            GridResizeDirection.Rows when VerticalAlignment == VerticalAlignment.Top =>
                GridResizeBehavior.PreviousAndCurrent,
            GridResizeDirection.Rows when VerticalAlignment == VerticalAlignment.Bottom =>
                GridResizeBehavior.CurrentAndNext,
            _ => GridResizeBehavior.PreviousAndNext,
        };
    }

    private static bool TryGetDefinitionIndices(
        int splitterIndex,
        GridResizeBehavior behavior,
        int count,
        out int firstIndex,
        out int secondIndex)
    {
        (firstIndex, secondIndex) = behavior switch
        {
            GridResizeBehavior.PreviousAndCurrent => (splitterIndex - 1, splitterIndex),
            GridResizeBehavior.CurrentAndNext => (splitterIndex, splitterIndex + 1),
            _ => (splitterIndex - 1, splitterIndex + 1),
        };
        return firstIndex >= 0 && secondIndex < count && firstIndex != secondIndex;
    }

    private static bool TryEqualizeColumns(Grid grid, int firstIndex, int secondIndex)
    {
        var first = grid.ColumnDefinitions[firstIndex];
        var second = grid.ColumnDefinitions[secondIndex];
        if (!first.Width.IsStar || !second.Width.IsStar ||
            !TryCalculateEqualLengths(
                first.ActualWidth + second.ActualWidth,
                first.MinWidth,
                first.MaxWidth,
                second.MinWidth,
                second.MaxWidth,
                out var firstLength,
                out var secondLength))
        {
            return false;
        }

        for (var index = 0; index < grid.ColumnDefinitions.Count; index++)
        {
            var definition = grid.ColumnDefinitions[index];
            if (!definition.Width.IsStar) continue;
            var length = index == firstIndex
                ? firstLength
                : index == secondIndex
                    ? secondLength
                    : definition.ActualWidth;
            definition.SetCurrentValue(
                ColumnDefinition.WidthProperty,
                new GridLength(Math.Max(length, .0001), GridUnitType.Star));
        }
        return true;
    }

    private static bool TryEqualizeRows(Grid grid, int firstIndex, int secondIndex)
    {
        var first = grid.RowDefinitions[firstIndex];
        var second = grid.RowDefinitions[secondIndex];
        if (!first.Height.IsStar || !second.Height.IsStar ||
            !TryCalculateEqualLengths(
                first.ActualHeight + second.ActualHeight,
                first.MinHeight,
                first.MaxHeight,
                second.MinHeight,
                second.MaxHeight,
                out var firstLength,
                out var secondLength))
        {
            return false;
        }

        for (var index = 0; index < grid.RowDefinitions.Count; index++)
        {
            var definition = grid.RowDefinitions[index];
            if (!definition.Height.IsStar) continue;
            var length = index == firstIndex
                ? firstLength
                : index == secondIndex
                    ? secondLength
                    : definition.ActualHeight;
            definition.SetCurrentValue(
                RowDefinition.HeightProperty,
                new GridLength(Math.Max(length, .0001), GridUnitType.Star));
        }
        return true;
    }

    private static bool TryCalculateEqualLengths(
        double totalLength,
        double firstMinimum,
        double firstMaximum,
        double secondMinimum,
        double secondMaximum,
        out double firstLength,
        out double secondLength)
    {
        firstLength = 0;
        secondLength = 0;
        if (!double.IsFinite(totalLength) || totalLength <= 0) return false;

        var minimumFirst = Math.Max(firstMinimum, totalLength - secondMaximum);
        var maximumFirst = Math.Min(firstMaximum, totalLength - secondMinimum);
        if (minimumFirst > maximumFirst) return false;

        firstLength = Math.Clamp(totalLength / 2, minimumFirst, maximumFirst);
        secondLength = totalLength - firstLength;
        return double.IsFinite(firstLength) && double.IsFinite(secondLength) &&
               firstLength >= 0 && secondLength >= 0;
    }

    private void UpdateDirectionState()
    {
        var direction = GetEffectiveResizeDirection();
        PseudoClasses.Set(":columns", direction == GridResizeDirection.Columns);
        PseudoClasses.Set(":rows", direction == GridResizeDirection.Rows);
        UpdateAutomationName();
    }

    private void UpdateAutomationName()
    {
        var currentName = AutomationProperties.GetName(this);
        if (!string.IsNullOrWhiteSpace(currentName) && currentName != _ownedAutomationName) return;

        _ownedAutomationName = MdLocalization.GetString(
            GetEffectiveResizeDirection() == GridResizeDirection.Columns
                ? "GridSplitterResizeColumns"
                : "GridSplitterResizeRows",
            this);
        AutomationProperties.SetName(this, _ownedAutomationName);
    }
}
