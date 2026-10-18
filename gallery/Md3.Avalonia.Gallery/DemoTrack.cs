using System;
using System.Buffers.Binary;
using System.IO;
using Avalonia.Platform;

namespace Md3.Avalonia.Gallery;

/// <summary>
/// Reads the gallery's bundled demo track into samples the waveform can be drawn from.
/// </summary>
/// <remarks>
/// <para>
/// This exists so the waveform demo shows a real recording rather than a shape that was written to
/// look like one. It is deliberately a plain PCM reader rather than a codec: the bundled file is
/// uncompressed WAV, so the gallery needs no audio dependency, nothing platform-specific, and
/// nothing that would be a problem under native AOT. An MP3 here would have meant a decoder
/// dependency in the gallery for the sake of a demo.
/// </para>
/// <para>
/// Nothing in this path touches an audio device. "Playback" on the waveform page is a timer walking
/// the playhead through the decoded samples, so the demo runs on every platform the gallery builds
/// for, including ones with no output device at all.
/// </para>
/// <para>
/// Parsing happens over a byte span with an explicit cursor rather than through a
/// <see cref="BinaryReader"/>: the chunk walk depends on knowing exactly where the previous chunk
/// ended, and a reader that buffers ahead of its own logical position makes that position mean
/// something other than what the loop assumes.
/// </para>
/// </remarks>
public static class DemoTrack
{
    public const string AssetUri = "avares://Md3.Avalonia.Gallery/Assets/Media/demo-track.wav";

    /// <summary>The decoded track: mono samples in -1..1, and the rate they were stored at.</summary>
    public readonly record struct Track(float[] Samples, int SampleRate)
    {
        public TimeSpan Duration => SampleRate > 0 ? TimeSpan.FromSeconds((double)Samples.Length / SampleRate) : TimeSpan.Zero;
    }

    public static Track Load(string uri = AssetUri)
    {
        using var stream = AssetLoader.Open(new Uri(uri));
        return Load(stream);
    }

    public static Track Load(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return Parse(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));
    }

    private static Track Parse(ReadOnlySpan<byte> wave)
    {
        if (wave.Length < 12 || !Tag(wave, 0, "RIFF") || !Tag(wave, 8, "WAVE"))
            throw new InvalidDataException("the stream is not a RIFF/WAVE file");

        var channels = 0;
        var sampleRate = 0;
        var bitsPerSample = 0;
        var format = 0;
        ReadOnlySpan<byte> data = default;

        // Chunks are walked rather than read at fixed offsets: a WAVE file may carry LIST, fact or
        // bext chunks in any order, and a reader that assumes one layout fails on files it should
        // accept.
        for (var cursor = 12; cursor + 8 <= wave.Length;)
        {
            var size = (int)BinaryPrimitives.ReadUInt32LittleEndian(wave.Slice(cursor + 4, 4));
            var body = cursor + 8;
            if (size < 0 || body + size > wave.Length)
                throw new InvalidDataException("the WAVE stream has a corrupt chunk size");

            var chunk = wave.Slice(body, size);
            if (Tag(wave, cursor, "fmt "))
            {
                if (size < 16) throw new InvalidDataException("the WAVE stream has a truncated fmt chunk");
                format = BinaryPrimitives.ReadUInt16LittleEndian(chunk);
                channels = BinaryPrimitives.ReadUInt16LittleEndian(chunk.Slice(2));
                sampleRate = (int)BinaryPrimitives.ReadUInt32LittleEndian(chunk.Slice(4));
                bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(chunk.Slice(14));
            }
            else if (Tag(wave, cursor, "data"))
            {
                data = chunk;
            }

            // Chunks are word aligned, so an odd-sized one is followed by a pad byte.
            cursor = body + size + size % 2;
        }

        if (format != 1)
            throw new InvalidDataException($"the WAVE stream is audio format {format}; only uncompressed PCM (1) is supported");
        if (data.IsEmpty || channels < 1 || sampleRate < 1)
            throw new InvalidDataException("the WAVE stream has no usable fmt/data chunks");
        if (bitsPerSample != 16)
            throw new InvalidDataException($"the WAVE stream is {bitsPerSample}-bit; only 16-bit PCM is supported");

        var frames = data.Length / (2 * channels);
        var samples = new float[frames];
        for (var frame = 0; frame < frames; frame++)
        {
            var sum = 0;
            for (var channel = 0; channel < channels; channel++)
            {
                sum += BinaryPrimitives.ReadInt16LittleEndian(data.Slice((frame * channels + channel) * 2, 2));
            }
            // Mono downmix by averaging: the bundled track is already mono, and averaging keeps a
            // stereo file at the same perceived level instead of doubling it.
            samples[frame] = sum / (float)channels / short.MaxValue;
        }

        return new Track(samples, sampleRate);
    }

    private static bool Tag(ReadOnlySpan<byte> wave, int offset, string expected) =>
        offset + 4 <= wave.Length
        && wave[offset] == (byte)expected[0]
        && wave[offset + 1] == (byte)expected[1]
        && wave[offset + 2] == (byte)expected[2]
        && wave[offset + 3] == (byte)expected[3];
}
