namespace Md3.Avalonia.Ecosystem;

/// <summary>
/// Identifies the optional ecosystem package. Controls are added only after the clean-room and
/// license gates in docs/FLUTTER_COMPONENT_PARITY.md have been completed.
/// </summary>
public static class EcosystemCatalog
{
    public const string PackageId = "Md3.Avalonia.Ecosystem";
    public static string CorePackageId => typeof(Md3.Avalonia.Controls.MdButton).Assembly.GetName().Name
                                          ?? "Md3.Avalonia";
}
