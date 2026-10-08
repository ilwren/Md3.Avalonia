# Gallery 与 Material Design 3 偏差审计（2026-10-07）

**审计日期：** 2026-10-07  
**审计对象：** `gallery/Md3.Avalonia.Gallery`（桌面 shell：顶部 app bar、96 DIP 主轨道、组件抽屉、目录窗格、94 个索引目的地）  
**基准：** m3.material.io 当前组件 / 自适应 / 无障碍指南；仓库既有审计 `docs/UI_PARITY_AUDIT_2026-09-27.md`、`docs/MOTION_PARITY_AUDIT_2026-09-27.md`、`docs/reports/material-design-full-component-audit-2026-10-02.md`、`docs/GALLERY_DEFECTS.md`

## 0. 方法说明（重要）

开发沙箱没有 .NET SDK，无法启动 gallery 做真实鼠标操作。本次“模拟操作”由三部分构成：

1. **交互路径走查**：按“打开窗口 → 调整宽度 → 点搜索 / 汉堡 / 导航项 → 输入查询 → 回车”等路径逐条读 shell 与页面的 XAML + code-behind，记录每一步实际会执行的代码分支。
2. **现有 headless 测试复用**：`Gallery_Uses_Five_Breakpoint_Bands_And_Search_Index`、`Every_Gallery_Page_Fits_A_Narrow_Window`、`Gallery_Language_Selector_Updates_Visible_Chrome` 等已经固定了一部分真实布局/交互行为，凡测试直接断言过的观察都标 ✅。
3. **定宽算术推演**：对固定 `Width`/`Padding` 的容器做可用宽度求和，判断溢出。此类结论标 **[推断]**，并给出建议的回归测试（第 6 节），可在 CI 里证实或证伪。

第 4 节是既有组件级审计中“在 gallery 里点一下就能看到”的部分，列出以便复现；第 5 节记录走查中确认与 M3 一致的部分，避免清单只呈现负面结论。

## 1. 高优先级（交互路径上的结构性偏差）

### G-01 compact 宽度下点搜索图标会把固定宽度搜索框挤进 app bar **[推断]**

- 证据：搜索栏固定 `Width="260"`（`MainWindow.axaml:74`）；`ApplyResponsiveLayout` 在宽度 `< 540` 时把搜索栏隐藏（`MainWindow.axaml.cs:359`）；而搜索图标按钮的处理函数无条件把它重新显示：
  ```csharp
  private void FocusGallerySearch(object? sender, RoutedEventArgs e)
  {
      GallerySearch.IsVisible = true;   // MainWindow.axaml.cs:439，不受断点约束
      GallerySearch.Focus();
  }
  ```
- 推演：< 540 DIP 时 app bar 已可见元素为 内边距 36 + 汉堡 48 + 间距 12 + 品牌方块 44 + 间距 12 + 标题列 + 搜索栏 260 + 间距 10 + 主题选择器 116（宽度 ≥ 420 时可见）。不含标题列已约 538 DIP；例如 480 DIP 窗口下右侧内容必然越过可用宽度被裁切，主题选择器可能落出可视区。
- M3 依据：compact 尺寸类下 search 应进入 **全屏 search view**，docked search bar 属于较宽尺寸类的形态；M3 的 docked search bar 会随容器收缩或切换形态，而不是以固定宽度溢出 app bar。
- 附带的状态机问题：该 `IsVisible = true` 属于一次性赋值，`ApplyResponsiveLayout` 只在尺寸变化时重算，因此“compact 被强制显示的搜索框”不会自行回到断点定义的状态，与 shell 其余部分“宽度决定可见性”的契约不一致。
- 建议：compact/medium 下点击搜索图标改为打开全屏 search view（或让搜索栏宽度改为 `可用宽度 - 其他元素` 的弹性值），并把该可见性纳入 `ApplyResponsiveLayout` 单一来源。

