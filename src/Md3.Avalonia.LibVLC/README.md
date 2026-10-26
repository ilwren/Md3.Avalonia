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

The Material controls are kept below `VideoView`, rather than layered over its native surface, so
they remain reliable across Windows, macOS and Linux airspace implementations. The host handles
fullscreen window ownership through the inherited `PlaybackRequested` event and synchronizes
`IsFullScreen` after moving the player.
