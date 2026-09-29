using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A Material list item with headline, supporting text, and leading/trailing slots.</summary>
[PseudoClasses(":has-supporting-text", ":has-leading", ":has-trailing")]
public sealed class MdListItem : ListBoxItem
{
    public static readonly StyledProperty<object?> HeadlineProperty =
        AvaloniaProperty.Register<MdListItem, object?>(nameof(Headline));
    public static readonly StyledProperty<object?> SupportingTextProperty =
        AvaloniaProperty.Register<MdListItem, object?>(nameof(SupportingText));
    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<MdListItem, object?>(nameof(LeadingContent));
    public static readonly StyledProperty<object?> TrailingContentProperty =
        AvaloniaProperty.Register<MdListItem, object?>(nameof(TrailingContent));

    static MdListItem()
    {
        SupportingTextProperty.Changed.AddClassHandler<MdListItem>((item, _) => item.UpdatePseudoClasses());
        LeadingContentProperty.Changed.AddClassHandler<MdListItem>((item, _) => item.UpdatePseudoClasses());
        TrailingContentProperty.Changed.AddClassHandler<MdListItem>((item, _) => item.UpdatePseudoClasses());
    }

    public MdListItem() => UpdatePseudoClasses();

    public object? Headline { get => GetValue(HeadlineProperty); set => SetValue(HeadlineProperty, value); }
    public object? SupportingText { get => GetValue(SupportingTextProperty); set => SetValue(SupportingTextProperty, value); }
    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }
    public object? TrailingContent { get => GetValue(TrailingContentProperty); set => SetValue(TrailingContentProperty, value); }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-supporting-text", SupportingText is not null);
        PseudoClasses.Set(":has-leading", LeadingContent is not null);
        PseudoClasses.Set(":has-trailing", TrailingContent is not null);
    }
}
