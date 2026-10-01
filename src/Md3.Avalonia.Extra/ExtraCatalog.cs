namespace Md3.Avalonia.Extra;

/// <summary>
/// Identifies the extra extension controls package.
/// </summary>
public static class ExtraCatalog
{
    public const string PackageId = "Md3.Avalonia.Extra";
    public static string CorePackageId => typeof(Md3.Avalonia.Controls.MdButton).Assembly.GetName().Name
                                          ?? "Md3.Avalonia";
}

namespace Md3.Avalonia.Extra
{
    /// <summary>Compatibility alias for <see cref="Md3.Avalonia.Extra.ExtraCatalog"/>.</summary>
    public static class EcosystemCatalog
    {
        public const string PackageId = Md3.Avalonia.Extra.ExtraCatalog.PackageId;
        public static string CorePackageId => Md3.Avalonia.Extra.ExtraCatalog.CorePackageId;
    }
}
