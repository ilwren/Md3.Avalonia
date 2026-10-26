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

For repository development, the launch helpers locate an installed VLC distribution, validate its
plugin directory, set the process-local loader variables, and run the managed desktop Gallery:

```bash
scripts/run-gallery-libvlc.sh --vlc-home /path/to/vlc
# Validate discovery without launching:
scripts/run-gallery-libvlc.sh --check-only --vlc-home /path/to/vlc
```

```powershell
.\scripts\run-gallery-libvlc.ps1 -VlcHome 'C:\Program Files\VideoLAN\VLC'
```

The scripts never install VLC, modify system loader paths, or add LibVLC to Android/NativeAOT.

Video frames are decoded by LibVLC into BGRA memory and copied into an Avalonia `WriteableBitmap`.
Unlike a `NativeControlHost`/`VideoView`, this composited surface follows Avalonia clipping,
scrolling, transforms and z-order, so it does not flash outside a scroller's viewport. The tradeoff
is CPU-side frame copying; this backend favors predictable Material UI composition over native
video-surface throughput. Audio continues through LibVLC's normal platform output.

The host handles fullscreen window ownership through the inherited `PlaybackRequested` event and
synchronizes `IsFullScreen` after moving the player.
