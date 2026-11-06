# 可展开文本行为调研（2026-11-06）

## 现有控件与冻结参考

`MdText` 是 Material 字阶的 `TextBlock`，`MdAutoSizeText` 在约束内缩小纯文本字号；两者均不提供“折叠后展开”状态。候选和 clean-room 流程见 `docs/FLUTTER_THIRD_PARTY_RESEARCH_2026-11-05.md`。本轮仅阅读 [`readmore` 3.0.0 的固定 README](https://github.com/jonataslaw/readmore/blob/d6c6c5fc44a34139465bd4ea4c7868ce9834174f/README.md)、`pubspec.yaml` 和 MIT 许可证，未阅读或复制 Dart 源码、资源或外观。来源与版本登记在 `spec-snapshot/manifest.json` 和 `THIRD-PARTY-NOTICES.md`。

## 独立设计

Extra `MdExpandableText` 提供纯文本、按行或字符折叠、独立展开/收起按钮及两向 `IsExpanded`。行模式使用实际排版宽度和 Avalonia `TextLayout` 检测未截断文本的溢出，显示由 `TextBlock` 的行数和省略号承担；字符模式按 .NET Unicode 文本元素截取，避免切开组合字符和 emoji 序列。文本、宽度或字号变化后重新布局；内容不再超限时隐藏按钮，但不重写宿主的展开状态。按钮使用原生键盘及 button automation 行为，标签由内置中英文本或宿主覆盖提供。无展开动画，因此无需单独的 reduced-motion 定时器。

## 限制

按钮位于文本下方而非行内；不支持富文本、可选文本、正则注解及 URL/话题/@ 交互，不声称 Flutter 全面兼容。行模式的 `CanExpand` 在 arrange 后有效；其默认模板契约要求文本与按钮部件，任意外层高度裁切不是触发折叠的条件。具体 API、示例与限制见 `docs/API.md`，实现子集列于中英文 `docs/FLUTTER_PARITY_STATUS*`。构建及测试以 CI 实测为准。
