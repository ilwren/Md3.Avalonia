#!/usr/bin/env bash
# Install + launch the Gallery on a booted emulator and collect performance evidence.
#
# This exists because the emulator only lives for the duration of the emulator step, and because
# a multi-command `script:` block inherits the action's `sh -e` wrapper: one failing command
# (an unmatched APK glob, a monkey launch that reports "No activities found") aborts everything
# after it, including the evidence collection. Here every step is recorded, nothing aborts the
# run, and failures surface as annotations instead of a silent step failure.
#
# Usage: scripts/ci-android-perf.sh [package] [apk-dir] [output-dir]
set +e

PACKAGE="${1:-io.arena.md3avalonia.gallery}"
APK_DIR="${2:-artifacts/android}"
OUT_DIR="${3:-artifacts/android-test}"
# Inside the evidence directory on purpose: the uploaded artifact then carries the exact
# command sequence and exit codes for the run.
LOG="${LOG:-$OUT_DIR/collector-run.log}"

mkdir -p "$OUT_DIR"
: > "$LOG"

step() {
  echo "=== $*" >>"$LOG"
  "$@" >>"$LOG" 2>&1
  local status=$?
  echo "exit=$status :: $*" >>"$LOG"
  return $status
}

# Annotate the log in chunks: a hosted-runner job log is not always retrievable, annotations are.
flush_log() {
  python3 - "$LOG" <<'EOF' || true
import pathlib, sys
lines = pathlib.Path(sys.argv[1]).read_text(errors="replace").splitlines()
for index in range(0, len(lines), 25):
    chunk = " | ".join(lines[index:index + 25])[:3800]
    print(f"::notice::android perf step log{index // 25}: {chunk}")
EOF
}
trap flush_log EXIT

step adb devices -l
step adb shell getprop ro.product.model
step adb shell getprop ro.build.version.sdk

apk=$(ls "$APK_DIR"/*.apk 2>/dev/null | head -1)
if [ -n "$apk" ]; then
  step adb install -r "$apk"
else
  echo "no APK under $APK_DIR" >>"$LOG"
  echo "::warning::android perf: no APK under $APK_DIR; nothing to launch"
fi

# monkey is the only launcher that does not need the generated activity's JNI name.
step adb shell monkey -p "$PACKAGE" -c android.intent.category.LAUNCHER 1
sleep 15

step python3 scripts/collect-android-perf.py \
  --package "$PACKAGE" \
  --apk "$APK_DIR" \
  --output-dir "$OUT_DIR" \
  --summary --github

step ls -la "$OUT_DIR"
exit 0
