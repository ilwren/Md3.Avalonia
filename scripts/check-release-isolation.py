#!/usr/bin/env python3
"""Guard the Avalonia 11 release line against asset and application-id collisions."""

from pathlib import Path
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
errors: list[str] = []


def require_text(relative_path: str, *needles: str) -> str:
    text = (ROOT / relative_path).read_text(encoding="utf-8")
    for needle in needles:
        if needle not in text:
            errors.append(f"{relative_path}: missing {needle!r}")
    return text


def reject_text(relative_path: str, *needles: str) -> None:
    text = (ROOT / relative_path).read_text(encoding="utf-8")
    for needle in needles:
        if needle in text:
            errors.append(f"{relative_path}: forbidden overwrite path {needle!r}")


workflow = require_text(
    ".github/workflows/publish-release.yml",
    'TAG="v${VERSION}-avalonia11"',
    'RELEASE_NAME="Md3.Avalonia11"',
    'RELEASE_TARGET="main"',
    'ASSET_SUFFIX="-avalonia11"',
    'name: release-nuget${{ needs.prepare.outputs.asset_suffix }}',
    'gallery${{ needs.prepare.outputs.asset_suffix }}-${{ matrix.rid }}.zip',
    'name: gallery-android${{ needs.prepare.outputs.asset_suffix }}',
    'checksum_file="SHA256SUMS${RELEASE_ASSET_SUFFIX}.txt"',
    'echo "Release ${RELEASE_TAG} already exists; refusing to upload or replace assets." >&2',
    'gh release create "${release_args[@]}" release-bundle/*',
)
reject_text(".github/workflows/publish-release.yml", "gh release upload", "--clobber")
if workflow.count("gh release create") != 1:
    errors.append("publish-release.yml: expected exactly one Release creation command")

package_projects = sorted((ROOT / "src").glob("Md3.Avalonia*/*.csproj"))
package_ids: list[str] = []
for project in package_projects:
    package_id = ET.parse(project).getroot().findtext("./PropertyGroup/PackageId")
    if package_id:
        package_ids.append(package_id)
        if not package_id.startswith("Md3.Avalonia11"):
            errors.append(f"{project.relative_to(ROOT)}: non-isolated PackageId {package_id!r}")
if len(package_ids) != 17:
    errors.append(f"expected 17 versioned package projects, found {len(package_ids)}")

desktop_assembly = "Md3.Avalonia11.Gallery.Desktop"
desktop_project = ET.parse(
    ROOT / "gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj"
).getroot()
if desktop_project.findtext("./PropertyGroup/AssemblyName") != desktop_assembly:
    errors.append("desktop Gallery AssemblyName is not Avalonia 11-specific")
require_text(
    "gallery/Md3.Avalonia.Gallery/Properties/AssemblyInfo.cs",
    f'InternalsVisibleTo("{desktop_assembly}")',
)
require_text("scripts/publish-gallery-aot.sh", f'APP_NAME="{desktop_assembly}"')
require_text("scripts/publish-gallery-aot.ps1", f'$appName = "{desktop_assembly}"')
require_text(
    "gallery/Md3.Avalonia.Gallery/GalleryNotificationIdentity.cs",
    'WindowsAppUserModelId = "Md3.Avalonia11.Gallery"',
)
require_text(
    "gallery/Md3.Avalonia.Gallery.Desktop/WindowsToastRegistration.cs",
    '"Material 3 for Avalonia 11 Gallery.lnk"',
)

android_id = "io.arena.md3avalonia11.gallery"
android_project = ET.parse(
    ROOT / "gallery/Md3.Avalonia.Gallery.Android/Md3.Avalonia.Gallery.Android.csproj"
).getroot()
if android_project.findtext("./PropertyGroup/ApplicationId") != android_id:
    errors.append("Android Gallery ApplicationId is not Avalonia 11-specific")
maestro_flows = sorted((ROOT / "tests/maestro").glob("*.yaml"))
if not maestro_flows:
    errors.append("no Maestro flows found")
for flow in maestro_flows:
    app_ids = [
        line.split(":", 1)[1].strip()
        for line in flow.read_text(encoding="utf-8").splitlines()
        if line.startswith("appId:")
    ]
    if app_ids != [android_id]:
        errors.append(f"{flow.relative_to(ROOT)}: expected appId {android_id}, found {app_ids}")

if errors:
    for error in errors:
        print(f"error: {error}", file=sys.stderr)
    raise SystemExit(1)

print(
    "Release isolation checks passed: "
    f"{len(package_ids)} NuGet IDs, desktop/AOT and Windows identities, "
    f"Android ApplicationId across {len(maestro_flows)} Maestro flows, "
    "separate Release assets, and no overwrite command."
)
