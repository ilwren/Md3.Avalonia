#!/usr/bin/env python3
"""Check spec-snapshot/tokens.json against the pinned upstream token sources.

The oracle in spec-snapshot/tokens.json is hand-curated so that it can never be derived from
Md3.Avalonia's own resource dictionaries. That curation has one failure mode: a transcription
error, or an upstream value that moved after the baseline was frozen. This script closes that
hole by re-reading the pinned androidx commit and reporting where the two disagree.

It is NOT part of the build gate:

  * it needs network access, which the offline/packaging jobs do not have;
  * a mismatch means "a human must decide", not "the code is wrong" - upstream moving is a
    re-baselining decision recorded in spec-snapshot/manifest.json, not a regression.

CI runs it with --soft so drift shows up in the job summary without turning the build red.

Usage:
  python3 scripts/sync-spec-tokens.py              # exit 1 on drift
  python3 scripts/sync-spec-tokens.py --soft       # always exit 0
  python3 scripts/sync-spec-tokens.py --offline    # validate the mapping table only
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import urllib.error
import urllib.request
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SPEC = REPO / "spec-snapshot"
RAW = "https://raw.githubusercontent.com/androidx/androidx/{commit}/{root}/{name}"
TIMEOUT = 20

# oracle key -> (kotlin token file, constant name)
#
# Only entries with an unambiguous upstream counterpart are listed. Values that M3 publishes as
# prose or only on m3.material.io (the Expressive button size scale, for instance) have no row
# here and are verified by review instead; the script reports them as unmapped rather than
# pretending they were checked.
MAPPING: dict[str, tuple[str, str]] = {
    "Md.Sys.Shape.Corner.ExtraSmall": ("ShapeTokens.kt", "CornerExtraSmall"),
    "Md.Sys.Shape.Corner.Small": ("ShapeTokens.kt", "CornerSmall"),
    "Md.Sys.Shape.Corner.Medium": ("ShapeTokens.kt", "CornerMedium"),
    "Md.Sys.Shape.Corner.Large": ("ShapeTokens.kt", "CornerLarge"),
    "Md.Sys.Shape.Corner.ExtraLarge": ("ShapeTokens.kt", "CornerExtraLarge"),

    "Md.Sys.State.Hover.Opacity": ("StateTokens.kt", "HoverStateLayerOpacity"),
    "Md.Sys.State.Focus.Opacity": ("StateTokens.kt", "FocusStateLayerOpacity"),
    "Md.Sys.State.Pressed.Opacity": ("StateTokens.kt", "PressedStateLayerOpacity"),
    "Md.Sys.State.Dragged.Opacity": ("StateTokens.kt", "DraggedStateLayerOpacity"),

    "Md.Comp.CheckBox.Container.Size": ("CheckboxTokens.kt", "ContainerSize"),
    "Md.Comp.CheckBox.StateLayer.Size": ("CheckboxTokens.kt", "StateLayerSize"),

    "Md.Comp.RadioButton.Icon.Size": ("RadioButtonTokens.kt", "IconSize"),
    "Md.Comp.RadioButton.StateLayer.Size": ("RadioButtonTokens.kt", "StateLayerSize"),

    "Md.Comp.Switch.Track.Width": ("SwitchTokens.kt", "TrackWidth"),
    "Md.Comp.Switch.Track.Height": ("SwitchTokens.kt", "TrackHeight"),
    "Md.Comp.Switch.Handle.Unselected.Size": ("SwitchTokens.kt", "UnselectedHandleWidth"),
    "Md.Comp.Switch.Handle.Selected.Size": ("SwitchTokens.kt", "SelectedHandleWidth"),

    "Md.Comp.Chip.Container.Height": ("AssistChipTokens.kt", "ContainerHeight"),
    "Md.Comp.Chip.Icon.Size": ("AssistChipTokens.kt", "IconSize"),

    "Md.Comp.Divider.Thickness": ("DividerTokens.kt", "Thickness"),

    "Md.Comp.NavigationBar.Container.Height": ("NavigationBarTokens.kt", "ContainerHeight"),
    "Md.Comp.NavigationRail.Container.Width": ("NavigationRailTokens.kt", "ContainerWidth"),
    "Md.Comp.NavigationDrawer.Container.Width": ("NavigationDrawerTokens.kt", "ContainerWidth"),

    "Md.Comp.TextField.Container.MinHeight": ("FilledTextFieldTokens.kt", "ContainerHeight"),
    "Md.Comp.AppBar.Small.Height": ("TopAppBarSmallTokens.kt", "ContainerHeight"),

    "Md.Comp.Fab.Regular.Size": ("FabBaselineTokens.kt", "ContainerHeight"),
    "Md.Comp.Fab.Regular.IconSize": ("FabBaselineTokens.kt", "IconSize"),
}

VALUE = re.compile(r"val\s+(\w+)\s*=\s*(-?\d+(?:\.\d+)?)\.(dp|sp)\b")


def fetch(commit: str, root: str, name: str) -> str | None:
    url = RAW.format(commit=commit, root=root, name=name)
    try:
        with urllib.request.urlopen(url, timeout=TIMEOUT) as response:  # noqa: S310 - pinned host
            return response.read().decode("utf-8", errors="replace")
    except (urllib.error.URLError, TimeoutError, OSError) as exc:
        print(f"  ! could not fetch {name}: {exc}")
        return None


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--soft", action="store_true", help="report drift but always exit 0")
    ap.add_argument("--offline", action="store_true", help="validate the mapping table without network access")
    args = ap.parse_args()

    manifest = json.loads((SPEC / "manifest.json").read_text(encoding="utf-8"))
    oracle = json.loads((SPEC / "tokens.json").read_text(encoding="utf-8"))
    upstream = manifest["androidx_compose_material3"]
    commit = upstream["commit"]
    root = upstream["token_root"]

    by_key = {t["key"]: t for t in oracle["tokens"]}

    unknown = sorted(k for k in MAPPING if k not in by_key)
    if unknown:
        print("mapping table references keys that are not in the oracle:")
        for key in unknown:
            print(f"  - {key}")
        return 1

    mapped = len(MAPPING)
    unmapped = sorted(k for k in by_key if k not in MAPPING)
    print(f"oracle entries: {len(by_key)}  mapped upstream: {mapped}  verified by review only: {len(unmapped)}")

    if args.offline:
        print("offline: mapping table is self-consistent; skipping the upstream comparison.")
        return 0

    print(f"comparing against androidx@{commit[:12]}")
    cache: dict[str, dict[str, float]] = {}
    drift: list[str] = []
    checked = 0

    for key, (filename, constant) in sorted(MAPPING.items()):
        if filename not in cache:
            text = fetch(commit, root, filename)
            cache[filename] = {m.group(1): float(m.group(2)) for m in VALUE.finditer(text)} if text else {}

        values = cache[filename]
        if constant not in values:
            print(f"  ? {key}: '{constant}' not found in {filename} (upstream may have renamed it)")
            continue

        entry = by_key[key]
        expected = entry["value"]
        expected = float(expected) if isinstance(expected, (int, float)) else float(expected[0])
        actual = values[constant]
        checked += 1

        if abs(expected - actual) > 1e-6:
            drift.append(
                f"{key}: oracle has {expected:g}, {filename}:{constant} has {actual:g} "
                f"(confidence={entry.get('confidence', 'medium')})"
            )

    print(f"compared {checked} value(s); {len(drift)} drifted")
    for item in drift:
        print(f"  DRIFT {item}")

    if drift:
        print()
        print("Upstream moving is a re-baselining decision, not a build failure. Update")
        print("spec-snapshot/tokens.json and bump the commit in spec-snapshot/manifest.json together,")
        print("so the oracle and its provenance never disagree.")

    return 0 if args.soft or not drift else 1


if __name__ == "__main__":
    sys.exit(main())
