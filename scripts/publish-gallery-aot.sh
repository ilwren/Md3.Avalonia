#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
  cat <<'EOF'
Publish the desktop Gallery with native AOT and prove it is what it claims to be.

Usage:
  scripts/publish-gallery-aot.sh [--rid linux-x64]
                                 [--output artifacts/aot/linux-x64]
                                 [--configuration Release]
                                 [--no-restore]
                                 [--allow-diagnostics]
                                 [--no-smoke]
                                 [--smoke-timeout 25]
                                 [--keep-intermediate]

What the script does, in order:

  1. Publishes gallery/Md3.Avalonia.Gallery.Desktop with -p:PublishAot=true.
  2. Fails unless the output is a native executable, and fails if a managed
     .dll is still sitting next to it - a publish that silently produced a
     framework-dependent app is the one failure mode a green build hides.
  3. Counts ILxxxx trim/AOT diagnostics in the publish log and fails on any
     of them. Native AOT fails at run time on reflection that was trimmed
     away, so "it compiled" is not the claim; zero diagnostics is.
  4. Smoke-runs the binary when a display can be provided (xvfb-run, or a
     real session): a window that is still up after the timeout has loaded
     the themes, the XAML and the icon font, which is the part ILC cannot
     prove. Exit code 124 means "still running when the timeout fired".

Options:
  --allow-diagnostics   Report ILxxxx diagnostics and continue instead of
                        failing. For a first look at a new call site, never
                        for the CI gate.
  --no-smoke            Skip step 4 (also the automatic outcome on a RID that
                        is not the host's, or when no display is available).
  --keep-intermediate   Keep bin/obj. The default removes them so an AOT
                        publish never leaves artifacts in the workspace.

Diagnostics are re-emitted as GitHub workflow annotations when the script
runs under Actions, so a failing call site is visible on the run page instead
of buried in a publish log.

A .NET 10 SDK is required, plus the native toolchain ILC links against
(clang and zlib1g-dev on Debian/Ubuntu).
EOF
}

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/gallery/Md3.Avalonia.Gallery.Desktop/Md3.Avalonia.Gallery.Desktop.csproj"

# The host RID is the useful default: an AOT publish can only be smoke-run on the
# machine that made it. uname reports "x86_64"/"aarch64"; .NET spells those "x64"/"arm64".
default_rid() {
  local machine arch
  machine="$(uname -m 2>/dev/null || echo unknown)"
  case "$machine" in
    x86_64|amd64) arch="x64" ;;
    aarch64|arm64) arch="arm64" ;;
    armv7l|armv6l) arch="arm" ;;
    riscv64) arch="riscv64" ;;
    *) arch="$machine" ;;
  esac
  case "$(uname -s 2>/dev/null || echo unknown)" in
    Linux)  echo "linux-$arch" ;;
    Darwin) echo "osx-$arch" ;;
    MINGW*|MSYS*|CYGWIN*) echo "win-$arch" ;;
    *) echo "linux-x64" ;;
  esac
}

RID="$(default_rid)"
CONFIGURATION="Release"
OUTPUT=""
RESTORE=1
ALLOW_DIAGNOSTICS=0
SMOKE=1
SMOKE_TIMEOUT=25
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
    --smoke-timeout)
      [[ $# -ge 2 ]] || { echo "error: --smoke-timeout requires a value" >&2; exit 2; }
      SMOKE_TIMEOUT="$2"; shift 2 ;;
    --no-restore) RESTORE=0; shift ;;
    --allow-diagnostics) ALLOW_DIAGNOSTICS=1; shift ;;
    --no-smoke) SMOKE=0; shift ;;
    --keep-intermediate) KEEP_INTERMEDIATE=1; shift ;;
    -h|--help) usage; exit 0 ;;
    *) echo "error: unknown argument '$1'" >&2; usage >&2; exit 2 ;;
  esac
done

