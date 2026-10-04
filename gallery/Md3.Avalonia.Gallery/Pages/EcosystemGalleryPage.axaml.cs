using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Md3.Avalonia.Extra.Controls;
using Md3.Avalonia.Extra.Infrastructure;

namespace Md3.Avalonia.Gallery.Pages;

public partial class EcosystemGalleryPage : UserControl
{
    private readonly ObservableCollection<MdChatMessage> _messages =
    [
        new("welcome", MdChatMessageRole.Assistant, L("Welcome. Select this bubble, quote it, or combine it with another selection.", "欢迎。选择此气泡后可引用它，也可与其他气泡多选。"), DateTimeOffset.Now.AddMinutes(-4), L("Material assistant", "Material 助手")),
        new("question", MdChatMessageRole.User, L("Can the chat preserve provider-neutral message actions?", "聊天能否保留与提供方无关的消息操作？"), DateTimeOffset.Now.AddMinutes(-3), L("You", "你")),
        new("answer", MdChatMessageRole.Assistant, L("Yes. Selection, quote, delete, and retry are surfaced as commands and events.", "可以。选择、引用、删除和重试均通过命令与事件公开。"), DateTimeOffset.Now.AddMinutes(-2), L("Material assistant", "Material 助手"), ReplyToId: "question", ReplyPreview: L("Can the chat preserve provider-neutral message actions?", "聊天能否保留与提供方无关的消息操作？")),
        new("failed", MdChatMessageRole.User, L("Send the release summary", "发送发布摘要"), DateTimeOffset.Now.AddMinutes(-1), L("You", "你"), MdAsyncRequestState.Error, ErrorText: L("Offline · not sent", "离线 · 未发送"))
    ];
    private readonly MdTextBoxRichEditorAdapter _editorAdapter;
    private readonly ObservableCollection<string> _tags = [L("Accessibility", "无障碍"), "Android"];
    private bool _sortAscending = true;
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public EcosystemGalleryPage()
    {
        InitializeComponent();
        _editorAdapter = new MdTextBoxRichEditorAdapter(EditorText, RichPreview);
        CommandPalette.ItemsSource = new[]
        {
            new MdCommandItem(L("Toggle skeleton", "切换骨架屏"), new RelayCommand(() => Skeleton.IsLoading = !Skeleton.IsLoading), Description: L("Switch loading placeholder state", "切换加载占位状态"), Keywords: "loading shimmer"),
            new MdCommandItem(L("Replay sequence", "重播序列"), new AsyncRelayCommand(async () => await Sequence.PlayAsync()), Description: L("Run staggered entrance motion", "运行交错进入动效"), Keywords: "animation motion"),
            new MdCommandItem(L("Refresh pages", "刷新分页"), new AsyncRelayCommand(async () => await PagedItems.RefreshAsync()), Description: L("Reload paginated records", "重新加载分页记录"), Keywords: "paging data")
        };

        EnterpriseGrid.Columns.Add(new MdDataGridColumn { Header = L("Name", "姓名"), PropertyName = nameof(GridRow.Name), Width = new GridLength(2, GridUnitType.Star) });
        EnterpriseGrid.Columns.Add(new MdDataGridColumn { Header = L("Team", "团队"), PropertyName = nameof(GridRow.Team), Width = new GridLength(1, GridUnitType.Star) });
        EnterpriseGrid.Columns.Add(new MdDataGridColumn { Header = L("Score", "分数"), PropertyName = nameof(GridRow.Score), Width = new GridLength(96), IsEditable = true });
        EnterpriseGrid.DataSource = new[] { new GridRow("Ada", L("Design", "设计"), 92), new GridRow("Lin", L("Engineering", "工程"), 97), new GridRow("Maya", L("Research", "研究"), 89), new GridRow("Noah", L("Design", "设计"), 94), new GridRow("Zoe", L("Engineering", "工程"), 91) };

        PagedItems.PageProvider = request =>
        {
            var page = Enumerable.Range(request.PageKey * request.PageSize + 1, request.PageSize).Select(number => (object?)L($"Record {number}", $"记录 {number}")).ToArray();
            return ValueTask.FromResult(new MdPageResult<object?>(page, request.PageKey >= 2 ? null : request.PageKey + 1, request.PageKey >= 2));
        };

        AsyncSelect.ItemsSource = new[]
        {
            L("Alabama", "阿拉巴马州"), L("Alaska", "阿拉斯加州"), L("Arizona", "亚利桑那州"), L("California", "加利福尼亚州"),
            L("Colorado", "科罗拉多州"), L("New Jersey", "新泽西州"), L("New York", "纽约州"), L("Washington", "华盛顿州")
        };
        Calendar.SelectionMode = MdCalendarSelectionMode.Range;
        Calendar.BadgeProvider = date => date.Day is 5 or 14 or 24 ? "•" : null;
        Timeline.ItemsSource = new[]
        {
            new MdTimelineItem(L("Created", "已创建"), L("Draft created", "草稿已创建"), DateTimeOffset.Now.AddHours(-4), MdTimelineItemState.Completed),
            new MdTimelineItem(L("Review", "评审"), L("Accessibility and visuals", "无障碍与视觉"), DateTimeOffset.Now.AddHours(-1), MdTimelineItemState.Active),
            new MdTimelineItem(L("Publish", "发布"), L("Package validation", "包验证"), null, MdTimelineItemState.Neutral)
        };
        Cascader.ItemsSource = new[]
        {
            new MdCascaderItem("design", L("Design", "设计"), [new MdCascaderItem("material", "Material", [new MdCascaderItem("controls", L("Controls", "控件")), new MdCascaderItem("motion", L("Motion", "动效"))])]),
            new MdCascaderItem("engineering", L("Engineering", "工程"), [new MdCascaderItem("desktop", L("Desktop", "桌面")), new MdCascaderItem("android", "Android")])
        };
        Transfer.ItemsSource = new[] { L("Accessibility", "无障碍"), "Android", L("Desktop", "桌面"), L("Localization", "本地化"), L("Motion", "动效"), L("Testing", "测试") };
        Transfer.SelectedItems = new[] { L("Accessibility", "无障碍"), L("Testing", "测试") };
        ResultView.ActionCommand = new RelayCommand(() =>
        {
            ResultView.Kind = MdResultKind.Success;
            ResultView.Title = L("Record created", "记录已创建");
            ResultView.Content = L("The primary result action executed successfully.", "主要结果操作已成功执行。");
        });

        Chart.Series =
        [
            new MdChartSeries(L("Adoption", "采用率"), [new(0, 18, L("Jan", "1月")), new(1, 31, L("Feb", "2月")), new(2, 28, L("Mar", "3月")), new(3, 54, L("Apr", "4月")), new(4, 68, L("May", "5月"))]),
            new MdChartSeries(L("Quality", "质量"), [new(0, 42), new(1, 45), new(2, 58), new(3, 72), new(4, 88)])
        ];
        RichEditor.Adapter = _editorAdapter;
        Chat.MessagesSource = _messages;
        Chat.SuggestionsSource = new[] { L("Show accessibility", "显示无障碍信息"), L("Explain paging", "解释分页"), L("Open docs", "打开文档") };
        Chat.AttachmentRequested += ChatAttachmentRequested;

        var controls = new MdTreeNode("controls", L("Controls", "控件"),
        [
            new MdTreeNode("input", L("Input", "输入"), [new MdTreeNode("pin", L("PIN input", "PIN 输入")), new MdTreeNode("tags", L("Tag input", "标签输入"))]),
            new MdTreeNode("navigation", L("Navigation", "导航"), [new MdTreeNode("tree", L("Tree view", "树形视图"))])
        ], L("Material components", "Material 组件")) { IsExpanded = true };
        controls.Children[0].IsExpanded = true;
        var platforms = new MdTreeNode("platforms", L("Platforms", "平台"),
        [
            new MdTreeNode("desktop", L("Desktop", "桌面"), data: L("Windows, macOS and Linux", "Windows、macOS 与 Linux")),
            new MdTreeNode("android", "Android", data: L("Touch and soft-keyboard host", "触摸与软键盘宿主"))
        ], L("Target hosts", "目标宿主")) { IsExpanded = true };
        Tree.Roots = [controls, platforms];
        Tree.SelectedNode = controls.Children[0].Children[0];

        TagInput.TagsSource = _tags;
        TagInput.SuggestionsSource = new[] { L("Accessibility", "无障碍"), "Android", L("Desktop", "桌面"), L("Localization", "本地化"), L("Motion", "动效"), L("Performance", "性能"), L("Testing", "测试"), L("Theming", "主题") };
        TagInput.TagValidator = tag => tag.Length < 3 ? L("Use at least 3 characters", "请至少输入 3 个字符") : null;
    }

