# Md3.Avalonia.LibVLC

Optional desktop playback backend for the core `MdVideoPlayer` Material transport.

```xml
<PackageReference Include="Md3.Avalonia.LibVLC" Version="0.5.0-preview.1" />
```

```csharp
using Md3.Avalonia.LibVLC;

var player = new MdLibVlcVideoPlayer
{
    Source = new Uri("file:///videos/demo.mp4"),
    AutoPlay = true,
    Loop = false
};
```

The application must also deploy native LibVLC. Common choices are
`VideoLAN.LibVLC.Windows`, `VideoLAN.LibVLC.Mac`, or the distribution's VLC/LibVLC packages on
Linux. This adapter intentionally does not bundle native binaries or codecs. It targets desktop
Avalonia and is not part of the Android or core NativeAOT dependency graph.

Video frames are decoded by LibVLC into BGRA memory and copied into an Avalonia `WriteableBitmap`.
Unlike a `NativeControlHost`/`VideoView`, this composited surface follows Avalonia clipping,
scrolling, transforms and z-order, so it does not flash outside a scroller's viewport. The tradeoff
is one CPU-side frame copy; this backend favors predictable Material UI composition over native
video-surface throughput. Audio continues through LibVLC's normal platform output.

The host handles fullscreen window ownership through the inherited `PlaybackRequested` event and
synchronizes `IsFullScreen` after moving the player.