[[ -f "$PROJECT" ]] || { echo "error: $PROJECT was not found" >&2; exit 1; }
if [[ -z "$OUTPUT" ]]; then OUTPUT="$ROOT/artifacts/aot/$RID"; fi
if [[ "$OUTPUT" != /* ]]; then OUTPUT="$ROOT/$OUTPUT"; fi

command -v dotnet >/dev/null 2>&1 || { echo "error: dotnet was not found in PATH" >&2; exit 1; }
SDK_VERSION="$(dotnet --version)"
SDK_MAJOR="${SDK_VERSION%%.*}"
if [[ ! "$SDK_MAJOR" =~ ^[0-9]+$ ]] || ((SDK_MAJOR < 10)); then
  echo "error: .NET SDK 10 or newer is required; selected SDK is $SDK_VERSION" >&2
  exit 1
fi

# The executable ILC emits is named after the assembly, which the project may
# override; fall back to the project file name.
BINARY_NAME="$(sed -n 's:.*<AssemblyName>\([^<]*\)</AssemblyName>.*:\1:p' "$PROJECT" | head -1)"
[[ -n "$BINARY_NAME" ]] || BINARY_NAME="$(basename "$PROJECT" .csproj)"
if [[ "$RID" == win-* ]]; then BINARY_NAME="$BINARY_NAME.exe"; fi

cleanup_intermediate() {
  # Must return 0 on every path: this runs directly and from the EXIT trap under `set -e`.
  if ((KEEP_INTERMEDIATE)); then
    return 0
  fi
  find "$ROOT/gallery/Md3.Avalonia.Gallery" "$ROOT/gallery/Md3.Avalonia.Gallery.Desktop" \
       -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
}
trap cleanup_intermediate EXIT

rm -rf "$OUTPUT"
mkdir -p "$OUTPUT"
LOG_DIR="$ROOT/artifacts/aot"
mkdir -p "$LOG_DIR"
LOG="$LOG_DIR/publish-$RID.log"

PUBLISH_ARGS=(-c "$CONFIGURATION" -r "$RID" -p:PublishAot=true -o "$OUTPUT")
# AOT publishes are not warning-free by construction: TreatWarningsAsErrors would stop the
# publish at the first analyzer diagnostic and hide whether the produced binary still runs.
# The diagnostics are counted below instead, and that count is what has to be zero.
PUBLISH_ARGS+=(-p:TreatWarningsAsErrors=false)
if ((RESTORE)); then
  PUBLISH_ARGS+=(--nologo)
else
  PUBLISH_ARGS+=(--no-restore --nologo)
fi

printf 'Publishing %s (%s, %s) with native AOT...\n' "$BINARY_NAME" "$RID" "$CONFIGURATION"
printf '  SDK:    %s\n' "$SDK_VERSION"
printf '  output: %s\n\n' "$OUTPUT"

# Without pipefail the `tee` would swallow a failing publish: the pipeline's exit status is
# tee's, and a build error would only surface later as a missing binary.
set -o pipefail
dotnet publish "$PROJECT" "${PUBLISH_ARGS[@]}" 2>&1 | tee "$LOG"

BINARY="$OUTPUT/$BINARY_NAME"
if [[ ! -e "$BINARY" ]]; then
  echo "error: no native binary was produced at $BINARY" >&2
  exit 1
fi
if [[ "$RID" == win-* ]]; then
  [[ -f "$BINARY" ]] || { echo "error: $BINARY is not a file" >&2; exit 1; }
else
  [[ -x "$BINARY" ]] || { echo "error: $BINARY is not executable" >&2; exit 1; }
fi

# Magic bytes rather than `file`, so the check works on a runner without the `file`
# package: a native AOT publish must not be a managed assembly, and a managed assembly
# starts with the PE/COFF "MZ" header exactly like the native Windows executable does.
# The discriminator that matters is the managed .dll the next check looks for.
MAGIC="$(od -An -tx1 -N4 "$BINARY" | tr -d ' \n')"
case "$MAGIC" in
  7f454c46)      KIND="ELF" ;;
  cffaedfe|feedfacf|cafebabe) KIND="Mach-O" ;;
  4d5a*)         KIND="PE" ;;
  *) echo "error: $BINARY does not start with a known executable header (got '$MAGIC')" >&2; exit 1 ;;
esac
if command -v file >/dev/null 2>&1; then
  printf 'Native binary: %s (%s)\n' "$(file -b "$BINARY")" "$KIND"
else
  printf 'Native binary: %s\n' "$KIND"
fi
printf 'Size: %s bytes\n' "$(wc -c < "$BINARY" | tr -d '[:space:]')"

MANAGED_DLLS="$(find "$OUTPUT" -maxdepth 1 -type f -name '*.dll' | wc -l | tr -d '[:space:]')"
if [[ "$MANAGED_DLLS" != "0" ]]; then
  echo "error: $MANAGED_DLLS managed .dll file(s) in $OUTPUT - this was not an AOT publish" >&2
  find "$OUTPUT" -maxdepth 1 -type f -name '*.dll' -print >&2
  exit 1
fi

# ---------------------------------------------------------------------------------------------
# ILxxxx diagnostics. The README claims a native-AOT publish with zero of them, so the count
# is asserted here rather than left as a number in a log nobody opens.
# ---------------------------------------------------------------------------------------------
DIAGNOSTICS=()
while IFS= read -r line; do
  [[ -n "$line" ]] && DIAGNOSTICS+=("$line")
done < <(grep -E '(warning|error) IL[0-9]{4}' "$LOG" 2>/dev/null | sed 's/^[[:space:]]*//' | sort -u || true)

summary_block() {
  if [[ -n "${GITHUB_STEP_SUMMARY:-}" ]]; then
    {
      echo "### Native AOT publish — $RID"
      echo
      echo "- binary: \`$BINARY_NAME\` ($KIND, $(wc -c < "$BINARY" | tr -d '[:space:]') bytes)"
      echo "- managed assemblies in output: $MANAGED_DLLS"
      echo "- IL diagnostics: ${#DIAGNOSTICS[@]}"
      echo
    } >>"$GITHUB_STEP_SUMMARY"
    if ((${#DIAGNOSTICS[@]})); then
      {
        echo '```'
        printf '%s\n' "${DIAGNOSTICS[@]}"
        echo '```'
        echo
      } >>"$GITHUB_STEP_SUMMARY"
    fi
  fi
}
summary_block

if ((${#DIAGNOSTICS[@]})); then
  printf '\nTrim/AOT diagnostics found (the count has to be zero):\n'
  printf '  %s\n' "${DIAGNOSTICS[@]}"
  # Re-emitted as annotations as well: a warning that names a file and a line lands on the
  # run page and in the API, so the person fixing it sees which call site is meant without
  # downloading the publish log. The runner keeps ten workflow-command annotations per step,
  # so a longer list gets one extra annotation carrying the remainder: the publish log is not
  # always reachable, the Checks API is.
  if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    emitted=0
    while IFS= read -r line; do
      [[ -z "$line" ]] && continue
      if [[ "$line" =~ ^(.+)\(([0-9]+)(,([0-9]+))?\):[[:space:]]+.*(warning|error)[[:space:]]+(IL[0-9]{4}):[[:space:]]*(.*)$ ]]; then
        file="${BASH_REMATCH[1]}"
        [[ "$file" == "$ROOT"/* ]] && file="${file#"$ROOT"/}"
        file="${file//%/%25}"; file="${file//$'\r'/%0D}"; file="${file//$'\n'/%0A}"
        code="${BASH_REMATCH[6]}"
        message="${BASH_REMATCH[7]//%/%25}"; message="${message//$'\r'/%0D}"; message="${message//$'\n'/%0A}"
        echo "::warning file=$file,line=${BASH_REMATCH[2]},col=${BASH_REMATCH[4]:-1}::$code: ${message:0:300}"
      else
        echo "::warning::${line:0:300}"
      fi
      emitted=$((emitted + 1))
      ((emitted >= 10)) && break
    done < <(printf '%s\n' "${DIAGNOSTICS[@]}")

    if ((${#DIAGNOSTICS[@]} > emitted)); then
      rest="$(printf '%s | ' "${DIAGNOSTICS[@]:emitted}")"
      rest="${rest//%/%25}"; rest="${rest//$'\r'/%0D}"; rest="${rest//$'\n'/%0A}"
      echo "::error::$((${#DIAGNOSTICS[@]} - emitted)) more of ${#DIAGNOSTICS[@]} IL diagnostics: ${rest:0:60000}"
    fi
  fi
  if ((ALLOW_DIAGNOSTICS)); then
    echo
    echo "warning: continuing despite the diagnostics because --allow-diagnostics was passed" >&2
  else
    echo "error: native AOT must not emit IL diagnostics; fix the call sites or re-run with --allow-diagnostics" >&2
    exit 1
  fi
else
  printf 'Trim/AOT diagnostics: 0\n'
fi

# ---------------------------------------------------------------------------------------------
# Smoke run. ILC compiling the app says nothing about reflection that is missing at run time,
# which is how native AOT actually fails.
# ---------------------------------------------------------------------------------------------
if ((SMOKE)); then
  HOST_RID="$(default_rid)"
  RUNNER=()
  if command -v xvfb-run >/dev/null 2>&1; then
    RUNNER=(xvfb-run -a)
  elif [[ "$RID" != "$HOST_RID" ]]; then
    SMOKE=0
  fi

  if ((SMOKE)) && [[ "$RID" == "$HOST_RID" ]]; then
    RUN_LOG="$LOG_DIR/run-$RID.log"
    printf '\nStarting %s with a %ss timeout...\n' "$BINARY_NAME" "$SMOKE_TIMEOUT"
    set +e
    if ((${#RUNNER[@]})); then
      "${RUNNER[@]}" timeout "$SMOKE_TIMEOUT" "$BINARY"
    else
      timeout "$SMOKE_TIMEOUT" "$BINARY"
    fi >"$RUN_LOG" 2>&1
    CODE=$?
    set -e
    printf 'Exit code %s (124 = still running when the timeout fired, which is a pass)\n' "$CODE"
    if [[ -s "$RUN_LOG" ]]; then tail -20 "$RUN_LOG" | sed 's/^/  /'; fi
    if [[ -n "${GITHUB_STEP_SUMMARY:-}" ]]; then
      {
        echo "### Runtime smoke — $RID"
        echo
        echo "- exit code: $CODE (124 = pass)"
        echo
      } >>"$GITHUB_STEP_SUMMARY"
    fi
    if [[ "$CODE" != "124" ]]; then
      echo "error: the AOT binary exited on its own with $CODE; it is not a working application" >&2
      exit 1
    fi
  else
    printf '\nSmoke run skipped: %s cannot run a %s binary here (pass --no-smoke to silence this).\n' "$HOST_RID" "$RID"
  fi
fi

printf '\nDone. Native binary: %s\n' "$BINARY"
