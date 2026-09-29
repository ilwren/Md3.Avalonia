namespace Md3.Avalonia.Controls;

/// <summary>
/// A labeled 56 DIP action for MdFabMenu. It retains Button command, keyboard and automation
/// behavior through MdExtendedFloatingActionButton.
/// </summary>
public sealed class MdFabMenuItem : MdExtendedFloatingActionButton
{
    public MdFabMenuItem()
    {
        Size = MdExtendedFabSize.Small;
        ContainerHeight = 56;
        ContainerCornerRadius = new global::Avalonia.CornerRadius(28);
    }

    protected override Type StyleKeyOverride => typeof(MdExtendedFloatingActionButton);
}
