using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Md3.Avalonia.Controls;
using Md3.Avalonia.Extra.Controls;

namespace Md3.Avalonia.Gallery;

internal static class GalleryLocalization
{
    private sealed class Original(string value) { public string Value { get; } = value; }
    private static readonly ConditionalWeakTable<object, Original> Originals = new();
    private static readonly ConditionalWeakTable<object, Original> LabelOriginals = new();
    private static readonly ConditionalWeakTable<object, Original> PlaceholderOriginals = new();
    private static readonly IReadOnlyDictionary<string, string> Zh = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Get started"]="开始使用", ["Develop"]="开发", ["Foundations"]="基础", ["Styles"]="样式", ["Components"]="组件",
        ["Avalonia component gallery"]="Avalonia 组件库", ["Headings are generated from the active page. Activate one to move it into view."]="目录根据当前页面的标题生成；激活一项即可将对应内容滚动到视图中。",
        ["Search components"]="搜索组件", ["Language"]="语言", ["Theme"]="主题", ["Light"]="浅色", ["Dark"]="深色", ["System"]="跟随系统",
        ["COMPONENTS"]="组件", ["FOUNDATIONS"]="基础", ["Components overview"]="组件概览", ["SAMPLES & APPS"]="示例应用", ["LIBRARY EXTENSIONS"]="库扩展", ["ALL COMPONENTS"]="全部组件", ["On this page"]="本页内容", ["Overview"]="概览", ["Examples"]="示例", ["Usage"]="用法", ["API"]="API",
        ["Android settings"]="Android 设置", ["Clock app"]="时钟应用", ["Tasks & todo"]="任务待办", ["Settings cards"]="设置卡片", ["Breadcrumbs"]="面包屑导航",
        ["Components are interactive building blocks for creating a user interface. They can be organized into categories based on their purpose: Action, containment, communication, navigation, selection, and text input."]="组件是构建用户界面的交互式基础单元。它们可以按用途组织为操作、容器、沟通、导航、选择和文本输入等类别。",
        ["Action"]="操作", ["Containment"]="容器", ["Communication"]="沟通", ["Navigation"]="导航", ["Selection"]="选择", ["Text input"]="文本输入",
        ["Controls that start or expose an action."]="用于启动或呈现操作的控件。", ["Surfaces that group information and related actions."]="对信息及其相关操作进行分组的表面。", ["Status, progress, prompts, and transient messages."]="用于状态、进度、提示和临时消息。", ["Components for moving through destinations and views."]="用于在目的地和视图之间移动的组件。", ["Controls for choosing values, dates, and options."]="用于选择值、日期和选项的控件。", ["Fields and temporary surfaces for entering or choosing data."]="用于输入或选择数据的字段与临时表面。",
        ["Buttons"]="按钮", ["Icon buttons"]="图标按钮", ["FABs"]="浮动操作按钮", ["App bars"]="应用栏", ["Badges"]="徽标", ["Text fields"]="文本字段",
        ["Checkbox"]="复选框", ["Radio buttons"]="单选按钮", ["Combo box"]="组合框", ["Carousel"]="轮播", ["Cards"]="卡片", ["Chips"]="标签块",
        ["Date & time pickers"]="日期和时间选择器", ["Date and time pickers"]="日期和时间选择器", ["Color picker"]="颜色选择器", ["Dialogs"]="对话框", ["Divider"]="分隔线", ["Lists"]="列表", ["Loading"]="加载", ["Progress"]="进度",
        ["Menus"]="菜单", ["Navigation bar"]="导航栏", ["Navigation drawer"]="导航抽屉", ["Navigation rail & adaptive"]="导航轨道与自适应", ["Search"]="搜索",
        ["Sheets"]="面板", ["Slider"]="滑块", ["Segmented & range"]="分段按钮与范围", ["Snackbar"]="消息条", ["Switch"]="开关", ["Tabs"]="标签页",
        ["Toolbars"]="工具栏", ["Tooltips"]="工具提示", ["Flutter parity"]="Flutter 组件补全", ["Desktop adapters"]="桌面适配控件", ["Theme resources"]="主题资源", ["Theme Lab"]="主题实验室", ["Material Symbols"]="Material 图标", ["Motion"]="动效",
        ["Theme controls"]="主题控制", ["Seed color"]="种子颜色", ["Scheme variant"]="配色方案变体", ["Contrast"]="对比度", ["Theme mode"]="主题模式", ["Typography"]="字体排印", ["Shape"]="形状",
        ["Apply theme"]="应用主题", ["Reset"]="重置", ["Copy JSON"]="复制 JSON", ["Export file"]="导出文件", ["Import file"]="导入文件", ["Apply JSON"]="应用 JSON",
        ["Generated color roles"]="生成的颜色角色", ["Contrast diagnostics"]="对比度诊断", ["Cross-component preview"]="跨组件预览", ["Theme JSON"]="主题 JSON", ["Portable configuration"]="可移植配置",
        ["Ready. Changes are applied application-wide through DynamicResource."]="就绪。更改通过 DynamicResource 应用于整个应用。",
        ["WCAG relative-luminance checks for each key on-color and its container."]="检查各关键前景色与容器的 WCAG 相对亮度对比度。",
        ["Live component surface"]="实时组件表面", ["Dynamic color"]="动态颜色", ["Text field"]="文本字段", ["Assist"]="辅助", ["Selected filter"]="已选筛选", ["Input"]="输入", ["Filled"]="填充", ["Tonal"]="色调", ["Outlined"]="描边", ["Toggle"]="切换", ["Radio"]="单选", ["Material 3 · Avalonia"]="Material 3 · Avalonia",
        ["Primary tabs"]="主标签页", ["Secondary tabs"]="次级标签页", ["Scrollable and inline icon"]="可滚动与内联图标", ["Surfaces"]="表面",
        ["Floating · standard"]="浮动 · 标准", ["Floating · vibrant with primary action"]="浮动 · 高强调与主要操作", ["Docked"]="停靠", ["Vertical floating"]="垂直浮动",
        ["Hover, focus, or long-press"]="悬停、聚焦或长按", ["Copy"]="复制", ["Selectable preview"]="可选择预览",
        ["Navigation rail"]="导航轨道", ["Adaptive navigation suite"]="自适应导航套件", ["Material scaffold"]="Material 页面骨架",
        ["Single selection"]="单选", ["Multiple selection"]="多选", ["Range slider"]="范围滑块", ["Date range picker"]="日期范围选择器", ["Tab view"]="标签内容视图",
        ["Menu anchor and submenu"]="菜单锚点与子菜单", ["Start date"]="开始日期", ["End date"]="结束日期",
        ["Autocomplete and numeric input"]="自动完成与数值输入", ["Semantic surfaces and type scale"]="语义表面与字体层级", ["Responsive content"]="响应式内容", ["Material scrolling surface"]="Material 滚动表面",
        ["Material Symbols font is invalid"]="Material Symbols 字体无效",
        ["The Icons package includes the official font. Reinstall the package or run scripts/verify-fonts.py if this diagnostic appears; invalid glyphs remain hidden instead of using a look-alike fallback."]="Icons 包已包含官方字体；若出现此诊断，请重新安装包或运行 scripts/verify-fonts.py。无效字形会保持隐藏，不使用仿制回退符号。",
        ["Material controls use scoped themes and cross-platform Avalonia APIs."]="Material 控件使用局部主题与跨平台 Avalonia API。",

