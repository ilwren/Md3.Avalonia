# 聊天能力实施计划（手机端 + 桌面端）

状态：草案 · 2026-10-11 · 分支 `arena/51784b16-md3-avalonia`

本计划把 `MdChatView` 从"聊天外壳"推进到"可用于在线聊天应用的控件集"。范围仅限 Md3.Avalonia 控件库本身：不内置网络层、后端、AI 服务、媒体编解码器或 Markdown 引擎，这些都通过 provider 接口注入，与 Ecosystem 包已有的 provider-neutral 原则保持一致。

## 0. 基线与已完成的修复

现有能力：`MdChatView`、`MdChatMessage`、`MdChatMessagePresenter`（位于 `src/Md3.Avalonia.Extra/Controls/MdDataVisualization.cs`），加上核心包的导航、键盘避让、安全区、Badge、Avatar、SlidableItem 等。缺口详见上一轮分析。

本轮修复（Phase 0）：

| 项目 | 问题 | 修复 |
|---|---|---|
| Enter 发送 | 输入框是 `AcceptsReturn=True` 的 TextBox，会先消费 Enter，`MdChatView.OnKeyDown` 的发送分支可能不触发（待实测确认） | 在 `PART_Composer` 上注册隧道阶段的 KeyDown 处理：普通 Enter 发送并吞掉按键；Shift+Enter 保留换行 |
| 硬编码英文 | 模板中 "Quote"、"Delete"、"Type a message..."、"N selected"、"Replying to …"、"Send failed" 未走本地化 | 新增 `ChatQuote`、`ChatDelete`、`ChatTypeMessage`、`ChatReplyingTo`、`ChatReplyingToMessage`、`ChatSendFailed` 以及 `QuoteText`/`DeleteText`/`TypeMessageText`/`SelectionCountText`/`QuoteHeaderText`/`ReplyHeaderText`/`FailureText` 等可绑定属性，中英文同步 |
| 回归测试 | 无 | 新增 `MdChatViewRegressionTests`：Enter 发送、Shift+Enter 不发送、本地化切换、引用头、失败文本 |

> 注意：本轮的代码修改基于源码审读，**未经编译和测试运行**（沙箱无法访问 NuGet 与 dot.net）。合入前需要在带 .NET 10 SDK 的环境执行 `dotnet build Md3.Avalonia.sln -c Release` 和 `dotnet test tests/Md3.Avalonia.HeadlessTests`。

## 1. 阶段总览

| 阶段 | 主题 | 优先级 | 依赖 |
|---|---|---|---|
| Phase 0 | Enter 发送、本地化、回归测试 | 已完成（待验证） | — |
| Phase 1 | 消息内容：Markdown provider、流式文本 | P0 | Phase 0 — 已实现（核心部分，待构建验证；见下） |
| Phase 2 | 富媒体气泡与图片查看器 | P0 | Phase 1 — 已实现（核心部分，待构建验证；见下） |
| Phase 3 | 独立输入框 `MdChatComposer` | P0 | Phase 0 |
| Phase 4 | 消息交互：上下文菜单、滑动回复、编辑/撤回、反应 | P0/P1 | Phase 2、3 |
| Phase 5 | 列表行为：自动滚动、回到最新、历史加载锚点、分组与分隔线 | P0 | Phase 1 |
| Phase 6 | 状态类控件：输入中、已读、在线点、会话列表项、连接横幅 | P1 | Phase 5 |
| Phase 7 | 平台适配：桌面拖放、移动键盘、触觉反馈、性能压测 | P1/P2 | Phase 3、5 |
| Phase 8 | 质量收口：本地化全量扫描、无障碍、Gallery、文档、Maestro | 持续 | 全部 |

## 2. 各阶段详情

### Phase 1 — 消息内容

**目标**：文本消息可渲染 Markdown、代码块与链接；AI 类场景可流式追加。

- 新增 `IMdMarkdownProvider`：输入 `string`，输出 Avalonia 控件（或 `Inline` 集合）。核心包只提供纯文本回退实现 `MdPlainTextMarkdownProvider`，不引用任何 Markdown 库。
- 新增 `MdChatMessage.IsStreaming`（或 `MdChatMessageState.Streaming`）与 `MdStreamingText` 控件：按增量追加文本、可选光标、token 淡入。流结束时切换为完整渲染。
- 代码块：`MdCodeBlock` 支持语言标签、复制按钮、水平滚动；高亮通过 `IMdSyntaxHighlighter` 注入。
- 链接：`MdChatMessagePresenter` 的文本内容支持可点击链接，通过 `IMdLinkOpener` 注入打开行为（桌面用系统浏览器，移动端用平台 Launcher）。

