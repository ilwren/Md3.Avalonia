# 从 ViewModel 弹出对话框：`IMdDialogService`

目的是让 ViewModel **不持有任何控件引用**：它只说"我要显示这个对话框模型"，然后 `await` 一个返回值。
视图由 `MdDialogHost` 的 `DataTemplates` 解析。

可运行的参考实现在 `gallery/Md3.Avalonia.Gallery/Pages/DialogGalleryPage.axaml(.cs)`，下面的代码
就是从那里来的。

---

## 1. 组合根：一个实例，两个身份

`MdDialogHost.Service` 要的是具体类型 `MdDialogService`，ViewModel 注入的是接口 `IMdDialogService`。
**必须是同一个实例。**

```csharp
var dialogs = new MdDialogService();

// 宿主（视图侧）
shell.DialogHost.Service = dialogs;

// 注入（ViewModel 侧）
services.AddSingleton<IMdDialogService>(dialogs);
```

不用 DI 容器也完全可以，构造函数直接传即可：

```csharp
ServiceDialogHost.Service = dialogs;
ServiceDemoRoot.DataContext = new DialogServiceDemoViewModel(dialogs);
```

`MdDialogService` 对宿主持的是**弱引用**，所以不会让窗口泄漏；但也意味着宿主必须由可视树自己
保活。整个 shell 放一个就够。

---

## 2. 宿主：放在 shell 顶层，并登记模板

`MdDialogHost` 是一个带遮罩层的容器控件，**它的内容就是被它遮住的那一层**。所以要把它放在应用
外壳的最外层，而不是某个页面内部。

```xml
<md:MdDialogHost x:Name="DialogHost" ZIndex="100">

  <md:MdDialogHost.DataTemplates>
    <!-- 仓库开启了 AvaloniaUseCompiledBindingsByDefault，所以 DataType 和 x:DataType 两个都要写 -->
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

    <!-- 每种对话框模型一个模板，宿主按运行时类型选 -->
    <DataTemplate DataType="{x:Type vm:RenameDialogModel}" x:DataType="vm:RenameDialogModel">
      ...
    </DataTemplate>
  </md:MdDialogHost.DataTemplates>

  <views:Shell />   <!-- 被遮罩的内容 -->
</md:MdDialogHost>
```

模板也可以放在 `App.axaml` 的 `DataTemplates` 里，继承作用域一样能解析到；放在宿主上只是便于
就近阅读。

---

## 3. 调用方 ViewModel：只认服务

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
        // 被遮罩关闭 / Escape / Android 返回手势 / token 取消 时返回 default，这里即 false
        var confirmed = await _dialogs.ShowAsync<bool>(
            new ConfirmDeleteDialogModel(_dialogs, "Quarterly report"));

        Status = confirmed ? "Deleted." : "Kept.";
    }
}
```

---

## 4. 对话框自己怎么给出答案

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

## 5. 需要知道的几条行为

| 情况 | 行为 |
|---|---|
| 已有对话框显示时再 `ShowAsync` | **排队**，不抢占（Material 规定同时只有一个模态） |
| 宿主还没挂上就 `ShowAsync` | 同样排队，不抛异常 —— 启动时序不用小心翼翼 |
| 遮罩点击 / Escape / Android 返回 / token 取消 | 挂起的 `ShowAsync` 以 `null` 完成；`ShowAsync<T>` 得到 `default` |
| 同一流程内换一步（about → 许可证、向导翻页） | 用 `ReplaceAsync`，它**插队**到最前；用 `ShowAsync` 会排在当前这个后面，用户关掉才看得见 |
| 从后台线程调用 | 服务内部会 marshal 回 UI 线程，`ShowAsync<T>` 的续体也回到 UI 线程 |
| 不关心结果 | `Show(dialog)` / `Replace(dialog)`，即发即忘 |

`IsOpen` 可读，用于「有对话框时禁用某些命令」这类判断。

---

## 6. 三条路线可以混用

`MdDialogHost` 同时支持三种驱动方式，按**调用点**选，不必全应用统一：

| 来源 | 写法 | 结果 |
|---|---|---|
| 视图 code-behind | `await DialogHost.ShowAsync(model)` | 返回值 |
| ViewModel | 注入 `IMdDialogService`，`await _dialogs.ShowAsync(model)` | 返回值 |
| 纯绑定 | 双向 `IsOpen` 配合 `Dialog` | ViewModel 状态，不 await |