        // Ecosystem Waves A–F
        ["Flutter ecosystem"]="Flutter 生态控件", ["Wave A · Overlay, commands and density"]="Wave A · 浮层、命令与密度",
        ["Open one transient surface, then scroll to verify it dismisses."]="打开一个临时浮层，然后滚动以验证其自动关闭。", ["Compact density is scoped to this information row only."]="紧凑密度仅应用于此信息行。",
        ["Wave B · Collections and adaptive data"]="Wave B · 集合与自适应数据", ["Swipe this row horizontally"]="水平滑动此行", ["Ctrl+Left, Ctrl+Right and Escape also work"]="也可使用 Ctrl+左、Ctrl+右和 Escape",
        ["Virtualized rows, sortable columns, editable score cells."]="虚拟化行、可排序列和可编辑分数单元格。", ["Project board · responsive masonry cards"]="项目看板 · 响应式瀑布流卡片",
        ["Each uneven card is a real task target. Activate one to open its project context; the panel demonstrates dense dashboard reflow rather than decorative blocks."]="每张不等高卡片都是真实任务入口。激活后打开项目上下文；该面板展示的是高密度看板重排，而非装饰色块。",
        ["Design review"]="设计评审", ["12 comments · open discussion"]="12 条评论 · 打开讨论", ["Android · ready"]="Android · 就绪", ["Accessibility audit"]="无障碍审计",
        ["Keyboard, touch target, contrast and semantics checks. Activate to inspect the checklist."]="检查键盘、触摸目标、对比度和语义。激活以查看清单。", ["Localization · 2 locales"]="本地化 · 2 种语言", ["Release candidate · package audit pending"]="候选版本 · 等待包审计", ["No project card opened."]="尚未打开项目卡片。",
        ["Load next page"]="加载下一页", ["Refresh pages"]="刷新分页", ["No page loaded. Load next page to append four records."]="尚未加载页面。加载下一页将追加四条记录。",
        ["Wave C · Selection, calendar and feedback"]="Wave C · 选择、日历与反馈", ["Type a state name, then choose a visible result."]="输入州名，然后选择可见结果。", ["Select a calendar date."]="请选择日历日期。",
        ["Open the hierarchy and choose a leaf destination."]="打开层级并选择叶级目标。", ["2 capabilities selected. Drag one or several rows across, or use the arrow actions."]="已选择 2 项能力。可将一行或多行拖到另一侧，也可使用箭头操作。",
        ["Wave D · Charts, rich editor contracts and chat"]="Wave D · 图表、富文本编辑器契约与聊天", ["Move the pointer across the chart."]="在图表上移动指针。", ["Axes show localized numeric ticks and month labels; grouped bar bands stay completely inside the plot."]="坐标轴显示本地化数字刻度与月份标签；分组柱形完全位于绘图区内。",
        ["Rich-text source"]="富文本源内容", ["LIVE RICH PREVIEW"]="实时富文本预览", ["Select source text and choose a command; the rendered preview updates immediately."]="选择源文本并执行命令；渲染预览会立即更新。",
        ["Select one or more bubbles to quote, cancel, or delete. Failed bubbles expose Retry; suggestions fill the composer."]="选择一个或多个气泡后可引用、取消或删除。失败气泡提供重试；建议项可填入编辑框。",
        ["Wave E · Skeleton and sequence"]="Wave E · 骨架屏与序列", ["Toggle loading"]="切换加载状态", ["Replay sequence"]="重播序列", ["Loaded content"]="已加载内容",
        ["Wave F · Structured input and hierarchy"]="Wave F · 结构化输入与层级", ["PIN / OTP input"]="PIN / OTP 输入", ["Click the segmented field, type or paste six digits, then use direct APIs to exercise masking, errors and completion."]="点击分段字段，输入或粘贴六位数字，再使用直接 API 演示遮罩、错误与完成状态。", ["Awaiting a six-digit verification code."]="等待六位验证码。",
        ["Set demo code"]="设置演示码", ["Clear"]="清除", ["Toggle mask"]="切换遮罩", ["Toggle error"]="切换错误状态",
        ["Tree view"]="树形视图", ["Expand folders, select rows, use Left/Right/Enter, or switch to RTL to verify mirrored hierarchy navigation."]="展开文件夹、选择行并使用左/右/Enter；也可切换 RTL 验证镜像层级导航。", ["Choose a node; Enter invokes the selected task."]="选择节点；按 Enter 调用所选任务。",
        ["Expand all"]="全部展开", ["Collapse all"]="全部折叠", ["Select Android"]="选择 Android", ["Toggle RTL"]="切换 RTL",
        ["Tag input"]="标签输入", ["Type to filter suggestions, press Enter or comma to add, click a suggestion, and use each chip’s remove action. Backspace removes the last chip when the editor is empty."]="输入以筛选建议，按 Enter 或逗号添加，点击建议或使用标签块的移除操作；编辑器为空时按退格删除最后一个标签。", ["Add release capability"]="添加发布能力", ["Add Localization"]="添加本地化", ["Clear tags"]="清除标签", ["2 capabilities selected; add up to 6."]="已选择 2 项能力；最多可添加 6 项。",
        ["Existing clean-room controls"]="现有净室实现控件", ["Assign the review to a teammate; the avatar is the actionable identity, not decoration."]="将评审分配给队友；头像是可操作身份，而非装饰。", ["No reviewer assigned."]="尚未分配评审人。", ["Click a specific half-star; spacing does not alter the selected step."]="点击准确的半星档位；间距不会改变所选步进。", ["Choose a path segment."]="请选择路径段。",

