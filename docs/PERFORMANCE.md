[English](PERFORMANCE.md) | [简体中文](PERFORMANCE.zh-CN.md)

# Performance verification

How UI responsiveness, CPU, memory and package size are quantified for this repository.
Three layers, cheapest first:

1. **Headless budget tests** (`MdPerformanceBudgetTests`, `MdMemoryLeakTests`) — regression
   tripwires that run in CI on every push.
2. **BenchmarkDotNet microbenchmarks** (`tests/Md3.Avalonia.Benchmarks`) — precise timings with
   allocation data, run locally on demand.
3. **Profiler recipes** — `dotnet-counters` / `dotnet-trace` / `dotnet-gcdump`, Avalonia DevTools
   and `adb`, for release-time deep dives.

## 1. Headless budget tests (CI gates)

Source: `tests/Md3.Avalonia.HeadlessTests/MdPerformanceBudgetTests.cs`.

Each test warms up once, takes the median of a few runs on the headless UI thread (real Skia
rendering via `TestAppBuilder`), and asserts against a deliberately generous ceiling. Shared CI
runners are noisy, so these are **not SLAs** — they catch 10x regressions such as a synchronous
wait on the UI thread or an accidental O(n²) in theme application.

| Test | What it gates | Budget |
|---|---|---|
| `Theme_Generation_Stays_Within_Budget` | HCT scheme generation (49 roles) | median < 2000ms |
| `Full_Scheme_Matrix_Generation_Stays_Within_Budget` | 6 variants × 3 contrasts × light/dark | median < 10000ms |
| `Icon_Lookup_Stays_Within_Budget` | 1000 `MdSymbols` name lookups, steady state | median < 2000ms |
| `First_Window_Render_Stays_Within_Budget` | Window with button + text box + check box, `Show` + first layout | median < 5000ms |
| `Dialog_Open_Stays_Within_Budget` | `MdDialogService` open + close round-trip | median < 3000ms |
| `Snackbar_Show_Stays_Within_Budget` | `MdSnackbarService` show + dismiss round-trip | median < 3000ms |
| `Theme_Switch_Stays_Within_Budget` | `MdThemeManager.Apply` + resource propagation (restores the previous theme) | median < 3000ms |
| `Large_Virtualized_List_First_Layout_Stays_Within_Budget` | 10k-row `MdList` first layout **and** realized containers < 500 | < 10000ms |

Source: `tests/Md3.Avalonia.HeadlessTests/MdMemoryLeakTests.cs`. These follow Avalonia's
own LeakTests recipe — attach to a live window, exercise, detach with a layout pass, collect,
assert — plus a forced fresh frame after detach (the headless renderer holds the last committed
scene until a new one replaces it) and bounded collect-retry with wall-clock delays for the
render timer. They are Release-only gates (they return immediately in Debug builds; CI runs
Release).
The window itself is never asserted: even Avalonia's suite only asserts content controls,
because the shared headless platform keeps closed windows rooted.

| Test | What it gates |
|---|---|
| `Detached_Material_Content_Is_Collected` | Content detached from a live window is collectable |
| `Attached_Dialog_Host_Detaches_Cleanly` | An attached-then-detached dialog host (never opened) is collectable |
| `Dialog_Open_Close_Cycle_Does_Not_Leak_Host` | The dialog host, detached after an open/close cycle, is collectable even while its service stays alive (services must hold hosts weakly; a focus sink outside the host releases modal focus first) |
| `Snackbar_Show_Dismiss_Cycle_Does_Not_Leak_Host` | Same for the snackbar host |

Run only these locally:

```bash
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release \
  --filter "FullyQualifiedName~MdPerformanceBudgetTests|FullyQualifiedName~MdMemoryLeakTests"
```

## 2. BenchmarkDotNet microbenchmarks

Source: `tests/Md3.Avalonia.Benchmarks` (net10.0 console, `BenchmarkDotNet` 0.15.8, not packed).
All benchmarks carry `[MemoryDiagnoser]`, so every timing comes with bytes allocated per operation.

| Class | Cases |
|---|---|
| `ThemeGenerationBenchmarks` | Single-scheme generation, full 36-scheme matrix, contrast diagnosis, primary tonal palette |
| `IconLookupBenchmarks` | Catalog miss, alias-catalog materialization, catalog size (hit timing needs a font manager, so it lives in the headless budget tests instead) |
| `MotionBenchmarks` | Motion token resolution, easing evaluation, spring sampling |

Run them (Release is required for meaningful numbers):

```bash
dotnet run -c Release --project tests/Md3.Avalonia.Benchmarks -- --filter "*"
dotnet run -c Release --project tests/Md3.Avalonia.Benchmarks -- --filter "*Theme*"
```

Results land in `tests/Md3.Avalonia.Benchmarks/BenchmarkDotNet.Artifacts/`. Compare runs with
the reports' mean time and allocated bytes; treat >20% regressions as investigation-worthy.

## 3. Profiler recipes (release-time)

