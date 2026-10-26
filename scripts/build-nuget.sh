#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Build the release NuGet packages for Md3.Avalonia, Md3.Avalonia.Icons,
Md3.Avalonia.Icons.Lite, Md3.Avalonia.Extra, Md3.Avalonia.DataGrid,
Md3.Avalonia.RichEditor, Md3.Avalonia.AvaloniaEdit, Md3.Avalonia.AvaloniaHex,
Md3.Avalonia.Dock, Md3.Avalonia.Tabalonia, Md3.Avalonia.Markdown,
Md3.Avalonia.LiveCharts2, Md3.Avalonia.ScottPlot,
Md3.Avalonia.PropertyGrid, Md3.Avalonia.NodifyM, Md3.Avalonia.Mapsui, Md3.Avalonia.InkCanvas,
Md3.Avalonia.Notifications and Md3.Avalonia.LibVLC.

Usage:
  scripts/build-nuget.sh [--output /absolute/or/relative/path]
                         [--configuration Release]
                         [--version 0.4.6-preview.1]
                         [--no-restore]
                         [--keep-intermediate]

By default the script deletes bin/ and obj/ for the nineteen packaged projects before and
after packing, so a release artifact can never pick up a stale intermediate. Pass
--keep-intermediate when a CI job has already produced the same configuration and the
run is a packaging *check* rather than a release build; that reuses the existing
compilation instead of paying for a second full rebuild.

The repository includes the pinned official Material Symbols Rounded fonts.
Their provenance and checksums are verified before packaging.

A .NET 10 SDK, Java 17 and the .NET Android workload are required (dotnet workload install android).
EOF
}

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIGURATION="Release"
OUTPUT="$ROOT/artifacts/nuget"
PACKAGE_VERSION=""
RESTORE=1
KEEP_INTERMEDIATE=0

