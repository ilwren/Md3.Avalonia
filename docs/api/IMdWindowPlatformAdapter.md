# `IMdWindowPlatformAdapter` API

**包：** `Md3.Avalonia`<br>
**命名空间：** `Md3.Avalonia.Controls`<br>
**源码：** [`MdBorderlessWindow.cs`](../../src/Md3.Avalonia/Controls/MdBorderlessWindow.cs)

`IMdWindowPlatformAdapter` 将 `MdBorderlessWindow` 的窗口装饰、拖动、缩放与系统菜单操作隔离在平台边界中。默认适配器由 `MdWindowPlatformAdapterResolver.Resolve()` 按运行平台创建，可通过 `MdBorderlessWindow.PlatformAdapter` 替换。

## 成员

```csharp
MdWindowPlatform Platform { get; }
MdWindowCapabilities Capabilities { get; }
void Apply(MdBorderlessWindow window, MdBorderlessWindowOptions options);
bool TryBeginMove(MdBorderlessWindow window, PointerPressedEventArgs args);
bool TryBeginResize(MdBorderlessWindow window, WindowEdge edge, PointerPressedEventArgs args);
bool TryShowSystemMenu(MdBorderlessWindow window, Point clientPoint);
```

| 成员 | 行为 |
|---|---|
| `Platform` | 适配器面向的平台标识。 |
| `Capabilities` | 适配器支持的操作标志：`Move`、`Resize`、`Minimize`、`Maximize`、`Close`、`SystemMenu` 或 `All`。 |
| `Apply` | 将 `MdBorderlessWindowOptions` 中的缩放、标题栏扩展、高度和原生边框设置应用到窗口。窗口打开、选项变化或更换适配器时由控件调用。 |
| `TryBeginMove` | 尝试开始窗口移动；返回 `true` 表示适配器已处理按下事件。 |
| `TryBeginResize` | 尝试开始指定边缘的窗口缩放；返回 `true` 表示已处理事件。 |
| `TryShowSystemMenu` | 尝试在窗口坐标 `clientPoint` 处打开系统菜单。返回 `false` 时，窗口会触发 `SystemMenuRequested` 事件。 |

项目提供 `MdWindowsWindowPlatformAdapter`、`MdMacOsWindowPlatformAdapter`、`MdLinuxWindowPlatformAdapter`、`MdAndroidWindowPlatformAdapter` 与通用 `MdAvaloniaWindowPlatformAdapter`。通用实现使用 Avalonia 的 move/resize API；Android adapter 不执行桌面窗口操作。自定义实现也可继承 `MdAvaloniaWindowPlatformAdapter`，只重写需要的平台行为。

```csharp
public sealed class CustomWindowAdapter : MdAvaloniaWindowPlatformAdapter
{
    public CustomWindowAdapter() : base(MdWindowPlatform.Windows) { }

    public override bool TryShowSystemMenu(MdBorderlessWindow window, Point point)
    {
        // 调用平台系统菜单；成功处理时返回 true。
        return false;
    }
}

borderlessWindow.PlatformAdapter = new CustomWindowAdapter();
```

平台原生窗口外观和系统动画仍需在目标 OS 验证。
