# Video-player control research

Research date: 2026-10-26

`MdVideoPlayer` follows the project's required sequence: inspect an Avalonia control first, redesign
its public seam with existing Material controls second, and study Flutter equivalents last.

## 1. Avalonia controls studied first

Avalonia 12 does not provide an open-source media decoder in the base `Avalonia` package. The
following existing controls were evaluated:

- [LibVLCSharp.Avalonia 3.10.2](https://github.com/videolan/libvlcsharp/tree/3.10.2/src/LibVLCSharp.Avalonia)
  is the selected optional desktop backend. Its `VideoView` exposes a `MediaPlayer` while native
  LibVLC remains a separately deployed platform dependency. Version 3.10.2 contains explicit
  Avalonia 11/12 compatibility. Because it uses a native video surface and a floating Avalonia
  overlay to work around airspace, the independent Material transport is placed below the video,
  not drawn as an unreliable sibling over it.
- [Iciclecreek.Avalonia.Controls.Media](https://github.com/tomlm/Iciclecreek.Avalonia.Controls.Media)
  demonstrated a useful view-model separation for play/pause, mute, rate, time, length and seek.
  It was not selected as a dependency because it wraps an older unofficial VideoView fork.
- [FFmpegVideoPlayer.Avalonia](https://github.com/jojomondag/FFmpegVideoPlayer.Avalonia) and
  [MediaPlayer](https://github.com/wieslawsoltes/MediaPlayer) were reviewed as composited renderer
  alternatives. Their renderer/backend selection reinforced keeping the Material shell independent
  from one decoder.
- Avalonia's newer `Avalonia.Controls.MediaPlayer` was not selected: it is an Avalonia commercial
  component rather than an open dependency this Apache-2.0 repository can assume every user owns.

The architecture is consequently two-layered. Core `MdVideoPlayer` is renderer-neutral and remains
usable on Android and in NativeAOT applications. It accepts any renderer through `MediaContent`,
exposes synchronized state, and emits typed `MdPlaybackRequestEventArgs`. The optional
`Md3.Avalonia.LibVLC` package supplies actual Windows, Linux and macOS playback without placing
LibVLCSharp or native codecs in the core package.

## 2. Independent Material redesign

The transport is composed from existing `MdIconButton`, `MdButton`, `MdSlider`, progress and
surface controls. It provides:

- play/pause, seek, mute, normalized volume, playback-rate and fullscreen requests;
- elapsed/duration text, buffering, live and error states;
- Space/K, arrow, M and F keyboard behavior;
- inherited English/Chinese localization and automation names;
- an expanded transport with volume/rate and a compact transport that keeps the critical actions;
- component geometry under `Md.Comp.VideoPlayer.*` and runtime system colors/type roles.

The controls remain outside the media viewport so native surfaces cannot obscure them. Composited
backends can still place custom overlays inside their own `MediaContent`. The core does not pretend
to implement fullscreen window ownership: it changes and reports player state while the host decides
how its window, dialog, mobile orientation, or picture-in-picture surface should change.

## 3. Flutter equivalents studied last

[`fluttercommunity/chewie`](https://github.com/fluttercommunity/chewie) was the primary Flutter
reference. Its separation of a low-level `video_player` controller from a Material/Cupertino control
shell matches the chosen backend-neutral architecture. Its desktop controls, playback speed,
subtitles/options extensibility, localized labels, fullscreen request and configurable controls
informed the state and request seam.

[`better_player`](https://github.com/jhomlala/betterplayer) was reviewed for advanced use cases:
live streams, buffering, subtitles/audio tracks, HLS quality selection, retry, playlist and
picture-in-picture. This first control intentionally implements the stable transport foundation and
leaves decoder-specific tracks, DRM, caching and streaming policy to the backend. No Dart source,
assets, icon data or visual trade dress was copied.

## Platform and licensing boundary

`MdVideoPlayer` itself decodes nothing. `MdLibVlcVideoPlayer` requires a native LibVLC deployment and
supports desktop Avalonia; it is not included in the Android or core NativeAOT graph. LibVLCSharp is
LGPL-2.1-or-later and LibVLC/codecs may carry additional distribution obligations. Applications are
responsible for choosing and complying with the native binaries and codecs they ship.
