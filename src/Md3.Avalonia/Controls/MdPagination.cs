using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>How <see cref="MdPagination"/> renders each page.</summary>
public enum MdPaginationDisplayMode
{
    /// <summary>Numbered page buttons (1, 2, 3...).</summary>
    Numbers,

    /// <summary>Material pager dots: small dots with a wide pill for the current page.</summary>
    Dots,
}

/// <summary>A Material 3 pagination control: previous/next buttons plus numbered or dot page indicators.</summary>
/// <remarks>
/// <para>
/// Extends the built-in <see cref="PipsPager"/> with Material styling and a numbered display mode.
/// <see cref="PipsPager.NumberOfPages"/> and <see cref="PipsPager.SelectedPageIndex"/> (two-way)
/// drive the control; <see cref="PipsPager.MaxVisiblePips"/> caps how many pages are visible at once
/// and the window scrolls to keep the selection in view.
/// </para>
/// <para>
/// In <see cref="MdPaginationDisplayMode.Numbers"/> each page renders as a 32 DIP button carrying its
/// one-based index; the selected page uses the secondary-container highlight. In
/// <see cref="MdPaginationDisplayMode.Dots"/> pages render as 8 DIP dots and the selected page is a
/// 24 DIP pill, matching the Material pager. Arrow keys and Home/End work through the base control.
/// </para>
/// </remarks>
[PseudoClasses(":numbers", ":dots")]
public class MdPagination : PipsPager
{
    public static readonly StyledProperty<MdPaginationDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<MdPagination, MdPaginationDisplayMode>(
            nameof(DisplayMode),
            MdPaginationDisplayMode.Numbers);

    static MdPagination()
    {
        DisplayModeProperty.Changed.AddClassHandler<MdPagination>((pagination, _) =>
            pagination.UpdateDisplayModePseudoClasses());
    }

    public MdPagination()
    {
        UpdateDisplayModePseudoClasses();
    }

    /// <summary>Gets or sets whether pages render as numbers or as Material dots.</summary>
    public MdPaginationDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    private void UpdateDisplayModePseudoClasses()
    {
        PseudoClasses.Set(":numbers", DisplayMode == MdPaginationDisplayMode.Numbers);
        PseudoClasses.Set(":dots", DisplayMode == MdPaginationDisplayMode.Dots);
    }
}
