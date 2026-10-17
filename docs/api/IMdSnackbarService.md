# `IMdSnackbarService` API

**包：** `Md3.Avalonia`<br>
**命名空间：** `Md3.Avalonia.Controls`<br>
**源码：** [`IMdSnackbarService.cs`](../../src/Md3.Avalonia/Controls/IMdSnackbarService.cs)

`IMdSnackbarService` 用于从 ViewModel 向附加的 `MdSnackbarHost` 提交短暂消息。服务按提交顺序显示消息；它本身不创建或附加视觉控件。

## 成员

```csharp
Task<MdSnackbarResult> ShowAsync(MdSnackbarMessage message);
void Show(MdSnackbarMessage message);
void Dismiss();
```

| 成员 | 行为 |
|---|---|
| `ShowAsync` | 将消息加入队列；操作按钮触发时以 `MdSnackbarResult.ActionInvoked` 完成，关闭或超时后以 `MdSnackbarResult.Dismissed` 完成。 |
| `Show` | 将消息加入队列，不等待结果。 |
| `Dismiss` | 关闭当前显示的消息；没有活动消息时不执行操作。 |

`MdSnackbarMessage` 由消息内容及可选操作构成。设置 `ActionContent`、`ActionCommand` 和 `ActionCommandParameter` 可添加操作按钮。设有操作按钮的消息不会因 `Duration` 到期而自动关闭；可通过单独的关闭 affordance 或 `Dismiss()` 关闭。

## 配置

将共享服务实例同时连接到宿主与 ViewModel。也可以直接调用 `MdSnackbarHost` 的同名方法；宿主未设置共享服务时，会使用自己的私有服务。

```csharp
var snackbars = new MdSnackbarService();
services.AddSingleton<IMdSnackbarService>(snackbars);
shell.SnackbarHost.Service = snackbars;

await snackbars.ShowAsync(new MdSnackbarMessage("草稿已归档")
{
    ActionContent = "撤销",
    ActionCommand = undoCommand,
    IsDismissible = true
});
```

队列项只有在对应的 `MdSnackbarHost` 附加并呈现消息后才会完成。
