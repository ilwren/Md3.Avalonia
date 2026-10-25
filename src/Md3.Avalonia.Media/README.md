# Md3.Avalonia.Media

An optional, separately packaged Avalonia video-player UI inspired by Better Player: overlay transport controls, seeking, elapsed/total time, volume, and a host-controlled fullscreen action.

## LibVLC integration and dependencies

The package references `LibVLCSharp.Avalonia`, so the managed LibVLCSharp bindings are restored transitively. `AttachLibVlc` renders decoded frames into an Avalonia `WriteableBitmap` using LibVLC callbacks rather than putting a native `VideoView` window above the Avalonia visual tree; that lets the player buttons and seek bar remain visible and interactive. It does **not** bundle every native LibVLC build into the base control package: native payloads are platform- and architecture-specific, the Windows package is very large, and Linux LibVLC is normally installed by the distribution. Keeping native runtimes out of the base UI package avoids adding unrelated native binaries to every consumer.

The Gallery desktop host demonstrates automatic native package restore where VideoLAN provides packages:

- Windows: `VideoLAN.LibVLC.Windows` is included in the desktop host.
- macOS: `VideoLAN.LibVLC.Mac` is included in the desktop host.
- Linux: install the system LibVLC runtime and plugins (for Debian/Ubuntu, for example, `sudo apt install libvlc5 libvlccore9 vlc-plugin-base vlc-plugin-video-output`).

An application creates and owns the LibVLC engine and media player, then connects it to the UI:

```xml
<PackageReference Include="Md3.Avalonia.Media" Version="0.5.0-preview.2" />
```

```csharp
using LibVLCSharp.Shared;
using Md3.Avalonia.Media.Controls;

Core.Initialize(); // Native LibVLC runtime must be deployed for the target OS/architecture.
var libVlc = new LibVLC();
var mediaPlayer = new MediaPlayer(libVlc);
mediaPlayer.Media = new Media(libVlc, new Uri(localFilePath));
var player = new MdVideoPlayer();
player.AttachLibVlc(mediaPlayer);
player.FullscreenRequested += (_, _) => window.WindowState = WindowState.FullScreen;
mediaPlayer.Play(); // Start once the view is attached to the visual tree.
```

`LibVlcPlaybackSession` adapts LibVLC transport and state to the control. The host owns the LibVLC instance, `MediaPlayer`, and media; dispose the session adapter to detach its event handlers. The host handles fullscreen/window behavior. Styling uses regular Avalonia `Button`, `Slider`, `Border`, and `TextBlock` controls.

## Gallery sample

The Gallery plays the first 30 seconds of **Big Buck Bunny** from a local MP4 bundled with the desktop app—playback makes no request to a video URL. The clip is © Blender Foundation, licensed under CC BY 3.0; its attribution and source are recorded in `gallery/Md3.Avalonia.Gallery/Assets/Media/BigBuckBunny-LICENSE.txt`. The desktop build copies the clip to `Assets/Media/BigBuckBunny-480p.mp4` beside the app.

## Current scope

The control includes play/pause, seek, elapsed/total time, volume, overlay controls, hover/press opacity transitions, and a fullscreen request event. Playlist, subtitles, adaptive track selection, caching, and picture-in-picture are not included yet.
