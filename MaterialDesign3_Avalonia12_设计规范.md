# Material Design 3 for Avalonia 12 设计规范

> 工作名称：`Md3.Avalonia`  
> 文档版本：0.5 Draft  
> 基线日期：2026-09-22  
> 目标框架：Avalonia 12.x / .NET 8+（Gallery 建议 .NET 10）  
> 必须支持的平台：Windows、macOS、Linux、Android  
> 设计基线：Material Design 3 当前规范，优先采用 M3 Expressive；尚未 Expressive 化的组件采用当前 Baseline 规范

---

## 1. 文档目的

本文定义一个面向 Avalonia 12 的 Material Design 3 控件库及配套 Gallery 的视觉、架构、API、主题、动画、资源、测试和交付规范。

最终产品分为两部分：

1. **控件库**：提供以 `Md` 为前缀的独立控件类型、完整资源字典、动态主题、深浅模式、字体、图标、动画和无障碍支持。
2. **Gallery**：作为组件文档、交互演示、主题编辑器和视觉回归基准，其信息架构与整体布局模仿 Material Design 3 官网。

本文同时作为后续控件实现与验收的约束；实现按组件分阶段推进。

---

## 2. 核心设计结论

### 2.1 强制约束

- 所有公开交互控件必须是独立 CLR 类型，例如 `MdButton`、`MdTextBox`、`MdSwitch`。
- **禁止**通过全局样式覆盖 Avalonia 原生控件，例如禁止 `Style Selector="Button"`、`Style Selector="TextBox"`。
- 允许从 Avalonia 原生控件继承以复用行为和 AutomationPeer，但必须使用自己的类型、默认主题键和完整模板。
- 控件库不得依赖 `Avalonia.Themes.Fluent` 或 `Avalonia.Themes.Simple` 才能正常渲染。
- Gallery 中所有面向用户的交互控件必须使用 `Md*` 控件；`Grid`、`Panel`、`Border`、`Path` 等纯布局/绘图原语可以直接使用 Avalonia 类型。
- 颜色、字体、圆角、间距、阴影、状态层和动画不得散落硬编码在模板中，必须来自设计令牌。
- 所有会在运行时变化的主题资源必须通过 `DynamicResource` 消费。
- 控件库必须可被 `net8.0-android` Avalonia 应用引用并在 Android 真机/模拟器上运行，不得在核心控件中引入仅桌面可用的 API。
- 工作区只交付源码、设计文档、必要的规范快照与经审批的参考图；不得交付 `bin/`、`obj/`、`TestResults/`、`artifacts/`、`.nupkg`、DLL、PDB 等编译或测试产物。

### 2.2 设计档位

提供两个设计档位：

- `MdDesignProfile.Expressive`：默认档位，采用当前 M3 Expressive 组件、强调字体、扩展形状和弹簧运动。
- `MdDesignProfile.Baseline`：兼容当前官网仍保留的 Baseline 组件及已标记为 deprecated 的历史变体。

若同一组件在官网同时存在 Expressive 和 Baseline：新项目默认 Expressive，Gallery 同时展示两者并标记状态。若官网尚未提供 Expressive 版本，则使用当前 Baseline 规范，不自行发明视觉规则。

---

## 3. “精准模仿”的定义

Material 官网是持续更新的网站，浏览器和 Avalonia 的文字光栅化也不同，因此“精准”按以下可验证标准定义：

| 项目 | 验收标准 |
|---|---|
| 颜色 | sRGB 令牌值、透明度、角色配对与冻结基线一致 |
| 几何 | 尺寸、间距、描边、圆角误差不超过 ±1 DIP |
| 字体 | 字号、行高、字重、字距、字体角色一致；允许平台抗锯齿差异 |
| 状态 | enabled、hover、focus、pressed、selected、dragged、error、disabled 等状态齐全 |
| 动画 | 使用相同语义弹簧令牌；阻尼比、刚度、初速度及属性分类一致 |
| 图标 | Material Symbols 图形、尺寸、视觉重量和填充状态一致 |
| 交互 | 键盘、鼠标、触摸、焦点、命令、弹层关闭规则与组件规范一致 |
| Gallery | 信息架构、三栏文档布局、导航层级、主题/搜索入口和响应式行为与官网一致 |

以下差异不作为缺陷：操作系统级字体 hinting、Skia 与浏览器间的亚像素抗锯齿差异、原生窗口装饰差异。

### 3.1 规范冻结流程

实现前必须生成 `spec-snapshot/manifest.json`，记录：

- 来源 URL；
- 抓取日期；
- 官网页面更新时间；
- AndroidX/Material Components 源码 commit SHA；
- M3 token generation version；
- Figma Design Kit 版本（若可合法获取）；
- 每个组件采用 Baseline 或 Expressive 的决定。

源优先级如下：

1. Material 3 官网当前组件 `Specs`；
2. Material 官方生成的 Compose/MDC/Web 组件令牌；
3. Material 3 Design Kit；
4. 本项目设计决定。

第 4 类值必须在代码和文档中明确标记为 `MdDecision`，不得伪装成官方 M3 数值。

---

## 4. 范围

### 4.1 首版范围

- Avalonia 12.x；所有 Avalonia 包必须锁定同一补丁版本。
- 核心控件库面向 `net8.0`，不得引用桌面专属 API；桌面 Gallery 面向 `net10.0`。
- Tier 1：Windows、macOS、Linux 桌面以及 Android。
- Android 宿主面向 `net8.0-android`，至少覆盖 Android 8.0（API 26）及以上、ARM64、触摸、软键盘、返回键、生命周期恢复、深浅主题和安全区域。
- Tier 2：iOS、Browser/WASM 的编译和基础交互验证。
- Light、Dark、System 三种主题模式。
- 任意 seed color 生成完整亮/暗配色，不限于 M3 预设紫色。
- 标准、Medium Contrast、High Contrast 色彩生成能力。
- M3 Standard 与 Expressive 两套 motion scheme。
- 中英文文本及 RTL 布局基础能力。

### 4.2 非首版目标

- 不将 Avalonia 原生控件全局“Material 化”。
- 不复制 Material 官网文字、插图或商标资产作为产品内容。
- 不承诺与 Chrome 的每个像素完全相同。
- 不为 M3 没有规范的复杂组件宣称“官方 Material 组件”，例如 DataGrid、TreeGrid、PropertyGrid。
- 不在核心包中强制依赖某一种 MVVM 框架。

---

## 5. 需求追踪

| ID | 原始需求 | 设计响应 |
|---|---|---|
| R1 | 精准模仿 M3 官网控件样式 | 规范冻结、官方令牌生成、视觉回归与状态矩阵 |
| R2 | Avalonia 12，颜色/UI/动画/字体完整 | 独立主题、令牌系统、Composition 动画、嵌入字体 |
| R3 | 控件库 + Gallery | 多项目解决方案，Gallery 采用官网式三栏文档布局 |
| R4 | 使用 `MdButton` 等类 | 所有公开控件独立类型，禁止覆盖原控件 |
| R5 | MVVM 友好且支持直接操作 | 标准 `ICommand`、双向绑定、事件、方法、服务接口 |
| R6 | 完整资源字典、字体、主题色、字体图标、任意强调色 | 三层令牌、HCT 主题生成、字体与 Material Symbols 包 |
| R7 | 深浅模式 | `ThemeVariant`、Light/Dark 字典、System 跟随与局部作用域 |
| R8 | 控件库可在 Android 运行 | Android 列为 Tier 1；提供 `net8.0-android` 宿主、触摸/软键盘/生命周期真机测试 |
| R9 | Workspace 不提供编译产物 | 只交付源码与必要文档；构建验证后清理所有 `bin/obj/TestResults/artifacts` 等产物 |

---

## 6. 解决方案结构

```text
Md3.Avalonia.sln
├─ src/
│  ├─ Md3.Avalonia/                  # 控件、公共 API、Themes/令牌、HCT 动态主题、Motion
│  ├─ Md3.Avalonia.Icons/            # 完整官方 Material Symbols 字体与强类型目录
│  ├─ Md3.Avalonia.Icons.Lite/       # 官方字体真实轮廓子集
│  ├─ Md3.Avalonia.Extra/            # Flutter 生态 clean-room 控件
│  ├─ Md3.Avalonia.DataGrid/         # Avalonia 原生 DataGrid 的 Material 主题（opt-in）
│  └─ Md3.Avalonia.RichEditor/       # AvaloniaRichEditor 的 Material 主题（opt-in）
├─ gallery/
│  ├─ Md3.Avalonia.Gallery/          # 官网式桌面 Gallery，使用 CommunityToolkit.Mvvm
│  ├─ Md3.Avalonia.Gallery.Desktop/  # 桌面宿主
│  └─ Md3.Avalonia.Gallery.Android/  # Android 宿主，共享 Gallery 页面/VM（solution 外）
├─ tests/
│  └─ Md3.Avalonia.HeadlessTests/    # 含 Spec/ 分层规范一致性校验（L1/L2/L3/L5）
└─ spec-snapshot/                     # 冻结的来源清单、令牌和一致性策略
```

