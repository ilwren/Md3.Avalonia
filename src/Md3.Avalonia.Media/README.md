# Md3.Avalonia.Media

An optional, separately packaged Avalonia video-player UI inspired by the familiar Better Player experience: overlay transport controls, seeking, time display, volume, and a host-controlled fullscreen action.

## Design

This package includes LibVLCSharp.Avalonia and provides an `AttachLibVlc` convenience method, plus the player chrome. The host creates and owns the LibVLC engine and `MediaPlayer`; the component creates the `VideoView` and a session adapter. Native VLC runtime binaries are still platform-specific and must be included by the consuming application.

```xml
<PackageReference Include="Md3.Avalonia.Media" Version="0.5.0-preview.2" />
```

```csharp
using LibVLCSharp.Shared;
using Md3.Avalonia.Media.Controls;

Core.Initialize(); // Ensure platform VLC native libraries are deployed with the app.
var libVlc = new LibVLC();
var mediaPlayer = new MediaPlayer(libVlc);
mediaPlayer.Media = new Media(libVlc, new Uri("https://example.com/video.mp4"));
var player = new MdVideoPlayer();
player.AttachLibVlc(mediaPlayer);
player.FullscreenRequested += (_, _) => window.WindowState = WindowState.FullScreen;
mediaPlayer.Play(); // Start after the view is attached to the visual tree.
```

`LibVlcPlaybackSession` adapts LibVLC's transport and state to the UI. The host owns and disposes the LibVLC instance, `MediaPlayer`, and media; dispose the adapter when no longer needed to detach event handlers. The host handles fullscreen/window behavior. Customize the look by styling Avalonia's standard `Button`, `Slider`, `Border`, and `TextBlock` controls.

## Current scope

The first iteration includes play/pause, seek, elapsed/total time, volume slider, overlay controls, and a fullscreen request event. Playlist, subtitles, adaptive track selection, caching, picture-in-picture, and runtime VLC binary packaging are not included yet.
