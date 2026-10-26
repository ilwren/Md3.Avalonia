#!/usr/bin/env python3
"""Generate the repository-owned Gallery video sample with an ffmpeg executable.

The output contains only procedural pixels and synthesized audio. It does not depend on external
media, fonts, images, or network access. Example:

    python3 scripts/generate-gallery-video.py \
        --ffmpeg /usr/bin/ffmpeg \
        gallery/Md3.Avalonia.Gallery/Assets/Media/material-motion.mp4
"""

from __future__ import annotations

import argparse
import math
import struct
import subprocess
import tempfile
import wave
from pathlib import Path

WIDTH = 640
HEIGHT = 360
FPS = 24
DURATION_SECONDS = 8
SAMPLE_RATE = 48_000


def clamp(value: float) -> int:
    return max(0, min(255, round(value)))


def paint_frame(index: int) -> bytes:
    phase = index / (FPS * DURATION_SECONDS)
    frame = bytearray(WIDTH * HEIGHT * 3)

    # Slowly changing Material-like dusk gradient.
    for y in range(HEIGHT):
        vertical = y / (HEIGHT - 1)
        pulse = (math.sin(phase * math.tau) + 1) * 0.5
        top = (20 + 8 * pulse, 32 + 10 * pulse, 65 + 14 * pulse)
        bottom = (196 + 18 * pulse, 101 + 12 * pulse, 75 + 8 * pulse)
        color = tuple(clamp(top[c] * (1 - vertical) + bottom[c] * vertical) for c in range(3))
        row = bytes(color) * WIDTH
        start = y * WIDTH * 3
        frame[start : start + WIDTH * 3] = row

    # A warm disc crossing the sky verifies continuous motion and seeking.
    sun_x = int(440 + 54 * math.sin(phase * math.tau))
    sun_y = int(90 + 13 * math.cos(phase * math.tau))
    radius = 43
    for y in range(max(0, sun_y - radius), min(HEIGHT, sun_y + radius + 1)):
        half = int(math.sqrt(max(0, radius * radius - (y - sun_y) ** 2)))
        for x in range(max(0, sun_x - half), min(WIDTH, sun_x + half + 1)):
            offset = (y * WIDTH + x) * 3
            edge = math.hypot(x - sun_x, y - sun_y) / radius
            frame[offset : offset + 3] = bytes((255, clamp(226 - edge * 18), clamp(170 - edge * 20)))

    # Three independently moving mountain layers make frame progression visually obvious.
    layers = (
        (205, (62, 76, 99), 19, 0.6),
        (250, (37, 49, 72), 27, 1.2),
        (294, (21, 29, 48), 18, 2.1),
    )
    for base, color, amplitude, speed in layers:
        for x in range(WIDTH):
            ridge = base + int(
                amplitude * math.sin(x / 72 + phase * math.tau * speed)
                + amplitude * 0.45 * math.sin(x / 29 - phase * math.tau * 0.4)
            )
            ridge = max(0, min(HEIGHT, ridge))
            pixel = bytes(color)
            for y in range(ridge, HEIGHT):
                offset = (y * WIDTH + x) * 3
                frame[offset : offset + 3] = pixel

    # A primary-color progress marker loops once during the sample.
    marker_x = int(24 + phase * (WIDTH - 48))
    for y in range(HEIGHT - 18, HEIGHT - 10):
        for x in range(max(0, marker_x - 18), min(WIDTH, marker_x + 19)):
            offset = (y * WIDTH + x) * 3
            frame[offset : offset + 3] = bytes((208, 188, 255))

    return bytes(frame)


def write_ambient_audio(path: Path) -> None:
    with wave.open(str(path), "wb") as output:
        output.setnchannels(2)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        frames = bytearray()
        frequencies = (196.0, 246.94, 293.66)
        for sample_index in range(SAMPLE_RATE * DURATION_SECONDS):
            seconds = sample_index / SAMPLE_RATE
            fade = min(1.0, seconds / 1.1, (DURATION_SECONDS - seconds) / 1.1)
            drift = 0.72 + 0.28 * math.sin(seconds * math.tau / DURATION_SECONDS)
            value = sum(math.sin(seconds * math.tau * frequency) for frequency in frequencies) / len(frequencies)
            left = int(32767 * 0.055 * fade * drift * value)
            right = int(32767 * 0.055 * fade * (1.0 - 0.1 * drift) * value)
            frames.extend(struct.pack("<hh", left, right))
        output.writeframes(frames)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("output", type=Path)
    parser.add_argument("--ffmpeg", default="ffmpeg", help="ffmpeg executable path")
    args = parser.parse_args()
    args.output.parent.mkdir(parents=True, exist_ok=True)

    with tempfile.TemporaryDirectory(prefix="md3-gallery-video-") as temporary:
        audio_path = Path(temporary) / "ambient.wav"
        write_ambient_audio(audio_path)
        command = [
            args.ffmpeg,
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-f",
            "image2pipe",
            "-vcodec",
            "ppm",
            "-framerate",
            str(FPS),
            "-i",
            "pipe:0",
            "-i",
            str(audio_path),
            "-map_metadata",
            "-1",
            "-c:v",
            "libx264",
            "-preset",
            "slow",
            "-crf",
            "24",
            "-pix_fmt",
            "yuv420p",
            "-c:a",
            "aac",
            "-b:a",
            "96k",
            "-movflags",
            "+faststart",
            "-shortest",
            str(args.output),
        ]
        process = subprocess.Popen(command, stdin=subprocess.PIPE)
        assert process.stdin is not None
        try:
            for frame_index in range(FPS * DURATION_SECONDS):
                process.stdin.write(f"P6\n{WIDTH} {HEIGHT}\n255\n".encode("ascii"))
                process.stdin.write(paint_frame(frame_index))
        finally:
            process.stdin.close()
        exit_code = process.wait()
        if exit_code:
            raise SystemExit(f"ffmpeg exited with status {exit_code}")

    print(f"generated {args.output} ({args.output.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
