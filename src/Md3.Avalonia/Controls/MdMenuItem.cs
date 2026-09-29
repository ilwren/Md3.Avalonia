using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A command-capable Material menu row with leading, label and trailing slots.</summary>
[PseudoClasses(":selected", ":has-leading", ":has-trailing")]
public class MdMenuItem : Button
{
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<MdMenuItem, object?>(nameof(LeadingContent));
    public static readonly StyledProperty<object?> TrailingContentProperty =
        AvaloniaProperty.Register<MdMenuItem, object?>(nameof(TrailingContent));
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<MdMenuItem, bool>(nameof(IsSelected));

    static MdMenuItem()
    {
        LeadingContentProperty.Changed.AddClassHandler<MdMenuItem>((item, _) => item.UpdatePseudoClasses());
        TrailingContentProperty.Changed.AddClassHandler<MdMenuItem>((item, _) => item.UpdatePseudoClasses());
        IsSelectedProperty.Changed.AddClassHandler<MdMenuItem>((item, _) => item.UpdatePseudoClasses());
    }

    public MdMenuItem() => UpdatePseudoClasses();

    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }
    public object? TrailingContent { get => GetValue(TrailingContentProperty); set => SetValue(TrailingContentProperty, value); }
    public bool IsSelected { get => GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":selected", IsSelected);
        PseudoClasses.Set(":has-leading", LeadingContent is not null);
        PseudoClasses.Set(":has-trailing", TrailingContent is not null);
    }
}