可发布包（六个，均可独立 pack）：

- `Md3.Avalonia`：核心控件、主题、动态 HCT 配色、motion 与公共 API；
- `Md3.Avalonia.Icons`：完整官方 Material Symbols Rounded 字体与强类型目录；
- `Md3.Avalonia.Icons.Lite`：官方字体的真实轮廓子集；
- `Md3.Avalonia.Extra`：依赖核心的 clean-room 扩展控件；
- `Md3.Avalonia.DataGrid`：唯一引入 `Avalonia.Controls.DataGrid` 依赖的 opt-in 主题包；
- `Md3.Avalonia.RichEditor`：唯一引入 AvaloniaRichEditor 依赖的 opt-in 主题包。

### 6.1 Android 工程约束

- 核心控件和主题项目只使用 Avalonia 跨平台 API，不引用 Win32、AppKit、X11 或桌面 lifetime 类型。
- Android 宿主仅负责平台启动、权限、返回键、安全区域和生命周期桥接；控件 XAML、页面与 ViewModel 必须与桌面共享。
- 文本输入控件必须验证软键盘弹出/收起、IME composing text、selection、复制粘贴、密码输入与窗口 resize/pan 行为。
- popup、dialog、sheet、menu 和 tooltip 必须适配触摸输入及 Android 返回键；不得假定存在 hover、右键或物理键盘。
- Android Release 至少执行一次 ARM64、trim/AOT 配置构建，并在模拟器和一台真机上完成 Light/Dark 与旋转恢复冒烟测试。

### 6.2 Workspace 交付与清理

工作区是源码交付区，不是二进制分发区：

- `.gitignore` 必须覆盖 `**/bin/`、`**/obj/`、`TestResults/`、`artifacts/`、IDE 缓存和用户文件。
- 可以在验证过程中执行 `dotnet build/test/pack`，但本轮交付前必须删除由其产生的目录和文件。
- 工作区不得保留 DLL、PDB、`.deps.json`、`.runtimeconfig.json`、`.nupkg`、测试结果、覆盖率文件或 Android APK/AAB。
- 经人工审批并用于文档/视觉回归的 PNG 参考图属于源码资产，不视为编译产物；临时渲染帧必须删除。
- NuGet 包和 APK/AAB 只能由 CI Release artifact 或正式发布渠道提供，不能提交到 workspace。
- 交付检查必须运行一次产物扫描；扫描发现上述文件即视为未完成。

---

## 7. 控件架构规范

### 7.1 控件类型选择

| 场景 | 基类策略 |
|---|---|
| 已有成熟行为语义 | 从 `Button`、`TextBox`、`ToggleButton`、`SelectingItemsControl` 等继承 |
| 外观与行为完全自定义 | 从 `TemplatedControl` 继承 |
| 列表/导航/菜单 | 从 `ItemsControl` 或选择类控件继承，必须支持虚拟化 |
| 纯视觉层 | 从 `Control` 继承并自绘或使用 Composition |
| 页面级组合 | 使用 `ContentControl`/`HeaderedContentControl`，不使用 UserControl 作为通用库控件 |

每个模板控件必须：

- 具有 `ControlTheme x:Key="{x:Type md:MdXxx}"`；
- 只匹配自己的 `Md*` 类型；
- 对模板部件使用 `PART_` 前缀；
- 对视觉状态使用伪类，不依赖 ViewModel 中的颜色/动画逻辑；
- 在 `OnApplyTemplate` 中校验必要模板部件并安全解绑旧事件；
- 为复杂控件提供自定义 AutomationPeer。

### 7.2 禁止的实现方式

```xml
<!-- 禁止：污染所有 Avalonia Button -->
<Style Selector="Button">...</Style>

<!-- 禁止：控件模板中散落品牌色 -->
<Border Background="#6750A4" />

<!-- 禁止：Gallery 直接用原生交互控件冒充 Material 控件 -->
<Button Content="Save" />
```

### 7.3 伪类

公共状态统一采用：

```text
:pointerover  :pressed  :focus  :focus-visible  :disabled
:checked      :selected :indeterminate          :read-only
:error        :expanded :dragged                 :loading
:opening      :open     :closing
```

状态优先级：`disabled > error > pressed/dragged > focus-visible > pointerover > selected/resting`。组件可增加私有伪类，但不得改变公共状态语义。

### 7.4 属性类型

- 可绑定、可样式化、可动画属性：`StyledProperty`。
- 只读状态或对性能敏感且不允许样式化的值：`DirectProperty`。
- `Text`、`Value`、`SelectedItem`、`SelectedDate`、`SelectedTime`、`IsChecked`、`IsOpen` 默认双向绑定。
- 命令统一使用 BCL `ICommand`，不得要求 ViewModel 引用 Avalonia 或本库类型。
- 集合使用 `IEnumerable`/`IList`/`ObservableCollection<T>` 兼容的 `ItemsSource`。
- 验证支持 `INotifyDataErrorInfo`、DataAnnotations、异常验证及 Avalonia `DataValidationErrors`。

---

## 8. 公共 API 规范

### 8.1 命名

- 控件：`MdButton`、`MdTextBox`。
- 枚举：`MdButtonVariant`、`MdControlSize`。
- 事件参数：`MdSelectionChangedEventArgs`。
- 服务：`IMdDialogService`、`IMdSnackbarService`。
- 资源键常量：`MdResourceKeys.SysColorPrimaryBrush`。
- 不使用 `MaterialButton` 与 Avalonia/其他 Material 库名称冲突。

### 8.2 MVVM 用法

核心库不依赖 CommunityToolkit.Mvvm，但必须与其生成的属性和命令直接兼容。

```csharp
public partial class LoginViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [Required]
    private string? userName;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private Task SubmitAsync(CancellationToken token) => LoginAsync(token);

    private bool CanSubmit() => !string.IsNullOrWhiteSpace(UserName);
}
```

```xml
<md:MdTextBox Text="{Binding UserName}"
              Label="User name"
              ValidatesOnDataErrors="True" />
<md:MdButton Content="Sign in"
             Variant="Filled"
             Command="{Binding SubmitCommand}" />
```

Gallery 及示例必须开启 compiled binding，并为视图声明 `x:DataType`。

### 8.3 直接操作用法

```csharp
var button = new MdButton
{
    Content = "Save",
    Variant = MdButtonVariant.Filled
};
button.Click += (_, _) => Save();

var input = new MdTextBox { Label = "Name", Text = "Avalonia" };
input.SelectAll();
```

弹层类控件同时提供：

- 控件方法：`ShowAsync`、`Open`、`Close`、`Dismiss`；
- `IsOpen` 双向属性；
- RoutedEvent；
- 可注入的服务接口，供纯 MVVM 场景使用；
- 不提供隐式全局 Service Locator。

---

## 9. 设计令牌与资源字典

### 9.1 三层令牌

遵循 Material 的三层模型：

1. **Reference tokens**：原始调色板、字体、尺寸。
2. **System tokens**：`primary`、`surface`、`headline-large` 等语义角色。
3. **Component tokens**：按钮容器色、文本框描边、FAB 高度等。

组件模板只能引用 Component/System token，不直接引用十六进制颜色或裸数值。

### 9.2 资源键格式

同时提供 Color 与 Brush：

```text
Md.Ref.Palette.Primary.40.Color
Md.Sys.Color.Primary.Color
Md.Sys.Color.Primary.Brush
Md.Sys.TypeScale.BodyLarge.FontSize
Md.Sys.Shape.Corner.Medium
Md.Sys.Elevation.Level2.Shadow
Md.Sys.Motion.Spring.Fast.Spatial
Md.Comp.Button.Filled.ContainerColor
```

对外同时提供强类型键常量，避免 C# 中使用魔法字符串。

### 9.3 字典拆分

```text
Themes/
├─ MaterialTheme.axaml
├─ Tokens/
│  ├─ Reference/
│  │  ├─ Palettes.axaml
│  │  └─ Typefaces.axaml
│  ├─ System/
│  │  ├─ Colors.axaml
│  │  ├─ Typography.axaml
│  │  ├─ Shape.axaml
│  │  ├─ Spacing.axaml
│  │  ├─ Elevation.axaml
│  │  ├─ State.axaml
│  │  └─ Motion.axaml
│  └─ Components/
│     ├─ ButtonTokens.axaml
│     ├─ TextFieldTokens.axaml
│     └─ ...
└─ Controls/
   ├─ MdButton.axaml
   ├─ MdTextBox.axaml
   └─ ...
```

---

## 10. 色彩与动态主题

### 10.1 色彩角色

必须提供当前 M3 完整角色集：

