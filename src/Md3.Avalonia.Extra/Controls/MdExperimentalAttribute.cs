namespace Md3.Avalonia.Extra.Controls;

/// <summary>Marks an API whose behavior is intentionally preview-only and not part of parity claims.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MdExperimentalAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
