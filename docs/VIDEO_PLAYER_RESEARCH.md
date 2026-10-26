# Video-player control research

Research date: 2026-10-26

`MdVideoPlayer` follows the project's required sequence: inspect an Avalonia control first, redesign
its public seam with existing Material controls second, and study Flutter equivalents last.

## 1. Avalonia controls studied first

Avalonia 12 does not provide an open-source media decoder in the base `Avalonia` package. The
following existing controls were evaluated:

- [LibVLCSharp 3.10.2](https://github.com/videolan/libvlcsharp/tree/3.10.2/src/LibVLCSharp)
  is the selected optional desktop backend while native LibVLC remains a separately deployed
  platform dependency. The upstream Avalonia `VideoView` was tested first, but its native child
  surface/floating-window airspace model produced stale or briefly visible frames while an ancestor
  scrolled. The adapter therefore uses LibVLC's decoded-frame callbacks and copies BGRA frames into
  an Avalonia `WriteableBitmap`. This costs CPU-side frame copies but gives the media normal Avalonia
  clipping, scrolling, transforms and z-order.
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

The follow-up visual pass keeps the renderer and transport in one clipped surface. A transparent-to-
black bottom scrim holds title, seek and the compact desktop action row, while a large center action
appears only when paused. Controls fade as a group and never reserve a detached footer below the
picture. Composited backends can still place custom overlays inside their own `MediaContent`. The
core does not pretend to implement fullscreen window ownership: it changes and reports player state
while the host decides how its window, dialog, mobile orientation, or picture-in-picture surface
should change.

## 3. Flutter equivalents studied last

[`media_kit_video`](https://github.com/media-kit/media-kit/tree/main/media_kit_video) is the primary
visual reference for the follow-up. Its Material desktop theme overlays a 56 dp bottom button bar and
seek track on the picture, fades controls in about 150 ms, enlarges the seek track on hover, and
separates normal/fullscreen configuration. Its mobile Material theme adds a center play action,
backdrop and gesture regions. `MdVideoPlayer` adopts the visual hierarchy—not its Dart code or red
accent—using Material system roles, the existing backend-neutral events and Avalonia composition.

[`fluttercommunity/chewie`](https://github.com/fluttercommunity/chewie) remains the main architecture
reference. Its separation of a low-level `video_player` controller from a Material/Cupertino control
shell matches the chosen backend-neutral architecture. Its in-picture bottom bar, center action,
250 ms control opacity, playback speed, localized labels and fullscreen request informed the revised
shell and the existing state/request seam.

[`better_player`](https://github.com/jhomlala/betterplayer) was reviewed for advanced use cases:
live streams, buffering, subtitles/audio tracks, HLS quality selection, retry, playlist and
picture-in-picture. This first control intentionally implements the stable transport foundation and
leaves decoder-specific tracks, DRM, caching and streaming policy to the backend. No Dart source,
assets, icon data or visual trade dress was copied.

## Gallery validation asset

The shared Gallery embeds `Assets/Media/material-motion.mp4`, an eight-second 640×360 H.264/AAC file containing only procedural pixels and synthesized tones. `scripts/generate-gallery-video.py` reproducibly creates it from scratch; no third-party footage, image, font, or audio asset is included. A regular managed desktop host conditionally references `Md3.Avalonia.LibVLC`, extracts the embedded resource to a stable temporary file, and supplies an `MdLibVlcVideoPlayer` through `GalleryVideoBackend.Factory`. This exercises an actual local file, composited decoded frames, audio, duration, seeking, volume, rate, looping and end events.

Android, headless tests and `PublishAot=true` leave the factory unset and continue to render the deterministic backend-neutral demonstration. Linux requires system LibVLC; Windows/macOS hosts likewise follow the adapter's documented native deployment requirement. Missing native LibVLC is surfaced in the player's error state rather than taking down the Gallery.

`scripts/run-gallery-libvlc.sh` and `scripts/run-gallery-libvlc.ps1` orchestrate repository development:
they discover an installed VLC, validate its plugin directory, set loader variables only for the
child process, and run the non-AOT desktop Gallery. `--check-only` validates discovery. They do not
install VLC or make a native package part of the shared project graph.

## Platform and licensing boundary

`MdVideoPlayer` itself decodes nothing. `MdLibVlcVideoPlayer` requires a native LibVLC deployment and
supports desktop Avalonia; it is not included in the Android or core NativeAOT graph. LibVLCSharp is
LGPL-2.1-or-later and LibVLC/codecs may carry additional distribution obligations. Applications are
responsible for choosing and complying with the native binaries and codecs they ship.
