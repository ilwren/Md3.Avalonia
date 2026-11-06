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

# adb answering is not the same as Android being up: an emulator mid-boot accepts the
# connection while the package and activity managers still refuse calls ("Failure calling
# service package: Broken pipe", "Unable to connect to activity manager"), so an install here
# would fail and the run would report n/a for every metric.
wait_boot() {
  local waited=0
  step adb wait-for-device
  while [ "$waited" -lt 300 ]; do
    if [ "$(adb shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" = "1" ]; then
      echo "boot_completed after ${waited}s" >>"$LOG"
      # boot_completed lands slightly before the package manager answers.
      in_pm=0
      while [ "$in_pm" -lt 30 ]; do
        adb shell pm list packages >/dev/null 2>&1 && break
        sleep 2
        in_pm=$((in_pm + 2))
      done
      return 0
    fi
    sleep 5
    waited=$((waited + 5))
  done
  echo "boot_completed not seen within 300s" >>"$LOG"
  echo "::warning::android perf: the emulator never finished booting; metrics will be n/a"
  return 1
}

step adb devices -l
wait_boot
step adb shell getprop ro.product.model
step adb shell getprop ro.build.version.sdk

apk=$(ls "$APK_DIR"/*.apk 2>/dev/null | head -1)
if [ -n "$apk" ]; then
  # A streamed install can still race the package service; retry instead of giving up and
  # reporting an uninstalled package.
  installed=1
  for attempt in 1 2 3; do
    step adb install -r "$apk" && { installed=0; break; }
    sleep 10
  done
  [ "$installed" -ne 0 ] && echo "::warning::android perf: adb install failed after 3 attempts"
else
  echo "no APK under $APK_DIR" >>"$LOG"
  echo "::warning::android perf: no APK under $APK_DIR; nothing to launch"
fi

# monkey is the only launcher that does not need the generated activity's JNI name.
step adb shell monkey -p "$PACKAGE" -c android.intent.category.LAUNCHER 1
# A cold .NET MAUI-style startup on a swiftshader emulator is slow; give it room before asking
# for frame stats, then record what it is actually doing.
sleep 30

# A dead process is the difference between "no data" and "the app crashed": say which.
step adb shell pidof "$PACKAGE"
pid=$(adb shell pidof "$PACKAGE" 2>/dev/null | tr -d '\r')
step sh -c "adb shell dumpsys window 2>/dev/null | grep -i -m2 'mCurrentFocus\|mFocusedApp'"
if [ -n "$pid" ]; then
  # The app's own log often explains a UI that never draws (missing asset, XAML error, ...).
  app_log=$(adb logcat -d --pid="$pid" 2>/dev/null | tail -25 | tr '\n' ' ')
  echo "::notice::android perf: app log (pid $pid): $app_log" | cut -c1-3800
else
  echo "::warning::android perf: the app process is not running after launch"
fi
step adb logcat -d -b crash
crash=$(adb logcat -d -b crash 2>/dev/null | tail -12 | tr '\n' ' ')
[ -n "$crash" ] && echo "::error::android perf: crash buffer: $crash"

step python3 scripts/collect-android-perf.py \
  --package "$PACKAGE" \
  --apk "$APK_DIR" \
  --output-dir "$OUT_DIR" \
  --summary --github

step ls -la "$OUT_DIR"
exit 0
