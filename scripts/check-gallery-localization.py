#!/usr/bin/env python3
"""Report what the Gallery's Chinese localization does not cover.

The Gallery is bilingual by key match: ``GalleryLocalization.Apply`` walks a page and rewrites each
``TextBlock.Text`` whose English string is a key in the dictionary, leaving everything else alone. A
page can therefore be silently half-translated, and nothing fails - the strings simply stay English.
Defect #29 lists that as an open sweep; this script is the measurement half of it, so the number can
only be argued with rather than estimated.

Two things are reported separately, because they are not the same problem:

* **prose** - a TextBlock carrying the ``h1``, ``h2``, ``h3`` or ``body`` class. This is the writing
  on the page and it is what a reader notices;
* **data** - every other TextBlock: swatch labels, sample rows, hex values, keyboard shortcuts. Much
  of it should never be translated, so counting it as a gap would be wrong.

Duplicate dictionary keys fail the run. An indexer initializer silently overwrites, so a key typed
twice means one of the two translations is dead, and the dead one is whichever was written first.

With ``--strict`` a missing prose translation also fails, which is what the check should become once
the backlog is cleared; until then the count is a notice, and a review can see it move.
"""

from __future__ import annotations

import argparse
import html
import os
import re
import sys
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
LOCALIZATION = ROOT / "gallery/Md3.Avalonia.Gallery/GalleryLocalization.cs"
PAGES = ROOT / "gallery/Md3.Avalonia.Gallery/Pages"

PAIR = re.compile(r'\["((?:[^"\\]|\\.)*)"\]\s*=\s*"((?:[^"\\]|\\.)*)"')
TEXT_BLOCK = re.compile(r"<TextBlock\b[^>]*?/?>", re.S)
ATTRIBUTE = re.compile(r'(\w+(?::\w+)?)="([^"]*)"')

PROSE_CLASSES = {"h1", "h2", "h3", "body"}

# Prose by class, but an identifier by meaning: Material colour role names printed beside a chart
# series, a SpinKit figure name, and a formatted value. Translating them would make the page lie
# about the API it is demonstrating, so they are neither a gap nor an entry.
VERBATIM = {
    "InversePrimary", "Outline", "PrimaryContainer", "Secondary", "Tertiary", "TertiaryContainer",
    "ThreeBounce", "20 — 80",
}


def dictionary() -> tuple[list[str], list[str]]:
    """Returns every key in the dictionary, and the ones that appear more than once."""
    source = LOCALIZATION.read_text(encoding="utf-8")
    keys = [match.group(1) for match in PAIR.finditer(source)]
    duplicates = sorted(key for key, count in Counter(keys).items() if count > 1)
    return keys, duplicates


def page_texts(path: Path) -> list[tuple[str, bool]]:
    """Returns each TextBlock's text on a page, and whether it is prose."""
    source = path.read_text(encoding="utf-8")
    found: list[tuple[str, bool]] = []
    for block in TEXT_BLOCK.finditer(source):
        attributes = {name: value for name, value in ATTRIBUTE.findall(block.group(0))}
        text = attributes.get("Text")
        # A binding or a markup extension is resolved at run time; there is nothing to key on.
        if text is None or text.startswith("{"):
            continue
        classes = set(attributes.get("Classes", "").split())
        found.append((html.unescape(text), bool(classes & PROSE_CLASSES)))
    return found


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--strict", action="store_true",
                        help="fail on a missing prose translation as well as on duplicate keys")
    parser.add_argument("--show-data", action="store_true",
                        help="also list the non-prose strings that have no entry")
    args = parser.parse_args()

    keys, duplicates = dictionary()
    known = set(keys)

    prose_missing: dict[str, list[str]] = {}
    data_missing: dict[str, list[str]] = {}
    verbatim = set()
    for path in sorted(PAGES.glob("*.axaml")):
        prose, data = [], []
        for text, is_prose in page_texts(path):
            if text in known:
                continue
            if text in VERBATIM:
                verbatim.add(text)
                continue
            (prose if is_prose else data).append(text)
        if prose:
            prose_missing[path.name] = sorted(set(prose))
        if data:
            data_missing[path.name] = sorted(set(data))

    prose_total = sum(len(value) for value in prose_missing.values())
    data_total = sum(len(value) for value in data_missing.values())

    summary = (f"gallery localization: {len(keys)} keys, {len(duplicates)} duplicate; "
               f"untranslated prose={prose_total} on {len(prose_missing)} pages, "
               f"other text={data_total} on {len(data_missing)} pages, "
               f"{len(verbatim)} identifier left verbatim")
    print(summary)
    if os.environ.get("GITHUB_ACTIONS"):
        print(f"::notice title=Gallery localization::{summary}")

    for name, missing in sorted(prose_missing.items()):
        print(f"  prose  {name}: {len(missing)}")
        for text in missing:
            print(f"         {text[:100]}")
    if args.show_data:
        for name, missing in sorted(data_missing.items()):
            print(f"  other  {name}: {len(missing)} -> {[text[:40] for text in missing][:6]}")

    failed = False
    for key in duplicates:
        print(f"::error title=Duplicate localization key::{key} is defined more than once in "
              f"GalleryLocalization.cs; the first definition is dead")
        failed = True
    if args.strict and prose_total:
        print(f"::error title=Untranslated prose::{prose_total} prose strings on "
              f"{len(prose_missing)} pages have no dictionary entry")
        failed = True
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
