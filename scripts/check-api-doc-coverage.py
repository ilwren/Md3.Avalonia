#!/usr/bin/env python3
"""Check the DocFX project set and bilingual XML documentation for public interfaces.

This static check reads C# sources and docfx.json directly; it needs no .NET SDK.

Usage:
    python3 scripts/check-api-doc-coverage.py            # fail when coverage is incomplete
    python3 scripts/check-api-doc-coverage.py --soft     # report, always exit 0
    python3 scripts/check-api-doc-coverage.py --enums    # dump enum members as markdown
"""

import argparse
import json
import pathlib
import re
import sys
import xml.etree.ElementTree as ET

PACKAGES = [
    "Md3.Avalonia",
    "Md3.Avalonia.Icons",
    "Md3.Avalonia.Icons.Lite",
    "Md3.Avalonia.Extra",
    "Md3.Avalonia.DataGrid",
    "Md3.Avalonia.AvaloniaEdit",
    "Md3.Avalonia.AvaloniaHex",
    "Md3.Avalonia.Dock",
    "Md3.Avalonia.Tabalonia",
    "Md3.Avalonia.Markdown",
    "Md3.Avalonia.LiveCharts2",
    "Md3.Avalonia.ScottPlot",
    "Md3.Avalonia.PropertyGrid",
    "Md3.Avalonia.NodifyM",
    "Md3.Avalonia.Mapsui",
    "Md3.Avalonia.InkCanvas",
    "Md3.Avalonia.Notifications",
]
EXPECTED_PROJECTS = {
    f"src/{package}/{package}.csproj"
    for package in PACKAGES
}
# Generated glyph catalogs are documented as a package feature, not per glyph constant.
SKIP_FILES = {"MdSymbols.cs", "MdSymbolsLite.cs"}
DOC = pathlib.Path("docs/API.md")
DOCFX_CONFIG = pathlib.Path("docfx.json")

BLOCK_COMMENT = re.compile(r"/\*.*?\*/", re.S)
LINE_COMMENT = re.compile(r"//[^\n]*")
TYPE_DECL = re.compile(
    r"^[ \t]*public[ \t]+(?:sealed[ \t]+|abstract[ \t]+|static[ \t]+|partial[ \t]+|readonly[ \t]+)*"
    r"(class|enum|interface|struct|record\s+struct|record\s+class|record)\s+"
    r"([A-Za-z_]\w*)",
    re.M,
)
ENUM_DECL = re.compile(r"public\s+enum\s+([A-Za-z_]\w*)\s*(?::\s*\w+\s*)?\{([^}]*)\}", re.S)


def _preserve_layout(match: re.Match[str]) -> str:
    return "".join("\n" if char == "\n" else " " for char in match.group(0))


def strip_comments_preserving_layout(text: str) -> str:
    """Hide comments from declaration matching without changing source offsets or lines."""
    return LINE_COMMENT.sub(_preserve_layout, BLOCK_COMMENT.sub(_preserve_layout, text))


def source_files():
    for package in PACKAGES:
        root = pathlib.Path("src") / package
        if not root.exists():
            continue
        for path in sorted(root.rglob("*.cs")):
            if path.name in SKIP_FILES or {"bin", "obj"}.intersection(path.parts):
                continue
            yield package, path


def collect():
    types: dict[str, tuple[str, str]] = {}
    enums: dict[str, tuple[str, list[str]]] = {}
    interfaces: list[tuple[str, pathlib.Path, str]] = []

    for package, path in source_files():
        original = path.read_text(encoding="utf-8", errors="ignore")
        visible = strip_comments_preserving_layout(original)
        raw_lines = original.splitlines()
        for match in TYPE_DECL.finditer(visible):
            kind, name = match.groups()
            types.setdefault(name, (" ".join(kind.split()), package))
            if kind == "interface":
                line_number = visible.count("\n", 0, match.start())
                comment_lines = []
                previous = line_number - 1
                while previous >= 0 and not raw_lines[previous].strip():
                    previous -= 1
                while previous >= 0 and raw_lines[previous].lstrip().startswith("///"):
                    comment_lines.insert(0, raw_lines[previous].lstrip()[3:].lstrip())
                    previous -= 1
                interfaces.append((name, path, "\n".join(comment_lines)))

        for name, members in ENUM_DECL.findall(visible):
            values = [value.split("=")[0].strip() for value in members.split(",")]
            values = [value for value in values if re.fullmatch(r"[A-Za-z_]\w*", value or "")]
            if values:
                enums[name] = (package, values)

    return types, enums, interfaces


