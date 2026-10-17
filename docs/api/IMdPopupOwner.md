# `IMdPopupOwner` API

**包：** `Md3.Avalonia`<br>
**命名空间：** `Md3.Avalonia.Controls`<br>
**源码：** [`IMdPopupOwner.cs`](../../src/Md3.Avalonia/Controls/IMdPopupOwner.cs)

`IMdPopupOwner` 描述拥有 Material 临时弹层的控件及其打开状态。

## 成员

```csharp
bool IsMaterialPopupOpen { get; set; }
```

核心与 Extra 中的若干内置弹层控件实现此接口。库内协调器据此在同一 UI Dispatcher 上关闭先前打开的弹层，并在锚点所在滚动区域发生位移时关闭弹层。

**扩展边界：**该接口本身只公开状态属性；用于注册和协调状态变化的 `MdPopupCoordinator` 是内部类型。因此，第三方控件仅实现 `IMdPopupOwner` 并不能加入库内互斥协调。它不是面向外部的弹层扩展服务；自定义控件应自行管理弹层生命周期。