- Primary、OnPrimary、PrimaryContainer、OnPrimaryContainer；
- Secondary、OnSecondary、SecondaryContainer、OnSecondaryContainer；
- Tertiary、OnTertiary、TertiaryContainer、OnTertiaryContainer；
- Error、OnError、ErrorContainer、OnErrorContainer；
- Surface、OnSurface、OnSurfaceVariant；
- SurfaceDim、SurfaceBright；
- SurfaceContainerLowest、Low、默认、High、Highest；
- Outline、OutlineVariant；
- InverseSurface、InverseOnSurface、InversePrimary；
- SurfaceTint、Scrim、Shadow；
- Primary/Secondary/Tertiary Fixed、FixedDim、OnFixed、OnFixedVariant；
- `Background`、`OnBackground`、`SurfaceVariant` 作为兼容别名保留，并标记 deprecated。

颜色必须按配对使用，例如 `Primary + OnPrimary`，不得将 `PrimaryContainer` 与 `OnTertiaryContainer` 任意组合。Filled action 控件使用独立 `PrimaryAction + OnPrimaryAction` 配对，以保证 Dark 下 action label 对比度；内容文字、outline 与 selection 语义仍使用标准 `Primary`。

### 10.2 任意强调色

提供 HCT/CAM16-UCS 兼容的主题生成器，最小 API：

```csharp
var scheme = MdThemeBuilder
    .FromSeed(Color.Parse("#006A6A"))
    .WithSchemeVariant(MdSchemeVariant.TonalSpot)
    .WithContrastLevel(0.0)
    .Build();

MdThemeManager.Current.Apply(scheme);
```

要求：

- 输入任意 sRGB seed color；
- 同时生成 Light 与 Dark；
- 支持 TonalSpot、Neutral、Vibrant、Expressive、Fidelity、Content、Monochrome 等 Material scheme variant；
- 可单独提供 primary/secondary/tertiary/error key colors；
- 可导入/导出 JSON；
- 可对单个角色做高级覆盖，但必须运行对比度诊断；
- 生成算法以官方 Material Color Utilities 为基准，使用 golden vectors 验证，不以 HSL 近似替代。

### 10.3 运行时更新

- 主题更新在 UI Dispatcher 上批量提交。
- 替换资源字典值，而非让控件逐一手动改色。
- 更新过程中不得出现中间半主题或明显闪烁。
- 触发一次 `MdThemeChanged` 通知。
- 1000 个可见基础控件的主题切换目标时间不超过 100 ms；动画过渡可在提交完成后执行。

---

## 11. Light、Dark 与作用域

使用 Avalonia `ThemeVariant` 作为基础机制：

```csharp
Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
Application.Current!.RequestedThemeVariant = ThemeVariant.Default; // 跟随系统
```

资源通过 `ResourceDictionary.ThemeDictionaries` 分为 `Light`、`Dark`、`Default`。

还必须支持：

- 全局主题；
- Window 级主题；
- `ThemeVariantScope` 子树主题；
- `MdThemeScope` 子树使用不同 seed color；
- Gallery 左右并排显示 Light/Dark 预览；
- System 模式在系统主题变化时实时同步。

---

## 12. 字体系统

### 12.1 字体资源

至少提供：

```text
Md.Sys.Typeface.Brand
Md.Sys.Typeface.Plain
Md.Sys.Typeface.Monospace
Md.Sys.Typeface.CjkFallback
Md.Sys.Typeface.Symbols.Outlined
Md.Sys.Typeface.Symbols.Rounded
Md.Sys.Typeface.Symbols.Sharp
```

默认建议：

- 控件正文与标签：Roboto 静态字重；
- Gallery 展示标题：在许可与 Avalonia 能力允许时使用 Google Sans Flex 固定实例；
- 中文 fallback：Noto Sans CJK SC/Noto Sans SC 或可配置的系统 fallback；
- 代码：Roboto Mono。

所有字体必须嵌入为 `AvaloniaResource`，通过 `avares://` URI 使用，并附带第三方许可文件。

### 12.2 M3 type scale

提供 15 个 Baseline 与 15 个 Emphasized 角色。Baseline 基础尺度如下：

| 角色 | 字号 | 行高 | 字重 | 字距 |
|---|---:|---:|---:|---:|
| Display Large | 57 | 64 | 400 | -0.25 |
| Display Medium | 45 | 52 | 400 | 0 |
| Display Small | 36 | 44 | 400 | 0 |
| Headline Large | 32 | 40 | 400 | 0 |
| Headline Medium | 28 | 36 | 400 | 0 |
| Headline Small | 24 | 32 | 400 | 0 |
| Title Large | 22 | 28 | 400 | 0 |
| Title Medium | 16 | 24 | 500 | 0.15 |
| Title Small | 14 | 20 | 500 | 0.1 |
| Body Large | 16 | 24 | 400 | 0.5 |
| Body Medium | 14 | 20 | 400 | 0.25 |
| Body Small | 12 | 16 | 400 | 0.4 |
| Label Large | 14 | 20 | 500 | 0.1 |
| Label Medium | 12 | 16 | 500 | 0.5 |
| Label Small | 11 | 16 | 500 | 0.5 |

数值单位为 Avalonia DIP。最终小数值以冻结 token manifest 为准，避免不同官方平台源码的四舍五入差异。

Emphasized 令牌保持相同字号/行高，使用当前官方强调字重与字距；不得简单在调用点写 `FontWeight="Bold"` 替代。

### 12.3 Avalonia 变量字体限制

Avalonia 12 当前不支持完整变量字体轴。处理策略：

1. 构建时将 Google Sans Flex/Material Symbols 的常用轴值生成固定 TTF 实例；
2. 在发行包中记录源字体版本、轴值、哈希和许可证；
3. `MdIcon` 的连续 `Weight/Grade/OpticalSize` 属性映射到最近的固定实例；
4. 对必须连续变化的图标使用 SVG Geometry fallback；
5. 不宣称支持运行时连续变量轴动画。

---

## 13. 图标系统

### 13.1 `MdIcon`

```csharp
public sealed class MdIcon : Control
{
    public MdSymbol Symbol { get; set; }
    public MdSymbolStyle Style { get; set; }   // Outlined/Rounded/Sharp
    public bool Filled { get; set; }
    public double Size { get; set; }
    public int Weight { get; set; }
    public int Grade { get; set; }
    public int OpticalSize { get; set; }
}
```

要求：

- 默认图标尺寸来自组件令牌，而非固定 24；
- 提供强类型 `MdSymbols.Home`、`MdSymbols.Search` 等目录；
- 支持字体 glyph 与 Geometry 两种渲染源；
- 装饰性图标默认不进入 Automation tree；
- 独立图标按钮必须有 `AutomationProperties.Name`；
- 图标颜色默认继承 `Foreground`。

---

## 14. Shape、Spacing、Elevation 与 State

### 14.1 Shape scale

| Token | 圆角 |
|---|---:|
| None | 0 |
| ExtraSmall | 4 |
| Small | 8 |
| Medium | 12 |
| Large | 16 |
| LargeIncreased | 20 |
| ExtraLarge | 28 |
| ExtraLargeIncreased | 32 |
| ExtraExtraLarge | 48 |
| Full | 50% / 充分大的半径 |

嵌套圆角遵循 optical roundness：`inner radius = outer radius - padding`。

### 14.2 Spacing scale

基于 8 DIP 基线，至少提供：

```text
0, 2, 4, 6, 8, 12, 16, 20, 24, 32, 40, 48, 56, 64, 72
```

组件只能引用 spacing token；Gallery 的页面边距可在 M3 breakpoint 处切换。

### 14.3 Elevation

提供 Level 0–5，基准高度：`0, 1, 3, 6, 8, 12 dp`。

- 优先使用 Surface Container 色调区分层级；
- 阴影只用于确需表现悬浮/重叠的组件；
- 每一级提供 `BoxShadows` 与 surface role 映射；
- hover/drag 可临时提升，但不得把 Level 4/5 当普通 resting elevation。

### 14.4 State layer

提供独立状态透明度令牌：hover、focus、pressed、dragged、disabled content、disabled container。`MdStateLayer` 必须裁切到组件当前形状，并与 ripple 分离。

---

## 15. Motion 系统

### 15.1 语义令牌

每套 motion scheme 包含六个弹簧：

- Fast Spatial；
- Default Spatial；
- Slow Spatial；
- Fast Effects；
- Default Effects；
- Slow Effects。

Spatial 用于位置、尺寸、旋转、形状等允许 overshoot 的属性；Effects 用于颜色、透明度等不得越界的属性。

### 15.2 当前参考参数

质量 `mass = 1`，初速度默认 `0`。

| Scheme | Token | Damping ratio | Stiffness |
|---|---|---:|---:|
| Expressive | Fast Spatial | 0.6 | 800 |
| Expressive | Default Spatial | 0.8 | 380 |
| Expressive | Slow Spatial | 0.8 | 200 |
| Expressive | Fast Effects | 1.0 | 3800 |
| Expressive | Default Effects | 1.0 | 1600 |
| Expressive | Slow Effects | 1.0 | 800 |
| Standard | Fast Spatial | 0.9 | 1400 |
| Standard | Default Spatial | 0.9 | 700 |
| Standard | Slow Spatial | 0.9 | 300 |
| Standard | Fast Effects | 1.0 | 3800 |
| Standard | Default Effects | 1.0 | 1600 |
| Standard | Slow Effects | 1.0 | 800 |

