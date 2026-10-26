#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Locate a desktop LibVLC installation and run the managed Gallery with its real video backend.

Usage:
  scripts/run-gallery-libvlc.sh [--vlc-home PATH] [--configuration Debug|Release]
                                [--no-build] [--check-only] [-- GALLERY_ARGS...]

VLC_HOME may point to VLC's install root or directly to the directory containing libvlc.
The script exports the platform loader path and VLC_PLUGIN_PATH only for the child Gallery.
It never downloads native binaries, invokes a package manager, or changes the shared Android/AOT graph.
EOF
}

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj"
CONFIGURATION="Debug"
VLC_HOME_VALUE="${VLC_HOME:-}"
NO_BUILD=0
CHECK_ONLY=0
GALLERY_ARGS=()

while (($#)); do
  case "$1" in
    --vlc-home)
      [[ $# -ge 2 ]] || { echo "error: --vlc-home requires a path" >&2; exit 2; }
      VLC_HOME_VALUE="$2"; shift 2 ;;
    --configuration)
      [[ $# -ge 2 ]] || { echo "error: --configuration requires a value" >&2; exit 2; }
      CONFIGURATION="$2"; shift 2 ;;
    --no-build) NO_BUILD=1; shift ;;
    --check-only) CHECK_ONLY=1; shift ;;
    --) shift; GALLERY_ARGS=("$@"); break ;;
    -h|--help) usage; exit 0 ;;
    *) echo "error: unknown argument '$1' (put Gallery arguments after --)" >&2; usage >&2; exit 2 ;;
  esac
done

command -v dotnet >/dev/null 2>&1 || { echo "error: dotnet was not found in PATH" >&2; exit 1; }
SDK_VERSION="$(dotnet --version)"
SDK_MAJOR="${SDK_VERSION%%.*}"
if [[ ! "$SDK_MAJOR" =~ ^[0-9]+$ ]] || ((SDK_MAJOR < 10)); then
  echo "error: .NET SDK 10 or newer is required; selected SDK is $SDK_VERSION" >&2
  exit 1
fi

find_file_dir() {
  local pattern="$1"; shift
  local candidate match
  for candidate in "$@"; do
    [[ -n "$candidate" && -d "$candidate" ]] || continue
    match="$(find "$candidate" -maxdepth 2 -type f -name "$pattern" -print -quit 2>/dev/null || true)"
    if [[ -n "$match" ]]; then dirname "$match"; return 0; fi
  done
  return 1
}

OS="$(uname -s)"
LIB_DIR=""
PLUGIN_DIR=""
case "$OS" in
  Linux*)
    candidates=(
      "$VLC_HOME_VALUE" "$VLC_HOME_VALUE/lib"
      /usr/lib/x86_64-linux-gnu /usr/lib/aarch64-linux-gnu /usr/lib64 /usr/lib /usr/local/lib
    )
    LIB_DIR="$(find_file_dir 'libvlc.so*' "${candidates[@]}" || true)"
    if [[ -z "$LIB_DIR" ]] && command -v ldconfig >/dev/null 2>&1; then
      lib_path="$(ldconfig -p 2>/dev/null | awk '/libvlc\.so/{print $NF; exit}')"
      [[ -n "$lib_path" ]] && LIB_DIR="$(dirname "$lib_path")"
    fi
    plugin_candidates=(
      "$VLC_HOME_VALUE/plugins" "$VLC_HOME_VALUE/vlc/plugins"
      "$LIB_DIR/vlc/plugins" /usr/lib/vlc/plugins /usr/local/lib/vlc/plugins
    )
    for candidate in "${plugin_candidates[@]}"; do
      if [[ -d "$candidate" ]]; then PLUGIN_DIR="$candidate"; break; fi
    done
    ;;
  Darwin*)
    candidates=(
      "$VLC_HOME_VALUE" "$VLC_HOME_VALUE/lib"
      /Applications/VLC.app/Contents/MacOS/lib
      /opt/homebrew/lib /usr/local/lib
    )
    LIB_DIR="$(find_file_dir 'libvlc*.dylib' "${candidates[@]}" || true)"
    plugin_candidates=(
      "$VLC_HOME_VALUE/plugins" "$VLC_HOME_VALUE/lib/vlc/plugins"
      /Applications/VLC.app/Contents/MacOS/plugins
      "$LIB_DIR/vlc/plugins"
    )
    for candidate in "${plugin_candidates[@]}"; do
      if [[ -d "$candidate" ]]; then PLUGIN_DIR="$candidate"; break; fi
    done
    ;;
  MINGW*|MSYS*|CYGWIN*)
    candidates=(
      "$VLC_HOME_VALUE"
      "${PROGRAMFILES:-}/VideoLAN/VLC"
      "${PROGRAMFILES_X86:-}/VideoLAN/VLC"
    )
    LIB_DIR="$(find_file_dir 'libvlc.dll' "${candidates[@]}" || true)"
    plugin_candidates=("$VLC_HOME_VALUE/plugins" "$LIB_DIR/plugins")
    for candidate in "${plugin_candidates[@]}"; do
      if [[ -d "$candidate" ]]; then PLUGIN_DIR="$candidate"; break; fi
    done
    ;;
  *) echo "error: unsupported host OS: $OS" >&2; exit 1 ;;
esac

if [[ -z "$LIB_DIR" ]]; then
  cat >&2 <<EOF
error: native LibVLC was not found.
Install VLC/LibVLC for this desktop OS, then rerun with --vlc-home PATH:
  Linux: install the distribution's libvlc and vlc-plugin packages
  macOS: install VLC.app, normally under /Applications
  Windows: install VLC, normally under Program Files/VideoLAN/VLC
The managed LibVLCSharp package alone does not contain the native decoder.
EOF
  exit 1
fi
if [[ -z "$PLUGIN_DIR" ]]; then
  echo "error: libvlc was found in '$LIB_DIR', but its plugins directory was not found; pass the VLC install root to --vlc-home" >&2
  exit 1
fi

case "$OS" in
  Linux*) export LD_LIBRARY_PATH="$LIB_DIR${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}" ;;
  Darwin*) export DYLD_LIBRARY_PATH="$LIB_DIR${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" ;;
  *) export PATH="$LIB_DIR${PATH:+:$PATH}" ;;
esac
export VLC_PLUGIN_PATH="$PLUGIN_DIR"

printf 'LibVLC library: %s\nLibVLC plugins: %s\n.NET SDK: %s\n' "$LIB_DIR" "$PLUGIN_DIR" "$SDK_VERSION"
((CHECK_ONLY)) && exit 0

args=(run --project "$PROJECT" --configuration "$CONFIGURATION" -p:PublishAot=false)
((NO_BUILD)) && args+=(--no-build)
args+=(-- "${GALLERY_ARGS[@]}")
exec dotnet "${args[@]}"
