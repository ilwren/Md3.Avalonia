#!/usr/bin/env python3
"""Check that every public type shipped by the packages is named in docs/API.md.

The API reference drifted badly once already: it documented properties that did not
exist and omitted 80 public types, including 41 enums and the whole Icons.Lite
package. This is a static check -- no SDK required -- so CI can hold the line.

Usage:
    python3 scripts/check-api-doc-coverage.py            # report, exit 1 when gaps exist
    python3 scripts/check-api-doc-coverage.py --soft     # report, always exit 0
    python3 scripts/check-api-doc-coverage.py --enums    # dump enum members as markdown
"""

import argparse
import pathlib
import re
import sys

PACKAGES = ["Md3.Avalonia", "Md3.Avalonia.Extra", "Md3.Avalonia.Icons", "Md3.Avalonia.Icons.Lite",
            "Md3.Avalonia.DataGrid"]
# 16k lines of generated glyph constants; the catalog is documented as a whole, not per glyph.
SKIP_FILES = {"MdSymbols.cs", "MdSymbolsLite.cs"}
DOC = pathlib.Path("docs/API.md")

BLOCK_COMMENT = re.compile(r"/\*.*?\*/", re.S)
LINE_COMMENT = re.compile(r"//[^\n]*")
TYPE_DECL = re.compile(
    r"^\s*public\s+(?:sealed\s+|abstract\s+|static\s+|partial\s+|readonly\s+)*"
    r"(class|enum|interface|struct|record\s+struct|record\s+class|record)\s+"
    r"([A-Za-z_]\w*)",
    re.M,
)
ENUM_DECL = re.compile(r"public\s+enum\s+([A-Za-z_]\w*)\s*(?::\s*\w+\s*)?\{([^}]*)\}", re.S)


def strip_comments(text: str) -> str:
    return LINE_COMMENT.sub("", BLOCK_COMMENT.sub("", text))


def source_files():
    for package in PACKAGES:
        root = pathlib.Path("src") / package
        if not root.exists():
            continue
        for path in sorted(root.rglob("*.cs")):
            if path.name in SKIP_FILES:
                continue
            yield package, path


def collect():
    types: dict[str, tuple[str, str]] = {}
    enums: dict[str, tuple[str, list[str]]] = {}
    for package, path in source_files():
        body = strip_comments(path.read_text(encoding="utf-8", errors="ignore"))
        for kind, name in TYPE_DECL.findall(body):
            types.setdefault(name, (" ".join(kind.split()), package))
        for name, members in ENUM_DECL.findall(body):
            values = [v.split("=")[0].strip() for v in members.split(",")]
            values = [v for v in values if re.fullmatch(r"[A-Za-z_]\w*", v or "")]
            if values:
                enums[name] = (package, values)
    return types, enums


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--soft", action="store_true", help="always exit 0")
    parser.add_argument("--enums", action="store_true", help="dump enum members as markdown")
    args = parser.parse_args()

    if not DOC.exists():
        print(f"error: {DOC} is missing", file=sys.stderr)
        return 1

    types, enums = collect()
    doc = DOC.read_text(encoding="utf-8")

    if args.enums:
        for name in sorted(enums):
            package, values = enums[name]
            print(f"| `{name}` | {' · '.join('`' + v + '`' for v in values)} |")
        return 0

    missing = sorted(n for n in types if not re.search(rf"\b{re.escape(n)}\b", doc))
    covered = len(types) - len(missing)
    print(
        f"api doc coverage: types={len(types)} documented={covered} "
        f"missing={len(missing)} doc={DOC}"
    )
    if missing:
        for name in missing:
            kind, package = types[name]
            print(f"  missing: {name} ({kind}, {package})")
    return 0 if args.soft or not missing else 1


if __name__ == "__main__":
    raise SystemExit(main())