### 15.3 Avalonia 实现

- 状态颜色等简单变化：Avalonia transitions。
- 多阶段模板动画：keyframe animation。
- 位移、缩放、弹簧、共享容器：Composition animation，运行在 render thread。
- `MdSpringAnimator` 将弹簧解析为稳定采样的 Composition keyframes，支持中断、反向和重新定向。
- ripple/loading indicator 等持续动画优先使用 Composition 或 CustomVisual。
- 动画不得依赖 UI 线程 `Task.Delay` 循环。

### 15.4 Reduced motion

提供 `MdMotionMode.System / Full / Reduced / None`。

- Full：完整 M3 motion。
- Reduced：取消弹跳和大幅空间位移，保留短淡入淡出与必要进度反馈。
- None：状态立即到达终点。
- System：通过平台适配器读取 Windows/macOS/Linux/Browser 的降低动态效果设置；无法读取时回退 Full。

Gallery 顶栏提供播放/暂停及 Reduced Motion 预览。

---

## 16. 控件目录

### 16.1 官方 M3 组件

| M3 类别 | 公开控件类型 | 主要变体/说明 |
|---|---|---|
| App bars | `MdTopAppBar`, `MdBottomAppBar` | small/medium/large；滚动状态 |
| Badges | `MdBadge` | small/large、数字/状态 |
| Buttons | `MdButton` | filled、tonal、outlined、text；elevated 兼容 |
| Button groups | `MdButtonGroup` | standard/connected、按压形变 |
| Icon buttons | `MdIconButton` | standard、filled、tonal、outlined、toggle |
| FAB | `MdFab`, `MdExtendedFab` | small/medium/large；primary/secondary/tertiary/surface |
| FAB menu | `MdFabMenu`, `MdFabMenuItem` | 展开/收起、焦点圈定 |
| Split button | `MdSplitButton` | primary action + menu action |
| Segmented buttons | `MdSegmentedButtonGroup`, `MdSegmentedButton` | single/multi select |
| Cards | `MdCard` | elevated、filled、outlined；可交互/不可交互 |
| Carousel | `MdCarousel`, `MdCarouselItem` | multi-browse、hero、uncontained |
| Checkbox | `MdCheckBox` | checked、unchecked、indeterminate、error |
| Chips | `MdAssistChip`, `MdFilterChip`, `MdInputChip`, `MdSuggestionChip` | leading/trailing icon、selected |
| Date picker | `MdDatePicker`, `MdDatePickerDialog` | docked/modal、single/range |
| Time picker | `MdTimePicker`, `MdTimePickerDialog` | 12/24h、input/dial |
| Dialogs | `MdDialog`, `MdDialogHost` | basic、full-screen |
| Divider | `MdDivider` | full width、inset、vertical |
| Lists | `MdList`, `MdListItem` | 1/2/3 line、leading/trailing slots |
| Loading | `MdLoadingIndicator` | Expressive shape morph |
| Progress | `MdLinearProgressIndicator`, `MdCircularProgressIndicator` | determinate/indeterminate、straight/wavy |
| Menus | `MdMenu`, `MdMenuItem` | vertical standard menu、slots、command |
| Navigation bar | `MdNavigationBar`, `MdNavigationBarItem` | compact navigation、badge slot |
| Navigation drawer | `MdNavigationDrawer` | modal/standard、left/right |
| Navigation rail | `MdNavigationRail`, `MdNavigationRailItem` | collapsed/expanded/modal/standard（后续） |
| Radio | `MdRadioButton` | group、error |
| Search | `MdSearchBar`, `MdSearchView` | bar、expandable results surface |
| Sheets | `MdSheetHost` | bottom/left/right、modal/standard |
| Slider | `MdSlider` | continuous/discrete、value/stop indicator |
| Snackbar | `MdSnackbar` | single/multi line、action、dismiss |
| Switch | `MdSwitch` | icons、selected/unselected |
| Tabs | `MdTabs`, `MdTabItem` | primary/secondary、scrollable/fixed |
| Text fields | `MdTextBox` | filled/outlined、label/supporting/error/counter、clear/password/reveal |
| Toolbars | `MdToolbar` | docked/floating、standard/vibrant、horizontal/vertical |
| Tooltips | `MdTooltip`, `MdTooltipHost` | plain/rich、hover/focus/long-press |

### 16.2 基础与桌面适配控件

以下类型不是新的“官方 M3 组件类别”，而是为了 Avalonia 桌面完整性提供的适配器：

- `MdText`；
- `MdIcon`；
- `MdSurface`；
- `MdFocusRing`；
- `MdStateLayer`；
- `MdRipplePresenter`；
- `MdScrollViewer` / `MdScrollBar`；
- `MdComboBox` / `MdAutoCompleteBox`（采用 exposed dropdown menu 规则）；
- `MdNumericBox`；
- `MdWindow`；
- `MdAdaptiveLayout`。

`MdDataGrid`、`MdTreeView`、`MdPropertyGrid` 如后续实现，必须标记为 `Material-inspired desktop extension`，不得标记为官方 M3 组件。

桌面端 `MdScrollViewer` 遵循 Flutter `ScrollBehavior.dragDevices` 的默认语义：鼠标主键拖页默认关闭，滚轮、触控板和滚动条保持可用；显式启用鼠标拖页时，focusable 子控件、用户内容处理 press 或取得 pointer capture，以及 `SuppressMouseDragScrolling` 子树拥有更高优先级。

---

## 17. 组件通用视觉与交互要求

每个组件都必须具有：

1. Anatomy：容器、label、leading/trailing slot、state layer、focus ring 等。
2. Variants：官网列出的当前变体。
3. Size：官网当前 size token，不使用统一拍脑袋高度。
4. State matrix：rest、hover、focus、pressed、selected、disabled、error、loading。
5. Input matrix：mouse、touch、pen、keyboard。
6. Light/Dark：同一语义角色在两个主题下自动切换。
7. High contrast：对比度提高时仍保持层级。
8. RTL：leading/trailing 自动镜像，方向性图标按语义处理。
9. Automation：正确 ControlType、Name、Value、Selection/ExpandCollapse 等 pattern。
10. Documentation：MVVM、直接操作、资源覆盖、无障碍示例。

触摸/点击目标原则上不得小于 48×48 DIP；视觉容器可以小于命中区域。

### 17.1 `MdTextBox` 专项要求

- Filled 与 Outlined 的可视容器为 56 DIP；supporting/error row 不得计入容器高度。
- label 从 resting 到 floating 使用不参与 measure/arrange 的 150 ms render-transform 过渡；不得通过切换字号、margin、容器高度或布局行高制造动画。
- Outlined focus 的 2 DIP 描边使用非测量 overlay；基础 1 DIP outline 保持不变，获得/失去焦点前后控件 `Bounds.Height` 必须一致，Gallery 相邻内容不得跳动或闪烁。
- 必须保留 `PART_TextPresenter`、`PART_ScrollViewer`、IME preedit、selection、caret 与 password reveal 绑定链路。
- `ShowClearButton` 以内嵌 icon action 清空双向 `Text` 并返回输入焦点；`IsPassword` 启用原生 `PasswordChar` 与双向 `RevealPassword`，不得复制实现文本编辑。

### 17.2 `MdCheckBox` 专项要求

- `MdCheckBox : CheckBox`；不得覆盖原生 `CheckBox`。
- visual container 18 DIP、corner 2 DIP、outline 2 DIP、state layer 40 DIP、target 48 DIP。
- 48 DIP interactive area 必须包含 visual box；鼠标在方框上必须触发 hover/pressed，点击方框必须切换 `IsChecked`，不能只允许 label 区域切换。
- check/uncheck 使用 150 ms stroke-draw、opacity 与 scale transition；indeterminate 使用对应短 transition。
- 支持 unselected、selected、indeterminate、error、disabled 以及 hover/focus/pressed state layer。
- 必须保留原生三态、`IsChecked` 双向绑定、`ICommand`、Space 键切换及 Toggle automation pattern。

### 17.3 `MdComboBox` 专项要求

- `MdComboBox : ComboBox`，按 M3 exposed dropdown menu 规则实现 Filled/Outlined text-field anchor。
- Outlined 锚点与 `MdTextBox` 共享浮动 label/notch 几何；Label transformed visual center 必须与 outline top edge 重合，Light/Dark field background 都不得产生不透明黑块。
- attached popup 使用方形 top corners、4 DIP bottom corners、零 VerticalOffset、显式 rounded clipping 和 `SurfaceContainerLow`，必须紧贴 anchor；item 高 56 DIP，常态 CornerExtraSmall，selected 使用 CornerMedium、`TertiaryContainer` / `OnTertiaryContainer`。
- dropdown arrow 使用 20 DIP Material Symbol，不得用过大的手绘 Path。
- 必须保留 `PART_Popup`、`PART_ItemsPresenter`、`PART_EditableTextBox`、`ItemsSource`、`SelectedItem`、`SelectedValue`、键盘导航、light-dismiss 和 ExpandCollapse/Selection automation pattern。
- popup 必须匹配 anchor 最小宽度，受可用屏幕与软键盘空间约束；Android 返回键先关闭 popup，不得直接退出页面。
- 打开任一 `MdComboBox` 时必须关闭此前打开的 `MdComboBox`；协调器使用弱引用，不得造成窗口/控件泄漏，也不得破坏 Avalonia 原生 light-dismiss。
- `ComboBoxItem` 视觉只能在 `MdComboBox` popup 内局部应用，不得全局覆盖 Avalonia 原生 `ComboBoxItem`。
- Editable 模式继续使用原生 `TextPresenter`/IME 输入链路；自动完成策略由后续独立 `MdAutoCompleteBox` 提供。

