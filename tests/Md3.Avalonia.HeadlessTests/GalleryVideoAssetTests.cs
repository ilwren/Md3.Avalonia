using System.Text;
using Avalonia.Headless.XUnit;
using Md3.Avalonia.Gallery;
using Md3.Avalonia.HeadlessTests.Spec;
using Xunit;

namespace Md3.Avalonia.HeadlessTests;

public sealed class GalleryVideoAssetTests
{
    [AvaloniaFact]
    public void Bundled_Local_Video_Is_Extractable_As_An_Mp4_File()
    {
        var uri = GalleryVideoBackend.ExtractBundledSample();

        Assert.True(uri.IsFile);
        Assert.True(File.Exists(uri.LocalPath));
        using var stream = File.OpenRead(uri.LocalPath);
        Assert.True(stream.Length > 100_000, "the sample should contain real encoded media, not a placeholder");
        Span<byte> header = stackalloc byte[12];
        Assert.Equal(header.Length, stream.Read(header));
        Assert.Equal("ftyp", Encoding.ASCII.GetString(header[4..8]));
    }

    [Fact]
    public void LibVlc_Launch_Helpers_Discover_Native_Files_Without_Installing_Them()
    {
        var scripts = Path.Combine(MdSpecPaths.RequireRoot(), "scripts");
        var shell = File.ReadAllText(Path.Combine(scripts, "run-gallery-libvlc.sh"));
        var powerShell = File.ReadAllText(Path.Combine(scripts, "run-gallery-libvlc.ps1"));

        Assert.Contains("--check-only", shell, StringComparison.Ordinal);
        Assert.Contains("VLC_PLUGIN_PATH", shell, StringComparison.Ordinal);
        Assert.Contains("-p:PublishAot=false", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("sudo ", shell, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CheckOnly", powerShell, StringComparison.Ordinal);
        Assert.Contains("VLC_PLUGIN_PATH", powerShell, StringComparison.Ordinal);
        Assert.Contains("-p:PublishAot=false", powerShell, StringComparison.Ordinal);
        Assert.DoesNotContain("Invoke-WebRequest", powerShell, StringComparison.OrdinalIgnoreCase);
    }
}