        // Flutter parity and interactive workflows
        ["Flutter Material parity · phases 1–4"]="Flutter Material 对齐 · 阶段 1–4", ["Home"]="首页", ["Ecosystem"]="生态控件", ["Account details"]="账户详情", ["Notification preferences"]="通知偏好", ["Danger zone"]="危险操作区", ["Choose a plan"]="选择方案", ["Configure"]="配置", ["Review"]="检查", ["Reorderable list"]="可重排序列表", ["Drag a row or use Ctrl+Up / Ctrl+Down. Buttons exercise the direct API."]="拖动行，或使用 Ctrl+上/下；按钮用于演示直接 API。", ["Move up"]="上移", ["Move down"]="下移",
        ["GridTile and GridTileBar"]="GridTile 与 GridTileBar", ["Open the project tile or use its trailing favorite action."]="打开项目磁贴，或使用尾部收藏操作。", ["Project preview"]="项目预览", ["No project action yet."]="尚无项目操作。",
        ["Adaptive controls · cross-platform sync settings"]="自适应控件 · 跨平台同步设置", ["The switches choose the platform policy. Run the same upload to compare Material and Cupertino progress metrics."]="开关用于选择平台策略。运行同一上传任务，以比较 Material 与 Cupertino 的进度样式。", ["Upload design tokens"]="上传设计令牌", ["One task, rendered with two platform policies"]="同一任务，以两种平台策略呈现", ["Ready to upload."]="准备上传。", ["Follow the current platform"]="跟随当前平台", ["Preview iOS control metrics"]="预览 iOS 控件尺寸", ["Run sync comparison"]="运行同步对比",
        ["About and licenses"]="关于与许可证", ["Open application details, then continue into a searchable license task and return."]="打开应用详情，然后进入可搜索的许可证任务并返回。", ["About this app"]="关于此应用", ["About dialog is closed."]="关于对话框已关闭。",
        ["Focus, shortcut and Hero workflow"]="焦点、快捷键与 Hero 工作流", ["Focus first"]="聚焦首项", ["Run Ctrl+Shift+S action"]="执行 Ctrl+Shift+S 操作", ["Move avatar"]="移动头像",
        ["Show banner"]="显示横幅", ["Learn more"]="了解更多", ["Retry"]="重试", ["Restore"]="恢复", ["Validate"]="验证", ["Choose account"]="选择账户", ["Save state"]="保存状态", ["Clear values"]="清除值", ["Restore state"]="恢复状态",
        ["Display name"]="显示名称", ["Role"]="角色", ["Required display name"]="必填显示名称", ["Choose one role"]="请选择一个角色", ["Select the deployment tier"]="选择部署层级", ["Set application preferences"]="设置应用偏好", ["Confirm and create"]="确认并创建",

