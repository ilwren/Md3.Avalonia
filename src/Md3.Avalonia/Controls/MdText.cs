using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Md3.Avalonia.Controls;

/// <summary>A TextBlock with an explicit Material type-scale role.</summary>
[PseudoClasses(":display-large", ":display-medium", ":display-small", ":headline-large", ":headline-medium", ":headline-small", ":title-large", ":title-medium", ":title-small", ":body-large", ":body-medium", ":body-small", ":label-large", ":label-medium", ":label-small")]
public sealed class MdText : TextBlock
{
    public static readonly StyledProperty<MdTextStyle> StyleRoleProperty =
        AvaloniaProperty.Register<MdText, MdTextStyle>(nameof(StyleRole), MdTextStyle.BodyLarge);

    static MdText()
    {
        StyleRoleProperty.Changed.AddClassHandler<MdText>((text, _) => text.UpdatePseudoClasses());
    }

    public MdText() => UpdatePseudoClasses();

    public MdTextStyle StyleRole { get => GetValue(StyleRoleProperty); set => SetValue(StyleRoleProperty, value); }

    private void UpdatePseudoClasses()
    {
        foreach (var value in Enum.GetValues<MdTextStyle>())
        {
            PseudoClasses.Set($":{ToKebabCase(value.ToString())}", value == StyleRole);
        }
    }

    private static string ToKebabCase(string value) =>
        string.Concat(value.Select((character, index) =>
            index > 0 && char.IsUpper(character) ? $"-{char.ToLowerInvariant(character)}" : char.ToLowerInvariant(character).ToString()));
}
