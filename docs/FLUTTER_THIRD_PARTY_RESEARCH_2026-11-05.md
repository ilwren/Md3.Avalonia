# Flutter 第三方控件 —— 收录标准与候选清单调研（2026-11-05）

本文件回答一个问题：**当前 pub.dev 上的第三方 Flutter 控件包中，哪些符合 Md3.Avalonia 的收录标准，值得进入行为调研与 clean-room 自研排期？** 结论先行，随后给出标准来源、已收录先例与候选数据。

数据日期 **2026-11-05**；likes 与许可证取自 pub.dev 当前页面（likes 为 pub.dev 的「likes」计数，随时间增长，接入前须按 §6 复核固定版本）。本文只做文档级调研：**不引入任何 Dart 源码、资产、截图、包名或视觉外观**，与 `THIRD-PARTY-NOTICES.md` 的 “Documentation-only ecosystem research” 约定一致。

**2026-11-06 复核：** 发现 Devolutions 有分组列表控件，`grouped_list` 自研排期暂停并先评估 vendor，粘性组头差异及发布/AOT 状态待核实。详见 `docs/WHEEL_PICKER_RESEARCH_2026-11-06.md`。跨列表拖拽也发现 Avalonia Sortable，需先评估 vendor；2.4.1 依赖 12.1.3 与本仓库固定版本不同，2.3.1 需实测。`slide_to_act` 的独立子集已进入实现；详见 `docs/SLIDE_CONFIRM_RESEARCH_2026-11-06.md`。下表是当日候选快照，不代表当前排期。

---

## 1. 结论速览

- **已按此标准收录 9 个 Flutter 第三方包**（表 2），它们以「冻结公开文档 → 独立行为需求 → clean-room 自研」的方式形成了 `Md3.Avalonia.Extra` 中的对应控件，并登记在 `THIRD-PARTY-NOTICES.md`、`docs/TIMELINE_SLIDABLE_RESEARCH_2026-10-29.md` 与 `docs/API.md`。
- **新候选 A 档 10 个**（表 3）：全部满足收录标准，且填补当前控件目录的真实空白——`auto_size_text`、`smooth_page_indicator`、`confetti`、`grouped_list`、`readmore`、`marquee`、`numberpicker`、`drag_and_drop_lists`、`emoji_picker_flutter`、`audio_video_progress_bar`。
- **B 档 5 个**（表 4）：模式成立但与现有控件部分重叠、热度偏低或需先做差距分析——`wolt_modal_sheet`、`like_button`、`animated_flip_counter`、`crop_your_image`、`slide_to_act`。
- **明确不收**（§5）：状态管理/路由/存储/平台插件类（非控件）、整套替代设计语言、已被自研控件覆盖的模式，以及 Avalonia 生态已有第三方控件、应走 12 个 vendor 主题包路线的对象。

---

## 2. 收录标准（自仓库文档提炼）

「收录」在本仓库有三条路径，Flutter 第三方包只对应其中一条：

| 路径 | 对象 | 去向 | 依据 |
|---|---|---|---|
| 官方 Material 3 组件 | m3.material.io / AndroidX / Flutter material 库 | `Md3.Avalonia` 核心包 | `README.md`、`spec-snapshot/manifest.json` |
| **Flutter 第三方生态模式** | **pub.dev 单用途控件包** | **`Md3.Avalonia.Extra` 自研控件（clean-room）** | **`THIRD-PARTY-NOTICES.md`、`docs/TIMELINE_SLIDABLE_RESEARCH_2026-10-29.md`** |
| Avalonia 第三方控件 | Avalonia 生态控件库 | 12 个 opt-in vendor 主题包 | `docs/THIRD_PARTY_THEME_INTEGRATION_RESEARCH.md` |

一个 Flutter 第三方控件要能被本仓库收录（作为 Extra 自研的行为参考），至少要过六关：