        // Search and borderless window pages
        ["Search bars accept a query; search views expand the field into suggestions or results."]="搜索栏接收查询；搜索视图会将字段展开为建议或结果。", ["Search bar"]="搜索栏", ["Expanded search view"]="展开式搜索视图", ["Open search · Ctrl+K"]="打开搜索 · Ctrl+K", ["Search destinations"]="搜索目标", ["Search files, folders, or types"]="搜索文件、文件夹或类型", ["No matching files"]="没有匹配文件", ["Enter submits the focused search. Escape dismisses the expanded search view."]="Enter 提交当前搜索；Escape 关闭展开式搜索视图。",
        ["Borderless windows"]="无边框窗口", ["Chrome preview"]="窗口框架预览", ["Open the real secondary window to validate dragging, double-click maximize/restore, caption actions, resize grips, and theme behavior."]="打开真实辅助窗口，验证拖动、双击最大化/还原、标题按钮、缩放手柄与主题行为。", ["Platform adapter"]="平台适配器", ["Applications may inject IMdWindowPlatformAdapter for native system menus or specialized hit testing without changing view models."]="应用可注入 IMdWindowPlatformAdapter，以提供原生系统菜单或专用命中测试，而无需更改视图模型。", ["Safety contract"]="安全契约", ["Material application"]="Material 应用", ["Open borderless demo window"]="打开无边框演示窗口",
        ["Open command palette"]="打开命令面板", ["Close"]="关闭", ["Delete"]="删除", ["Quote"]="引用", ["Cancel"]="取消", ["Send"]="发送", ["Selected hierarchy"]="所选层级", ["Choose a destination"]="选择目标", ["Filter available"]="筛选可选项", ["Filter selected"]="筛选已选项", ["Message"]="消息", ["Search commands"]="搜索命令", ["No matching commands"]="没有匹配命令", ["Search states"]="搜索州", ["No results"]="没有结果", ["Search failed. Try again."]="搜索失败，请重试。", ["Replying to"]="正在回复", ["Filter people by name or team"]="按姓名或团队筛选人员", ["Create a record to see it in this result surface."]="创建记录后将在此结果表面显示。", ["Nothing here yet"]="这里暂时为空", ["Accessibility"]="无障碍", ["Localization"]="本地化", ["Testing"]="测试", ["Engineering"]="工程", ["Desktop"]="桌面", ["Android"]="Android", ["Controls"]="控件", ["Navigation"]="导航", ["PIN input"]="PIN 输入", ["Platforms"]="平台", ["Target hosts"]="目标宿主", ["Material components"]="Material 组件", ["Welcome. Select this bubble, quote it, or combine it with another selection."]="欢迎。选择此气泡后可引用它，也可与其他气泡多选。", ["Can the chat preserve provider-neutral message actions?"]="聊天能否保留与提供方无关的消息操作？", ["Yes. Selection, quote, delete, and retry are surfaced as commands and events."]="可以。选择、引用、删除和重试均通过命令与事件公开。", ["Send the release summary"]="发送发布摘要", ["Offline · not sent"]="离线 · 未发送", ["Show accessibility"]="显示无障碍信息", ["Explain paging"]="解释分页", ["Open docs"]="打开文档",
        ["Clean-room, Material-themed controls informed by frozen public behavior baselines. The package depends on Md3.Avalonia, never on the optional icon/font package. Every affordance below is interactive."]="依据冻结公开行为基线完成的净室 Material 控件。该包仅依赖 Md3.Avalonia，不依赖可选图标/字体包；下方每项操作都可真实交互。", ["Rating 3.5 of 5"]="评分 3.5 / 5", ["Sort name"]="按姓名排序", ["Preview copied rows"]="预览复制行", ["Line"]="折线", ["Bar"]="柱形",
        ["Official Flutter Material collection, form/dialog, advanced-surface, adaptive, focus, shortcut, and restoration patterns mapped to independent Avalonia controls."]="将 Flutter Material 的集合、表单/对话框、高级表面、自适应、焦点、快捷键与状态恢复模式映射为独立 Avalonia 控件。", ["Material banner"]="Material 横幅", ["Your changes could not be synchronized. The banner remains visible until an action is taken."]="无法同步更改。执行操作前横幅会保持可见。", ["Banner ready."]="横幅已就绪。", ["Expansion panels"]="展开面板", ["Edit lightweight settings without navigating away from the current page."]="无需离开当前页面即可编辑轻量设置。", ["Data table"]="数据表", ["All"]="全部", ["Select rows or a sortable heading."]="选择行或可排序表头。", ["Stepper"]="步骤器", ["Review the values, then press Continue to finish."]="检查各项值，然后按“继续”完成。", ["Step 1 is active."]="当前为第 1 步。", ["Pull to refresh"]="下拉刷新", ["Pull down from the top"]="从顶部向下拉", ["Release after the indicator is armed."]="指示器进入就绪状态后松开。", ["Phase 2 · paginated data table"]="阶段 2 · 分页数据表", ["Package"]="包", ["Platform"]="平台", ["Score"]="分数", ["Rows 1–3"]="第 1–3 行", ["Design tokens"]="设计令牌", ["Core controls"]="核心控件", ["Android validation"]="Android 验证", ["Selected: Design tokens · position 1 of 4"]="已选：设计令牌 · 第 1 / 4 项", ["Dismissible"]="可滑动移除", ["Archive"]="归档", ["Swipe this notification in either direction"]="向任一方向滑动此通知", ["Phase 3 · form validation"]="阶段 3 · 表单验证", ["Validation has not run."]="尚未执行验证。", ["Simple dialog"]="简单对话框", ["A modal account task blocks the page until an option or Cancel is chosen."]="模态账户任务会阻止页面操作，直到选择选项或取消。", ["No account selected."]="尚未选择账户。", ["Restorable picker state"]="可恢复的选择器状态", ["State is held by a provider-agnostic store."]="状态由与提供方无关的存储器保存。", ["Phase 4 · draggable scrollable sheet"]="阶段 4 · 可拖动滚动面板", ["Drag to resize · scroll after expansion"]="拖动调整大小 · 展开后滚动", ["The handle resizes the sheet. Nested wheel input expands/collapses at scroll boundaries."]="拖动手柄调整面板大小；嵌套滚轮输入会在滚动边界展开或收起。", ["Extent 50%"]="展开比例 50%", ["Use the focus controls, press Ctrl+Shift+S while this group is focused, or run the same command directly."]="使用焦点控件；该组聚焦时按 Ctrl+Shift+S，或直接执行同一命令。", ["Focus and shortcut workflow is ready."]="焦点与快捷键工作流已就绪。", ["Product updates"]="产品更新", ["Security alerts"]="安全提醒", ["Delete local cache"]="删除本地缓存", ["Downloads"]="下载", ["Enable telemetry"]="启用遥测", ["Use dynamic color"]="使用动态颜色", ["Simulate refresh"]="模拟刷新", ["Plan"]="方案", ["Last edited 2 minutes ago"]="2 分钟前编辑", ["Aurora dashboard"]="Aurora 仪表板",
        ["Extended client-area chrome keeps the platform resize frame, shadow and corner treatment while Material owns the caption content. This follows FluentAvalonia's AppWindow separation instead of removing all native decorations."]="扩展客户区窗口框架会保留平台缩放边框、阴影与圆角，同时由 Material 呈现标题内容。此实现遵循 FluentAvalonia AppWindow 的职责分离，而非移除全部原生装饰。", ["Android resolves to a no-op adapter. Windows keeps WindowDecorations.Full for native DWM state animations; macOS/Linux keep the portable border frame. All desktop adapters extend the client area and use Avalonia move/resize APIs. Interactive title-bar children never start a drag."]="Android 使用无操作适配器；Windows 保留 WindowDecorations.Full 以获得原生 DWM 状态动画，macOS/Linux 保留可移植边框。桌面适配器扩展客户区并使用 Avalonia 移动/缩放 API；标题栏中的可交互子项不会启动窗口拖动。",