Target the published Gallery (`gallery/Md3.Avalonia.Gallery.Desktop`, Release) while driving a
fixed scenario: open the Gallery, switch seed + light/dark, open a dialog, scroll the longest
list page.

```bash
# Live counters: CPU, working set, GC heap, GC frequency, exceptions
dotnet-counters monitor -p <pid> System.Runtime

# CPU sampling → open the .nettrace in PerfView or SpeedScope; look for
# Measure/Arrange, template expansion, DynamicResource resolution, Skia draws
dotnet-trace collect -p <pid> --providers Microsoft-DotNETCore-SampleProfiler

# Heap snapshots before/after an open-close loop; Md* instance counts must not grow
dotnet-gcdump collect -p <pid> -o before.gcdump
# ... drive the scenario ...
dotnet-gcdump collect -p <pid> -o after.gcdump
```

Day-to-day UI work: run the Gallery with `AttachDeveloperTools()` (Debug) and press F12 for
layout/render timings and the visual tree.

Android is collected automatically: after the Maestro scenario, the Android workflow runs
`scripts/collect-android-perf.py`, which captures frame stats (`dumpsys gfxinfo` summary plus
the raw `framestats` table), a SurfaceFlinger frame timeline (see below), memory (`dumpsys meminfo`), cold start (the ActivityManager
`Displayed` line for a force-stop + relaunch, falling back to the `am start -W` timings when
the image never logs one) and APK size into
`artifacts/android-test/android-perf.json`. A `crash.txt` dump rides along with it: when the app
dies on launch every metric is empty, and the crash buffer is the only thing that explains why.
The JSON, the raw dumps and a rendered
`android-perf.md` table ship in the `android-ui-regression-evidence` artifact, and the headline
numbers are mirrored as annotations and a step summary. Collection is advisory by design — any
failure becomes a null with a warning, never a red step — and the numbers couple to the Maestro
scenario on a swiftshader emulator, so compare runs against each other, not against a budget.

Two frame sources are collected on purpose. `dumpsys gfxinfo` only counts the platform renderer
(`ThreadedRenderer`), so an app that draws through its own EGL surface — Avalonia does — reports
`Total frames rendered: 0` and sentinel percentiles forever, no matter how smoothly it runs.
`dumpsys SurfaceFlinger --latency <layer>` sees every presented frame regardless of who drew it, so
the report carries `sf.framesPresented` plus frame-interval p50/p90/p99 and the share of intervals
longer than 1.5 refresh periods. Read the `gfx` numbers for platform-drawn UI and `sf` for ours.

What a hosted runner can measure today, with `Google ATD` + software rendering: APK size, PSS, and
the `am start -W` launch wait. Not frame times — the Gallery's UI never reaches the screen there
(SurfaceFlinger presents zero frames on every app layer, and the platform itself sits in an
Application Not Responding state while a 215 MB .NET process starts). `-gpu off` and host GL behave
identically. Frame and cold-start numbers therefore come from a machine with working GPU
acceleration: a developer's emulator, or a device.

Against a local device or emulator, run the collector directly (any single attached `adb`
device; pass `--serial` when several are attached):

```bash
python3 scripts/collect-android-perf.py --package io.arena.md3avalonia.gallery \
  --apk artifacts/android --output-dir /tmp/android-perf --summary --github
```

That evidence exists only when the hosted runner boots an emulator. On the current macOS image
it often does not: the emulator step warns "Timeout waiting for emulator to boot" and uploads no
files, while the run still reports success because the step is advisory. Treat a missing
`android-perf.json` as "no runner", not as "no data". When the file is there but every metric is
`n/a`, read `collector-run.log`, `crash.txt` and the `::warning::` annotations first: the observed
causes have been a package manager that was not up yet, the app dying on launch, and the
emulator's own `system_server` crashing mid-run under swiftshader.

The collector itself is covered by `scripts/test-collect-android-perf.py`, which runs it as a real
process against a fake `adb` answering with recorded `dumpsys` output: parsers (including the
Android 12 rename of "Number Frame dead" to "Number Frame deadline missed"), the cold-start probe,
the written evidence files and the annotations, all with no device attached. Run it after touching
the collector; CI runs it in the static-check job.

For ad-hoc inspection without the collector:

```bash
adb shell dumpsys gfxinfo <package> framestats   # P90/P99 frame times, jank counts
adb shell dumpsys meminfo <package>              # PSS / heap after a fixed scenario
```

Package size and trimming are already gated: every package declares `IsTrimmable` and
`IsAotCompatible`, the AOT probe workflow counts `ILxxxx` diagnostics, and publish output size
should be compared per release (flag >5% growth).

## Adjusting budgets

1. Change the ceiling in `MdPerformanceBudgetTests` and update the table above (and the Chinese
   twin) in the same commit.
2. Tighten only from local Release measurements on quiet hardware, never from a single CI sample.
3. If a budget fails only on CI, suspect runner noise first: re-run before widening.
