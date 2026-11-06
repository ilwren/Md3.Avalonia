[English](PERFORMANCE.md) | [简体中文](PERFORMANCE.zh-CN.md)

# 性能验证

本仓库量化 UI 响应速度、CPU、内存和包体积的方法。共三层，按成本从低到高：

1. **Headless 预算测试**（`MdPerformanceBudgetTests`、`MdMemoryLeakTests`）——每次推送都在 CI
   里跑的回归绊线。
2. **BenchmarkDotNet 微基准**（`tests/Md3.Avalonia.Benchmarks`）——带分配数据的精确计时，按需
   在本地跑。
3. **Profiler 配方**——`dotnet-counters` / `dotnet-trace` / `dotnet-gcdump`、Avalonia DevTools
   和 `adb`，用于发版前的深入分析。

## 1. Headless 预算测试（CI 门禁）

源码：`tests/Md3.Avalonia.HeadlessTests/MdPerformanceBudgetTests.cs`。

每个测试先预热一次，然后在 headless UI 线程上取几次运行的中位数（经 `TestAppBuilder` 做真
Skia 渲染），再与一个故意放得很宽松的上限比较。CI 共享机本身抖动大，所以这些**不是 SLA**
——它们的存在意义是抓住 10 倍级的退化，比如 UI 线程上的同步等待、主题应用里不小心写出
的 O(n²)。

| 测试 | 卡住的东西 | 预算 |
|---|---|---|
| `Theme_Generation_Stays_Within_Budget` | HCT 配色生成（49 个 role） | 中位数 < 2000ms |
| `Full_Scheme_Matrix_Generation_Stays_Within_Budget` | 6 变体 × 3 对比度 × 明/暗全矩阵 | 中位数 < 10000ms |
| `Icon_Lookup_Stays_Within_Budget` | 稳态下 1000 次 `MdSymbols` 名称查找 | 中位数 < 2000ms |
| `First_Window_Render_Stays_Within_Budget` | 含按钮 + 文本框 + 复选框的窗口 `Show` + 首次布局 | 中位数 < 5000ms |
| `Dialog_Open_Stays_Within_Budget` | `MdDialogService` 打开 + 关闭一轮 | 中位数 < 3000ms |
| `Snackbar_Show_Stays_Within_Budget` | `MdSnackbarService` 显示 + 消除一轮 | 中位数 < 3000ms |
| `Theme_Switch_Stays_Within_Budget` | `MdThemeManager.Apply` + 资源传播（测完恢复原主题） | 中位数 < 3000ms |
| `Large_Virtualized_List_First_Layout_Stays_Within_Budget` | 1 万行 `MdList` 首次布局，**且**实际生成的容器 < 500 个 | < 10000ms |

源码：`tests/Md3.Avalonia.HeadlessTests/MdMemoryLeakTests.cs`。写法沿用 Avalonia 官方
LeakTests 的配方——挂到 live 窗口上、操作、Detach + 布局、回收、断言；另加 detach 后强制刷一帧
（headless 渲染器会钉住上一提交场景直到新帧替换），以及带墙钟延迟的有界回收重试来等渲染
时钟。且是 Release 专属门禁（Debug 下直接返回，CI 跑的是 Release）。从不断言窗口本身：连 Avalonia 自己的套件都只断言
内容控件，因为共享 headless 平台会钉住已关闭的窗口。

| 测试 | 卡住的东西 |
|---|---|
| `Detached_Material_Content_Is_Collected` | 从 live 窗口 detach 下来的内容必须可被回收 |
| `Attached_Dialog_Host_Detaches_Cleanly` | 挂上又摘下（从未打开过）的 dialog host 必须可被回收 |
| `Dialog_Open_Close_Cycle_Does_Not_Leak_Host` | 打开/关闭一轮后 detach 的 dialog host 必须可被回收，即使它的 service 还活着（service 对 host 只能持弱引用；先用 host 外的焦点宿转移走模态焦点） |
| `Snackbar_Show_Dismiss_Cycle_Does_Not_Leak_Host` | 同上，针对 snackbar host |

本地只跑这些：

```bash
dotnet test tests/Md3.Avalonia.HeadlessTests/Md3.Avalonia.HeadlessTests.csproj -c Release \
  --filter "FullyQualifiedName~MdPerformanceBudgetTests|FullyQualifiedName~MdMemoryLeakTests"
```

## 2. BenchmarkDotNet 微基准

源码：`tests/Md3.Avalonia.Benchmarks`（net10.0 控制台，`BenchmarkDotNet` 0.15.8，不打包发布）。
所有基准都带 `[MemoryDiagnoser]`，每次计时都附带单次操作的分配字节数。