**验收**：纯文本回退与 Markdown provider 两种模式的 Gallery 示例；流式追加 1000 个 token 不引发整列表重排；链接可键盘聚焦并激活。

### Phase 1 — 实现状态（2026-10-11）

- 已实现：`IMdMarkdownProvider` 契约、`MdPlainTextMarkdownProvider`、`MdChatView.MarkdownProvider` 与逐气泡传递、`MdChatMessage.IsStreaming`（流式光标 `PART_Caret`）、同 Id 替换时保留选择且不重复播报。
- 偏差：纯文本回退实现放在 `Md3.Avalonia.Chat` 包而不是核心包（聊天包拆分后，核心包不再承载聊天功能）。
- 追加实现：`MdCodeBlock`（语言标签、复制按钮、横向滚动）与参考实现 `MdFencedCodeMarkdownProvider`（识别 ``` 围栏；流式时未闭合的围栏延伸到末尾）。
- 追加实现：链接。`MdFencedCodeMarkdownProvider` 把正文中的 http/https 地址渲染为 `HyperlinkButton`，点击时调用注入的 `IMdLinkOpener.Open(Uri)`；控件本身不打开浏览器，桌面与移动端由应用决定。
- 追加实现：`IMdSyntaxHighlighter` 与通用参考实现 `MdKeywordSyntaxHighlighter`，由 `MdCodeBlock.Highlighter` 使用。
- 已验证：流式追加 1000 个 token（同一消息 Id 替换 1000 次）时，列表行容器实例保持不变，列表不重建（headless 测试 `Streaming_1000_Tokens_Keeps_The_Message_Row_Instance`）。
- 已知成本（未解决）：每个 token 都是新的 `MdChatMessage` 记录，行内的 `ContentPresenter` 会对新内容重新套用模板，因此气泡控件每次替换都会重建。这不是整列表重排，但对长流式回复有开销。改进方向：用稳定的行包装对象承载可变内容，避免替换记录。该项尚未实现，验收的"不引发整列表重排"仅指行容器层面。
- 未实现：`MdStreamingText` 独立控件与 token 淡入（流式光标已由 `PART_Caret` 提供）；完整 Markdown 解析（表格、强调、链接）由应用的 provider 实现。

### Phase 2 — 实现状态（2026-10-11）

- 已实现：`MdChatAttachment` / `MdChatAttachmentKind`（含 `Uri`）；`MdChatMessage.Attachments`；附件行（图片或文件卡片，`PART_Attachments`）；`MdChatView.AttachmentActivated`；`MdImageViewer`（滚轮与 +/- 缩放 1×–8×、拖动平移、0 复位、Escape 发出 `CloseRequested`）；`IMdImageProvider` 异步加载（`MdAttachmentImage`，含加载中、失败与重试状态）。
- 偏差：没有独立的 `MdChatImageMessage` / `MdChatFileMessage` 类型，统一由 `MdChatAttachment` 与气泡模板承载。
- 追加实现：`Thumbnail`（加载期间显示）、`Progress`（进度条）、`Duration` 与 `Waveform`（语音消息的时长与波形，`Waveform` 为以设备无关像素计的柱高）。语音播放由应用在 `AttachmentActivated` 中处理。
- 追加实现：查看器左右键分别发出 `PreviousRequested` / `NextRequested`，由应用决定切换哪张图。
- 追加实现：查看器挂载时记录当前焦点控件，卸载后把焦点还给它；应用从气泡打开查看器时，关闭后焦点回到该气泡（headless 测试 `Image_Viewer_Returns_Focus_To_The_Control_That_Opened_It`）。
- 未实现：`MdChatAttachment.State`；图片缓存（由 `IMdImageProvider` 实现方负责）；查看器未接入默认覆盖层（由应用在 `AttachmentActivated` 中打开）。

### Phase 2 — 富媒体与图片查看器

**目标**：图片、文件、语音消息作为一等公民。

- `MdChatAttachment` record：`Kind`（Image/File/Audio/Video）、`Name`、`Size`、`Uri`、`Thumbnail`、`Progress`、`State`。
- `MdChatImageMessage`：缩略图、宽高比占位、加载中 Skeleton、加载失败重试；点击打开查看器。
- `MdChatFileMessage`：文件图标、名称、大小、进度环、下载/打开动作。
- `MdChatVoiceMessage`：时长、播放/暂停、波形（数据由 provider 提供，控件不做解码）、倍速按钮。
- `MdImageViewer`：全屏覆盖层，双指/滚轮缩放、拖动、左右切换、Esc 关闭、焦点返回。可复用 `MdOverlays` 的 Popover 焦点管理。
- 图片加载走 `IMdImageProvider`（异步、可取消、带缓存），避免控件内直接发 HTTP 请求。

**验收**：大图（4000×3000）缩放不卡顿；图片加载失败显示占位与重试；查看器关闭后焦点回到触发的气泡。

### Phase 3 — 独立输入框 `MdChatComposer`

**目标**：把输入区从 `MdChatView` 模板中拆出，成为可单独使用、可替换的控件。`MdChatView` 内部改为组合它。

- 自动增高：最小 1 行，最大行数可配置（默认 6 行），超出后内部滚动。
- 键盘：Enter 发送、Shift+Enter 换行，并提供 `SendOnEnter` 开关（部分用户习惯 Ctrl+Enter 发送）。输入法组合期间（`Key.ImeProcessed`）不得误发送，需在 Windows/macOS/Linux 的 IME 下实测。
- 附件托盘：待发送附件的缩略图列表，可单个移除，发送前显示上传进度。
- 粘贴图片与拖放文件（与 Phase 7 的窗口级拖放共用 `IMdFileDropHandler`）。
- @提及：基于 `MdAutoCompleteBox` 的弹层，输入 `@` 触发，候选来自 `IMdMentionProvider`。
- 草稿：通过 `IMdDraftStore` 按会话保存与恢复。
- 表情：`MdEmojiPicker` 网格面板，分类、最近使用；数据由 provider 提供，不内置表情资源。
- 本地化与无障碍：所有提示、按钮、附件状态走 `MdLocalization`。

**验收**：多行输入时发送按钮位置稳定；粘贴图片后出现附件托盘；草稿在切换会话后保留。

### Phase 4 — 消息交互

- **上下文菜单**：`MdMessageContextMenu`。桌面端右键；移动端长按触发，紧凑宽度下改用底部 `MdSheetHost` 呈现。动作集合：复制、引用、编辑、撤回/删除、转发、反应、选择。动作由 `ICommand` 绑定。
- **滑动回复**：为 `MdSlidableItem` 提供一个聊天预设（左滑回复，带图标与阈值）。桌面端用键盘与按钮替代。
- **编辑**：气泡内联编辑态，Esc 取消、Enter 保存；编辑后显示"已编辑"标记（`IsEdited`）。
- **撤回**：`MdChatMessage` 增加 `IsRecalled`，气泡替换为灰色占位文本，不可再交互。
- **反应**：`MdReactionBar` 显示反应计数芯片（基于 `MdAssistChip`/`MdFilterChip` 样式），点击切换自己的反应；`MdChatMessage.Reactions` 只读集合。

**验收**：右键与长按均能弹出同一套动作；撤回后的消息不可引用、不可编辑；反应切换后计数即时更新。

### Phase 5 — 列表行为（最关键、风险最高）

**目标**：滚动体验对齐主流 IM：新消息自动贴底，用户上翻时不被打断。

- `MdChatView` 暴露：`IsAtBottom`（只读）、`UnreadCount`、`ScrollToLatest()`、`ScrollToMessage(MdChatMessage)`。
- 新消息到达：若 `IsAtBottom` 为真则平滑滚动到底；否则不滚动，并递增 `UnreadCount`，显示"回到最新"浮动按钮（带未读数）。
- 历史加载：滚动到顶部附近时触发 `HistoryProvider`；加载完成后保持原可见消息的锚点位置（前插内容时补偿偏移），不得跳动。
- 分组：`MdChatGroupingPolicy`（时间窗口、同一发送者合并）决定是否显示头像与昵称；合并的消息只保留气泡。
- 日期分隔与未读分割线：`MdChatDateSeparator`、`MdUnreadDivider`，由消息时间戳和 `LastReadMessageId` 计算。
- 虚拟化：`ListBox` 默认 `VirtualizingStackPanel`，需验证可变高度 + 锚点补偿的正确性。

**验收**（需要自动化测试）：
1. 在底部时收到消息，视口停在底部；
2. 在中间时收到消息，视口位置不变且 `UnreadCount` 增加；
3. 前插 50 条历史后，第一条可见消息的屏幕位置偏差 ≤ 1 DIP；
4. 10 万条消息滚动不超过 16 ms/帧（在目标机器上记录基准）。

### Phase 6 — 状态类控件

- `MdTypingIndicator`：三点动画，支持多人文案（"A 和 B 正在输入"），遵守减少动态效果设置。
- `MdReadReceipts`：已读头像组（复用 `MdAvatarGroup`），超过 N 人显示计数。
- `MdPresenceDot`：在线/离开/忙碌/离线的状态点，附无障碍名称。
- `MdConversationListItem`：头像、名称、最后消息摘要、时间、`MdBadge` 未读数、置顶标记。可直接放进 `MdList`。
- `MdConnectionBanner`：基于 `MdBanner` 的预设，展示"连接中/已离线/重连中"，带重试动作。

### Phase 7 — 平台适配

- **桌面**：窗口级拖放区域（`MdFileDropZone`），拖入文件显示覆盖提示；窗口内快捷键（发送、换行、上一条编辑、搜索）用 `MdShortcutScope`。
- **移动**：`MdKeyboardAvoidingHost` 接入 `MdChatComposer`，键盘弹出时保证输入框与最后一条消息可见；`MdSafeArea` 处理刘海与手势条；底部动作用 `MdSheetHost`。
- **触觉反馈**：长按、撤回等动作通过 `IMdHapticFeedback` provider 触发，默认空实现。
- **性能**：建立聊天专用基准（`tests/` 下的 headless 压测），记录 10 万条消息的首屏、滚动和流式追加耗时。

### Phase 8 — 质量收口

- 全量扫描 Chat 相关控件模板的字符串字面量，确保全部走 `MdLocalization`（当前仅完成 `MdChatView` 与 `MdChatMessagePresenter`）。
- 无障碍：每个气泡的 AutomationPeer 名称包含发送者、时间、内容与状态；新消息通过 `AutomationProperties.LiveSetting` 播报。
- Gallery：`ChatViewGalleryPage` 增加流式、富媒体、分组、移动布局示例。
- 文档：更新 `docs/wiki/Extra-Controls-API.md` 的聊天章节，并在 `docs/FLUTTER_COMPONENT_PARITY.md` 中把尚未实现的能力标为"计划中"。
- Maestro：增加"发送、引用、多选删除、历史加载"四条端到端流程。

## 3. 文档修正

`docs/FLUTTER_COMPONENT_PARITY.md` 原先写明 Chat 支持"message grouping"和"incremental history loading"。经源码核对，消息分组尚未实现，历史加载也需要应用侧主动调用 `LoadHistoryAsync`，没有自动触发。本轮已把这两项改为计划中。

## 4. 风险与待决问题

| 风险 | 说明 | 应对 |
|---|---|---|
| 锚点补偿在虚拟化列表中的正确性 | Avalonia `VirtualizingStackPanel` 对可变高度的前插补偿需要专门验证 | Phase 5 先做原型与压测，不通过则考虑自定义 Panel |
| IME 与 Enter 发送冲突 | 中文输入法下 Enter 常用于确认候选词 | 依赖 `Key.ImeProcessed` 判断，需三平台实测 |
| Markdown 与代码高亮的依赖选择 | 第三方库会带来许可证与 AOT/裁剪问题 | 只定义 provider 接口；Gallery 示例可用可选包 |
| 移动端原生验证 | Android 工作负载在本沙箱不可用 | 发布前在真机完成触摸与键盘验收 |
| 未编译的本轮改动 | 沙箱无法构建 | 合入前必须在 .NET 10 环境构建与跑测试 |

## 5. 建议的执行顺序

1. 先在 CI 或本地跑通 Phase 0 的构建与测试，确认 Enter 修复有效；
2. Phase 5（列表行为）与 Phase 3（Composer 拆分）并行推进，前者风险最高，应尽早做原型；
3. Phase 1、2 在 Phase 3 稳定后再做，以免 Composer 的 API 频繁变动；
4. Phase 4、6、7 按需求优先级排期，Phase 8 随每个阶段收口。
