# 庆祝粒子行为调研（2026-11-06）

## 现有控件与冻结参考

`MdSpinKit` 和 `MdLoadingIndicator` 是加载指示；`MdWaveform` 有装饰性脉冲，但现有库没有庆祝粒子。候选与 clean-room 流程见 `docs/FLUTTER_THIRD_PARTY_RESEARCH_2026-11-05.md`。本次仅阅读 [`confetti` 固定 README](https://github.com/funwithflutter/flutter_confetti/blob/736dd344a4f6a9d38a45d937964544e1bb741c7e/README.md)、`pubspec.yaml` 和 MIT 许可证，未阅读或复制 Dart 源码、资源或外观。虽然该 GitHub 合并提交的消息称 release 0.8.0，但同一提交 `pubspec.yaml` 实际写的是 **0.7.0**；因此本次按 0.7.0 登记，不把候选清单上的 pub.dev 0.8.0 当作已核实的源码版本。来源见 `spec-snapshot/manifest.json` 与 `THIRD-PARTY-NOTICES.md`。

## 独立设计

Extra `MdConfetti` 是无命中测试、无无障碍内容的装饰覆盖层；宿主在控件附着、可见且完成布局后调用 `Burst()` 发射一次有限粒子。可设置方向、角度、数量、尺寸、速度、重力、初速度指数衰减、持续时间、归一化位置和三个 Material 画刷。配置在发射时快照（画刷仍动态），可选 seed 便于可重复测试。粒子按控件边界裁切，不生成全屏覆盖层。

使用 render-frame 回调并限制长帧跳跃；自然结束才触发 `Completed`，手动停止、隐藏、卸载和 `Reduced`/`None` 动效方案均取消，不自动恢复。减少动效不显示静止彩纸，成功信息须由宿主提供，不依赖视觉特效。

## 未纳入

不支持持续发射、循环控制器、发射频率、自定义路径、任意长度调色板、声音、触觉或交互式粒子；重力不受拖曳衰减，公式与 Flutter 的模拟不等价，不声称 Material 官方或 Flutter 完整兼容。API、Gallery 和双语状态文档标明边界；构建及测试以 CI 实测为准。