1. **模式归类正确**。属于「可选生态模式」，而非官方 Material 组件目录（`docs/FLUTTER_PARITY_STATUS.md`：「`Md3.Avalonia.Extra` 是可选扩展包，不属于官方 Material 组件目录」；`docs/API.md`：「optional ecosystem patterns, not as claims of official Material 3 parity」）。Flutter 一等公民才有的 widget（`Hero`、`Dismissible`、`Stepper` 等）走 parity 路径，不算第三方收录。
2. **单一用途且热度较高**。优先「一个包、一个交互模式」的对象，热度是近似门槛（`docs/THIRD_PARTY_THEME_INTEGRATION_RESEARCH.md` §2 的取向；该文针对 Avalonia 控件，但选型口味一致）。本表以 pub.dev likes ≈ 250 为软下限，低于此值除非模式独特否则排后。
3. **许可证可登记**。MIT / Apache-2.0 / BSD 系，能写进 `THIRD-PARTY-NOTICES.md`（既有 9 个先例全部是 MIT 或 BSD）。
4. **可走 clean-room 流程**。只读冻结的公开文档提炼独立行为需求，不复制源码/资产/命名/视觉；研究记录成文（如 `docs/TIMELINE_SLIDABLE_RESEARCH_2026-10-29.md` 的三段式），冻结版本与处置记入 `spec-snapshot/manifest.json`。
5. **优先级规则**：若 Avalonia 生态已有现成第三方控件覆盖同一模式，改走 vendor 主题包路线（五关见 `docs/THIRD_PARTY_THEME_INTEGRATION_RESEARCH.md` §2）；「有第三方控件就做 Material 美化，没有才自研」（`docs/RELEASE_NOTES_0.5.0-preview.1.md`）。`MdWaveform` 即先例：检索无第三方 Avalonia 波形控件后才自研。
6. **实现约束**：独立 `Md*` CLR 类型 + scoped `ControlTheme`，颜色/字体/形状/动效全部走 token 与 `DynamicResource`，不全局覆盖原生控件，`IsTrimmable`/`IsAotCompatible`，Gallery 页与中英双语文档齐备（`README.md` Design principles）。

---

## 3. 已收录的 Flutter 第三方包（先例）

以下 9 个包是上述标准已经落地的证据；除注明者外均登记在 `THIRD-PARTY-NOTICES.md` 的 “Documentation-only ecosystem research” 表：

| 包快照 | 许可 | 形成的控件 | 吸收的行为 | 登记位置 |
|---|---|---|---|---|
| GetWidget 7.0.2 | MIT | `MdAvatar` / `MdAvatarGroup` | 头像回退与叠放 | THIRD-PARTY-NOTICES |
| flutter_rating_bar 4.0.1 | MIT | `MdRating` | 小数/只读评分 | THIRD-PARTY-NOTICES |
| shadcn_ui 0.57.0 | MIT | `MdBreadcrumb` | 可组合面包屑与条目调用（仅单点行为） | THIRD-PARTY-NOTICES |
| flutter_pinput 1.0.3 | MIT | `MdPinInput` | 定长 PIN/OTP、粘贴、掩码、完成态 | THIRD-PARTY-NOTICES |
| animated_tree_view 2.3.0 | MIT | `MdTreeView` | 嵌套树节点、展开、缩进、RTL | THIRD-PARTY-NOTICES |
| chips_input_autocomplete 1.2.2 | MIT | `MdTagInput` | 动态可删标签、建议、上限与校验 | THIRD-PARTY-NOTICES |
| timeline_tile | MIT | `MdTimeline` | 单轴双向、before/after 连线、首尾终止、可替换标记 | docs/TIMELINE_SLIDABLE_RESEARCH_2026-10-29.md |
| flutter_slidable | MIT | `MdSlidableItem` | 语义 start/end 面板、阈值/速度 settle、四种 motion、分组协调 | docs/TIMELINE_SLIDABLE_RESEARCH_2026-10-29.md |
| audio_waveforms | MIT | `MdWaveform` | 全圆角柱帽（`StrokeCap.round`）与 2dp 播放头默认值 | docs/API.md |

另有若干模式已由自研控件覆盖、但未登记第三方参考（不构成行为参考先例）：`MdSkeleton`（shimmer/skeletons 类占位）、`MdSpinKit`（flutter_spinkit 类加载动效）、`MdBeforeAfter`（before_after 类对比）、`MdAnimatedText`（animated_text_kit 类文本动效）、`MdStaggeredPanel`（staggered 入场）、`MdPagedItemsView`（infinite_scroll_pagination 类分页）。

