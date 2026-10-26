using System.Text;
using Avalonia.Headless.XUnit;
using Md3.Avalonia.Gallery;
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
}
