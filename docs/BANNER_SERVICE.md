# 从 ViewModel 显示 Banner：`IMdBannerService`

`MdBannerHost` 让 Banner 与 Dialog、Snackbar 一样可以通过依赖注入和 MVVM 调用：ViewModel 只提交一个模型，View 使用按类型匹配的 `DataTemplate` 决定视觉，二者通过 `IMdBannerService` 解耦。

可运行示例：`gallery/Md3.Avalonia.Gallery/Pages/BannerGalleryPage.axaml(.cs)`。

## 1. 在组合根共享同一个实例

宿主需要具体的 `MdBannerService`，ViewModel 注入接口 `IMdBannerService`。两处必须指向同一个实例：

```csharp
var banners = new MdBannerService();

shell.BannerHost.Service = banners;
services.AddSingleton<IMdBannerService>(banners);
```

不用 DI 容器时，直接把同一个对象传给 ViewModel 构造函数即可。Service 对宿主使用弱引用，不会单独延长窗口生命周期。

## 2. View 声明模型对应的 Banner 模板

`MdBannerHost` 的 `Content` 是页面或 shell；Banner 打开时位于内容上方并占据自己的布局行。每个 `DataTemplate` 的根应为 `MdBanner`，这样 leading、actions、窄屏 action 换行、关闭按钮和 motion 都继续使用标准控件实现。

```xml
<md:MdBannerHost x:Name="BannerHost">
  <md:MdBannerHost.DataTemplates>
    <DataTemplate DataType="{x:Type vm:SyncBannerModel}"
                  x:DataType="vm:SyncBannerModel">
      <md:MdBanner IsDismissible="True">
        <md:MdBanner.LeadingContent>
          <md:MdIcon Glyph="{x:Static md:MdSymbols.SyncProblem}" />
        </md:MdBanner.LeadingContent>
        <md:MdBanner.Actions>
          <StackPanel Orientation="Horizontal">
            <md:MdButton Content="Keep offline" Variant="Text"
                         Command="{Binding KeepOfflineCommand}" />
            <md:MdButton Content="Retry" Variant="Text"
                         Command="{Binding RetryCommand}" />
          </StackPanel>
        </md:MdBanner.Actions>
        <TextBlock Text="{Binding Message}" TextWrapping="Wrap" />
      </md:MdBanner>
    </DataTemplate>
  </md:MdBannerHost.DataTemplates>

  <views:Shell />
</md:MdBannerHost>
```

`DataType` 用于运行时模板选择；开启 compiled bindings 时再写 `x:DataType`，让模板内绑定获得编译检查。模板也可放到上层 `DataTemplates` 作用域，或者通过 `BannerTemplate` 显式指定。

## 3. ViewModel 只注入接口

```csharp
public sealed class SyncViewModel
{
    private readonly IMdBannerService _banners;

    public SyncViewModel(IMdBannerService banners) => _banners = banners;

    public async Task SynchronizeAsync()
    {
        var result = await _banners.ShowAsync<string>(
            new SyncBannerModel(_banners, "Could not synchronize changes."));

        if (result == "retry")
            await RetryAsync();
    }
}
```

模型中的 action 也通过同一接口关闭 surface 并返回结果：

```csharp
public sealed class SyncBannerModel
{
    public SyncBannerModel(IMdBannerService banners, string message)
    {
        Message = message;
        RetryCommand = new RelayCommand(() => banners.Close("retry"));
        KeepOfflineCommand = new RelayCommand(() => banners.Close("offline"));
    }

    public string Message { get; }
    public RelayCommand RetryCommand { get; }
    public RelayCommand KeepOfflineCommand { get; }
}
```

点击 `MdBanner` 自带的 dismiss 按钮、取消 token，或通过双向绑定把宿主 `IsOpen` 设为 `false` 时，`ShowAsync` 以 `null` 完成；`ShowAsync<T>` 返回 `default`。

## 4. 队列与替换语义

| 调用 | 行为 |
|---|---|
| `ShowAsync(model)` | 显示并等待 `Close(result)`；已有 Banner 时排队 |
| `Show(model)` | 与 `ShowAsync` 相同，但不等待返回值 |
| `ReplaceAsync(model)` | 关闭当前 Banner，并让新状态排到现有队列最前面 |
| `Replace(model)` | 不等待结果的替换 |
| `Close(result)` | 关闭当前 Banner，并把结果交给等待方 |
| `IsOpen` | 当前是否有该 service 发出的 Banner 正在显示 |

宿主尚未挂入可视树时，请求会保留到宿主连接。队列始终一次显示一个 Banner；下一个请求会等待上一个退出 motion 完成，避免两个持久 surface 重叠。

## 5. 三种调用方式可以并存

| 来源 | 写法 |
|---|---|
| View 直接持有 Banner | `banner.Show()` / `banner.Dismiss()` |
| View code-behind 使用宿主 | `await BannerHost.ShowAsync(model)` / `BannerHost.Close(result)` |
| ViewModel | 注入 `IMdBannerService` 后调用相同 API |
| 纯绑定 | 双向 `MdBannerHost.IsOpen` + `Banner` |

直接 `MdBanner` 的现有 API 没有改变。只有在调用方不应持有控件引用、需要模板选择或请求队列时，才需要 host/service 路线。