---

## 4. 新候选（数据 2026-11-05，pub.dev）

### 4.1 A 档：符合全部标准，填补目录空白

| 包 | likes | 许可 | 模式 → 建议形态 | 为何符合 |
|---|---|---|---|---|
| auto_size_text 3.0.0 | 5.1k | MIT（Flutter Favorite） | 约束内自动缩放字号、`group` 联动 → `MdAutoSizeText`（Foundations，`MdText` 家族） | 目录空白；核心是测量-缩放算法，可干净提炼行为需求 |
| smooth_page_indicator 3.0.0 | 4.1k | MIT | 轮播页指示器（worm/expanding/jumping/scrolling 等效果）→ `MdPageIndicator`（与 `MdCarousel` 配套） | `MdPagination` 的 Dots 模式只覆盖分页圆点，轮播指示器动画是空白 |
| confetti 0.8.0 | 1.65k | MIT | 庆祝彩纸粒子（方向/重力/阻力/自定义粒子路径）→ `MdConfetti`（Motion and visual extras） | 与 `MdSpinKit` 同类的「非 Material 装饰动效」先例；reduced-motion 边界可直接套用现有动效方案 |
| grouped_list 6.0.0 | 1.62k | MIT | 分组列表 + 粘性/浮动组头 → `MdGroupedList`（Collections and layout） | `MdList`/`MdPagedItemsView` 无分组粘性组头；chat 反向列表行为也有独立价值 |
| readmore 3.0.0 | 1.15k | MIT | 按行/按字符折叠展开 + URL/话题/@ 注解 → `MdExpandableText`（Content） | 目录空白；TrimMode/注解 span 是纯行为契约 |
| marquee 2.3.0 | 1.07k | MIT | 无限滚动文本（方向/时长/曲线/轮次停顿）→ `MdMarquee`（Motion and visual extras） | 目录空白；Reduced/None 动效方案下的静止回退有现成纪律 |
| numberpicker 2.1.2 | 688 | BSD-2-Clause | 滚轮数字/小数选择 → `MdWheelPicker`（Pickers） | `MdTimeDial` 是钟面，滚轮选择器是空白；也是 `MdDatePicker` 内部可复用的部件 |
| drag_and_drop_lists 0.4.2 | 457 | BSD-3-Clause | 两级列表跨列表拖拽（看板式）→ `MdDragAndDropBoard`（Lists and interaction patterns） | `MdReorderableList` 仅覆盖单列表；跨列表 + 列表自身重排是空白 |
| emoji_picker_flutter 4.5.4 | 662 | MIT（2026-11-06 固定快照复核） | 表情选择器（分类/搜索/肤色/最近使用）→ `MdEmojiPicker`（Selection and transfer） | `MdColorPicker` 同族的「选择器」空白；需自备 emoji 数据集与字体策略 |
| audio_video_progress_bar 2.0.3 | 445 | MIT | 媒体进度条（已缓冲段、seek、时间标签位置）→ `MdMediaProgressBar`（与 `MdWaveform` 同属媒体面） | 目录空白；与 `MdWaveform` 的播放头/scrub 纪律天然衔接 |

### 4.2 B 档：模式成立，先做差距分析或排后

| 包 | likes | 许可 | 说明 |
|---|---|---|---|
| wolt_modal_sheet 0.12.0 | 1.4k | MIT | 多页模态 sheet + 页间动效 + 滚动联动。与 `MdSheetHost`/`MdDraggableScrollableSheet` 部分重叠；更适合作为行为参考补强现有 sheet（多页导航、`Loading`/`Error` 页面形态）而非新控件 |
| like_button 2.1.0 | 1.42k | MIT | Twitter 心形爆裂 + 计数动画。可作 `MdIconButton` 的状态动效扩展或独立 `MdLikeButton`；价值集中在动画配方，形态归属待定 |
| animated_flip_counter 0.3.4 | 653 | MIT | 数字翻牌过渡（小数/负数/前后缀）。可并入 `MdAnimatedText` 家族作 `MdAnimatedNumber`，而非独立控件 |
| crop_your_image 2.0.0 | 564 | Apache-2.0 | 纯 widget 图片裁剪（视口/形状/宽高比）。目录空白，但交互面大（手势、缩放、输出），与 `MdSignaturePad` 同属画布族，需先出 research 文档 |
| slide_to_act 2.0.2 | 271 | MIT | 滑动确认（阈值、回弹、键盘替代操作）。模式清晰且与 `MdDismissible` 的 settle 纪律相通，但热度低于软下限，排后 |

