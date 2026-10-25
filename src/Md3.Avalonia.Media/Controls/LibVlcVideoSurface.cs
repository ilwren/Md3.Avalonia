using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace Md3.Avalonia.Media.Controls;

/// <summary>Renders LibVLC frames into Avalonia's composition surface so player chrome can overlay the video.</summary>
/// <remarks>
/// LibVLC's native video view is hosted in a native window on some platforms and can cover Avalonia controls
/// placed above it. This surface uses LibVLC callbacks and an Avalonia bitmap instead, preserving normal
/// Avalonia z-order. Frames are copied to the UI thread; this favors reliable overlay interaction over
/// the zero-copy performance of a native video surface.
/// </remarks>
public sealed class LibVlcVideoSurface : UserControl
{
    private readonly Image _image = new() { Stretch = Avalonia.Media.Stretch.Uniform };
    // Keep callback delegates alive for as long as LibVLC may call them.
    private readonly object[] _callbacks;
    private FrameData? _pendingFrame;
    private int _renderScheduled;
    private WriteableBitmap? _bitmap;

    /// <summary>Creates an Avalonia-rendered video surface and registers its LibVLC callbacks.</summary>
    public LibVlcVideoSurface(MediaPlayer mediaPlayer)
    {
        ArgumentNullException.ThrowIfNull(mediaPlayer);
        Background = Avalonia.Media.Brushes.Black;
        Content = _image;

        var format = new MediaPlayer.LibVLCVideoFormatCb(VideoFormatCallback);
        var cleanup = new MediaPlayer.LibVLCVideoCleanupCb(CleanupCallback);
        var lockFrame = new MediaPlayer.LibVLCVideoLockCb(LockVideo);
        var unlockFrame = new MediaPlayer.LibVLCVideoUnlockCb(UnlockVideo);
        var display = new MediaPlayer.LibVLCVideoDisplayCb(DisplayVideo);
        _callbacks = [format, cleanup, lockFrame, unlockFrame, display];

        mediaPlayer.SetVideoFormatCallbacks(format, cleanup);
        mediaPlayer.SetVideoCallbacks(lockFrame, unlockFrame, display);
    }

    private uint VideoFormatCallback(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
    {
        var w = checked((int)width);
        var h = checked((int)height);
        if (w <= 0 || h <= 0) return 0;

        var pitch = Align(checked(w * 4), 32);
        var lineCount = Align(h, 32);
        var buffer = Marshal.AllocHGlobal(checked(pitch * lineCount));
        var context = new FrameContext(buffer, w, h, pitch);
        var handle = GCHandle.Alloc(context);
        opaque = GCHandle.ToIntPtr(handle);

        pitches = (uint)pitch;
        lines = (uint)lineCount;
        ToFourCc("RV32", chroma);
        return 1;
    }

    private static void ToFourCc(string value, IntPtr destination)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        Marshal.Copy(bytes, 0, destination, 4);
    }

    private static int Align(int value, int alignment) => (value + alignment - 1) & ~(alignment - 1);
    private static FrameContext? GetContext(IntPtr opaque) => opaque == IntPtr.Zero ? null : GCHandle.FromIntPtr(opaque).Target as FrameContext;

    private IntPtr LockVideo(IntPtr opaque, IntPtr planes)
    {
        if (GetContext(opaque) is not { } context) return IntPtr.Zero;
        Marshal.WriteIntPtr(planes, context.Buffer);
        return IntPtr.Zero;
    }

    private void UnlockVideo(IntPtr opaque, IntPtr picture, IntPtr planes) { }

    private void DisplayVideo(IntPtr opaque, IntPtr picture)
    {
        if (GetContext(opaque) is not { } context || Volatile.Read(ref _disposed) != 0) return;

        // Copy the decoded frame before returning to LibVLC: its callback buffer is reused immediately.
        var packedStride = checked(context.Width * 4);
        var frame = new byte[checked(packedStride * context.Height)];
        for (var row = 0; row < context.Height; row++)
            Marshal.Copy(IntPtr.Add(context.Buffer, row * context.Pitch), frame, row * packedStride, packedStride);

        Interlocked.Exchange(ref _pendingFrame, new FrameData(frame, context.Width, context.Height)); // Drop older frames if the UI thread is behind.
        if (Interlocked.Exchange(ref _renderScheduled, 1) == 0)
            Dispatcher.UIThread.Post(RenderPendingFrame, DispatcherPriority.Render);
    }

    private void RenderPendingFrame()
    {
        var frameData = Interlocked.Exchange(ref _pendingFrame, null);
        if (frameData is not null && Volatile.Read(ref _disposed) == 0)
        {
            var frame = frameData.Pixels;
            var width = frameData.Width;
            var height = frameData.Height;
            var size = new PixelSize(width, height);
            if (_bitmap is null || _bitmap.PixelSize != size)
            {
                _bitmap?.Dispose();
                _bitmap = new WriteableBitmap(size, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
                _image.Source = _bitmap;
            }

            using var framebuffer = _bitmap.Lock();
            var sourceStride = width * 4;
            var destinationStride = framebuffer.RowBytes;
            for (var row = 0; row < height; row++)
                Marshal.Copy(frame, row * sourceStride, IntPtr.Add(framebuffer.Address, row * destinationStride), sourceStride);
        }

        Interlocked.Exchange(ref _renderScheduled, 0);
        if (Volatile.Read(ref _pendingFrame) is not null && Interlocked.Exchange(ref _renderScheduled, 1) == 0)
            Dispatcher.UIThread.Post(RenderPendingFrame, DispatcherPriority.Render);
    }

    private void CleanupCallback(ref IntPtr opaque)
    {
        if (opaque != IntPtr.Zero)
        {
            var handle = GCHandle.FromIntPtr(opaque);
            if (handle.Target is FrameContext context && context.Buffer != IntPtr.Zero)
                Marshal.FreeHGlobal(context.Buffer);
            handle.Free();
            opaque = IntPtr.Zero;
        }

        GC.KeepAlive(_callbacks);
        Dispatcher.UIThread.Post(() =>
        {
            if (Volatile.Read(ref _disposed) == 0) return;
            _image.Source = null;
            _bitmap?.Dispose();
            _bitmap = null;
        });
    }

    /// <summary>Stops accepting frames and clears the rendered bitmap.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        Dispatcher.UIThread.Post(() =>
        {
            _image.Source = null;
            _bitmap?.Dispose();
            _bitmap = null;
        });
    }

    private sealed record FrameContext(IntPtr Buffer, int Width, int Height, int Pitch);
    private sealed record FrameData(byte[] Pixels, int Width, int Height);
}
