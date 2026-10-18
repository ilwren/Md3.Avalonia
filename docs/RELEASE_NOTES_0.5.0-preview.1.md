# Md3.Avalonia 0.5.0-preview.1 release notes

发布日期：2026-10-18

版本号从 `0.4.1-preview.1` 进到 `0.5.0-preview.1`。这一版没有破坏性改动，但公开 API 面增加了不少：
可发布包从六个变成十六个，`Md3.Avalonia.Extra` 多了三个自研控件，所以走次版本而不是补丁版本。

> 仓库中没有 `0.4.1-preview.1` 的 tag 或 release；`docs/RELEASE_NOTES_0.4.1-preview.1.md` 保留为
> 那一轮工作的记录。本版条目原先记在 CHANGELOG 的 `0.4.1-preview.1` 小节下，已归入
> `## [0.5.0-preview.1]`。

## 新增

### 十个第三方控件主题包

按「有第三方控件就做 Material 美化，没有才自研」的原则，为下面每个库各建了一个独立的 opt-in
主题包。每个包都是引入该依赖的唯一一处，不用就不会被拖进来：

| 包 | 上游 | 形态 |
|---|---|---|
| `Md3.Avalonia.AvaloniaEdit` | AvaloniaEdit | Avalonia 样式集合 |
| `Md3.Avalonia.AvaloniaHex` | AvaloniaHex | Avalonia 样式集合 |
| `Md3.Avalonia.Dock` | Dock | 样式集合（含一个无法回避的原生 Button 主题）|
| `Md3.Avalonia.Tabalonia` | Tabalonia | Avalonia 样式集合 |
| `Md3.Avalonia.Markdown` | Markdown.Avalonia | 赋给 `MarkdownScrollViewer.MarkdownStyle` |
| `Md3.Avalonia.LiveCharts2` | LiveCharts2 | 配置入口 `LiveCharts.Configure(...)` |
| `Md3.Avalonia.ScottPlot` | ScottPlot.Avalonia | 绘图对象入口 `MaterialScottPlotTheme` |
| `Md3.Avalonia.PropertyGrid` | bodong.Avalonia.PropertyGrid | 32 个键控控件主题 |
| `Md3.Avalonia.NodifyM` | NodifyM.Avalonia | 画刷重定向，模板仍归上游 |
| `Md3.Avalonia.Mapsui` | Mapsui.Avalonia12 | 地图级入口 `MaterialMapsuiTheme` |

主题化只做外观，不改上游的数据管线、模板与部件命名。确实无法在库侧解决的三处（NodifyM 的连线
几何、PropertyGrid 的编辑器模板、Tabalonia 的标签样式）在 `docs/API.md` 中标注为「留在上游」。

### 三个自研控件（`Md3.Avalonia.Extra`）

- **`MdWaveform`** —— 峰值驱动的波形图。播放过的部分用 `Primary`，其余用
  `SurfaceContainerHighest`，与 Material 3 滑块区分活动/非活动轨道的方式一致；播放头可拖拽、
  可键盘操作（方向键、Home、End），`PlayheadPosition` 双向绑定。**不内置解码器**：`Peaks` 是每根
  条一个归一化幅度，`MdWaveform.PeaksFromSamples` 供已有解码器的宿主把采样缓冲降采样成条。
  两个默认值参照 Flutter 的 `audio_waveforms`：条形端点是该包从 `StrokeCap.round` 得到的圆头，
  播放线是它用的 2dp。`IsPulsing` 让条形原地脉动，按每根条自己的峰值缩放，所以安静的段落不会和
  高潮一样跳；它跑在 `TopLevel.RequestAnimationFrame` 上而不是定时器上，隐藏时不产生任何开销，
  在 Reduced 与 None 动效方案下**完全静止**（不是变慢，是画得一模一样）。
- **`MdWaterfallPanel` / `MdWaterfall`** —— 瀑布流布局。每个子项按列宽测量后放进当前最短的列，
  高度不一的卡片因此不会像网格那样留出参差的空隙。`Columns` 固定列数；留默认 `0` 时由
  `MinColumnWidth` 推导，并以子项数为上限，所以两三项的内容不会为多余的空间预留空列。放置是贪心
  的且从不重排子项 —— 信息流的顺序就是内容本身。`AnimateLayout`（默认开）在**列数变化**时用
  first-last-invert-play 的方式把子项送到新位置；只动画列数变化，因为保持列数的缩放本来就在逐帧
  跟随，每一帧重启过渡会让子项永远落后于指针。`MdWaterfall` 是同一个面板套在 items control 上，
  四个布局数值从控件上读回，绑定的信息流不需要自己写 `ItemsPanelTemplate`。
