# Showing dialogs from a ViewModel: `IMdDialogService` / 从 ViewModel 弹出对话框：`IMdDialogService`

The goal is to keep the ViewModel **free of control references**: it requests a dialog model and then `await`s a result. `MdDialogHost` resolves the view through its `DataTemplates`.

目的是让 ViewModel **不持有任何控件引用**：它只说“我要显示这个对话框模型”，然后 `await` 一个返回值。视图由 `MdDialogHost` 的 `DataTemplates` 解析。

An executable reference implementation is in `gallery/Md3.Avalonia.Gallery/Pages/DialogGalleryPage.axaml(.cs)`; the examples below are based on it.

可运行的参考实现在 `gallery/Md3.Avalonia.Gallery/Pages/DialogGalleryPage.axaml(.cs)`，下面的代码就是从那里来的。

---

## 1. Composition root: one instance, two roles / 组合根：一个实例，两个身份

`MdDialogHost.Service` requires the concrete `MdDialogService`, while the ViewModel receives the `IMdDialogService` interface. **They must be the same instance.**

`MdDialogHost.Service` 要的是具体类型 `MdDialogService`，ViewModel 注入的是接口 `IMdDialogService`。**必须是同一个实例。**

```csharp
var dialogs = new MdDialogService();

// Host (view side) / 宿主（视图侧）
shell.DialogHost.Service = dialogs;

// Injection (ViewModel side) / 注入（ViewModel 侧）
services.AddSingleton<IMdDialogService>(dialogs);
```

A DI container is optional; you can pass the instance directly to constructors:

不用 DI 容器也完全可以，构造函数直接传即可：

```csharp
ServiceDialogHost.Service = dialogs;
ServiceDemoRoot.DataContext = new DialogServiceDemoViewModel(dialogs);
```

`MdDialogService` holds a **weak reference** to the host, so it will not keep a window alive. The host must therefore be kept alive by the visual tree. One instance for the whole shell is enough.

`MdDialogService` 对宿主持的是**弱引用**，所以不会让窗口泄漏；但也意味着宿主必须由可视树自己保活。整个 shell 放一个就够。

---

## 2. Host: place it at the shell root and register templates / 宿主：放在 shell 顶层，并登记模板

`MdDialogHost` is a container with an overlay; **its content is the layer that it covers**. Put it at the outermost level of the application shell, not inside an individual page.

`MdDialogHost` 是一个带遮罩层的容器控件，**它的内容就是被它遮住的那一层**。所以要把它放在应用外壳的最外层，而不是某个页面内部。

```xml
<md:MdDialogHost x:Name="DialogHost" ZIndex="100">

  <md:MdDialogHost.DataTemplates>
    <!-- The repository enables AvaloniaUseCompiledBindingsByDefault, so both DataType and x:DataType are required. / 仓库开启了 AvaloniaUseCompiledBindingsByDefault，所以 DataType 和 x:DataType 两个都要写。 -->
    <DataTemplate DataType="{x:Type vm:ConfirmDeleteDialogModel}"
                  x:DataType="vm:ConfirmDeleteDialogModel">
      <md:MdDialog Headline="{Binding Headline}" Variant="Basic">
        <TextBlock MaxWidth="340" TextWrapping="Wrap" Text="{Binding Message}" />
        <md:MdDialog.Actions>
          <StackPanel Orientation="Horizontal" Spacing="8">
            <md:MdButton Content="Keep"   Variant="Text" Command="{Binding CancelCommand}" />
            <md:MdButton Content="Delete" Variant="Text" Command="{Binding ConfirmCommand}" />
          </StackPanel>
        </md:MdDialog.Actions>
      </md:MdDialog>
    </DataTemplate>

    <!-- One template per dialog model; the host selects by runtime type. / 每种对话框模型一个模板，宿主按运行时类型选。 -->
    <DataTemplate DataType="{x:Type vm:RenameDialogModel}" x:DataType="vm:RenameDialogModel">
      ...
    </DataTemplate>
  </md:MdDialogHost.DataTemplates>

  <views:Shell />   <!-- Content covered by the overlay / 被遮罩的内容 -->
</md:MdDialogHost>
```

Templates can also live in `App.axaml` under `DataTemplates`; they remain visible through the same inherited scope. Keeping them on the host simply makes them easier to find beside their use.

模板也可以放在 `App.axaml` 的 `DataTemplates` 里，继承作用域一样能解析到；放在宿主上只是便于就近阅读。

---

## 3. Calling ViewModel: depend only on the service / 调用方 ViewModel：只认服务