### G-02 搜索结果在 540–1199 DIP 下没有任何可见反馈 **[推断，逻辑部分已由代码确认]**

- 证据：查询只切换左侧索引按钮的可见性，没有结果面、计数或空状态：
  ```csharp
  // MainWindow.axaml.cs:531-536
  foreach (var entry in _galleryIndex)
      entry.Button.IsVisible = string.IsNullOrEmpty(query) || entry.Matches(query);
  ```
  而 600–1200 DIP 期间 `_usesModalNavigation = true`、抽屉默认关闭（`ApplyResponsiveLayout`，`MainWindow.axaml.cs:349,376-388`）。现有测试只在 1300/1700 DIP（`width >= 1200`，抽屉默认打开）下断言搜索生效，恰好绕过了这个区间。
- 后果：用户在 540–1199 DIP 窗口里在 app bar 输入关键词，屏幕上**不会发生任何变化**（被过滤的是关闭着的抽屉里的按钮）。这既不是 M3 的 docked search，也不是 search view，缺少“输入 → 结果 → 选择 → 回到上下文”的闭环。
- 建议：把搜索做成独立的 search view / 结果列表（含“无结果”状态）覆盖内容区，或在抽屉未打开时自动把结果呈现在内容区；同时给 `MdSearchBar` 加结果计数或 live region。

### G-03 回车后查询被替换成页面标题，导航列表被永久过滤 **[推断]**

- 证据：
  ```csharp
  // MainWindow.axaml.cs:538-544
  var match = _galleryIndex.FirstOrDefault(entry => entry.Matches(query));
  if (match is null) return;
  Navigate(match.Factory(), match.Button);
  GallerySearch.Text = match.Title;   // 会再次触发 TextChanged → 只保留该项可见
  ```
- 后果：提交后 `GallerySearch.Text` 变成目标页标题，`TextChanged` 随即把导航列表过滤到单项；用户想继续浏览目录必须先手工清空搜索框。M3 的 docked search / search view 在选中结果后应关闭搜索状态并恢复原有导航上下文，而不是把查询框改写成“当前位置”。
- 建议：提交后清空（或保持）查询并显式恢复索引可见性，把“当前页面”表达交给 app bar 标题（见 G-04）。

### G-04 app bar 高度与标题语义都不匹配 M3 规格

- 证据：`ContentShell` 首行固定 88 DIP（`MainWindow.axaml:58`）；标题区恒为产品名 `Material Design 3` + 副标题 `Avalonia component gallery`（`MainWindow.axaml:70-72`），不随目的地变化。
- M3 依据：top app bar 的高度规格是 small 64 / medium 112 / large 152 DIP，88 DIP 不对应任何一种；top app bar 的标题应反映**当前页面/目的地**，产品名属于品牌区（当前实现里品牌已在主轨道和 compact 品牌方块出现两次，app bar 再次重复）。
- 后果：进入任意组件页后窗口标题区仍显示产品名，用户失去“我在哪”的锚点（右侧目录窗格要到 ≥ 1200 DIP 才出现，窄窗口完全没有位置提示）。
- 建议：选择 64 DIP（small）或 112 DIP（medium）规格，并把标题绑定到当前目的地（`BuildTableOfContents` 已经能拿到页面标题，可直接复用）。

## 2. 中优先级（组件语义与规格）

### G-05 抽屉/轨道导航项用 `MdButton` 拼装，不是 M3 navigation item

