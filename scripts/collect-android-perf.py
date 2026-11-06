#!/usr/bin/env python3
"""Collect Android UI performance evidence from a connected device or emulator.

Runs after the Maestro scenario and captures, for one package:
  * frame stats (dumpsys gfxinfo summary, plus the raw framestats table),
  * memory (dumpsys meminfo),
  * cold start (the ActivityManager "Displayed" line for a force-stop + launcher start),
  * APK size.

Advisory by design: every collection step is best-effort, missing data becomes null
with a warning, and the exit code is always 0. The numbers couple to the Maestro
scenario and the emulator (Pixel 6, API 34, swiftshader), so they are comparable
across runs of the same workflow, not SLAs.

Usage:
    python3 scripts/collect-android-perf.py --package io.arena.md3avalonia.gallery \\
        --apk artifacts/android --output-dir artifacts/android-test --summary --github
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import time
from pathlib import Path

# Kept tight on purpose: this runs inside the emulator step's five-minute budget, so the
# collector must never be the reason evidence goes missing.
ADB_TIMEOUT = 45
PROBE_BUDGET_SECONDS = 40
PROBE_POLL_SECONDS = 2


def run(cmd: list[str], timeout: int = ADB_TIMEOUT) -> tuple[int, str]:
    """Run a command, returning (returncode, combined output). Never raises."""
    try:
        proc = subprocess.run(cmd, capture_output=True, text=True, errors="replace", timeout=timeout)
        return proc.returncode, (proc.stdout or "") + (proc.stderr or "")
    except FileNotFoundError:
        return 127, f"command not found: {cmd[0]}"
    except subprocess.TimeoutExpired:
        return 124, f"timed out after {timeout}s: {' '.join(cmd)}"
    except Exception as exc:  # noqa: BLE001 - evidence collection must not crash
        return 1, f"failed to run {' '.join(cmd)}: {exc}"


def parse_gfxinfo_summary(text: str) -> dict:
    gfx: dict = {
        "totalFrames": None, "jankyFrames": None, "jankyPct": None,
        "p50Ms": None, "p90Ms": None, "p95Ms": None, "p99Ms": None,
        "missedVsync": None, "highInputLatency": None, "slowUiThread": None,
        "slowBitmapUploads": None, "slowIssueDrawCommands": None, "frameDead": None,
    }

    def first_int(pattern: str) -> int | None:
        match = re.search(pattern, text, re.M)
        return int(match.group(1)) if match else None

    gfx["totalFrames"] = first_int(r"Total frames rendered:\s*(\d+)")
    janky = re.search(r"Janky frames:\s*(\d+)\s*\(([\d.]+)%\)", text)
    if janky:
        gfx["jankyFrames"] = int(janky.group(1))
        gfx["jankyPct"] = float(janky.group(2))
    for pct, key in (("50", "p50Ms"), ("90", "p90Ms"), ("95", "p95Ms"), ("99", "p99Ms")):
        ordinal = {"50": "50th", "90": "90th", "95": "95th", "99": "99th"}[pct]
        gfx[key] = first_int(rf"{ordinal} percentile:\s*(\d+)ms")
    gfx["missedVsync"] = first_int(r"Number Missed Vsync:\s*(\d+)")
    gfx["highInputLatency"] = first_int(r"Number High input latency:\s*(\d+)")
    gfx["slowUiThread"] = first_int(r"Number Slow UI thread:\s*(\d+)")
    gfx["slowBitmapUploads"] = first_int(r"Number Slow bitmap uploads:\s*(\d+)")
    gfx["slowIssueDrawCommands"] = first_int(r"Number Slow issue draw commands:\s*(\d+)")
    gfx["frameDead"] = first_int(r"Number Frame dead:\s*(\d+)")
    return gfx


def parse_meminfo(text: str) -> dict:
    mem: dict = {"totalPssKb": None, "nativePssKb": None, "dalvikPssKb": None}

    def first_int(pattern: str) -> int | None:
        match = re.search(pattern, text, re.M)
        return int(match.group(1)) if match else None

    # Main table: "    TOTAL      123456   ..." (Pss Total first). The App Summary
    # fallback reads "TOTAL PSS: 123456" on newer releases.
    mem["totalPssKb"] = first_int(r"^\s*TOTAL\s+(\d+)\s+\d+\s+\d+") or first_int(r"TOTAL PSS:\s*(\d+)")
    mem["nativePssKb"] = first_int(r"Native Heap\s+(\d+)")
    mem["dalvikPssKb"] = first_int(r"Dalvik Heap\s+(\d+)")
    return mem


def parse_displayed_ms(line: str) -> int | None:
    match = re.search(r":\s*\+(?:(\d+)s)?(\d+)ms", line)
    if not match:
        return None
    seconds = int(match.group(1)) if match.group(1) else 0
    return seconds * 1000 + int(match.group(2))


def displayed_lines(logcat: str, package: str) -> list[str]:
    return [line for line in logcat.splitlines()
            if "Displayed" in line and f"{package}/" in line]


def resolve_apk(raw: str | None, warnings: list[str]) -> tuple[Path | None, int | None]:
    if not raw:
        return None, None
    path = Path(raw)
    if path.is_dir():
        apks = sorted(path.glob("*.apk"), key=lambda p: p.stat().st_size, reverse=True)
        if not apks:
            warnings.append(f"no APK found in directory {raw}")
            return None, None
        path = apks[0]
    if not path.is_file():
        warnings.append(f"APK not found: {raw}")
        return None, None
    return path, path.stat().st_size


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--package", required=True, help="application id, e.g. io.arena.md3avalonia.gallery")
    ap.add_argument("--apk", default=None, help="APK file or directory containing the APK (for size)")
    ap.add_argument("--output-dir", default="artifacts/android-test", help="evidence directory")
    ap.add_argument("--serial", default=None, help="adb device serial when several are attached")
    ap.add_argument("--no-cold-start", action="store_true", help="skip the force-stop + relaunch probe")
    ap.add_argument("--github", action="store_true", help="emit GitHub Actions annotations")
    ap.add_argument("--summary", action="store_true", help="append the report to $GITHUB_STEP_SUMMARY")
    args = ap.parse_args()

    warnings: list[str] = []
    out_dir = Path(args.output_dir)
    out_dir.mkdir(parents=True, exist_ok=True)

    package: str = args.package
    adb = ["adb"] + (["-s", args.serial] if args.serial else [])

    gfx: dict = {key: None for key in (
        "totalFrames", "jankyFrames", "jankyPct", "p50Ms", "p90Ms", "p95Ms", "p99Ms",
        "missedVsync", "highInputLatency", "slowUiThread",
        "slowBitmapUploads", "slowIssueDrawCommands", "frameDead")}
    mem: dict = {"totalPssKb": None, "nativePssKb": None, "dalvikPssKb": None}
    device: dict = {"model": None, "sdk": None}
    initial_displayed_ms: int | None = None
    cold_start_displayed_ms: int | None = None
    displayed_count = 0

    if shutil.which("adb") is None:
        warnings.append("adb is not on PATH; skipping all device metrics")
    else:
        code, devices = run(adb + ["devices"])
        if code != 0 or "more than one" in devices:
            warnings.append("no single adb device (attach one device or pass --serial); skipping device metrics")
        elif not re.search(r"\n\S+\s+device\b", "\n" + devices):
            warnings.append(f"no authorized adb device visible: {devices.strip() or '(empty)'}")
        else:
            code, model = run(adb + ["shell", "getprop", "ro.product.model"])
            if code == 0 and model.strip():
                device["model"] = model.strip()
            code, sdk = run(adb + ["shell", "getprop", "ro.build.version.sdk"])
            if code == 0 and sdk.strip():
                device["sdk"] = sdk.strip()

            code, listed = run(adb + ["shell", "pm", "list", "packages"])
            installed_lines = [line.strip() for line in listed.splitlines()]
            installed = code == 0 and f"package:{package}" in installed_lines
            if not installed:
                warnings.append(f"package {package} is not installed on the device; skipping device metrics")
            else:
                code, gfxinfo = run(adb + ["shell", "dumpsys", "gfxinfo", package])
                (out_dir / "gfxinfo.txt").write_text(gfxinfo, encoding="utf-8")
                if code == 0 and "Total frames rendered" in gfxinfo:
                    gfx = parse_gfxinfo_summary(gfxinfo)
                else:
                    warnings.append("dumpsys gfxinfo produced no frame summary (app may not have drawn yet)")

                code, framestats = run(adb + ["shell", "dumpsys", "gfxinfo", package, "framestats"])
                (out_dir / "framestats.txt").write_text(framestats, encoding="utf-8")
                if code != 0:
                    warnings.append("dumpsys gfxinfo framestats failed; raw table may be incomplete")

                code, meminfo = run(adb + ["shell", "dumpsys", "meminfo", package])
                (out_dir / "meminfo.txt").write_text(meminfo, encoding="utf-8")
                if code == 0 and re.search(r"^\s*TOTAL\s+\d+", meminfo, re.M):
                    mem = parse_meminfo(meminfo)
                else:
                    warnings.append("dumpsys meminfo produced no process section")

                code, logcat = run(adb + ["logcat", "-d"])
                before = displayed_lines(logcat if code == 0 else "", package)
                if before:
                    initial_displayed_ms = parse_displayed_ms(before[0])

                if args.no_cold_start:
                    warnings.append("cold-start probe skipped by --no-cold-start")
                else:
                    run(adb + ["shell", "am", "force-stop", package])
                    time.sleep(2)
                    code, monkey = run(adb + ["shell", "monkey", "-p", package,
                                              "-c", "android.intent.category.LAUNCHER", "1"])
                    if code != 0 or "No activities found" in monkey:
                        warnings.append("monkey could not launch the launcher activity; no cold-start sample")
                    else:
                        deadline = time.time() + PROBE_BUDGET_SECONDS
                        probe_lines: list[str] = []
                        while time.time() < deadline:
                            time.sleep(PROBE_POLL_SECONDS)
                            poll_code, poll = run(adb + ["logcat", "-d", "ActivityManager:I", "*:S"])
                            if poll_code != 0:
                                continue
                            probe_lines = displayed_lines(poll, package)
                            if len(probe_lines) > len(before):
                                break
                        displayed_count = len(probe_lines)
                        if displayed_count > len(before):
                            (out_dir / "displayed.txt").write_text("\n".join(probe_lines) + "\n", encoding="utf-8")
                            cold_start_displayed_ms = parse_displayed_ms(probe_lines[-1])
                            if cold_start_displayed_ms is None:
                                warnings.append(f"could not parse the Displayed line: {probe_lines[-1][:160]}")
                        else:
                            warnings.append("no fresh Displayed line appeared within the probe budget")

                if not (out_dir / "displayed.txt").exists():
                    code, final_log = run(adb + ["logcat", "-d", "ActivityManager:I", "*:S"])
                    final_lines = displayed_lines(final_log if code == 0 else "", package)
                    displayed_count = displayed_count or len(final_lines)
                    (out_dir / "displayed.txt").write_text(
                        "\n".join(final_lines) + ("\n" if final_lines else ""), encoding="utf-8")

    apk_path, apk_bytes = resolve_apk(args.apk, warnings)

    report = {
        "package": package,
        "device": device,
        "apkPath": str(apk_path) if apk_path else None,
        "apkBytes": apk_bytes,
        "gfx": gfx,
        "mem": mem,
        "startup": {
            "initialDisplayedMs": initial_displayed_ms,
            "coldStartDisplayedMs": cold_start_displayed_ms,
            "displayedLinesSeen": displayed_count,
        },
        "raw": {
            "gfxinfo": "gfxinfo.txt" if (out_dir / "gfxinfo.txt").exists() else None,
            "framestats": "framestats.txt" if (out_dir / "framestats.txt").exists() else None,
            "meminfo": "meminfo.txt" if (out_dir / "meminfo.txt").exists() else None,
            "displayed": "displayed.txt" if (out_dir / "displayed.txt").exists() else None,
        },
        "warnings": warnings,
    }
    (out_dir / "android-perf.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")

    def cell(value: object, suffix: str = "") -> str:
        return f"{value}{suffix}" if value is not None else "n/a"

    md_lines = [
        "### Android performance evidence",
        "",
        f"package `{package}` · device {device['model'] or 'n/a'} (SDK {device['sdk'] or 'n/a'}) · "
        f"APK {cell(None if apk_bytes is None else round(apk_bytes / 1e6, 1), ' MB')}",
        "",
        "| Metric | Value |",
        "|---|---|",
        f"| Frames rendered | {cell(gfx['totalFrames'])} |",
        f"| Janky frames | {cell(gfx['jankyFrames'])} ({cell(gfx['jankyPct'], '%')}) |",
        f"| Frame p50 / p90 / p99 | {cell(gfx['p50Ms'], 'ms')} / {cell(gfx['p90Ms'], 'ms')} / {cell(gfx['p99Ms'], 'ms')} |",
        f"| Missed vsync / slow UI thread | {cell(gfx['missedVsync'])} / {cell(gfx['slowUiThread'])} |",
        f"| PSS total / native / Dalvik | {cell(mem['totalPssKb'], ' KB')} / {cell(mem['nativePssKb'], ' KB')} / {cell(mem['dalvikPssKb'], ' KB')} |",
        f"| Cold start (Displayed) | {cell(cold_start_displayed_ms, 'ms')} (initial launch {cell(initial_displayed_ms, 'ms')}) |",
    ]
    if warnings:
        md_lines += ["", "Warnings:"] + [f"- {w}" for w in warnings]
    md_report = "\n".join(md_lines) + "\n"
    (out_dir / "android-perf.md").write_text(md_report, encoding="utf-8")

    if args.github:
        print(f"::notice::Android perf [{package}]: "
              f"frames={cell(gfx['totalFrames'])} janky={cell(gfx['jankyFrames'])} "
              f"p50/p90/p99={cell(gfx['p50Ms'], 'ms')}/{cell(gfx['p90Ms'], 'ms')}/{cell(gfx['p99Ms'], 'ms')}")
        print(f"::notice::Android perf [{package}]: "
              f"pss[{cell(mem['totalPssKb'], 'KB')}] coldstart[{cell(cold_start_displayed_ms, 'ms')}] "
              f"apk[{cell(None if apk_bytes is None else round(apk_bytes / 1e6, 1), 'MB')}]")
        for w in warnings[:5]:
            print(f"::warning::Android perf [{package}]::{w}")

    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if args.summary and summary_path:
        with open(summary_path, "a", encoding="utf-8") as fh:
            fh.write(md_report)

    print(f"android perf: package={package} "
          f"frames={cell(gfx['totalFrames'])} janky={cell(gfx['jankyFrames'])} "
          f"p90={cell(gfx['p90Ms'], 'ms')} pss={cell(mem['totalPssKb'], 'KB')} "
          f"coldstart={cell(cold_start_displayed_ms, 'ms')} apk={cell(apk_bytes, 'B')} "
          f"warnings={len(warnings)} report={out_dir / 'android-perf.json'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