```csharp
public sealed class DialogServiceDemoViewModel
{
    private readonly IMdDialogService _dialogs;

    public DialogServiceDemoViewModel(IMdDialogService dialogs)
    {
        _dialogs = dialogs;
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    public AsyncRelayCommand DeleteCommand { get; }

    private async Task DeleteAsync()
    {
        // Overlay dismissal / Escape / Android back / token cancellation returns default (false here). / 被遮罩关闭、Escape、Android 返回手势或 token 取消时返回 default（此处即 false）。
        var confirmed = await _dialogs.ShowAsync<bool>(
            new ConfirmDeleteDialogModel(_dialogs, "Quarterly report"));

        Status = confirmed ? "Deleted." : "Kept.";
    }
}
```

---

## 4. How the dialog returns its answer / 对话框自己怎么给出答案

The dialog model also receives the service. Call `Close(result)` to provide the result returned by `ShowAsync`:

对话框模型同样注入服务，用 `Close(result)` 回填 `ShowAsync` 的返回值：

```csharp
public sealed class ConfirmDeleteDialogModel
{
    private readonly IMdDialogService _dialogs;

    public ConfirmDeleteDialogModel(IMdDialogService dialogs, string item)
    {
        _dialogs = dialogs;
        Headline = $"Delete {item}?";
        ConfirmCommand = new RelayCommand(() => _dialogs.Close(true));
        CancelCommand  = new RelayCommand(() => _dialogs.Close(false));
    }

    public string Headline { get; }
    public string Message => "This cannot be undone.";
    public RelayCommand ConfirmCommand { get; }
    public RelayCommand CancelCommand { get; }
}
```

---

## 5. Behaviors to know / 需要知道的几条行为

| Situation / 情况 | Behavior / 行为 |
|---|---|
| Call `ShowAsync` while another dialog is open / 已有对话框显示时再 `ShowAsync` | **Queued**, not preemptive (Material permits only one modal dialog at a time). / **排队**，不抢占（Material 规定同时只有一个模态）。 |
| Call `ShowAsync` before the host is attached / 宿主还没挂上就 `ShowAsync` | Queued as well; no exception, so startup timing does not need special handling. / 同样排队，不抛异常 —— 启动时序不用小心翼翼。 |
| Overlay click, Escape, Android back, or token cancellation / 遮罩点击、Escape、Android 返回或 token 取消 | The pending `ShowAsync` completes with `null`; `ShowAsync<T>` returns `default`. / 挂起的 `ShowAsync` 以 `null` 完成；`ShowAsync<T>` 得到 `default`。 |
| Move to the next step in the same flow (about → license, wizard step) / 同一流程内换一步（about → 许可证、向导翻页） | Use `ReplaceAsync` to **move it to the front**. `ShowAsync` queues it behind the current dialog, so it will not appear until that dialog closes. / 用 `ReplaceAsync`，它**插队**到最前；用 `ShowAsync` 会排在当前这个后面，用户关掉才看得见。 |
| Call from a background thread / 从后台线程调用 | The service marshals the call to the UI thread; the continuation of `ShowAsync<T>` also resumes on the UI thread. / 服务内部会 marshal 回 UI 线程，`ShowAsync<T>` 的续体也回到 UI 线程。 |
| Result is not needed / 不关心结果 | Use `Show(dialog)` or `Replace(dialog)` as fire-and-forget calls. / `Show(dialog)` / `Replace(dialog)`，即发即忘。 |

`IsOpen` is readable and can be used for checks such as disabling selected commands while a dialog is open.

`IsOpen` 可读，用于“有对话框时禁用某些命令”这类判断。

---

## 6. The three routes can be mixed / 三条路线可以混用

`MdDialogHost` supports three ways to drive dialogs. Choose by **call site**; the whole application does not need to use just one approach:

`MdDialogHost` 同时支持三种驱动方式，按**调用点**选，不必全应用统一：

| Source / 来源 | Usage / 写法 | Result / 结果 |
|---|---|---|
| View code-behind / 视图 code-behind | `await DialogHost.ShowAsync(model)` | Return value / 返回值 |
| ViewModel / ViewModel | Inject `IMdDialogService`, then `await _dialogs.ShowAsync(model)` / 注入 `IMdDialogService`，再 `await _dialogs.ShowAsync(model)` | Return value / 返回值 |
| Binding only / 纯绑定 | Two-way `IsOpen` together with `Dialog` / 双向 `IsOpen` 配合 `Dialog` | ViewModel state; no `await` / ViewModel 状态，不 await |
