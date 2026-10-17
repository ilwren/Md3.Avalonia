# `IMdChartDataProvider` API

**包：** `Md3.Avalonia.Extra`<br>
**命名空间：** `Md3.Avalonia.Extra.Controls`<br>
**源码：** [`MdDataVisualization.cs`](../../src/Md3.Avalonia.Extra/Controls/MdDataVisualization.cs)

`IMdChartDataProvider` 为 `MdChart` 提供一组 `MdChartSeries`。提供器返回数据，不负责绘图、坐标轴或命中测试。

## 成员

```csharp
IReadOnlyList<MdChartSeries> GetSeries();
```

这是同步读取接口，不包含变化通知或刷新事件。`MdChart.Provider` 是普通 CLR 属性；控件不会自动监听提供器数据的变化。更新数据后显式调用 `RefreshProvider()`：

```csharp
chart.Provider = new SalesChartProvider(repository);
chart.RefreshProvider();
```

也可直接绑定或赋值 `MdChart.Series`，不使用提供器。图表数据引擎、缓存及数据请求策略由应用负责。