def iter_xml_doc_blocks(text: str):
    lines = text.splitlines()
    index = 0
    while index < len(lines):
        if not lines[index].lstrip().startswith("///"):
            index += 1
            continue

        start_line = index + 1
        block = []
        while index < len(lines) and lines[index].lstrip().startswith("///"):
            block.append(lines[index].lstrip()[3:].lstrip())
            index += 1
        yield start_line, "\n".join(block)


def validate_xml_docs() -> list[str]:
    """Validate every XML doc-comment block across the packaged projects."""
    errors = []
    for _, path in source_files():
        text = path.read_text(encoding="utf-8", errors="ignore")
        if "///" not in text:
            continue
        for line_number, block in iter_xml_doc_blocks(text):
            try:
                ET.fromstring(f"<doc>\n{block}\n</doc>")
            except ET.ParseError as error:
                errors.append(f"invalid XML documentation in {path}:{line_number}: {error}")
    return errors


def configured_projects() -> set[str]:
    config = json.loads(DOCFX_CONFIG.read_text(encoding="utf-8"))
    configured = set()

    def visit(value):
        if isinstance(value, dict):
            for key, child in value.items():
                if key == "files":
                    values = [child] if isinstance(child, str) else child
                    if isinstance(values, list):
                        configured.update(item for item in values if isinstance(item, str))
                else:
                    visit(child)
        elif isinstance(value, list):
            for child in value:
                visit(child)

    visit(config.get("metadata", []))
    return configured


def missing_xml_projects() -> list[str]:
    missing = []
    for project in sorted(EXPECTED_PROJECTS):
        path = pathlib.Path(project)
        if not path.exists() or not re.search(
            r"<GenerateDocumentationFile>\s*true\s*</GenerateDocumentationFile>",
            path.read_text(encoding="utf-8", errors="ignore") if path.exists() else "",
            re.IGNORECASE,
        ):
            missing.append(project)
    return missing


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--soft", action="store_true", help="always exit 0")
    parser.add_argument("--enums", action="store_true", help="dump enum members as markdown")
    args = parser.parse_args()

    if not DOC.exists():
        print(f"error: {DOC} is missing", file=sys.stderr)
        return 0 if args.soft else 1
    if not DOCFX_CONFIG.exists():
        print(f"error: {DOCFX_CONFIG} is missing", file=sys.stderr)
        return 0 if args.soft else 1

    try:
        configured = configured_projects()
    except (json.JSONDecodeError, OSError) as error:
        print(f"error: cannot read {DOCFX_CONFIG}: {error}", file=sys.stderr)
        return 0 if args.soft else 1

    types, enums, interfaces = collect()
    if args.enums:
        for name in sorted(enums):
            _, values = enums[name]
            print(f"| `{name}` | {' · '.join('`' + value + '`' for value in values)} |")
        return 0

    missing_projects = sorted(EXPECTED_PROJECTS - configured)
    missing_xml = missing_xml_projects()
    missing_interfaces = []
    for name, path, docs in interfaces:
        if "<summary" not in docs or not re.search(r"[\u3400-\u9fff]", docs):
            missing_interfaces.append((name, path))

    xml_errors = validate_xml_docs()
    interface_count = len(interfaces)
    documented = interface_count - len(missing_interfaces)
    print(
        f"api doc coverage: docfx_projects={len(EXPECTED_PROJECTS) - len(missing_projects)}/"
        f"{len(EXPECTED_PROJECTS)}; xml_output_projects={len(EXPECTED_PROJECTS) - len(missing_xml)}/"
        f"{len(EXPECTED_PROJECTS)}; bilingual_interfaces={documented}/{interface_count}; "
        f"xml_errors={len(xml_errors)}; public_types_scanned={len(types)}"
    )

    if missing_projects:
        for project in missing_projects:
            print(f"  missing DocFX project: {project}")
    if missing_xml:
        for project in missing_xml:
            print(f"  XML documentation output is not enabled: {project}")
    if missing_interfaces:
        for name, path in missing_interfaces:
            print(f"  missing bilingual XML summary: {name} ({path})")
    for error in xml_errors:
        print(f"  {error}")

    success = not missing_projects and not missing_xml and not missing_interfaces and not xml_errors
    return 0 if args.soft or success else 1


if __name__ == "__main__":
    raise SystemExit(main())
