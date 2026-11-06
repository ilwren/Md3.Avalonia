# 媒体进度条行为调研（2026-11-06）

## 现有控件与缺口

`MdSlider` 提供 Avalonia 范围、指针和键盘交互，但没有已缓冲段和媒体时间标签；`MdWaveform` 绘制宿主提供的峰值与播放头，不含独立的缓冲进度。媒体播放、解码和网络流不属于这些控件的职责。

## 冻结参考与独立设计

参考 [`audio_video_progress_bar` README](https://github.com/suragch/audio_video_progress_bar/blob/a8d21089c529d3872002863eaf8917ef0937e72f/README.md)，该 GitHub 提交对应仓库 `pubspec.yaml` 版本 **2.0.2**，许可证 MIT。候选调研曾列出 pub.dev 2.0.3；本轮无法从该站获取并验证此版本文档，因此不将本次 GitHub 快照误标为 2.0.3。仅阅读 README、包元数据及许可证，未阅读或复制 Dart 源码、资产、截图或样式。来源登记见 `spec-snapshot/manifest.json` 和 `THIRD-PARTY-NOTICES.md`。

独立实现 `MdMediaProgressBar`：使用秒为单位的 Avalonia 范围值、缓冲段和可选时间标签；宿主更新播放进度并处理 seek 请求。指针拖动更新两向绑定的值，释放时请求 seek；键盘和无障碍请求立即报告。`IsScrubbing` 帮助宿主暂停播放进度覆盖，捕获丢失取消 seek 请求。主题使用 Material 色彩 token；标记不自启动动画，因此没有自主动画需要降级。

## 未纳入

不实现播放器、解码器、流媒体状态、Flutter 类型兼容或 Flutter 全部自定义选项。控件归 Extra 可选生态模式，不声称官方 Material 组件或像素一致。
