using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Md3.Avalonia.Extra.Controls;

/// <summary>A clean-room Material avatar composition inspired by common Flutter UI-kit behavior.</summary>
[PseudoClasses(":image", ":initials", ":content")]
public sealed class MdAvatar : ContentControl
{
    public static readonly StyledProperty<IImage?> ImageProperty = AvaloniaProperty.Register<MdAvatar, IImage?>(nameof(Image));
    public static readonly StyledProperty<string?> InitialsProperty = AvaloniaProperty.Register<MdAvatar, string?>(nameof(Initials));
    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<MdAvatar, double>(nameof(Size), 48);
    static MdAvatar()
    {
        ImageProperty.Changed.AddClassHandler<MdAvatar>((avatar, _) => avatar.UpdateState());
        InitialsProperty.Changed.AddClassHandler<MdAvatar>((avatar, _) => avatar.UpdateState());
        ContentProperty.Changed.AddClassHandler<MdAvatar>((avatar, _) => avatar.UpdateState());
        SizeProperty.Changed.AddClassHandler<MdAvatar>((avatar, _) => avatar.ApplySize());
    }
    public MdAvatar() { ApplySize(); UpdateState(); }
    public IImage? Image { get => GetValue(ImageProperty); set => SetValue(ImageProperty, value); }
    public string? Initials { get => GetValue(InitialsProperty); set => SetValue(InitialsProperty, value); }
    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    private void ApplySize() { Width = Math.Max(24, Size); Height = Math.Max(24, Size); }
    private void UpdateState()
    {
        PseudoClasses.Set(":image", Image is not null);
        PseudoClasses.Set(":initials", Image is null && !string.IsNullOrWhiteSpace(Initials));
        PseudoClasses.Set(":content", Image is null && string.IsNullOrWhiteSpace(Initials) && Content is not null);
    }
}

/// <summary>An overlapping avatar collection with native ItemsControl binding and templating.</summary>
public sealed class MdAvatarGroup : ItemsControl
{
    public static readonly StyledProperty<double> OverlapProperty = AvaloniaProperty.Register<MdAvatarGroup, double>(nameof(Overlap), 12);
    static MdAvatarGroup() => OverlapProperty.Changed.AddClassHandler<MdAvatarGroup>((group, _) => group.UpdateMargins());
    public double Overlap { get => GetValue(OverlapProperty); set => SetValue(OverlapProperty, value); }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        container.Margin = new Thickness(index == 0 ? 0 : -Math.Max(0, Overlap), 0, 0, 0);
    }

    private void UpdateMargins()
    {
        var index = 0;
        foreach (var container in GetRealizedContainers())
            container.Margin = new Thickness(index++ == 0 ? 0 : -Math.Max(0, Overlap), 0, 0, 0);
    }
}
