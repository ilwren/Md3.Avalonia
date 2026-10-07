#!/usr/bin/env python3
"""Assert that every packed library carries exactly the expected target frameworks.

A green pack step only proves the files landed in the output directory. It says nothing about
what is inside them: a package can lose a target framework, or one can be added without the
support policy being updated, and both look identical from the outside. This script opens each
.nupkg and .snupkg and compares the set of target frameworks under ``lib/`` against the set the
build script declared with ``--expect``.

Usage:
    verify-package-assets.py --dir artifacts/nuget --version 0.4.1-preview.1 \
        --expect Md3.Avalonia=net8.0,net10.0 \
        --expect Md3.Avalonia.RichEditor=net10.0

Exits non-zero on the first mismatch; the message lists what was expected and what was found so
the failure is actionable from a CI annotation.
"""

from __future__ import annotations

import argparse
import sys
import zipfile
from pathlib import Path


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dir", required=True, help="directory holding the packed .nupkg files")
    parser.add_argument("--version", required=True, help="package version, used to build file names")
    parser.add_argument(
        "--expect",
        action="append",
        required=True,
        metavar="PACKAGE=TFM[,TFM...]",
        help="expected lib/ target frameworks for one package id; repeat per package",
    )
    return parser.parse_args()


def parse_expectations(entries: list[str]) -> dict[str, set[str]]:
    expectations: dict[str, set[str]] = {}
    for entry in entries:
        package_id, _, frameworks = entry.partition("=")
        if not package_id or not frameworks:
            raise SystemExit(f"error: --expect needs PACKAGE=TFM[,TFM...], got {entry!r}")
        expectations[package_id] = {value.strip() for value in frameworks.split(",") if value.strip()}
    return expectations


def lib_frameworks(package: Path) -> tuple[set[str], set[str]]:
    """Return (frameworks, empty_frameworks) for the target frameworks under lib/.

    A regular package carries the assembly (``.dll``) per framework; a symbol package carries
    the matching ``.pdb``. Either way an empty framework folder is a packaging bug, so the
    payload extension is what is asserted.
    """
    payload = ".pdb" if package.suffix == ".snupkg" else ".dll"
    with zipfile.ZipFile(package) as archive:
        names = archive.namelist()
        frameworks = {
            name.split("/")[1]
            for name in names
            if name.startswith("lib/") and len(name.split("/")) > 2 and name.split("/")[1]
        }
        empty = {
            framework
            for framework in frameworks
            if not any(
                name.startswith(f"lib/{framework}/") and name.endswith(payload) for name in names
            )
        }
    return frameworks, empty


def main() -> int:
    arguments = parse_arguments()
    expectations = parse_expectations(arguments.expect)
    output = Path(arguments.dir)
    failures: list[str] = []

    for package_id, expected in sorted(expectations.items()):
        for extension in ("nupkg", "snupkg"):
            package = output / f"{package_id}.{arguments.version}.{extension}"
            payload = ".pdb" if extension == "snupkg" else ".dll"
            if not package.is_file():
                failures.append(f"{package.name}: file is missing")
                continue
            found, empty = lib_frameworks(package)
            if found != expected:
                failures.append(
                    f"{package.name}: expected lib/ {{{', '.join(sorted(expected))}}}, "
                    f"found {{{', '.join(sorted(found)) or 'nothing'}}}"
                )
                continue
            for framework in sorted(empty):
                failures.append(f"{package.name}: lib/{framework}/ carries no {payload}")

    if failures:
        for failure in failures:
            print(f"error: {failure}", file=sys.stderr)
        return 1

    for package_id, expected in sorted(expectations.items()):
        frameworks = ", ".join(sorted(expected))
        print(f"package assets ok: {package_id} -> {frameworks}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