- 证据：抽屉里约 94 个目的地全部是 `<md:MdButton Variant="Text" HorizontalContentAlignment="Left" .../>`（`MainWindow.axaml:85-195`，其中 `Click="Show*"` 95 处；全文件 100 处含轨道与示例入口），未设置高度 → 取 `MdButton` 默认 Small 容器 **40 DIP**（`Themes/Tokens/ButtonTokens.axaml:12`），默认圆角取 `Md.Comp.Button.Small.Shape.Round`；选中态靠切换 `Variant = Tonal` 表达（`MainWindow.axaml.cs:487`），配色正好是 M3 活动指示器用的 `SecondaryContainer` / `OnSecondaryContainer`（`Themes/Controls/MdButton.axaml:104-107`）。
- 与 M3 的差异：
  - 尺寸：M3 navigation drawer item 高 56 DIP、指示器为 28 DIP 圆角的整行药丸；当前是 40 DIP 高的普通按钮（圆角 14.4 DIP 一类），行高与指示器形状都不符。
  - 结构：M3 drawer item 含 24 DIP leading icon；当前项只有文本。
  - 语义：M3 要求暴露 navigation item 角色与选中状态；当前是 `Button`，读屏只报“按钮 + 文本”，选中态只体现在颜色上。
- 讽刺点：库里已经有更贴近规范的 `MdList` / `MdListItem`（`src/Md3.Avalonia/Controls/MdListItem.cs`、`Themes/Controls/MdList.axaml`，含 40 DIP leading 容器），gallery 却没有用它来示范自己的导航项。
- 建议：抽屉项改用带 leading icon 的 list item（56 DIP 高 + 28 圆角指示器），并补 `NavigationViewItem` 语义与 selected 状态；轨道项保持图标 + 标签，但按第 G-06 条调整尺寸。

### G-06 主轨道规格偏离 navigation rail 的常用值

- 证据：轨道列宽 96 DIP（`MainWindow.axaml:24`），分类项 `Width="80" Height="64" ContainerCornerRadius="20"`，图标 `Size="22"`、标签 `FontSize="11"`（`MainWindow.axaml:37-49`）。
- M3 navigation rail 的常用规格：轨道宽 80 DIP，活动指示器 56×32 DIP（full shape），图标 24 DIP，标签 12sp；指示器只包住图标，不覆盖整行。
- 后果：这里“选中项”是把整个 80×64 按钮填成 tonal 色，视觉上更像纵向排列的 tonal 按钮组，而不是 rail 的图标指示器；标签 11 DIP 也小于 12sp，长语言（德语/中文）下更拥挤。
- 说明：若这是刻意采用的 Expressive 规格，需要在文档里写明出处；否则建议回到 80 DIP 轨道 + 56×32 指示器。

### G-07 窄窗口下语言与主题切换不可达 **[代码确认]**

- 证据：`LanguageSelector.IsVisible = width >= 1040`、`ThemeSelector.IsVisible = width >= 420`（`MainWindow.axaml.cs:360-361`）；整个 shell 只有这两个语言/主题入口（`MainWindow.axaml:75-76`），抽屉与轨道里没有任何等价项（已核对 shell 全文件）。
- 后果：< 1040 DIP 无法切换语言；< 420 DIP 无法切换主题。M3 自适应原则是“尺寸类只改变布局，不把关键控制变成不可达”。Theme Lab 页面可以改主题，但那是演示页，语言则完全没有替代路径。
- 建议：把语言/主题放进抽屉底部或 app bar 的 overflow 菜单，保证任意宽度至少有一个入口。

### G-08 次级文本用 `Opacity` 模拟，而不是 `OnSurfaceVariant` 角色

- 证据：页面里 `Opacity="0.78"` 10 处、`Opacity="0.72"` 2 处、另有 `0.8`/`0.85` 若干（例：`BeforeAfterGalleryPage.axaml:28,31,34`、`ComponentsOverviewGalleryPage.axaml:11`、`PagedItemsGalleryPage.axaml:27`）。
- 与 M3/本库的关系：M3 用颜色角色（`onSurfaceVariant`）表达次级文本，库也提供了 `Md.Sys.Color.OnSurfaceVariant.Brush`；用透明度叠加会让实际对比度随背景变化、脱离 Theme Lab 的对比度诊断口径，并且在非 Surface 背景上不可预测。
- 建议：把这类 `Opacity` 换成 `OnSurfaceVariant`（或对应的语义角色）笔刷。

