using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A Material 3 basic or full-screen dialog surface hosted by <see cref="MdDialogHost"/>.</summary>
[PseudoClasses(":basic", ":full-screen", ":has-icon", ":has-headline", ":has-actions")]
public sealed class MdDialog : ContentControl
{
    public static readonly StyledProperty<MdDialogVariant> VariantProperty =
        AvaloniaProperty.Register<MdDialog, MdDialogVariant>(nameof(Variant));
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MdDialog, object?>(nameof(Icon));
    public static readonly StyledProperty<object?> HeadlineProperty =
        AvaloniaProperty.Register<MdDialog, object?>(nameof(Headline));
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<MdDialog, object?>(nameof(Actions));

    static MdDialog()
    {
        VariantProperty.Changed.AddClassHandler<MdDialog>((dialog, _) => dialog.UpdatePseudoClasses());
        IconProperty.Changed.AddClassHandler<MdDialog>((dialog, _) => dialog.UpdatePseudoClasses());
        HeadlineProperty.Changed.AddClassHandler<MdDialog>((dialog, _) => dialog.UpdatePseudoClasses());
        ActionsProperty.Changed.AddClassHandler<MdDialog>((dialog, _) => dialog.UpdatePseudoClasses());
    }

    public MdDialog() => UpdatePseudoClasses();

    public MdDialogVariant Variant { get => GetValue(VariantProperty); set => SetValue(VariantProperty, value); }
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public object? Headline { get => GetValue(HeadlineProperty); set => SetValue(HeadlineProperty, value); }
    public object? Actions { get => GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }

    protected override AutomationPeer OnCreateAutomationPeer() => new MdDialogAutomationPeer(this);

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":basic", Variant == MdDialogVariant.Basic);
        PseudoClasses.Set(":full-screen", Variant == MdDialogVariant.FullScreen);
        PseudoClasses.Set(":has-icon", Icon is not null);
        PseudoClasses.Set(":has-headline", Headline is not null);
        PseudoClasses.Set(":has-actions", Actions is not null);
    }
}

internal sealed class MdDialogAutomationPeer(MdDialog owner) : ContentControlAutomationPeer(owner)
{
    private MdDialog DialogOwner => (MdDialog)Owner;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Window;
    protected override bool IsControlElementCore() => true;
    protected override bool IsContentElementCore() => true;

    protected override string? GetNameCore()
    {
        var explicitName = global::Avalonia.Automation.AutomationProperties.GetName(DialogOwner);
        if (!string.IsNullOrWhiteSpace(explicitName)) return explicitName;
        return DialogOwner.Headline switch
        {
            string headline when !string.IsNullOrWhiteSpace(headline) => headline,
            TextBlock textBlock when !string.IsNullOrWhiteSpace(textBlock.Text) => textBlock.Text,
            _ => base.GetNameCore()
        };
    }
}
