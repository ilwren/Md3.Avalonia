#!/usr/bin/env python3
"""Tests for scripts/collect-android-perf.py.

Two layers:

* Parser tests per dump format. Real `dumpsys` output shapes are reproduced verbatim
  enough that a renaming upstream (Android 12's "Frame deadline missed") fails a test
  instead of silently producing a null in the shipped evidence.
* An end-to-end test that puts a fake `adb` on PATH and runs the collector as a real
  process. That covers what a parser test cannot: device discovery, the cold-start probe
  (force-stop, monkey launch, poll for a *fresh* `Displayed` line), the written evidence
  files, the JSON schema, the annotations, and the guarantee that the script exits 0.

A device is not required, which matters because the emulator job cannot be relied on to
produce evidence on every run. Run with `python3 scripts/test-collect-android-perf.py`.
"""

from __future__ import annotations

import importlib.util
import pathlib
import json
import os
import stat
import subprocess
import sys
import tempfile
import textwrap
import unittest
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
COLLECTOR = REPO_ROOT / "scripts" / "collect-android-perf.py"
PACKAGE = "io.arena.md3avalonia.gallery"


def load_collector():
    spec = importlib.util.spec_from_file_location("collect_android_perf", COLLECTOR)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


collector = load_collector()


# Reproduced from an Android 14 device, trimmed to the counters the collector reads.
GFXINFO = textwrap.dedent(
    """\
    ** Graphics info for pid 4321 [io.arena.md3avalonia.gallery] **

    Stats since: 18362184000000ns
    Total frames rendered: 1204
    Janky frames: 46 (3.82%)
    Janky frames (legacy): 46 (3.82%)
    50th percentile: 5ms
    90th percentile: 9ms
    95th percentile: 12ms
    99th percentile: 27ms
    Number Missed Vsync: 2
    Number High input latency: 1
    Number Slow UI thread: 9
    Number Slow bitmap uploads: 0
    Number Slow issue draw commands: 3
    Number Frame deadline missed: 11
    Number Frame deadline missed (legacy): 4
    HISTOGRAM: 5ms=1000 6ms=100 7ms=50 8ms=20 9ms=10 12ms=8 27ms=5 0=0
    """
)

# The same dump as an Android 11 device prints it: the counter was still "Frame dead",
# percentiles are absent, and the unused counters are all zero.
GFXINFO_LEGACY = textwrap.dedent(
    """\
    ** Graphics info for pid 4321 [io.arena.md3avalonia.gallery] **

    Total frames rendered: 300
    Janky frames: 12 (4.00%)
    90th percentile: 11ms
    Number Missed Vsync: 0
    Number High input latency: 0
    Number Slow UI thread: 4
    Number Slow bitmap uploads: 0
    Number Slow issue draw commands: 1
    Number Frame dead: 6
    """
)

MEMINFO = textwrap.dedent(
    """\
    Applications Memory Usage (in Kilobytes):
    Uptime: 418834 Realtime: 418834

    ** MEMINFO in pid 4321 [io.arena.md3avalonia.gallery] **
                       Pss  Private  Private  SwapPss      Rss     Heap     Heap     Heap
                     Total    Dirty    Clean    Dirty    Total     Size    Alloc     Free
                    ------   ------   ------   ------   ------   ------   ------   ------
      Native Heap    35120    34984        0        0    61204    69632    41234    28398
      Dalvik Heap    20345    20200        0        0    22560    32768    18315    14453
     Dalvik Other     2411     2411        0        0     2411
            Code     9112     9112        0        0    19004
           Stack      420      420        0        0      420
         Gfx dev     1024     1024        0        0     1024
       Other dev       28        0       28        0      676
        .so mmap    29411      340    28612        0    51844
       .jar mmap        8        0        8        0    40960
       .apk mmap     1044        0     1044        0     1044
       .dex mmap     2710       12     2698        0     2720
       .oat mmap      218        0        0        0    16284
       .art mmap     2100     1984        0        0    12252
      Other mmap     1450       60      140        0     3800
       GL mtrack    12288    12288        0        0    12288
         Unknown      612      612        0       0       612
           TOTAL   108307   104353    32530       0   290313    102400    59549    42851

     App Summary
                           Pss(KB)
                            ------
               Java Heap:    22329
             Native Heap:    35120
                    Code:     9112
                   Stack:      420
                Graphics:    13312
           Private Other:     6640
                  System:    21374

                   TOTAL:   108307       TOTAL SWAP PSS:        0
    """
)

