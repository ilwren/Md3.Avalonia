#!/usr/bin/env python3
"""Verify the checked-in Google Material Symbols Rounded fonts.

The full font is the unmodified upstream variable TTF.  The Lite font is a
real-outline subset generated from that file for the glyphs exposed by
MdSymbolsLite.  This script deliberately never fabricates or downloads a
fallback font: a missing or changed release asset is a hard build failure.
"""

from __future__ import annotations

import hashlib
import pathlib
import struct
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
FULL_FONT = ROOT / "src/Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded.ttf"
LITE_FONT = ROOT / "src/Md3.Avalonia.Icons.Lite/Assets/Fonts/MaterialSymbolsRounded-Lite.ttf"

# google/material-design-icons, commit 737e3324305806514d7909874fa1818ae1808232
# variablefont/MaterialSymbolsRounded[FILL,GRAD,opsz,wght].ttf
UPSTREAM_COMMIT = "737e3324305806514d7909874fa1818ae1808232"
UPSTREAM_BLOB = "101997d7897c7c9864b3bdd2262320bddcdc11db"
FULL_SHA256 = "95b24392bb49efd1bc3e92cff4e2452ad094461bab7c97e7d8723fab97e330ca"
LITE_SHA256 = "44b4f7010399c84db46f72d03c26f0a2692018aafb55fcfcf80e7ba60c484a87"
REQUIRED_VARIABLE_TABLES = {b"fvar", b"gvar", b"avar", b"HVAR", b"STAT", b"GSUB", b"glyf"}


def fail(message: str) -> None:
    raise SystemExit(f"font verification failed: {message}")


def inspect_font(path: pathlib.Path, expected_sha: str, minimum_glyphs: int) -> tuple[int, set[bytes]]:
    if not path.is_file():
        fail(f"required checked-in font is missing: {path.relative_to(ROOT)}")

    data = path.read_bytes()
    digest = hashlib.sha256(data).hexdigest()
    if digest != expected_sha:
        fail(
            f"unexpected SHA-256 for {path.relative_to(ROOT)}: {digest}; "
            f"expected {expected_sha}"
        )
    if len(data) < 12:
        fail(f"{path.relative_to(ROOT)} is not a valid SFNT font")

    _, table_count = struct.unpack_from(">IH", data, 0)
    tables: dict[bytes, tuple[int, int]] = {}
    for index in range(table_count):
        record = 12 + index * 16
        if record + 16 > len(data):
            fail(f"truncated table directory in {path.relative_to(ROOT)}")
        tag, _, offset, length = struct.unpack_from(">4sIII", data, record)
        if offset + length > len(data):
            fail(f"truncated {tag!r} table in {path.relative_to(ROOT)}")
        tables[tag] = (offset, length)

    missing = REQUIRED_VARIABLE_TABLES.difference(tables)
    if missing:
        names = ", ".join(tag.decode("ascii") for tag in sorted(missing))
        fail(f"{path.relative_to(ROOT)} is not the real variable font; missing tables: {names}")
    if b"maxp" not in tables:
        fail(f"missing maxp table in {path.relative_to(ROOT)}")

    maxp_offset, maxp_length = tables[b"maxp"]
    if maxp_length < 6:
        fail(f"invalid maxp table in {path.relative_to(ROOT)}")
    glyph_count = struct.unpack_from(">H", data, maxp_offset + 4)[0]
    if glyph_count < minimum_glyphs:
        fail(
            f"{path.relative_to(ROOT)} contains only {glyph_count} glyphs; "
            f"expected at least {minimum_glyphs} real outlines"
        )
    return glyph_count, set(tables)


def main() -> int:
    full_glyphs, _ = inspect_font(FULL_FONT, FULL_SHA256, 6000)
    lite_glyphs, _ = inspect_font(LITE_FONT, LITE_SHA256, 60)
    print(
        "Verified official Material Symbols Rounded fonts: "
        f"full={full_glyphs} glyphs, lite={lite_glyphs} glyphs, "
        f"upstream={UPSTREAM_COMMIT}, blob={UPSTREAM_BLOB}"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
