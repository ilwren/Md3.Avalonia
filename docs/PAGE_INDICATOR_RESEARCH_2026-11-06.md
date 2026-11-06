# 轮播页面指示器行为调研（2026-11-06）

## 既有能力与缺口

`MdPagination` 基于 Avalonia `PipsPager`，已提供离散页码、圆点、键盘导航和可见窗口；`MdCarousel` 暴露整数 `SelectedIndex`，但不暴露连续滚动页位置。二者不提供由宿主小数页位置直接驱动的标记几何。新控件不取代分页器，也不修改 carousel 的滚动物理。

## 文档参考与独立设计

冻结参考：[`smooth_page_indicator` 3.0.0 README](https://github.com/Milad-Akarie/smooth_page_indicator/blob/bb6402dd2d94dc8f44ec3f13b580d7148354884e/README.md)，MIT，GitHub commit `bb6402dd2d94dc8f44ec3f13b580d7148354884e`（对应公开 `pubspec.yaml` 的版本 3.0.0）。仅阅读 README 与包元数据，未阅读或复制 Dart 实现、资产、截图或样式。出处与处置见 `spec-snapshot/manifest.json`、`THIRD-PARTY-NOTICES.md`。

参考页面数量、位置驱动标记、水平/垂直、点击回调及滑动/拉伸效果的公开概念，独立设计 `MdPageIndicator`：传入页数和小数位置；宿主拥有切页、滚动位置与动画策略；画刷取 Material token；真实指针、方向键、Home/End 和范围无障碍请求可通知宿主。水平 RTL 反转标记和命中方向。控件没有独立定时器，不产生自动动效；Reduced/None 的策略由提供动画位置的宿主负责。

## 未纳入

不接入 Flutter `PageController`，不实现其他效果、自动索引动画或滚动圆点窗口；超过大量页数不虚拟化。此为 Extra 可选生态模式，并非官方 Material 组件或 Flutter API/像素级兼容声明。
