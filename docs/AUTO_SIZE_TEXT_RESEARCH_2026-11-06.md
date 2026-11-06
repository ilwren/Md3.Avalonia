# 自动字号文本行为调研（2026-11-06）

## 现状与参考

`src/Md3.Avalonia/Controls/MdText.cs` 是带 Material 类型角色的 `TextBlock`，没有约束内自动缩小字号。候选记录见 `docs/FLUTTER_THIRD_PARTY_RESEARCH_2026-11-05.md`。本次只阅读 [`auto_size_text` v3.0.0 固定 README](https://github.com/simc/auto_size_text/blob/2178f0821132636bc4f0dea1bc4d40a889d8249b/README.md)、该提交的 `pubspec.yaml` 和 MIT `LICENSE`，未阅读或复制 Dart 源码、素材或样式。参考行为为有界空间的缩放、字号下限、步进、最大行数及下限仍溢出时的处理。来源登记在 `spec-snapshot/manifest.json` 和 `THIRD-PARTY-NOTICES.md`。

## 独立实现与限制

`MdAutoSizeText` 是 Extra 中单独的纯文本控件，使用 Avalonia `TextLayout` 按可用宽高测量未截断文本，检查实际行数；对从请求字号向下按步进排列的有限候选做二分搜索（假设字号与适配性单调），最小字号为最终候选。测量和实际排列尺寸均可触发重新计算，带 padding、换行和 RTL；不可容纳时交由 Avalonia trimming 和边界裁剪。请求字号保持原绑定/样式不变，`EffectiveFontSize` 只读；两维无限时不缩小。控件使用文字无障碍名称与 Material on-surface 色彩。

这是行为子集，不是官方 Material 控件或 Flutter 兼容实现；不支持 rich inlines、选择、group、预设字号列表、overflowReplacement、字体特性或字间距。搜索上限和下限分别夹在 1–512 DIP，步进夹在 0.1–512 DIP；并未保证像素一致或极端复杂排版的适配单调性。Gallery 示例在 Animated text 页面，API 及双语状态文档列出使用方式和限制。实际编译、无头布局/绘制测试以 CI 结果为准。
