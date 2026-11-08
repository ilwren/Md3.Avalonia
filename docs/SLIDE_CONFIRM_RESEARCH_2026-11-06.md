# 滑动确认及跨列表拖拽复核（2026-11-06）

## 已有控件与冻结参考

`MdDismissible` 是滑动移除内容，并不提供一次性确认控件；Avalonia 的 `Slider` 是连续数值输入，`ToggleSwitch` 是开关，两者默认不提供“从手柄拖至阈值并释放后触发一次”的约定。搜索 Avalonia 的 slide-to-confirm / slide-unlock 项目未发现可直接复用的实现（不证明不存在）。

仅查阅 [`slide_to_act` 2.0.2 固定 README](https://github.com/muhammadtalhasultan/flutter-slide-to-act/blob/36deeafa3810d8bb88f2e533aa365b59f177b54a/README.md)、对应 pubspec 与许可证：README 描述 slide-to-unlock 模式；许可证为 MIT，Copyright 2020 Salvatore Giordano。未查阅或复制 Dart 实现、资产或外观。冻结版本和处置登记于 manifest 和 notices。这里的阈值、回弹、键盘与无障碍语义为本项目自主设计，并不声称是该 README 中逐项列出的 API。

## 独立设计与边界

Extra `MdSlideToConfirm` 拖动手柄到阈值后**松开**才确认一次，短拖动继承松手速度并按 Material spatial spring 回弹；拖过阈值松手后立即确认，手柄带惯性弹簧滑至末端。惯性不会让阈值以下的手势触发确认；Reduced/None motion、键盘和程序状态变更及重置立即定位。支持 RTL 逻辑方向、双向 `IsConfirmed`、`Confirmed`、可访问按钮 Invoke、Enter/Space、可设置标签/画刷与中英文默认文本。禁用、捕获丢失、方向切换或脱离视觉树会取消手势；轨道点击不会确认。宿主应提供具体的操作标签，并在需要时执行验证或额外确认。

这是可通过键盘、自动化及 `Confirm()` 直接触发的无障碍按钮替代，**不是**防误触的安全保证、权限控制或二次确认机制；宿主不得依赖强制拖动来保护危险操作。首期不提供命令、异步提交/等待状态、加载动画、haptics、任意模板、复杂图标或 Flutter API/视觉一致性。触摸设备和系统屏幕阅读器需要平台实测，headless 测试不能代替。

## 看板候选暂停自研：vendor 优先

此前候选 `drag_and_drop_lists` 的跨列表拖拽应先评估现有 Avalonia 库，不创建重复 `MdDragAndDropBoard`。[`Sortable.Avalonia` 2.4.1 的固定版本](https://github.com/russkyc/sortable-avalonia/tree/de439ffb835eb14fc7fcf2a2f014df4e2456043f)为 MIT（Copyright 2026 Russell Camo），README 描述 `ItemsControl` 行为、看板、跨集合转移、排序、手柄、鼠标和触摸；项目声明 `IsAotCompatible=true`。**声明不等于本项目完成的原生 AOT 或交互测试。**

2.4.1 与 2.4.0 的项目引用 Avalonia **12.1.3**，本仓库固定 **12.1.2**，因此不能直接作为当前兼容版本接入。较旧 [2.3.1](https://github.com/russkyc/sortable-avalonia/tree/52631c35d902fbf3dff5f8e801d9da049a5edd24) 项目引用 12.0.0 且声明 AOT 兼容，README 也描述跨集合与鼠标/触摸；这仅表明值得验证，不保证与 12.1.2 或本项目主题、原生 AOT、触摸、键盘和内存生命周期兼容。2.4.1 README 还列出后续触摸和捕获修复，不应假定 2.3.1 包含这些修复。

后续复核发现 2.3.1 存在上游已确认的跟踪引用泄漏；2.4.0 修复该问题但要求 12.1.3。源码还显示无效源索引下并非原子回滚，并向标准输出记录对象内容。现有仅测试工程引用的 NuGet 探针与详细决策见 `docs/SORTABLE_VENDOR_PROBE_2026-11-07.md`；未将该包加入 Gallery 或产品，未声称 vendor 集成完成或通过原生 AOT 拖拽测试。