| 类 | 用例 |
|---|---|
| `ThemeGenerationBenchmarks` | 单配色生成、36 配色全矩阵、对比度诊断、主色调色板 |
| `IconLookupBenchmarks` | 目录 miss、别名词典物化、目录规模（命中计时需要 font manager，所以放在有 Application 的 headless 预算测试里） |
| `MotionBenchmarks` | 动效 token 解析、easing 求值、弹簧采样 |

运行（必须 Release，否则数字无意义）：

```bash
dotnet run -c Release --project tests/Md3.Avalonia.Benchmarks -- --filter "*"
dotnet run -c Release --project tests/Md3.Avalonia.Benchmarks -- --filter "*Theme*"
```

结果在 `tests/Md3.Avalonia.Benchmarks/BenchmarkDotNet.Artifacts/`。对比看平均耗时和分配字节，
退化超过 20% 就值得调查。

## 3. Profiler 配方（发版时用）

靶子是 Release 下发布的 Gallery（`gallery/Md3.Avalonia.Gallery.Desktop`），跑固定场景：打开
Gallery、切换 seed + 明/暗、打开一个 dialog、滚动最长的列表页。

```bash
# 实时计数：CPU、工作集、GC 堆、GC 频率、异常数
dotnet-counters monitor -p <pid> System.Runtime

# CPU 采样 → .nettrace 用 PerfView 或 SpeedScope 打开，重点看
# Measure/Arrange、模板展开、DynamicResource 解析、Skia 绘制各占多少
dotnet-trace collect -p <pid> --providers Microsoft-DotNETCore-SampleProfiler

# 打开-关闭循环前后各打一个堆快照对比，Md* 实例数不得增长
dotnet-gcdump collect -p <pid> -o before.gcdump
# ... 跑固定场景 ...
dotnet-gcdump collect -p <pid> -o after.gcdump
```

日常 UI 开发：Debug 下用 `AttachDeveloperTools()` 跑 Gallery，按 F12 看布局/渲染耗时和可视树。

Android 侧是自动采集的：Maestro 场景跑完后，Android 工作流会跑
`scripts/collect-android-perf.py`，抓取帧统计（`dumpsys gfxinfo` 摘要 + 原始 `framestats` 表）、
内存（`dumpsys meminfo`）、冷启动（force-stop + 从 Launcher 重启后的 ActivityManager
`Displayed` 行）和 APK 体积，写入 `artifacts/android-test/android-perf.json`。JSON、原始 dump
和渲染好的 `android-perf.md` 表格都会进 `android-ui-regression-evidence` 产物，关键数字还会镜像
到 annotations 和 step summary。采集是建议性质的——任何失败都变成带 warning 的 null，绝不会挂红
步骤；数字与 swiftshader 模拟器上的 Maestro 场景强相关，所以只做横向对比，不设预算红线。

连着本地真机/模拟器时可以直接跑采集器（默认用唯一在线的 `adb` 设备，多设备时传 `--serial`）：

```bash
python3 scripts/collect-android-perf.py --package io.arena.md3avalonia.gallery \
  --apk artifacts/android --output-dir /tmp/android-perf --summary --github
```

这些证据只有在托管 runner 真的启动了模拟器时才存在。当前的 macOS 镜像经常起不来：模拟器步骤会
警告 "Timeout waiting for emulator to boot" 且不上传任何文件，而工作流因为该步骤是建议性质仍报
success。看到没有 `android-perf.json` 时，请理解为"这次没有 runner"，而不是"没有数据"。

采集器本身由 `scripts/test-collect-android-perf.py` 覆盖：把真实进程跑在一个假的 `adb` 上，用录制
好的 `dumpsys` 输出喂给它，验证解析（含 Android 12 起 "Number Frame dead" 改名为
"Number Frame deadline missed"）、冷启动探测、落盘证据文件和 annotations，全程不需要设备。改完采集器
跑一遍即可；CI 的静态检查任务也会跑。

不用采集器、临时看一眼：

```bash
adb shell dumpsys gfxinfo <package> framestats   # P90/P99 帧时间、掉帧数
adb shell dumpsys meminfo <package>              # 固定场景后的 PSS / 堆
```

包体积和裁剪已有门禁：所有包都声明了 `IsTrimmable` 和 `IsAotCompatible`，AOT probe 工作流会
数 `ILxxxx` 诊断数；每次发版对比 publish 产物大小，增长超 5% 即告警。

## 调整预算的方法

1. 改 `MdPerformanceBudgetTests` 里的上限，同一提交里同步更新上面的表格（中英两份）。
2. 只能依据安静硬件上的本地 Release 实测值收紧，绝不依据某一次 CI 采样。
3. 如果预算只在 CI 挂，先怀疑机器抖动：重跑一次，确认后再放宽。
