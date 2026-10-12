using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace Md3.Avalonia.Gallery.Media;

/// <summary>
/// Gallery-only LibVLC video sink. Decoded frames are copied into an Avalonia bitmap so the
/// MdVideoPlayerControls remain ordinary Avalonia visuals and are not tied to a native VLC view.
/// </summary>
public sealed class VlcVideoSurface : Control, IDisposable
{
    private const int PictureBufferCount = 3;
    private const uint MaximumOutputWidth = 1920;
    private const uint MaximumOutputHeight = 1080;
    private static readonly byte[] Rv32Chroma = [(byte)'R', (byte)'V', (byte)'3', (byte)'2'];

    private readonly object _sync = new();
    private IntPtr[] _pictureBuffers = [];
    private byte[]? _pendingFrame;
    private int _frameWidth;
    private int _frameHeight;
    private int _framePitch;
    private int _nextBuffer;
    private int _framePending;
    private int _disposed;
    private WriteableBitmap? _bitmap;

    /// <summary>Attach this sink to a LibVLCSharp media player before opening video media.</summary>
    public void Attach(MediaPlayer player)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        player.SetVideoFormatCallbacks(ConfigureVideo, CleanupVideo);
        player.SetVideoCallbacks(LockVideo, UnlockVideo, DisplayVideo);
    }

    private uint ConfigureVideo(
        ref IntPtr opaque,
        IntPtr chroma,
        ref uint width,
        ref uint height,
        ref uint pitches,
        ref uint lines)
    {
        if (Volatile.Read(ref _disposed) != 0 || chroma == IntPtr.Zero || width == 0 || height == 0)
            return 0;

        // Keep the Gallery surface responsive and bound the three native frame buffers on phones
        // and high-resolution desktop media. LibVLC scales into the requested output dimensions.
        if (width > MaximumOutputWidth || height > MaximumOutputHeight)
        {
            var scale = Math.Min(MaximumOutputWidth / (double)width, MaximumOutputHeight / (double)height);
            width = Math.Max(1, (uint)Math.Floor(width * scale));
            height = Math.Max(1, (uint)Math.Floor(height * scale));
        }

        int pitch;
        int frameSize;
        try
        {
            pitch = checked((int)width * 4);
            frameSize = checked(pitch * (int)height);
        }
        catch (OverflowException)
        {
            return 0;
        }

        byte[] pendingFrame;
        try
        {
            pendingFrame = new byte[frameSize];
        }
        catch (OutOfMemoryException)
        {
            return 0;
        }

        var buffers = new IntPtr[PictureBufferCount];
        try
        {
            for (var i = 0; i < buffers.Length; i++)
                buffers[i] = Marshal.AllocHGlobal(frameSize);
        }
        catch (OutOfMemoryException)
        {
            FreeBuffers(buffers);
            return 0;
        }

        lock (_sync)
        {
            ReleasePictureBuffers();
            _pictureBuffers = buffers;
            _frameWidth = (int)width;
            _frameHeight = (int)height;
            _framePitch = pitch;
            _pendingFrame = pendingFrame;
            _nextBuffer = 0;
        }

        Marshal.Copy(Rv32Chroma, 0, chroma, Rv32Chroma.Length);
        pitches = (uint)pitch;
        lines = height;
        return PictureBufferCount;
    }

    private IntPtr LockVideo(IntPtr opaque, IntPtr planes)
    {
        if (planes == IntPtr.Zero)
            return IntPtr.Zero;

        lock (_sync)
        {
            if (_pictureBuffers.Length == 0)
                return IntPtr.Zero;

            var index = (int)((uint)Interlocked.Increment(ref _nextBuffer) % (uint)_pictureBuffers.Length);
            var buffer = _pictureBuffers[index];
            Marshal.WriteIntPtr(planes, buffer);
            return buffer;
        }
    }

    private static void UnlockVideo(IntPtr opaque, IntPtr picture, IntPtr planes)
    {
        // LibVLC has finished writing this buffer; the display callback consumes it next.
    }

    private void DisplayVideo(IntPtr opaque, IntPtr picture)
    {
        if (picture == IntPtr.Zero || Volatile.Read(ref _disposed) != 0 ||
            Interlocked.CompareExchange(ref _framePending, 1, 0) != 0)
            return;

        try
        {
            lock (_sync)
            {
                if (_pendingFrame is null)
                {
                    Volatile.Write(ref _framePending, 0);
                    return;
                }

                Marshal.Copy(picture, _pendingFrame, 0, _pendingFrame.Length);
            }

            Dispatcher.UIThread.Post(ApplyPendingFrame, DispatcherPriority.Render);
        }
        catch
        {
            // Never allow managed UI teardown or frame-copy failures to escape into the VLC callback.
            Volatile.Write(ref _framePending, 0);
        }
    }

    private void ApplyPendingFrame()
    {
        try
        {
            if (Volatile.Read(ref _disposed) != 0)
                return;

            lock (_sync)
            {
                if (_pendingFrame is null || _frameWidth <= 0 || _frameHeight <= 0)
                    return;

                var size = new PixelSize(_frameWidth, _frameHeight);
                if (_bitmap is null || _bitmap.PixelSize != size)
                {
                    _bitmap?.Dispose();
                    _bitmap = new WriteableBitmap(size, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
                }

                using var framebuffer = _bitmap.Lock();
                if (framebuffer.RowBytes < _framePitch)
                    return;

                if (framebuffer.RowBytes == _framePitch)
                {
                    Marshal.Copy(_pendingFrame, 0, framebuffer.Address, _pendingFrame.Length);
                }
                else
                {
                    for (var row = 0; row < _frameHeight; row++)
                    {
                        Marshal.Copy(
                            _pendingFrame,
                            row * _framePitch,
                            IntPtr.Add(framebuffer.Address, row * framebuffer.RowBytes),
                            _framePitch);
                    }
                }
            }

            InvalidateVisual();
        }
        catch
        {
            // Drop a frame if Avalonia cannot allocate or lock its bitmap; the next VLC frame can retry.
        }
        finally
        {
            Volatile.Write(ref _framePending, 0);
        }
    }

    private void CleanupVideo(ref IntPtr opaque)
    {
        lock (_sync)
            ReleasePictureBuffers();
    }

    private void ReleasePictureBuffers()
    {
        FreeBuffers(_pictureBuffers);
        _pictureBuffers = [];
        _pendingFrame = null;
        _frameWidth = 0;
        _frameHeight = 0;
        _framePitch = 0;
    }

    private static void FreeBuffers(IEnumerable<IntPtr> buffers)
    {
        foreach (var buffer in buffers)
        {
            if (buffer != IntPtr.Zero)
                Marshal.FreeHGlobal(buffer);
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var bitmap = _bitmap;
        if (bitmap is null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var source = new Rect(bitmap.Size);
        var scale = Math.Min(Bounds.Width / source.Width, Bounds.Height / source.Height);
        var width = source.Width * scale;
        var height = source.Height * scale;
        var destination = new Rect((Bounds.Width - width) / 2, (Bounds.Height - height) / 2, width, height);
        context.DrawImage(bitmap, source, destination);
    }

    /// <summary>Clear the last frame and any configured buffers while keeping this visual reusable.</summary>
    public void Reset()
    {
        if (Volatile.Read(ref _disposed) != 0)
            return;

        lock (_sync)
        {
            ReleasePictureBuffers();
            _bitmap?.Dispose();
            _bitmap = null;
        }
        Volatile.Write(ref _framePending, 0);
        InvalidateVisual();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        lock (_sync)
        {
            ReleasePictureBuffers();
            _bitmap?.Dispose();
            _bitmap = null;
        }
    }
}