MEMINFO_SIMPLE = textwrap.dedent(
    """\
    ** MEMINFO in pid 999 [io.arena.md3avalonia.gallery] **
     App Summary
                           Pss(KB)
                            ------
               Java Heap:    18000
             Native Heap:    22000
                   TOTAL PSS:    51234            TOTAL RSS:    90000       TOTAL SWAP PSS:        0
    """
)

LOGACT_PRELAUNCH = (
    "11-06 06:00:01.000  1234  1300 I ActivityTaskManager: Displayed "
    "io.arena.md3avalonia.gallery/.MainActivity: +1s234ms\n"
    "11-06 06:00:02.000  1234  1300 I ActivityTaskManager: Displayed "
    "com.android.settings/.Settings: +320ms\n"
)

LOGACT_COLD_START = LOGACT_PRELAUNCH + (
    "11-06 06:05:11.000  1234  1300 I ActivityTaskManager: Displayed "
    "io.arena.md3avalonia.gallery/.MainActivity: +812ms\n"
)


class GfxinfoParsingTests(unittest.TestCase):
    def test_modern_dump(self):
        gfx = collector.parse_gfxinfo_summary(GFXINFO)
        self.assertEqual(gfx["totalFrames"], 1204)
        self.assertEqual(gfx["jankyFrames"], 46)
        self.assertAlmostEqual(gfx["jankyPct"], 3.82)
        self.assertEqual((gfx["p50Ms"], gfx["p90Ms"], gfx["p95Ms"], gfx["p99Ms"]), (5, 9, 12, 27))
        self.assertEqual(gfx["missedVsync"], 2)
        self.assertEqual(gfx["highInputLatency"], 1)
        self.assertEqual(gfx["slowUiThread"], 9)
        self.assertEqual(gfx["slowBitmapUploads"], 0)
        self.assertEqual(gfx["slowIssueDrawCommands"], 3)
        # Renamed counter on Android 12+: reading only "Frame dead" left this null.
        self.assertEqual(gfx["frameDead"], 11)

    def test_legacy_dump_keeps_old_counter(self):
        gfx = collector.parse_gfxinfo_summary(GFXINFO_LEGACY)
        self.assertEqual(gfx["frameDead"], 6)
        self.assertEqual(gfx["p90Ms"], 11)
        self.assertIsNone(gfx["p50Ms"])
        self.assertIsNone(gfx["p99Ms"])

    def test_zero_is_not_missing(self):
        gfx = collector.parse_gfxinfo_summary(GFXINFO_LEGACY)
        self.assertEqual(gfx["missedVsync"], 0)

    def test_empty_dump_is_all_none(self):
        gfx = collector.parse_gfxinfo_summary("")
        self.assertTrue(all(value is None for value in gfx.values()), gfx)


class MeminfoParsingTests(unittest.TestCase):
    def test_main_table(self):
        mem = collector.parse_meminfo(MEMINFO)
        self.assertEqual(mem["totalPssKb"], 108307)
        self.assertEqual(mem["nativePssKb"], 35120)
        self.assertEqual(mem["dalvikPssKb"], 20345)

    def test_app_summary_fallback(self):
        mem = collector.parse_meminfo(MEMINFO_SIMPLE)
        self.assertEqual(mem["totalPssKb"], 51234)
        self.assertEqual(mem["nativePssKb"], 22000)
        # App Summary names the Dalvik heap "Java Heap".
        self.assertEqual(mem["dalvikPssKb"], 18000)

    def test_swap_row_does_not_shadow_total(self):
        mem = collector.parse_meminfo(MEMINFO.replace("       TOTAL   108307", "TOTAL SWAP PSS:        7\n       TOTAL   108307"))
        self.assertEqual(mem["totalPssKb"], 108307)

    def test_empty_dump_is_all_none(self):
        mem = collector.parse_meminfo("")
        self.assertTrue(all(value is None for value in mem.values()), mem)


class DisplayedParsingTests(unittest.TestCase):
    def test_seconds_and_milliseconds(self):
        self.assertEqual(collector.parse_displayed_ms("... Displayed pkg/.MainActivity: +1s234ms"), 1234)
        self.assertEqual(collector.parse_displayed_ms("... Displayed pkg/.MainActivity: +812ms"), 812)
        self.assertEqual(collector.parse_displayed_ms("... Displayed pkg/.MainActivity: +2s"), 2000)

    def test_unparseable_line(self):
        self.assertIsNone(collector.parse_displayed_ms("... Displayed pkg/.MainActivity: +"))

    def test_only_launch_lines_for_this_package(self):
        lines = collector.displayed_lines(LOGACT_PRELAUNCH, PACKAGE)
        self.assertEqual(len(lines), 1)
        self.assertIn(PACKAGE, lines[0])
        self.assertEqual(collector.displayed_lines(LOGACT_PRELAUNCH, "io.arena.other"), [])

    def test_package_prefix_does_not_match_longer_package(self):
        log = "I ActivityTaskManager: Displayed io.arena.md3avalonia.gallery.debug/.MainActivity: +100ms\n"
        self.assertEqual(collector.displayed_lines(log, PACKAGE), [])


