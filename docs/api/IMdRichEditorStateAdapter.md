# `IMdRichEditorStateAdapter` API

**包：** `Md3.Avalonia.Extra`<br>
**命名空间：** `Md3.Avalonia.Extra.Controls`<br>
**源码：** [`MdDataVisualization.cs`](../../src/Md3.Avalonia.Extra/Controls/MdDataVisualization.cs)

`IMdRichEditorStateAdapter` 是可选的富文本状态接口。它报告当前选择区对应的切换命令是否处于启用状态，例如粗体或斜体。

## 成员

```csharp
bool IsCommandChecked(MdRichEditorCommand command);
```

将同一适配器同时赋给 `MdRichEditor.Adapter`；控件会在刷新工具栏时将适配器转换为此接口并读取状态：

```csharp
public sealed class EditorAdapter : IMdRichEditorAdapter, IMdRichEditorStateAdapter
{
    // 示例中该状态由文档引擎的当前选择区提供。
    public bool SelectionIsBold { get; set; }
    public object? Document { get; set; }
    public event EventHandler? StateChanged;

    public bool CanExecute(MdRichEditorCommand command, object? parameter = null) => true;
    public void Execute(MdRichEditorCommand command, object? parameter = null) { }
    public bool IsCommandChecked(MdRichEditorCommand command) =>
        command == MdRichEditorCommand.Bold && SelectionIsBold;
}
```

若适配器未实现此接口，工具栏切换命令按未选中状态显示。`IMdRichEditorStateAdapter` 不负责执行命令，也不提供状态变更通知；变更后仍应由 `IMdRichEditorAdapter.StateChanged` 通知宿主。