while (($#)); do
  case "$1" in
    --output)
      [[ $# -ge 2 ]] || { echo "error: --output requires a path" >&2; exit 2; }
      OUTPUT="$2"; shift 2 ;;
    --configuration)
      [[ $# -ge 2 ]] || { echo "error: --configuration requires a value" >&2; exit 2; }
      CONFIGURATION="$2"; shift 2 ;;
    --version)
      [[ $# -ge 2 ]] || { echo "error: --version requires a value" >&2; exit 2; }
      PACKAGE_VERSION="$2"; shift 2 ;;
    --no-restore)
      RESTORE=0; shift ;;
    --keep-intermediate)
      KEEP_INTERMEDIATE=1; shift ;;
    -h|--help)
      usage; exit 0 ;;
    *)
      echo "error: unknown argument '$1'" >&2
      usage >&2
      exit 2 ;;
  esac
done

if [[ "$OUTPUT" != /* ]]; then OUTPUT="$ROOT/$OUTPUT"; fi

if [[ -n "$PACKAGE_VERSION" ]] && [[ ! "$PACKAGE_VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$ ]]; then
  echo "error: invalid package version '$PACKAGE_VERSION' (expected SemVer such as 0.4.6 or 0.4.6-preview.1)" >&2
  exit 2
fi
VERSION_ARGS=()
if [[ -n "$PACKAGE_VERSION" ]]; then
  VERSION_ARGS=("-p:Version=$PACKAGE_VERSION")
fi

command -v dotnet >/dev/null 2>&1 || { echo "error: dotnet was not found in PATH" >&2; exit 1; }
SDK_VERSION="$(dotnet --version)"
SDK_MAJOR="${SDK_VERSION%%.*}"
if [[ ! "$SDK_MAJOR" =~ ^[0-9]+$ ]] || ((SDK_MAJOR < 10)); then
  echo "error: .NET SDK 10 or newer is required; selected SDK is $SDK_VERSION" >&2
  exit 1
fi

PROJECTS=(
  "$ROOT/src/Md3.Avalonia/Md3.Avalonia.csproj"
  "$ROOT/src/Md3.Avalonia.Icons/Md3.Avalonia.Icons.csproj"
  "$ROOT/src/Md3.Avalonia.Icons.Lite/Md3.Avalonia.Icons.Lite.csproj"
  "$ROOT/src/Md3.Avalonia.Extra/Md3.Avalonia.Extra.csproj"
  "$ROOT/src/Md3.Avalonia.DataGrid/Md3.Avalonia.DataGrid.csproj"
  "$ROOT/src/Md3.Avalonia.RichEditor/Md3.Avalonia.RichEditor.csproj"
  "$ROOT/src/Md3.Avalonia.AvaloniaEdit/Md3.Avalonia.AvaloniaEdit.csproj"
  "$ROOT/src/Md3.Avalonia.AvaloniaHex/Md3.Avalonia.AvaloniaHex.csproj"
  "$ROOT/src/Md3.Avalonia.Dock/Md3.Avalonia.Dock.csproj"
  "$ROOT/src/Md3.Avalonia.Tabalonia/Md3.Avalonia.Tabalonia.csproj"
  "$ROOT/src/Md3.Avalonia.Markdown/Md3.Avalonia.Markdown.csproj"
  "$ROOT/src/Md3.Avalonia.LiveCharts2/Md3.Avalonia.LiveCharts2.csproj"
  "$ROOT/src/Md3.Avalonia.ScottPlot/Md3.Avalonia.ScottPlot.csproj"
  "$ROOT/src/Md3.Avalonia.PropertyGrid/Md3.Avalonia.PropertyGrid.csproj"
  "$ROOT/src/Md3.Avalonia.NodifyM/Md3.Avalonia.NodifyM.csproj"
  "$ROOT/src/Md3.Avalonia.Mapsui/Md3.Avalonia.Mapsui.csproj"
  "$ROOT/src/Md3.Avalonia.InkCanvas/Md3.Avalonia.InkCanvas.csproj"
  "$ROOT/src/Md3.Avalonia.Notifications/Md3.Avalonia.Notifications.csproj"
  "$ROOT/src/Md3.Avalonia.LibVLC/Md3.Avalonia.LibVLC.csproj"
)

if command -v python3 >/dev/null 2>&1; then
  python3 "$ROOT/scripts/verify-fonts.py"
elif command -v python >/dev/null 2>&1; then
  python "$ROOT/scripts/verify-fonts.py"
else
  echo "error: Python is required to verify the embedded official fonts" >&2
  exit 1
fi

cleanup_intermediate() {
  # Must return 0 on every path: this runs both directly and from the EXIT trap under `set -e`.
  if ((KEEP_INTERMEDIATE)); then
    return 0
  fi
  find "$ROOT/src/Md3.Avalonia" \
       "$ROOT/src/Md3.Avalonia.Icons" \
       "$ROOT/src/Md3.Avalonia.Icons.Lite" \
       "$ROOT/src/Md3.Avalonia.Extra" \
       "$ROOT/src/Md3.Avalonia.DataGrid" \
       "$ROOT/src/Md3.Avalonia.RichEditor" \
       "$ROOT/src/Md3.Avalonia.AvaloniaEdit" \
       "$ROOT/src/Md3.Avalonia.AvaloniaHex" \
       "$ROOT/src/Md3.Avalonia.Dock" \
       "$ROOT/src/Md3.Avalonia.Tabalonia" \
       "$ROOT/src/Md3.Avalonia.Markdown" \
       "$ROOT/src/Md3.Avalonia.LiveCharts2" \
       "$ROOT/src/Md3.Avalonia.ScottPlot" \
       "$ROOT/src/Md3.Avalonia.PropertyGrid" \
       "$ROOT/src/Md3.Avalonia.NodifyM" \
       "$ROOT/src/Md3.Avalonia.Mapsui" \
       "$ROOT/src/Md3.Avalonia.InkCanvas" \
       "$ROOT/src/Md3.Avalonia.Notifications" \
       "$ROOT/src/Md3.Avalonia.LibVLC" \
       -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
}
trap cleanup_intermediate EXIT

cleanup_intermediate
rm -rf "$OUTPUT"
mkdir -p "$OUTPUT"

project_args_for() {
  if [[ "$1" == "$ROOT/src/Md3.Avalonia.Notifications/Md3.Avalonia.Notifications.csproj" ]]; then
    printf '%s\n' '-p:IncludeAndroidTarget=true'
  fi
}

if ((RESTORE)); then
  for proj in "${PROJECTS[@]}"; do
    mapfile -t PROJECT_ARGS < <(project_args_for "$proj")
    dotnet restore "$proj" --nologo "${PROJECT_ARGS[@]}" "${VERSION_ARGS[@]}"
  done
fi

NO_RESTORE=(--no-restore)
for proj in "${PROJECTS[@]}"; do
  mapfile -t PROJECT_ARGS < <(project_args_for "$proj")
  dotnet build "$proj" -c "$CONFIGURATION" "${NO_RESTORE[@]}" --nologo "${PROJECT_ARGS[@]}" "${VERSION_ARGS[@]}"
  dotnet pack "$proj" -c "$CONFIGURATION" --no-build --no-restore --nologo -o "$OUTPUT" "${PROJECT_ARGS[@]}" "${VERSION_ARGS[@]}"
done

VERSION="${PACKAGE_VERSION:-$(sed -n 's:.*<Version>\([^<]*\)</Version>.*:\1:p' "${PROJECTS[0]}" | head -1)}"
[[ -n "$VERSION" ]] || { echo "error: package version is missing" >&2; exit 1; }
PACKAGE_IDS=(Md3.Avalonia Md3.Avalonia.Icons Md3.Avalonia.Icons.Lite Md3.Avalonia.Extra Md3.Avalonia.DataGrid Md3.Avalonia.RichEditor Md3.Avalonia.AvaloniaEdit Md3.Avalonia.AvaloniaHex Md3.Avalonia.Dock Md3.Avalonia.Tabalonia Md3.Avalonia.Markdown Md3.Avalonia.LiveCharts2 Md3.Avalonia.ScottPlot Md3.Avalonia.PropertyGrid Md3.Avalonia.NodifyM Md3.Avalonia.Mapsui Md3.Avalonia.InkCanvas Md3.Avalonia.Notifications Md3.Avalonia.LibVLC)
for package_id in "${PACKAGE_IDS[@]}"; do
  for extension in nupkg snupkg; do
    package="$OUTPUT/$package_id.$VERSION.$extension"
    [[ -f "$package" ]] || { echo "error: expected package is missing: $package" >&2; exit 1; }
  done
done

# Derived rather than written down, so adding a package cannot leave this behind: one .nupkg
# and one .snupkg each.
EXPECTED_COUNT=$((${#PACKAGE_IDS[@]} * 2))
PACKAGE_COUNT="$(find "$OUTPUT" -maxdepth 1 -type f \( -name '*.nupkg' -o -name '*.snupkg' \) | wc -l | tr -d '[:space:]')"
if ((PACKAGE_COUNT != EXPECTED_COUNT)); then
  echo "error: expected exactly $EXPECTED_COUNT package files for version $VERSION, found $PACKAGE_COUNT in $OUTPUT" >&2
  exit 1
fi

printf '\nBuilt packages (%s):\n' "$SDK_VERSION"
find "$OUTPUT" -maxdepth 1 -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -print \
  | while IFS= read -r package; do printf '  %s\n' "$(basename "$package")"; done \
  | sort
printf '\nOutput: %s\n' "$OUTPUT"