- **`MdFloatingWindow`** —— 应用内部的悬浮窗，不是操作系统窗口。标题栏拖动、右下角缩放、最小化
  收成一条标题栏。最小化**只隐藏不卸载**内容，所以内嵌的播放器、编辑器保留绑定、媒体状态和撤销
  历史；显式的 `Height` 会被记住并恢复，不会留下一个空的高框。关闭走和 `MdBanner` 相同的
  presence 控制器，先播退场动画再卸载，紧接着重开会取消卸载而不是闪一下。拖动**逐帧**钳制在可视
  父级内 —— 这是在应用内的表面不需要合成器的代价，也是它的好处：窗口能在面板里任意停靠，但不会
  被拖丢。`CanDrag`/`CanResize`/`CanMinimize`/`CanClose` 各自拿掉对应的交互入口。

> `MdSideSheet`（贴边收起的边缘面板）曾在本轮一并实现，后按需求撤下，代码库中已无残留。

## 修复

- **Markdown 代码块偶发无法成形。** `CCode` 的 `MonospaceFontFamily` 默认取
  `FontFamilyCollector.TryGetMonospace()` 按**枚举顺序**挑出的第一个系统字体族，该字体带
  `fonts:SystemFonts` 键且无回退，导致 CI 间歇性抛 `Could not create glyphTypeface`。改为设置
  一个字体族列表（复合键可回退），并按各字体族的实际命名大小写，因为候选匹配是序数比较。必须设在
  `MonospaceFontFamily` 而不是 `FontFamily` 上：`CCode` 的构造函数把后者绑到前者，而绑定的优先级
  高于样式 setter。
- **`MdWaveform` 的脉冲在祖先隐藏它之后永不恢复。** 帧循环在 `ShouldAnimate` 变假时停摆，而这个
  判断包含 `IsEffectivelyVisible` —— 但没有任何东西在可见性回来时通知控件。Avalonia 12.1.2 只把
  `IsEffectivelyVisible` 暴露成普通 CLR 属性，变更事件是 `internal`，所以隐藏父级对子级完全没有
  通知，已经停摆的循环就一直停着。悬浮窗最小化、折叠面板收起、标签页切走都会因此让脉冲永久冻结。
  现在控件在挂载期间订阅每个可视祖先的 `IsVisible`；Avalonia 在把新的有效可见性下发给子级**之后**
  才发出该通知，所以重新判断读到的是当前状态而不是上一帧的。
- **两个主题项目不在解决方案的构建配置里。**

## Gallery

110 个已索引页面，1022 条中英对照，`scripts/check-gallery-localization.py --strict` 下未翻译正文
为 0。悬浮窗页面内嵌一个用真实曲目驱动的迷你播放器：那段 20 秒音频来自 `mdn/dom-examples` 的
*Okay*，以 CC0 1.0 进入公有领域；播放是定时器走播放头，不接音频设备，所以在完全没有输出的机器上
行为一致。

## 测试

- `check-api-doc-coverage.py`：types=344，documented=344，missing=0
- `check-gallery-localization.py --strict`：1022 keys，0 重复，未翻译正文 0
- `lint-design-tokens.py`：errors=0
- 六个（现为十六个）`src/` 包声明 `IsAotCompatible`，并以 IL3xxx 分析器构建；本轮未新增反射
- CI、AOT probe、Build NuGet Packages、Build Multi-Platform Gallery 四个 workflow 全绿

## 升级

```xml
<PackageReference Include="Md3.Avalonia" Version="0.5.0-preview.1" />
<!-- 需要哪个生态主题就再加哪一个 -->
<PackageReference Include="Md3.Avalonia.Extra" Version="0.5.0-preview.1" />
```

没有已知的破坏性改动。两处行为变化值得知道：`MdWaveform` 现在会在祖先可见性变化时重新评估是否
播放脉动（此前会永久停摆），以及 `MaterialMarkdownTheme` 现在设置 `MonospaceFontFamily` 而不是
让它回落到宿主挑出的单个字体族。
