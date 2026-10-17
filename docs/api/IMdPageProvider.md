# `IMdPageProvider<T>` API

**包：** `Md3.Avalonia.Extra`<br>
**命名空间：** `Md3.Avalonia.Extra.Infrastructure`<br>
**源码：** [`MdEcosystemInfrastructure.cs`](../../src/Md3.Avalonia.Extra/Infrastructure/MdEcosystemInfrastructure.cs)

`IMdPageProvider<T>` 是应用数据层的异步分页契约。请求通过 `MdPageRequest` 传入页键、页大小、可选查询与取消令牌，返回 `MdPageResult<T>`。

## 成员

```csharp
ValueTask<MdPageResult<T>> LoadPageAsync(MdPageRequest request);
```

- `MdPageRequest.PageKey`：请求的整数页键。
- `MdPageRequest.PageSize`：请求的项目数量。
- `MdPageRequest.Query`：可选查询字符串。
- `MdPageRequest.CancellationToken`：取消当前请求的令牌。
- `MdPageResult<T>.Items`：当前页数据。
- `MdPageResult<T>.NextPageKey`：后续页键；当前 `MdPagedItemsView` 将 `null` 视为最后一页。
- `MdPageResult<T>.IsLastPage`：显式标记当前页是否为最后一页。

## 与 `MdPagedItemsView` 配合

当前控件的 `PageProvider` 属性是委托类型，并不直接接收 `IMdPageProvider<T>`。可在应用层适配接口；示例中的 `Record` 数据类型与 `repository` 由应用提供：

```csharp
public interface IRecordRepository
{
    ValueTask<MdPageResult<Record>> LoadPageAsync(MdPageRequest request);
}

public sealed class RecordProvider(IRecordRepository repository) : IMdPageProvider<Record>
{
    public ValueTask<MdPageResult<Record>> LoadPageAsync(MdPageRequest request) =>
        repository.LoadPageAsync(request);
}

var provider = new RecordProvider(repository);
view.PageProvider = async request =>
{
    var page = await provider.LoadPageAsync(request);
    return new MdPageResult<object?>(
        page.Items.Cast<object?>().ToArray(),
        page.NextPageKey,
        page.IsLastPage);
};

await view.LoadNextPageAsync();
```

提供器负责读取数据；控件通过 `State`、`Error` 和 `Items` 管理加载展示。