### 17.4 Toggle / Icon / Split button 专项要求

- `MdToggleButton : ToggleButton`；提供 Filled、Tonal、Outlined，不创建 M3 未定义的 Text toggle variant；selected 必须同时切换颜色并执行 Round/Square shape morph。
- `MdIconButton : Button`、`MdToggleIconButton : ToggleButton`；提供 Standard/Filled/Tonal/Outlined、XS–XL、Narrow/Default/Wide、Round/Square；toggle 类型提供独立 `SelectedIcon`。
- Icon Button visual container 使用 32/40/56/96/136 DIP，icon 使用 20/24/24/32/40 DIP；XS/S 外层仍保证至少 48 DIP 命中目标。
- `MdSplitButton` 的主 segment 与 trailing segment 必须支持独立 command/parameter；高度为 32/40/56/96/136 DIP，visual segment gap 必须精确为 2 DIP，不得因外层 48 DIP target 增大可视间距；外圆角完整、连接处使用 token inner corner。
- Split 使用独立 `MdDropdownMenu`，并由 Material Symbols 显示 expand icon；`IsDropDownOpen` 默认双向绑定，打开/关闭必须有 opacity/scale/spatial transition，并遵循 light-dismiss、Esc 和 focus-return 规则。
- trailing segment selected 后只能改变颜色/图标与允许的右侧 outer shape；连接主 segment 的左上、左下角必须始终保持 4/8/12 DIP inner corner，不得执行独立 icon-toggle 的 selected shape morph。

### 17.5 Button groups 专项要求

- `MdStandardButtonGroup : ItemsControl`；XS/S/M/L/XL item spacing 分别为 18/12/8/8/8 DIP。
- `MdConnectedButtonGroup : ItemsControl`；所有尺寸使用 2 DIP spacing，首/中/尾 item 的 corner geometry 形成连续外轮廓和方形内角。
- group 只负责布局和 child shape；不得替代子按钮自己的 command、toggle state、automation 或 MVVM binding。
- 动态增删与尺寸切换后必须重新计算位置伪类/圆角；RTL 下逻辑 leading/trailing 必须正确。

### 17.6 FAB / Extended FAB / FAB Menu 专项要求

- `MdFloatingActionButton : Button`；Small/Regular/Medium/Large 为 40/56/80/96 DIP；颜色提供 Primary、PrimaryContainer、SecondaryContainer、TertiaryContainer。
- FAB resting 使用 Level 3 elevation，hover 使用 Level 4；state layer 必须覆盖整个 visual container 并裁切到当前 shape，不能只覆盖扣除 padding 后的 content box。
- `MdExtendedFloatingActionButton : Button` 采用当前 Expressive Small/Medium/Large，容器高度 56/80/96 DIP；label 与 icon 必须在 container 内垂直居中；不得把已不推荐的 baseline Extended FAB 当默认配置。
- `MdFabMenu : ItemsControl` 从任意 FAB trigger 展开 2–6 个相关 labeled actions，不与 Extended FAB trigger 搭配，并取代旧 speed dial 模式。
- FAB Menu item/close button 高 56 DIP、Full shape；item 间 4 DIP、item 到 close button 8 DIP、Level 3 elevation；`IsOpen` 默认双向绑定，打开/关闭提供可逆 expand/opacity/scale transition、键盘焦点流与 Esc 返回 trigger。默认按 Material 模式向上展开；项目扩展可通过独立 `ExpansionDirection=Up|Down` 控制 action 相对 trigger 的方向，不得借用控件在父布局中的 `VerticalAlignment` 代替展开方向。

### 17.7 Motion、Ripple 与图标字体专项要求

- `MdMotion` 提供可继承的 Expressive、Standard、Reduced、None scheme；空间属性使用 spatial spring，颜色/透明度使用 effects spring。
- 冻结的 Expressive Fast Spatial 为 damping ratio 0.6 / stiffness 800；Default Spatial 为 0.8 / 380；Effects spring 必须临界阻尼。
- 所有 Button、Toggle Button、Icon Button、Split segment、FAB 和可点击 AppBar action 必须使用 `MdRipplePresenter` 或等价实现。
- ripple 从 pointer press origin 扩张，覆盖并裁切到整个 container shape；不得只显示固定透明焦点块，不得阻断原生 Click、Command、键盘或 Automation。
- Reduced 模式取消 overshoot 并缩短 effects；None 模式不得启动装饰性 motion。
- 内嵌 Material Symbols Rounded font、codepoint map 和许可证；`MdIcon` 与 `MdSymbols` 不得依赖网络字体或 Unicode lookalike。

### 17.8 Radio、App bars 与 Badge 专项要求

- `MdRadioButton : RadioButton`；icon 20 DIP、state layer 40 DIP、target 48 DIP；保留 `GroupName`、原生命令、Space 键、Toggle automation 和 mutually-exclusive semantics。
- `MdTopAppBar` 提供 Small（64 DIP）、Medium Flexible（112 DIP）与 Large Flexible；支持 centered title、subtitle、leading/trailing slots 和 scrolled surface role。Flexible bar 带 subtitle 时分别扩展到 136/152 DIP，组件模板自身绘制并裁切完整四角 outline，Gallery action button 必须提供可操作菜单。
- Baseline medium/large 不作为默认；`MdBottomAppBar` 只用于兼容，并在 Gallery 标出替代方向。
- `MdBadge` 提供 6×6 DIP small dot，以及高 16 DIP、最小宽 16 DIP、4 DIP horizontal padding 的 labeled badge；颜色固定使用 Error/OnError 角色。
- `MdBadgedBox` 在任意 content 的 top-trailing 位置布局 badge，并保持 badge 不进入交互 hit target。

### 17.9 Carousel、Card、Chips 与 Picker 专项要求

- `MdCarousel : ListBox`；提供 MultiBrowse、Hero、CenterAligned、Uncontained，局部准备 item container，不覆盖原生 `ListBoxItem`；保留 `ItemsSource`、`ItemTemplate`、selection 与键盘导航。
- `MdCard : Button`；提供 Elevated、Filled、Outlined 和 12 DIP shape；交互模式保留 `Click`/`Command`/焦点/ripple，`IsInteractive=False` 时作为纯展示容器。
- `MdChip : ToggleButton`；Assist/Filter/Input/Suggestion 共用 scoped anatomy；Filter 的 `IsChecked` 默认双向，Input 的移除动作同时支持 `RemoveCommand` 和 `RemoveRequested`。
- `MdDatePicker : TemplatedControl`；提供 Docked/Modal、固定 42 格完整日期网格、日期范围、按当前 CultureInfo 的 `FirstDayOfWeek` 和本地化最短星期名排序、双向 `SelectedDate`/`IsOpen`、Escape 和 day-button automation。
- `MdTimePicker : TemplatedControl`；提供 Dial/Input、12/24 小时、默认 1 分钟步长、小时/分钟区鼠标滚轮、直接文本输入、Cancel/OK、双向 `SelectedTime`/`IsOpen`；Input 与 24-hour 输入框复用统一 outlined field label/notch 视觉。
- Picker 不依赖 Fluent/SimpleTheme 的原生 DatePicker/TimePicker presenter；模板与 popup surface 必须完全 scoped，核心 API 仅使用跨平台 Avalonia 类型。
- ComboBox、Dropdown、DatePicker、TimePicker 共享弱引用 popup coordinator；打开任一个时自动关闭此前 owner。所有 popup 不强制 `OverlayLayer`，而由 Avalonia 原生 popup/fallback host 承载，并使用自身裁切后的 Material surface；这可避免宿主没有可用 OverlayLayer 时出现卡死或崩溃，Light/Dark 下也不得透出平台白色尖角。

### 17.10 Dialog、Divider、Lists、Loading 与 Progress 专项要求

