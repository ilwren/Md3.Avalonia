# 滚动文本行为调研（2026-11-06）

## 现有控件与冻结参考

`MdAnimatedText` 提供有限时长的逐字/淡入等出现动效，并非持续滚动的单行文本；`MdAutoSizeText` 通过缩小字号处理空间限制。候选与 clean-room 流程见 `docs/FLUTTER_THIRD_PARTY_RESEARCH_2026-11-05.md`。本轮仅阅读 [`marquee` 2.3.0 固定 README](https://github.com/MarcelGarus/marquee/blob/06a6cb1072f151a4053d17e8f613765b797daf69/README.md)、`pubspec.yaml` 与 MIT 许可证，未阅读或复制 Dart 源码、资源和样式。固定来源登记在 `spec-snapshot/manifest.json` 与 `THIRD-PARTY-NOTICES.md`。

## 独立设计

Extra `MdMarquee` 测量单行文本，仅在实际有界宽度内溢出时启动水平循环；速度、文本间距、物理方向、每轮暂停可配置，Auto 方向遵循 RTL。使用 Avalonia 文本排版和 render-frame 请求，不维护持续运行的 dispatcher timer；隐藏、卸载、无溢出、停用、零速及 `MdMotion` Reduced/None 均停止并回到静态裁剪内容，恢复后从逻辑起点开始。长帧间隔限幅，避免最小化后突然跳过多轮。文本自动化仅暴露一次完整原文，屏幕上的重复副本不是独立无障碍节点。

## 边界

只实现纯文本和水平恒速，不提供垂直滚动、有限轮次、初始填充、加减速曲线、富文本、选择或文本交互，不声称 Flutter 完整兼容。换行符仅在视觉上替换为空格，自动化保留原文；动画中轮间等待仍属于 `IsAnimating`。`docs/API.md` 和中英文 parity 文档说明约束；Gallery 示例位于 Animated text 页面。构建及测试以 CI 实测为准。
