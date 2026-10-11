#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Publish the desktop Gallery as a native AOT binary.

Usage:
  scripts/publish-gallery-aot.sh [--rid linux-x64|win-x64|osx-x64|osx-arm64]
                                 [--output /absolute/or/relative/path]
                                 [--configuration Release]
                                 [--smoke]
                                 [--keep-intermediate]

The publish asserts what an AOT publish must produce: a native executable for the
target RID and no managed copy of the app assembly beside it. Compiling says almost
nothing about native AOT - it fails at runtime on reflection that was trimmed away -
so --smoke starts the binary under xvfb (Linux only, requires xvfb-run and the X
libraries) and passes only if the window is still alive when the timeout fires.

Native linking needs the platform toolchain: clang + zlib on Linux, the MSVC or
Xcode toolchain on Windows/macOS. A .NET 10 SDK is required.

By default bin/ and obj/ of every project in the build graph are deleted afterwards,
so the workspace never carries intermediates; --keep-intermediate skips that.
EOF
}

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj"

case "$(uname -s)" in
  Linux*)   DEFAULT_RID="linux-x64" ;;
  Darwin*)  DEFAULT_RID="osx-x64" ;;
  MINGW*|MSYS*|CYGWIN*) DEFAULT_RID="win-x64" ;;
  *)        DEFAULT_RID="linux-x64" ;;
esac

RID="$DEFAULT_RID"
OUTPUT="$ROOT/artifacts/aot"
CONFIGURATION="Release"
SMOKE=0
KEEP_INTERMEDIATE=0

while (($#)); do
  case "$1" in
    --rid)
      [[ $# -ge 2 ]] || { echo "error: --rid requires a value" >&2; exit 2; }
      RID="$2"; shift 2 ;;
    --output)
      [[ $# -ge 2 ]] || { echo "error: --output requires a path" >&2; exit 2; }
      OUTPUT="$2"; shift 2 ;;
    --configuration)
      [[ $# -ge 2 ]] || { echo "error: --configuration requires a value" >&2; exit 2; }
      CONFIGURATION="$2"; shift 2 ;;
    --smoke)
      SMOKE=1; shift ;;
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

cleanup_intermediate() {
  # Must return 0 on every path: this runs both directly and from the EXIT trap under `set -e`.
  if ((KEEP_INTERMEDIATE)); then
    return 0
  fi
  find "$ROOT/src" "$ROOT/gallery" -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
}
trap cleanup_intermediate EXIT

rm -rf "$OUTPUT"
mkdir -p "$OUTPUT"

echo "Publishing native AOT ($RID) with SDK $SDK_VERSION ..."
dotnet publish "$PROJECT" \
  -c "$CONFIGURATION" -r "$RID" \
  -p:PublishAot=true \
  -p:TreatWarningsAsErrors=false \
  --nologo -o "$OUTPUT"

APP_NAME="Md3.Avalonia.Gallery.Desktop"
if [[ "$RID" == win-* ]]; then BIN="$OUTPUT/$APP_NAME.exe"; else BIN="$OUTPUT/$APP_NAME"; fi

[[ -f "$BIN" ]] || { echo "error: no native binary was produced at $BIN" >&2; exit 1; }

# An AOT publish ships the app as machine code, not as a managed assembly.
MANAGED="$OUTPUT/$APP_NAME.dll"
if [[ -f "$MANAGED" ]]; then
  echo "error: managed assembly still present at $MANAGED - this was not an AOT publish" >&2
  exit 1
fi

if [[ "$RID" != win-* ]] && command -v file >/dev/null 2>&1; then
  DESC="$(file -b "$BIN")"
  echo "binary: $DESC ($(stat -c%s "$BIN" 2>/dev/null || stat -f%z "$BIN") bytes)"
  case "$DESC" in
    *ELF*executable*) ;;
    *Mach-O*executable*) ;;
    *) echo "error: not a native executable: $DESC" >&2; exit 1 ;;
  esac
fi

if ((SMOKE)); then
  if [[ "$RID" != linux-* ]]; then
    echo "note: --smoke is only implemented on Linux; skipping" >&2
  else
    command -v xvfb-run >/dev/null 2>&1 || { echo "error: --smoke requires xvfb-run" >&2; exit 1; }
    echo "Smoke-running the native binary for 25 s under xvfb ..."
    set +e
    xvfb-run -a timeout 25 "$BIN" > "$OUTPUT/smoke.log" 2>&1
    CODE=$?
    set -e
    tail -20 "$OUTPUT/smoke.log" || true
    # 124 = still running when the timeout fired, which is a pass: the themes, the compiled
    # XAML and the icon font all resolved at runtime.
    if [[ "$CODE" != "124" ]]; then
      echo "error: the AOT binary exited on its own with code $CODE" >&2
      exit 1
    fi
    echo "smoke: window still alive after 25 s - runtime AOT OK"
  fi
fi

printf '\nNative AOT publish: %s\n' "$BIN"
