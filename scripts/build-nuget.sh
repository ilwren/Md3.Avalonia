#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Build the release NuGet packages for Md3.Avalonia, Md3.Avalonia.Icons,
Md3.Avalonia.Icons.Lite, Md3.Avalonia.Extra, Md3.Avalonia.DataGrid and
Md3.Avalonia.RichEditor.

Usage:
  scripts/build-nuget.sh [--output /absolute/or/relative/path]
                         [--configuration Release]
                         [--no-restore]
                         [--keep-intermediate]

By default the script deletes bin/ and obj/ for the six packaged projects before and
after packing, so a release artifact can never pick up a stale intermediate. Pass
--keep-intermediate when a CI job has already produced the same configuration and the
run is a packaging *check* rather than a release build; that reuses the existing
compilation instead of paying for a second full rebuild.

The repository includes the pinned official Material Symbols Rounded fonts.
Their provenance and checksums are verified before packaging.

A .NET 10 SDK is required.
EOF
}

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIGURATION="Release"
OUTPUT="$ROOT/artifacts/nuget"
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
       -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
}
trap cleanup_intermediate EXIT

cleanup_intermediate
rm -rf "$OUTPUT"
mkdir -p "$OUTPUT"

if ((RESTORE)); then
  for proj in "${PROJECTS[@]}"; do
    dotnet restore "$proj" --nologo
  done
fi

NO_RESTORE=(--no-restore)
for proj in "${PROJECTS[@]}"; do
  dotnet build "$proj" -c "$CONFIGURATION" "${NO_RESTORE[@]}" --nologo
  dotnet pack "$proj" -c "$CONFIGURATION" --no-build --no-restore --nologo -o "$OUTPUT"
done

VERSION="$(sed -n 's:.*<Version>\([^<]*\)</Version>.*:\1:p' "${PROJECTS[0]}" | head -1)"
[[ -n "$VERSION" ]] || { echo "error: package version is missing" >&2; exit 1; }
PACKAGE_IDS=(Md3.Avalonia Md3.Avalonia.Icons Md3.Avalonia.Icons.Lite Md3.Avalonia.Extra Md3.Avalonia.DataGrid Md3.Avalonia.RichEditor)
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