### G-09 主题/语言用 `MdComboBox` 表达（判断项）

- 证据：`<md:MdComboBox ... Label="Language"/>`、`<md:MdComboBox ... Label="Theme"/>`（`MainWindow.axaml:75-76`）。
- 说明：M3 目录里没有 combo box 组件，主题切换在 M3 站点/示例里通常用图标按钮 + 菜单或分段按钮表达。这条属于“与 M3 习惯不一致”，不是硬性规格冲突，优先级最低；如果保留，建议至少在文档中说明这是 Avalonia 桌面适配的选择。

### G-10 抽屉遮罩把 scrim 颜色硬编码在 shell 里

- 证据：`<Border x:Name="NavigationScrim" ... Background="#66000000" />`（`MainWindow.axaml:215`）。
- 与 M3/本库的关系：M3 的 scrim 是独立语义角色（黑色 + 32% 不透明度，随主题走），`#66000000` 是 40% 黑；库自身的审计（P2-06）也把散落的 `#66000000`/`#B3000000` 列为待替换项，gallery 的 shell 又多出一处。
- 后果：Theme Lab 生成的配色不会影响这个遮罩；深色/高对比方案下遮罩权重与其他模态不一致。
- 建议：改用语义 scrim token（若库尚无该 token，先按 P2-06 补齐），shell 与组件共用同一来源。

### G-11 折叠用的隐藏规则不一致 **[代码确认]**

- 证据：隐藏策略是逐个控件写死阈值（`GallerySearch` 540、`BrandTitle` 640、`LanguageSelector` 1040、`ThemeSelector` 420、`TableOfContentsPane` 1200，`MainWindow.axaml.cs:359-363`），与 `CurrentBreakpoint` 五档没有映射关系，也没有集中表；G-01 正是这种散落策略的直接后果。
- 建议：把“控件 → 可见阈值”收敛到一张表（或按尺寸类声明），让断点、可见性与搜索入口三个概念共用同一份定义，便于测试整表。

### G-12 1200 DIP 断点处内容宽度塌缩 **[推断，算术]**

- 证据：`ShellGrid.ColumnDefinitions` 在 Large（1200–1599）为 `260,*,210`、ExtraLarge 为 `280,*,240`（`MainWindow.axaml.cs:367-372`），`PageHost.Margin` 从 Expanded 的 `36,36,32,44` 跳到 `64,56,56,72`（`MainWindow.axaml.cs:374-380`），同时抽屉从 modal 转为常驻（`NavigationPane.IsOpen = true`）。
- 推演：宽度 1199 DIP 时内容列 = 1199 − 96（轨道）− 68（边距）≈ **1035 DIP**；宽度 1200 DIP 时内容列 = 1200 − 96 − 260 − 210 − 120 ≈ **514 DIP**。跨过一个像素，正文可用宽度掉一半。
- 与 M3 的关系：自适应布局的预期是内容宽度随窗口单调（或至少平滑）变化；在三栏出现的瞬间让正文腰斩，会让该区间内的组件示例（如 480–520 DIP 宽的示例块）被压到窄列里，而 1199 DIP 时它们很宽裕。
- 建议：把目录窗格推迟到更宽的阈值（例如 ≥ 1440），或让右侧窗格随宽度滑动式出现；至少用测试固定 1200/1199 两个宽度下的内容列宽度，避免今后无声地把正文压得更窄。

## 3. 无障碍（gallery 自身，不是库）

### G-13 shell 完全没有 Automation 信息

