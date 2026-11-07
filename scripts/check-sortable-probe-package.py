#!/usr/bin/env python3
"""Verify the actual restored test-only package, not just upstream source metadata."""
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

path = Path(sys.argv[1])
root = ET.parse(path).getroot()
ns = {"n": root.tag.partition("}")[0].lstrip("{")}
metadata = root.find("n:metadata", ns)
if metadata is None:
    raise SystemExit("No NuGet package metadata")

def text(name):
    node = metadata.find("n:" + name, ns)
    return (node.text or "").strip() if node is not None else ""

package, version = text("id"), text("version")
if package != "Sortable.Avalonia" or version != "2.3.1":
    raise SystemExit(f"Unexpected probe package: {package} {version}")
groups = metadata.findall("n:dependencies/n:group", ns)
if not groups:
    raise SystemExit("No dependency groups in restored package")
for group in groups:
    deps = [d for d in group.findall("n:dependency", ns) if d.get("id", "").lower() == "avalonia"]
    if len(deps) != 1:
        raise SystemExit(f"Expected one Avalonia dependency for {group.get('targetFramework')}")
    declared = deps[0].get("version", "")
    # NuGet often writes minimum-only versions as either 12.0.0 or [12.0.0, ).
    if not re.match(r"^\[?12\.0\.0(?:\s*,\s*\))?$", declared):
        raise SystemExit(f"Unexpected Avalonia dependency: {declared}")
    print(f"::notice::Sortable.Avalonia {version} {group.get('targetFramework')}: Avalonia {declared}")
