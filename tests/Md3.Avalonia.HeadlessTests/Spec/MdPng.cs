using System.Buffers.Binary;
using System.IO.Compression;

namespace Md3.Avalonia.HeadlessTests.Spec;

/// <summary>An 8-bit RGBA raster, row-major, stride = <c>Width * 4</c>.</summary>
internal sealed class MdRgbaImage(int width, int height, byte[] pixels)
{
    public int Width { get; } = width;

    public int Height { get; } = height;

    public byte[] Pixels { get; } = pixels;

    public long PixelCount => (long)Width * Height;
}

/// <summary>
/// A minimal PNG codec for golden-image comparison.
/// </summary>
/// <remarks>
/// <para>
/// Both the committed baseline and the freshly rendered frame are routed through this decoder, so
/// the comparison never has to reason about platform pixel layout: Avalonia encodes the live frame
/// with its own PNG encoder and this type decodes both sides into the same canonical RGBA order.
/// That removes every assumption about BGRA vs RGBA, premultiplication, stride padding and
/// framebuffer locking from the test.
/// </para>
/// <para>
/// Supports what Skia emits: 8-bit, non-interlaced, colour types 0, 2, 4 and 6. Anything else
/// fails loudly rather than silently mis-decoding.
/// </para>
/// </remarks>
internal static class MdPng
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly uint[] CrcTable = BuildCrcTable();

    public static MdRgbaImage Decode(byte[] data)
    {
        if (data.Length < 8 || !data.AsSpan(0, 8).SequenceEqual(Signature))
        {
            throw new InvalidDataException("Not a PNG stream: signature mismatch.");
        }

        var offset = 8;
        var width = 0;
        var height = 0;
        var bitDepth = 0;
        var colorType = 0;
        var interlace = 0;
        var seenHeader = false;
        using var compressed = new MemoryStream();

        while (offset + 8 <= data.Length)
        {
            var length = (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, 4));
            var type = System.Text.Encoding.ASCII.GetString(data, offset + 4, 4);
            var payload = offset + 8;
            if (length < 0 || payload + length > data.Length)
            {
                throw new InvalidDataException($"Truncated PNG chunk '{type}'.");
            }

            switch (type)
            {
                case "IHDR":
                    width = (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(payload, 4));
                    height = (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(payload + 4, 4));
                    bitDepth = data[payload + 8];
                    colorType = data[payload + 9];
                    interlace = data[payload + 12];
                    seenHeader = true;
                    break;
                case "IDAT":
                    compressed.Write(data, payload, length);
                    break;
                case "IEND":
                    offset = data.Length;
                    break;
            }

            if (offset >= data.Length)
            {
                break;
            }

            offset = payload + length + 4;
        }

        if (!seenHeader)
        {
            throw new InvalidDataException("PNG stream contains no IHDR chunk.");
        }

        if (bitDepth != 8 || interlace != 0 || colorType is not (0 or 2 or 4 or 6))
        {
            throw new NotSupportedException(
                $"Unsupported PNG variant (bitDepth={bitDepth}, colorType={colorType}, interlace={interlace}). " +
                "The golden-image harness only handles the 8-bit non-interlaced forms Skia produces.");
        }

        var channels = colorType switch
        {
            0 => 1,
            2 => 3,
            4 => 2,
            _ => 4
        };

        compressed.Position = 0;
        using var inflate = new ZLibStream(compressed, CompressionMode.Decompress);
        using var raw = new MemoryStream();
        inflate.CopyTo(raw);
        var scanlines = raw.GetBuffer();
        var rawLength = (int)raw.Length;

        var stride = width * channels;
        var expected = (stride + 1) * height;
        if (rawLength < expected)
        {
            throw new InvalidDataException(
                $"PNG payload is {rawLength} bytes but {expected} are required for {width}x{height}.");
        }

        var totalBytes = (long)width * height * 4;
        if (totalBytes <= 0 || totalBytes > int.MaxValue)
        {
            throw new NotSupportedException(
                $"Image dimensions {width}x{height} are out of range for the golden-image harness.");
        }

        var current = new byte[stride];
        var previous = new byte[stride];
        var output = new byte[(int)totalBytes];

        var cursor = 0;
        for (var y = 0; y < height; y++)
        {
            var filter = scanlines[cursor++];
            Buffer.BlockCopy(scanlines, cursor, current, 0, stride);
            cursor += stride;
            Unfilter(filter, current, previous, channels);

            var destination = y * width * 4;
            for (var x = 0; x < width; x++)
            {
                var source = x * channels;
                byte r, g, b, a;
                switch (colorType)
                {
                    case 0:
                        r = g = b = current[source];
                        a = 255;
                        break;
                    case 2:
                        r = current[source];
                        g = current[source + 1];
                        b = current[source + 2];
                        a = 255;
                        break;
                    case 4:
                        r = g = b = current[source];
                        a = current[source + 1];
                        break;
                    default:
                        r = current[source];
                        g = current[source + 1];
                        b = current[source + 2];
                        a = current[source + 3];
                        break;
                }

                var target = destination + (x * 4);
                output[target] = r;
                output[target + 1] = g;
                output[target + 2] = b;
                output[target + 3] = a;
            }

            (previous, current) = (current, previous);
        }

        return new MdRgbaImage(width, height, output);
    }

    public static byte[] Encode(MdRgbaImage image)
    {
        var stride = image.Width * 4;
        var raw = new byte[(stride + 1) * image.Height];
        for (var y = 0; y < image.Height; y++)
        {
            raw[y * (stride + 1)] = 0; // filter: None
            Buffer.BlockCopy(image.Pixels, y * stride, raw, (y * (stride + 1)) + 1, stride);
        }

        byte[] compressed;
        using (var buffer = new MemoryStream())
        {
            using (var deflate = new ZLibStream(buffer, CompressionLevel.Optimal, leaveOpen: true))
            {
                deflate.Write(raw, 0, raw.Length);
            }

            compressed = buffer.ToArray();
        }

        using var output = new MemoryStream();
        output.Write(Signature, 0, Signature.Length);

        var header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), (uint)image.Width);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), (uint)image.Height);
        header[8] = 8;  // bit depth
        header[9] = 6;  // colour type: truecolour with alpha
        header[10] = 0; // compression
        header[11] = 0; // filter
        header[12] = 0; // interlace
        WriteChunk(output, "IHDR", header);
        WriteChunk(output, "IDAT", compressed);
        WriteChunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void Unfilter(byte filter, byte[] current, byte[] previous, int bpp)
    {
        switch (filter)
        {
            case 0:
                break;
            case 1:
                for (var i = bpp; i < current.Length; i++)
                {
                    current[i] = (byte)(current[i] + current[i - bpp]);
                }

                break;
            case 2:
                for (var i = 0; i < current.Length; i++)
                {
                    current[i] = (byte)(current[i] + previous[i]);
                }

                break;
            case 3:
                for (var i = 0; i < current.Length; i++)
                {
                    var left = i >= bpp ? current[i - bpp] : 0;
                    current[i] = (byte)(current[i] + ((left + previous[i]) >> 1));
                }

                break;
            case 4:
                for (var i = 0; i < current.Length; i++)
                {
                    var left = i >= bpp ? current[i - bpp] : 0;
                    var up = previous[i];
                    var upperLeft = i >= bpp ? previous[i - bpp] : 0;
                    current[i] = (byte)(current[i] + Paeth(left, up, upperLeft));
                }

                break;
            default:
                throw new InvalidDataException($"Unknown PNG filter type {filter}.");
        }
    }

    private static int Paeth(int a, int b, int c)
    {
        var p = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);
        if (pa <= pb && pa <= pc)
        {
            return a;
        }

        return pb <= pc ? b : c;
    }

    private static void WriteChunk(Stream stream, string type, byte[] payload)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)payload.Length);
        stream.Write(length);

        var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes, 0, typeBytes.Length);
        stream.Write(payload, 0, payload.Length);

        var crc = 0xFFFFFFFFu;
        crc = UpdateCrc(crc, typeBytes);
        crc = UpdateCrc(crc, payload);
        crc ^= 0xFFFFFFFFu;

        Span<byte> checksum = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksum, crc);
        stream.Write(checksum);
    }

    private static uint UpdateCrc(uint crc, byte[] data)
    {
        foreach (var value in data)
        {
            crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (var n = 0u; n < 256u; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