- 证据：`MainWindow.axaml` 中 `AutomationProperties` 出现 **0** 次；整个 gallery 只有 5 处（`AndroidGalleryView.axaml:24,40,62`、`CodeExample.axaml:44`、`GridTileGalleryPage.axaml:15`）。两个纯图标按钮 `ToolTip.Tip="Search"` / `"Open navigation"`（`MainWindow.axaml:35,63`）没有任何可访问名称。
- 依据：Avalonia 不会把 `ToolTip.Tip` 映射为 `AutomationProperties.Name`；库内其它控件都是显式 `AutomationProperties.SetName/SetHelpText`（`src/Md3.Avalonia` 共 17 处），说明本仓库既有约定就是“显式命名”，shell 漏掉了。
- 后果：读屏用户在主界面只能听到“按钮”，无法区分搜索与导航。
- 建议：给 shell 的所有图标按钮补 `AutomationProperties.Name`（并用 `GalleryLocalization` 一起本地化），并在 CI 里加一条“shell 内每个无文本按钮必须有 Name”的断言（现有测试完全没有覆盖 gallery shell 的 AX）。

### G-14 图标按钮的 tooltip 未本地化

- 证据：`GalleryLocalization.Apply` 只处理 `TextBlock.Text`、`ContentControl.Content`、`TextBox.PlaceholderText`、`MdTextBox/MdComboBox/MdCascader` 的 `Label`/`PlaceholderText`（`GalleryLocalization.cs:171-220`），不处理 `ToolTip.Tip`；zh-CN 字典里也没有 `Search` / `Open navigation`（grep 计数 0）。
- 后果：切到中文后主界面两个 tooltip 仍是英文，而页面正文已中文化，语言体验不一致。
- 建议：在 `Apply` 中处理 `ToolTip.Tip`（与 G-13 的 Name 同步），并把两条文案加入字典。

### G-15 导航项的选中状态没有进入无障碍树

- 证据：选中态仅体现为 `Variant` 颜色切换（`MainWindow.axaml.cs:487`、`500`），`MdButton` 不暴露 selected/current 语义。
- 后果：读屏用户无法判断当前所在目的地；这也是 G-05 换用 navigation item 语义后自然修好的一项。

## 4. 既有组件级审计中“在 gallery 里可直接看到”的项

下列问题来自 2026-10-02 的组件审计与工具链审计，gallery 有独立页面可以直接复现，本次不重复论证，仅给出观察位置以便实跑验收：

| 编号 | 现象 | gallery 观察位置 | 来源 |
|---|---|---|---|
| P1-01 | Chip 32 / segmented 40 / rating 约 32 / 日期选择器 36–40 DIP，都不满足 48 DIP 触控包围盒；Small FAB 无独立 48 DIP 命中区 | Chips、Segmented & range、Rating、Date & time pickers、FABs 页 | 2026-10-02 §4 |
| P1-05 | Carousel 在 Reduced Motion 下仍按选中项改变条目宽度（M3 要求所有条目同尺寸） | Carousel 页 | 同上 |
| P1-08 | Tooltip 默认 100ms 隐藏（官方约 1.5s），且与触发控件的辅助描述没有关联 | Tooltips 页 | 同上 |
| P1-09 | Bottom sheet 拖拽把手仅 32–36 DIP 且不可聚焦，无 Space/Enter/Escape | Sheets 页 | 同上 |
| P1-19 | Popover 对任意顶层滚动都关闭；HoverCard 打开时会抢焦点 | Overlay / 命令面板 / HoverCard 页 | 同上 |
| P2-01 | Top/Bottom app bar 默认 1 DIP 描边 + 20 圆角，呈“描边圆角卡片”而非边到边 app bar；Bottom app bar 属当前不再推荐的兼容组件 | App bars 页（以及 shell 自身的 app bar，见 G-04） | 2026-10-02 §5 |
| P2-02 | Toolbar 默认 Padding 8 / 项间距 4，与当前 toolbar 的边缘与操作间隔指导偏离 | Toolbars 页 | 同上 |
| P2-07 | 固定宽高（如 License 页 `260,*`、命令面板 `Width=600`）在 200% 字体与窄弹层下的裁剪风险未验证 | License、Command palette 等页 | 同上 |
| P2-08 | 多处硬编码英文默认文案与 Automation 名称（shell 已走字典，部分 Extra 页仍未覆盖） | 逐页扫描 | 同上 |

