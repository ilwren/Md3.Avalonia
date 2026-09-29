#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Build the three release NuGet packages for net8.0 and net10.0.

Usage:
  scripts/build-nuget.sh [--font /absolute/path/MaterialSymbolsRounded.ttf]
                         [--output /absolute/or/relative/path]
                         [--configuration Release]
                         [--no-restore]

The official Material Symbols Rounded font is not stored in this repository. Either:
  1. copy it to src/Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded.ttf; or
  2. pass --font with an offline .ttf/.otf file.

A .NET 10 SDK is required. It can build both net8.0 and net10.0 targets.
EOF
}

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIGURATION="Release"
OUTPUT="$ROOT/artifacts/nuget"
FONT="$ROOT/src/Md3.Avalonia.Icons/Assets/Fonts/MaterialSymbolsRounded.ttf"
RESTORE=1

while (($#)); do
  case "$1" in
    --font)
      [[ $# -ge 2 ]] || { echo "error: --font requires a path" >&2; exit 2; }
      FONT="$2"; shift 2 ;;
    --output)
      [[ $# -ge 2 ]] || { echo "error: --output requires a path" >&2; exit 2; }
      OUTPUT="$2"; shift 2 ;;
    --configuration)
      [[ $# -ge 2 ]] || { echo "error: --configuration requires a value" >&2; exit 2; }
      CONFIGURATION="$2"; shift 2 ;;
    --no-restore)
      RESTORE=0; shift ;;
    -h|--help)
      usage; exit 0 ;;
    *)
      echo "error: unknown argument '$1'" >&2
      usage >&2
      exit 2 ;;
  esac
done

if [[ "$FONT" != /* ]]; then FONT="$ROOT/$FONT"; fi
if [[ "$OUTPUT" != /* ]]; then OUTPUT="$ROOT/$OUTPUT"; fi

command -v dotnet >/dev/null 2>&1 || { echo "error: dotnet was not found in PATH" >&2; exit 1; }
SDK_VERSION="$(dotnet --version)"
SDK_MAJOR="${SDK_VERSION%%.*}"
if [[ ! "$SDK_MAJOR" =~ ^[0-9]+$ ]] || ((SDK_MAJOR < 10)); then
  echo "error: .NET SDK 10 or newer is required; selected SDK is $SDK_VERSION" >&2
  exit 1
fi

if [[ ! -f "$FONT" ]]; then
  cat >&2 <<EOF
error: Material Symbols Rounded font is missing.
Expected: $FONT
Copy the official font into the reserved slot or pass:
  scripts/build-nuget.sh --font /offline/path/MaterialSymbolsRounded.ttf
EOF
  exit 1
fi

FONT_LOWER="$(printf '%s' "$FONT" | tr '[:upper:]' '[:lower:]')"
case "$FONT_LOWER" in
  *.ttf|*.otf) ;;
  *) echo "error: the font must be a .ttf or .otf file: $FONT" >&2; exit 1 ;;
esac

FONT_SIZE="$(wc -c < "$FONT" | tr -d '[:space:]')"
if ((FONT_SIZE < 100000)); then
  echo "error: '$FONT' is too small to be the official Material Symbols font ($FONT_SIZE bytes)" >&2
  exit 1
fi

FONT_MAGIC="$(od -An -tx1 -N4 "$FONT" | tr -d ' \n')"
case "$FONT_MAGIC" in
  00010000|4f54544f|74727565|74797031) ;;
  *) echo "error: '$FONT' does not have a supported OpenType/TrueType header" >&2; exit 1 ;;
esac

PROJECTS=(
  "$ROOT/src/Md3.Avalonia/Md3.Avalonia.csproj"
  "$ROOT/src/Md3.Avalonia.Icons/Md3.Avalonia.Icons.csproj"
  "$ROOT/src/Md3.Avalonia.Ecosystem/Md3.Avalonia.Ecosystem.csproj"
)
FONT_PROPERTY="-p:MaterialSymbolsRoundedFontFile=$FONT"

cleanup_intermediate() {
  find "$ROOT/src/Md3.Avalonia" \
       "$ROOT/src/Md3.Avalonia.Icons" \
       "$ROOT/src/Md3.Avalonia.Ecosystem" \
       -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
}
trap cleanup_intermediate EXIT

cleanup_intermediate
rm -rf "$OUTPUT"
mkdir -p "$OUTPUT"

if ((RESTORE)); then
  dotnet restore "${PROJECTS[0]}" --nologo
  dotnet restore "${PROJECTS[1]}" --nologo "$FONT_PROPERTY"
  dotnet restore "${PROJECTS[2]}" --nologo
fi

NO_RESTORE=(--no-restore)
dotnet build "${PROJECTS[0]}" -c "$CONFIGURATION" "${NO_RESTORE[@]}" --nologo
dotnet build "${PROJECTS[1]}" -c "$CONFIGURATION" "${NO_RESTORE[@]}" --nologo "$FONT_PROPERTY"
dotnet build "${PROJECTS[2]}" -c "$CONFIGURATION" "${NO_RESTORE[@]}" --nologo

dotnet pack "${PROJECTS[0]}" -c "$CONFIGURATION" --no-build --no-restore --nologo -o "$OUTPUT"
dotnet pack "${PROJECTS[1]}" -c "$CONFIGURATION" --no-build --no-restore --nologo -o "$OUTPUT" "$FONT_PROPERTY"
dotnet pack "${PROJECTS[2]}" -c "$CONFIGURATION" --no-build --no-restore --nologo -o "$OUTPUT"

PACKAGE_COUNT="$(find "$OUTPUT" -maxdepth 1 -type f \( -name '*.nupkg' -o -name '*.snupkg' \) | wc -l | tr -d '[:space:]')"
if ((PACKAGE_COUNT != 6)); then
  echo "error: expected 6 package files, found $PACKAGE_COUNT in $OUTPUT" >&2
  exit 1
fi

printf '\nBuilt packages (%s):\n' "$SDK_VERSION"
find "$OUTPUT" -maxdepth 1 -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -print \
  | while IFS= read -r package; do printf '  %s\n' "$(basename "$package")"; done \
  | sort
printf '\nOutput: %s\n' "$OUTPUT"
printf 'Intermediate bin/obj directories are removed automatically.\n'
