using Avalonia.Media.Imaging;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>
/// Writes the documentation preview bitmaps that several render tests produce as a side effect.
/// </summary>
/// <remarks>
/// <para>
/// Those tests exist to prove a control matrix rasterises without throwing, and every assertion
/// they make still runs unconditionally. Encoding the result to PNG and writing it to disk,
/// however, is asset generation rather than verification: eleven call sites were spending the cost
/// of a full-size PNG encode plus file I/O on every CI run, and nothing downstream consumed the
/// files.
/// </para>
/// <para>
/// Set <c>MD3_WRITE_PREVIEWS=1</c> to restore the old behaviour when refreshing documentation
/// images. CI enables it only on <c>main</c>, where the artefacts are actually published.
/// </para>
/// <para>
/// This is not the golden-image harness. Layer L3 in <c>Spec/</c> owns pixel comparison and keeps
/// its own baselines; these previews are never compared against anything.
/// </para>
/// </remarks>
internal static class MdPreviewAssets
{
    public static bool Enabled { get; } =
        string.Equals(Environment.GetEnvironmentVariable("MD3_WRITE_PREVIEWS"), "1", StringComparison.Ordinal);

    public static void Save(Bitmap? frame, string fileName)
    {
        if (!Enabled || frame is null)
        {
            return;
        }

        using var stream = File.Create(Path.Combine(AppContext.BaseDirectory, fileName));
        frame.Save(stream, new PngBitmapEncoderOptions());
    }
}