- `MdDialog` 与 `MdDialogHost` 提供 Basic/FullScreen；`IsOpen` 默认双向绑定并支持 XAML 声明；Host 的 `DataTemplates` 可按模型类型预声明多个对话框视图，但同一时刻只激活一个；直接操作提供 `ShowAsync(dialogOrModel)` 与 `Close(result)`；Escape、可配置 scrim-dismiss 和 awaited result 必须一致。
- `MdDivider : Control` 提供 Horizontal/Vertical、leading inset、line thickness 和 brush；默认使用 `OutlineVariant` 1 DIP。
- `MdList : ListBox` 与 `MdListItem : ListBoxItem` 提供 Standard/Segmented、单选/多选、leading/headline/supporting/trailing slots；保留 `ItemsSource`、`ItemTemplate`、selection、键盘与 automation，不覆盖原生 `ListBoxItem` 全局样式。
- `MdLoadingIndicator : Control` 使用 Expressive morphing shape，并读取继承的 Expressive/Standard/Reduced/None motion scheme；`IsActive=False` 必须停止计时器。
- `MdLinearProgressIndicator` 与 `MdCircularProgressIndicator` 提供 determinate/indeterminate 并保留 `ProgressBar` 的 Value/Minimum/Maximum 语义；active 使用 Primary，track 使用 SecondaryContainer，默认厚度 4 DIP。
- Linear progress 另提供 Flat/Wavy 形状与可配置 thickness/amplitude/wavelength；Reduced/None motion 必须减少或停用装饰性循环动画。
- 上述控件均必须具有 scoped `ControlTheme`、Light/Dark 动态资源、Gallery AXAML/C# 示例和 Headless 渲染/API 测试。

### 17.11 Menus、Navigation、Search、Sheets、Slider、Snackbar 与 Switch 专项要求

- `MdMenu : ItemsControl` 与 `MdMenuItem : Button` 使用临时 surface、标准项目高度、leading/content/trailing slots；item 保留 `Click`、`Command`、disabled 与键盘焦点语义。
- `MdNavigationBar : ListBox` 与 `MdNavigationBarItem : ListBoxItem` 面向 compact 布局的 3–5 个顶层 destination；active indicator、图标、label、badge slot 必须随 selection 更新，并保留 `ItemsSource`、`SelectedItem` 与键盘选择。
- `MdNavigationDrawer : ContentControl` 提供 Standard/Modal、left/right placement、scrim dismiss 与 Escape；`DrawerContent` 只能由单一 presenter 承载，防止同一个控件被附加到两个视觉父级。
- `MdSearchBar : TextBox` 与 `MdSearchView : ContentControl` 提供 leading/trailing action、搜索输入、展开结果 surface 与 Escape；可编辑输入必须保留 IME、selection 与 MVVM text binding。
- `MdSheetHost : ContentControl` 提供 Bottom/Left/Right、Standard/Modal、drag handle、scrim dismiss、Escape、双向 `IsOpen` 及 `Show()`/`Dismiss()`。
- `MdSlider : Slider` 保留 Avalonia 的 value/range、键盘、pointer、step 行为；默认 track 为 16 DIP、handle 为 44×4 DIP，可显示 stop indicator 与跟随 thumb 的 value indicator。
- `MdSnackbar : ContentControl` 使用 `InverseSurface`/`InverseOnSurface`/`InversePrimary`；提供 action command、dismiss、timeout、hover pause、双向 `IsOpen` 及 `Show()`/`Dismiss()`。带 action 的消息保持显示直至操作或关闭。ViewModel 通过共享 `IMdSnackbarService` 向视觉树中的 `MdSnackbarHost` 排队发送消息，不直接创建游离控件实例。
- `MdSwitch : ToggleButton` 使用完整 52×32 DIP track、selected/unselected handle 与可选状态图标；保留双向 `IsChecked`、`Command`、pointer、keyboard 和 automation。
- 上述 8 个 Gallery 页面必须各自以根级 `ScrollViewer` 支持独立预览，含交互 demo 及 AvaloniaEdit AXAML/C# 双语言示例。实际 Gallery shell 由单一 `MdScrollViewer` 持有有限 viewport，导航时解包页面预览用根 viewer；不得把两个 ScrollViewer 同时保留在视觉树中，以避免无限高度测量或滚动失效。
- Headless 回归必须验证所有模板均可渲染、直接 API 可操作、popup 模板不强制 OverlayLayer、实际 shell 中无嵌套 ScrollViewer，并以真实 wheel input 验证 extent、viewport 和 offset。

### 17.12 Tabs、Toolbars 与 Tooltips 专项要求

- `MdTabs : ListBox` 与 `MdTabItem : ListBoxItem` 保留 `ItemsSource`、`SelectedItem`、single selection 和键盘行为；提供 Primary/Secondary、fixed/scrollable、48 DIP 纯文本或 inline icon tab、72 DIP stacked icon tab、badge slot 和全容器状态层。
- Primary tab 使用短 rounded active indicator，Secondary tab 使用较宽的 2 DIP indicator；active/inactive 前景和 indicator 必须全部读取动态色彩角色，并在 Light/Dark 下保持对比。
- `MdToolbar : ItemsControl` 提供 Docked/Floating、Standard/Vibrant 与 Horizontal/Vertical；floating toolbar 使用 64 DIP container、full shape、8 DIP padding 和 elevation，docked toolbar 跨可用宽度且不使用浮动圆角。
- Toolbar 支持 leading/trailing slots 与任意 action content，子按钮继续保留 Command、ripple、keyboard、automation；Standard 使用低强调 surface container，Vibrant 使用 primary container 角色。
- `MdTooltip : ContentControl` 提供 Plain/Rich；Plain 使用 `InverseSurface`/`InverseOnSurface` 和 body-small 文本，Rich 使用 `SurfaceContainer`、12 DIP shape、Level 2 elevation、最大 312 DIP，并支持 title、supporting content 与 action slot。
- `MdTooltipHost : ContentControl` 负责 trigger、hover、focus、long-press、可选 click、Escape 与 outside dismiss；`IsOpen` 默认双向绑定，并提供 `Show()`/`Dismiss()` 直接 API。Rich tooltip 在 pointer 移入 surface 时不得被 trigger 的 hide delay 提前关闭。
- Tooltip popup 不得强制 `OverlayLayer`；必须由 Avalonia 原生 popup/fallback host 承载，以避免 Android/headless host 缺少 overlay 时卡死或崩溃。
- Tabs、Toolbars、Tooltips Gallery 页面必须各自使用根级垂直 `ScrollViewer`，提供交互演示和可复制的 AvaloniaEdit AXAML/C# 示例；Headless tests 覆盖三类控件联合渲染、selection/direct API 和 popup hosting 回归。

---

## 18. Gallery 设计

### 18.1 信息架构

Gallery 顶级栏目与 M3 官网一致采用：

```text
Home
Get started
Develop
Foundations
Styles
Components
```

本项目不需要复制官网 Blog 内容，可用 `About / Changelog` 替代。

### 18.2 大屏布局

```text
┌───────────────────────────────────────────────────────────────┐
│ Logo / Material 3 Avalonia | Top nav | Search | Motion | Theme│
├────────────────┬────────────────────────────┬─────────────────┤
│ Left nav       │ Main content               │ On this page    │
│ grouped        │ title / intro / tabs       │ section anchors │
│ collapsible    │ interactive demos          │                 │
│ active pill    │ specs / code / API         │                 │
└────────────────┴────────────────────────────┴─────────────────┘
```

要求：

- 顶栏固定；滚动时保持搜索、motion 和主题入口可用；Light/Dark/System 使用 `MdComboBox` 切换。
- 主内容必须置于全局 `ScrollViewer`，不得依赖每个页面碰巧提供自己的滚动容器。
- 左侧导航按组件类别分组，可折叠，当前项使用 M3 selected container。
- 主内容限制最大阅读宽度；演示区域可突破正文宽度。
- 右侧目录只在足够宽时显示，并跟踪当前章节。
- 组件页包含 `Overview / Guidelines / Specs / API` 页签或等价二级导航。
- 搜索覆盖控件名、别名、API、资源 token 和示例标题。

### 18.3 响应式断点

采用 M3 五档 breakpoint：

| Breakpoint | 宽度 | Gallery 行为 |
|---|---:|---|
| Compact | `< 600` | 单栏；左导航改为 modal drawer；隐藏右目录 |
| Medium | `600–839` | 单栏主内容；可展开导航 rail/drawer；隐藏右目录 |
| Expanded | `840–1199` | 左导航 + 主内容；右目录折叠 |
| Large | `1200–1599` | 左导航 + 主内容 + 右目录 |
| ExtraLarge | `>= 1600` | 三栏；增加留白，不无限拉宽正文 |

Avalonia 宽度按 DIP 判断，不按物理像素判断。

### 18.4 组件页模板

每个组件页面必须包含：

1. 标题、用途、规范状态（Expressive/Baseline/Deprecated）；
2. Hero demo；
3. 变体演示；
4. 所有交互状态；
5. Light/Dark 并排预览；
6. 自定义 seed color 预览；
7. MVVM 示例；
8. 直接 C# 示例；
9. 公共属性、事件、命令、方法；
10. Component token 表；
11. 键盘操作表；
12. Automation/无障碍说明；
13. “Do/Don’t”或已知限制；
14. 可选择、滚动且一键复制的 AXAML/C# 双页签代码块（统一使用 Gallery `CodeExample` + AvaloniaEdit 12 语法高亮组件；AvaloniaEdit 所需 Fluent resources 仅在 `CodeExample` 局部作用域加载，不进入核心库或全局应用样式）。

### 18.5 Foundation 展示页

Gallery 必须提供独立 Foundation 导航：