        // Review 14 item 6: labels and headings the gallery still rendered in English.
        ["24-hour time"]="24 小时制", ["A slower weekend"]="慢节奏周末", ["Accent"]="强调色", ["Account"]="账户", ["Activity page content"]="活动页内容",
        ["Adaptive controls"]="自适应控件", ["Adaptive layout"]="自适应布局", ["Add new alarm"]="添加闹钟", ["Add to calendar"]="添加到日历", ["Add to collection"]="添加到收藏夹",
        ["Add to favorites"]="添加到收藏", ["Alpha"]="透明度", ["Americas"]="美洲", ["Amount"]="金额", ["Anchored menu"]="锚定菜单",
        ["Android host"]="Android 宿主", ["Animated text"]="动画文本", ["Animation sequence"]="动画序列", ["Any content, not just images"]="任意内容，不限于图片", ["Anyone with link"]="知道链接的人",
        ["Apply"]="应用", ["Apply source to app theme"]="将源色应用到应用主题", ["Archived"]="已归档", ["Asia Pacific"]="亚太", ["Async select"]="异步选择",
        ["Attachment"]="附件", ["Autocomplete"]="自动补全", ["Avatar"]="头像", ["Banner"]="横幅", ["Before and after"]="前后对比",
        ["Bluetooth"]="蓝牙", ["Bold"]="粗体", ["Bottom input"]="底部输入框", ["Bottom sheet"]="底部面板", ["Calendar"]="日历",
        ["Cascader"]="级联选择", ["Centered small"]="居中小型", ["Chart"]="图表", ["Chat view"]="聊天视图", ["Choose a framework"]="选择框架",
        ["Choose a menu action."]="选择一个菜单操作。", ["Choose a source color"]="选择源色", ["Choose an option"]="选择一项", ["Circular"]="环形", ["Clear and password actions"]="清除与密码操作",
        ["Collapse"]="收起", ["Collapsed deep path"]="折叠的深层路径", ["Collapsible Content Section"]="可折叠内容区", ["Color roles"]="颜色角色", ["Color styles"]="颜色样式",
        ["Command palette"]="命令面板", ["Compact color buttons"]="紧凑配色按钮", ["Compact layout"]="紧凑布局", ["Compact · 480"]="紧凑 · 480", ["Compose"]="撰写",
        ["Configurations"]="配置", ["Confirm"]="确认", ["Connected button group"]="连接式按钮组", ["Connection restored"]="连接已恢复", ["Contained"]="包含式",
        ["Container"]="容器", ["Container high"]="高层容器", ["Container low"]="低层容器", ["Continue"]="继续", ["Continuous"]="连续",
        ["Copy HEX"]="复制 HEX", ["Copy link"]="复制链接", ["Create"]="新建", ["Create record"]="创建记录", ["Data"]="数据",
        ["Data grids"]="数据表格", ["Date picker"]="日期选择器", ["Day"]="日", ["Default button color styles"]="默认按钮配色", ["Default density"]="默认密度",
        ["Default icon buttons"]="默认图标按钮", ["Delete item"]="删除项", ["Density"]="密度", ["Design"]="设计", ["Design Token Architecture"]="设计令牌架构",
        ["Design team"]="设计团队", ["Dial time"]="表盘选时", ["Direct control APIs"]="直接控件 API", ["Disabled"]="已禁用", ["Disabled indeterminate"]="禁用·不确定",
        ["Disabled selected"]="禁用·已选", ["Disabled setting"]="已禁用的设置", ["Disabled unselected"]="禁用·未选", ["Discard"]="放弃", ["Discrete"]="离散",
        ["Docked date"]="停靠式日期", ["Docs"]="文档", ["Document"]="文档", ["Done"]="完成", ["Draft moved to archive"]="草稿已移入归档",
        ["Draft saved"]="草稿已保存", ["Draggable scrollable sheet"]="可拖拽滚动面板", ["Draggable sheet"]="可拖拽面板", ["Dynamic scheme preview"]="动态配色预览", ["Elevated"]="浮起",
        ["Elevated card"]="浮起卡片", ["Email"]="邮箱", ["Enter a valid email address"]="请输入有效的邮箱地址", ["Error"]="错误", ["Error and disabled"]="错误与禁用",
        ["Error states"]="错误状态", ["Europe"]="欧洲", ["Expandable settings card"]="可展开设置卡片", ["Expanded destinations"]="展开的目的地", ["Expanded · 1040"]="展开 · 1040",
        ["Explore"]="探索", ["Export"]="导出", ["Export PDF"]="导出 PDF", ["Export image"]="导出图片", ["Expressive"]="表现力",
        ["Expressive filled"]="表现力填充", ["Expressive segmented"]="表现力分段", ["Expressive sizes"]="表现力尺寸", ["Extended FAB"]="扩展浮动操作按钮", ["FAB menu direction"]="浮动按钮菜单方向",
        ["FAB sizes"]="浮动按钮尺寸", ["Fade and reveal"]="淡入与揭示", ["Favorites"]="收藏", ["Feature"]="特性", ["Featured"]="精选",
        ["Featured story"]="精选内容", ["Filled and outlined"]="填充与描边", ["Filled card"]="填充卡片", ["Filled label"]="填充标签", ["Filter"]="筛选",
        ["First item"]="第一项", ["First option"]="第一个选项", ["Flights"]="航班", ["Flights content"]="航班内容", ["Floating action buttons"]="浮动操作按钮",
        ["Focus bottom input"]="聚焦底部输入框", ["Form validation"]="表单校验", ["Framework"]="框架", ["Full-width and inset"]="通栏与内缩", ["Gallery"]="组件库",
        ["Gamma"]="伽马", ["Generated Material scheme"]="生成的 Material 配色", ["Get directions"]="获取路线", ["Graded"]="分级", ["Grid"]="网格",
        ["Grid tiles"]="网格瓦片", ["Headline large"]="大标题", ["Hero"]="英雄动画", ["High"]="高", ["Hover card"]="悬浮卡片",
        ["Hover cards"]="悬浮卡片", ["Image comparison"]="图像对比", ["Inbox"]="收件箱", ["Inbox settings"]="收件箱设置", ["Indeterminate"]="不确定",
        ["Indeterminate error"]="不确定·错误", ["Input time"]="输入选时", ["Interaction"]="交互", ["Interactive breadcrumb"]="可交互面包屑", ["Inverse"]="反色",
        ["Inverse surface"]="反色表面", ["Inverse surface roles."]="反色表面角色。", ["Invite people"]="邀请成员", ["Italic"]="斜体", ["Item 1"]="项目 1",
        ["Item 2"]="项目 2", ["Item 3"]="项目 3", ["Item 4"]="项目 4", ["Keep"]="保留", ["Keyboard avoidance"]="键盘避让",
        ["Large"]="大", ["Large flexible"]="大型弹性", ["Layered hierarchy"]="分层层级", ["Leading icon and editable"]="前置图标与可编辑", ["Leading stepper"]="前置步进器",
        ["Left side sheet"]="左侧边栏", ["Light surface"]="浅色表面", ["Linear"]="线性", ["Link"]="链接", ["List"]="列表",
        ["Loading indicator"]="加载指示器", ["Local favorites"]="本地收藏", ["Main content"]="主要内容", ["Manage access"]="管理访问权限", ["Manage storage"]="管理存储",
        ["Map"]="地图", ["Mark all as read"]="全部标为已读", ["Masonry panel"]="瀑布流面板", ["Medium"]="中", ["Medium flexible"]="中型弹性",
        ["Medium · 760"]="中等 · 760", ["Menu surface"]="菜单表面", ["Message deleted"]="消息已删除", ["Modal"]="模态", ["Modal date"]="模态式日期",
        ["Month"]="月", ["Motion & Animations"]="动效与动画", ["Motion and style"]="动效与样式", ["Multi-browse"]="多项浏览", ["My Tasks"]="我的任务",
        ["Nature"]="自然", ["Navigate"]="导航", ["Nearby"]="附近", ["New document"]="新建文档", ["New folder"]="新建文件夹",
        ["No records"]="暂无记录", ["None"]="无", ["Notifications"]="通知", ["Numeric input"]="数字输入", ["Open"]="打开",
        ["Open basic dialog"]="打开基础对话框", ["Open full-screen"]="全屏打开", ["Open menu"]="打开菜单", ["Open now"]="营业中", ["Optional semantic elevation."]="可选的语义化高程。",
        ["Outlined card"]="描边卡片", ["Outlined label"]="描边标签", ["Overview page content"]="概览页内容", ["PIN and OTP input"]="PIN 与验证码输入", ["Page body"]="页面正文",
        ["Page content"]="页面内容", ["Paged items"]="分页项", ["Paged items view"]="分页项视图", ["Paginated data table"]="分页数据表", ["Paginated table"]="分页表格",
        ["Password"]="密码", ["Photo"]="照片", ["Picker restoration"]="选择器状态恢复", ["Play motion"]="播放动效", ["Playback speed"]="播放速度",
        ["Popover"]="气泡卡片", ["Popular"]="热门", ["Populated and disabled"]="有内容与禁用", ["Popup menu"]="弹出菜单", ["Powerline variant"]="Powerline 样式",
        ["Predeclared dialog templates"]="预声明对话框模板", ["Primary"]="主色", ["Primary container"]="主色容器", ["Privacy"]="隐私", ["Profile"]="个人资料",
        ["Progress indicators"]="进度指示器", ["Project name"]="项目名称", ["Quantity"]="数量", ["Queue from service"]="来自服务的队列", ["Range and precision"]="范围与精度",
        ["Rating"]="评分", ["Read only value"]="只读值", ["Read-only"]="只读", ["Ready"]="就绪", ["Ready to test"]="可以开始测试",
        ["Reduced"]="降低", ["Region"]="地区", ["Rename"]="重命名", ["Required option"]="必选项", ["Reset path"]="重置路径",
        ["Restaurants"]="餐厅", ["Result view"]="结果视图", ["Reviews"]="评价", ["Rich editor"]="富文本编辑器", ["Rich tooltip"]="富文本提示",
        ["Right side sheet"]="右侧边栏", ["Right-click menu"]="右键菜单", ["Right-click this surface"]="右键点击此表面", ["Sample apps"]="示例应用", ["Save a copy"]="保存副本",
        ["Saved"]="已保存", ["Scanner"]="扫描", ["Scrollable form area"]="可滚动表单区", ["Scrollable item 1"]="可滚动项 1", ["Scrollable item 2"]="可滚动项 2",
        ["Scrollable item 3"]="可滚动项 3", ["Scrollable item 4"]="可滚动项 4", ["Scrollable item 5"]="可滚动项 5", ["Scrollable item 6"]="可滚动项 6", ["Scrolling surface"]="滚动表面",
        ["Search all symbols"]="搜索全部图标", ["Search or select"]="搜索或选择", ["Search settings"]="搜索设置", ["Second item"]="第二项", ["Second option"]="第二个选项",
        ["Segmented buttons"]="分段按钮", ["Select one option"]="选择一项", ["Selected"]="已选", ["Selected disabled"]="已选·禁用", ["Selected error"]="已选·错误",
        ["Selection group"]="选择组", ["Selection states"]="选择状态", ["September"]="九月", ["Settings"]="设置", ["Shape and motion"]="形状与动效",
        ["Share"]="分享", ["Share this item"]="分享此项", ["Share with"]="分享给", ["Shared"]="已共享", ["Show contextual toolbar"]="显示上下文工具栏",
        ["Show status icons"]="显示状态图标", ["Sign out"]="退出登录", ["Sizes and motion schemes"]="尺寸与动效方案", ["Skeleton"]="骨架屏", ["Slidable item"]="可滑动项",
        ["Small"]="小", ["Small and large badges"]="小型与大型徽标", ["Soft keyboard avoidance"]="软键盘避让", ["Sort library"]="排序", ["Specifications"]="规格",
        ["Spin kit"]="加载动画", ["Split buttons"]="拆分按钮", ["Spring physics comparison"]="弹簧物理对比", ["Staggered panel"]="错落面板", ["Standalone anatomy"]="独立结构",
        ["Standard"]="标准", ["Standard button group"]="标准按钮组", ["Standard card"]="标准卡片", ["Starred"]="已加星标", ["Start"]="开始",
        ["Start typing"]="开始输入", ["States"]="状态", ["Steps of two"]="步长为二", ["Suggestion"]="建议", ["Supporting text"]="辅助文本",
        ["Surface container"]="表面容器", ["Surfaces and type scale"]="表面与字阶", ["Team"]="团队", ["Technology"]="技术", ["Temperature"]="温度",
        ["Text"]="文本", ["Time picker"]="时间选择器", ["Timeline"]="时间线", ["Title medium"]="中标题", ["Today"]="今天",
        ["Today (4)"]="今天 (4)", ["Toggle buttons"]="切换按钮", ["Toggle icon buttons"]="切换图标按钮", ["Toggle panel"]="切换面板", ["Toggle virtual keyboard"]="切换虚拟键盘",
        ["Top input"]="顶部输入框", ["Top rated"]="高分推荐", ["Transfer"]="穿梭框", ["Travel ideas"]="旅行灵感", ["Trips"]="行程",
        ["Two images"]="两张图片", ["Type a value"]="输入一个值", ["Type scale"]="字阶", ["Typed only"]="仅限输入", ["Unavailable"]="不可用",
        ["Uncontained"]="非包含式", ["Underline"]="下划线", ["Undo"]="撤销", ["Ungraded"]="未分级", ["Unselected"]="未选",
        ["Tray icon"]="托盘图标", ["Live demo"]="实时演示", ["Menu model"]="菜单模型", ["Menu ownership"]="菜单归属",
        ["Last activation"]="最近一次激活", ["Show tray icon"]="显示托盘图标", ["Material Gallery"]="Material Gallery",
        ["The notification area is desktop chrome: the operating system draws the icon and the menu, so Material owns the naming, defaults, lifetime and a replaceable platform adapter instead of a styled surface."]="通知区域属于桌面外壳：图标与菜单由操作系统绘制，因此 Material 负责命名、默认值、生命周期与可替换的平台适配器，而不是提供可样式化的表面。",
        ["Entries are native menu items, so commands, gestures, check and radio state, enabled state and submenus behave exactly as the platform menu bridge defines them."]="条目就是原生菜单项，因此命令、快捷键、勾选与单选状态、启用状态和子菜单的行为完全遵循平台菜单桥的定义。",
        ["The icon is rasterised from a Material Symbols glyph at run time, because the platform takes a bitmap rather than a vector. Switch it on, then use the notification area: every entry reports back into this page."]="图标在运行时由 Material Symbols 字形栅格化得到，因为平台需要位图而非矢量。打开开关后使用通知区域：每个条目都会把结果回传到本页。",
        ["Header, Command, CommandParameter and Gesture"]="Header、Command、CommandParameter 与 Gesture",
        ["IsChecked with ToggleType = CheckBox"]="IsChecked 配 ToggleType = CheckBox",
        ["MdTrayMenuItem.Items creates the nested NativeMenu on demand"]="MdTrayMenuItem.Items 按需创建嵌套 NativeMenu",
        ["MdTrayMenuItemSeparator, or Avalonia's NativeMenuItemSeparator"]="MdTrayMenuItemSeparator，或 Avalonia 的 NativeMenuItemSeparator",
        ["ToggleType = Radio, usually inside a submenu"]="ToggleType = Radio，通常位于子菜单内",
        ["Unselected error"]="未选·错误", ["Upcoming"]="即将到来", ["Updated today"]="今日更新", ["Urban"]="都市", ["Value indicator"]="数值指示器",
        ["Value is fixed"]="数值固定", ["Variants"]="变体", ["Vertical"]="垂直", ["View shortcuts"]="查看快捷键", ["Voice"]="语音",
        ["Wave"]="波浪", ["Week"]="周", ["Weekend plans"]="周末计划", ["Width and shape"]="宽度与形状", ["Without a stepper"]="无步进器",
        ["Workspace"]="工作区",
    };

    public static void Apply(Control root, CultureInfo culture)
    {
        var elements = new object[] { root }
            .Concat(root.GetLogicalDescendants().Cast<object>())
            .Concat(root.GetVisualDescendants().Cast<object>())
            .Distinct();
        foreach (var element in elements)
        {
            if (element is TextBlock text && text.Text is { } value)
            {
                var original = Originals.GetValue(text, _ => new Original(value)).Value;
                text.Text = Translate(original, culture);
            }
            else if (element is ContentControl content && content.Content is string label)
            {
                var original = Originals.GetValue(content, _ => new Original(label)).Value;
                content.Content = Translate(original, culture);
            }
            if (element is TextBox input && input.PlaceholderText is { } placeholder)
            {
                var original = PlaceholderOriginals.GetValue(input, _ => new Original(placeholder)).Value;
                input.PlaceholderText = Translate(original, culture);
            }
            if (element is MdTextBox field && field.Label is string fieldLabel)
            {
                var original = LabelOriginals.GetValue(field, _ => new Original(fieldLabel)).Value;
                field.Label = Translate(original, culture);
            }
            if (element is MdComboBox combo && combo.Label is string comboLabel)
            {
                var original = LabelOriginals.GetValue(combo, _ => new Original(comboLabel)).Value;
                combo.Label = Translate(original, culture);
            }
            if (element is MdCascader cascader)
            {
                if (cascader.Label is { } cascaderLabel)
                {
                    var original = LabelOriginals.GetValue(cascader, _ => new Original(cascaderLabel)).Value;
                    cascader.Label = Translate(original, culture);
                }
                if (cascader.PlaceholderText is { } cascaderPlaceholder)
                {
                    var original = PlaceholderOriginals.GetValue(cascader, _ => new Original(cascaderPlaceholder)).Value;
                    cascader.PlaceholderText = Translate(original, culture);
                }
            }
        }
    }

    public static string Choose(string english, string chinese) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh" ? chinese : english;

    private static string Translate(string source, CultureInfo culture) =>
        culture.TwoLetterISOLanguageName == "zh" && Zh.TryGetValue(source, out var value) ? value : source;
}