## 5. 走查中确认符合 M3（或已达到工程要求）的部分

- 五档断点（600/840/1200/1600）+ 内容区 `MaxWidth=960` + 1200 DIP 起出现目录窗格，✅ 由 `Gallery_Uses_Five_Breakpoint_Bands_And_Search_Index` 固定。
- 一次导航只有一个“活动指示器”：`Navigate()` 有意让分类轨道在进入叶页面时清除高亮（`MainWindow.axaml.cs:498-503` 的注释与实现一致），符合 M3“只有一个当前目的地”的表达。
- 语言切换后 shell 与页面一起本地化（`Apply(this, culture)` + `Apply(page, culture)`，142 条字典），✅ 由 `Gallery_Language_Selector_Updates_Visible_Chrome` 覆盖。
- 图标按钮主题自带 48 DIP 最小命中区（`Themes/Controls/MdIconButton.axaml:5`），所以 G-01 是布局溢出问题，不是触控目标问题。
- 页面在窄窗口下会重排（`Every_Gallery_Page_Fits_A_Narrow_Window`），且 shell 会收缩页面内过宽的固定尺寸示例（`ApplyResponsivePageSizing`）——即 M3 的 reflow 要求在页面层面已被测试固定，问题集中在 shell 自身。
- 全部目的地可从导航与概览页到达，无死链（`Components_Overview_Links_Resolve_And_Cover_Every_Indexed_Page`、`Every_Overview_Link_Navigates_To_Its_Page`）。

## 6. 建议落成 headless 回归测试（可在 CI 证实/证伪）

| 编号 | 建议测试 | 期望 |
|---|---|---|
| G-01 | 窗口 480 DIP → 找到搜索图标按钮并 `RaiseEvent`/点击 → 断言 app bar 内元素实测总宽 ≤ 可用宽（或断言搜索以全屏视图打开） | 无溢出、无裁切 |
| G-02 | 窗口 1000 DIP → 在搜索栏输入 “theme” → 断言存在用户可见的结果面或空状态（当前断言只验证被隐藏按钮的 `IsVisible`） | 输入即有可见反馈 |
| G-03 | 提交搜索后断言查询框内容与索引可见性恢复策略 | 提交后导航上下文可恢复 |
| G-04 | 断言 app bar 标题等于当前目的地标题（`BuildTableOfContents` 已能取到） | 标题随页面变化 |
| G-05 | 断言抽屉项实测高度 56 DIP 且存在 leading icon 容器 | 与 M3 drawer item 一致 |
| G-07 | 窗口 400 DIP → 断言存在可达的语言/主题入口（例如抽屉内存在对应按钮） | 关键控制不因宽度消失 |
| G-08 | 静态检查 gallery 的 AXAML 不出现用 `Opacity` 表达文本层级（或显式白名单） | 次级文本统一走角色色 |
| G-11 | 断言 shell 的“控件 → 可见阈值”表覆盖所有受断点影响的控件 | 隐藏规则集中且可测 |
| G-12 | 断言 1199 与 1200 DIP 下 `PageHost` 的实测宽度差不超过阈值（当前约 1035 → 514） | 断点处内容宽度不塌缩 |
| G-13 | 遍历 shell 中无文本的按钮，断言 `AutomationProperties.Name` 非空 | 图标按钮全部有可访问名称 |

## 7. 尚未验证（需要真机/视觉确认）

- 触摸、读屏（Narrator/VoiceOver/Orca）、200% 字体、RTL：gallery 目前没有 RTL 演示页，本次也未做真机核对。
- G-01 的裁剪位置与 G-06 的视觉权重属于像素级结论，需要一次真实桌面运行或 headless 帧截取确认。
- 本文未覆盖 Android single-view host（`AndroidGalleryView.axaml`）的导航/无障碍一致性，仅在其 Automation 名称覆盖率上作为对照出现。
