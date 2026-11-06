# 数字滚轮与分组列表复核（2026-11-06）

## 分组列表的排期修正

原候选清单将 grouped_list 列为自研候选。复核发现 Avalonia 生态已有 MIT 的
[Devolutions.AvaloniaControls](https://github.com/Devolutions/avalonia-extensions/tree/0b8c07225dbdd03dc966d86461809ecb00b47a1a/src/Devolutions.AvaloniaControls)：
`GroupedListBox` 提供分组、可折叠组与选择，`GroupedTileListBox` 提供分组磁贴。
当前源码中的 `GroupedListBox` 标有 trim/动态代码警告，其公开能力不等于粘性组头；
实际发布版本、Avalonia 12 兼容性、模板扩展和 AOT 使用仍需逐项验证。
因此按“同模式先评估 vendor”的规则暂停新建 `MdGroupedList`，不宣称它已完成，
也不将现有第三方实现等同于 sticky/floating header。普通 Avalonia `ItemsControl` 可用
[扁平化的分组头/条目](https://docs.avaloniaui.net/docs/data-binding/collection-views)组合。

## 数字滚轮冻结参考

Avalonia `DateTimePickerPanel` 是日期时间专用的滚动布局，并非公开的通用整数选择器；
本次检索未发现可直接替代的 Avalonia 通用数字滚轮控件（不代表生态中一定不存在）。
仅阅读 [`numberpicker` 2.1.2 固定 README](https://github.com/MarcinusX/NumberPicker/blob/566ab314da385d76df0405c990bd4d5ad6562dcc/README.md)、包元数据和 BSD-2-Clause 许可证，未阅读或复制 Dart 实现和资产。该文档描述用滚动 spinner 选择有界整数或小数；固定版本与处置登记于 manifest 与 notices。

## 独立设计与边界

Extra `MdWheelPicker` 首期仅提供有界整数网格：五行居中、上下文绑定、点击相邻行、拖动、聚焦后的鼠标滚轮、键盘、数值范围自动化、只读与动态 Material 色彩。值在最小值锚定的步长网格上取整，最大值不在网格上时仅作上界；交互即时生效，无动画，天然适用于 reduced/none motion。测试包括边界、整数溢出、键盘、鼠标、触发事件与自动化。未做小数、无限循环、水平轴、惯性、模板、自定义格式、控制器或滚动物理一致性；这不是 Flutter API 或视觉复刻。
