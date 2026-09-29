namespace Md3.Avalonia.Controls;

/// <summary>Material assist chip with the shared MdChip anatomy and command semantics.</summary>
public sealed class MdAssistChip : MdChip
{
    public MdAssistChip() => Variant = MdChipVariant.Assist;
    protected override Type StyleKeyOverride => typeof(MdChip);
}

/// <summary>Material filter chip whose IsChecked property is the selection state.</summary>
public sealed class MdFilterChip : MdChip
{
    public MdFilterChip() => Variant = MdChipVariant.Filter;
    protected override Type StyleKeyOverride => typeof(MdChip);
}

/// <summary>Material input chip with RemoveCommand and RemoveRequested support.</summary>
public sealed class MdInputChip : MdChip
{
    public MdInputChip()
    {
        Variant = MdChipVariant.Input;
        IsRemovable = true;
    }
    protected override Type StyleKeyOverride => typeof(MdChip);
}

/// <summary>Material suggestion chip with the shared MdChip command semantics.</summary>
public sealed class MdSuggestionChip : MdChip
{
    public MdSuggestionChip() => Variant = MdChipVariant.Suggestion;
    protected override Type StyleKeyOverride => typeof(MdChip);
}