- `Theme resources`：展示 Light/Dark 动态 color roles、shape scale、elevation、state opacity 和 motion physics tokens；
- `Material Symbols`：使用真实 `MdIcon` 与内嵌 Material Symbols Rounded 展示 glyph name/codepoint，不得用系统 Unicode 字符冒充；每个 symbol tile 点击后复制对应 `MdSymbols.*` 常量；
- `Motion`：并排展示 Expressive、Standard、Reduced spring 轨迹，并提供 ripple、shape morph 与 reversible menu 交互；
- 两页必须随 Gallery 的 Light/Dark/System 切换实时更新；
- 组件导航必须包含 Radio buttons、App bars 和 Badges 页面及其状态/变体。

### 18.6 Theme Lab

Gallery 必须提供 Theme Lab：

- seed color picker 与十六进制输入；
- scheme variant；
- contrast level；
- Light/Dark/System；
- Standard/Expressive motion；
- Full/Reduced/None motion；
- Brand/Plain 字体选择；
- shape scale 覆盖；
- JSON 导入/导出；
- 色彩对比度诊断；
- 全组件快速预览。

---

## 19. 无障碍、国际化与输入

### 19.1 无障碍

- 使用 Avalonia AutomationPeer，优先复用原生控件已有 peer。
- 复杂控件实现 Selection、RangeValue、Toggle、ExpandCollapse、Invoke 等正确 pattern。
- 普通文本对比度目标至少 4.5:1；大文本和非文本 UI 至少 3:1。
- 焦点不得只靠颜色表达；`:focus-visible` 必须清晰。
- 验证错误通过 `DataValidationErrors` 和 HelpText 暴露给辅助技术。
- Snackbar/异步状态使用适当 LiveSetting。
- 装饰图标、ripple、阴影不出现在 Automation tree。

### 19.2 键盘

- Tab/Shift+Tab：焦点导航；
- Enter/Space：激活按钮、切换控件；
- Esc：关闭最高层可关闭 popup/dialog/sheet；
- Arrow/Home/End/PageUp/PageDown：列表、菜单、tabs、slider 按规范处理；
- 焦点返回触发弹层的控件；
- 不允许键盘焦点被 modal 外部元素获取。

### 19.3 国际化

- 所有文本由 Gallery 资源文件提供；
- 控件不内置不可替换的英文；
- 日期、时间、数字遵循 `CultureInfo`；
- 支持 M3 当前的 language script height 分类接口；
- 支持 RTL 与 logical leading/trailing。

---

## 20. 性能指标

- 常用控件状态动画在 60 Hz 下不得持续超过 16.7 ms/frame；目标支持 120 Hz 的 8.3 ms/frame。
- Composition 动画运行期间避免每帧 UI-thread 分配。
- `MdList`、菜单、导航列表、Carousel 必须支持容器回收或虚拟化。
- 主题切换不得重新创建整个视觉树。
- 字体和完整 Material Symbols 包可独立安装，避免核心包体积失控。
- Gallery 启动时只加载当前页面，示例页按需创建。
- 对 ripple、loading indicator、wavy progress 建立持续运行 10 分钟的内存泄漏测试。

---

## 21. 测试与验收

### 21.1 单元测试

- StyledProperty 默认值、coerce、双向绑定；
- Command、CanExecute、事件和直接方法；
- HCT 生成 golden vectors；
- 主题 JSON round-trip；
- 弹簧采样曲线；
- 资源键完整性和无循环引用；
- 每个公开控件可无 Fluent/SimpleTheme 独立实例化。

### 21.2 Headless 视觉测试矩阵

每个组件至少覆盖：

```text
Theme: Light / Dark
Profile: Expressive / Baseline（适用时）
State: Rest / Hover / Focus / Pressed / Selected / Disabled / Error
Scale: 1.0 / 1.25 / 2.0
Direction: LTR / RTL
Contrast: Standard / Medium / High
```

验收方式：

- 几何差异 ±1 DIP；
- 非抗锯齿区域颜色完全匹配；
- 文本区域使用感知差异阈值；
- 任何 golden image 更新必须人工审批并附来源变化说明。

### 21.3 交互测试

- 鼠标、触摸、键盘、焦点恢复；
- popup stacking、modal focus trap、Esc 关闭；
- drag/scroll 与点击冲突；
- 快速重复点击与异步 Command；
- 动画中途反向、重定向、窗口失焦和控件卸载。

### 21.4 无障碍测试

- Automation tree 快照；
- Windows Narrator、macOS VoiceOver、Linux Orca 手工冒烟测试；
- 键盘-only 全 Gallery 巡检；
- 200% 缩放和高对比度检查；
- 色盲条件下信息不只依赖颜色。

### 21.5 Gallery 视觉基准

固定窗口尺寸：

- 390×844；
- 768×1024；
- 1024×768；
- 1440×900；
- 1920×1080。

检查顶栏、左导航、正文、右目录、drawer、搜索、主题切换和滚动锚点。

### 21.6 Android 验证

- `net8.0-android` Debug/Release 编译检查；
- Android API 26 与当前目标 API 模拟器冒烟测试；
- ARM64 真机触摸、长按、拖拽、返回键和屏幕旋转；
- `MdTextBox` 的软键盘、IME composing、selection、复制粘贴与焦点切换；
- Light/Dark/System、字体缩放 1.0/1.3/2.0 和横竖屏布局；
- Activity 暂停/恢复、进程重建后的状态与主题恢复；
- trim/AOT 配置下资源字典、反射和 compiled binding 不得失效。

---

## 22. 版本与兼容性

- 使用 Semantic Versioning。
- 公共 API 变化启用 API compatibility analyzer。
- Material 规范升级与 Avalonia 升级分别记录。
- `MaterialSpecVersion` 与 NuGet 包版本分离，例如：

```text
PackageVersion: 1.2.0
MaterialSpecSnapshot: 2026-09-22
AvaloniaCompatibility: >= 12.0 < 13.0
```

- Deprecated 的 M3 变体至少保留一个 major version，并在 Gallery 标记替代方案。
- 所有 Avalonia package reference 使用中央版本管理，禁止混用 12.0/12.1 的不同包版本。

---

## 23. 实施阶段

### Phase 0：规范冻结

- 建立官方来源清单；
- 抓取 token；
- 建立参考图与组件状态矩阵；
- 确认字体与图标许可。

### Phase 1：Foundation

- Color、Typography、Shape、Spacing、Elevation、State、Motion；
- Light/Dark/System；
- seed color theme builder；
- `MdSurface`、`MdText`、`MdIcon`、state layer、ripple、focus ring。

### Phase 2：基础输入与操作（已完成）

- Button、IconButton、FAB、Checkbox、Radio、Switch、TextBox、Slider、Progress；
- 补充桌面输入适配：`MdAutoCompleteBox`、`MdNumericBox`；
- Release build、API/render/headless input 回归通过。

### Phase 3：容器与反馈（已完成）

- Card、List、Badge、Divider、Dialog、Snackbar、Tooltip、Menu、Chips；
- Chips 同时提供共享 `MdChip` 与 `MdAssistChip` / `MdFilterChip` / `MdInputChip` / `MdSuggestionChip` 官方类型；
- `MdSurface`、`MdText`、`MdStateLayer`、`MdFocusRing` 与 contained/uncontained Loading API 已纳入 scoped theme 与测试。

### Phase 4：导航与高级组件（已完成）

- AppBar、Tabs、NavigationBar/Drawer/Rail、Search、Sheets、Carousel、Pickers、Toolbar、ButtonGroup、SplitButton、FAB Menu、Loading Indicator；
- NavigationBar 默认采用当前 M3 Expressive Flexible，并保留 Baseline 与 horizontal/stacked item；
- 补充 `MdCarouselItem`、`MdFabMenuItem`、picker dialog 类型、`MdAdaptiveLayout`、`MdScrollViewer` / `MdScrollBar` 与直接 Show/Dismiss API；
- Gallery 演示、MVVM/直接操作示例及 Phase 2–4 联合渲染/交互回归通过。

### Phase 5：Gallery（已完成）

- 官网式 shell 已实现五档 breakpoint：Compact `<600`、Medium `600–839`、Expanded `840–1199`、Large `1200–1599`、Extra-large `>=1600`；compact/medium 使用 modal navigation，Large/Extra-large 显示三栏且正文限制阅读宽度；
- 保持 shell 单一有限 viewport 滚动模型，并提供当前页面标题自动生成的可跳转右侧目录；
- 35 个全组件页面、Desktop adapters、Material Symbols、Motion 与 AXAML/C# 双页签代码编辑器已接入；
- Gallery 顶部搜索已建立组件标题/关键字索引，可实时筛选并在提交时导航；
- Theme Lab 已实现任意 seed color、六种 scheme variant、三档 contrast、Light/Dark/System、motion、Brand/Plain、shape scale、49 role 预览、对比度诊断以及 JSON 文本/文件导入导出；
- Headless 覆盖五档响应式行为、搜索索引、页面渲染与真实滚动。

