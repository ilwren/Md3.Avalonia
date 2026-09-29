using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Layout;

namespace Md3.Avalonia.Controls;

/// <summary>A docked or floating Material toolbar for frequently used page actions.</summary>
[PseudoClasses(":docked", ":floating", ":standard", ":vibrant", ":horizontal", ":vertical", ":has-leading", ":has-trailing")]
public sealed class MdToolbar : ItemsControl
{
    public static readonly StyledProperty<MdToolbarMode> ModeProperty =
        AvaloniaProperty.Register<MdToolbar, MdToolbarMode>(nameof(Mode), MdToolbarMode.Floating);
    public static readonly StyledProperty<MdToolbarVariant> VariantProperty =
        AvaloniaProperty.Register<MdToolbar, MdToolbarVariant>(nameof(Variant));
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MdToolbar, Orientation>(nameof(Orientation));
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<MdToolbar, object?>(nameof(LeadingContent));
    public static readonly StyledProperty<object?> TrailingContentProperty =
        AvaloniaProperty.Register<MdToolbar, object?>(nameof(TrailingContent));

    static MdToolbar()
    {
        ModeProperty.Changed.AddClassHandler<MdToolbar>((toolbar, _) => toolbar.UpdatePseudoClasses());
        VariantProperty.Changed.AddClassHandler<MdToolbar>((toolbar, _) => toolbar.UpdatePseudoClasses());
        OrientationProperty.Changed.AddClassHandler<MdToolbar>((toolbar, _) => toolbar.UpdatePseudoClasses());
        LeadingContentProperty.Changed.AddClassHandler<MdToolbar>((toolbar, _) => toolbar.UpdatePseudoClasses());
        TrailingContentProperty.Changed.AddClassHandler<MdToolbar>((toolbar, _) => toolbar.UpdatePseudoClasses());
    }

    public MdToolbar() => UpdatePseudoClasses();

    public MdToolbarMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public MdToolbarVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public object? LeadingContent
    {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
    }

    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":docked", Mode == MdToolbarMode.Docked);
        PseudoClasses.Set(":floating", Mode == MdToolbarMode.Floating);
        PseudoClasses.Set(":standard", Variant == MdToolbarVariant.Standard);
        PseudoClasses.Set(":vibrant", Variant == MdToolbarVariant.Vibrant);
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":has-leading", LeadingContent is not null);
        PseudoClasses.Set(":has-trailing", TrailingContent is not null);
    }
}
