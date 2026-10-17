# `IMdRichEditorAdapter` API

**包：** `Md3.Avalonia.Extra`<br>
**命名空间：** `Md3.Avalonia.Extra.Controls`<br>
**源码：** [`MdDataVisualization.cs`](../../src/Md3.Avalonia.Extra/Controls/MdDataVisualization.cs)

`IMdRichEditorAdapter` 将 `MdRichEditor` 工具栏连接到应用所选的文档模型与编辑引擎。适配器负责文档存储、命令可用性与命令执行；控件提供 Material 工具栏外观。

## 成员

```csharp
object? Document { get; set; }
bool CanExecute(MdRichEditorCommand command, object? parameter = null);
void Execute(MdRichEditorCommand command, object? parameter = null);
event EventHandler? StateChanged;
```

| 成员 | 契约 |
|---|---|
| `Document` | 获取或替换适配器所维护的文档值；其具体类型由适配器约定。 |
| `CanExecute` | 按当前文档、选择区及可选参数报告命令是否可执行。 |
| `Execute` | 对当前文档与选择区执行命令。 |
| `StateChanged` | 文档、选择区或命令状态变化时触发；`MdRichEditor` 据此刷新工具栏并发出 `EditorStateChanged`。 |

实现示例：

```csharp
public sealed class EditorAdapter : IMdRichEditorAdapter
{
    public object? Document { get; set; }
    public event EventHandler? StateChanged;

    public bool CanExecute(MdRichEditorCommand command, object? parameter = null) => true;
    public void Execute(MdRichEditorCommand command, object? parameter = null)
    {
        // 更新文档或选择区后通知宿主。
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}

editor.Adapter = new EditorAdapter();
```

控件的 `Execute(command, parameter)` 在适配器缺失或 `CanExecute` 返回 `false` 时返回 `false`，否则执行命令并返回 `true`。内置 `MdTextBoxRichEditorAdapter` 以文本框和轻量格式标记作为示例实现；需要完整文档引擎时应接入应用选定的实现。