class AmStartParsingTests(unittest.TestCase):
    AM_START = ("Starting: Intent { cmp=io.arena.md3avalonia.gallery/.MainActivity }\n"
                "Status: ok\nLaunchState: COLD\nTotalTime: 2145\nWaitTime: 2210\n")

    def test_timings(self):
        self.assertEqual(collector.parse_am_start_ms(self.AM_START),
                         {"waitTimeMs": 2210, "totalTimeMs": 2145})

    def test_failed_start_is_all_none(self):
        self.assertEqual(collector.parse_am_start_ms("Error: Activity not started"),
                         {"waitTimeMs": None, "totalTimeMs": None})


class ApkResolutionTests(unittest.TestCase):
    def test_directory_prefers_largest_apk(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            (root / "small.apk").write_bytes(b"x" * 10)
            (root / "big-Signed.apk").write_bytes(b"x" * 40)
            (root / "notes.txt").write_text("ignore me")
            path, size = collector.resolve_apk(str(root), warnings := [])
            self.assertEqual(path.name, "big-Signed.apk")
            self.assertEqual(size, 40)
            self.assertEqual(warnings, [])

    def test_missing_inputs_warn_instead_of_raising(self):
        for raw in ("/nope/missing.apk", None):
            warnings: list[str] = []
            path, size = collector.resolve_apk(raw, warnings)
            self.assertIsNone(path)
            self.assertIsNone(size)
            if raw is not None:
                self.assertEqual(len(warnings), 1)

    def test_empty_directory_warns(self):
        with tempfile.TemporaryDirectory() as tmp:
            warnings: list[str] = []
            self.assertEqual(collector.resolve_apk(tmp, warnings), (None, None))
            self.assertTrue(warnings)


FAKE_ADB = '''#!{python}
"""Fake adb: answers the handful of calls the collector makes against a canned device."""
import os, sys
from pathlib import Path

STATE = Path(os.environ["FAKE_ADB_STATE"])
args = sys.argv[1:]
calls = STATE / "calls.txt"
with calls.open("a", encoding="utf-8") as fh:
    fh.write(" ".join(args) + "\\n")

logs = STATE / "logcat-count"
seen = int(logs.read_text()) if logs.exists() else 0

if args[:1] == ["devices"]:
    print("List of devices attached")
    print("emulator-5554\\tdevice")
elif args[:2] == ["shell", "getprop"]:
    print("Pixel 6" if args[2].endswith("model") else "34")
elif args[:3] == ["shell", "pm", "list"]:
    print("package:android")
    print("package:" + os.environ.get("FAKE_ADB_INSTALLED", "{package}"))
elif args[:3] == ["shell", "dumpsys", "gfxinfo"] and args[3:] == ["{package}", "framestats"]:
    print("---PROFILEDATA---")
    print("Flags,FrameTimelineVsyncId,IntendedVsync")
    print("---PROFILEDATA---")
elif args[:3] == ["shell", "dumpsys", "gfxinfo"]:
    print((STATE / "gfxinfo.txt").read_text())
elif args[:3] == ["shell", "dumpsys", "meminfo"]:
    print((STATE / "meminfo.txt").read_text())
elif args[:1] == ["logcat"] and "-b" in args:
    crash = STATE / "crash.txt"
    print(crash.read_text() if crash.exists() else "", end="")
elif args[:1] == ["logcat"]:
    logs.write_text(str(seen + 1))
    # The first dump carries the launch that already happened; after the collector forces
    # a stop and relaunches, a *second* line appears, which is the cold-start sample.
    if seen == 0:
        print((STATE / "logcat-initial.txt").read_text(), end="")
    else:
        print((STATE / "logcat-cold.txt").read_text(), end="")
elif args[:4] == ["shell", "cmd", "package", "resolve-activity"]:
    print("priority=0")
    print("{package}/.MainActivity")
elif args[:4] == ["shell", "am", "start", "-W"]:
    print("Starting: Intent {{ cmp={package}/.MainActivity }}")
    print("Status: ok")
    print("LaunchState: COLD")
    print("TotalTime: 2145")
    print("WaitTime: 2210")
elif args[:3] == ["shell", "am", "force-stop"]:
    print("")
elif args[:2] == ["shell", "monkey"]:
    print("Events injected: 1")
else:
    print("")
sys.exit(0)
'''


def write_fake_adb(bin_dir: Path, state_dir: Path) -> None:
    adb = bin_dir / "adb"
    adb.write_text(FAKE_ADB.format(python=sys.executable, package=PACKAGE), encoding="utf-8")
    adb.chmod(adb.stat().st_mode | stat.S_IEXEC)
    (state_dir / "gfxinfo.txt").write_text(GFXINFO, encoding="utf-8")
    (state_dir / "meminfo.txt").write_text(MEMINFO, encoding="utf-8")
    (state_dir / "logcat-initial.txt").write_text(LOGACT_PRELAUNCH, encoding="utf-8")
    (state_dir / "logcat-cold.txt").write_text(LOGACT_COLD_START, encoding="utf-8")
    (state_dir / "crash.txt").write_text("", encoding="utf-8")


class EndToEndTests(unittest.TestCase):
    """Runs the collector as a command against a fake device."""

    def run_collector(self, extra_args=(), with_adb=True, installed=PACKAGE, crashed=False,
                      crash_text=None):
        tmp = tempfile.TemporaryDirectory()
        self.addCleanup(tmp.cleanup)
        root = Path(tmp.name)
        bin_dir, state_dir, out_dir, apk_dir = root / "bin", root / "state", root / "out", root / "apk"
        for path in (bin_dir, state_dir, out_dir, apk_dir):
            path.mkdir()
        (apk_dir / "io.arena.md3avalonia.gallery-Signed.apk").write_bytes(b"x" * 2_500_000)

        env = {key: value for key, value in os.environ.items() if key != "FAKE_ADB_STATE"}
        env["PATH"] = (f"{bin_dir}{os.pathsep}{env.get('PATH', '')}" if with_adb else "/usr/bin:/bin")
        if with_adb:
            write_fake_adb(bin_dir, state_dir)
            if crash_text is not None:
                (state_dir / "crash.txt").write_text(crash_text, encoding="utf-8")
            if crashed:
                # The process died on launch: no frame summary, no process section, a crash log.
                (state_dir / "gfxinfo.txt").write_text(
                    "** Graphics info for pid 1 **\n\n", encoding="utf-8")
                (state_dir / "meminfo.txt").write_text(
                    "Applications Memory Usage (in Kilobytes):\n\nNo process found for: "
                    + installed + "\n", encoding="utf-8")
                (state_dir / "crash.txt").write_text(
                    "FATAL EXCEPTION: main\nProcess: " + installed + ", PID: 1\n"
                    "java.lang.RuntimeException: Unable to start activity\n", encoding="utf-8")
            env["FAKE_ADB_STATE"] = str(state_dir)
            env["FAKE_ADB_INSTALLED"] = installed
        summary = root / "summary.md"
        env["GITHUB_STEP_SUMMARY"] = str(summary)

        proc = subprocess.run(
            [sys.executable, str(COLLECTOR), "--package", PACKAGE, "--apk", str(apk_dir),
             "--output-dir", str(out_dir), "--github", "--summary", *extra_args],
            capture_output=True, text=True, env=env, timeout=180,
        )
        report = json.loads((out_dir / "android-perf.json").read_text()) if (
            out_dir / "android-perf.json").exists() else None
        return proc, out_dir, report, summary

    def test_full_collection_writes_evidence(self):
        proc, out_dir, report, summary = self.run_collector()
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertIsNotNone(report)

        self.assertEqual(report["package"], PACKAGE)
        self.assertEqual(report["device"], {"model": "Pixel 6", "sdk": "34"})
        self.assertEqual(report["apkBytes"], 2_500_000)
        self.assertEqual(report["gfx"]["totalFrames"], 1204)
        self.assertEqual(report["gfx"]["p90Ms"], 9)
        self.assertEqual(report["mem"]["totalPssKb"], 108307)
        # Cold start comes from the *second* Displayed line, not the pre-existing launch.
        self.assertEqual(report["startup"]["initialDisplayedMs"], 1234)
        self.assertEqual(report["startup"]["coldStartDisplayedMs"], 812)
        self.assertEqual(report["startup"]["coldStartSource"], "logcat Displayed")
        self.assertEqual(report["startup"]["activity"], f"{PACKAGE}/.MainActivity")
        self.assertEqual(report["startup"]["totalTimeMs"], 2145)
        self.assertEqual(report["warnings"], [])

        for name in ("gfxinfo.txt", "framestats.txt", "meminfo.txt", "displayed.txt",
                     "android-perf.json", "android-perf.md"):
            self.assertTrue((out_dir / name).is_file(), f"{name} missing")
        self.assertEqual(report["raw"], {"gfxinfo": "gfxinfo.txt", "framestats": "framestats.txt",
                                        "meminfo": "meminfo.txt", "displayed": "displayed.txt",
                                        "crash": "crash.txt", "amstart": "amstart.txt"})
        self.assertIn("Displayed io.arena.md3avalonia.gallery", (out_dir / "displayed.txt").read_text())

        # Annotations and step summary are what a reviewer actually reads.
        self.assertIn(f"::notice::Android perf [{PACKAGE}]: frames=1204", proc.stdout)
        self.assertIn("p50/p90/p99=5ms/9ms/27ms", proc.stdout)
        self.assertNotIn("::warning::", proc.stdout)
        self.assertIn("Android performance evidence", summary.read_text())
        self.assertIn("| Frames rendered | 1204 |", summary.read_text())
        markdown = (out_dir / "android-perf.md").read_text()
        self.assertIn("device Pixel 6 (SDK 34)", markdown)

    def test_no_cold_start_still_reports(self):
        proc, _, report, _ = self.run_collector(["--no-cold-start"])
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertIsNone(report["startup"]["coldStartDisplayedMs"])
        self.assertEqual(report["startup"]["initialDisplayedMs"], 1234)
        self.assertTrue(any("--no-cold-start" in w for w in report["warnings"]))

    def test_missing_adb_is_advisory(self):
        proc, out_dir, report, _ = self.run_collector(with_adb=False)
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertEqual(report["device"], {"model": None, "sdk": None})
        self.assertIsNone(report["gfx"]["totalFrames"])
        self.assertIsNone(report["mem"]["totalPssKb"])
        self.assertTrue(any("adb" in w for w in report["warnings"]))
        # The APK size is still measured, and the Markdown must say n/a rather than "n/ams".
        self.assertEqual(report["apkBytes"], 2_500_000)
        markdown = (out_dir / "android-perf.md").read_text()
        self.assertIn("| Frames rendered | n/a |", markdown)
        self.assertNotIn("n/ams", markdown)
        self.assertIn("::warning::", proc.stdout)

    def test_dead_app_reports_the_crash_reason(self):
        # The app was installed and launched but died: every metric is empty, and the warning must
        # point at the crash buffer instead of leaving a reader to guess "app may not have drawn".
        proc, out_dir, report, _ = self.run_collector(crashed=True)
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertIsNone(report["gfx"]["totalFrames"])
        self.assertIsNone(report["mem"]["totalPssKb"])
        self.assertTrue(any("crash buffer" in w for w in report["warnings"]), report["warnings"])
        self.assertIn("FATAL EXCEPTION", (out_dir / "crash.txt").read_text())
        self.assertEqual(report["raw"]["crash"], "crash.txt")

    def test_unrelated_crash_is_not_blamed_on_the_app(self):
        # A fresh emulator's crash buffer is full of droid.bluetooth SIGABRTs; they must not be
        # reported as the app crashing, and must not distort the evidence.
        proc, out_dir, report, _ = self.run_collector(
            crash_text="F libc : Fatal signal 6 (SIGABRT) in tid 3912 (bt_stack_manage), "
                       "pid 3660 (droid.bluetooth)\n")
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertFalse(any("crash" in w for w in report["warnings"]), report["warnings"])
        self.assertIn("droid.bluetooth", (out_dir / "crash.txt").read_text())

    def test_package_not_installed_is_advisory(self):
        proc, out_dir, report, _ = self.run_collector(installed="io.arena.md3avalonia.other")
        self.assertEqual(proc.returncode, 0, proc.stderr)
        self.assertIsNone(report["gfx"]["totalFrames"])
        self.assertIsNone(report["mem"]["totalPssKb"])
        self.assertTrue(any("is not installed" in w for w in report["warnings"]), report["warnings"])
        # Raw dumps must be absent rather than empty files that look like real evidence.
        self.assertFalse((out_dir / "gfxinfo.txt").exists())
        self.assertIsNone(report["raw"]["gfxinfo"])
        self.assertIn("| Frames rendered | n/a |", (out_dir / "android-perf.md").read_text())


if __name__ == "__main__":
    unittest.main(verbosity=2)
