using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace Md3.Avalonia.Controls;

public enum MdScrollbarThumbVisibility { Auto, Always, Hidden }

/// <summary>A scoped interactive Material scrollbar with Flutter-style thumb and track policies.</summary>
[PseudoClasses(":thumb-auto", ":thumb-always", ":thumb-hidden", ":track-visible")]
public sealed class MdScrollBar : ScrollBar
{
    public static readonly StyledProperty<MdScrollbarThumbVisibility> ThumbVisibilityProperty =
        AvaloniaProperty.Register<MdScrollBar, MdScrollbarThumbVisibility>(nameof(ThumbVisibility));
    public static readonly StyledProperty<bool> IsTrackVisibleProperty =
        AvaloniaProperty.Register<MdScrollBar, bool>(nameof(IsTrackVisible));
    public static readonly StyledProperty<bool> IsInteractiveProperty =
        AvaloniaProperty.Register<MdScrollBar, bool>(nameof(IsInteractive), true);

    static MdScrollBar()
    {
        ThumbVisibilityProperty.Changed.AddClassHandler<MdScrollBar>((bar, _) => bar.UpdatePolicies());
        IsTrackVisibleProperty.Changed.AddClassHandler<MdScrollBar>((bar, _) => bar.UpdatePolicies());
        IsInteractiveProperty.Changed.AddClassHandler<MdScrollBar>((bar, _) => bar.UpdatePolicies());
    }

    public MdScrollBar() => UpdatePolicies();

    public MdScrollbarThumbVisibility ThumbVisibility { get => GetValue(ThumbVisibilityProperty); set => SetValue(ThumbVisibilityProperty, value); }
    public bool IsTrackVisible { get => GetValue(IsTrackVisibleProperty); set => SetValue(IsTrackVisibleProperty, value); }
    public bool IsInteractive { get => GetValue(IsInteractiveProperty); set => SetValue(IsInteractiveProperty, value); }

    private void UpdatePolicies()
    {
        PseudoClasses.Set(":thumb-auto", ThumbVisibility == MdScrollbarThumbVisibility.Auto);
        PseudoClasses.Set(":thumb-always", ThumbVisibility == MdScrollbarThumbVisibility.Always);
        PseudoClasses.Set(":thumb-hidden", ThumbVisibility == MdScrollbarThumbVisibility.Hidden);
        PseudoClasses.Set(":track-visible", IsTrackVisible);
        SetCurrentValue(AllowAutoHideProperty, ThumbVisibility == MdScrollbarThumbVisibility.Auto);
        SetCurrentValue(IsHitTestVisibleProperty, IsInteractive);
    }
}
