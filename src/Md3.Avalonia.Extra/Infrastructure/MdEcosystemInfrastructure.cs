using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Md3.Avalonia.Extra.Infrastructure;

public enum MdEcosystemDensity { Comfortable, Compact, Touch }
public enum MdAsyncRequestState { Idle, Loading, Data, Empty, Error, Completed }
public enum MdOverlayAlignment { Start, Center, End }

/// <summary>Inherited density metadata for Extra controls without changing core Material identity.</summary>
public sealed class MdDensity : AvaloniaObject
{
    public static readonly AttachedProperty<MdEcosystemDensity> ModeProperty = AvaloniaProperty.RegisterAttached<MdDensity, StyledElement, MdEcosystemDensity>(
        "Mode", MdEcosystemDensity.Comfortable, inherits: true);
    private MdDensity() { }
    public static MdEcosystemDensity GetMode(AvaloniaObject target) => target.GetValue(ModeProperty);
    public static void SetMode(AvaloniaObject target, MdEcosystemDensity value) => target.SetValue(ModeProperty, value);
}

/// <summary>Provider-neutral asynchronous page request used by paging, search, chat, and calendar adapters. 分页、搜索、聊天与日历适配器使用的通用异步分页请求。</summary>
/// <param name="PageKey">Application-defined key for the requested page. <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> starts with key 0. 请求页的应用自定义键；<see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> 从键 0 开始。</param>
/// <param name="PageSize">Requested number of items. <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> clamps its page size to at least 1. 请求的数据项数量；<see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> 会将页大小限制为至少 1。</param>
/// <param name="Query">Optional query string for filtering or searching. 用于筛选或搜索的可选查询字符串。</param>
/// <param name="CancellationToken">Token that the provider should observe while loading the page. 提供器加载当前页时应遵循的取消令牌。</param>
public sealed record MdPageRequest(int PageKey, int PageSize, string? Query = null, CancellationToken CancellationToken = default);

/// <summary>Items and continuation state returned for one page. 单页数据及其后续分页状态。</summary>
/// <typeparam name="T">The item type in <see cref="Items"/>. <see cref="Items"/> 中的数据项类型。</typeparam>
/// <param name="Items">Items returned for the requested page. 当前请求页返回的数据项。</param>
/// <param name="NextPageKey">Key for the next request, or <see langword="null"/> when there is no next page. 后续请求使用的页键；没有后续页时为 <see langword="null"/>。</param>
/// <param name="IsLastPage">Explicitly marks this result as the final page. 显式标记当前结果为最后一页。</param>
/// <remarks><see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> treats either a null <c>NextPageKey</c> or a true <c>IsLastPage</c> as completion. <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> 会将 <c>NextPageKey</c> 为 null 或 <c>IsLastPage</c> 为 true 视为分页完成。</remarks>
public sealed record MdPageResult<T>(IReadOnlyList<T> Items, int? NextPageKey, bool IsLastPage = false);

/// <summary>Loads one page of application-owned data for an asynchronous collection adapter. 为异步集合适配器加载一页应用数据。</summary>
/// <typeparam name="T">The item type returned by the provider. 提供器返回的项目类型。</typeparam>
/// <remarks>
/// <para>Implementations own the data source, query semantics, and cancellation behavior. Return
/// page items with their continuation information in <see cref="MdPageResult{T}"/>. When used with
/// <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/>, the control exposes loading state
/// through its <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView.State"/>,
/// <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView.Error"/>, and
/// <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView.Items"/> properties.
/// 实现者负责数据源、查询语义与取消行为，并通过 <see cref="MdPageResult{T}"/> 返回当前页数据及后续页信息。与 <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> 配合时，控件通过 <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView.State"/>、<see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView.Error"/> 和 <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView.Items"/> 属性提供加载状态。</para>
/// <para><see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> currently accepts a delegate of type
/// <c>Func&lt;MdPageRequest, ValueTask&lt;MdPageResult&lt;object?&gt;&gt;&gt;</c>, not this interface
/// directly. Adapt <see cref="LoadPageAsync"/> at the application boundary and convert the items
/// to <see cref="object"/> as needed.</para>
/// <para>目前 <see cref="Md3.Avalonia.Extra.Controls.MdPagedItemsView"/> 接收 <c>Func&lt;MdPageRequest, ValueTask&lt;MdPageResult&lt;object?&gt;&gt;&gt;</c> 委托，而非此接口本身。请在应用层适配 <see cref="LoadPageAsync"/>，并按需将数据项转换为 <see cref="object"/>。</para>
/// </remarks>
/// <example>
/// <para><c>Record</c>, <c>IRecordRepository</c>, and <c>repository</c> represent application-defined types and data.</para>
/// <para><c>Record</c>、<c>IRecordRepository</c> 和 <c>repository</c> 均代表应用自定义类型与数据。</para>
/// <code language="csharp"><![CDATA[
/// using System.Linq;
/// using Md3.Avalonia.Extra.Controls;
/// using Md3.Avalonia.Extra.Infrastructure;
///
/// public sealed record Record(string Name);
///
/// public interface IRecordRepository
/// {
///     ValueTask<MdPageResult<Record>> LoadPageAsync(MdPageRequest request);
/// }
///
/// public sealed class RecordProvider(IRecordRepository repository) : IMdPageProvider<Record>
/// {
///     public ValueTask<MdPageResult<Record>> LoadPageAsync(MdPageRequest request) =>
///         repository.LoadPageAsync(request);
/// }
///
/// var provider = new RecordProvider(repository);
/// view.PageProvider = async request =>
/// {
///     var page = await provider.LoadPageAsync(request);
///     return new MdPageResult<object?>(
///         page.Items.Select(item => (object?)item).ToArray(),
///         page.NextPageKey,
///         page.IsLastPage);
/// };
///
/// await view.LoadNextPageAsync();
/// ]]></code>
/// </example>
public interface IMdPageProvider<T>
{
    /// <summary>Loads the requested page and observes its cancellation token. 加载指定页并遵循请求中的取消令牌。</summary>
    /// <param name="request">Page key, requested size, optional query, and cancellation token. 页键、请求数量、可选查询和取消令牌。</param>
    /// <returns>The items and continuation information for the page. 当前页数据及后续页信息。</returns>
    ValueTask<MdPageResult<T>> LoadPageAsync(MdPageRequest request);
}

/// <summary>Shared shortcut descriptor suitable for ICommand/CommunityToolkit.MVVM.</summary>
public sealed record MdShortcutBinding(KeyGesture Gesture, ICommand Command, object? Parameter = null, string? Description = null);

/// <summary>Placement metadata consumed by popover/hover-card hosts; no platform popup type leaks into view models.</summary>
public sealed record MdOverlayPlacement(MdOverlayAlignment Horizontal = MdOverlayAlignment.Start, MdOverlayAlignment Vertical = MdOverlayAlignment.End, double OffsetX = 0, double OffsetY = 4, bool FlipToFit = true);

/// <summary>Captures a weak focus target before opening transient content and restores it after close.</summary>
public sealed class MdFocusReturnScope
{
    private WeakReference<InputElement>? _target;
    public void Capture(InputElement? target) { if (target is not null) _target = new WeakReference<InputElement>(target); }
    public bool Restore()
    {
        if (_target?.TryGetTarget(out var target) != true) return false;
        return target?.Focus() == true;
    }
}
