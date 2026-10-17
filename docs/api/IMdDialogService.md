# `IMdDialogService` API

**包：** `Md3.Avalonia`<br>
**命名空间：** `Md3.Avalonia.Controls`<br>
**源码：** [`IMdDialogService.cs`](../../src/Md3.Avalonia/Controls/IMdDialogService.cs)

`IMdDialogService` 为 ViewModel 提供与视图解耦的对话框入口。调用方传入对话框模型；附加的 `MdDialogHost` 通过匹配的 `DataTemplate` 创建视图。若应用中有多个请求，同一时刻只显示一个。

## 成员

```csharp
bool IsOpen { get; }
Task<object?> ShowAsync(object dialog, CancellationToken cancellationToken = default);
void Show(object dialog);
Task<object?> ReplaceAsync(object dialog, CancellationToken cancellationToken = default);
void Replace(object dialog);
void Close(object? result = null);
```

| 成员 | 行为 |
|---|---|
| `IsOpen` | 通过此服务显示的对话框处于打开状态时为 `true`。 |
| `ShowAsync` | 显示或排队显示 `dialog`，任务以 `Close(result)` 传入的结果完成。遮罩、Escape、Android 返回手势或取消令牌关闭对话框时，结果为 `null`。 |
| `Show` | 不等待结果，按队列请求显示对话框。 |
| `ReplaceAsync` | 将当前显示的对话框以 `null` 结果结束，并优先显示新请求；没有当前对话框时按 `ShowAsync` 处理。 |
| `Replace` | 不等待结果，执行 `ReplaceAsync` 对应的替换操作。 |
| `Close` | 关闭当前显示的对话框，并将可选的 `result` 交还给等待方。 |

请求可以在宿主尚未附加时排队。取消令牌取消等待或当前请求；关闭结果为 `null`。`MdDialogServiceExtensions.ShowAsync<TResult>` 是一个扩展方法，可将结果转换为 `TResult?`，不属于接口成员。

## 配置

在应用 Shell 中放置一个宿主，并将同一个 `MdDialogService` 实例注册到依赖注入容器：

```csharp
var dialogs = new MdDialogService();
services.AddSingleton<IMdDialogService>(dialogs);
shell.DialogHost.Service = dialogs;
```

ViewModel 只依赖接口：

```csharp
public sealed class DeleteViewModel(IMdDialogService dialogs)
{
    public async Task DeleteAsync(object model)
    {
        var confirmed = await dialogs.ShowAsync<bool>(model);
        if (confirmed) { /* 执行删除 */ }
    }
}
```

模型对应的视图应注册为 `MdDialogHost.DataTemplates` 或可见的 `DataTemplates`。从视图或 ViewModel 调用 `Close(true)` 可返回结果。完整 AXAML 模板、依赖注入与关闭行为示例见 [`DIALOG_SERVICE.md`](../DIALOG_SERVICE.md)。