### 4.3 候选对照速查

A 档相对现有目录的最近邻（避免重复收录的自查）：

| 候选 | 目录最近邻 | 差异 |
|---|---|---|
| `MdAutoSizeText` | `MdText` | 无缩放测量逻辑 |
| `MdPageIndicator` | `MdPagination`（Dots）、`MdCarousel` | 无轮播指示动画效果 |
| `MdConfetti` | `MdSpinKit` | 粒子庆祝 vs 加载 |
| `MdGroupedList` | `MdList` | 无分组/粘性组头 |
| `MdExpandableText` | `MdText` | 无折叠/展开与注解 |
| `MdMarquee` | `MdAnimatedText` | 无持续滚动 |
| `MdWheelPicker` | `MdTimeDial`、`MdDatePicker` | 无通用滚轮值选择 |
| `MdDragAndDropBoard` | `MdReorderableList` | 无跨列表拖拽 |
| `MdEmojiPicker` | `MdColorPicker` | 不同数据域 |
| `MdMediaProgressBar` | `MdWaveform`、`MdSlider` | 无缓冲段/时间标签语义 |

---

## 5. 不符合 / 不建议收录

| 类别 | 例子 | 原因 |
|---|---|---|
| 非控件 | get、provider、shared_preferences、dio、flutter_bloc 等 | 状态管理/路由/存储/网络不属于 UI 控件库范畴 |
| 平台插件 | image_picker、file_picker、mobile_scanner、video_player | 平台能力而非控件；文件浏览已由 `MdFileSystemPicker` 自研覆盖 |
| 整套替代设计语言 | neumorphism、glassmorphism、convex_bottom_bar 等替代导航样式 | 与 M3 令牌体系正面冲突（`docs/THIRD_PARTY_THEME_INTEGRATION_RESEARCH.md` §1「明确不要接」的同款理由）；只允许像 shadcn_ui 那样做单点行为参考 |
| 已被自研覆盖 | shimmer、flutter_spinkit、before_after、carousel_slider、badges、fluttertoast、flutter_staggered_grid_view、sliding sheet 类、typeahead 类 | 模式已收录（见 §3 末段与核心目录），再按第三方包登记无增量 |
| Avalonia 已有第三方控件 | Lottie（Avalonia.Labs）、SVG（Svg.Skia）、QR（Avalonia.Labs Qr）、缩放平移（PanAndZoom）、代码/十六进制编辑器 | 按优先级规则走 vendor 主题包路线（五关），不是 Extra 自研 |

---

## 6. 接入流程（若启动某项）

沿用既有纪律，任何 A/B 档候选的落地都应：

1. **冻结快照**：固定包版本，公开文档快照与处置记入 `spec-snapshot/manifest.json`（与现有 flutter 条目并列，注明「documentation-only」）。
2. **行为需求成文**：写 research 文档（参照 `docs/TIMELINE_SLIDABLE_RESEARCH_2026-10-29.md` 三段式：相似 Avalonia 控件 → 本库设计复审 → Flutter 等价物采纳的概念），明确采纳/不采纳清单。
3. **clean-room 实现**：`Md3.Avalonia.Extra` 独立 `Md*` 类型 + scoped `ControlTheme`，token 驱动，motion-scheme 感知（Reduced/None 回退），AOT/裁剪通过。
4. **登记与边界声明**：`THIRD-PARTY-NOTICES.md` 增加条目；`docs/FLUTTER_PARITY_STATUS.md` 增加行，声明实现子集与已知边界（`[MdExperimental]` 不作为 parity 证据）。
5. **Gallery 与双语**：Gallery 演示页、API XML 注释与中英文档同步；`scripts/check-api-doc-coverage.py` 门禁通过。

> 本文是候选调研，不构成排期承诺；项目不发布整体 parity 百分比，任何行为对齐声明只限定到具体 API 与已测试情形。