    private void OverlayOpened(object? sender, EventArgs e) => OverlayStatus.Text = L("Popover opened; scroll, click outside, or press Escape to dismiss it.", "浮层已打开；滚动、点击外部或按 Escape 可关闭。");
    private void HoverCardOpened(object? sender, EventArgs e) => OverlayStatus.Text = L("Hover card opened; moving away or scrolling dismisses it.", "悬停卡片已打开；移开指针或滚动可关闭。");
    private void OverlayClosed(object? sender, EventArgs e) => OverlayStatus.Text = L("Transient surface dismissed; later sections remain unobstructed.", "临时浮层已关闭；后续区域不会被遮挡。");
    private void OpenCommandPalette(object? sender, RoutedEventArgs e) => CommandPalette.Show();
    private void CommandPaletteInvoked(object? sender, MdCommandItem item) => OverlayStatus.Text = L($"Command executed: {item.Title}.", $"已执行命令：{item.Title}。");
    private void CloseSlidable(object? sender, RoutedEventArgs e) => Slidable.Close();
    private void GridFilterChanged(object? sender, TextChangedEventArgs e) => EnterpriseGrid.FilterText = (sender as TextBox)?.Text;
    private void SortGrid(object? sender, RoutedEventArgs e)
    {
        EnterpriseGrid.SortBy(nameof(GridRow.Name), _sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending);
        _sortAscending = !_sortAscending;
    }
    private void PreviewGridClipboard(object? sender, RoutedEventArgs e) => GridStatus.Text = EnterpriseGrid.BuildClipboardText().Replace(Environment.NewLine, " · ");
    private void MasonryCardInvoked(object? sender, RoutedEventArgs e) =>
        MasonryStatus.Text = L($"Opened project context: {(sender as Control)?.Tag}.", $"已打开项目上下文：{(sender as Control)?.Tag}。");
    private async void LoadNextPage(object? sender, RoutedEventArgs e)
    {
        await PagedItems.LoadNextPageAsync();
        PagingStatus.Text = L($"{PagedItems.Items.Count} records loaded · state {PagedItems.State}.", $"已加载 {PagedItems.Items.Count} 条记录 · 状态 {PagedItems.State}。");
    }
    private async void RefreshPages(object? sender, RoutedEventArgs e)
    {
        await PagedItems.RefreshAsync();
        PagingStatus.Text = L($"Refreshed from page 1 · {PagedItems.Items.Count} records loaded.", $"已从第 1 页刷新 · 共加载 {PagedItems.Items.Count} 条记录。");
    }
    private void PagesRefreshed(object? sender, EventArgs e) => PagingStatus.Text = L("Existing pages cleared; loading page 1…", "已清除现有页面；正在加载第 1 页…");
    private void AsyncSelectionCommitted(object? sender, object? item) =>
        AsyncSelectStatus.Text = L($"Selected state: {item}.", $"已选择州：{item}。");
    private void CalendarDateInvoked(object? sender, DateTimeOffset date) =>
        CalendarStatus.Text = Calendar.RangeEnd is { } end
            ? L($"Range: {Calendar.SelectedDate:d} – {end:d}", $"范围：{Calendar.SelectedDate:d} – {end:d}")
            : L($"Range starts {date:d}. Drag to an end date.", $"范围起点为 {date:d}。请拖动到结束日期。");
    private void CascaderSelectionChanged(object? sender, IReadOnlyList<MdCascaderItem> path) =>
        CascaderStatus.Text = path.Count == 0 ? L("Hierarchy cleared.", "层级选择已清除。") : L($"Destination: {string.Join(" / ", path.Select(item => item.Label))}.", $"目标：{string.Join(" / ", path.Select(item => item.Label))}。");
    private void TransferSelectionChanged(object? sender, EventArgs e) =>
        TransferStatus.Text = L($"{Transfer.TargetItems.Count} selected · {Transfer.AvailableItems.Count} available.", $"已选择 {Transfer.TargetItems.Count} 项 · 可选 {Transfer.AvailableItems.Count} 项。");
    private void ChangeChartKind(object? sender, RoutedEventArgs e) { if (sender is Button { Tag: string tag } && Enum.TryParse<MdChartKind>(tag, out var kind)) Chart.Kind = kind; }
    private void ChartPointChanged(object? sender, MdChartPoint? point) => ChartStatus.Text = point is null
        ? L("Move the pointer across the chart.", "在图表上移动指针。")
        : $"{point.Label ?? $"X {point.X:0.#}"}: {point.Y:0.#}%.";
    private void EditorStateChanged(object? sender, EventArgs e) =>
        EditorStatus.Text = L($"Executed {_editorAdapter.LastCommand}; current text length {EditorText.Text?.Length ?? 0}.", $"已执行 {_editorAdapter.LastCommand}；当前文本长度为 {EditorText.Text?.Length ?? 0}。");
    private void ChatSubmitted(object? sender, string text)
    {
        var quote = Chat.QuotedMessage;
        _messages.Add(new MdChatMessage(Guid.NewGuid().ToString("N"), MdChatMessageRole.User, text, DateTimeOffset.Now, L("You", "你"),
            ReplyToId: quote?.Id, ReplyPreview: quote?.Content));
        _messages.Add(new MdChatMessage(Guid.NewGuid().ToString("N"), MdChatMessageRole.Assistant, L($"Provider adapter received: {text}", $"提供方适配器已收到：{text}"), DateTimeOffset.Now, L("Demo adapter", "演示适配器")));
        ChatStatus.Text = quote is null
            ? L($"Sent “{text}”; demo provider appended a response.", $"已发送“{text}”；演示提供方已追加回复。")
            : L($"Sent a reply to {quote.Sender}.", $"已向 {quote.Sender} 发送引用回复。");
    }
    private void ChatSelectionChanged(object? sender, IReadOnlyList<MdChatMessage> messages) =>
        ChatStatus.Text = messages.Count == 0
            ? L("Bubble selection cleared.", "已清除气泡选择。")
            : L($"{messages.Count} bubble{(messages.Count == 1 ? string.Empty : "s")} selected; use Quote, Delete, or Cancel.", $"已选择 {messages.Count} 个气泡；可引用、删除或取消。");
    private void ChatDeleteRequested(object? sender, IReadOnlyList<MdChatMessage> messages)
    {
        foreach (var message in messages) _messages.Remove(message);
        ChatStatus.Text = L($"Deleted {messages.Count} selected message{(messages.Count == 1 ? string.Empty : "s")}.", $"已删除 {messages.Count} 条所选消息。");
    }
    private void ChatQuoteRequested(object? sender, MdChatMessage message) =>
        ChatStatus.Text = L($"Replying to {message.Sender}; submit or cancel the quote preview.", $"正在回复 {message.Sender}；请发送或取消引用预览。");
    private void ChatRetryRequested(object? sender, MdChatMessage message)
    {
        var index = _messages.IndexOf(message);
        if (index >= 0) _messages[index] = message with { State = MdAsyncRequestState.Data, ErrorText = null, Timestamp = DateTimeOffset.Now };
        ChatStatus.Text = L($"Retried “{message.Content}”; provider marked it sent.", $"已重试“{message.Content}”；提供方已将其标记为已发送。");
    }
    private void ChatAttachmentRequested(object? sender, EventArgs e)
    {
        _messages.Add(new MdChatMessage(Guid.NewGuid().ToString("N"), MdChatMessageRole.User, L("📎 Release-notes.md (24 KB)", "📎 发布说明.md (24 KB)"), DateTimeOffset.Now, L("You", "你")));
        ChatStatus.Text = L("Attachment action invoked; sample attachment file added.", "已调用附件操作；已添加示例附件文件。");
    }
    private void ToggleSkeleton(object? sender, RoutedEventArgs e) => Skeleton.IsLoading = !Skeleton.IsLoading;
    private async void ReplaySequence(object? sender, RoutedEventArgs e) => await Sequence.PlayAsync();

    private void PinCompleted(object? sender, string code) => PinStatus.Text = L($"Completed: {code}. VerifyCommand/event paths are now eligible.", $"已完成：{code}。现在可执行验证命令/事件路径。");
    private void SetPinDemo(object? sender, RoutedEventArgs e) => PinInput.SetCode("273841");
    private void ClearPin(object? sender, RoutedEventArgs e) { PinInput.Clear(); PinStatus.Text = L("Cleared; focus returned to the editor.", "已清除；焦点已返回编辑器。"); }
    private void TogglePinMask(object? sender, RoutedEventArgs e) { PinInput.IsObscured = !PinInput.IsObscured; PinStatus.Text = PinInput.IsObscured ? L("Digits are masked.", "数字已遮罩。") : L("Digits are visible.", "数字可见。"); }
    private void TogglePinError(object? sender, RoutedEventArgs e) { PinInput.IsError = !PinInput.IsError; PinStatus.Text = PinInput.IsError ? L("Error state enabled.", "已启用错误状态。") : L("Error state cleared.", "已清除错误状态。"); }

    private void TreeNodeInvoked(object? sender, MdTreeNode node) => TreeStatus.Text = L($"Invoked {node.Label} ({node.Id}).", $"已调用 {node.Label}（{node.Id}）。");
    private void TreeExpansionChanged(object? sender, MdTreeNode node) => TreeStatus.Text = L($"{node.Label} is {(node.IsExpanded ? "expanded" : "collapsed")}.", $"{node.Label} 已{(node.IsExpanded ? "展开" : "折叠")}。");
    private void ExpandTree(object? sender, RoutedEventArgs e) { Tree.ExpandAll(); TreeStatus.Text = L($"Expanded {Tree.VisibleRows.Count} visible nodes.", $"已展开 {Tree.VisibleRows.Count} 个可见节点。"); }
    private void CollapseTree(object? sender, RoutedEventArgs e) { Tree.CollapseAll(); TreeStatus.Text = L("All hierarchy branches collapsed.", "已折叠全部层级分支。"); }
    private void SelectAndroidNode(object? sender, RoutedEventArgs e) => TreeStatus.Text = Tree.SelectById("android") ? L("Android revealed and selected through the direct API.", "已通过直接 API 显示并选择 Android。") : L("Android node was not found.", "未找到 Android 节点。");
    private void ToggleTreeRtl(object? sender, RoutedEventArgs e)
    {
        Tree.FlowDirection = Tree.FlowDirection == global::Avalonia.Media.FlowDirection.RightToLeft
            ? global::Avalonia.Media.FlowDirection.LeftToRight
            : global::Avalonia.Media.FlowDirection.RightToLeft;
        TreeStatus.Text = L($"Tree flow direction: {Tree.FlowDirection}.", $"树形视图流向：{Tree.FlowDirection}。");
    }

    private void TagAdded(object? sender, MdTagChangedEventArgs e) => TagStatus.Text = L($"Added {e.Tag}; {TagInput.Tags.Count} of {TagInput.MaximumTags} selected.", $"已添加 {e.Tag}；已选择 {TagInput.Tags.Count} / {TagInput.MaximumTags} 项。");
    private void TagRemoved(object? sender, MdTagChangedEventArgs e) => TagStatus.Text = L($"Removed {e.Tag}; {TagInput.Tags.Count} remain.", $"已移除 {e.Tag}；剩余 {TagInput.Tags.Count} 项。");
    private void AddLocalizationTag(object? sender, RoutedEventArgs e) => TagInput.AddTag(L("Localization", "本地化"));
    private void ClearTags(object? sender, RoutedEventArgs e) { TagInput.ClearTags(); TagStatus.Text = L("All tags cleared through the direct API.", "已通过直接 API 清除全部标签。"); }

    private void AssignReviewer(object? sender, RoutedEventArgs e) =>
        ReviewerStatus.Text = L($"Review assigned to {(sender as Control)?.Tag}.", $"评审已分配给 {(sender as Control)?.Tag}。");
    private void RatingChanged(object? sender, RangeBaseValueChangedEventArgs e) => RatingStatus.Text = L($"Rating {e.NewValue:0.#} of 5", $"评分 {e.NewValue:0.#} / 5");
    private sealed class GridRow(string name, string team, int score)
    {
        public string Name { get; } = name;
        public string Team { get; } = team;
        public int Score { get; set; } = score;
    }

}
