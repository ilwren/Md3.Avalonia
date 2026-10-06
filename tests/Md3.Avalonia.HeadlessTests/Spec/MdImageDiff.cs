namespace Md3.Avalonia.HeadlessTests.Spec;

internal readonly record struct MdImageDiffResult(
    bool SizeMatches,
    long ChangedPixels,
    long TotalPixels,
    int MaxChannelDelta)
{
    public double ChangedRatio => TotalPixels == 0 ? 1 : (double)ChangedPixels / TotalPixels;
}

/// <summary>Per-pixel comparison with a per-channel tolerance, plus a human-readable diff raster.</summary>
internal static class MdImageDiff
{
    public static MdImageDiffResult Compare(MdRgbaImage baseline, MdRgbaImage candidate, int perChannelTolerance)
    {
        if (baseline.Width != candidate.Width || baseline.Height != candidate.Height)
        {
            return new MdImageDiffResult(false, 0, baseline.PixelCount, 255);
        }

        long changed = 0;
        var worst = 0;
        var a = baseline.Pixels;
        var b = candidate.Pixels;

        for (var i = 0; i < a.Length; i += 4)
        {
            var delta = 0;
            for (var c = 0; c < 4; c++)
            {
                delta = Math.Max(delta, Math.Abs(a[i + c] - b[i + c]));
            }

            if (delta > worst)
            {
                worst = delta;
            }

            if (delta > perChannelTolerance)
            {
                changed++;
            }
        }

        return new MdImageDiffResult(true, changed, baseline.PixelCount, worst);
    }

    /// <summary>
    /// Renders the baseline at reduced contrast with every out-of-tolerance pixel punched out in
    /// magenta, so a reviewer can see <em>where</em> a golden moved without flicking between files.
    /// </summary>
    public static MdRgbaImage Visualise(MdRgbaImage baseline, MdRgbaImage candidate, int perChannelTolerance)
    {
        var width = Math.Min(baseline.Width, candidate.Width);
        var height = Math.Min(baseline.Height, candidate.Height);
        var output = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var target = ((y * width) + x) * 4;
                var baseIndex = ((y * baseline.Width) + x) * 4;
                var candidateIndex = ((y * candidate.Width) + x) * 4;

                var delta = 0;
                for (var c = 0; c < 4; c++)
                {
                    delta = Math.Max(delta, Math.Abs(baseline.Pixels[baseIndex + c] - candidate.Pixels[candidateIndex + c]));
                }

                if (delta > perChannelTolerance)
                {
                    output[target] = 255;
                    output[target + 1] = 0;
                    output[target + 2] = 255;
                    output[target + 3] = 255;
                }
                else
                {
                    output[target] = Fade(baseline.Pixels[baseIndex]);
                    output[target + 1] = Fade(baseline.Pixels[baseIndex + 1]);
                    output[target + 2] = Fade(baseline.Pixels[baseIndex + 2]);
                    output[target + 3] = 255;
                }
            }
        }

        return new MdRgbaImage(width, height, output);
    }

    private static byte Fade(byte value) => (byte)(200 + (value * 55 / 255));
}
