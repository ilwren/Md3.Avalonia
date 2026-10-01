#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Build the release NuGet packages for Md3.Avalonia, Md3.Avalonia.Icons,
Md3.Avalonia.Icons.Lite, and Md3.Avalonia.Extra.

Usage:
  scripts/build-nuget.sh [--output /absolute/or/relative/path]
                         [--configuration Release]
                         [--no-restore]

A .NET 10 SDK is required.
EOF
}

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIGURATION="Release"
OUTPUT="$ROOT/artifacts/nuget"
RESTORE=1

while (($#)); do
  case "$1" in
    --font)
      [[ $# -ge 2 ]] || { echo "error: --font requires a path" >&2; exit 2; }
      shift 2 ;;
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
)

cleanup_intermediate() {
  find "$ROOT/src/Md3.Avalonia" \
       "$ROOT/src/Md3.Avalonia.Icons" \
       "$ROOT/src/Md3.Avalonia.Icons.Lite" \
       "$ROOT/src/Md3.Avalonia.Extra" \
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

PACKAGE_COUNT="$(find "$OUTPUT" -maxdepth 1 -type f \( -name '*.nupkg' -o -name '*.snupkg' \) | wc -l | tr -d '[:space:]')"
if ((PACKAGE_COUNT < 4)); then
  echo "error: expected at least 4 package files, found $PACKAGE_COUNT in $OUTPUT" >&2
  exit 1
fi

printf '\nBuilt packages (%s):\n' "$SDK_VERSION"
find "$OUTPUT" -maxdepth 1 -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -print \
  | while IFS= read -r package; do printf '  %s\n' "$(basename "$package")"; done \
  | sort
printf '\nOutput: %s\n' "$OUTPUT"