### Phase 6：硬化与发布（仓库内可执行项已完成）

- 已增加 HCT golden vectors、主题 JSON round-trip、49 role/contrast、LTR/RTL、Light/Dark、三档 contrast、五种参考宽度渲染矩阵；
- `MdList` / `MdCarousel` 改用 `VirtualizingStackPanel` 并增加大型数据集有界实现测试；重复主题替换和 attach/detach 生命周期纳入测试；
- 原生基类 AutomationPeer 继续复用；Loading 使用 ProgressBar automation，Snackbar 使用 polite LiveSetting，ripple/state/focus 装饰层退出 automation control/content view；CommunityToolkit.Mvvm 双向绑定和 RelayCommand 已自动验证；
- 已加入 `gallery/Md3.Avalonia.Gallery.Android` single-view host、Gallery Android view 与 Android 生命周期分支；该工程刻意不加入 desktop solution；
- NuGet metadata、XML docs、API 入口、Apache-2.0、第三方声明、CHANGELOG、兼容策略和发布验证清单已补齐；
- 当前环境无法替代 Android ARM64 真机、Narrator、VoiceOver、Orca 人工验收，因此这些项目保留在 `docs/RELEASE_VALIDATION.md` 等待外部签署，不伪造完成结论。

---

## 24. 主要风险与处理

| 风险 | 影响 | 处理 |
|---|---|---|
| M3 官网持续更新 | 数值和组件变体漂移 | 规范快照、来源 SHA、显式升级流程 |
| Avalonia 不支持变量字体 | 字体/图标轴不能连续变化 | 构建时固定实例 + Geometry fallback |
| Avalonia 无直接 M3 spring API | 动画不够一致 | 自定义 spring solver + Composition keyframes |
| 官方不同平台令牌存在差异 | 无法判断哪个值“最准” | 按来源优先级与 profile 固定；记录差异 |
| 全量 Material Symbols 体积大 | 包体积和启动时间 | 独立图标包、子集生成、延迟加载 |
| 桌面与触摸交互差异 | 状态/命中区不一致 | 输入矩阵与 48 DIP target；桌面 focus/hover 明确化 |
| 全局资源替换闪烁 | 主题体验差 | Dispatcher 批量提交与 DynamicResource |
| 自定义控件无障碍退化 | 屏幕阅读器不可用 | 继承原生行为、AutomationPeer 测试、真机巡检 |

---

## 25. 首版完成定义（Definition of Done）

只有同时满足以下条件，才能声明“Material Design 3 for Avalonia 12 首版完成”：

- [x] 官方 M3 当前组件目录中的组件均有对应 `Md*` 类型，或有明确的平台不适用说明；
- [x] 无任何全局 Avalonia 原生控件样式覆盖；
- [x] Gallery 不直接使用 Avalonia 原生交互控件冒充 Material 控件；
- [x] 完整 Light/Dark/System 和任意 seed color 生效；
- [x] 颜色、字体、形状、间距、阴影、状态和 motion 资源字典齐全；
- [x] Material Symbols 字体图标可直接使用；完整官方 variable TTF 与真实 Lite 子集已入库并随 Icons 包分发，`scripts/verify-fonts.py` 以固定 upstream commit、SHA-256、variable tables 与 glyph 数量门禁校验；
- [x] CommunityToolkit.Mvvm 示例和直接操作示例均通过；
- [x] 所有控件通过可自动执行的基础键盘与 Automation 测试；屏幕阅读器人工巡检见外部签署项；
- [x] 自动视觉渲染矩阵无未审批差异；人工 OS/设备矩阵仍按发布记录签署；
- [x] Gallery 在五档 breakpoint 下布局正确；
- [ ] 核心控件库已在 Android 模拟器与 ARM64 真机运行，触摸、软键盘、返回键、旋转和主题切换通过；**已提供 host 与自动化可验证边界，当前无 Android workload/设备，不伪造签署。**
- [x] Workspace 产物扫描通过，不包含 `bin/obj/TestResults/artifacts`、DLL/PDB、NuGet 包或 APK/AAB；每次最终交付前重新执行；
- [x] NuGet metadata、第三方许可、API 文档和变更记录齐全（发布包仅用于临时验证，清理后不进入 workspace）。

---

## 26. 参考资料

### Material Design 3

- [Material Design 3 首页](https://m3.material.io/)
- [M3 Components](https://m3.material.io/components)
- [Carousel specs](https://m3.material.io/components/carousel/specs)
- [Cards specs](https://m3.material.io/components/cards/specs)
- [Chips specs](https://m3.material.io/components/chips/specs)
- [Date pickers specs](https://m3.material.io/components/date-pickers/specs)
- [Time pickers specs](https://m3.material.io/components/time-pickers/specs)
- [Menus specs](https://m3.material.io/components/menus/specs)
- [Navigation bar specs](https://m3.material.io/components/navigation-bar/specs)
- [Navigation drawer specs](https://m3.material.io/components/navigation-drawer/specs)
- [Search specs](https://m3.material.io/components/search/specs)
- [Bottom sheets specs](https://m3.material.io/components/bottom-sheets/specs)
- [Side sheets specs](https://m3.material.io/components/side-sheets/specs)
- [Sliders specs](https://m3.material.io/components/sliders/specs)
- [Snackbar specs](https://m3.material.io/components/snackbar/specs)
- [Switch specs](https://m3.material.io/components/switch/specs)
- [Tabs specs](https://m3.material.io/components/tabs/specs)
- [Toolbars specs](https://m3.material.io/components/toolbars/specs)
- [Tooltips specs](https://m3.material.io/components/tooltips/specs)
- [Design tokens](https://m3.material.io/foundations/design-tokens)
- [Color roles](https://m3.material.io/styles/color/roles)
- [Typography](https://m3.material.io/styles/typography/overview)
- [Type scale & tokens](https://m3.material.io/styles/typography/type-scale-tokens)
- [Shape corner radius scale](https://m3.material.io/styles/shape/corner-radius-scale)
- [Spacing](https://m3.material.io/styles/spacing/overview)
- [Elevation](https://m3.material.io/styles/elevation/applying-elevation)
- [Icons](https://m3.material.io/styles/icons/overview)
- [Motion physics system](https://m3.material.io/styles/motion/overview/how-it-works)
- [Breakpoints](https://m3.material.io/foundations/layout/breakpoints/overview)
- [Material Theme Builder](https://m3.material.io/theme-builder/)

### 官方实现参考

- [AndroidX Material 3 TypeScaleTokens](https://github.com/androidx/androidx/blob/androidx-main/compose/material3/material3/src/commonMain/kotlin/androidx/compose/material3/tokens/TypeScaleTokens.kt)
- [AndroidX ExpressiveMotionTokens](https://github.com/androidx/androidx/blob/androidx-main/compose/material3/material3/src/commonMain/kotlin/androidx/compose/material3/tokens/ExpressiveMotionTokens.kt)
- [AndroidX StandardMotionTokens](https://github.com/androidx/androidx/blob/androidx-main/compose/material3/material3/src/commonMain/kotlin/androidx/compose/material3/tokens/StandardMotionTokens.kt)
- [Material Components Android Motion](https://github.com/material-components/material-components-android/blob/master/docs/theming/Motion.md)
- [Material Symbols](https://github.com/google/material-design-icons)

### Avalonia 12

- [Avalonia 12 breaking changes](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes)
- [Creating custom controls](https://docs.avaloniaui.net/docs/custom-controls/)
- [Custom control library](https://docs.avaloniaui.net/docs/custom-controls/custom-control-library)
- [Defining properties](https://docs.avaloniaui.net/docs/custom-controls/defining-properties)
- [Control themes](https://docs.avaloniaui.net/docs/basics/user-interface/styling/control-themes)
- [Resources](https://docs.avaloniaui.net/docs/app-development/resources)
- [Theme variants](https://docs.avaloniaui.net/docs/guides/styles-and-resources/how-to-use-theme-variants)
- [Animations](https://docs.avaloniaui.net/docs/graphics-animation/animations)
- [Composition animations](https://docs.avaloniaui.net/docs/graphics-animation/composition-animations)
- [Custom fonts](https://docs.avaloniaui.net/docs/styling/custom-fonts)
- [Accessibility](https://docs.avaloniaui.net/docs/app-development/accessibility)

### MVVM

- [CommunityToolkit.Mvvm source generators](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/generators/overview)
- [RelayCommand generator](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/generators/relaycommand)

---

## 27. 待确认项

以下项目不阻塞本规范，但应在 Phase 0 结束前确认：

1. 最终 NuGet/CLR namespace 名称是否使用 `Md3.Avalonia`；
2. iOS/WASM 是否在后续版本提升为 Tier 1；
3. Gallery 默认语言为中文、英文还是双语；
4. 是否同时发布 Baseline profile，还是只保留必要的 deprecated 兼容项；
5. 字体包是否允许包含 Noto Sans SC 完整字库，或改为可选语言包；
6. Material Symbols 发布完整集还是默认子集 + 生成器；
7. Theme JSON 是否需要兼容 Material Theme Builder 的导出格式。
